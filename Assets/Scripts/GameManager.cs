using System.Collections.Generic;
using UnityEngine;

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
                while (unit.transform.position.x != unit.path[unit.path.Count - 1].transform.position.x && unit.transform.position.y != unit.path[unit.path.Count - 1].transform.position.y)
                {
                    unit.Move();
                }
                unit.Attack(unit.AttackChoice(), unit.WeaponDamage);
                if (unit.CurrentHP > 0)
                {
                }
                else
                {
                    Destroy(unit.gameObject);
                }
            }
            EndTurn(unitTurn);
        }
        else if (unitTurn == "Player Unit")
        {

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
