using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Network
{
    /// <summary>
    /// 网络管理：HTTP 登录后建立 WebSocket，后续请求与推送通过 WebSocket。
    /// 挂在启动场景一个物体上，不销毁。
    /// </summary>
    public class NetworkManager : MonoBehaviour
    {
        public static NetworkManager Instance { get; private set; }

        [SerializeField] private string baseUrl = "http://localhost:8080";
        [SerializeField] private float timeout = 10f;

        public string BaseUrl { get => baseUrl; set => baseUrl = value?.TrimEnd('/') ?? ""; }
        public bool IsConnected { get; private set; }
        public string AuthToken { get; set; }
        public PlayerInfo PlayerInfo { get; private set; }

        public bool IsWsConnected => _ws != null && _ws.State == WebSocketState.Open;
        public event Action<PlayerInfo> OnPlayerInfoReceived;
        public event Action<bool> OnWsConnectionChanged;

        private ClientWebSocket _ws;
        private CancellationTokenSource _wsCts;
        private readonly ConcurrentQueue<string> _wsMessageQueue = new ConcurrentQueue<string>();
        private bool _wsConnectedFlag;
        private bool _wsDisconnectedFlag;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            CloseWebSocket();
            if (Instance == this) Instance = null;
        }

        private void Update()
        {
            if (_wsConnectedFlag)
            {
                _wsConnectedFlag = false;
                OnWsConnectionChanged?.Invoke(true);
            }
            if (_wsDisconnectedFlag)
            {
                _wsDisconnectedFlag = false;
                OnWsConnectionChanged?.Invoke(false);
            }
            while (_wsMessageQueue.TryDequeue(out string raw))
            {
                try
                {
                    var envelope = JsonUtility.FromJson<WsMessageDto>(raw);
                    if (envelope == null) continue;
                    if (envelope.cmd == "player.get" && envelope.code == 0 && !string.IsNullOrEmpty(envelope.data))
                    {
                        PlayerInfo = JsonUtility.FromJson<PlayerInfo>(envelope.data);
                        OnPlayerInfoReceived?.Invoke(PlayerInfo);
                    }
                }
                catch (Exception) { /* ignore parse error */ }
            }
        }

        /// <summary>发送账号密码建立连接（POST 登录接口）</summary>
        /// <param name="onResult">(是否成功, 失败时的错误信息)</param>
        public void EstablishConnection(string account, string password, Action<bool, string> onResult)
        {
            if (string.IsNullOrEmpty(account) || string.IsNullOrEmpty(password))
            {
                onResult?.Invoke(false, "账号或密码为空");
                return;
            }

            string json = $"{{\"username\":\"{Escape(account)}\",\"password\":\"{Escape(password)}\"}}";
            Post("/api/login", json, (ok, body) =>
            {
                IsConnected = ok;
                if (!ok)
                {
                    onResult?.Invoke(false, body ?? "请求失败");
                    return;
                }
                try
                {
                    var dto = JsonUtility.FromJson<ApiResponseDto>(body);
                    if (dto != null && dto.IsSuccess)
                    {
                        if (!string.IsNullOrEmpty(dto.data)) AuthToken = dto.data;
                        onResult?.Invoke(true, null);
                    }
                    else
                        onResult?.Invoke(false, dto?.msg ?? "登录失败");
                }
                catch (Exception e)
                {
                    onResult?.Invoke(false, e.Message);
                }
            });
        }

        /// <summary>建立 WebSocket 连接（需在登录成功后调用，使用当前 AuthToken）</summary>
        public void ConnectWebSocket(Action<bool, string> onResult)
        {
            if (string.IsNullOrEmpty(AuthToken))
            {
                onResult?.Invoke(false, "请先登录");
                return;
            }
            CloseWebSocket();
            string wsUrl = BaseUrl.Replace("http://", "ws://").Replace("https://", "wss://").TrimEnd('/') + "/ws?token=" + Uri.EscapeDataString(AuthToken);
            StartCoroutine(ConnectWebSocketCoroutine(wsUrl, onResult));
        }

        private IEnumerator ConnectWebSocketCoroutine(string wsUrl, Action<bool, string> onResult)
        {
            _ws = new ClientWebSocket();
            _wsCts = new CancellationTokenSource();
            var task = _ws.ConnectAsync(new Uri(wsUrl), _wsCts.Token);
            while (!task.IsCompleted)
                yield return null;
            if (task.IsFaulted)
            {
                onResult?.Invoke(false, task.Exception?.Message ?? "WebSocket 连接失败");
                yield break;
            }
            if (task.IsCanceled)
            {
                onResult?.Invoke(false, "已取消");
                yield break;
            }
            _wsConnectedFlag = true;
            onResult?.Invoke(true, null);
            _ = ReceiveLoop();
        }

        private async Task ReceiveLoop()
        {
            var buffer = new byte[4096];
            var cts = _wsCts?.Token ?? CancellationToken.None;
            try
            {
                while (_ws != null && _ws.State == WebSocketState.Open)
                {
                    var segment = new ArraySegment<byte>(buffer);
                    var result = await _ws.ReceiveAsync(segment, cts);
                    if (result.MessageType == System.Net.WebSockets.WebSocketMessageType.Close)
                        break;
                    if (result.MessageType == System.Net.WebSockets.WebSocketMessageType.Text && result.Count > 0)
                        _wsMessageQueue.Enqueue(Encoding.UTF8.GetString(buffer, 0, result.Count));
                }
            }
            catch (ObjectDisposedException) { }
            catch (OperationCanceledException) { }
            finally
            {
                _wsDisconnectedFlag = true;
            }
        }

        /// <summary>通过 WebSocket 请求玩家信息（需先 ConnectWebSocket）</summary>
        public void RequestPlayerInfo()
        {
            if (!IsWsConnected)
                return;
            string json = "{\"cmd\":\"player.get\",\"reqId\":\"" + Guid.NewGuid().ToString("N") + "\"}";
            var bytes = Encoding.UTF8.GetBytes(json);
            _ = _ws.SendAsync(new ArraySegment<byte>(bytes), System.Net.WebSockets.WebSocketMessageType.Text, true, _wsCts?.Token ?? default);
        }

        private void CloseWebSocket()
        {
            _wsCts?.Cancel();
            try
            {
                _ws?.CloseAsync(System.Net.WebSockets.WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
            }
            catch (Exception) { }
            _ws?.Dispose();
            _ws = null;
            _wsCts = null;
        }

        /// <summary>发送请求，在 onResponse 中接收响应（HTTP，未建 WS 时可用）</summary>
        /// <param name="path">路径，如 /api/room/list</param>
        /// <param name="jsonBody">请求体 JSON，可为 null</param>
        /// <param name="onResponse">(是否成功, 响应内容或错误信息)</param>
        public void SendRequest(string path, string jsonBody, Action<bool, string> onResponse)
        {
            if (onResponse == null) return;
            StartCoroutine(PostCoroutine(path, jsonBody, onResponse));
        }

        private static string Escape(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }

        private string FullUrl(string path)
        {
            path = (path ?? "").Trim();
            if (!path.StartsWith("/")) path = "/" + path;
            return BaseUrl + path;
        }

        private void Post(string path, string jsonBody, Action<bool, string> onResponse)
        {
            if (onResponse == null) return;
            StartCoroutine(PostCoroutine(path, jsonBody, onResponse));
        }

        private IEnumerator PostCoroutine(string path, string jsonBody, Action<bool, string> onResponse)
        {
            byte[] body = string.IsNullOrEmpty(jsonBody) ? null : Encoding.UTF8.GetBytes(jsonBody);
            using (var req = new UnityWebRequest(FullUrl(path), "POST"))
            {
                req.timeout = (int)timeout;
                req.downloadHandler = new DownloadHandlerBuffer();
                if (body != null && body.Length > 0)
                    req.uploadHandler = new UploadHandlerRaw(body);
                req.SetRequestHeader("Content-Type", "application/json");
                req.SetRequestHeader("Accept", "application/json");
                if (!string.IsNullOrEmpty(AuthToken))
                    req.SetRequestHeader("Authorization", "Bearer " + AuthToken);
                yield return req.SendWebRequest();

                if (req.result == UnityWebRequest.Result.Success)
                    onResponse(true, req.downloadHandler?.text ?? "");
                else
                    onResponse(false, req.error ?? req.downloadHandler?.text ?? "Unknown error");
            }
        }
    }
}
