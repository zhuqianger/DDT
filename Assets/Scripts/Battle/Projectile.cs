using UnityEngine;

namespace DDT.Battle
{
    /// <summary>
    /// 简单炮弹逻辑：使用 Rigidbody2D 施加初速度，碰撞后销毁。
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class Projectile : MonoBehaviour
    {
        public float lifeTime = 5f;

        private Rigidbody2D _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        private void Start()
        {
            Destroy(gameObject, lifeTime);
        }

        /// <summary>
        /// 由 PlayerController 调用，按照当前炮口朝向和力度发射。
        /// </summary>
        public void Launch(float power)
        {
            if (_rb == null) return;

            // 使用当前物体的“上”方向（local up）作为发射方向。
            Vector2 dir = transform.up.normalized;
            _rb.velocity = dir * power;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            // 简单处理：碰撞后立即销毁。
            Destroy(gameObject);
        }
    }
}

