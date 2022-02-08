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
        Debug.Log("Cost: " + unitScript.AridCost.ToString());
        //Populates the collections for node access and edge cost of each node.
        /*foreach (MapNode mapNode in GameObject.Find("MapGraph").GetComponent<MapGraph>().graphCostDict.Keys)
        {
            bestAccessToNode.Add(mapNode, null);
            if (mapNode.terrainType == "Grassland")
            {
                unitScript.NodeCostDict.Add(mapNode, unitScript.GrassCost);
            }
            else if (mapNode.terrainType == "Arid")
            {
                unitScript.nodeCostDict.Add(mapNode, unitScript.AridCost);
            }
            else if (mapNode.terrainType == "Icefield")
            {
                unitScript.nodeCostDict.Add(mapNode, unitScript.IceCost);
            }
            else if (mapNode.terrainType == "Mountain")
            {
                unitScript.nodeCostDict.Add(mapNode, unitScript.MountainCost);
            }
            else if (mapNode.terrainType == "River")
            {
                unitScript.nodeCostDict.Add(mapNode, unitScript.RiverCost);
            }
            else if (mapNode.terrainType == "Ocean")
            {
                unitScript.nodeCostDict.Add(mapNode, unitScript.OceanCost);
            }
        }*/
    }

    //Generates a graph with the total cost of moving to each node.
    public bool DijkstraCalc(Dictionary<MapNode, float> costDict)
    {
        Dictionary<MapNode, bool> visited = new Dictionary<MapNode, bool>();
        MapNode currentNode;
        //Sets up the priority queue and initial values.
        foreach (KeyValuePair<MapNode, float> mapNode in costDict)
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
                if (gameObject.CompareTag("Enemy Unit"))
                {
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
                else if (gameObject.CompareTag("Player Unit"))
                {

                    if (startNeighbour.isOccupied == true && startNeighbour.occupyingObject.CompareTag("Enemy Unit"))
                    {
                        i++;
                    }
                    if (i == startNode.adjacentNodeDict.Keys.Count)
                    {
                        Debug.Log("No space to move into!");
                        return false;
                    }
                }
                
            }

            //Checks each neighbouring node of the current node being investigated and updates their costs and priority.
            foreach (MapNode nextNode in currentNode.adjacentNodeDict.Keys)
            {
                if (visited[nextNode] == false)
                {
                    if (gameObject.CompareTag("Enemy Unit"))
                    {
                        if (nextNode.isOccupied)
                        {
                            if (nextNode.occupyingObject.CompareTag("Player Unit"))
                            {
                                dijkstraDict.Remove(nextNode);
                                continue;
                            }
                        }
                        float newScore = dijkstraDict[currentNode] + unitScript.NodeCostDict[nextNode];
                        Debug.Log("Current Next Node Cost: " + dijkstraDict[nextNode].ToString());
                        Debug.Log("newScore: " + newScore.ToString());
                        if (newScore < dijkstraDict[nextNode])
                        {
                            Debug.Log("Score Changed!");
                            dijkstraDict[nextNode] = newScore;
                            bestAccessToNode[nextNode] = currentNode;
                            priorityQueue.UpdatePriority(nextNode, newScore);
                        }
                    }
                    else if (gameObject.CompareTag("Player Unit"))
                    {
                        if (nextNode.occupyingObject.CompareTag("Enemy Unit"))
                        {
                            dijkstraDict.Remove(nextNode);
                            continue;
                        }
                        float newScore = dijkstraDict[currentNode] + unitScript.NodeCostDict[nextNode];
                        if (newScore < dijkstraDict[nextNode])
                        {
                            dijkstraDict[nextNode] = newScore;
                            bestAccessToNode[nextNode] = currentNode;
                            priorityQueue.UpdatePriority(nextNode, newScore);
                        }
                    }
                }
            }
            //Exits the while loop if there are no nodes left in the priority queue.
            if (priorityQueue.Count == 0)
            {
                return true;
            }
            //Checks if the only remaining nodes cannot be reached.
           /* if (priorityQueue.GetPriority(priorityQueue.First) == Mathf.Infinity)
            {
                return true;
            }*/
        }
    }

    //Creates a list of all the nodes the linked unit can move to based on it's stamina.
    public List<MapNode> PossibleMoves()
    {
        List<MapNode> possibleMoves;
        //Ensures that the unit can't end it's turn on the same space as another unit.
        if (gameObject.CompareTag("Enemy Unit"))
        {
            Debug.Log("Current stamina: " + unitScript.CurrentStamina.ToString());
            possibleMoves = dijkstraDict.Where(x => x.Value <= unitScript.CurrentStamina && !x.Key.GetComponent<MapNode>().isOccupied).Select(x => x.Key).ToList();
        }
        else
        {
            possibleMoves = dijkstraDict.Where(x => x.Value <= unitScript.CurrentStamina && !x.Key.GetComponent<MapNode>().isOccupied).Select(x => x.Key).ToList();
        }

        return possibleMoves;
    }

    //Builds a path to the selected node.
    public List<MapNode> BuildPath(MapNode startNode, MapNode targetNode)
    {
        Debug.Log("startNode: " + startNode.name.ToString() + " targetNode: " + targetNode.name.ToString());
        List<MapNode> route = new List<MapNode>();
        MapNode currentNode = targetNode;
        while (currentNode != startNode)
        {
            route.Add(currentNode);
            Debug.Log("Route Count: " + route.Count + " " + currentNode.GetType().ToString());
            Debug.Log("Best access to node: " + bestAccessToNode[currentNode].name.ToString());
            if (bestAccessToNode.ContainsKey(currentNode))
            {
                currentNode = bestAccessToNode[currentNode];
            }
            else
            {
                route.Reverse();
                foreach (MapNode routeNode in route)
                {
                    Debug.Log("Route Node " + (route.IndexOf(routeNode)).ToString() + ": " + routeNode.name.ToString());
                }
                return route;
            }
        }
        route.Reverse();
        foreach (MapNode routeNode in route)
        {
            Debug.Log("Route Node " + route.IndexOf(routeNode) + ": " + routeNode.name.ToString());
        }
        return route;
    }
}