using UnityEngine;
public class PlayerController : MonoBehaviour
{
    public float speed = 5f;
    private void Update()
    {
        Move();
    }
    private void Move()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 movement = new Vector3(h, 0f, v);
        transform.Translate(movement * speed * Time.deltaTime);
    }
}