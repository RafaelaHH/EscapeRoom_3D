using UnityEngine;

// Evita que el jugador atraviese paredes y muebles. PlayerController se mueve con
// transform.Translate (sin física), así que después de moverse empujamos al jugador
// hacia afuera de cualquier collider que esté tocando.
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerCollision : MonoBehaviour
{
    private CapsuleCollider capsule;
    private readonly Collider[] nearby = new Collider[24];

    private void Awake()
    {
        capsule = GetComponent<CapsuleCollider>();
    }

    private void LateUpdate()
    {
        Vector3 center = transform.TransformPoint(capsule.center);
        float searchRadius = capsule.height * 0.5f + capsule.radius;
        int count = Physics.OverlapSphereNonAlloc(center, searchRadius, nearby, ~0, QueryTriggerInteraction.Ignore);

        for (int i = 0; i < count; i++)
        {
            Collider other = nearby[i];
            if (other == capsule)
                continue;

            if (Physics.ComputePenetration(capsule, transform.position, transform.rotation,
                                           other, other.transform.position, other.transform.rotation,
                                           out Vector3 direction, out float distance))
            {
                direction.y = 0f; // solo hacia los costados: el piso no empuja
                transform.position += direction * distance;
            }
        }
    }
}
