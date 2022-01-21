using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Priority_Queue;

public class Dijkstra : MonoBehaviour
{
    //Declaration of linked unit.
    private Unit unitScript;
    //Initialisation of collections for pathfinding.
    public Dictionary<MapNode, float> dijkstraDict = new Dictionary<MapNode, float>();
    private Dictionary<MapNode, MapNode> bestAccessToNode = new Dictionary<MapNode, MapNode>();
    public List<MapNode> path = new List<MapNode>();
    private SimplePriorityQueue<MapNode> priorityQueue = new SimplePriorityQueue<MapNode>();

    private void Awake()
    {
        unitScript = gameObject.GetComponent<Unit>();
    }

    private void Start()
    {
        //Populates the collections for node access and edge cost of each node.
        foreach (MapNode mapNode in GameObject.Find("MapGraph").GetComponent<MapGraph>().graphCostDict.Keys)
        {
            bestAccessToNode.Add(mapNode, null);
            if (mapNode.terrainType == "Grassland")
            {
                unitScript.nodeCostDict.Add(mapNode, mapNode.cost * unitScript.GrassCost);
            }
            else if (mapNode.terrainType == "Arid")
            {
                unitScript.nodeCostDict.Add(mapNode, mapNode.cost * unitScript.AridCost);
            }
            else if (mapNode.terrainType == "Icefield")
            {
                unitScript.nodeCostDict.Add(mapNode, mapNode.cost * unitScript.IceCost);
            }
            else if (mapNode.terrainType == "Mountain")
            {
                unitScript.nodeCostDict.Add(mapNode, mapNode.cost * unitScript.MountainCost);
            }
            else if (mapNode.terrainType == "River")
            {
                unitScript.nodeCostDict.Add(mapNode, mapNode.cost * unitScript.RiverCost);
            }
            else if (mapNode.terrainType == "Ocean")
            {
                unitScript.nodeCostDict.Add(mapNode, mapNode.cost * unitScript.OceanCost);
            }
        }
    }

    //Generates a graph with the total cost of moving to each node.
    public bool DijkstraCalc()
    {
        Dictionary<MapNode, bool> visited = new Dictionary<MapNode, bool>();
        MapNode currentNode;
        //Sets up the priority queue and initial values.
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
        //Iterates through each node based on it's priority.
        while (true)
        {
            currentNode = priorityQueue.Dequeue();
            MapNode startNode = currentNode;
            visited[currentNode] = true;
            //Checks that the unit can move (is not surrounded on all sides).
            foreach (MapNode startNeighbour in startNode.adjacentNodeDict.Keys)
            {
                int i = 0;
                if (startNeighbour.isOccupied == true && startNeighbour.occupyingObject.CompareTag("Player Unit"))
                {
                    i++;
                }
                if (i == startNode.adjacentNodeDict.Keys.Count)
                {
                    Debug.Log("No space to move into!");
                    return false;
                }
            }
            //Checks each neighbouring node of the current node being investigated and updates their costs and priority.
            foreach (MapNode nextNode in currentNode.adjacentNodeDict.Keys)
            {
                if (visited[nextNode] == false)
                {
                    if (nextNode.occupyingObject.CompareTag("Player Unit"))
                    {
                        continue;
                    }
                    float newScore = unitScript.nodeCostDict[currentNode] + unitScript.nodeCostDict[nextNode];
                    if (newScore < nextNode.cost)
                    {
                        dijkstraDict[nextNode] = newScore;
                        bestAccessToNode[nextNode] = currentNode;
                        priorityQueue.UpdatePriority(nextNode, newScore);
                    }
                }
            }
            //Checks if the only remaining nodes cannot be reached.
            if (priorityQueue.GetPriority(priorityQueue.First) == Mathf.Infinity)
            {
                return true;
            }
            //Exits the while loop if there are no nodes left in the priority queue.
            if (priorityQueue.Count == 0)
            {
                return true;
            }
        }
    }
    //Creates a list of all the nodes the linked unit can move to based on it's stamina.
    public List<MapNode> PossibleMoves()
    {
        var possibleMoves = from x in dijkstraDict.Keys
                            where dijkstraDict[x] <= unitScript.CurrentStamina
                            select x;

        return possibleMoves.ToList();
    }
    //Builds a path to the selected node.
    public List<MapNode> BuildPath(MapNode startNode, MapNode targetNode)
    {
        List<MapNode> route = new List<MapNode>();
        MapNode currentNode = targetNode;
        while (targetNode != startNode)
        {
            route.Add(currentNode);
            currentNode = bestAccessToNode[currentNode];
        }
        return route;
    }

    
}
