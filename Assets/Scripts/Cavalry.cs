using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cavalry : Unit
{
    private void Awake()
    {
        CheckCurrentNode();
    }
    private void Start()
    {
        UnitName = "Test";
        UnitType = "Cavalry";
        UnitDescription = "Horse";
        MaxHP = 15;
        CurrentHP = 15;
        MaxStamina = 8;
        CurrentStamina = 8;
        WeaponDamage = 8;
        AttackOrDefence = true;
        GrassCost = 1;
        AridCost = 1;
        IceCost = 3;
        MountainCost = 4;
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

    //Selects which possible move to take, based on the least number of non-archer and non-swordsmen units adjacent to the node.
    public override MapNode MovePriority(List<MapNode> possibleMoveList)
    {
        MapNode currentBest = null;
        float currentBestScore = Mathf.Infinity;
        foreach (MapNode node in possibleMoveList)
        {
            List<GameObject> unitDistList = new List<GameObject>();
            float distanceVar = 0;

            foreach (GameObject unit in GameObject.FindGameObjectsWithTag("Player Unit"))
            {
                distanceVar += Vector2.Distance(node.transform.position, unit.transform.position);
            }
            int i = 0;
            float score;
            foreach (MapNode adjacentNode in node.adjacentNodeDict.Keys)
            {
                if (adjacentNode.isOccupied && adjacentNode.occupyingObject.CompareTag("Player Unit"))
                {
                    if (adjacentNode.occupyingObject.GetComponent<Unit>().UnitType == "Archers")
                    {
                        i -= 2;
                        continue;
                    }
                    else if (adjacentNode.occupyingObject.GetComponent<Unit>().UnitType == "Swordsmen")
                    {
                        i--;
                        continue;
                    }
                    else if (adjacentNode.occupyingObject.GetComponent<Unit>().UnitType == "Spearmen")
                    {
                        i += 2;
                        continue;
                    }
                    i++;
                }
            }
            if (AttackOrDefence)
            {
                score = i * 2 + distanceVar;
            }
            else
            {
                score = i * 2 - distanceVar;
            }
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
            if (adjacentNode.isOccupied)
            {
                if (adjacentNode.occupyingObject.GetComponent<Unit>().UnitType == "Archers")
                {
                    score += 10;
                }
                else if (adjacentNode.occupyingObject.GetComponent<Unit>().UnitType == "Swordsmen")
                {
                    score += 5;
                }
                if (adjacentNode.terrainType == "Grassland")
                {
                    score += 5;
                }
                else if (adjacentNode.terrainType == "Icefield")
                {
                    score -= 5; ;
                }
                else if (adjacentNode.terrainType == "Ocean")
                {
                    score = -10;
                }
            }
            Debug.Log("Attack Score: " + score.ToString());
            Debug.Log("Unit Tag: " + adjacentNode.occupyingObject.tag.ToString());
            if (score >= currentBestScore && adjacentNode.occupyingObject.CompareTag("Player Unit"))
            {
                currentBestScore = score;
                unitToAttack = adjacentNode.occupyingObject.GetComponent<Unit>();
                Debug.Log("Unit to attack: " + unitToAttack.name.ToString());
            }
        }
        if (currentBestScore <= -10)
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
        if (target == null)
        {
            Debug.Log("NO TARGET");
            return false;
        }
        else if (target.UnitType == "Spearmen")
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
            Debug.Log("Unit Killed");
            target.currentMapNode.isOccupied = false;
            target.currentMapNode.occupyingObject = target.currentMapNode.gameObject;
            Destroy(target.gameObject);
            return true;
        }
        else
        {
            Debug.Log("Target HP: " + target.CurrentHP.ToString());
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
