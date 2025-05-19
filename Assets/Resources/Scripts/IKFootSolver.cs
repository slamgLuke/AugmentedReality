using UnityEngine;

public class IKFootSolver : MonoBehaviour
{
    [Header("Setup")]
    public Transform body;              // Referencia al cuerpo
    public Transform target;            // El target del IK que este script moverá
    public IKFootSolver otherFoot;      // Referencia al script del otro pie para alternar pasos

    [Header("Foot Placement")]
    public float footSpacing;           // Desplazamiento lateral desde el centro del cuerpo
    public float forwardOffset = 0.2f;  // Desplazamiento hacia adelante siempre
    public LayerMask terrainLayer;      // Capa del terreno
    public float raycastLength = 1f;    // Longitud del raycast hacia abajo

    [Header("Stepping Animation")]
    public float stepDistanceThreshold = 0.2f; // Umbral de distancia para iniciar un nuevo paso
    public float stepHeight = 0.15f;    // Altura del arco del paso
    public float stepSpeed = 5f;        // Velocidad de la animación del paso

    private Vector3 oldPosition;
    private Vector3 currentPosition;
    private Vector3 newPosition;
    private float lerp;

    void Start()
    {
        UpdateFootTarget();
        lerp = 1f;
    }

    void Update()
    {
        if (target != null)
            target.position = currentPosition;

        if (lerp >= 1f) UpdateFootTarget();

        if (lerp < 1f)
        {
            Vector3 tempFootPosition = Vector3.Lerp(oldPosition, newPosition, lerp);
            tempFootPosition.y += Mathf.Sin(lerp * Mathf.PI) * stepHeight;

            currentPosition = tempFootPosition;
            lerp += Time.deltaTime * stepSpeed;
            lerp = Mathf.Clamp01(lerp);
        }
        else
        {
            currentPosition = newPosition;
            oldPosition = newPosition;
        }
    }

    private void UpdateFootTarget()
    {
        Vector3 rayOrigin = body.position + (body.right * footSpacing) + (body.forward * forwardOffset);
        Ray ray = new Ray(rayOrigin, Vector3.down);

        if (Physics.Raycast(ray, out RaycastHit hit, raycastLength, terrainLayer))
        {
            if (Vector3.Distance(newPosition, hit.point) > stepDistanceThreshold &&
                (otherFoot == null || otherFoot.IsGrounded()))
            {
                lerp = 0f;
                newPosition = hit.point;
            }
        }
    }

    public bool IsGrounded() => lerp >= 1f;

    private void OnDrawGizmos()
    {
        if (body == null) return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(newPosition, 0.05f);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(body.position + (body.right * footSpacing) + (body.forward * forwardOffset),
                        body.position + (body.right * footSpacing) + (body.forward * forwardOffset) + Vector3.down * raycastLength);
    }
}
