using UnityEngine;

// Mirar con el ratón en primera persona: el cuerpo gira a los lados y la cámara sube o baja.
// Q y R de PlayerController siguen funcionando igual. Clic para capturar el ratón.
public class FirstPersonLook : MonoBehaviour
{
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private float sensitivity = 2f;
    [SerializeField] private float maxPitch = 75f;

    private float pitch;

    private void OnEnable()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if (Cursor.lockState != CursorLockMode.Locked || cameraPivot == null)
            return;

        transform.Rotate(0f, Input.GetAxis("Mouse X") * sensitivity, 0f);
        pitch = Mathf.Clamp(pitch - Input.GetAxis("Mouse Y") * sensitivity, -maxPitch, maxPitch);
        cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }
}
