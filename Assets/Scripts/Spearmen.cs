using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spearmen : Unit
{
    private void Awake()
    {
        //Declaration of variables for spearmen unit.
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        uIController = GameObject.Find("UIController").GetComponent<UIController>();
        mapGraph = GameObject.Find("MapGraph").GetComponent<MapGraph>();

        UnitType = "Spearmen";
        UnitDescription = "Spear";
        MaxHP = 25;
        CurrentHP = 25;
        MaxStamina = 4;
        CurrentStamina = 4;
        WeaponDamage = 5;
        WeaponRange = 2;
        AttackOrDefence = true;
        GrassCost = 1;
        AridCost = 2;
        IceCost = 2;
        MountainCost = 3;
        RiverCost = 2f;
        OceanCost = 10;
        NodeCostDict = new Dictionary<MapNode, float>();
        
        dijkstraScript = gameObject.GetComponent<Dijkstra>();
        GameObject.Find("GameManager").GetComponent<GameManager>().startupComplete = true;
    }
    
    //Selects which possible move to take, based on the least number of non-cavalry units adjacent to the node.
    public override MapNode MovePriority(List<MapNode> possibleMoveList)
    {
        //Ensures there are no occupied MapNodes in the list of possible moves.
        List<MapNode> tempMoves = possibleMoveList;
        foreach (MapNode node in tempMoves)
        {
            if (mapGraph.tileOccupationDict[node] != null)
            {
                possibleMoveList.Remove(node);
            }
        }
        possibleMoveList.Add(currentMapNode);
        //Sets up parameters for checking which MapNode is best to move to.
        MapNode currentBest = null;
        float currentBestScore = Mathf.Infinity;
        //Iterates through each possible move and calculates a score based on the number of a particular type of player
        //units adjacent to it and the distance to all player units.
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
                /*Debug.Log("Adjacent Node Occupied: " + adjacentNode.isOccupied.ToString());
                Debug.Log("OccObj: " + occObj.ToString());
                Debug.Log("OccObjEquals?: " + (occObj == GameObject.Find("Archers")));*/
                if (mapGraph.tileOccupationDict[adjacentNode] != null)
                {
                    if (gameManager.playerUnits.Contains(mapGraph.tileOccupationDict[adjacentNode].gameObject))
                    {
                        if (mapGraph.tileOccupationDict[adjacentNode].UnitType == "Cavalry")
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
                score = i * 2;
            }
            if (score <= currentBestScore)
            {
                currentBestScore = score;
                currentBest = node;
                //Debug.Log("Current Best: " + currentBest.name.ToString());
            }
        }
        destination = currentBest.transform.position;
        return currentBest;
    }
    //Chooses which target to attack, prioritising cavalry and grassland.
    public override Unit AttackChoice()
    {
        Unit unitToAttack = null;
        float currentBestScore = Mathf.NegativeInfinity;
        //Checks each adjacent node and calculates a score for the unit there based on it's type and the terrain.
        foreach (MapNode adjacentNode in currentMapNode.adjacentNodeDict.Keys)
        {
            if (mapGraph.tileOccupationDict[adjacentNode] != null)
            {
                float score = 0;
                if (mapGraph.tileOccupationDict[adjacentNode].UnitType == "Cavalry")
                {
                    score += 10;
                }
                else if (mapGraph.tileOccupationDict[adjacentNode].UnitType == "Swordsmen")
                {
                    score -= 4;
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
                    unitToAttack = mapGraph.tileOccupationDict[adjacentNode];
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
        if (target.UnitType == "Spearmen")
        {
            target.CurrentHP -= Mathf.RoundToInt(damage * 0.9f);
        }
        else if (target.UnitType == "Cavalry")
        {
            target.CurrentHP -= Mathf.RoundToInt(damage * 2);
        }
        else if (target.UnitType == "Swordsmen")
        {
            target.CurrentHP -= Mathf.RoundToInt(damage * 0.6f);
        }
        else if (target.UnitType == "Archers")
        {
            target.CurrentHP -= Mathf.RoundToInt(damage);
        }
        if (target.CurrentHP <= 0)
        {
            Destroy(target.gameObject);
            gameManager.isAttacking = false;
            gameManager.unitAttackedDict[this] = true;
            return true;
        }
        //If the target has not been killed, it attacks back.
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
            target.CurrentHP -= Mathf.RoundToInt(damage * 0.45f);
        }
        else if (target.UnitType == "Cavalry")
        {
            target.CurrentHP -= Mathf.RoundToInt(damage * 1.5f);
        }
        else if (target.UnitType == "Swordsmen")
        {
            target.CurrentHP -= Mathf.RoundToInt(damage * 0.3f);
        }
        else if (target.UnitType == "Archers")
        {
            target.CurrentHP -= Mathf.RoundToInt(damage * 0.5f);
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