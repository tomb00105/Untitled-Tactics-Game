using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldMapManager : MonoBehaviour
{
    public GameObject locations;
    public Dictionary<WorldMapLocation, bool> worldMapLocationStates = new Dictionary<WorldMapLocation, bool>();
    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);

        locations = GameObject.Find("Locations");

        CheckLocationStates();
    }

    private void CheckLocationStates()
    {
        foreach (WorldMapLocation location in locations.transform.GetComponentsInChildren<WorldMapLocation>(true))
        {
            if (!worldMapLocationStates.ContainsKey(location))
            {
                worldMapLocationStates.Add(location, false);
                Debug.Log("Location added to worldMapLocationStates: " + location + " with state false");
            }
        }
    }
}
