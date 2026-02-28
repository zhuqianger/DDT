using System;
using System.Collections.Generic;
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
    /// 网络层：HTTP 登录、WebSocket 连接、按协议名(cmd)注册回调，收到推送后自动派发到对应回调列表。
    /// </summary>
    public class NetworkManager : MonoBehaviour
    {
        public static NetworkManager Instance { get; private set; }

        [SerializeField] string _baseUrl = "http://localhost:8080";
        [SerializeField] float _timeout = 10f;

        public string BaseUrl { get => _baseUrl; set => _baseUrl = value?.TrimEnd('/') ?? ""; }
        public bool IsConnected { get; private set; }
        public string AuthToken { get; set; }
        public bool IsWsConnected => _ws != null && _ws.State == WebSocketState.Open;
        public event Action<bool> OnWsConnectionChanged;

        ClientWebSocket _ws;
        CancellationTokenSource _wsCts;
        readonly ConcurrentQueue<string> _msgQueue = new ConcurrentQueue<string>();
        readonly ConcurrentQueue<Action> _pending = new ConcurrentQueue<Action>();
        readonly Dictionary<string, List<Action<string, int, string, string>>> _handlers = new Dictionary<string, List<Action<string, int, string, string>>>();

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void OnDestroy()
        {
            CloseWs();
            if (Instance == this) Instance = null;
        }

        void Update()
        {
            while (_pending.TryDequeue(out var a)) a?.Invoke();
            while (_msgQueue.TryDequeue(out string raw))
            {
                try
                {
                    var e = JsonUtility.FromJson<WsMessageDto>(raw);
                    if (e == null) continue;
                    var cmd = e.cmd ?? "";
                    if (_handlers.TryGetValue(cmd, out var list))
                    {
                        var reqId = e.reqId ?? "";
                        var code = e.code;
                        var msg = e.msg ?? "";
                        var data = e.data ?? "";
                        foreach (var h in list) try { h(reqId, code, msg, data); } catch (Exception) { }
                    }
                }
                catch (Exception) { }
            }
        }

        /// <summary>按协议名注册：服务端返回的 cmd 一致时调用 handler(reqId, code, msg, data)。</summary>
        public void Register(string cmd, Action<string, int, string, string> handler)
        {
            if (string.IsNullOrEmpty(cmd) || handler == null) return;
            if (!_handlers.ContainsKey(cmd)) _handlers[cmd] = new List<Action<string, int, string, string>>();
            _handlers[cmd].Add(handler);
        }

        /// <summary>取消协议回调注册。</summary>
        public void Unregister(string cmd, Action<string, int, string, string> handler)
        {
            if (string.IsNullOrEmpty(cmd) || handler == null) return;
            if (_handlers.TryGetValue(cmd, out var list)) list.Remove(handler);
        }

        /// <summary>发送 WebSocket 命令（需已连接）。</summary>
        public void SendWsCommand(string cmd)
        {
            if (!IsWsConnected) return;
            var json = "{\"cmd\":\"" + Escape(cmd) + "\",\"reqId\":\"" + Guid.NewGuid().ToString("N") + "\"}";
            var bytes = Encoding.UTF8.GetBytes(json);
            _ = _ws.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, _wsCts?.Token ?? default);
        }

        public void EstablishConnection(string account, string password, Action<bool, string> onResult)
        {
            if (string.IsNullOrEmpty(account) || string.IsNullOrEmpty(password)) { onResult?.Invoke(false, "账号或密码为空"); return; }
            var json = "{\"username\":\"" + Escape(account) + "\",\"password\":\"" + Escape(password) + "\"}";
            Post("/api/login", json, (ok, body) =>
            {
                IsConnected = ok;
                if (!ok) { onResult?.Invoke(false, body ?? "请求失败"); return; }
                try
                {
                    var dto = JsonUtility.FromJson<ApiResponseDto>(body);
                    if (dto != null && dto.IsSuccess) { if (!string.IsNullOrEmpty(dto.data)) AuthToken = dto.data; onResult?.Invoke(true, null); }
                    else onResult?.Invoke(false, dto?.msg ?? "登录失败");
                }
                catch (Exception ex) { onResult?.Invoke(false, ex.Message); }
            });
        }

        public void ConnectWebSocket(Action<bool, string> onResult)
        {
            if (string.IsNullOrEmpty(AuthToken)) { onResult?.Invoke(false, "请先登录"); return; }
            CloseWs();
            var wsUrl = BaseUrl.Replace("http://", "ws://").Replace("https://", "wss://").TrimEnd('/') + "/ws?token=" + Uri.EscapeDataString(AuthToken);
            StartCoroutine(ConnectWsCo(wsUrl, onResult));
        }

        IEnumerator ConnectWsCo(string wsUrl, Action<bool, string> onResult)
        {
            _ws = new ClientWebSocket();
            _wsCts = new CancellationTokenSource();
            var task = _ws.ConnectAsync(new Uri(wsUrl), _wsCts.Token);
            while (!task.IsCompleted) yield return null;
            if (task.IsFaulted) { onResult?.Invoke(false, task.Exception?.Message ?? "连接失败"); yield break; }
            if (task.IsCanceled) { onResult?.Invoke(false, "已取消"); yield break; }
            _pending.Enqueue(() => OnWsConnectionChanged?.Invoke(true));
            onResult?.Invoke(true, null); // 建议在此或 OnWsConnectionChanged 中调用各模块 Init（如 PlayerInit.Init、DailySignInit.Init）
            _ = RecvLoop();
        }

        async Task RecvLoop()
        {
            var buf = new byte[4096];
            var cts = _wsCts?.Token ?? CancellationToken.None;
            try
            {
                while (_ws != null && _ws.State == WebSocketState.Open)
                {
                    var seg = new ArraySegment<byte>(buf);
                    var res = await _ws.ReceiveAsync(seg, cts);
                    if (res.MessageType == WebSocketMessageType.Close) break;
                    if (res.MessageType == WebSocketMessageType.Text && res.Count > 0)
                        _msgQueue.Enqueue(Encoding.UTF8.GetString(buf, 0, res.Count));
                }
            }
            catch (ObjectDisposedException) { }
            catch (OperationCanceledException) { }
            finally { _pending.Enqueue(() => OnWsConnectionChanged?.Invoke(false)); }
        }

        void CloseWs()
        {
            _wsCts?.Cancel();
            try { _ws?.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None); } catch (Exception) { }
            _ws?.Dispose();
            _ws = null;
            _wsCts = null;
        }

        public void SendRequest(string path, string jsonBody, Action<bool, string> onResponse)
        {
            if (onResponse == null) return;
            StartCoroutine(PostCo((path ?? "").TrimStart('/'), jsonBody, onResponse));
        }

        static string Escape(string s) => string.IsNullOrEmpty(s) ? "" : s.Replace("\\", "\\\\").Replace("\"", "\\\"");

        void Post(string path, string jsonBody, Action<bool, string> onResponse)
        {
            if (onResponse == null) return;
            StartCoroutine(PostCo(path.TrimStart('/'), jsonBody, onResponse));
        }

        IEnumerator PostCo(string path, string jsonBody, Action<bool, string> onResponse)
        {
            path = string.IsNullOrEmpty(path) ? "" : (path.StartsWith("/") ? path : "/" + path);
            var body = string.IsNullOrEmpty(jsonBody) ? null : Encoding.UTF8.GetBytes(jsonBody);
            using (var req = new UnityWebRequest(BaseUrl + path, "POST"))
            {
                req.timeout = (int)_timeout;
                req.downloadHandler = new DownloadHandlerBuffer();
                if (body != null && body.Length > 0) req.uploadHandler = new UploadHandlerRaw(body);
                req.SetRequestHeader("Content-Type", "application/json");
                req.SetRequestHeader("Accept", "application/json");
                if (!string.IsNullOrEmpty(AuthToken)) req.SetRequestHeader("Authorization", "Bearer " + AuthToken);
                yield return req.SendWebRequest();
                if (req.result == UnityWebRequest.Result.Success)
                    onResponse(true, req.downloadHandler?.text ?? "");
                else
                    onResponse(false, req.error ?? req.downloadHandler?.text ?? "Unknown error");
            }
        }
    }
}
