using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    //Unit info delcarations.
    public string unitName;
    public string unitDescription;
    public int hP;
    public int stamina;

    public float grassCost;
    public float aridCost;
    public float iceCost;
    public float mountainCost;
    public float riverCost;
    public float oceanCost;

    //Dictionary to keep track of the cost for this unit to move to each MapNode.
    public Dictionary<MapNode, float> nodeCostDict = new Dictionary<MapNode, float>();
    public MapNode currentMapNode;

    private void Awake()
    {
        //Checks which MapNode the unit is currently on.
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
}