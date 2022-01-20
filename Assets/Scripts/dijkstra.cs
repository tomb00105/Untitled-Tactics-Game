using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Priority_Queue;

public class dijkstra : MonoBehaviour
{
    private Unit unitScript;
    private Dictionary<MapNode, float> dijkstraDict = new Dictionary<MapNode, float>();
    private Dictionary<MapNode, MapNode> bestAccessToNode = new Dictionary<MapNode, MapNode>();
    public List<MapNode> path = new List<MapNode>();
    private SimplePriorityQueue<MapNode> priorityQueue = new SimplePriorityQueue<MapNode>();

    private void Awake()
    {
        unitScript = gameObject.GetComponent<Unit>();
    }
    private void Start()
    {
        foreach (MapNode mapNode in GameObject.Find("MapGraph").GetComponent<MapGraph>().graphCostDict.Keys)
        {
            if (mapNode.terrainType == "Grassland")
            {
                unitScript.nodeCostDict.Add(mapNode, mapNode.cost * unitScript.grassCost);
            }
            else if (mapNode.terrainType == "Arid")
            {
                unitScript.nodeCostDict.Add(mapNode, mapNode.cost * unitScript.aridCost);
            }
            else if (mapNode.terrainType == "Icefield")
            {
                unitScript.nodeCostDict.Add(mapNode, mapNode.cost * unitScript.iceCost);
            }
            else if (mapNode.terrainType == "Mountain")
            {
                unitScript.nodeCostDict.Add(mapNode, mapNode.cost * unitScript.mountainCost);
            }
            else if (mapNode.terrainType == "River")
            {
                unitScript.nodeCostDict.Add(mapNode, mapNode.cost * unitScript.riverCost);
            }
            else if (mapNode.terrainType == "Ocean")
            {
                unitScript.nodeCostDict.Add(mapNode, mapNode.cost * unitScript.riverCost);
            }
        }
    }

    public void Dijkstra()
    {
        Dictionary<MapNode, bool> visited = new Dictionary<MapNode, bool>();
        MapNode currentNode;
        foreach (KeyValuePair<MapNode, float> mapNode in unitScript.nodeCostDict)
        {
            if (mapNode.Key == gameObject.GetComponent<Unit>().currentMapNode)
            {
                dijkstraDict[mapNode.Key] = 0;
                priorityQueue.Enqueue(mapNode.Key, 0);
                visited.Add(mapNode.Key, true);
                currentNode = mapNode.Key;
            }
            else
            {
                dijkstraDict[mapNode.Key] = Mathf.Infinity;
                priorityQueue.Enqueue(mapNode.Key, Mathf.Infinity);
                visited.Add(mapNode.Key, false);
            }
        }

        while (true)
        {
            currentNode = priorityQueue.Dequeue();
            visited[currentNode] = true;
            foreach (MapNode nextNode in currentNode.adjacentNodeDict.Keys)
            {
                if (visited[nextNode] == false)
                {
                    if (nextNode.occupyingObject.CompareTag("Player Unit"))
                    {

                    }
                    float newScore = 
                }
            }
        }
    }
}
