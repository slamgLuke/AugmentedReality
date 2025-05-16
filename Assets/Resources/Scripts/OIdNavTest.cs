using UnityEngine;
using Niantic.Lightship.AR.NavigationMesh;

/// SUMMARY:
/// LightshipNavMeshSample
/// 
/// El BoxCollider seguirá la posición del agente cada frame.
public class TEST : MonoBehaviour
{
    [SerializeField]
    private Camera _camera;

    [SerializeField]
    private LightshipNavMeshAgent _agentPrefab;

    [SerializeField]
    private GameObject _playerPrefab;

    [Header("Player Settings")]
    [SerializeField, Range(0.1f, 20f)]
    private float followSpeed = 5f; // Velocidad de seguimiento del objeto físico (ajustable desde el editor).


    private float pushForce = 5f; 

    private LightshipNavMeshAgent _agentInstance;
    private GameObject _playerInstance;
    private Rigidbody _playerRigidbody;

    void Update()
    {
        HandleTouch();

        if (_playerInstance.transform.position.y <= -5f)
        {
            // Destruir ambos objetos y nulificar referencias
            Destroy(_playerInstance);
            Destroy(_agentInstance.gameObject);

            _playerInstance = null;
            _agentInstance = null;
            _playerRigidbody = null;

            Debug.Log("Player y Agent destruidos por caer debajo de -5 en Y.");
        }

    }

    void FixedUpdate()
    {
        MovePlayer();
    }

    private void HandleTouch()
    {
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
#else
        if (Input.touchCount <= 0) return;

        var touch = Input.GetTouch(0);
        if (touch.phase == UnityEngine.TouchPhase.Began)
#endif
        {
#if UNITY_EDITOR
            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
#else
            Ray ray = _camera.ScreenPointToRay(touch.position);
#endif
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                Debug.Log("HIT!!");
                if (_agentInstance == null)
                {
                    _agentInstance = Instantiate(_agentPrefab);
                    _agentInstance.transform.position = hit.point;

                    _playerInstance = Instantiate(_playerPrefab);
                    _playerInstance.transform.position = hit.point + new Vector3(0, 0.05f, 0);
                    _playerRigidbody = _playerInstance.GetComponent<Rigidbody>();

                    if (_playerRigidbody == null)
                    {
                        Debug.LogError("El PlayerPrefab debe tener un Rigidbody.");
                    }
                }
                else
                {

                    if (hit.collider.gameObject == _playerInstance)
                    {
                        // Dirección de la cámara (hacia adelante)
                        Vector3 forceDirection = _camera.transform.forward;
                        forceDirection.y = 0; // Mantener la fuerza en el plano horizontal

                        // Magnitud proporcional a la masa del Rigidbody del player
                        float forceMagnitude = _playerRigidbody.mass * pushForce; // Ajusta el factor multiplicador según la fuerza deseada
                        _playerRigidbody.AddForce(forceDirection.normalized * forceMagnitude, ForceMode.Impulse);
                        Debug.Log("Fuerza aplicada al Player en dirección de la cámara.");
                    }

                    _agentInstance.SetDestination(hit.point);
                }
            }
        }
    }

    private void MovePlayer()
    {
        if (_playerInstance == null || _agentInstance == null) return;

        // Asegurarse que el Rigidbody esté asignado
        if (_playerRigidbody == null)
            _playerRigidbody = _playerInstance.GetComponent<Rigidbody>();

        // Calcular la dirección hacia el agente sin cambiar la altura (Y)
        Vector3 direction = _agentInstance.transform.position - _playerInstance.transform.position;
        direction.y = 0; // Remover componente vertical
        direction = direction.normalized;

        float distance = Vector3.Distance(
            new Vector3(_agentInstance.transform.position.x, 0, _agentInstance.transform.position.z),
            new Vector3(_playerInstance.transform.position.x, 0, _playerInstance.transform.position.z)
        );

        // Solo moverse si está lo suficientemente lejos
        if (distance > 0.1f)
        {
            // Movimiento del objeto físico siguiendo al agente (solo en XZ)
            _playerRigidbody.MovePosition(_playerInstance.transform.position + direction * followSpeed * Time.fixedDeltaTime);
        }
    }
}
