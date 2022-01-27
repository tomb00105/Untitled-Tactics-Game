using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Unit : MonoBehaviour
{
    protected Dijkstra dijkstraScript;

    //Unit info declarations.
    public string UnitName
    { get; protected set; }
    public string UnitType
    { get; set; }
    public string UnitDescription
    { get; protected set; }
    public float MaxHP
    { get; protected set; }
    public float CurrentHP
    { get; set; }
    public float MaxStamina
    { get; protected set; }
    public float CurrentStamina
    { get; set; }
    public float WeaponDamage
    { get; set; }

    public bool AttackOrDefence
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
    public MapNode currentMapNode;
    public List<MapNode> path = new List<MapNode>();

    //Constructor for Units
    

    //Checks which MapNode the unit is currently on.
    public virtual void CheckCurrentNode()
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
    public virtual bool CheckCanMove(Dictionary<MapNode, float> costDict)
    {
        if (!dijkstraScript.DijkstraCalc(costDict))
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
        currentMapNode.isOccupied = false;
        currentMapNode.occupyingObject = currentMapNode.gameObject;
        CheckCurrentNode();
        currentMapNode.isOccupied = true;
        currentMapNode.occupyingObject = gameObject;
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