using System;

namespace Logic.Player
{
    /// <summary>
    /// 玩家信息模型，与后端 PlayerInfoDto / player 表对应。
    /// </summary>
    [Serializable]
    public class PlayerModel
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
