using UnityEngine;
using UnityEngine.InputSystem;
using Yarn.Unity;

public class NPC : MonoBehaviour
{
    [Header("NPC Info")]
    [SerializeField] private string npcName = "NPC";
    [SerializeField] private string yarnNodeName = "NPC_Start";
    
    [Header("Speech Bubble")]
    [SerializeField] private GameObject speechBubblePrefab;
    private SpeechBubble speechBubble;
    
    [Header("Interaction")]
    [SerializeField] private float interactionRange = 3f;
    [SerializeField] private bool rotatePlayerWhenNearby = true;
    
    private DialogueRunner dialogueRunner;
    private Transform player;
    private PlayerController playerController;
    
    void Start()
    {
    dialogueRunner = FindObjectOfType<DialogueRunner>();
    if (dialogueRunner == null)
    {
        Debug.LogError($"[NPC {npcName}] DialogueRunner not found!");
        return;
    }
    
    player = GameObject.FindGameObjectWithTag("Player")?.transform;
    if (player == null)
    {
        Debug.LogError($"[NPC {npcName}] Player not found! Assign 'Player' tag.");
        return;
    }
    
    playerController = player.GetComponent<PlayerController>();
    
    if (speechBubblePrefab != null)
    {
        GameObject bubbleObj = Instantiate(speechBubblePrefab);
        speechBubble = bubbleObj.GetComponent<SpeechBubble>();
        
        if (speechBubble != null)
        {
            speechBubble.SetTarget(transform);
            
            // CHANGED: Lower offset to match player bubble height
            speechBubble.SetOffset(new Vector3(0, 2.5f, 0)); // Was 3.5, now 2.5
            
            speechBubble.Hide();
        }
    }
    else
    {
        Debug.LogError($"[NPC {npcName}] Speech Bubble Prefab not assigned!");
    }
    
    dialogueRunner.onDialogueStart.AddListener(OnDialogueStart);
    dialogueRunner.onDialogueComplete.AddListener(OnDialogueEnd);
    }
    
    void Update()
    {
        if (player == null || dialogueRunner == null) return;
        
        float distance = Vector3.Distance(transform.position, player.position);
        
        // Smooth rotation when nearby (before dialogue)
        if (distance <= interactionRange && !dialogueRunner.IsDialogueRunning)
        {
            FacePlayer();
            
            if (rotatePlayerWhenNearby && playerController != null && playerController.enabled)
            {
                MakePlayerFaceNPC();
            }
        }
        
        // E to start dialogue
        if (distance <= interactionRange && !dialogueRunner.IsDialogueRunning)
        {
            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                StartDialogue();
            }
        }
        
        // ESC to cancel
        if (dialogueRunner.IsDialogueRunning)
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                dialogueRunner.Stop();
            }
        }
    }
    
    void FacePlayer()
    {
        // NPC faces Player
        Vector3 direction = player.position - transform.position;
        direction.y = 0;
        
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 6f);
        }
    }
    
    void MakePlayerFaceNPC()
    {
        // Player faces NPC (smooth)
        Vector3 direction = transform.position - player.position;
        direction.y = 0;
        
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            player.rotation = Quaternion.Slerp(player.rotation, targetRotation, Time.deltaTime * 6f);
        }
    }
    
    void StartDialogue()
    {
        if (string.IsNullOrEmpty(yarnNodeName))
        {
            Debug.LogError($"[NPC {npcName}] Yarn Node Name not set!");
            return;
        }
        
        Debug.Log($"[NPC {npcName}] Starting dialogue: {yarnNodeName}");
        dialogueRunner.StartDialogue(yarnNodeName);
    }
    
    void OnDialogueStart()
    {
        Debug.Log($"[NPC {npcName}] Dialogue started");
        
        // SIMPLE: Just disable player movement
        // The smooth rotation has already positioned them correctly!
        if (playerController != null)
        {
            playerController.enabled = false;
        }
        
        // NO rotation snap - trust the smooth rotation from Update()
    }
    
    void OnDialogueEnd()
    {
        Debug.Log($"[NPC {npcName}] Dialogue ended");
        
        // Re-enable player movement
        if (playerController != null)
        {
            playerController.enabled = true;
        }
        
        // Hide bubble
        if (speechBubble != null)
        {
            speechBubble.Hide();
        }
    }
    
    public SpeechBubble GetSpeechBubble()
    {
        return speechBubble;
    }
    
    public string GetNPCName()
    {
        return npcName;
    }
    
    void OnDestroy()
    {
        if (dialogueRunner != null)
        {
            dialogueRunner.onDialogueStart.RemoveListener(OnDialogueStart);
            dialogueRunner.onDialogueComplete.RemoveListener(OnDialogueEnd);
        }
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}