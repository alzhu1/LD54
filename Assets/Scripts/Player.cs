using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class Player : MonoBehaviour {
    [SerializeField] private float moveSpeed;
    [SerializeField] private float jumpVelocity;
    [SerializeField] private Transform groundCheckTransform;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask winLayer;

    [SerializeField] private CinemachineVirtualCamera[] cameras;
    [SerializeField] private Vector2[] startPositions;
    [SerializeField] private int[] numJumpsInLevel;
    private int levelIndex;

    private int numJumpsLeft = 1;
    private int NumJumpsLeft {
        get { return numJumpsLeft; }
        set {
            numJumpsLeft = value;
            EventBus.instance.TriggerOnJump(numJumpsLeft);
        }
    }

    private float horizontal;

    private bool won;
    private bool canMove;
    private bool shouldJump;
    private bool grounded;

    private Animator animator;
    private Rigidbody2D rb;

    void Awake() {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Start() {
        EventBus.instance.OnLevelStart += ReceiveLevelStartEvent;
        EventBus.instance.OnLevelFail += ReceiveLevelEndEvent;
        EventBus.instance.OnLevelComplete += ReceiveLevelEndEvent;

        StartCoroutine(StartGame());
    }

    void OnDestroy() {
        EventBus.instance.OnLevelStart -= ReceiveLevelStartEvent;
        EventBus.instance.OnLevelFail -= ReceiveLevelEndEvent;
        EventBus.instance.OnLevelComplete -= ReceiveLevelEndEvent;
    }

    void Update() {
        if (!canMove) {
            animator.SetBool("Sleeping", true);
            horizontal = 0;
            shouldJump = false;

            if (!won && Input.GetKeyDown(KeyCode.R)) {
                EventBus.instance.TriggerOnLevelStart(levelIndex);
            }
            return;
        }

        Vector3 localScale = transform.localScale;
        if (rb.velocity.x > 0.01) {
            localScale.x = 1;
        } else if (rb.velocity.x < -0.01) {
            localScale.x = -1;
        }
        transform.localScale = localScale;

        horizontal = Input.GetAxisRaw("Horizontal");

        if (NumJumpsLeft > 0 && grounded && !shouldJump && Input.GetButtonDown("Jump")) {
            shouldJump = true;
        }

        animator.SetFloat("xSpeed", Mathf.Abs(rb.velocity.x));
        animator.SetFloat("yVelocity", rb.velocity.y);
        animator.SetBool("Grounded", grounded);
        animator.SetBool("Sleeping", false);
    }

    void FixedUpdate() {
        Vector2 currVelocity = rb.velocity;
        currVelocity.x = horizontal * moveSpeed * Time.fixedDeltaTime;

        bool groundedBefore = grounded;
        grounded = Physics2D.OverlapCircle(groundCheckTransform.position, 0.02f, groundLayer);

        if (shouldJump) {
            NumJumpsLeft--;
            currVelocity.y = jumpVelocity;
            shouldJump = false;
        }

        // Level fail check
        if (!groundedBefore && grounded) {
            // Touched ground for first time, check if we win or lose
            if (Physics2D.OverlapCircle(groundCheckTransform.position, 0.02f, winLayer)) {
                EventBus.instance.TriggerOnLevelComplete();
                StartCoroutine(CompleteLevel());
            } else if (NumJumpsLeft <= 0) {
                EventBus.instance.TriggerOnLevelFail();
            }
        }

        rb.velocity = currVelocity;
    }

    void ReceiveLevelStartEvent(int levelIndex) {
        // Set limited jumps for next/restarted level
        canMove = true;
        if (levelIndex >= 0 && levelIndex < numJumpsInLevel.Length) {
            this.levelIndex = levelIndex;
            NumJumpsLeft = numJumpsInLevel[levelIndex];

            for (int i = 0; i < cameras.Length; i++) {
                cameras[i].m_Priority = i == levelIndex ? 10 : 0;
            }

            transform.position = startPositions[levelIndex];
        }
    }
    
    void ReceiveLevelEndEvent() {
        // Win or lose, stop moving
        canMove = false;
    }

    IEnumerator StartGame() {
        // Hacky, but ensures all events are subscribed to
        canMove = true;
        yield return new WaitForSeconds(0.25f);
        EventBus.instance.TriggerOnLevelStart(0);
    }

    IEnumerator CompleteLevel() {
        won = true;
        yield return new WaitForSeconds(1f);
        won = false;
        levelIndex = (levelIndex + 1) % numJumpsInLevel.Length;
        EventBus.instance.TriggerOnLevelStart(levelIndex);
    }
}
