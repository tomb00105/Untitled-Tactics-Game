using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public UIController uIController;
    public List<Unit> allUnits = new List<Unit>();
    public List<Unit> turnUnits = new List<Unit>();
    public Dictionary<Unit, bool> turnUnitsDict = new Dictionary<Unit, bool>();
    public Dictionary<Unit, bool> unitMovedDict = new Dictionary<Unit, bool>();
    public Dictionary<Unit, bool> unitAttackedDict = new Dictionary<Unit, bool>();
    public string currentUnitTurn;
    public int turnNumber = 0;
    public bool startupComplete = false;
    public bool runOnce = false;
    public bool turnTaken = false;

    private void Awake()
    {
        foreach (GameObject unit in GameObject.FindGameObjectsWithTag("Player Unit"))
        {
            allUnits.Add(unit.GetComponent<Unit>());
        }
        foreach (GameObject unit in GameObject.FindGameObjectsWithTag("Enemy Unit"))
        {
            allUnits.Add(unit.GetComponent<Unit>());
        }
    }

    private void Update()
    {
        if (startupComplete && !runOnce)
        {
            runOnce = true;
            StartTurn("Enemy Unit");
        }
        
    }

    public bool StartTurn(string unitTurn)
    {
        turnNumber++;
        turnUnits.Clear();
        unitMovedDict.Clear();
        unitAttackedDict.Clear();
        uIController.highlightedObjects.Clear();
        uIController.selectedInfoUnit = null;
        uIController.selectedInfoMapNode = null;
        uIController.selectedMoveMapNode = null;
        uIController.selectedAttackUnit = null;
        foreach (GameObject unit in GameObject.FindGameObjectsWithTag(unitTurn))
        {
            unit.GetComponent<Unit>().path.Clear();
            unit.GetComponent<Dijkstra>().path.Clear();
            turnUnits.Add(unit.GetComponent<Unit>());
            turnUnitsDict.Add(unit.GetComponent<Unit>(), false);
            unitMovedDict.Add(unit.GetComponent<Unit>(), false);
            unitAttackedDict.Add(unit.GetComponent<Unit>(), false);
            //unit.GetComponent<Unit>().CurrentHP += 2;
            unit.GetComponent<Unit>().CurrentStamina = unit.GetComponent<Unit>().MaxStamina;
        }
        TakeTurn(unitTurn);
        return true;
    }

    public void TakeTurn(string unitTurn)
    {
        turnTaken = false;
        if (unitTurn == "Enemy Unit")
        {
            uIController.playerTurn = false;
            foreach (Unit unit in turnUnits)
            {
                unit.CheckCanMove(unit.NodeCostDict);
                /*while (unit.transform.position.x != unit.path.Last().transform.position.x && unit.transform.position.y != unit.path.Last().transform.position.y)
                {
                    unit.Move();
                }*/
                unit.currentMapNode.isOccupied = false;
                unit.currentMapNode.occupyingObject = unit.currentMapNode.gameObject;
                unit.currentMapNode = unit.path.Last();
                unit.currentMapNode.isOccupied = true;
                unit.currentMapNode.occupyingObject = unit.gameObject;
                unit.gameObject.transform.position = unit.path.Last().transform.position;
                unit.Attack(unit.AttackChoice(), unit.WeaponDamage);
            }
            EndTurn(unitTurn);
        }
        else if (unitTurn == "Player Unit")
        {
            uIController.playerTurn = true;
            uIController.endTurnButton.SetActive(true);
            uIController.unitPanel.SetActive(true);
            //uIController.TurnOnUI(new List<string> { "End Turn Button", "Unit Panel" });
        }
    }

    public bool EndTurn(string unitTurn)
    {
        foreach (Unit unit in turnUnits)
        {
            //CHANGES FOR ALL UNITS WHO HAVE JUST TAKEN THEIR TURN
        }
        if (unitTurn == "Player Unit")
        {
            uIController.playerTurn = false;
            uIController.TurnOffUI(new List<string> { "End Turn Button", "Unit Panel", "Unit Info Panel", "Move Panel", "Terrain Panel", "Attack Panel" });
            //TURN OFF UI AND OTHER UNWANTED INTERACTIONS
            unitTurn = "Enemy Unit";
        }
        else
        {
            //MAKE SURE THE AI IS NOT ACTIVE
            unitTurn = "Player Unit";
        }
        StartTurn(unitTurn);
        return true;
    }
}
