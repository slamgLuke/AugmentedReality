using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    public float strength = 200f;
    public float forwardDistanceFactor = 0.4f;
    public Rigidbody[] projectilePrefabs = new Rigidbody[5];
    public Transform spawnablesTransform;

    // Public static reference to the second object
    public static Transform targetToChase;


    void Update()
    {
        if (UIManager.IsUIActive)
        {
            return;
        }
        if (UIManager.CurrentMode != UIManager.InteractionMode.Spawn)
        {
            return;
        }
        if (projectilePrefabs == null || projectilePrefabs.Length == 0)
        {
            return;
        }


        bool validInputOccurred = false;
        Vector2 inputPosition = Vector2.zero;

#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            validInputOccurred = true;
            inputPosition = Input.mousePosition;
        }
#else
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            validInputOccurred = true;
            inputPosition = Input.GetTouch(0).position;
        }
#endif

        if (validInputOccurred)
        {
            float screenHeight = Screen.height;
            float topTenPercentBoundary = screenHeight * 0.90f;

            if (inputPosition.y > topTenPercentBoundary)
            {
                return;
            }

            int randomIndex = Random.Range(0, projectilePrefabs.Length);
            Rigidbody selectedPrefab = projectilePrefabs[randomIndex];

            if (selectedPrefab == null)
            {
                return;
            }

            var pos = Camera.main.transform.position;
            var forw = Camera.main.transform.forward;

            // Instantiate and set parent
            Rigidbody thing = Instantiate(selectedPrefab, pos + (forw * forwardDistanceFactor), Quaternion.identity, spawnablesTransform);
            MeshCollider meshCollider = thing.GetComponent<MeshCollider>();
            if (meshCollider != null)
            {
                meshCollider.convex = true;
            }
            
             if (thing != null)
            {
                thing.AddForce(forw * strength);
            }
        }
    }
}