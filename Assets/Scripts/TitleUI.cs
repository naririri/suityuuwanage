using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

// public class TitleUI : MonoBehaviour
// {
//     [SerializeField] private Button _gameStartButton;
//     // Start is called before the first frame update
//     void Start()
//     {
//         _gameStartButton.onClick.AddListener(() =>
//         {
//             //SceneManager.LoadScene("main"); // メインシーンの名前に変更
//             UnityEngine.SceneManagement.SceneManager.LoadScene("Main");
//         });
//     }
// }


public class TitleUI : MonoBehaviour
{
    public TMP_Dropdown difficultyDropdown;

    public void OnStartButtonClicked()
    {
        // ドロップダウンの選択値を難易度に変換
        switch (difficultyDropdown.value)
        {
            case 0:
                GameSettings.SelectedDifficulty = CurrentManager.Difficulty.Easy;
                break;
            case 1:
                GameSettings.SelectedDifficulty = CurrentManager.Difficulty.Normal;
                break;
            case 2:
                GameSettings.SelectedDifficulty = CurrentManager.Difficulty.Hard;
                break;
            default:
                GameSettings.SelectedDifficulty = CurrentManager.Difficulty.Easy;
                break;
        }

        // ゲームシーンへ遷移
        SceneManager.LoadScene("Main");
    }
}
