using System;
using UnityEngine;
using Network;

namespace Logic.DailySign
{
    /// <summary>
    /// 每日签到模块：单例。Init 时按协议名在 NetworkManager 注册，收到推送后更新数据并触发事件。
    /// </summary>
    public class DailySignControl
    {
        static DailySignControl _instance;
        public static DailySignControl Instance => _instance ??= new DailySignControl();

        public DailySignModel DailySignInfo { get; private set; }
        public event Action<DailySignModel> OnDailySignInfoReceived;
        public event Action<DailySignModel> OnDailySignSuccess;
        public event Action<string> OnDailySignFailed;

        const string CmdInfo = "dailySign.info";
        const string CmdSign = "dailySign.sign";
        Action<string, int, string, string> _handlerInfo;
        Action<string, int, string, string> _handlerSign;
        bool _inited;

        public void Init()
        {
            if (NetworkManager.Instance == null || _inited) return;
            _inited = true;
            _handlerInfo = OnInfo;
            _handlerSign = OnSign;
            NetworkManager.Instance.Register(CmdInfo, _handlerInfo);
            NetworkManager.Instance.Register(CmdSign, _handlerSign);
        }

        public void Dispose()
        {
            if (!_inited || NetworkManager.Instance == null) return;
            NetworkManager.Instance.Unregister(CmdInfo, _handlerInfo);
            NetworkManager.Instance.Unregister(CmdSign, _handlerSign);
            _inited = false;
            if (_instance == this) _instance = null;
        }

        void OnInfo(string reqId, int code, string msg, string data)
        {
            if (code != 0 || string.IsNullOrEmpty(data)) return;
            try
            {
                DailySignInfo = JsonUtility.FromJson<DailySignModel>(data);
                OnDailySignInfoReceived?.Invoke(DailySignInfo);
            }
            catch (Exception) { /* ignore */ }
        }

        void OnSign(string reqId, int code, string msg, string data)
        {
            if (code == 0 && !string.IsNullOrEmpty(data))
            {
                try
                {
                    DailySignInfo = JsonUtility.FromJson<DailySignModel>(data);
                    OnDailySignSuccess?.Invoke(DailySignInfo);
                }
                catch (Exception) { /* ignore */ }
            }
            else
                OnDailySignFailed?.Invoke(msg ?? "签到失败");
        }

        public void RequestDailySignInfo()
        {
            if (NetworkManager.Instance != null && NetworkManager.Instance.IsWsConnected)
                NetworkManager.Instance.SendWsCommand(CmdInfo);
        }

        public void DoDailySign()
        {
            if (NetworkManager.Instance != null && NetworkManager.Instance.IsWsConnected)
                NetworkManager.Instance.SendWsCommand(CmdSign);
        }
    }
}
