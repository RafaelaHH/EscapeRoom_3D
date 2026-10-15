using UnityEngine;
public class PlayerController : MonoBehaviour
{
    public float speed = 5f;
    public float rotationSpeed = 120f;
    private void Start()
    {
        Debug.Log("Movement system active");
    }
    private void Update()
    {
        Move();
        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("Interaction key pressed");
        }
    }
    private void Move()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 movement = new Vector3(h, 0f, v);
        transform.Translate(movement * speed * Time.deltaTime);
        float turn = 0f;
        if (Input.GetKey(KeyCode.Q)) turn -= 1f;
        if (Input.GetKey(KeyCode.R)) turn += 1f;
        transform.Rotate(0f, turn * rotationSpeed * Time.deltaTime, 0f);
    }
}