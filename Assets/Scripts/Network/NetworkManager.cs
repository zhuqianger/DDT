using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Network
{
    /// <summary>
    /// 网络管理：发送账号密码建立连接、发送请求、在回调中接收响应。
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
            if (Instance == this) Instance = null;
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

        /// <summary>发送请求，在 onResponse 中接收响应</summary>
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
