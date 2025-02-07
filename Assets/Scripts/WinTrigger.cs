using UnityEngine;
using UnityEngine.UI;

public class WinTrigger : MonoBehaviour
{
    public GameObject winText; // 引用 UI 文本对象

    private void Start()
    {
        if (winText != null)
        {
            winText.SetActive(false); // 开始时隐藏
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) // 检查是否是 Player
        {
            if (winText != null)
            {
                winText.SetActive(true); // 显示 "You Win!"
            }
            Debug.Log("You Win!"); // 在控制台输出信息
        }
    }
}
