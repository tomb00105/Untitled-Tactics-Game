using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Priority_Queue;

public class Dijkstra : MonoBehaviour
{
    //Declaration of linked unit.
    private Unit unitScript;
    private GameManager gameManager;
    private MapGraph mapGraph;
    //Initialisation of collections for pathfinding.
    public Dictionary<MapNode, float> dijkstraDict = new Dictionary<MapNode, float>();
    private Dictionary<MapNode, MapNode> bestAccessToNode = new Dictionary<MapNode, MapNode>();
    public List<MapNode> path = new List<MapNode>();
    private SimplePriorityQueue<MapNode> priorityQueue = new SimplePriorityQueue<MapNode>();

    private void Awake()
    {
        unitScript = gameObject.GetComponent<Unit>();
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        mapGraph = GameObject.Find("MapGraph").GetComponent<MapGraph>();
    }

    private void Start()
    {
        //Debug.Log("Cost: " + unitScript.AridCost.ToString());
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
    public bool DijkstraCalc()
    {
        Dictionary<MapNode, bool> visited = new Dictionary<MapNode, bool>();
        MapNode currentNode;

        //Debug.Log("Cost Dict Count: " + costDict.Keys.Count);

        foreach (GameObject node in GameObject.FindGameObjectsWithTag("Terrain"))
        {
            Debug.Log("dijkstraDict Count: " + dijkstraDict.Count);
            if (!dijkstraDict.ContainsKey(node.GetComponent<MapNode>()))
            {
                Debug.Log("Node was not already in dijkstraDict");
                if (node.transform.position == transform.position)
                {
                    dijkstraDict.Add(node.GetComponent<MapNode>(), 0);
                    priorityQueue.Enqueue(node.GetComponent<MapNode>(), 0);
                    visited.Add(node.GetComponent<MapNode>(), true);
                    currentNode = node.GetComponent<MapNode>();
                }
                else
                {
                    dijkstraDict.Add(node.GetComponent<MapNode>(), Mathf.Infinity);
                    priorityQueue.Enqueue(node.GetComponent<MapNode>(), Mathf.Infinity);
                    visited.Add(node.GetComponent<MapNode>(), false);
                }
            }
            else
            {
                if (node.transform.position == transform.position)
                {
                    dijkstraDict[node.GetComponent<MapNode>()] = 0;
                    priorityQueue.Enqueue(node.GetComponent<MapNode>(), 0);
                    visited.Add(node.GetComponent<MapNode>(), true);
                    currentNode = node.GetComponent<MapNode>();
                }
                else
                {
                    dijkstraDict[node.GetComponent<MapNode>()] = Mathf.Infinity;
                    priorityQueue.Enqueue(node.GetComponent<MapNode>(), Mathf.Infinity);
                    visited.Add(node.GetComponent<MapNode>(), false);
                }
            }
            
        }
        //Sets up the priority queue and initial values.
       /* foreach (MapNode mapNode in dijkstraDict.Keys)
        {
            if (!priorityQueue.Contains(mapNode))
            {
                if (mapNode == gameObject.GetComponent<Unit>().currentMapNode)
                {
                    Debug.Log("Current Node added to priority queue");
                    dijkstraDict[mapNode] = 0;
                    priorityQueue.Enqueue(mapNode, 0);
                    visited.Add(mapNode, true);
                    currentNode = mapNode;
                }
                else
                {
                    dijkstraDict[mapNode] = Mathf.Infinity;
                    priorityQueue.Enqueue(mapNode, Mathf.Infinity);
                    Debug.Log("Priority Queue Count: " + priorityQueue.Count);
                    visited.Add(mapNode, false);
                }
            }
        }*/
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
                    if (mapGraph.tileOccupationDict[startNeighbour] != null)
                    {
                        if (mapGraph.tileOccupationDict[startNeighbour].CompareTag("Player Unit"))
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
                else if (gameObject.CompareTag("Player Unit"))
                {
                    if (mapGraph.tileOccupationDict[startNeighbour] != null)
                    {
                        if (mapGraph.tileOccupationDict[startNeighbour].CompareTag("Enemy Unit"))
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

            }

            //Checks each neighbouring node of the current node being investigated and updates their costs and priority.
            foreach (MapNode nextNode in currentNode.adjacentNodeDict.Keys)
            {
                Debug.Log("Current Unit Dijkstra: " + gameObject.name.ToString());
                Debug.Log("Priority queue count: " + priorityQueue.Count.ToString());
                float newScore;
                if (visited[nextNode] == false)
                {
                    if (gameObject.CompareTag("Enemy Unit"))
                    {
                        if (mapGraph.tileOccupationDict[nextNode] != null)
                        {
                            if (mapGraph.tileOccupationDict[nextNode].CompareTag("Player Unit"))
                            {
                                if (dijkstraDict.ContainsKey(nextNode))
                                {
                                    Debug.Log(nextNode.name.ToString() + "REMOVED from dijkstraDict");
                                    dijkstraDict.Remove(nextNode);
                                }
                                continue;
                            }
                            else if (mapGraph.tileOccupationDict[nextNode].CompareTag("Enemy Unit"))
                            {
                                if (dijkstraDict.ContainsKey(currentNode))
                                {
                                    Debug.Log("CurrentNode " + currentNode.name.ToString() + " is in dijkstraDict");
                                }
                                else if (dijkstraDict.ContainsKey(currentNode))
                                {
                                    Debug.Log("CurrentNode " + currentNode.name.ToString() + " is NOT in dijkstraDict");
                                }
                                Debug.Log("NextNode terrainType: " + nextNode.terrainType.ToString());
                                if (nextNode.terrainType == "Grassland")
                                {
                                    Debug.Log("dijkstraDict[currentNode]: " + dijkstraDict[currentNode].ToString());

                                    newScore = dijkstraDict[currentNode] + unitScript.GrassCost;
                                }
                                else if (nextNode.terrainType == "Arid")
                                {
                                    Debug.Log("dijkstraDict[currentNode]: " + dijkstraDict[currentNode].ToString() + " unitScript.AridCost: " + unitScript.AridCost.ToString());
                                    newScore = dijkstraDict[currentNode] + unitScript.AridCost;
                                }
                                else if (nextNode.terrainType == "Icefield")
                                {
                                    Debug.Log("dijkstraDict[currentNode]: " + dijkstraDict[currentNode].ToString());

                                    newScore = dijkstraDict[currentNode] + unitScript.IceCost;
                                }
                                else if (nextNode.terrainType == "Mountain")
                                {
                                    Debug.Log("dijkstraDict[currentNode]: " + dijkstraDict[currentNode].ToString());

                                    newScore = dijkstraDict[currentNode] + unitScript.MountainCost;
                                }
                                else if (nextNode.terrainType == "River")
                                {
                                    Debug.Log("dijkstraDict[currentNode]: " + dijkstraDict[currentNode].ToString());

                                    newScore = dijkstraDict[currentNode] + unitScript.RiverCost;
                                }
                                else
                                {
                                    Debug.Log("dijkstraDict[currentNode]: " + dijkstraDict[currentNode].ToString());

                                    newScore = dijkstraDict[currentNode] + unitScript.OceanCost;
                                }
                                Debug.Log("Current Next Node Cost: " + dijkstraDict[nextNode].ToString());
                                Debug.Log("newScore: " + newScore.ToString());
                                if (newScore <= dijkstraDict[nextNode])
                                {
                                    Debug.Log("Score Changed!");
                                    dijkstraDict[nextNode] = newScore;
                                    bestAccessToNode[nextNode] = currentNode;
                                    priorityQueue.UpdatePriority(nextNode, newScore);
                                }
                            }
                        }
                        else if (mapGraph.tileOccupationDict[nextNode] == null)
                        {
                            if (dijkstraDict.ContainsKey(currentNode))
                            {
                                Debug.Log("CurrentNode" + currentNode.name.ToString() + " is in dijkstraDict");
                            }
                            else
                            {
                                Debug.Log("CurrentNode" + currentNode.name.ToString() + " is NOT in dijkstraDict");
                                continue;
                            }
                            if (nextNode.terrainType == "Grassland")
                            {
                                Debug.Log("dijkstraDict[currentNode]: " + dijkstraDict[currentNode].ToString());

                                newScore = dijkstraDict[currentNode] + unitScript.GrassCost;
                            }
                            else if (nextNode.terrainType == "Arid")
                            {
                                Debug.Log("dijkstraDict[currentNode]: " + dijkstraDict[currentNode].ToString() + " unitScript.AridCost: " + unitScript.AridCost.ToString());

                                newScore = dijkstraDict[currentNode] + unitScript.AridCost;
                            }
                            else if (nextNode.terrainType == "Icefield")
                            {
                                Debug.Log("dijkstraDict[currentNode]: " + dijkstraDict[currentNode].ToString());

                                newScore = dijkstraDict[currentNode] + unitScript.IceCost;
                            }
                            else if (nextNode.terrainType == "Mountain")
                            {
                                Debug.Log("dijkstraDict[currentNode]: " + dijkstraDict[currentNode].ToString());

                                newScore = dijkstraDict[currentNode] + unitScript.MountainCost;
                            }
                            else if (nextNode.terrainType == "River")
                            {
                                Debug.Log("dijkstraDict[currentNode]: " + dijkstraDict[currentNode].ToString());

                                newScore = dijkstraDict[currentNode] + unitScript.RiverCost;
                            }
                            else
                            {
                                Debug.Log("dijkstraDict[currentNode]: " + dijkstraDict[currentNode].ToString());

                                newScore = dijkstraDict[currentNode] + unitScript.OceanCost;
                            }
                            Debug.Log("Current Next Node Cost: " + dijkstraDict[nextNode].ToString());
                            Debug.Log("newScore: " + newScore.ToString());
                            if (newScore <= dijkstraDict[nextNode])
                            {
                                Debug.Log("Score Changed!");
                                dijkstraDict[nextNode] = newScore;
                                bestAccessToNode[nextNode] = currentNode;
                                priorityQueue.UpdatePriority(nextNode, newScore);
                            }
                        }
                    }
                    else if (gameObject.CompareTag("Player Unit"))
                    {
                        if (mapGraph.tileOccupationDict[nextNode] != null)
                        {
                            if (mapGraph.tileOccupationDict[nextNode].CompareTag("Enemy Unit"))
                            {
                                if (dijkstraDict.ContainsKey(nextNode))
                                {
                                    dijkstraDict.Remove(nextNode);
                                    continue;
                                }
                            }
                            else if (mapGraph.tileOccupationDict[nextNode].CompareTag("Player Unit"))
                            {
                                if (dijkstraDict.ContainsKey(currentNode))
                                {
                                    Debug.Log("CurrentNode " + currentNode.name.ToString() + " is in dijkstraDict");
                                }
                                else if (dijkstraDict.ContainsKey(currentNode))
                                {
                                    Debug.Log("CurrentNode " + currentNode.name.ToString() + " is NOT in dijkstraDict");
                                }
                                Debug.Log("NextNode terrainType: " + nextNode.terrainType.ToString());
                                if (nextNode.terrainType == "Grassland")
                                {
                                    Debug.Log("dijkstraDict[currentNode]: " + dijkstraDict[currentNode].ToString());

                                    newScore = dijkstraDict[currentNode] + unitScript.GrassCost;
                                }
                                else if (nextNode.terrainType == "Arid")
                                {
                                    Debug.Log("dijkstraDict[currentNode]: " + dijkstraDict[currentNode].ToString() + " unitScript.AridCost: " + unitScript.AridCost.ToString());
                                    newScore = dijkstraDict[currentNode] + unitScript.AridCost;
                                }
                                else if (nextNode.terrainType == "Icefield")
                                {
                                    Debug.Log("dijkstraDict[currentNode]: " + dijkstraDict[currentNode].ToString());

                                    newScore = dijkstraDict[currentNode] + unitScript.IceCost;
                                }
                                else if (nextNode.terrainType == "Mountain")
                                {
                                    Debug.Log("dijkstraDict[currentNode]: " + dijkstraDict[currentNode].ToString());

                                    newScore = dijkstraDict[currentNode] + unitScript.MountainCost;
                                }
                                else if (nextNode.terrainType == "River")
                                {
                                    Debug.Log("dijkstraDict[currentNode]: " + dijkstraDict[currentNode].ToString());

                                    newScore = dijkstraDict[currentNode] + unitScript.RiverCost;
                                }
                                else
                                {
                                    Debug.Log("dijkstraDict[currentNode]: " + dijkstraDict[currentNode].ToString());

                                    newScore = dijkstraDict[currentNode] + unitScript.OceanCost;
                                }
                                Debug.Log("Current Next Node Cost: " + dijkstraDict[nextNode].ToString());
                                Debug.Log("newScore: " + newScore.ToString());
                                if (newScore <= dijkstraDict[nextNode])
                                {
                                    Debug.Log("Score Changed!");
                                    dijkstraDict[nextNode] = newScore;
                                    bestAccessToNode[nextNode] = currentNode;
                                    priorityQueue.UpdatePriority(nextNode, newScore);
                                }
                            }
                        }
                        else if (mapGraph.tileOccupationDict[nextNode] == null)
                        {
                            if (dijkstraDict.ContainsKey(currentNode))
                            {
                                Debug.Log("CurrentNode" + currentNode.name.ToString() + " is in dijkstraDict");
                            }
                            else
                            {
                                Debug.Log("CurrentNode" + currentNode.name.ToString() + " is NOT in dijkstraDict");
                                continue;
                            }
                            if (nextNode.terrainType == "Grassland")
                            {
                                Debug.Log("dijkstraDict[currentNode]: " + dijkstraDict[currentNode].ToString());

                                newScore = dijkstraDict[currentNode] + unitScript.GrassCost;
                            }
                            else if (nextNode.terrainType == "Arid")
                            {
                                Debug.Log("dijkstraDict[currentNode]: " + dijkstraDict[currentNode].ToString() + " unitScript.AridCost: " + unitScript.AridCost.ToString());

                                newScore = dijkstraDict[currentNode] + unitScript.AridCost;
                            }
                            else if (nextNode.terrainType == "Icefield")
                            {
                                Debug.Log("dijkstraDict[currentNode]: " + dijkstraDict[currentNode].ToString());

                                newScore = dijkstraDict[currentNode] + unitScript.IceCost;
                            }
                            else if (nextNode.terrainType == "Mountain")
                            {
                                Debug.Log("dijkstraDict[currentNode]: " + dijkstraDict[currentNode].ToString());

                                newScore = dijkstraDict[currentNode] + unitScript.MountainCost;
                            }
                            else if (nextNode.terrainType == "River")
                            {
                                Debug.Log("dijkstraDict[currentNode]: " + dijkstraDict[currentNode].ToString());

                                newScore = dijkstraDict[currentNode] + unitScript.RiverCost;
                            }
                            else
                            {
                                Debug.Log("dijkstraDict[currentNode]: " + dijkstraDict[currentNode].ToString());

                                newScore = dijkstraDict[currentNode] + unitScript.OceanCost;
                            }
                            Debug.Log("Current Next Node Cost: " + dijkstraDict[nextNode].ToString());
                            Debug.Log("newScore: " + newScore.ToString());
                            if (newScore <= dijkstraDict[nextNode])
                            {
                                Debug.Log("Score Changed!");
                                dijkstraDict[nextNode] = newScore;
                                bestAccessToNode[nextNode] = currentNode;
                                priorityQueue.UpdatePriority(nextNode, newScore);
                            }
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
        List<MapNode> possibleMoves = new List<MapNode>();
        //Ensures that the unit can't end it's turn on the same space as another unit.
        Debug.Log("Current stamina: " + unitScript.CurrentStamina.ToString());
        Debug.Log("dijkstraDict PossibleMoves Count: " + dijkstraDict.Count.ToString());
        //FIX BY COMPARING MAPNODE POSITION TO POSITION OF ALL UNITS IN A DICTIONARY
        foreach (KeyValuePair<MapNode, float> node in dijkstraDict)
        {
            if (node.Value != Mathf.Infinity)
            {
                Debug.Log("Node cost lower than infinity");
            }
            Debug.Log("dijkstraDict PossibleMoves Node: " + node.Key.name.ToString() + " Cost: " + node.Value.ToString());
            if (mapGraph.tileOccupationDict[node.Key] == null)
            {
                Debug.Log("Node not occupied");
            }
            else
            {
                Debug.Log("dijkstraDict PossibleMoves occupation: " + mapGraph.tileOccupationDict[node.Key].ToString());

            }
            if (node.Value <= unitScript.CurrentStamina && mapGraph.tileOccupationDict[node.Key] == null)
            {
                Debug.Log("Node added to possible moves.");
                possibleMoves.Add(node.Key);
            }
        }
        //possibleMoves = dijkstraDict.Where(x => x.Value <= unitScript.CurrentStamina && mapGraph.tileOccupationDict[x.Key] == null).Select(x => x.Key).ToList();

        return possibleMoves;
    }

    //Builds a path to the selected node.
    public List<MapNode> BuildPath(MapNode startNode, MapNode targetNode)
    {
        //Debug.Log("startNode: " + startNode.name.ToString() + " targetNode: " + targetNode.name.ToString());
        List<MapNode> route = new List<MapNode>();
        MapNode currentNode = targetNode;
        while (currentNode != startNode)
        {
            route.Add(currentNode);
            Debug.Log("Route Count: " + route.Count + " " + currentNode.GetType().ToString());
            //Debug.Log("Best access to node: " + bestAccessToNode[currentNode].name.ToString());
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