using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TitleUI : MonoBehaviour
{
    [SerializeField] private Button _gameStartButton;
    // Start is called before the first frame update
    void Start()
    {
        _gameStartButton.onClick.AddListener(() =>
        {
            //SceneManager.LoadScene("main"); // メインシーンの名前に変更
            UnityEngine.SceneManagement.SceneManager.LoadScene("Main");
        });
    }
}