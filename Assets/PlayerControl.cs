using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public float baseSpeed = 10f;
    public float sideSpeed = 5f;
    public float jumpForce = 7f;
    public float knockbackForce = 18f;

    private Rigidbody rb;
    private Vector3 startPosition;
    private bool isGrounded;
    private bool isPlayerActive = true;
    private bool isInvulnerable = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        startPosition = transform.position;
    }

    private void OnEnable() => GameManager.OnGameOver += () => isPlayerActive = false;

    private void Update()
    {
        if (!isPlayerActive || isInvulnerable) return;

        if (Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }
    }

    private void FixedUpdate()
    {
        if (!isPlayerActive || isInvulnerable) return;

        float horizontal = 0f;
        if (Keyboard.current.dKey.isPressed) horizontal = 1f;
        if (Keyboard.current.aKey.isPressed) horizontal = -1f;

        Vector3 move = (transform.forward * baseSpeed) + (transform.right * (horizontal * sideSpeed));
        move.y = rb.linearVelocity.y;
        rb.linearVelocity = move;
    }

    private void OnCollisionEnter(Collision col)
    {
        Debug.Log($"<b>[Физика]</b> Вдарився об: {col.gameObject.name} | Тег: {col.gameObject.tag}");

        if (col.gameObject.CompareTag("Ground")) isGrounded = true;

        if (col.gameObject.CompareTag("Obstacle") && isPlayerActive)
        {
            if (isInvulnerable) { Debug.Log("<b>[Player]</b> Удар об Obstacle проігноровано (невразливість)"); return; }
            ApplyKnockback();
        }
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
            if (isInvulnerable) { Debug.Log("<b>[Player]</b> Пастка проігнорована (невразливість)"); return; }
            Debug.Log("<color=red><b>[Player]</b> ВИКЛИКАЮ ШКОДУ ВІД ПАСТКИ</color>");
            ApplyDamage(false);
        }
    }

    private void ApplyKnockback()
    {
        ApplyDamage(false);
        rb.linearVelocity = Vector3.zero;
        Vector3 knockbackDir = -transform.forward + Vector3.up * 0.4f;
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
        yield return new WaitForSeconds(0.4f);
        isInvulnerable = false;
        Debug.Log("<b>[Player]</b> Невразливість закінчилась.");
    }
}