using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro; // Добавляем поддержку TextMeshPro
using TheUnique.Core.SaveSystem;

public class MainMenuController : MonoBehaviour
{
    [Header("Панели интерфейса")]
    [SerializeField] private GameObject _mainPanel;
    [SerializeField] private GameObject _newGamePopup;
    [SerializeField] private GameObject _settingsPanel;

    [Header("Ввод данных для нового мира")]
    [SerializeField] private TMP_InputField _widthInput;
    [SerializeField] private TMP_InputField _heightInput;
    [SerializeField] private Button _loadButton;

    [Header("Настройки графики")]
    [SerializeField] private TMP_Dropdown _resolutionDropdown;
    [SerializeField] private Toggle _fullScreenToggle;

    private Resolution[] _resolutions;

    private void Start()
    {
        ShowMainPanel();

        if (_loadButton != null)
            _loadButton.interactable = SaveManager.HasSaveFile();

        SetupSettings();
    }

    public void ShowMainPanel()
    {
        _mainPanel.SetActive(true);
        _newGamePopup.SetActive(false);
        _settingsPanel.SetActive(false);
    }

    public void OpenNewGamePopup()
    {
        _mainPanel.SetActive(false);
        _newGamePopup.SetActive(true);
    }

    public void OpenSettings()
    {
        _mainPanel.SetActive(false);
        _settingsPanel.SetActive(true);
    }

    public void StartNewGame()
    {
        int width = string.IsNullOrEmpty(_widthInput.text) ? 50 : int.Parse(_widthInput.text);
        int height = string.IsNullOrEmpty(_heightInput.text) ? 50 : int.Parse(_heightInput.text);

        PlayerPrefs.SetInt("WorldWidth", width);
        PlayerPrefs.SetInt("WorldHeight", height);
        PlayerPrefs.SetInt("IsLoadingSave", 0);
        PlayerPrefs.Save();

        SceneManager.LoadScene("MainScene");
    }

    public void LoadExistingGame()
    {
        PlayerPrefs.SetInt("IsLoadingSave", 1);
        PlayerPrefs.Save();
        SceneManager.LoadScene("MainScene");
    }

    public void ExitGame() => Application.Quit();

    private void SetupSettings()
    {
        _resolutions = Screen.resolutions;
        _resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();
        int currentResIndex = 0;

        for (int i = 0; i < _resolutions.Length; i++)
        {
            string option = _resolutions[i].width + " x " + _resolutions[i].height;
            options.Add(option);

            if (_resolutions[i].width == Screen.currentResolution.width &&
                _resolutions[i].height == Screen.currentResolution.height)
            {
                currentResIndex = i;
            }
        }

        _resolutionDropdown.AddOptions(options);
        _resolutionDropdown.value = currentResIndex;
        _resolutionDropdown.RefreshShownValue();
        
        _fullScreenToggle.isOn = Screen.fullScreen;
    }

    public void SetResolution(int index)
    {
        Resolution res = _resolutions[index];
        Screen.SetResolution(res.width, res.height, Screen.fullScreen);
    }

    public void SetFullScreen(bool isFull) => Screen.fullScreen = isFull;
}