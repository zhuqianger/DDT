using System;
using System.Collections.Generic;

namespace DDT.Network
{
    /// <summary>
    /// 统一的网络管理中心。
    /// - 所有协议发送入口：Send(protoName, message)
    /// - 所有协议接收入口：Dispatch(protoName, message)
    /// - 各功能模块（如背包、装备等）只需要注册协议名和回调。
    /// </summary>
    public sealed class NetworkManager
    {
        private static readonly Lazy<NetworkManager> _instance =
            new Lazy<NetworkManager>(() => new NetworkManager());

        public static NetworkManager Instance => _instance.Value;

        /// <summary>
        /// key: 协议名（通常为 proto 消息名）
        /// value: 该协议对应的回调列表
        /// </summary>
        private readonly Dictionary<string, List<Action<object>>> _handlers =
            new Dictionary<string, List<Action<object>>>(StringComparer.Ordinal);

        /// <summary>
        /// 真实网络发送实现，可由外部（例如 Socket/HTTP 封装层）进行注入。
        /// 参数：协议名 + 已经构造好的消息对象（通常是 protobuf 生成的类实例）。
        /// </summary>
        public Action<string, object> Sender { get; set; }

        private NetworkManager()
        {
        }

        /// <summary>
        /// 注册某个协议名对应的回调。
        /// </summary>
        public void RegisterHandler(string protoName, Action<object> handler)
        {
            if (string.IsNullOrEmpty(protoName)) throw new ArgumentException("protoName is null or empty", nameof(protoName));
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            if (!_handlers.TryGetValue(protoName, out var list))
            {
                list = new List<Action<object>>();
                _handlers[protoName] = list;
            }

            if (!list.Contains(handler))
            {
                list.Add(handler);
            }
        }

        /// <summary>
        /// 取消注册某个协议名的指定回调。
        /// </summary>
        public void UnregisterHandler(string protoName, Action<object> handler)
        {
            if (string.IsNullOrEmpty(protoName) || handler == null) return;

            if (_handlers.TryGetValue(protoName, out var list))
            {
                list.Remove(handler);
                if (list.Count == 0)
                {
                    _handlers.Remove(protoName);
                }
            }
        }

        /// <summary>
        /// 对外发送协议。内部只负责分发到 Sender，由 Sender 负责真正落地（序列化、写入 Socket 等）。
        /// </summary>
        public void Send(string protoName, object message)
        {
            if (string.IsNullOrEmpty(protoName))
                throw new ArgumentException("protoName is null or empty", nameof(protoName));

            if (Sender == null)
            {
                // 这里可以根据需要改为抛异常或记录日志
                return;
            }

            Sender(protoName, message);
        }

        /// <summary>
        /// 从底层网络收到消息后，由底层调用该方法进行分发。
        /// </summary>
        public void Dispatch(string protoName, object message)
        {
            if (string.IsNullOrEmpty(protoName)) return;

            if (_handlers.TryGetValue(protoName, out var list))
            {
                // 拷贝一份，避免回调里再注册/反注册导致遍历问题
                var snapshot = list.ToArray();
                for (int i = 0; i < snapshot.Length; i++)
                {
                    snapshot[i]?.Invoke(message);
                }
            }
        }
    }
}

