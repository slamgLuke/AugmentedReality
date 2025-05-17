using UnityEngine;
using Niantic.Lightship.AR.NavigationMesh;

/// SUMMARY:
/// LightshipNavMeshSample
/// 
/// El BoxCollider seguirá la posición del agente cada frame.
public class NavMeshHowTo : MonoBehaviour
{
    [SerializeField]
    private Camera _camera;

    [SerializeField]
    private LightshipNavMeshAgent _agentPrefab;

    [SerializeField]
    private GameObject _playerPrefab;

    [SerializeField]
    private Transform spawnablesTransform;

    [Header("Player Settings")]
    [SerializeField, Range(0.1f, 20f)]
    private float followSpeed = 5f; // Velocidad de seguimiento del objeto físico (ajustable desde el editor).

    private float pushForce = 5f;

    private LightshipNavMeshAgent _agentInstance;
    private GameObject _playerInstance;
    private Rigidbody _playerRigidbody;


    private bool isRagdoll = false;

    void Update()
    {
        HandleTouch();

        if (_playerInstance != null && _playerInstance.transform.position.y <= -5f)
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
        if (!isRagdoll)
        {
            MovePlayer();
            RotatePlayer();
        }
    }

    private void HandleTouch()
    {
        if (UIManager.IsUIActive)
        {
            return;
        }
        if (UIManager.CurrentMode != UIManager.InteractionMode.Touch)
        {
            return;
        }

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
                    _agentInstance = Instantiate(_agentPrefab, spawnablesTransform.position, Quaternion.identity, spawnablesTransform);
                    _agentInstance.transform.position = hit.point;

                    _playerInstance = Instantiate(_playerPrefab, spawnablesTransform.position, Quaternion.identity, spawnablesTransform);
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
                        // Aplicar fuerza al player (modo ragdoll)
                        Vector3 forceDirection = _camera.transform.forward;
                        forceDirection.y = 0;

                        float forceMagnitude = _playerRigidbody.mass * pushForce;
                        _playerRigidbody.AddForce(forceDirection.normalized * forceMagnitude, ForceMode.Impulse);

                        isRagdoll = true; // Activa el modo ragdoll
                        Debug.Log("Fuerza aplicada al Player en dirección de la cámara.");
                    }
                    else
                    {
                        _agentInstance.transform.position = hit.point;
                        isRagdoll = false; // Sale del modo ragdoll
                    }
                }
            }
        }
    }

    private void MovePlayer()
    {
        if (_playerInstance == null || _agentInstance == null) return;

        if (_playerRigidbody == null)
            _playerRigidbody = _playerInstance.GetComponent<Rigidbody>();

        // Calcular la dirección hacia el agente sin cambiar la altura (Y)
        Vector3 direction = _agentInstance.transform.position - _playerInstance.transform.position;
        direction.y = 0;
        direction = direction.normalized;

        float distance = Vector3.Distance(
            new Vector3(_agentInstance.transform.position.x, 0, _agentInstance.transform.position.z),
            new Vector3(_playerInstance.transform.position.x, 0, _playerInstance.transform.position.z)
        );

        // Solo moverse si está lo suficientemente lejos
        if (distance > 0.1f)
        {
            _playerRigidbody.MovePosition(_playerInstance.transform.position + direction * followSpeed * Time.fixedDeltaTime);
        }
    }

    private void RotatePlayer()
    {
        if (_playerInstance == null || _agentInstance == null) return;

        // Rotar suavemente al player hacia el agente
        Vector3 direction = _agentInstance.transform.position - _playerInstance.transform.position;
        direction.y = 0; // Mantener solo en plano XZ

        if (direction.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            _playerInstance.transform.rotation = Quaternion.Slerp(
                _playerInstance.transform.rotation,
                targetRotation,
                followSpeed * Time.fixedDeltaTime
            );
        }
    }
}
