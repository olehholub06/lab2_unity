using UnityEngine;
using UnityEngine.InputSystem; 

public class PlayerController : MonoBehaviour
{
    [Header("Налаштування руху")]
    public float baseSpeed = 10f;
    public float sideSpeed = 5f;
    public float jumpForce = 7f;

    [Header("Налаштування прискорення")]
    public float sprintMultiplier = 1.5f;
    public float maxSprintTime = 3f;
    private float currentSprintTime;

    [Header("Налаштування ям")]
    public float fallThreshold = -5f;

    private Rigidbody rb;
    private Vector3 startPosition;
    private float currentSpeed;
    private bool isGrounded;
    private bool isFinished = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        startPosition = transform.position;
        currentSpeed = baseSpeed;
        currentSprintTime = maxSprintTime;
    }

    void Update()
    {
        if (isFinished || Keyboard.current == null) return;

        HandleJump();
        HandleSprint();
        CheckFall();
    }

    void FixedUpdate()
    {
        if (isFinished || Keyboard.current == null) return;
        HandleMovement();
    }

    void HandleMovement()
    {
        float horizontalInput = 0f;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) horizontalInput = 1f;
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) horizontalInput = -1f;
        Vector3 forwardMovement = transform.forward * currentSpeed;
        Vector3 sideMovement = transform.right * (horizontalInput * sideSpeed);
        Vector3 verticalMovement = new Vector3(0, rb.linearVelocity.y, 0);

        rb.linearVelocity = forwardMovement + sideMovement + verticalMovement;
    }

    void HandleJump()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }
    }

    void HandleSprint()
    {
        if (Keyboard.current.leftShiftKey.isPressed && currentSprintTime > 0)
        {
            currentSpeed = baseSpeed * sprintMultiplier;
            currentSprintTime -= Time.deltaTime;
        }
        else
        {
            currentSpeed = baseSpeed;

            if (currentSprintTime < maxSprintTime)
            {
                currentSprintTime += Time.deltaTime;
            }
        }
    }

    void CheckFall()
    {
        if (transform.position.y < fallThreshold)
        {
            Respawn();
        }
    }

    void Respawn()
    {
        transform.position = startPosition;
        rb.linearVelocity = Vector3.zero;
        currentSprintTime = maxSprintTime;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
        else if (collision.gameObject.CompareTag("Obstacle"))
        {
            Respawn();
        }
        else if (collision.gameObject.CompareTag("Finish"))
        {
            isFinished = true; 

            rb.linearVelocity = Vector3.zero; 
            rb.isKinematic = true; 

            Debug.Log("Фініш! Персонаж завмер на платформі.");
        }
    }
}