using UnityEngine;
using UnityEngine.UI;

public class WinTrigger : MonoBehaviour
{
    public GameObject winText; // ���� UI �ı�����

    private void Start()
    {
        if (winText != null)
        {
            winText.SetActive(false); // ��ʼʱ����
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) // ����Ƿ��� Player
        {
            if (winText != null)
            {
                winText.SetActive(true); // ��ʾ "You Win!"
            }
            Debug.Log("You Win!"); // �ڿ���̨�����Ϣ
        }
    }
}
