using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Swordsmen : Unit
{
    private Dijkstra dijkstraScript;
    //Constructor for Swordsmen units.
    public Swordsmen(string unitName, string unitType, string unitDescription) : base(unitName, unitType, unitDescription)
    {
        MaxHP = 20;
        MaxStamina = 15;
        GrassCost = 1;
        AridCost = 2;
        IceCost = 2;
        MountainCost = 3;
        RiverCost = 1.5f;
        OceanCost = 10;
    }

    private void Awake()
    {
        dijkstraScript = gameObject.GetComponent<Dijkstra>();
        CheckCurrentNode();
    }

    //Selects which possible move to take, based on the least number of non-spearmen units adjacent to the node.
    public override MapNode MovePriority(List<MapNode> possibleMoveList)
    {
        MapNode currentBest = null;
        float currentBestScore = Mathf.Infinity;
        foreach  (MapNode node in possibleMoveList)
        {
            int i = 0;
            float score;
            foreach  (MapNode adjacentNode in node.adjacentNodeDict.Keys)
            {
                if (adjacentNode.isOccupied && adjacentNode.occupyingObject.CompareTag("Player Unit"))
                {
                    if (adjacentNode.occupyingObject.GetComponent<Unit>().UnitType == "Spearmen")
                    {
                        continue;
                    }
                    i++;
                }
            }
            score = 2 * i + dijkstraScript.dijkstraDict[node];
            if (score < currentBestScore)
            {
                currentBestScore = score;
                currentBest = node;
            }
        }
        return currentBest;
    }
    public override Unit AttackChoice()
    {
        Unit unitToAttack = null;
        float currentBestScore = 0;
        foreach (MapNode adjacentNode in currentMapNode.adjacentNodeDict.Keys)
        {
            float score = 0;
            if (adjacentNode.isOccupied && adjacentNode.occupyingObject.GetComponent<Unit>().UnitType == "Spearmen")
            {
                score += 10;
            }
            else if (adjacentNode.isOccupied && adjacentNode.occupyingObject.GetComponent<Unit>().UnitType == "Archers")
            {
                score += 5;
            }
            if (adjacentNode.terrainType == "Grassland")
            {
                score += 5;
            }
            else if (adjacentNode.terrainType == "Icefield")
            {
                score -= 5;
            }
            else if (adjacentNode.terrainType == "Ocean")
            {
                score = -10;
            }
            if (score > currentBestScore)
            {
                currentBestScore = score;
                unitToAttack = adjacentNode.occupyingObject.GetComponent<Unit>();
            }
        }
        if (currentBestScore == -10)
        {
            return null;
        }
        else
        {
            return unitToAttack;
        }
    }
}
