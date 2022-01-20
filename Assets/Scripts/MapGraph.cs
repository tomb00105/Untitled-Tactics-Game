using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGraph : MonoBehaviour
{
    //Dictionary of every node and it's associated movement cost.
    public Dictionary<MapNode, float> graphCostDict = new Dictionary<MapNode, float>();

    private void Awake()
    {
        //Gets every terrain tile and adds the MapNode and it's movement cost to the dictionary.
        foreach (GameObject terrainTile in GameObject.FindGameObjectsWithTag("Terrain"))
        {
            graphCostDict.Add(terrainTile.GetComponent<MapNode>(), terrainTile.GetComponent<MapNode>().cost);
        }
    }
}
