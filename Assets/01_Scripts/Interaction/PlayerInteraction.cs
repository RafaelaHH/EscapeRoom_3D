using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private float interactionDistance = 2f;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
            TryInteract();
    }

    private void TryInteract()
    {
        Debug.Log("Buscando objeto interactuable...");

        IInteractable closest = null;
        float closestDistance = float.MaxValue;

        foreach (Collider hit in Physics.OverlapSphere(transform.position, interactionDistance))
        {
            IInteractable interactable = hit.GetComponent<IInteractable>();
            if (interactable == null)
                continue;

            float distance = Vector3.Distance(transform.position, hit.transform.position);
            if (distance < closestDistance)
            {
                closest = interactable;
                closestDistance = distance;
            }
        }

        if (closest != null)
            closest.Interact();
    }
}
