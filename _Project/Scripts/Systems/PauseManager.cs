using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem; // <--- ОБЯЗАТЕЛЬНО ДОБАВЬ ЭТО

public class PauseManager : MonoBehaviour
{
    [Header("Панель паузы")]
    [SerializeField] private GameObject _pausePanel;
    
    private bool _isPaused = false;

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (_isPaused) Resume();
            else Pause();
        }
    }

    public void Resume()
    {
        _pausePanel.SetActive(false);
        Time.timeScale = 1f;
        _isPaused = false;
    }

    public void Pause()
    {
        _pausePanel.SetActive(true);
        Time.timeScale = 0f;
        _isPaused = true;
    }

    public void SaveAndExit()
    {
        GameManager gm = Object.FindFirstObjectByType<GameManager>();
        
        if (gm != null)
        {
            gm.SaveCurrentGame();
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}