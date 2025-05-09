using UnityEngine;
using Niantic.Lightship.AR.NavigationMesh;

/// SUMMARY:
/// LightshipNavMeshSample
/// This sample shows how to use LightshipNavMesh to add user driven point and click navigation.
/// When you first touch the screen, it will place your agent prefab.
/// Tapping a location moves the agent to that location.
/// The toggle button shows/hides the navigation mesh and path.
/// It assumes the _agentPrefab has LightshipNavMeshAgent on it.
/// If you have written your own agent type, either swap yours in or inherit from it.
///
public class NavMeshHowTo : MonoBehaviour
{
    [SerializeField]
    private Camera _camera;

    [SerializeField]
    private LightshipNavMeshAgent _agentPrefab;

    private LightshipNavMeshAgent _agentInstance;

    void Update()
    {
        HandleTouch();
    }

    private void HandleTouch()
    {
        //in the editor we want to use mouse clicks, on phones we want touches.
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
#else
        //if there is no touch or touch selects UI element
        if (Input.touchCount <= 0)
            return;

        var touch = Input.GetTouch(0);

        if (touch.phase == UnityEngine.TouchPhase.Began)
#endif
        {
#if UNITY_EDITOR
            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
#else
            Ray ray = _camera.ScreenPointToRay(touch.position);
#endif
            //project the touch point from screen space into 3d and pass that to your agent as a destination
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (_agentInstance == null)
                {
                    _agentInstance = Instantiate(_agentPrefab);
                    _agentInstance.transform.position = hit.point;
                }
                else
                {
                    _agentInstance.SetDestination(hit.point);
                }
            }
        }
    }
}