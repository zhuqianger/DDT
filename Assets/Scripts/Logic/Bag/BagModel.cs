using System;
using System.Collections.Generic;

namespace DDT.Logic.Bag
{
    /// <summary>
    /// 背包内部数据与逻辑处理
    /// 仅负责本地数据，不关心网络细节。
    /// </summary>
    public class BagModel
    {
        /// <summary>
        /// key: itemId, value: BagItemData
        /// </summary>
        private readonly Dictionary<long, BagItemData> _items = new Dictionary<long, BagItemData>();

        /// <summary>
        /// 背包整体刷新事件（如从服务器拉取完整列表）
        /// </summary>
        public event Action OnBagRefreshed;

        /// <summary>
        /// 单个道具数量变化事件
        /// </summary>
        public event Action<long, int> OnItemCountChanged;

        public IReadOnlyDictionary<long, BagItemData> Items => _items;

        /// <summary>
        /// 用服务器返回的完整列表重建本地背包
        /// </summary>
        public void ResetBag(IEnumerable<BagItemData> itemList)
        {
            _items.Clear();
            if (itemList != null)
            {
                foreach (var item in itemList)
                {
                    if (item == null) continue;
                    _items[item.ItemId] = item.Clone();
                }
            }

            OnBagRefreshed?.Invoke();
        }

        /// <summary>
        /// 更新（或新增）某个道具的数量
        /// </summary>
        public void SetItemCount(long itemId, int count)
        {
            if (!_items.TryGetValue(itemId, out var data))
            {
                data = new BagItemData
                {
                    Id = 0,
                    ItemId = itemId,
                    Count = 0
                };
            }

            data.Count = Math.Max(0, count);
            _items[itemId] = data;

            OnItemCountChanged?.Invoke(itemId, data.Count);
        }

        /// <summary>
        /// 在本地尝试扣除一定数量的道具，成功返回 true
        /// （是否真正生效以服务器结果为准）
        /// </summary>
        public bool TryConsumeItem(long itemId, int count)
        {
            if (count <= 0) return false;

            if (!_items.TryGetValue(itemId, out var data) || data.Count < count)
            {
                return false;
            }

            data.Count -= count;
            _items[itemId] = data;

            OnItemCountChanged?.Invoke(itemId, data.Count);
            return true;
        }
    }

    /// <summary>
    /// 与 bag.proto 中 BagItem 字段对应的本地结构
    /// </summary>
    [Serializable]
    public class BagItemData
    {
        public long Id;
        public long ItemId;
        public int Count;

        public BagItemData Clone()
        {
            return new BagItemData
            {
                Id = Id,
                ItemId = ItemId,
                Count = Count
            };
        }
    }
}

