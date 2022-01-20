using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapNode : MonoBehaviour
{
    public float cost;
    public string terrainType;

    //Dictionary of all MapNodes and distance to them from this MapNode.
    private Dictionary<MapNode, float> allNodes = new Dictionary<MapNode, float>();

    //Dictionary of adjacent MapNodes.
    public Dictionary<MapNode, Vector3> adjacentNodeDict = new Dictionary<MapNode, Vector3>();

    //Used to track if the MapNode currently has anything on it, e.g. a character.
    public bool isOccupied = false;
    public GameObject occupyingObject;

    private void Start()
    {
        //Gets adjacent MapNodes and adds them to the dictionary.
        foreach (Collider2D collider in Physics2D.OverlapCircleAll(transform.position, 1f))
        {
            //Checks that the object connected to the collider found is a MapNode and is not the current MapNode before
            //adding to the dictionary.
            if (collider.gameObject.TryGetComponent(out MapNode node) && gameObject != collider.gameObject)
            {
                adjacentNodeDict.Add(node, node.transform.position);
            }
        }
        /*DEBUGGING FOR ADJACENT NODE DICTIONARY CREATION
        foreach (Vector3 nodePosition in adjacentNodeDict.Values)
        {
            Debug.Log("Adjacent Node at: " + nodePosition);
        }
        */
    }
}
