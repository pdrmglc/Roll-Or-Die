using UnityEngine;

public class PriestScript : MonoBehaviour
{

    public Rigidbody2D rb;

    [Header("Movement details")]
    public float moveSpeed = 5f;
    private float xInput;
    private float yInput;
    private bool facingRight = true;


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        HandleInput();
        HandleMovement();
        HandleFlip();
    }

    private void HandleInput()
    {
        xInput = Input.GetAxisRaw("Horizontal");
        yInput = Input.GetAxisRaw("Vertical");
    }

        private void HandleMovement()
    {
        rb.linearVelocity = new Vector2(xInput * moveSpeed, yInput * moveSpeed);
    }

    private void HandleFlip()
    {
        if (rb.linearVelocity.x > 0 && facingRight == false)
        {
            Flip();
        }
        else if (rb.linearVelocity.x < 0 && facingRight == true)
        {
            Flip();
        }
    }

    private void Flip()
    {
        facingRight = !facingRight;
        transform.Rotate(0, 180, 0);
    }
}
