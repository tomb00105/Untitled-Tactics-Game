using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public UIController uIController;
    public List<Unit> allUnits = new List<Unit>();
    public List<Unit> turnUnits = new List<Unit>();
    public List<GameObject> playerUnits = new List<GameObject>();
    public List<GameObject> enemyUnits = new List<GameObject>();
    public Dictionary<Unit, bool> turnUnitsDict = new Dictionary<Unit, bool>();
    public Dictionary<Unit, bool> unitMovedDict = new Dictionary<Unit, bool>();
    public Dictionary<Unit, bool> unitAttackedDict = new Dictionary<Unit, bool>();
    public string currentUnitTurn;
    public int turnNumber = 0;
    public bool startupComplete = false;
    public bool runOnce = false;
    public bool turnComplete = false;

    public bool turnSetupComplete = false;
    public bool turnSetupInProgress = false;
    public bool turnInProgress = false;

    private void Awake()
    {
        foreach (GameObject unit in GameObject.FindGameObjectsWithTag("Player Unit"))
        {
            allUnits.Add(unit.GetComponent<Unit>());
            playerUnits.Add(unit);
        }
        foreach (GameObject unit in GameObject.FindGameObjectsWithTag("Enemy Unit"))
        {
            allUnits.Add(unit.GetComponent<Unit>());
            enemyUnits.Add(unit);
        }
    }

    private void Update()
    {
        if (!runOnce)
        {
            runOnce = true;
            currentUnitTurn = "Enemy Unit";
        }
        if (currentUnitTurn == "Enemy Unit")
        {
            if (!turnSetupComplete && !turnSetupInProgress && !turnComplete)
            {
                turnSetupInProgress = true;
                turnUnits.Clear();
                turnUnitsDict.Clear();
                unitMovedDict.Clear();
                unitAttackedDict.Clear();
                uIController.highlightedObjects.Clear();
                uIController.selectedInfoUnit = null;
                uIController.selectedInfoMapNode = null;
                uIController.selectedMoveMapNode = null;
                uIController.selectedAttackUnit = null;
                foreach (GameObject unit in GameObject.FindGameObjectsWithTag("Enemy Unit"))
                {

                    unit.GetComponent<Unit>().CheckCurrentNode();
                    unit.GetComponent<Unit>().path.Clear();
                    unit.GetComponent<Dijkstra>().path.Clear();
                    turnUnits.Add(unit.GetComponent<Unit>());
                    turnUnitsDict.Add(unit.GetComponent<Unit>(), false);
                    unitMovedDict.Add(unit.GetComponent<Unit>(), false);
                    unitAttackedDict.Add(unit.GetComponent<Unit>(), false);
                    //unit.GetComponent<Unit>().CurrentHP += 2;
                    unit.GetComponent<Unit>().CurrentStamina = unit.GetComponent<Unit>().MaxStamina;
                }
                turnSetupComplete = true;
            }
            if (turnSetupComplete && !turnInProgress && !turnComplete)
            {
                turnInProgress = true;
                while (!turnComplete)
                {
                    foreach (Unit unit in turnUnits)
                    {
                        unit.CheckCurrentNode();
                        Debug.Log("NodeCostDict Check at Update: " + unit.NodeCostDict.Keys.Count);
                        unit.CheckCanMove(unit.NodeCostDict);
                        /*while (unit.transform.position.x != unit.path.Last().transform.position.x && unit.transform.position.y != unit.path.Last().transform.position.y)
                        {
                            unit.Move();
                        }*/
                        unit.gameObject.transform.position = unit.path.Last().transform.position;
                        unit.currentMapNode.isOccupied = false;
                        unit.currentMapNode.occupyingObject = null;
                        unit.CheckCurrentNode();
                        unit.Attack(unit.AttackChoice(), unit.WeaponDamage);
                    }
                    turnComplete = true;
                }
            }
            if (turnComplete)
            {
                turnSetupComplete = false;
                turnSetupInProgress = false;
                turnInProgress = false;
                currentUnitTurn = "Player Unit";
                turnComplete = false;
            }
        }
        if (currentUnitTurn == "Player Unit")
        {
            if (!turnSetupComplete && !turnSetupInProgress && !turnComplete)
            {
                turnSetupInProgress = true;
                turnUnits.Clear();
                turnUnitsDict.Clear();
                unitMovedDict.Clear();
                unitAttackedDict.Clear();
                uIController.highlightedObjects.Clear();
                uIController.selectedInfoUnit = null;
                uIController.selectedInfoMapNode = null;
                uIController.selectedMoveMapNode = null;
                uIController.selectedAttackUnit = null;
                foreach (GameObject unit in GameObject.FindGameObjectsWithTag("Player Unit"))
                {
                    if (unit.transform.position != unit.GetComponent<Unit>().currentMapNode.transform.position)
                    {
                        unit.GetComponent<Unit>().CheckCurrentNode();
                    }
                    
                    unit.GetComponent<Unit>().path.Clear();
                    unit.GetComponent<Dijkstra>().path.Clear();
                    turnUnits.Add(unit.GetComponent<Unit>());
                    turnUnitsDict.Add(unit.GetComponent<Unit>(), false);
                    unitMovedDict.Add(unit.GetComponent<Unit>(), false);
                    unitAttackedDict.Add(unit.GetComponent<Unit>(), false);
                    //unit.GetComponent<Unit>().CurrentHP += 2;
                    unit.GetComponent<Unit>().CurrentStamina = unit.GetComponent<Unit>().MaxStamina;
                }
                uIController.playerCanvas.SetActive(true);
                uIController.unitPanel.SetActive(true);
                uIController.endTurnButton.SetActive(true);
                turnSetupComplete = true;
                
            }
            if (turnSetupComplete && !turnComplete)
            {
                uIController.playerTurn = true;
            }
            if (turnComplete)
            {
                turnSetupComplete = false;
                turnSetupInProgress = false;
                turnInProgress = false;
                uIController.playerCanvas.SetActive(false);
                currentUnitTurn = "Enemy Unit";
                turnComplete = false;
            }
        }
    }

    /*public bool StartTurn(string unitTurn)
    {
        turnNumber++;
        turnUnits.Clear();
        turnUnitsDict.Clear();
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
            uIController.playerCanvas.SetActive(true);
            uIController.endTurnButton.SetActive(true);
            uIController.unitPanel.SetActive(true);
        }
    }*/

    public bool EndTurn(string unitTurn)
    {
        foreach (Unit unit in turnUnits)
        {
            //CHANGES FOR ALL UNITS WHO HAVE JUST TAKEN THEIR TURN
        }
        if (unitTurn == "Player Unit")
        {
            turnComplete = true;
            uIController.playerTurn = false;
            uIController.playerCanvas.SetActive(false); 
            //TURN OFF UI AND OTHER UNWANTED INTERACTIONS
            unitTurn = "Enemy Unit";
        }
        else
        {
            //MAKE SURE THE AI IS NOT ACTIVE
            unitTurn = "Player Unit";
            uIController.playerCanvas.SetActive(true);
        }
        //StartTurn(unitTurn);
        return true;
    }
}
