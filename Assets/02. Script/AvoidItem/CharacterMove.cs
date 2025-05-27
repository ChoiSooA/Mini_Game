using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class CharacterMove : MonoBehaviour
{
    private FallObject fallObject;
    private Rigidbody rb;

    public float moveSpeed = 5f;
    private float moveDirection;
    private float minX = -5f;
    private float maxX = 5f;
    private bool isGameOver = false;

    [Header("Mobile Buttons")]
    public GameObject buttonCanvas;
    Button leftButton;
    Button rightButton;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        fallObject = FindObjectOfType<FallObject>();
    }

    void Start()
    {
        SetEdge();
        StopMovement(); // 시작 시 멈춘 상태

        leftButton = buttonCanvas.transform.GetChild(0).GetComponent<Button>();
        rightButton = buttonCanvas.transform.GetChild(1).GetComponent<Button>();
        buttonCanvas.SetActive(false); // 모바일 버튼 캔버스 활성화

        // 버튼 이벤트 등록 (PC에서도 테스트 가능하게 항상 등록)
        AddButtonEvents(leftButton, MoveLeft, StopMove);
        AddButtonEvents(rightButton, MoveRight, StopMove);
    }

    private void OnEnable()
    {
        GameManager.GameStartEvent += StartMovement;
        GameManager.OnTimerFinishedEvent += StopMovement;
    }

    private void OnDisable()
    {
        GameManager.GameStartEvent -= StartMovement;
        GameManager.OnTimerFinishedEvent -= StopMovement;
    }

    public void StartMovement()
    {
        if (rb == null || !rb) return;
        buttonCanvas.SetActive(true);
        isGameOver = false;
        rb.velocity = Vector3.zero;
    }

    public void StopMovement()
    {
        isGameOver = true;
        buttonCanvas.SetActive(false);
        if (rb != null && rb)
        {
            rb.velocity = Vector3.zero;
        }
    }

    void Update()
    {
        if (isGameOver || rb == null || !rb) return;

        float moveDir = moveDirection; // 기본은 버튼 방향

        float keyInput = Input.GetAxis("Horizontal");
        if (Mathf.Abs(keyInput) > 0.01f)
            moveDir = keyInput;

        rb.velocity = new Vector3(moveDir * moveSpeed, rb.velocity.y, rb.velocity.z);
    }

    void LateUpdate()
    {
        if (rb == null || !rb) return;

        Vector3 clamped = transform.position;
        clamped.x = Mathf.Clamp(clamped.x, minX, maxX);
        transform.position = clamped;
    }

    private void SetEdge()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        float zDistance = Mathf.Abs(cam.transform.position.z - transform.position.z);
        minX = cam.ViewportToWorldPoint(new Vector3(0f, 0.5f, zDistance)).x;
        maxX = cam.ViewportToWorldPoint(new Vector3(1f, 0.5f, zDistance)).x;
    }

    public void MoveLeft() => moveDirection = -1f;
    public void MoveRight() => moveDirection = 1f;
    public void StopMove() => moveDirection = 0f;

    private void AddButtonEvents(Button button, UnityEngine.Events.UnityAction onDown, UnityEngine.Events.UnityAction onUp)
    {
        if (button == null) return;

        EventTrigger trigger = button.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = button.gameObject.AddComponent<EventTrigger>();

        trigger.triggers = new List<EventTrigger.Entry>();

        EventTrigger.Entry entryDown = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerDown
        };
        entryDown.callback.AddListener((eventData) => onDown());
        trigger.triggers.Add(entryDown);

        EventTrigger.Entry entryUp = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerUp
        };
        entryUp.callback.AddListener((eventData) => onUp());
        trigger.triggers.Add(entryUp);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Point"))
        {
            Debug.Log("Point Up!");
            if (GameManager.Instance != null)
                GameManager.Instance.ScoreUp();

            if (fallObject != null)
                fallObject.DeSpawnNow(other.gameObject);
        }
    }
}
