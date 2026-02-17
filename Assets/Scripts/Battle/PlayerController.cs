using UnityEngine;

namespace DDT.Battle
{
    /// <summary>
    /// 简单的玩家控制器：左右移动 + 调整角度 + 蓄力发射炮弹。
    /// 需要搭配 Rigidbody2D、Collider2D 使用 2D 物理。
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("移动参数")]
        public float moveSpeed = 5f;

        [Header("发射参数")]
        public Transform firePoint;         // 炮口位置
        public GameObject projectilePrefab; // 炮弹预制体
        public float angleSpeed = 60f;      // 每秒角度调整速度（度）
        public float minAngle = 10f;
        public float maxAngle = 80f;

        public float minPower = 10f;
        public float maxPower = 50f;
        public float chargeSpeed = 30f;     // 每秒蓄力增加值

        private float _currentAngle = 45f;
        private float _currentPower;
        private bool _isCharging;

        private void Start()
        {
            _currentPower = minPower;
        }

        private void Update()
        {
            HandleMove();
            HandleAim();
            HandleFire();
        }

        private void HandleMove()
        {
            float h = Input.GetAxisRaw("Horizontal"); // A/D 或 左右键
            if (Mathf.Abs(h) > 0.01f)
            {
                Vector3 delta = new Vector3(h * moveSpeed * Time.deltaTime, 0f, 0f);
                transform.position += delta;
            }
        }

        private void HandleAim()
        {
            if (Input.GetKey(KeyCode.UpArrow))
            {
                _currentAngle += angleSpeed * Time.deltaTime;
            }
            else if (Input.GetKey(KeyCode.DownArrow))
            {
                _currentAngle -= angleSpeed * Time.deltaTime;
            }

            _currentAngle = Mathf.Clamp(_currentAngle, minAngle, maxAngle);

            if (firePoint != null)
            {
                firePoint.localRotation = Quaternion.Euler(0f, 0f, _currentAngle);
            }
        }

        private void HandleFire()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                _isCharging = true;
                _currentPower = minPower;
            }

            if (_isCharging)
            {
                _currentPower += chargeSpeed * Time.deltaTime;
                _currentPower = Mathf.Clamp(_currentPower, minPower, maxPower);
                // 这里可以把 _currentPower 映射到 UI 蓄力条（后续扩展）。
            }

            if (Input.GetKeyUp(KeyCode.Space))
            {
                if (firePoint != null)
                {
                    FireProjectile();
                }
                else
                {
                    Debug.LogWarning("PlayerController: FirePoint 未设置，无法发射炮弹。");
                }

                _isCharging = false;
            }
        }

        /// <summary>
        /// 发射炮弹。
        /// - 如果 inspector 中指定了 projectilePrefab，则实例化该预制体。
        /// - 否则在运行时代码动态创建一个带 Rigidbody2D + CircleCollider2D + Projectile 的简单炮弹对象。
        /// </summary>
        private void FireProjectile()
        {
            GameObject projObj;
            Projectile projectile;

            if (projectilePrefab != null)
            {
                // 使用用户配置的预制体
                projObj = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
                projectile = projObj.GetComponent<Projectile>();
                if (projectile == null)
                {
                    projectile = projObj.AddComponent<Projectile>();
                }
            }
            else
            {
                // 没有提供预制体时，使用代码动态生成一个最简版炮弹
                projObj = new GameObject("Projectile");
                projObj.transform.position = firePoint.position;
                projObj.transform.rotation = firePoint.rotation;

                var rb = projObj.AddComponent<Rigidbody2D>();
                rb.gravityScale = 1f;

                projObj.AddComponent<CircleCollider2D>();

                projectile = projObj.AddComponent<Projectile>();
            }

            projectile.Launch(_currentPower);

            Debug.Log($"发射炮弹，角度={_currentAngle:F1}，力度={_currentPower:F1}");
        }
    }
}

