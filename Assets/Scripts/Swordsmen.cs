using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Swordsmen : Unit
{
    private void Awake()
    {
        CheckCurrentNode();
    }
    private void Start()
    {
        UnitName = "Test";
        UnitType = "Swordsmen";
        UnitDescription = "Sword";
        MaxHP = 20;
        CurrentHP = 20;
        MaxStamina = 5;
        CurrentStamina = 5;
        WeaponDamage = 5;
        AttackOrDefence = true;
        GrassCost = 1;
        AridCost = 2;
        IceCost = 2;
        MountainCost = 3;
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
        GameObject.Find("GameManager").GetComponent<GameManager>().startupComplete = true;
    }

    //Selects which possible move to take, based on the least number of non-spearmen units adjacent to the node.
    public override MapNode MovePriority(List<MapNode> possibleMoveList)
    {
        MapNode currentBest = null;
        float currentBestScore = Mathf.Infinity;
        foreach  (MapNode node in possibleMoveList)
        {
            List<GameObject> unitDistList = new List<GameObject>();
            float distanceVar = 0;

            foreach (GameObject unit in GameObject.FindGameObjectsWithTag("Player Unit"))
            {
                distanceVar += Vector2.Distance(node.transform.position, unit.transform.position);
            }
            int i = 0;
            float score;
            foreach  (MapNode adjacentNode in node.adjacentNodeDict.Keys)
            {
                if (adjacentNode.isOccupied && adjacentNode.occupyingObject.CompareTag("Player Unit"))
                {
                    if (adjacentNode.occupyingObject.GetComponent<Unit>().UnitType == "Swordsmen")
                    {
                        i--;
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
                score = i * 2;
            }
            Debug.Log(node.name.ToString() + " Score: " + score.ToString() + " Current Best Score: " + currentBestScore.ToString());
            if (score <= currentBestScore)
            {
                currentBestScore = score;
                currentBest = node;
            }
        }
        Debug.Log("Best Node to move to: " + currentBest.name.ToString());
        return currentBest;
    }
    //Chooses which unit to attack, prioritising spearmen and grassland;
    public override Unit AttackChoice()
    {
        Unit unitToAttack = null;
        float currentBestScore = 0;
        foreach (MapNode adjacentNode in currentMapNode.adjacentNodeDict.Keys)
        {
            float score = 0;
            if (adjacentNode.isOccupied)
            {
                if (adjacentNode.occupyingObject.CompareTag("Player Unit"))
                {
                    if (adjacentNode.occupyingObject.GetComponent<Unit>().UnitType == "Spearmen")
                    {
                        score += 10;
                    }
                    else if (adjacentNode.occupyingObject.GetComponent<Unit>().UnitType == "Archers")
                    {
                        score += 5;
                    }
                }
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
            target.CurrentHP -= Mathf.RoundToInt(damage * 1.5f);
        }
        else if (target.UnitType == "Cavalry")
        {
            target.CurrentHP -= Mathf.RoundToInt(damage * 0.8f);
        }
        else if (target.UnitType == "Swordsmen")
        {
            target.CurrentHP -= Mathf.RoundToInt(damage);
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
            Debug.Log("Target HP: " + target.CurrentHP.ToString());
            if (target.UnitType != "Archers" && Vector2.Distance(currentMapNode.transform.position, target.currentMapNode.transform.position) <= 2)
            {
                target.Reaction(GetComponent<Unit>(), target.WeaponDamage);
            }
            else if (target.UnitType == "Archers" && Vector2.Distance(currentMapNode.transform.position, target.transform.position) <= 6)
            {
                target.Reaction(GetComponent<Unit>(), target.WeaponDamage);
            }
            return false;
        }
    }
    //If within range when attacked, this unit will retaliate, although will deal less damage than if attacking themselves.
    public override bool Reaction(Unit target, float damage)
    {
        Debug.Log("Reaction WeaponDamage: " + WeaponDamage.ToString());
        if (target.UnitType == "Spearmen")
        {
            target.CurrentHP -= Mathf.RoundToInt(damage * 1.2f);
        }
        else if (target.UnitType == "Cavalry")
        {
            target.CurrentHP -= Mathf.RoundToInt(damage * 0.4f);
        }
        else if (target.UnitType == "Swordsmen")
        {
            target.CurrentHP -= Mathf.RoundToInt(damage * 0.5f);
        }
        else if (target.UnitType == "Archers")
        {
            target.CurrentHP -= Mathf.RoundToInt(damage);
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
            Debug.Log("Reaction target HP: " + target.CurrentHP.ToString());
            return false;
        }
    }
}
