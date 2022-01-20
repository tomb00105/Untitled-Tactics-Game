using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    private Dijkstra dijkstraScript;

    //Unit info delcarations.
    public string unitName;
    public string unitDescription;
    public int hP;
    public int stamina;

    //Initialisation of costs for this unit to move across each type of terrain.
    public float grassCost;
    public float aridCost;
    public float iceCost;
    public float mountainCost;
    public float riverCost;
    public float oceanCost;

    //Dictionary to keep track of the cost for this unit to move to each MapNode.
    public Dictionary<MapNode, float> nodeCostDict = new Dictionary<MapNode, float>();
    public MapNode currentMapNode;
    public List<MapNode> path = new List<MapNode>();

    private void Awake()
    {
        dijkstraScript = gameObject.GetComponent<Dijkstra>();
        CheckCurrentNode();
    }

    //Checks which MapNode the unit is currently on.
    public void CheckCurrentNode()
    {
        foreach (Collider2D collider in Physics2D.OverlapCircleAll(transform.position, 1f))
        {
            if (collider.transform.position.x == transform.position.x && collider.transform.position.y == transform.position.y && collider.gameObject != gameObject)
            {
                currentMapNode = collider.gameObject.GetComponent<MapNode>();
                collider.gameObject.GetComponent<MapNode>().isOccupied = true;
                collider.gameObject.GetComponent<MapNode>().occupyingObject = gameObject;
            }
        }
    }

    //Decides whether the unit can/will move and where to.
    public bool MoveDecision()
    {
        if (!dijkstraScript.DijkstraCalc())
        {
            Debug.Log("Cannot move!");
            return false;
        }
        List<MapNode> moves = dijkstraScript.PossibleMoves();
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

    //Moves the unit and updates MapNode occupation status.
    public void Move()
    {
        while (transform.position.x != path[path.Count].transform.position.x && transform.position.y != path[path.Count].transform.position.x)
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
        }
        currentMapNode.isOccupied = false;
        currentMapNode.occupyingObject = null;
        currentMapNode = path[path.Count];
        currentMapNode.isOccupied = true;
        currentMapNode.occupyingObject = gameObject;
    }

    //Virtual method for deciding which possible node to move to, will be different for each unit.
    public virtual MapNode MovePriority(List<MapNode> possibleMoveList)
    {
        return currentMapNode;
    }
}