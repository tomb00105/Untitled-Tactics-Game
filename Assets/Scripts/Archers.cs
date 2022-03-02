using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Archers : Unit
{
    private void Awake()
    {
        //Declaration of variables for Archer Unit.
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        uIController = GameObject.Find("UIController").GetComponent<UIController>();
        mapGraph = GameObject.Find("MapGraph").GetComponent<MapGraph>();

        UnitType = "Archers";
        UnitDescription = "Bow";
        MaxHP = 10;
        CurrentHP = 10;
        MaxStamina = 5;
        CurrentStamina = 5;
        WeaponDamage = 7;
        WeaponRange = 6;
        AttackOrDefence = true;
        GrassCost = 1;
        AridCost = 2;
        IceCost = 2.5f;
        MountainCost = 3.5f;
        RiverCost = 1.5f;
        OceanCost = 10;
        NodeCostDict = new Dictionary<MapNode, float>();
        
        dijkstraScript = gameObject.GetComponent<Dijkstra>();
        //GameObject.Find("GameManager").GetComponent<GameManager>().startupComplete = true; Deprecated but kept for posterity
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
        if (gameManager.playerUnits.Contains(gameObject))
        {
            gameManager.playerUnits.Remove(gameObject);
        }
        if (gameManager.enemyUnits.Contains(gameObject))
        {
            gameManager.enemyUnits.Remove(gameObject);
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

    
    //Selects which possible move to take, based on the least number of any units on adjacent nodes.
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
            float distanceVar = 0;
            float totaldistanceVar = 0;

            foreach (GameObject unit in GameObject.FindGameObjectsWithTag("Player Unit"))
            {
                float distance = Mathf.Abs(node.transform.position.x - unit.transform.position.x) + Mathf.Abs(node.transform.position.y - unit.transform.position.y);
                totaldistanceVar += Vector2.Distance(node.transform.position, unit.transform.position) / 10;
                //Debug.Log("Distance from " + node.name.ToString() + " to " + unit.name.ToString() + " is " + distance.ToString());
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
            //Debug.Log("distanceVar: " + distanceVar.ToString());
            float i = 0;
            float score;
            foreach (MapNode adjacentNode in node.adjacentNodeDict.Keys)
            {
                if (mapGraph.tileOccupationDict[adjacentNode] != null)
                {
                    if (gameManager.playerUnits.Contains(mapGraph.tileOccupationDict[adjacentNode].gameObject))
                    {
                        if (mapGraph.tileOccupationDict[adjacentNode].UnitType == "Spearmen")
                        {
                            i++;
                            continue;
                        }
                        else if (mapGraph.tileOccupationDict[adjacentNode].UnitType == "Swordsmen")
                        {
                            i++;
                            continue;
                        }
                        else if (mapGraph.tileOccupationDict[adjacentNode].UnitType == "Cavalry")
                        {
                            i += 2;
                            continue;
                        }
                        else
                        {
                            i--;
                        }
                    }
                    else if (gameManager.enemyUnits.Contains(mapGraph.tileOccupationDict[adjacentNode].gameObject))
                    {
                        i --;
                    }
                }
                else
                {
                    i--;
                }

            }
            //Debug.Log("i: " + i.ToString());
            if (AttackOrDefence)
            {
                score = i * 2 + distanceVar + totaldistanceVar;
                //Debug.Log("i * 2: " + (i * 2).ToString() + " + distanceVar: " + distanceVar.ToString() + " - totalDistanceVar: " + totaldistanceVar.ToString());
                //Debug.Log("Score: " + score.ToString() + " Node: " + node.name);
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
    //Checks which targets are in range and then prioritises ones that are below the archers, if they are in mountains, and cavalry above others.
    public override Unit AttackChoice()
    {
        List<Unit> possibleTargets = new List<Unit>();
        Unit unitToAttack = null;
        float currentBestScore = Mathf.NegativeInfinity;
        foreach (GameObject unit in GameObject.FindGameObjectsWithTag("Player Unit"))
        {
            float distance = Mathf.Abs(transform.position.x - unit.transform.position.x) + Mathf.Abs(transform.position.y - unit.transform.position.y);
            //Debug.Log("Distance to " + unit.name.ToString() + " is " + distance);
            if (distance <= 6)
            {
                //Debug.Log("Possible target: " + unit.name.ToString());
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
            //Debug.Log("Attack Score: " + score.ToString());
            //Debug.Log("Unit Tag: " + unit.tag.ToString());
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
            //Debug.Log("Unit to attack: " + unitToAttack.name.ToString());
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
            Destroy(target.gameObject);
            gameManager.isAttacking = false;
            gameManager.unitAttackedDict[this] = true;
            return true;
        }
        else
        {
            if (target.UnitType != "Archers" && Vector2.Distance(transform.position, target.transform.position) <= 2)
            {
                target.Reaction(this.GetComponent<Unit>(), target.WeaponDamage);
            }
            else if (target.UnitType == "Archers" && Vector2.Distance(transform.position, target.transform.position) <= 6)
            {
                target.Reaction(this.GetComponent<Unit>(), target.WeaponDamage);
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
