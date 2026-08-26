using UnityEngine;
using UnityEngine.EventSystems;

public class PlayersController : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Movement")]
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

    [Header("Internal variables")]
    private Rigidbody2D rb;
    private Camera cam;
    private Vector3 offset;
    private Vector2 targetPos;
    private bool isDragging = false;
    private Color particleColor;
    private GameObject particles;

    void Start()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = PlayerPrefs.GetInt("FPS", 60);
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main;
        targetPos = rb.position;
        float volume = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
        AudioListener.volume = volume;
        SkinData currentSkin = Resources.Load<SkinData>(PlayerPrefs.GetString("CurrentSkin", "DefSkin"));
        ApplySkin(currentSkin);
    }

    void Update()
    {
        if(timer != null && timer.TimerOn && isDragging)
        {
            ResetDragState();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if(timer != null && timer.TimerOn) return; 
        Vector3 mousePos = cam.ScreenToWorldPoint(eventData.position);
        offset = (Vector2)transform.position - (Vector2)mousePos;
        isDragging = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if(timer != null && timer.TimerOn || !isDragging) return; 

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

    private void OnCollisionEnter2D(Collision2D other) {
        if(other.gameObject.name.Equals("Puck") && audioSource != null)
        {
            audioSource.PlayOneShot(puckSound);
        }
    }

    public void ApplySkin(SkinData skin)
    {
        if(gameObject.name.Equals("Player1")) {
            skin = Resources.Load<SkinData>(PlayerPrefs.GetString("CurrentSkin"));
        } 
        else if(gameObject.name.Equals("Player2")) {
            skin = Resources.Load<SkinData>(PlayerPrefs.GetString("CurrentSkin") + "Pl2");
        }
        GetComponent<SpriteRenderer>().sprite = skin.sprite;
        puckSound = skin.sound;
        if(skin.particles != null) { 
            particles = skin.particles;
            var ps = particles.GetComponent<ParticleSystem>();
            var psMain = ps.main;
            particles.gameObject.SetActive(true);
            if(gameObject.name.Equals("Player2") && ColorUtility.TryParseHtmlString("#ff6a6a", out particleColor))
            {
                psMain.startColor = particleColor;
            } else if(gameObject.name.Equals("Player1") && ColorUtility.TryParseHtmlString("#9abaf5", out particleColor))
            {
                psMain.startColor = particleColor;
            }
            var newParticles = Instantiate(particles, GetComponent<Transform>());
            newParticles.gameObject.SetActive(true);
            newParticles.GetComponent<ParticleSystem>().Play(); 
        }
        if(skin.trail != null)
        {
            if(PlayerPrefs.GetInt("Trail", 1) == 1)
            {
                var trail = skin.trail.GetComponent<TrailRenderer>();
                var newTrail = gameObject.GetComponent<TrailRenderer>();
                newTrail.sharedMaterial = trail.sharedMaterial;
                newTrail.time = trail.time;
                newTrail.startWidth = trail.startWidth;
                newTrail.endWidth = trail.endWidth;
                newTrail.colorGradient = trail.colorGradient;
                newTrail.numCornerVertices = trail.numCornerVertices;
                newTrail.numCapVertices = trail.numCapVertices;
                newTrail.alignment = trail.alignment;
                newTrail.textureMode = trail.textureMode;
                gameObject.GetComponent<TrailRenderer>().enabled = true;
            } else gameObject.GetComponent<TrailRenderer>().enabled = false;
        } else
        {
            if(PlayerPrefs.GetInt("Trail", 1) == 1) GetComponent<TrailRenderer>().enabled = true;
            else GetComponent<TrailRenderer>().enabled = false;
        }
    }
}