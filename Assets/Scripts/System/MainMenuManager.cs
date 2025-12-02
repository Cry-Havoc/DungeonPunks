using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

/// <summary>
/// Manages the main menu UI and game state transitions
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    public static MainMenuManager Instance { get; private set; }

    [Header("Menu Panels")]
    public GameObject mainMenuPanel;
    public GameObject optionsPanel;
    public GameObject creditsPanel;

    [Header("Main Menu Buttons")]
    public Button startContinueButton;
    public Button optionsButton;
    public Button creditsButton;
    public Button exitButton;

    [Header("Button Text")]
    public TextMeshProUGUI startContinueButtonText;

    [Header("Options UI")]
    public Slider volumeSlider;
    public TextMeshProUGUI volumeValueText;
    public Button optionsBackButton;

    [Header("Credits UI")]
    public TextMeshProUGUI creditsText;
    public Button creditsBackButton;
    public TextAsset creditsFile;

    [Header("Settings")]
    public string gameSceneName = "GameScene";

    private bool isGameLoaded = false;
    private bool isLoading = false;
    private bool isGamePaused = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializeMenu();
    }

    void Start()
    {
        // Check if running in WebGL
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            if (exitButton != null)
            {
                exitButton.gameObject.SetActive(false);
            }
        }
    }

    void Update()
    {
        // Handle Escape key to toggle pause
        if (Input.GetKeyDown(KeyCode.Escape) && isGameLoaded && !isLoading)
        {
            TogglePause();
        }
    }

    /// <summary>
    /// Initializes menu buttons and UI
    /// </summary>
    void InitializeMenu()
    {
        // Main Menu Buttons
        if (startContinueButton != null)
            startContinueButton.onClick.AddListener(OnStartContinueClicked);

        if (optionsButton != null)
            optionsButton.onClick.AddListener(OnOptionsClicked);

        if (creditsButton != null)
            creditsButton.onClick.AddListener(OnCreditsClicked);

        if (exitButton != null)
            exitButton.onClick.AddListener(OnExitClicked);

        // Options Panel
        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
            volumeSlider.value = PlayerPrefs.GetFloat("MasterVolume", 1f);
        }

        if (optionsBackButton != null)
            optionsBackButton.onClick.AddListener(OnOptionsBack);

        // Credits Panel
        if (creditsBackButton != null)
            creditsBackButton.onClick.AddListener(OnCreditsBack);

        // Load credits from file
        LoadCredits();

        // Show main menu, hide others
        ShowMainMenu();

        // Set initial button text
        UpdateStartButtonText();
    }

    /// <summary>
    /// Updates the start/continue button text
    /// </summary>
    void UpdateStartButtonText()
    {
        if (startContinueButtonText != null)
        {
            if (isLoading)
            {
                startContinueButtonText.text = "Loading...";
            }
            else if (isGameLoaded)
            {
                startContinueButtonText.text = "Continue Game";
            }
            else
            {
                startContinueButtonText.text = "Start Game";
            }
        }
    }

    /// <summary>
    /// Handles start/continue button click
    /// </summary>
    void OnStartContinueClicked()
    {
        if (isLoading) return;

        if (isGameLoaded)
        {
            // Continue - just hide menu
            HideMenu();
            ResumeGame();
        }
        else
        {
            // Start - load game
            StartCoroutine(LoadGameScene());
        }
    }

    /// <summary>
    /// Loads the game scene additively
    /// </summary>
    IEnumerator LoadGameScene()
    {
        isLoading = true;
        UpdateStartButtonText();

        // Disable buttons during loading
        SetButtonsInteractable(false);

        // Load game scene additively
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(gameSceneName, LoadSceneMode.Additive);

        // Wait until scene is loaded
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // Set the game scene as active
        Scene gameScene = SceneManager.GetSceneByName(gameSceneName);
        if (gameScene.isLoaded)
        {
            SceneManager.SetActiveScene(gameScene);
        }

        isLoading = false;
        isGameLoaded = true;
        UpdateStartButtonText();

        // Re-enable buttons
        SetButtonsInteractable(true);

        // Hide menu
        HideMenu();

        // Enable game input
        ResumeGame();

        Debug.Log("Game scene loaded and started");
    }

    /// <summary>
    /// Toggles pause state
    /// </summary>
    void TogglePause()
    {
        if (isGamePaused)
        {
            ResumeGame();
            HideMenu();
        }
        else
        {
            PauseGame();
            ShowMenu();
        }
    }

    /// <summary>
    /// Pauses the game
    /// </summary>
    void PauseGame()
    {
        isGamePaused = true;
        Time.timeScale = 0f;

        // Disable game input
        if (PlayerPartyController.Instance != null)
        {
            PlayerPartyController.Instance.enabled = false;
        }

        // Disable UI input (character selection, etc.)
        SetGameInputEnabled(false);

        Debug.Log("Game paused");
    }

    /// <summary>
    /// Resumes the game
    /// </summary>
    void ResumeGame()
    {
        isGamePaused = false;
        Time.timeScale = 1f;

        // Enable game input
        if (PlayerPartyController.Instance != null)
        {
            PlayerPartyController.Instance.enabled = true;
        }

        // Enable UI input
        SetGameInputEnabled(true);

        Debug.Log("Game resumed");
    }

    /// <summary>
    /// Enables/disables game input handling
    /// </summary>
    void SetGameInputEnabled(bool enabled)
    {
        // Store state for other systems to check
        PlayerPrefs.SetInt("InputEnabled", enabled ? 1 : 0);
    }

    /// <summary>
    /// Shows the main menu
    /// </summary>
    void ShowMenu()
    {
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);
        }

        ShowMainMenu();
        UpdateStartButtonText();
    }

    /// <summary>
    /// Hides the entire menu
    /// </summary>
    void HideMenu()
    {
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(false);
        }

        if (optionsPanel != null)
        {
            optionsPanel.SetActive(false);
        }

        if (creditsPanel != null)
        {
            creditsPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Shows main menu panel
    /// </summary>
    void ShowMainMenu()
    {
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);

        if (optionsPanel != null)
            optionsPanel.SetActive(false);

        if (creditsPanel != null)
            creditsPanel.SetActive(false);
    }

    /// <summary>
    /// Handles options button click
    /// </summary>
    void OnOptionsClicked()
    {
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);

        if (optionsPanel != null)
            optionsPanel.SetActive(true);
    }

    /// <summary>
    /// Handles credits button click
    /// </summary>
    void OnCreditsClicked()
    {
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);

        if (creditsPanel != null)
            creditsPanel.SetActive(true);
    }

    /// <summary>
    /// Handles exit button click
    /// </summary>
    void OnExitClicked()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif

        Debug.Log("Exiting game");
    }

    /// <summary>
    /// Handles volume slider change
    /// </summary>
    void OnVolumeChanged(float value)
    {
        // Update audio listener volume
        AudioListener.volume = value;

        // Save to PlayerPrefs
        PlayerPrefs.SetFloat("MasterVolume", value);
        PlayerPrefs.Save();

        // Update display text
        if (volumeValueText != null)
        {
            volumeValueText.text = Mathf.RoundToInt(value * 100) + "%";
        }

        Debug.Log($"Volume changed to {value * 100}%");
    }

    /// <summary>
    /// Handles options back button
    /// </summary>
    void OnOptionsBack()
    {
        ShowMainMenu();
    }

    /// <summary>
    /// Handles credits back button
    /// </summary>
    void OnCreditsBack()
    {
        ShowMainMenu();
    }

    /// <summary>
    /// Loads credits from text file
    /// </summary>
    void LoadCredits()
    {
        if (creditsText != null && creditsFile != null)
        {
            creditsText.text = creditsFile.text;
        }
        else if (creditsText != null)
        {
            // Default credits if no file is assigned
            creditsText.text = "GUTTER KNIGHT\n\n" +
                             "A Dungeon Crawler RPG\n\n" +
                             "Created with Unity\n\n" +
                             "Thank you for playing!";
        }
    }

    /// <summary>
    /// Enables/disables menu buttons
    /// </summary>
    void SetButtonsInteractable(bool interactable)
    {
        if (startContinueButton != null)
            startContinueButton.interactable = interactable;

        if (optionsButton != null)
            optionsButton.interactable = interactable;

        if (creditsButton != null)
            creditsButton.interactable = interactable;

        if (exitButton != null)
            exitButton.interactable = interactable;
    }

    /// <summary>
    /// Public method to check if input should be blocked
    /// </summary>
    public static bool IsInputEnabled()
    {
        return PlayerPrefs.GetInt("InputEnabled", 1) == 1;
    }

    /// <summary>
    /// Public method to check if game is paused
    /// </summary>
    public static bool IsGamePaused()
    {
        return Instance != null && Instance.isGamePaused;
    }
}