using UnityEngine;
using TMPro;
using Yarn.Unity;
using System.Collections;
using UnityEngine.InputSystem;

public class OverheadBubbleView : DialogueViewBase
{
    [Header("Bubble Settings")]
    [SerializeField] private float typingSpeed = 0.05f;
    [SerializeField] private GameObject speechBubblePrefab; // Assign in Inspector!
    
    private SpeechBubble currentBubble;
    private SpeechBubble playerBubble;
    private Coroutine typingCoroutine;
    private PlayerInputActions playerInput;
    private bool waitingForInput = false;
    
    void Awake()
    {
        playerInput = new PlayerInputActions();
    }
    
    void OnEnable()
    {
        if (playerInput != null)
        {
            playerInput.Player.Enable();
        }
    }
    
    void OnDisable()
    {
        if (playerInput != null)
        {
            playerInput.Player.Disable();
        }
    }
    
    public override void DialogueStarted()
    {
        Debug.Log("[Dialogue] Started");
        waitingForInput = false;
    }
    
    public override void DialogueComplete()
    {
        Debug.Log("[Dialogue] Complete");
        
        // Hide all bubbles
        if (currentBubble != null)
        {
            currentBubble.Hide();
        }
        
        if (playerBubble != null)
        {
            playerBubble.Hide();
        }
        
        waitingForInput = false;
    }
    
    public override void RunLine(LocalizedLine dialogueLine, System.Action onDialogueLineFinished)
    {
        string speakerName = dialogueLine.CharacterName;
        string text = dialogueLine.TextWithoutCharacterName.Text;
        
        Debug.Log($"[Dialogue] Speaker: {speakerName}, Text: {text}");
        
        // Find speaker and get their bubble
        SpeechBubble bubble = GetBubbleForSpeaker(speakerName);
        
        if (bubble == null)
        {
            Debug.LogError($"[Dialogue] No bubble found for {speakerName}!");
            onDialogueLineFinished();
            return;
        }
        
        // Hide previous bubble if switching speakers
        if (currentBubble != null && currentBubble != bubble)
        {
            currentBubble.Hide();
        }
        
        currentBubble = bubble;
        
        // Stop existing typing
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        
        // Start typing
        typingCoroutine = StartCoroutine(TypeTextAndWait(text, onDialogueLineFinished));
    }
    
    IEnumerator TypeTextAndWait(string text, System.Action onComplete)
    {
        currentBubble.Show();
        currentBubble.SetText("");
        
        // Type out word by word
        string[] words = text.Split(' ');
        
        for (int i = 0; i < words.Length; i++)
        {
            if (i > 0)
            {
                currentBubble.AddWord(" " + words[i]);
            }
            else
            {
                currentBubble.AddWord(words[i]);
            }
            
            yield return new WaitForSeconds(typingSpeed);
        }
        
        // Wait for E key to continue
        Debug.Log("[Dialogue] Waiting for E to continue...");
        waitingForInput = true;
        
        yield return new WaitUntil(() => playerInput.Player.Interact.WasPressedThisFrame());
        
        waitingForInput = false;
        Debug.Log("[Dialogue] E pressed, continuing");
        
        // Signal Yarn Spinner we're done
        onComplete();
    }
    
    public override void DismissLine(System.Action onDismissalComplete)
    {
        // Hide current bubble
        if (currentBubble != null)
        {
            currentBubble.Hide();
        }
        
        // Signal dismissal complete
        onDismissalComplete();
    }
    
    SpeechBubble GetBubbleForSpeaker(string speakerName)
    {
        // Check if it's the Player
        if (speakerName.Equals("Player", System.StringComparison.OrdinalIgnoreCase))
        {
            if (playerBubble == null)
            {
                playerBubble = CreatePlayerBubble();
            }
            return playerBubble;
        }
        
        // Find NPC by name
        NPC[] npcs = FindObjectsOfType<NPC>();
        foreach (NPC npc in npcs)
        {
            if (npc.GetNPCName().Equals(speakerName, System.StringComparison.OrdinalIgnoreCase))
            {
                return npc.GetSpeechBubble();
            }
        }
        
        Debug.LogError($"[Dialogue] Could not find speaker: {speakerName}");
        return null;
    }
    
    SpeechBubble CreatePlayerBubble()
    {
        if (speechBubblePrefab == null)
        {
            Debug.LogError("[Dialogue] Speech Bubble Prefab not assigned in OverheadBubbleView!");
            return null;
        }
        
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("[Dialogue] Player not found!");
            return null;
        }
        
        // Instantiate bubble
        GameObject bubbleObj = Instantiate(speechBubblePrefab);
        SpeechBubble bubble = bubbleObj.GetComponent<SpeechBubble>();
        
        if (bubble != null)
        {
            bubble.SetTarget(player.transform);
            bubble.SetOffset(new Vector3(0, 3.5f, 0));
            bubble.Hide();
            
            Debug.Log("[Dialogue] Created Player bubble");
            return bubble;
        }
        
        Debug.LogError("[Dialogue] Bubble prefab doesn't have SpeechBubble component!");
        return null;
    }
    
    void OnDestroy()
    {
        if (playerInput != null)
        {
            playerInput.Player.Disable();
        }
    }
}