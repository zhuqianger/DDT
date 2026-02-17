using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DDT.Lobby
{
    /// <summary>
    /// 大厅控制器：负责响应"开始单人战斗"按钮，切换到 Battle 场景。
    /// </summary>
    public class LobbyManager : MonoBehaviour
    {
        [Header("UI 引用")]
        [SerializeField] private Button startBattleButton;

        private void Awake()
        {
            // 如果没有在 Inspector 中指定按钮，则通过名称查找
            if (startBattleButton == null)
            {
                GameObject buttonObj = GameObject.Find("StartBattleButton");
                if (buttonObj != null)
                {
                    startBattleButton = buttonObj.GetComponent<Button>();
                }
            }

            // 绑定按钮点击事件
            if (startBattleButton != null)
            {
                startBattleButton.onClick.AddListener(OnClickStartSingleBattle);
                Debug.Log("已成功绑定 StartBattleButton 点击事件");
            }
            else
            {
                Debug.LogError("未找到 StartBattleButton 按钮！请确保场景中存在名为 StartBattleButton 的按钮对象。");
            }
        }

        private void OnDestroy()
        {
            // 清理事件监听，避免内存泄漏
            if (startBattleButton != null)
            {
                startBattleButton.onClick.RemoveListener(OnClickStartSingleBattle);
            }
        }

        /// <summary>
        /// UI 按钮点击后调用：加载 Battle 场景。
        /// </summary>
        private void OnClickStartSingleBattle()
        {
            Debug.Log("OnClickStartSingleBattle - 开始加载 Battle 场景");
            SceneManager.LoadScene("Battle");
        }
    }
}
