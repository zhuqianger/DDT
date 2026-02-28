using Network;

namespace Logic.DailySign
{
    /// <summary>
    /// 每日签到模块入口：初始化 Control（向 NetworkManager 注册协议 dailySign.info / dailySign.sign）。
    /// </summary>
    public static class DailySignInit
    {
        public static void Init()
        {
            if (NetworkManager.Instance == null) return;
            DailySignControl.Instance.Init();
        }
    }
}
