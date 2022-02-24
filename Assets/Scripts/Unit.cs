using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Unit : MonoBehaviour
{
    [SerializeField] protected UIController uIController;
    [SerializeField] protected GameManager gameManager;
    [SerializeField] protected MapGraph mapGraph;
    protected Dijkstra dijkstraScript;
    public string unitSide;

    //Unit info declarations.
    public string UnitName
    { get;  set; }
    public string UnitType
    { get; set; }
    public string UnitDescription
    { get; set; }
    public float MaxHP
    { get;  set; }
    public float CurrentHP
    { get; set; }
    public float MaxStamina
    { get; set; }
    public float CurrentStamina
    { get; set; }
    public float WeaponDamage
    { get; set; }
    public float WeaponRange
    { get; set; }
    public bool AttackOrDefence
    { get; set; }
    public List<string> Advantages
    { get; set; }
    public List<string> Disadvantages
    { get; set; }

    //Initialisation of costs for this unit to move across each type of terrain.
    public float GrassCost
    { get; set; }
    public float AridCost
    { get; set; }
    public float IceCost
    { get; set; }
    public float MountainCost
    { get; set; }
    public float RiverCost
    { get; set; }
    public float OceanCost
    { get; set; }

    public Dictionary<MapNode, float> NodeCostDict
    { get; set; }
    //Dictionary to keep track of the cost for this unit to move to each MapNode.
    public MapNode currentMapNode = null;
    public List<MapNode> path = new List<MapNode>();

    private void Awake()
    {
        uIController = GameObject.Find("UIController").GetComponent<UIController>();
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        mapGraph = GameObject.Find("MapGraph").GetComponent<MapGraph>();
    }


    //Checks which MapNode the unit is currently on.
    public virtual void CheckCurrentNode()
    {
        if (!mapGraph.tileOccupationDict.Values.Contains(this))
        {
            foreach (GameObject mapNode in GameObject.FindGameObjectsWithTag("Terrain"))
            {
                if (transform.position == mapNode.transform.position)
                {
                    mapGraph.tileOccupationDict[mapNode.GetComponent<MapNode>()] = this;
                    currentMapNode = mapNode.GetComponent<MapNode>();
                }
            }
        }
        else
        {
            foreach (GameObject mapNode in GameObject.FindGameObjectsWithTag("Terrain"))
            {
                if (mapGraph.tileOccupationDict[mapNode.GetComponent<MapNode>()] == this && transform.position != mapNode.transform.position)
                {
                    mapGraph.tileOccupationDict[mapNode.GetComponent<MapNode>()] = null;
                }
                if (transform.position == mapNode.transform.position)
                {
                    mapGraph.tileOccupationDict[mapNode.GetComponent<MapNode>()] = this;
                    currentMapNode = mapNode.GetComponent<MapNode>();
                }
            }
        }
    }

    public void NodeCostSetup()
    {
        foreach (GameObject node in GameObject.FindGameObjectsWithTag("Terrain"))
        {
            if (node.GetComponent<MapNode>().terrainType == "Grassland")
            {
                NodeCostDict.Add(node.GetComponent<MapNode>(), GrassCost);
                //Debug.Log("Grass Cost: " + NodeCostDict[node].ToString());
            }
            else if (node.GetComponent<MapNode>().terrainType == "Arid")
            {
                NodeCostDict.Add(node.GetComponent<MapNode>(), AridCost);
            }
            else if (node.GetComponent<MapNode>().terrainType == "Icefield")
            {
                NodeCostDict.Add(node.GetComponent<MapNode>(), IceCost);
            }
            else if (node.GetComponent<MapNode>().terrainType == "Mountain")
            {
                NodeCostDict.Add(node.GetComponent<MapNode>(), MountainCost);
            }
            else if (node.GetComponent<MapNode>().terrainType == "River")
            {
                NodeCostDict.Add(node.GetComponent<MapNode>(), RiverCost);
            }
            else if (node.GetComponent<MapNode>().terrainType == "Ocean")
            {
                NodeCostDict.Add(node.GetComponent<MapNode>(), OceanCost);
            }
            else
            {
                Debug.LogWarning("NO TERRAIN TYPE FOR THIS NODE: " + node.name.ToString());
            }
        }
    }
    //Decides whether the unit can/will move and where to.
    public virtual bool CheckCanMove()
    {
        if (!dijkstraScript.DijkstraCalc())
        {
            Debug.Log("Cannot move!");
            return false;
        }
        List<MapNode> moves = dijkstraScript.PossibleMoves();
        Debug.Log("Moves list count: " + moves.Count);
        if (CompareTag("Enemy Unit"))
        {
            if (moves.Count != 0)
            {
                path = dijkstraScript.BuildPath(currentMapNode, MovePriority(moves));
                return true;
            }
            else
            {
                Debug.Log("Cannot move as stamina is too low!");
                return false;
            }
        }
        return true;
    }

    //Moves the unit and updates MapNode occupation status.
    public void Move()
    {
        /*while (transform.position.x != path[path.Count - 1].transform.position.x && transform.position.y != path[path.Count - 1].transform.position.x)
        {
            int i = 0;
            if (transform.position.x != path[i].transform.position.x)
            {
                Vector2.MoveTowards(transform.position, path[i].transform.position, 5 * Time.deltaTime);
            }
            else if (transform.position.y != path[i].transform.position.y)
            {
                Vector2.MoveTowards(transform.position, path[i].transform.position, 5 * Time.deltaTime);
            }
            else
            {
                i++;
            }
        }*/
        transform.position = path.Last().transform.position;
        CheckCurrentNode();
    }

    //Virtual method for deciding which possible node to move to, will be different for each unit.
    public virtual MapNode MovePriority(List<MapNode> possibleMoveList)
    {
        return currentMapNode;
    }

    public virtual Unit AttackChoice()
    {
        return null;
    }

    public virtual bool Attack(Unit target, float damage)
    {
        return false;
    }

    public virtual bool Reaction(Unit target, float damage)
    {
        return false;
    }
}