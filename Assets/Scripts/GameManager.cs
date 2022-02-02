using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public List<Unit> allUnits = new List<Unit>();
    public Dictionary<Unit, bool> turnUnits = new Dictionary<Unit, bool>();
    public string currentUnitTurn;
    public int turnNumber = 0;
    public bool startupComplete = false;
    public bool runOnce = false;

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
        foreach (GameObject unit in GameObject.FindGameObjectsWithTag(unitTurn))
        {
            unit.GetComponent<Unit>().path.Clear();
            unit.GetComponent<Dijkstra>().path.Clear();
            turnUnits.Add(unit.GetComponent<Unit>(), false);
            //unit.GetComponent<Unit>().CurrentHP += 2;
            unit.GetComponent<Unit>().CurrentStamina = unit.GetComponent<Unit>().MaxStamina;
        }
        TakeTurn(unitTurn);
        return true;
    }

    public void TakeTurn(string unitTurn)
    {
        if (unitTurn == "Enemy Unit")
        {
            foreach (Unit unit in turnUnits.Keys)
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
            int turnTaken = 0;
            while (turnTaken < GameObject.FindGameObjectsWithTag("Player Unit").Count())
            {
                turnTaken = 10;
                //TURN ON PLAYER UI.
                //CHECK HOW MANY UNITS HAVE TAKEN THEIR TURN/IF THE PLAYER HAS PRESSED END TURN.
            }
            EndTurn(unitTurn);
        }
    }

    public bool EndTurn(string unitTurn)
    {
        foreach (Unit unit in turnUnits.Keys)
        {
        }
        if (unitTurn == "Player Unit")
        {
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
