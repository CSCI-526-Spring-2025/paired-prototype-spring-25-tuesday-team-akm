using UnityEngine;
using UnityEngine.UI;  // Or use TMPro if you're using TextMeshPro

public class InstructionsManager : MonoBehaviour
{
    public GameObject instructionsPanel;  // Reference to the UI panel or text

    void Start()
    {
        instructionsPanel.SetActive(true);  // Show instructions at the start
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))  // Detect left mouse click
        {
            instructionsPanel.SetActive(false);  // Hide the instructions
        }
    }
}
