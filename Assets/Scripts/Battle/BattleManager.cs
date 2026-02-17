using UnityEngine;

namespace DDT.Battle
{
    /// <summary>
    /// 战斗主控制器：目前只负责初始化和与玩家交互。
    /// 后续可以扩展回合、风力等系统。
    /// </summary>
    public class BattleManager : MonoBehaviour
    {
        [Header("玩家控制器")]
        public PlayerController player;

        private void Start()
        {
            if (player == null)
            {
                player = FindObjectOfType<PlayerController>();
            }

            // 这里可以做战场初始化，比如重设玩家位置等。
        }

        /// <summary>
        /// 预留：当玩家开火结束时回调（以后可用于切换回合）。
        /// </summary>
        public void OnPlayerFire()
        {
            Debug.Log("玩家已开火（本地单人模式下仅日志提示）。");
        }
    }
}

