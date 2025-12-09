using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ObjectSpawner : MonoBehaviour
{
    public GameObject basePrefab;      // your defense base
    public GameObject turretPrefab;    // turret you want to place
    private PlacementMarker placement;
    public GameManager gameMgr;
    public GameObject placeTurretText;

    private bool basePlaced = false;
    private bool placingTurret = false;

    public int turretPrice = 3;

    void Start()
    {
        placement = FindFirstObjectByType<PlacementMarker>();
        placeTurretText.SetActive(false);
    }

    void Update()
    {
        if (Input.touchCount == 0) return;
        if (Input.touches[0].phase != TouchPhase.Began) return;

        if (placement == null) return;

        if (!placement.HasValidPosition) return;

        // If base is not placed yet: FIRST TAP places base
        if (!basePlaced)
        {
            PlaceBase();
            return;
        }

        // If we are in turret placement mode
        if (placingTurret && gameMgr.GetGoldAmount() >= turretPrice)
        {
            PlaceTurret();
            gameMgr.UseGold(turretPrice);
            placingTurret = false;  // exit turret placing mode
        }
    }

    // Call this from your UI Button!
    public void EnableTurretPlacement()
    {
        if (gameMgr.GetGoldAmount() < turretPrice) return;
        placingTurret = true;
        placeTurretText.SetActive(true);
    }

    public void DisableTurretPlacement()
    {
        placingTurret = false;
        placeTurretText.SetActive(false);
    }

    private void PlaceBase()
    {
        if (placement == null) return;

        GameObject baseObj = Instantiate(basePrefab, placement.transform.position, placement.transform.rotation);

        FindAnyObjectByType<ARPlaneManager>().enabled = false; // if you disabled planes already, fine
        Base baseScript = baseObj.GetComponent<Base>();
        basePlaced = true;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetDefenseBase(baseObj.transform, baseScript);
        }
    }

    private void PlaceTurret()
    {
        if (placement == null) return;

        Instantiate(turretPrefab, placement.transform.position, placement.transform.rotation);
        placeTurretText.SetActive(false);
    }
}
