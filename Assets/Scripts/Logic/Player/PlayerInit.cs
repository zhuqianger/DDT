using Network;

namespace Logic.Player
{
    /// <summary>
    /// 玩家模块入口：初始化 Control（向 NetworkManager 注册协议 player.get）。
    /// </summary>
    public static class PlayerInit
    {
        public static void Init()
        {
            if (NetworkManager.Instance == null) return;
            PlayerControl.Instance.Init();
        }
    }
}
