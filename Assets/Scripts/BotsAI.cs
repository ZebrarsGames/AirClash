using UnityEngine;

public class BotsAI : MonoBehaviour
{
    [Header("Object links")]
    [SerializeField] private Rigidbody2D botRb;
    [SerializeField] private GoalHandler goalHandler;
    [SerializeField] private TimerScr timer;

    [Header("Movement settings")]
    [SerializeField] private Vector2 botStartPos;
    public float moveSpeed;
    public Vector3 puckKoof;

    [Header("AI Boundary")]
    [SerializeField] private float minX;
    [SerializeField] private float maxX;
    [SerializeField] private float minY;
    [SerializeField] private float maxY;

    [Header("Human-like Behavior")]
    [SerializeField] private float reactionSpeed = 15f;

    private Transform puckTransform;
    private TrailRenderer trailRenderer;
    private float baseSpeed;
    private float botOffsetX;
    private float botOffsetY;
    private int score1;
    private int score2;

    private Vector2 currentTargetDestination;
    private float decisionTimer;
    private float currentWanderOffset;

    void Start()
    {
        if(botRb == null) botRb = GetComponent<Rigidbody2D>();
        
        trailRenderer = GetComponent<TrailRenderer>();

        GameObject puckObj = GameObject.FindWithTag("Puck");
        if(puckObj != null) puckTransform = puckObj.transform;

        if(botRb != null) botRb.linearVelocity = Vector2.zero;
        
        moveSpeed = PlayerPrefs.GetFloat("Difficulty", 5f);
        baseSpeed = moveSpeed;
        botOffsetX = PlayerPrefs.GetFloat("BotOffsetX", 0f);
        botOffsetY = PlayerPrefs.GetFloat("BotOffsetY", 0f);

        if(trailRenderer != null)
        {
            trailRenderer.enabled = PlayerPrefs.GetInt("Trail", 1) == 1;
        }

        currentTargetDestination = botStartPos;
    }

    void FixedUpdate()
    {
        if(timer != null && timer.TimerOn) 
        {
            if(botRb != null) botRb.linearVelocity = Vector2.zero;
            return;
        }
        if(puckTransform == null || botRb == null) return;

        Vector2 puckPos = puckTransform.position;
        Vector2 currentBotPos = botRb.position;
        Vector2 idealDestination = currentBotPos;
        float currentSpeed = moveSpeed;

        if(puckPos.x > 0 && puckPos.x < 2f)
        {
            idealDestination.x = (-puckPos.x) - botOffsetX;
            
            decisionTimer += Time.fixedDeltaTime;
            if(decisionTimer > 0.4f)
            {
                currentWanderOffset = (Random.value > 0.5f) ? botOffsetY : -botOffsetY;
                decisionTimer = 0f;
            }
            
            idealDestination.y = puckPos.y + currentWanderOffset;
            currentSpeed = moveSpeed * 0.25f;
        } 
        else if(puckPos.x > 2f)
        {
            idealDestination = botStartPos;
            currentSpeed = moveSpeed * 0.33f;
        } 
        else if((puckPos.x < 0 && (puckPos.y > 4f || puckPos.y < -4f)) || (puckPos.x < -6f && (puckPos.y > 3.5f || puckPos.y < -3.5f)))
        {
            idealDestination = botStartPos;
            currentSpeed = moveSpeed * 0.25f;
        } 
        else if(puckPos.x < -6f)
        {
            idealDestination = puckPos;
            currentSpeed = moveSpeed * 1.5f;
        } 
        else
        {
            idealDestination.x = puckPos.x - puckKoof.x;
            
            if(puckPos.y > 0)
            {
                idealDestination.y = puckPos.y - (puckKoof.y + 0.5f);
            } 
            else if(puckPos.y < 0)
            {
                idealDestination.y = puckPos.y - (puckKoof.y - 0.5f);
            } 
            else
            {
                idealDestination.y = puckPos.y;
            }
            currentSpeed = moveSpeed;
        }

        idealDestination.x = Mathf.Clamp(idealDestination.x, minX, maxX);
        idealDestination.y = Mathf.Clamp(idealDestination.y, minY, maxY);

        currentTargetDestination = Vector2.Lerp(currentTargetDestination, idealDestination, Time.fixedDeltaTime * reactionSpeed);

        Vector2 direction = currentTargetDestination - currentBotPos;
        float sqrDistance = direction.sqrMagnitude;

        if(sqrDistance > 0.0001f)
        {
            float distance = Mathf.Sqrt(sqrDistance);
            float fixedDelta = Time.fixedDeltaTime;
            
            float maxSpeedPossible = fixedDelta > 0f ? (distance / fixedDelta) : currentSpeed;
            float speedThisFrame = Mathf.Min(currentSpeed, maxSpeedPossible);
            
            botRb.linearVelocity = (direction / distance) * speedThisFrame;
        }
        else
        {
            botRb.linearVelocity = Vector2.zero;
        }
    }

    public void UpdateBotSpeed(int s1, int s2)
    {
        score1 = s1;
        score2 = s2;
        int scoreDifference = score1 - score2;

        switch(scoreDifference)
        {
            // EasyMode
            case >= 10:  moveSpeed = baseSpeed * 0.33f; break;
            case >= 7:   moveSpeed = baseSpeed * 0.4f;  break;
            case >= 5:   moveSpeed = baseSpeed * 0.5f;  break;
            case >= 3:   moveSpeed = baseSpeed * 0.66f; break;
            
            // FuryMode
            case <= -10: moveSpeed = baseSpeed * 2.0f;  break;
            case <= -7:  moveSpeed = baseSpeed * 1.7f;  break;
            case <= -5:  moveSpeed = baseSpeed * 1.5f;  break;
            case <= -3:  moveSpeed = baseSpeed * 1.2f;  break;
            
            default:     moveSpeed = baseSpeed;         break;
        }
    }
}