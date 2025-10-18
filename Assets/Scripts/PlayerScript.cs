using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    public Rigidbody2D rb;
    public float speed = 5f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    
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
        float moveX = Input.GetAxis("Horizontal");
        // float moveY = Input.GetAxis("Vertical");
        rb.linearVelocity = new Vector2(moveX * speed, rb.linearVelocity.y);
    }
}
