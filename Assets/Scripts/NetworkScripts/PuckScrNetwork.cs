using Mirror;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PuckScrNetwork : NetworkBehaviour
{
    private Rigidbody2D puckRb;
    public float maxSpeed = 22f;
    [SerializeField] private TimerScr timer;

    [Header("Network Smoothing")]
    [SerializeField] private float snapThreshold = 5.0f;
    [SerializeField] private float errorCorrectionFactor = 8f;

    private float ignoreServerUntil = 0f;
    [SerializeField] private float hitCooldown = 0.25f;

    [SyncVar(hook = nameof(OnServerStateReceived))]
    private PuckState serverState;

    private Vector2 targetServerPos;
    private Vector2 targetServerVel;
    private bool hasNetworkTarget = false;

    private struct PuckState
    {
        public Vector2 position;
        public Vector2 velocity;
        public double serverTime;
    }

    private void Awake()
    {
        puckRb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        puckRb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        
        puckRb.interpolation = RigidbodyInterpolation2D.Interpolate;
        
        syncInterval = 0.016f;
    }

    void FixedUpdate()
    {
        CapVelocity();
        PreventStuckLogic();

        if(isServer)
        {
            serverState = new PuckState
            {
                position = puckRb.position,
                velocity = puckRb.linearVelocity,
                serverTime = NetworkTime.time
            };
        }
        else if(isClient)
        {
            ApplyClientSmoothing();
        }
    }

    private void ApplyClientSmoothing()
    {
        if(Time.time < ignoreServerUntil || !hasNetworkTarget) return;

        Vector2 positionError = targetServerPos - puckRb.position;

        if(positionError.sqrMagnitude > snapThreshold * snapThreshold)
        {
            puckRb.position = targetServerPos;
            puckRb.linearVelocity = targetServerVel;
            hasNetworkTarget = false;
            return;
        }

        Vector2 correctionVelocity = targetServerVel + (positionError * errorCorrectionFactor);
        puckRb.linearVelocity = Vector2.ClampMagnitude(correctionVelocity, maxSpeed);
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if(isServer && GoalHandlerNetwork.Instance != null)
        {
            GoalHandlerNetwork.Instance.OnPuckCollisionEnter2D(other);
        }

        if(isClient && !isServer)
        {
            NetworkIdentity netId = other.gameObject.GetComponent<NetworkIdentity>();
            
            if(netId != null && netId.isLocalPlayer)
            {
                ignoreServerUntil = Time.time + hitCooldown;
                hasNetworkTarget = false;

                CmdApplyClientHit(puckRb.linearVelocity, other.gameObject.transform.position);
            }
        }
    }

    [Command(requiresAuthority = false)]
    private void CmdApplyClientHit(Vector2 clientPuckVelocity, Vector2 clientMalletPos)
    {
        float distToMallet = Vector2.Distance(puckRb.position, clientMalletPos);

        if(distToMallet < 2.5f)
        {
            puckRb.linearVelocity = clientPuckVelocity;

            serverState = new PuckState
            {
                position = puckRb.position,
                velocity = puckRb.linearVelocity,
                serverTime = NetworkTime.time
            };
        }
    }

    private void OnServerStateReceived(PuckState oldState, PuckState newState)
    {
        if(isServer || newState.position == Vector2.zero) return;

        bool isReset = newState.velocity == Vector2.zero;

        if(isReset)
        {
            puckRb.position = newState.position;
            puckRb.linearVelocity = Vector2.zero;
            hasNetworkTarget = false;
            ignoreServerUntil = 0f;
            return;
        }

        if(Time.time < ignoreServerUntil) return;

        float latency = Mathf.Clamp((float)(NetworkTime.time - newState.serverTime), 0f, 0.04f);
        
        targetServerPos = newState.position + (newState.velocity * latency);
        targetServerVel = newState.velocity;
        hasNetworkTarget = true;
    }

    public void ClientForceReset(Vector2 newPosition)
    {
        ignoreServerUntil = 0f;
        hasNetworkTarget = false;
        targetServerVel = Vector2.zero;
        targetServerPos = newPosition;

        if(puckRb != null)
        {
            puckRb.position = newPosition;
            puckRb.transform.position = newPosition;
            puckRb.linearVelocity = Vector2.zero;
            puckRb.angularVelocity = 0f;
        }
    }

    private void CapVelocity()
    {
        if(puckRb.linearVelocity.magnitude > maxSpeed)
        {
            puckRb.linearVelocity = Vector2.ClampMagnitude(puckRb.linearVelocity, maxSpeed);
        }
    }

    private void PreventStuckLogic()
    {
        bool isTimerActive = timer != null && timer.TimerOn;

        if(puckRb.linearVelocityY < 0.1f && puckRb.linearVelocityY > -0.1f && !isTimerActive && puckRb.linearVelocityX != 0)
        {
            puckRb.linearVelocity = new Vector2(puckRb.linearVelocity.x, 0.1f * Mathf.Sign(puckRb.linearVelocity.y == 0 ? 1 : puckRb.linearVelocity.y));
        }

        if(puckRb.linearVelocityX < 0.1f && puckRb.linearVelocityX > -0.1f && !isTimerActive && puckRb.linearVelocityY != 0)
        {
            puckRb.linearVelocity = new Vector2(0.1f * Mathf.Sign(puckRb.linearVelocity.x == 0 ? 1 : puckRb.linearVelocity.x), puckRb.linearVelocity.y);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(!isServer) return;
        GoalHandlerNetwork.Instance.ServerProcessGoal(collision);
    }
}