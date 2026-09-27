using UnityEngine;
using UnityEngine.EventSystems;

public class PlayersController : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public enum PlayerType { Player1, Player2 }

    [Header("Settings")]
    public PlayerType playerType;
    
    [Header("Movement Limits")]
    public float minX;
    public float maxX;
    public float minY;
    public float maxY;
    [SerializeField] private TimerScr timer;

    [Header("Particles")]
    public GameObject particleSkin;
    public GameObject bubbleParticles;
    public GameObject goldParticles;

    [Header("Audio")]
    public AudioSource audioSource;
    [SerializeField] private AudioClip puckSound;

    [Header("Internal Variables")]
    private Rigidbody2D rb;
    private Camera cam;
    private SpriteRenderer spriteRenderer;
    private TrailRenderer trailRenderer;
    
    private Vector2 offset;
    private Vector2 targetPos;
    private bool isDragging = false;
    private GameObject activeParticles;

    private readonly Color P1_PARTICLE_COLOR = new Color(0.604f, 0.729f, 0.961f); // #9abaf5
    private readonly Color P2_PARTICLE_COLOR = new Color(1.0f, 0.416f, 0.416f);   // #ff6a6a

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        trailRenderer = GetComponent<TrailRenderer>();
        cam = Camera.main;
    }

    void Start()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = PlayerPrefs.GetInt("FPS", 60);
        targetPos = rb.position;
        
        AudioListener.volume = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
        
        string baseSkin = PlayerPrefs.GetString("CurrentSkin", "DefSkin");
        string skinPath = playerType == PlayerType.Player1 ? baseSkin : baseSkin + "Pl2";
        
        SkinData currentSkin = Resources.Load<SkinData>(skinPath);
        if (currentSkin != null)
        {
            ApplySkin(currentSkin);
        }
    }

    void Update()
    {
        if(isDragging && timer != null && timer.TimerOn)
        {
            ResetDragState();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if(timer != null && timer.TimerOn) return; 
        
        Vector3 mousePos = cam.ScreenToWorldPoint(eventData.position);
        offset = rb.position - (Vector2)mousePos;
        isDragging = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if(!isDragging || (timer != null && timer.TimerOn)) return; 

        Vector3 mousePos = cam.ScreenToWorldPoint(eventData.position);
        
        Vector2 calculatedPos = new Vector2(mousePos.x + offset.x, mousePos.y + offset.y);

        calculatedPos.x = Mathf.Clamp(calculatedPos.x, minX, maxX);
        calculatedPos.y = Mathf.Clamp(calculatedPos.y, minY, maxY);

        targetPos = calculatedPos;
    }

    private void FixedUpdate()
    {
        if(timer != null && timer.TimerOn)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        MoveRigidbodyPhysically(targetPos);
    }

    public void TeleportToPosition(Vector2 newPos)
    {
        isDragging = false;
        rb.position = newPos;
        rb.linearVelocity = Vector2.zero;
        targetPos = newPos;
    }

    private void MoveRigidbodyPhysically(Vector2 target)
    {
        Vector2 desiredVelocity = (target - rb.position) / Time.fixedDeltaTime;
        rb.linearVelocity = desiredVelocity;
        rb.MovePosition(target);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
    }

    private void ResetDragState()
    {
        isDragging = false;
        targetPos = rb.position;
        rb.linearVelocity = Vector2.zero;
    }

    private void OnCollisionEnter2D(Collision2D other) 
    {
        if(other.gameObject.CompareTag("Puck") && audioSource != null)
        {
            audioSource.PlayOneShot(puckSound);
        }
    }

    public void ApplySkin(SkinData skin)
    {
        spriteRenderer.sprite = skin.sprite;
        puckSound = skin.sound;

        if(skin.particles != null) 
        { 
            activeParticles = skin.particles;
            var ps = activeParticles.GetComponent<ParticleSystem>();
            var psMain = ps.main;
            
            psMain.startColor = playerType == PlayerType.Player1 ? P1_PARTICLE_COLOR : P2_PARTICLE_COLOR;
            
            var newParticles = Instantiate(activeParticles, transform);
            newParticles.gameObject.SetActive(true);
            newParticles.GetComponent<ParticleSystem>().Play(); 
        }

        bool trailsEnabled = PlayerPrefs.GetInt("Trail", 1) == 1;

        if(skin.trail != null && trailsEnabled)
        {
            var sourceTrail = skin.trail.GetComponent<TrailRenderer>();
            
            trailRenderer.sharedMaterial = sourceTrail.sharedMaterial;
            trailRenderer.time = sourceTrail.time;
            trailRenderer.startWidth = sourceTrail.startWidth;
            trailRenderer.endWidth = sourceTrail.endWidth;
            trailRenderer.colorGradient = sourceTrail.colorGradient;
            trailRenderer.numCornerVertices = sourceTrail.numCornerVertices;
            trailRenderer.numCapVertices = sourceTrail.numCapVertices;
            trailRenderer.alignment = sourceTrail.alignment;
            trailRenderer.textureMode = sourceTrail.textureMode;
            
            trailRenderer.enabled = true;
        } 
        else
        {
            trailRenderer.enabled = trailsEnabled;
        }
    }
}