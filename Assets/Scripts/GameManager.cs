using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public List<Unit> allUnits = new List<Unit>();
    public Dictionary<Unit, bool> turnUnits = new Dictionary<Unit, bool>();
    public string currentUnitTurn;
    public int turnNumber;

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

    public bool StartTurn(string unitTurn)
    {
        turnNumber++;
        turnUnits.Clear();
        foreach (GameObject unit in GameObject.FindGameObjectsWithTag(unitTurn))
        {
            turnUnits.Add(unit.GetComponent<Unit>(), false);
            unit.GetComponent<Unit>().CurrentHP += 2;
            unit.GetComponent<Unit>().CurrentStamina = unit.GetComponent<Unit>().MaxStamina;
        }
        return true;
    }

    public bool EndTurn(string unitTurn)
    {
        foreach (Unit unit in turnUnits.Keys)
        {
            turnUnits[unit] = true;
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
