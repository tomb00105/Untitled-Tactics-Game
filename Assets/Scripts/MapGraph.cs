using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGraph : MonoBehaviour
{
    //Dictionary of every node and it's associated movement cost.
    public Dictionary<MapNode, Vector2> graphCostDict = new Dictionary<MapNode, Vector2>();
    public Dictionary<MapNode, Unit> tileOccupationDict = new Dictionary<MapNode, Unit>();
}
