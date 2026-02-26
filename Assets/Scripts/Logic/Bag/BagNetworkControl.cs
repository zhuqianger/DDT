using System;
using System.Collections.Generic;
using DDT.Logic.Bag;

namespace DDT.Network
{
    /// <summary>
    /// 背包相关的网络收发控制。
    /// 所有协议都通过统一的 NetworkManager 进行发送与分发。
    /// </summary>
    public class BagNetworkControl
    {
        private readonly BagModel _bagModel;

        // 通过协议名（消息名）进行收发与路由
        private const string PROTO_GET_BAG_REQ = "GetPlayerBagReq";
        private const string PROTO_GET_BAG_RESP = "GetPlayerBagResp";
        private const string PROTO_USE_ITEM_REQ = "UseItemReq";
        private const string PROTO_USE_ITEM_RESP = "UseItemResp";

        public BagNetworkControl(BagModel bagModel)
        {
            _bagModel = bagModel ?? throw new ArgumentNullException(nameof(bagModel));

            RegisterHandlers();
        }

        private void RegisterHandlers()
        {
            NetworkManager.Instance.RegisterHandler(PROTO_GET_BAG_RESP, OnGetBagResp);
            NetworkManager.Instance.RegisterHandler(PROTO_USE_ITEM_RESP, OnUseItemResp);
        }

        /// <summary>
        /// 主动向服务器请求背包数据（playerId 由长连接上下文携带）
        /// </summary>
        public void RequestBag()
        {
            var req = new GetPlayerBagReq
            {
            };

            NetworkManager.Instance.Send(PROTO_GET_BAG_REQ, req);
        }

        /// <summary>
        /// 向服务器发送使用道具请求（playerId 由长连接上下文携带）
        /// </summary>
        public void RequestUseItem(long itemId, int count)
        {
            var req = new UseItemReq
            {
                ItemId = itemId,
                Count = count
            };

            NetworkManager.Instance.Send(PROTO_USE_ITEM_REQ, req);
        }

        /// <summary>
        /// 收到服务器返回的背包数据
        /// </summary>
        private void OnGetBagResp(object message)
        {
            if (!(message is GetPlayerBagResp resp))
            {
                return;
            }

            var list = new List<BagItemData>();
            if (resp.Items != null)
            {
                foreach (var item in resp.Items)
                {
                    if (item == null) continue;
                    list.Add(new BagItemData
                    {
                        Id = item.Id,
                        ItemId = item.ItemId,
                        Count = item.Count
                    });
                }
            }

            _bagModel.ResetBag(list);
        }

        /// <summary>
        /// 收到服务器返回的使用道具结果
        /// </summary>
        private void OnUseItemResp(object message)
        {
            if (!(message is UseItemResp resp))
            {
                return;
            }

            // 这里假设 result == 0 表示成功
            if (resp.Result == 0)
            {
                _bagModel.SetItemCount(resp.ItemId, resp.LeftCount);
            }
            else
            {
                // TODO: 这里可以根据错误码做提示或重同步背包
            }
        }
    }

}

