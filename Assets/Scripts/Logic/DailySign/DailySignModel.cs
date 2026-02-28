using System;

namespace Logic.DailySign
{
    /// <summary>
    /// 每日签到信息模型，与后端 DailySignInfoDto 对应。
    /// </summary>
    [Serializable]
    public class DailySignModel
    {
        public int signInDays;
        public bool signedToday;
        public string lastSignInDate;
    }
}
