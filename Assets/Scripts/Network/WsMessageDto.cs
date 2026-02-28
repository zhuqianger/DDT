using System;

namespace Network
{
    /// <summary>
    /// WebSocket 消息信封，与后端 WsEnvelope 对应。cmd/reqId 请求，code/msg/data 响应。
    /// </summary>
    [Serializable]
    public class WsMessageDto
    {
        public string cmd;
        public string reqId;
        public int code;
        public string msg;
        public string data;
    }
}
