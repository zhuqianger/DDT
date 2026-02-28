using System;
using UnityEngine;
using Network;

namespace Logic.Player
{
    /// <summary>
    /// 玩家模块：单例。Init 时按协议名在 NetworkManager 注册，收到 player.get 后更新数据并触发事件。
    /// </summary>
    public class PlayerControl
    {
        static PlayerControl _instance;
        public static PlayerControl Instance => _instance ??= new PlayerControl();

        public PlayerModel PlayerInfo { get; private set; }
        public event Action<PlayerModel> OnPlayerInfoReceived;

        const string CmdPlayerGet = "player.get";
        Action<string, int, string, string> _handlerPlayerGet;

        bool _inited;

        public void Init()
        {
            if (NetworkManager.Instance == null || _inited) return;
            _inited = true;
            _handlerPlayerGet = OnPlayerGet;
            NetworkManager.Instance.Register(CmdPlayerGet, _handlerPlayerGet);
        }

        public void Dispose()
        {
            if (!_inited || NetworkManager.Instance == null) return;
            NetworkManager.Instance.Unregister(CmdPlayerGet, _handlerPlayerGet);
            _inited = false;
            if (_instance == this) _instance = null;
        }

        void OnPlayerGet(string reqId, int code, string msg, string data)
        {
            if (code != 0 || string.IsNullOrEmpty(data)) return;
            try
            {
                PlayerInfo = JsonUtility.FromJson<PlayerModel>(data);
                OnPlayerInfoReceived?.Invoke(PlayerInfo);
            }
            catch (Exception) { /* ignore */ }
        }

        /// <summary>请求玩家信息（需已 Init 且 WS 已连接）。</summary>
        public void RequestPlayerInfo()
        {
            if (NetworkManager.Instance != null && NetworkManager.Instance.IsWsConnected)
                NetworkManager.Instance.SendWsCommand(CmdPlayerGet);
        }
    }
}
