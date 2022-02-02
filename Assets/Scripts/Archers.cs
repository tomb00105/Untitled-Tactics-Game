using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Archers : Unit
{
    private void Start()
    {
        UnitName = "Test";
        UnitType = "Archers";
        UnitDescription = "Bow";
        MaxHP = 10;
        CurrentHP = 10;
        MaxStamina = 5;
        CurrentStamina = 5;
        WeaponDamage = 7;
        AttackOrDefence = true;
        GrassCost = 1;
        AridCost = 2;
        IceCost = 2.5f;
        MountainCost = 3.5f;
        RiverCost = 1.5f;
        OceanCost = 10;
        NodeCostDict = new Dictionary<MapNode, float>();
        foreach (MapNode node in GameObject.Find("MapGraph").GetComponent<MapGraph>().graphCostDict.Keys)
        {
            if (node.terrainType == "Grassland")
            {
                NodeCostDict.Add(node, GrassCost);
                //Debug.Log("Grass Cost: " + NodeCostDict[node].ToString());
            }
            else if (node.terrainType == "Arid")
            {
                NodeCostDict.Add(node, AridCost);
            }
            else if (node.terrainType == "Icefield")
            {
                NodeCostDict.Add(node, IceCost);
            }
            else if (node.terrainType == "Mountain")
            {
                NodeCostDict.Add(node, MountainCost);
            }
            else if (node.terrainType == "River")
            {
                NodeCostDict.Add(node, RiverCost);
            }
            else if (node.terrainType == "Ocean")
            {
                NodeCostDict.Add(node, OceanCost);
            }
            else
            {
                Debug.LogWarning("NO TERRAIN TYPE FOR THIS NODE: " + node.name.ToString());
            }
        }
        dijkstraScript = gameObject.GetComponent<Dijkstra>();
        CheckCurrentNode();
        GameObject.Find("GameManager").GetComponent<GameManager>().startupComplete = true;
    }

    //Selects which possible move to take, based on the least number of any units on adjacent nodes.
    public override MapNode MovePriority(List<MapNode> possibleMoveList)
    {
        MapNode currentBest = null;
        float currentBestScore = Mathf.Infinity;
        foreach (MapNode node in possibleMoveList)
        {
            if (node.isOccupied)
            {
                continue;
            }
            List<GameObject> unitDistList = new List<GameObject>();
            float distanceVar = 0;

            foreach (GameObject unit in GameObject.FindGameObjectsWithTag("Player Unit"))
            {
                float distance = Mathf.Abs(node.transform.position.x - unit.transform.position.x) + Mathf.Abs(node.transform.position.y - unit.transform.position.y);
                Debug.Log("Distance from " + node.name.ToString() + " to " + unit.name.ToString() + " is " + distance.ToString());
                if (distance > 6 && distance <= 10)
                {
                    distanceVar += 0;
                }
                else if (distance > 4 && distance <= 6)
                {
                    distanceVar -= 2;
                }
                else if (distance > 2 && distance <= 4)
                {
                    distanceVar -= 1;
                    if (unit.GetComponent<Unit>().UnitType == "Swordsmen" || unit.GetComponent<Unit>().UnitType == "Spearmen" || unit.GetComponent<Unit>().UnitType == "Cavalry")
                    {
                        Debug.Log("Unit is Swordsmen or Spearmen");
                        distanceVar += 3;
                    }
                }
                else if (distance <= 2)
                {
                    distanceVar += 1;
                    if (unit.GetComponent<Unit>().UnitType == "Cavalry")
                    {
                        distanceVar += 2;
                    }
                }
                else
                {
                    distanceVar += 0;
                }

                
            }
            Debug.Log("distanceVar: " + distanceVar.ToString());
            float i = 0;
            float score;
            foreach (MapNode adjacentNode in node.adjacentNodeDict.Keys)
            {
                if (adjacentNode.isOccupied && adjacentNode.occupyingObject.CompareTag("Player Unit"))
                {
                    if (adjacentNode.occupyingObject.GetComponent<Unit>().UnitType == "Spearmen")
                    {
                        i++;
                        continue;
                    }
                    else if (adjacentNode.occupyingObject.GetComponent<Unit>().UnitType == "Swordsmen")
                    {
                        i++;
                        continue;
                    }
                    else if (adjacentNode.occupyingObject.GetComponent<Unit>().UnitType == "Cavalry")
                    {
                        i += 2;
                        continue;
                    }
                    else
                    {
                        i--;
                    }
                }
            }
            Debug.Log("i: " + i.ToString());
            if (AttackOrDefence)
            {
                score = i + distanceVar;
                Debug.Log("Score: " + score.ToString());
            }
            else
            {
                score = i - distanceVar;
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
        foreach (GameObject unit in GameObject.FindGameObjectsWithTag("Player Unit"))
        {
            float distance = Mathf.Abs(transform.position.x - unit.transform.position.x) + Mathf.Abs(transform.position.y - unit.transform.position.y);
            Debug.Log("Distance to " + unit.name.ToString() + " is " + distance);
            if (distance <= 6)
            {
                Debug.Log("Possible target: " + unit.name.ToString());
                possibleTargets.Add(unit.GetComponent<Unit>());
            }
        }
        foreach (Unit unit in possibleTargets)
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
            Debug.Log("Attack Score: " + score.ToString());
            Debug.Log("Unit Tag: " + unit.tag.ToString());
            if (score >= currentBestScore)
            {
                currentBestScore = score;
                unitToAttack = unit;
            }
        }
        if (currentBestScore <= 0 && unitToAttack == null)
        {
            return null;
        }
        else
        {

            Debug.Log("Unit to attack: " + unitToAttack.name.ToString());
            return unitToAttack;
        }
    }
    //Damages the target based on the unit type and returns true if the unit is wiped out.
    public override bool Attack(Unit target, float damage)
    {
        if (target == null)
        {
            Debug.Log("NO TARGET");
            return false;
        }
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
            target.currentMapNode.isOccupied = false;
            target.currentMapNode.occupyingObject = target.currentMapNode.gameObject;
            Destroy(target.gameObject);
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
            target.currentMapNode.isOccupied = false;
            target.currentMapNode.occupyingObject = target.currentMapNode.gameObject;
            Destroy(target.gameObject);
            return true;
        }
        else
        {
            return false;
        }
    }
}
