using UnityEngine;

public class IKFootSolver : MonoBehaviour
{
    [Header("Setup")]
    public Transform body;              // Referencia al cuerpo
    public Transform target;            // El target del IK que este script moverá
    public IKFootSolver otherFoot;      // Referencia al script del otro pie para alternar pasos

    [Header("Foot Placement")]
    public float footSpacing;           // Desplazamiento lateral desde el centro del cuerpo
    public LayerMask terrainLayer;      // Capa del terreno
    public float raycastLength = 1f;    // Longitud del raycast hacia abajo

    [Header("Stepping Animation")]
    public float stepDistanceThreshold = 0.2f; // Umbral de distancia para iniciar un nuevo paso
    public float stepHeight = 0.15f;    // Altura del arco del paso
    public float stepSpeed = 5f;        // Velocidad de la animación del paso

    // Estado interno - variables para replicar la lógica de la imagen
    private Vector3 oldPosition;        // Posición de inicio del paso actual
    private Vector3 currentPosition;    // Posición actual calculada del pie (se aplica al target)
    private Vector3 newPosition;        // Posición objetivo del paso actual en el suelo
    private float lerp;                 // Progreso de la interpolación del paso (0 a 1)

    void Start()
    {
        // Determinar la posición inicial en el suelo
        Vector3 initialGroundedPos;
        Vector3 initialRayOrigin = body.position + (body.right * footSpacing);
        Ray ray = new Ray(initialRayOrigin, Vector3.down);

        if (Physics.Raycast(ray, out RaycastHit hit, raycastLength, terrainLayer))
        {
            initialGroundedPos = hit.point;
        }
        else
        {
            // Si no hay hit, usar una posición por defecto relativa al origen del rayo
            initialGroundedPos = initialRayOrigin - Vector3.up * 0.1f;
        }

        // Al inicio, todas las posiciones clave se alinean y el pie está "en el suelo"
        currentPosition = initialGroundedPos;
        newPosition = initialGroundedPos;
        oldPosition = initialGroundedPos;

        if (target != null)
            target.position = currentPosition; // Aplicar la posición inicial al target del IK

        lerp = 1f; // Indica que el pie comienza en el suelo (paso completado)
    }

    void Update()
    {
        // Aplicar la 'currentPosition' calculada en el frame anterior al target del IK.
        // Esto es similar a 'transform.position = currentPosition;' al inicio del Update en la imagen.
        if (target != null)
            target.position = currentPosition;

        // Raycast para detectar la posición ideal en el suelo
        Vector3 rayOrigin = body.position + (body.right * footSpacing);
        Ray पैरRay = new Ray(rayOrigin, Vector3.down); // 'पैरRay' es 'footRay' ;)

        if (Physics.Raycast(पैरRay, out RaycastHit hitInfo, raycastLength, terrainLayer))
        {
            // Condición para iniciar un nuevo paso:
            // 1. El nuevo punto detectado está suficientemente lejos del objetivo actual (newPosition).
            // 2. El otro pie está en el suelo (o no hay referencia al otro pie).
            // 3. El paso actual de este pie ha terminado (lerp >= 1f).
            if (Vector3.Distance(newPosition, hitInfo.point) > stepDistanceThreshold &&
                (otherFoot == null || otherFoot.IsGrounded()) &&
                lerp >= 1f)
            {
                lerp = 0f; // Resetear lerp para iniciar la animación del nuevo paso
                // 'oldPosition' ya fue establecida al final del paso anterior (o en Start).
                // 'newPosition' se convierte en el nuevo punto de impacto.
                newPosition = hitInfo.point;
            }
        }

        // Lógica de animación del paso
        if (lerp < 1f)
        {
            // Interpolar linealmente entre la posición antigua y la nueva
            Vector3 tempFootPosition = Vector3.Lerp(oldPosition, newPosition, lerp);
            // Añadir la altura del paso usando una curva sinusoidal
            tempFootPosition.y += Mathf.Sin(lerp * Mathf.PI) * stepHeight;

            currentPosition = tempFootPosition; // Esta 'currentPosition' se aplicará en el *siguiente* frame
            lerp += Time.deltaTime * stepSpeed;
            lerp = Mathf.Clamp01(lerp); // Asegurar que lerp no exceda 1
        }
        else // lerp >= 1f (el paso ha terminado o no se estaba dando un paso)
        {
            // El pie está (o debería estar) firmemente en 'newPosition'
            currentPosition = newPosition;
            // 'oldPosition' se actualiza a 'newPosition' para el *posible* siguiente paso.
            // Si se inicia un nuevo paso, este 'oldPosition' será el punto de partida.
            oldPosition = newPosition;
        }
    }

    /// <summary>
    /// Indica si este pie ha completado su movimiento y está "en el suelo".
    /// </summary>
    public bool IsGrounded()
    {
        return lerp >= 1f;
    }

    private void OnDrawGizmos()
    {
        if (body == null) return;

        // Visualizar newPosition (objetivo del paso actual)
        Gizmos.color = Color.red;
        Vector3 newPosGizmo = Application.isPlaying ? newPosition : (target ? target.position : transform.position + Vector3.down * 0.1f);
        Gizmos.DrawSphere(newPosGizmo, 0.05f);

        if (Application.isPlaying) // Solo dibujar oldPosition y currentPosition durante el juego
        {
            // Visualizar oldPosition (inicio del paso actual)
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(oldPosition, 0.04f);

            // Visualizar currentPosition (posición animada actual del pie)
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(currentPosition, 0.06f);
        }

        // Dibujar el rayo para depuración
        Vector3 rayOriginGizmo = body.position + (body.right * footSpacing);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(rayOriginGizmo, rayOriginGizmo + Vector3.down * raycastLength);
    }
}