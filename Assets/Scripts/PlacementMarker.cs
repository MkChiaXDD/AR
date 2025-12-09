using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class PlacementMarker : MonoBehaviour
{
    private ARRaycastManager rayManager;
    private GameObject visual;

    public bool HasValidPosition { get; private set; }

    private static readonly List<ARRaycastHit> hits = new List<ARRaycastHit>();

    private void Awake()
    {
        rayManager = FindAnyObjectByType<ARRaycastManager>();
        if (rayManager == null)
        {
            Debug.LogWarning("PlacementMarker: No ARRaycastManager found in scene.");
        }

        visual = transform.GetChild(0).gameObject;
    }

    private void OnEnable()
    {
        HasValidPosition = false;
        if (visual != null)
            visual.SetActive(false);
    }

    private void Start()
    {
        HasValidPosition = false;
        if (visual != null)
            visual.SetActive(false);
    }

    private void Update()
    {
        if (rayManager == null)
            return;

        hits.Clear();
        Vector2 screenCenter = new Vector2(Screen.width / 2.0f, Screen.height / 2.0f);

        if (rayManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinPolygon))
        {
            Pose pose = hits[0].pose;
            transform.SetPositionAndRotation(pose.position, pose.rotation);

            if (!visual.activeSelf)
                visual.SetActive(true);

            HasValidPosition = true;
        }
        else
        {
            if (visual.activeSelf)
                visual.SetActive(false);

            HasValidPosition = false;
        }
    }

    public void DisableMarker()
    {
        if (visual != null)
            visual.SetActive(false);

        enabled = false;
        HasValidPosition = false;
    }

    public void ResetMarker()
    {
        enabled = true;
        HasValidPosition = false;
        if (visual != null)
            visual.SetActive(false);
    }
}
