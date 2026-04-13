using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Налаштування руху")]
    public float baseSpeed = 10f;
    public float sideSpeed = 5f;
    public float jumpForce = 7f;
    public float knockbackForce = 18f;

    private Rigidbody rb;
    private Vector3 startPosition;
    private bool isGrounded;
    private bool isPlayerActive = true;
    private bool isInvulnerable = false;
    private bool isKnockedBack = false;
    private WaitForSeconds invulnTimer;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        startPosition = transform.position;

        invulnTimer = new WaitForSeconds(0.4f);
    }

    private void OnEnable() => GameManager.OnGameOver += DisablePlayer;
    private void OnDisable() => GameManager.OnGameOver -= DisablePlayer;

    private void DisablePlayer()
    {
        isPlayerActive = false;
    }

    private void Update()
    {
        if (!isPlayerActive || isKnockedBack || Keyboard.current == null) return;

        if (Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }
    }

    private void FixedUpdate()
    {
        if (!isPlayerActive || isKnockedBack || Keyboard.current == null) return;

        float horizontal = 0f;
        if (Keyboard.current.dKey.isPressed) horizontal = 1f;
        if (Keyboard.current.aKey.isPressed) horizontal = -1f;

        Vector3 move = (transform.forward * baseSpeed) + (transform.right * (horizontal * sideSpeed));
        move.y = rb.linearVelocity.y; 
        rb.linearVelocity = move;
    }

    private void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;

            if (isKnockedBack)
            {
                isKnockedBack = false;
            }
        }

        if (col.gameObject.CompareTag("Obstacle") && isPlayerActive)
        {
            if (isInvulnerable) return;
            ApplyKnockback();
        }

        if (col.gameObject.CompareTag("Finish") && isPlayerActive)
        {
            ReachFinish();
        }
    }

    private void ReachFinish()
    {
        isPlayerActive = false;
        isInvulnerable = true;

        rb.isKinematic = true;
        rb.linearVelocity = Vector3.zero;

        GameManager.Instance.FinishLevel();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Coin"))
        {
            GameManager.Instance.AddCoin();
            Destroy(other.gameObject);
        }
        if (other.CompareTag("Trap") && isPlayerActive)
        {
            if (isInvulnerable) return;
            ApplyDamage(false);
        }
    }
    private void ApplyKnockback()
    {
        ApplyDamage(false);

        isKnockedBack = true;
        isGrounded = false;

        rb.linearVelocity = Vector3.zero;

        Vector3 knockbackDir = -transform.forward + Vector3.up * 1.0f;
        rb.AddForce(knockbackDir.normalized * knockbackForce, ForceMode.Impulse);
    }

    private void ApplyDamage(bool respawn)
    {
        isInvulnerable = true;

        GameManager.Instance.LoseLife();

        if (GameManager.Instance.remainingLives > 0 && respawn)
        {
            transform.position = startPosition;
        }

        StartCoroutine(ResetInvul());
    }

    IEnumerator ResetInvul()
    {
        yield return invulnTimer;
        isInvulnerable = false;
    }
}