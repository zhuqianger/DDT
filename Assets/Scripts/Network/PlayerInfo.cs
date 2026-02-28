using System;

namespace Network
{
    /// <summary>
    /// 与后端 player 表 / PlayerInfoDto 对应的玩家信息（登录后通过 WebSocket player.get 获取）。
    /// </summary>
    [Serializable]
    public class PlayerInfo
    {
        public long id;
        public string username;
        public string nickname;
        public string avatar;
        public int level;
        public long exp;
        public long gold;
        public long diamond;
        public int status;
        public string lastLoginTime;
        public string createTime;
        public string updateTime;
    }
}
