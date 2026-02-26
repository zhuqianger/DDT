using System;

namespace Network
{
    /// <summary>
    /// 与 Java 后端统一返回格式对接的 DTO。常见格式：{ "code": 0, "msg": "ok", "data": {} }
    /// 使用 JsonUtility.FromJson&lt;ApiResponseDto&gt;(responseBody) 解析。
    /// </summary>
    [Serializable]
    public class ApiResponseDto
    {
        public int code;
        public string msg;
        public string data; // 业务数据 JSON 字符串，可再按需反序列化

        public bool IsSuccess => code == 0;
    }
}
