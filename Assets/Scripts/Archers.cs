using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Archers : Unit
{
    //Constructor for Archer units.
  

    private void Awake()
    {
        MaxHP = 10;
        MaxStamina = 5;
        WeaponDamage = 7;
        GrassCost = 1;
        AridCost = 2;
        IceCost = 2.5f;
        MountainCost = 3.5f;
        RiverCost = 1.5f;
        OceanCost = 10;
        NodeCostDict = new Dictionary<MapNode, float>();
        dijkstraScript = gameObject.GetComponent<Dijkstra>();
        CheckCurrentNode();
    }

    //Selects which possible move to take, based on the least number of any units on adjacent nodes.
    public override MapNode MovePriority(List<MapNode> possibleMoveList)
    {
        MapNode currentBest = null;
        float currentBestScore = Mathf.Infinity;
        foreach (MapNode node in possibleMoveList)
        {
            float i = 0;
            float score;
            foreach (MapNode adjacentNode in node.adjacentNodeDict.Keys)
            {
                if (adjacentNode.isOccupied && adjacentNode.occupyingObject.CompareTag("Player Unit"))
                {
                    if (adjacentNode.occupyingObject.GetComponent<Unit>().UnitType == "Spearmen")
                    {
                        continue;
                    }
                    else if (adjacentNode.occupyingObject.GetComponent<Unit>().UnitType == "Swordsmen")
                    {
                        continue;
                    }
                    else if (adjacentNode.occupyingObject.GetComponent<Unit>().UnitType == "Cavalry")
                    {
                        i += 2;
                        continue;
                    }
                    else
                    {
                        i++;
                    }
                }
            }
            if (i == 0)
            {
                score = dijkstraScript.dijkstraDict[node] - 1;
            }
            else
            {
                score = 2 * i + dijkstraScript.dijkstraDict[node];
            }
            if (score <= currentBestScore)
            {
                currentBestScore = score;
                currentBest = node;
            }
        }
        return currentBest;
    }
    //Checks which targets are in range and then prioritises ones that are below the archers, if they are in mountains, and cavalry above others.
    public override Unit AttackChoice()
    {
        List<Unit> possibleTargets = new List<Unit>();
        Unit unitToAttack = null;
        float currentBestScore = 0;
        foreach (MapNode mapNode in dijkstraScript.dijkstraDict.Keys)
        {
            if (Vector2.Distance(currentMapNode.transform.position, mapNode.transform.position) <= 6 && mapNode.occupyingObject.CompareTag("Player Unit"))
            {
                possibleTargets.Add(mapNode.occupyingObject.GetComponent<Unit>());
            }
        }
        foreach (Unit unit in  possibleTargets)
        {
            float score = 0;
            if (currentMapNode.terrainType == "Mountain" && unit.currentMapNode.terrainType != "Mountain")
            {
                score += 6;
            }
            if (unit.UnitType == "Cavalry")
            {
                score += 10;
            }
            else if (unit.UnitType == "Swordsmen" || unit.UnitType == "Archers")
            {
                score += 5;
            }
            if (score > currentBestScore)
            {
                currentBestScore = score;
                unitToAttack = unit;
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
            target.CurrentHP -= Mathf.RoundToInt(damage * 0.8f);
        }
        else if (target.UnitType == "Cavalry")
        {
            target.CurrentHP -= Mathf.RoundToInt(damage * 1.7f);
        }
        else if (target.UnitType == "Swordsmen")
        {
            target.CurrentHP -= Mathf.RoundToInt(damage * 1.2f);
        }
        else if (target.UnitType == "Archers")
        {
            target.CurrentHP -= Mathf.RoundToInt(damage * 1.3f);
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
            target.CurrentHP -= Mathf.RoundToInt(damage * 0.6f);
        }
        else if (target.UnitType == "Cavalry")
        {
            target.CurrentHP -= Mathf.RoundToInt(damage * 0.85f);
        }
        else if (target.UnitType == "Swordsmen")
        {
            target.CurrentHP -= Mathf.RoundToInt(damage * 0.7f);
        }
        else if (target.UnitType == "Archers")
        {
            target.CurrentHP -= Mathf.RoundToInt(damage * 1f);
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
