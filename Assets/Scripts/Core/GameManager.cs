using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using Yarn.Unity;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    [Header("Game State")]
    [SerializeField] private bool isGamePaused = false;
    
    [Header("References")]
    [SerializeField] private FeatherManager featherManager;
    [SerializeField] private GameObject player;
    
    [Header("Settings")]
    [SerializeField] private bool startWithCursorLocked = true;
    
    private PlayerInputActions gameInput;
    private DialogueRunner dialogueRunner; // ADD THIS

    void Awake()
    {
        // Singleton pattern
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
        
        // Initialize input for UI actions
        gameInput = new PlayerInputActions();
        gameInput.Player.Enable();
        
        InitializeGame();
    }

    void InitializeGame()
    {
        Debug.Log("GameManager: Initializing game...");
        
        // Find dialogue runner
        dialogueRunner = FindObjectOfType<DialogueRunner>(); // ADD THIS
        
        // Set up cursor
        if (startWithCursorLocked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        
        // Initialize systems in order
        InitializeFeatherSystem();
        InitializePlayer();
        
        Debug.Log("GameManager: Game initialized!");
    }

    void InitializeFeatherSystem()
    {
        if (featherManager == null)
        {
            featherManager = FindObjectOfType<FeatherManager>();
        }
        
        if (featherManager != null)
        {
            Debug.Log("GameManager: Feather system initialized");
        }
        else
        {
            Debug.LogWarning("GameManager: No FeatherManager found!");
        }
    }

    void InitializePlayer()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }
        
        if (player != null)
        {
            Debug.Log("GameManager: Player initialized");
        }
        else
        {
            Debug.LogWarning("GameManager: No Player found!");
        }
    }

    void Update()
    {
        // ESC to toggle pause - using New Input System
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            // Don't allow pausing during dialogue
            if (dialogueRunner == null || !dialogueRunner.IsDialogueRunning)
            {
                TogglePause();
            }
        }
    }

    public void TogglePause()
    {
        isGamePaused = !isGamePaused;
        
        if (isGamePaused)
        {
            PauseGame();
        }
        else
        {
            ResumeGame();
        }
    }

    void PauseGame()
    {
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Debug.Log("Game Paused");
    }

    void ResumeGame()
    {
        Time.timeScale = 1f;
        
        // CHANGED: Only lock cursor if NOT in dialogue
        if (dialogueRunner == null || !dialogueRunner.IsDialogueRunning)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        
        Debug.Log("Game Resumed");
    }

    public bool IsGamePaused()
    {
        return isGamePaused;
    }

    // ADD THIS PUBLIC METHOD:
    public bool IsInDialogue()
    {
        return dialogueRunner != null && dialogueRunner.IsDialogueRunning;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    void OnDestroy()
    {
        if (gameInput != null)
        {
            gameInput.Player.Disable();
        }
    }
}