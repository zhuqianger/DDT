using UnityEngine;

namespace DDT.Battle
{
    /// <summary>
    /// Battle 场景引导脚本：在运行时通过代码动态生成最小可玩战斗场景。
    /// - 自动创建地面（BoxCollider2D）。
    /// - 自动创建带 PlayerController 的玩家对象及其 FirePoint。
    /// - 自动创建 BattleManager 并绑定玩家引用。
    /// - 炮弹可以使用代码动态生成的默认外观，也可以通过 inspector 指定自定义预制体。
    /// </summary>
    public class BattleBootstrap : MonoBehaviour
    {
        [Header("可选：自定义炮弹预制体")]
        [Tooltip("如果留空，则在运行时使用代码创建一个简单的炮弹 GameObject。")]
        public GameObject projectilePrefab;

        [Header("玩家出生与地面参数")]
        public Vector2 playerSpawnPosition = new Vector2(-4f, -2f);
        public float groundY = -3f;
        public float groundWidth = 20f;
        public float groundThickness = 1f;

        private void Start()
        {
            CreateGround();
            var player = CreatePlayer();
            CreateBattleManager(player);
        }

        /// <summary>
        /// 创建简易地面，仅包含 BoxCollider2D，用于承载玩家/炮弹。
        /// </summary>
        private void CreateGround()
        {
            var ground = new GameObject("Ground");
            ground.transform.position = new Vector3(0f, groundY, 0f);

            var collider = ground.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(groundWidth, groundThickness);
        }

        /// <summary>
        /// 创建带 PlayerController、Rigidbody2D、BoxCollider2D 的玩家对象，并自动创建 FirePoint。
        /// </summary>
        private PlayerController CreatePlayer()
        {
            var playerObj = new GameObject("Player");
            playerObj.transform.position = playerSpawnPosition;

            var rb = playerObj.AddComponent<Rigidbody2D>();
            rb.gravityScale = 1f;

            playerObj.AddComponent<BoxCollider2D>();

            var controller = playerObj.AddComponent<PlayerController>();

            // 创建 FirePoint 子物体，作为炮口位置与朝向
            var firePoint = new GameObject("FirePoint").transform;
            firePoint.SetParent(playerObj.transform);
            firePoint.localPosition = new Vector3(0.5f, 0.5f, 0f);

            controller.firePoint = firePoint;
            controller.projectilePrefab = projectilePrefab;

            return controller;
        }

        /// <summary>
        /// 创建 BattleManager 并绑定玩家引用。
        /// </summary>
        private void CreateBattleManager(PlayerController player)
        {
            var battleManagerObj = new GameObject("BattleManager");
            var battleManager = battleManagerObj.AddComponent<BattleManager>();
            battleManager.player = player;
        }
    }
}

