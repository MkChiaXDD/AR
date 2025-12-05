using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    public GameObject objectToSpawn;   // this is your defense base prefab
    private PlacementMarker placement;

    private GameObject spawnedObject;
    private bool hasSpawned = false;

    void Start()
    {
        placement = FindFirstObjectByType<PlacementMarker>();
    }

    void Update()
    {
        if (hasSpawned) return;

        if (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began)
        {
            if (placement == null) return;

            spawnedObject = Instantiate(objectToSpawn, placement.transform.position, placement.transform.rotation);
            Base baseScript = spawnedObject.GetComponent<Base>();
            hasSpawned = true;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetDefenseBase(spawnedObject.transform, baseScript);
            }
        }
    }
}
