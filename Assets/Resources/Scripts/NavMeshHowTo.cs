using UnityEngine;

/// SUMMARY:
/// LightshipNavMeshSample
/// 
/// El BoxCollider seguirá la posición del agente cada frame.
public class NavMeshHowTo : MonoBehaviour
{
    [SerializeField]
    private Camera _camera;

    [SerializeField]
    private GameObject _agentPrefab;

    [SerializeField]
    private GameObject _playerPrefab;

    [SerializeField]
    private Transform spawnablesTransform;

    [Header("Player Settings")]
    [SerializeField, Range(0.1f, 20f)]
    private float followSpeed = 5f; // Velocidad de seguimiento del objeto físico.

    [SerializeField, Range(10f, 500f)]
    private float rotationSpeed = 100f; // Velocidad de rotación ajustable.

    private float pushForce = 5f;

    private GameObject _agentInstance;
    private GameObject _playerInstance;
    private Rigidbody _playerRigidbody;
    private Transform modelTransform; // Asigna aquí el transform del modelo hijo.


    private bool isRagdoll = false;

    void Update()
    {
        HandleTouch();

        if (_playerInstance != null && _playerInstance.transform.position.y <= -5f)
        {
            // Destruir ambos objetos y nulificar referencias
            Destroy(_playerInstance);
            if (_agentInstance != null) // Check if agentInstance still exists
            {
                Destroy(_agentInstance.gameObject);
            }

            _playerInstance = null;
            _agentInstance = null;
            _playerRigidbody = null;
            modelTransform = null;

            Debug.Log("Player y Agent destruidos por caer debajo de -5 en Y.");
        }
    }

    void FixedUpdate()
    {
        if (!isRagdoll)
        {
            MovePlayer();
            RotatePlayer();
            TiltPlayerModel();
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

        Vector2 screenPosition = Vector2.zero;
        bool inputBegan = false;

#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
        {
            screenPosition = Input.mousePosition;
            inputBegan = true;
        }
#else
        if (Input.touchCount > 0)
        {
            var touch = Input.GetTouch(0);
            if (touch.phase == UnityEngine.TouchPhase.Began)
            {
                screenPosition = touch.position;
                inputBegan = true;
            }
        }
#endif

        if (inputBegan)
        {
            // Check if the touch/click is in the top 10% of the screen
            if (screenPosition.y > Screen.height * 0.9f)
            {
                // Debug.Log("Touch in top 10% of screen, ignoring for agent movement.");
                return; // Ignore touches in the top 10% of the screen
            }

            Ray ray = _camera.ScreenPointToRay(screenPosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                Debug.Log("HIT!!");
                if (_agentInstance == null)
                {
                    _agentInstance = Instantiate(_agentPrefab, spawnablesTransform.position, Quaternion.identity, spawnablesTransform);
                    _agentInstance.transform.position = hit.point;

                    _playerInstance = Instantiate(_playerPrefab, spawnablesTransform.position, Quaternion.identity, spawnablesTransform);
                    _playerInstance.transform.position = hit.point + new Vector3(0, 0.1f, 0);
                    _playerRigidbody = _playerInstance.GetComponent<Rigidbody>();

                    modelTransform = _playerInstance.transform.GetChild(0); // Obtiene el primer hijo


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
                rotationSpeed * Time.fixedDeltaTime
            );
        }
    }


    private void TiltPlayerModel()
    {
        if (_playerInstance == null || modelTransform == null) return;

        float speed = _playerRigidbody.linearVelocity.magnitude;

        // Ajustar el ángulo proporcionalmente a la velocidad, con un límite máximo
        float maxTiltAngle = 90f; // Máximo ángulo de inclinación permitido
        float targetTilt = Mathf.Clamp(-speed * 2f, -maxTiltAngle, maxTiltAngle);

        modelTransform.localRotation = Quaternion.Slerp(
            modelTransform.localRotation,
            Quaternion.Euler(targetTilt, 0, 0),
            Time.fixedDeltaTime * 5f
        );
    }
}