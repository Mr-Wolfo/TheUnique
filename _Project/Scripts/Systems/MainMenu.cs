using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TheUnique.Core.SaveSystem;

public class MainMenu : MonoBehaviour
{
    public Button LoadGameButton;

    private void Start()
    {
        if (LoadGameButton != null)
        {
            LoadGameButton.interactable = SaveManager.HasSaveFile();
        }
    }

    public void OnNewGameClicked()
    {
        PlayerPrefs.SetInt("IsLoadingSave", 0);
        PlayerPrefs.Save();
        SceneManager.LoadScene("MainScene");
    }

    public void OnLoadGameClicked()
    {
        PlayerPrefs.SetInt("IsLoadingSave", 1);
        PlayerPrefs.Save();
        SceneManager.LoadScene("MainScene");
    }

    public void OnExitClicked()
    {
        Application.Quit();
    }
}