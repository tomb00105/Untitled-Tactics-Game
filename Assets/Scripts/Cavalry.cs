using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cavalry : Unit
{
    private void Start()
    {
        //Delcarations for cavalry unit variables.
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        uIController = GameObject.Find("UIController").GetComponent<UIController>();
        mapGraph = GameObject.Find("MapGraph").GetComponent<MapGraph>();

        UnitType = "Cavalry";
        UnitDescription = "Horse";
        MaxHP = 15;
        CurrentHP = 15;
        MaxStamina = 8;
        CurrentStamina = 8;
        WeaponDamage = 8;
        WeaponRange = 2;
        AttackOrDefence = true;
        GrassCost = 1;
        AridCost = 1;
        IceCost = 3;
        MountainCost = 4;
        RiverCost = 1.5f;
        OceanCost = 10;
        NodeCostDict = new Dictionary<MapNode, float>();
        //Setup of MapNode edge costs for cavalry unit.
        
        dijkstraScript = gameObject.GetComponent<Dijkstra>();
        GameObject.Find("GameManager").GetComponent<GameManager>().startupComplete = true;
    }

    private void OnDestroy()
    {
        if (gameManager.currentUnitTurn == "Enemy Unit" && CompareTag("Enemy Unit"))
        {
            gameManager.unitTurnsTaken--;
        }
        List<MapNode> mapList = new List<MapNode>();
        foreach (MapNode node in mapGraph.tileOccupationDict.Keys)
        {
            mapList.Add(node);
        }
        foreach (MapNode mapNode in mapList)
        {
            if (mapGraph.tileOccupationDict[mapNode] == this)
            {
                mapGraph.tileOccupationDict[mapNode] = null;
            }
            
        }
        gameManager.allUnits.Remove(this);
        if (gameManager.turnUnits.Contains(this))
        {
            gameManager.turnUnits.Remove(this);
        }
        if (gameManager.playerUnits.Contains(this.gameObject))
        {
            gameManager.playerUnits.Remove(this.gameObject);
        }
        if (gameManager.enemyUnits.Contains(this.gameObject))
        {
            gameManager.enemyUnits.Remove(this.gameObject);
        }
        if (gameManager.turnUnitsDict.ContainsKey(this))
        {
            gameManager.turnUnitsDict.Remove(this);
        }
        if (gameManager.unitMovedDict.ContainsKey(this))
        {
            gameManager.unitMovedDict.Remove(this);
        }
        if (gameManager.unitAttackedDict.ContainsKey(this))
        {
            gameManager.unitAttackedDict.Remove(this);
        }
        if (gameManager.enemyUnits.Count == 0)
        {
            gameManager.Wipeout("Enemy Unit");
        }
        else if (gameManager.playerUnits.Count == 0)
        {
            gameManager.Wipeout("Player Unit");
        }
    }

    //Selects which possible move to take, based on the least number of non-archer and non-swordsmen units adjacent to the node.
    public override MapNode MovePriority(List<MapNode> possibleMoveList)
    {
        List<MapNode> tempMoves = possibleMoveList;
        foreach (MapNode node in tempMoves)
        {
            if (mapGraph.tileOccupationDict[node] != null)
            {
                possibleMoveList.Remove(node);
            }
        }
        possibleMoveList.Add(currentMapNode);
        MapNode currentBest = null;
        float currentBestScore = Mathf.Infinity;
        foreach (MapNode node in possibleMoveList)
        {
            List<GameObject> unitDistList = new List<GameObject>();
            float distanceVar = 0;

            foreach (GameObject unit in GameObject.FindGameObjectsWithTag("Player Unit"))
            {
                distanceVar += Vector2.Distance(node.transform.position, unit.transform.position) / 10;
            }
            int i = 0;
            float score;
            foreach (MapNode adjacentNode in node.adjacentNodeDict.Keys)
            {
                if (mapGraph.tileOccupationDict[adjacentNode] != null)
                {
                    if (gameManager.playerUnits.Contains(mapGraph.tileOccupationDict[adjacentNode].gameObject))
                    {
                        if (mapGraph.tileOccupationDict[adjacentNode].UnitType == "Archers")
                        {
                            i -= 2;
                            continue;
                        }
                        else
                        {
                            i--;
                            continue;
                        }
                    }
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
        destination = currentBest.transform.position;
        return currentBest;
    }
    //Chooses which unit to attack, prioritising Archers, Swordsmen and grassland.
    public override Unit AttackChoice()
    {
        Unit unitToAttack = null;
        float currentBestScore = Mathf.NegativeInfinity;
        foreach (MapNode adjacentNode in currentMapNode.adjacentNodeDict.Keys)
        {
            if (mapGraph.tileOccupationDict[adjacentNode] != null)
            {
                float score = 0;
                    if (mapGraph.tileOccupationDict[adjacentNode].UnitType == "Archers")
                    {
                        score += 10;
                    }
                    else if (mapGraph.tileOccupationDict[adjacentNode].UnitType == "Swordsmen")
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
                //Debug.Log("Attack Score: " + score.ToString());
                //Debug.Log("Unit Tag: " + mapGraph.tileOccupationDict[adjacentNode].tag.ToString());
                if (score >= currentBestScore && mapGraph.tileOccupationDict[adjacentNode].CompareTag("Player Unit"))
                {
                    currentBestScore = score;
                    unitToAttack = mapGraph.tileOccupationDict[adjacentNode];
                    //Debug.Log("Unit to attack: " + unitToAttack.name.ToString());
                }
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
            //Debug.Log("NO TARGET");
            gameManager.unitAttackedDict[this] = true;
            gameManager.isAttacking = false;
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
            Destroy(target.gameObject);
            gameManager.isAttacking = false;
            gameManager.unitAttackedDict[this] = true;
            return true;
        }
        else
        {
            //Debug.Log("Target HP: " + target.CurrentHP.ToString());
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
            uIController.UnitPanelsDefault();
            Destroy(target.gameObject);
            gameManager.isAttacking = false;
            gameManager.unitAttackedDict[target] = true;
            return true;
        }
        else
        {
            gameManager.isAttacking = false;
            gameManager.unitAttackedDict[target] = true;
            return false;
        }
    }
}
