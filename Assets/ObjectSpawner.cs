using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    public GameObject objectToSpawn;
    private PlacementMarker placement;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        placement = FindFirstObjectByType<PlacementMarker>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began)
        {
            GameObject obj = Instantiate(objectToSpawn, placement.transform.position, placement.transform.rotation);
        }
    }
}
