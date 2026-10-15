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
        IInteractable closest = null;
        GameObject closestObject = null;
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
                closestObject = hit.gameObject;
                closestDistance = distance;
            }
        }

        if (closest != null)
        {
            Debug.Log("Interactuando con " + closestObject.name + ".");
            closest.Interact();
        }
        else
        {
            Debug.Log("No hay nada con qué interactuar.");
        }
    }
}
