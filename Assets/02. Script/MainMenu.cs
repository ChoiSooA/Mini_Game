using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    GameManager gameManager;
    public Button minMaxButton;
    public Button AvoidButton;

    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        minMaxButton.onClick.AddListener(() => gameManager.ChangeScene("MinMax"));
        AvoidButton.onClick.AddListener(() => gameManager.ChangeScene("AvoidItem"));
    }
}
