using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ObjectSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject basePrefab;
    public GameObject turretPrefab;
    public GameObject farmPrefab;          // NEW — coin spawner building

    private PlacementMarker placement;
    public GameManager gameMgr;

    [Header("UI Text")]
    public GameObject placeTurretText;
    public GameObject placeFarmText;       // NEW — UI hint for placing farm

    [Header("Prices")]
    public int turretPrice = 3;
    public int farmPrice = 5;              // NEW — cost for farm

    private bool basePlaced = false;
    private bool placingTurret = false;
    private bool placingFarm = false;      // NEW

    void Start()
    {
        placement = FindFirstObjectByType<PlacementMarker>();
        placeTurretText.SetActive(false);
        placeFarmText.SetActive(false);
    }

    void Update()
    {
        if (Input.touchCount == 0) return;
        if (Input.touches[0].phase != TouchPhase.Began) return;
        if (placement == null) return;
        if (!placement.HasValidPosition) return;

        // BASE placement (only once)
        if (!basePlaced)
        {
            PlaceBase();
            return;
        }
    }

    //????????????????????????????????????????????
    // BASE (farm stays separate — this is just your defense base)
    //????????????????????????????????????????????
    public void OnPlaceBaseButton()
    {
        if (basePlaced) return;
        if (!placement.HasValidPosition) return;
        PlaceBase();
    }

    private void PlaceBase()
    {
        GameObject baseObj = Instantiate(basePrefab, placement.transform.position, placement.transform.rotation);

        ARPlaneManager pm = FindAnyObjectByType<ARPlaneManager>();
        if (pm != null) pm.enabled = false;

        Base baseScript = baseObj.GetComponent<Base>();
        basePlaced = true;

        gameMgr.SetDefenseBase(baseObj.transform, baseScript);
    }

    //????????????????????????????????????????????
    // TURRET SYSTEM
    //????????????????????????????????????????????
    public void EnableTurretPlacement()
    {
        if (!basePlaced) return;
        if (gameMgr.GetGoldAmount() < turretPrice) return;

        placingTurret = true;
        placingFarm = false; // turn off other modes
        placeTurretText.SetActive(true);
        placeFarmText.SetActive(false);
    }

    public void OnPlaceTurretButton()
    {
        if (!placingTurret) return;
        if (!placement.HasValidPosition) return;
        if (gameMgr.GetGoldAmount() < turretPrice) return;

        AudioManager.Instance.PlaySFX("Place");

        Instantiate(turretPrefab, placement.transform.position, placement.transform.rotation);
        gameMgr.UseGold(turretPrice);

        placingTurret = false;
        placeTurretText.SetActive(false);
    }

    //????????????????????????????????????????????
    // FARM / COIN SPAWNER SYSTEM — NEW
    //????????????????????????????????????????????
    public void EnableFarmPlacement()
    {
        if (!basePlaced) return;
        if (!placement.HasValidPosition) return;
        if (gameMgr.GetGoldAmount() < farmPrice) return;

        placingFarm = true;
        placingTurret = false;

        placeFarmText.SetActive(true);
        placeTurretText.SetActive(false);
    }

    public void OnPlaceFarmButton()
    {
        if (!placingFarm) return;
        if (gameMgr.GetGoldAmount() < farmPrice) return;

        AudioManager.Instance.PlaySFX("Place");

        Instantiate(farmPrefab, placement.transform.position, placement.transform.rotation);
        gameMgr.UseGold(farmPrice);

        placingFarm = false;
        placeFarmText.SetActive(false);
    }

    //????????????????????????????????????????????
    // Optional: Cancel
    //????????????????????????????????????????????
    public void CancelPlacement()
    {
        placingTurret = false;
        placingFarm = false;

        placeTurretText.SetActive(false);
        placeFarmText.SetActive(false);
    }
}
