using UnityEngine;

namespace DDT.Common
{
    /// <summary>
    /// 游戏入口单例，后续可以在这里初始化配置、网络、声音等。
    /// 建议挂在首个加载的场景（如 Lobby 或 Loading）的一个 GameObject 上。
    /// </summary>
    public class GameEntry : MonoBehaviour
    {
        private static GameEntry _instance;
        public static GameEntry Instance => _instance;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            InitServices();
        }

        /// <summary>
        /// 初始化全局服务/管理器（配置、网络、UI 等）。
        /// 目前仅占位，后续按需扩展。
        /// </summary>
        private void InitServices()
        {
            // TODO: 在这里初始化 GameConfigManager、NetworkClient、UIManager 等全局系统。
        }
    }
}

