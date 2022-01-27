using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cavalry : Unit
{
    //Constructor for Cavalry units.
   

    private void Awake()
    {
        MaxHP = 15;
        MaxStamina = 8;
        WeaponDamage = 8;
        AttackOrDefence = true;
        GrassCost = 1;
        AridCost = 1;
        IceCost = 3;
        MountainCost = 4;
        RiverCost = 1.5f;
        OceanCost = 10;
        NodeCostDict = new Dictionary<MapNode, float>();
        dijkstraScript = gameObject.GetComponent<Dijkstra>();
        CheckCurrentNode();
    }

    //Selects which possible move to take, based on the least number of non-archer and non-swordsmen units adjacent to the node.
    public override MapNode MovePriority(List<MapNode> possibleMoveList)
    {
        MapNode currentBest = null;
        float currentBestScore = Mathf.Infinity;
        foreach (MapNode node in possibleMoveList)
        {
            int i = 0;
            float score;
            foreach (MapNode adjacentNode in node.adjacentNodeDict.Keys)
            {
                if (adjacentNode.isOccupied && adjacentNode.occupyingObject.CompareTag("Player Unit"))
                {
                    if (adjacentNode.occupyingObject.GetComponent<Unit>().UnitType == "Archers")
                    {
                        continue;
                    }
                    else if (adjacentNode.occupyingObject.GetComponent<Unit>().UnitType == "Swordsmen")
                    {
                        continue;
                    }
                    i++;
                }
            }
            score = 2 * i + dijkstraScript.dijkstraDict[node];
            if (score <= currentBestScore)
            {
                currentBestScore = score;
                currentBest = node;
            }
        }
        return currentBest;
    }
    //Chooses which unit to attack, prioritising Archers, Swordsmen and grassland.
    public override Unit AttackChoice()
    {
        Unit unitToAttack = null;
        float currentBestScore = 0;
        foreach (MapNode adjacentNode in currentMapNode.adjacentNodeDict.Keys)
        {
            float score = 0;
            if (adjacentNode.isOccupied && adjacentNode.occupyingObject.GetComponent<Unit>().UnitType == "Archers")
            {
                score += 10;
            }
            else if (adjacentNode.isOccupied && adjacentNode.occupyingObject.GetComponent<Unit>().UnitType == "Swordsmen")
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
    //Damages the target based on the unit type and returns true if the unit is wiped out.
    public override bool Attack(Unit target, float damage)
    {
        if (target.UnitType == "Spearmen")
        {
            target.CurrentHP -= Mathf.RoundToInt(damage * 0.5f);
        }
        else if (target.UnitType == "Cavalry")
        {
            target.CurrentHP -= Mathf.RoundToInt(damage);
        }
        else if (target.UnitType == "Swordsmen")
        {
            target.CurrentHP -= Mathf.RoundToInt(damage * 1.2f);
        }
        else if (target.UnitType == "Archers")
        {
            target.CurrentHP -= Mathf.RoundToInt(damage * 1.5f);
        }
        if (target.CurrentHP <= 0)
        {
            return true;
        }
        else
        {
            if (target.UnitType != "Archers" && Vector2.Distance(currentMapNode.transform.position, target.currentMapNode.transform.position) <= 2)
            {
                target.Reaction(this, target.WeaponDamage);
            }
            else if (target.UnitType == "Archers" && Vector2.Distance(currentMapNode.transform.position, target.transform.position) <= 6)
            {
                target.Reaction(this, target.WeaponDamage);
            }
            return false;
        }
    }
    //If within range when attacked, this unit will retaliate, although will deal less damage than if attacking themselves.
    public override bool Reaction(Unit target, float damage)
    {
        if (target.UnitType == "Spearmen")
        {
            target.CurrentHP -= Mathf.RoundToInt(damage * 0.3f);
        }
        else if (target.UnitType == "Cavalry")
        {
            target.CurrentHP -= Mathf.RoundToInt(damage * 0.7f);
        }
        else if (target.UnitType == "Swordsmen")
        {
            target.CurrentHP -= Mathf.RoundToInt(damage * 0.8f);
        }
        else if (target.UnitType == "Archers")
        {
            target.CurrentHP -= Mathf.RoundToInt(damage * 1.2f);
        }
        if (target.CurrentHP <= 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
