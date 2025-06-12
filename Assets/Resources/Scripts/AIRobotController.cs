using UnityEngine;
using UnityEngine.AI;
using UnityEngine.XR.ARFoundation; 
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

public class AIRobotController : MonoBehaviour
{
    public float moveSpeed = 1.5f;
    public float rotationSpeed = 3f;
    public float repathInterval = 3f;
    public float minDistanceFromPlayer = 0.5f;

    private Transform _player;
    private Rigidbody _rigidbody;
    private Vector3 _targetPosition;
    private float _repathTimer;
    private ARPlaneManager _planeManager;

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _player = GameObject.FindWithTag("Player")?.transform;
        _planeManager = FindObjectOfType<ARPlaneManager>();
        PickNewTarget();
    }

    private void FixedUpdate()
    {
        if (_planeManager == null || _player == null) return;

        _repathTimer += Time.deltaTime;

        float distanceToPlayer = Vector3.Distance(transform.position, _player.position);
        if (_repathTimer >= repathInterval || distanceToPlayer < minDistanceFromPlayer)
        {
            PickNewTarget();
            _repathTimer = 0f;
        }

        MoveTowardTarget();
        RotateTowardTarget();
    }

    private void PickNewTarget()
    {
        var planes = new List<ARPlane>();
        foreach (var plane in _planeManager.trackables)
            planes.Add(plane);

        if (planes.Count == 0) return;

        ARPlane randomPlane = planes[Random.Range(0, planes.Count)];
        Vector2 size = randomPlane.size;

        for (int i = 0; i < 10; i++) 
        {
            Vector3 randomOffset = new Vector3(
                Random.Range(-size.x / 2f, size.x / 2f),
                0,
                Random.Range(-size.y / 2f, size.y / 2f)
            );

            Vector3 candidate = randomPlane.center + randomOffset;
            if (_player != null && Vector3.Distance(candidate, _player.position) > minDistanceFromPlayer)
            {
                _targetPosition = candidate + Vector3.up * 0.1f;
                return;
            }
        }

        _targetPosition = randomPlane.center + Vector3.up * 0.1f;
    }

    private void MoveTowardTarget()
    {
        Vector3 direction = _targetPosition - transform.position;
        direction.y = 0;

        if (direction.magnitude > 0.1f)
        {
            Vector3 move = direction.normalized * moveSpeed * Time.fixedDeltaTime;
            _rigidbody.MovePosition(transform.position + move);
        }
    }

    private void RotateTowardTarget()
    {
        Vector3 direction = _targetPosition - transform.position;
        direction.y = 0;

        if (direction.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
    }
}