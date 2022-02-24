using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LevelSave
{
    public int levelCode;

    public List<Unit> allUnits = new List<Unit>();

    public List<Unit> turnUnits = new List<Unit>();

    public Dictionary<Unit, bool> turnUnitsDict = new Dictionary<Unit, bool>();

    public Dictionary<Unit, bool> unitMovedDict = new Dictionary<Unit, bool>();

    public Dictionary<Unit, bool> unitAttackedDict = new Dictionary<Unit, bool>();

    public string currentUnitTurn;

    public int turnNumber;

    public Dictionary<GameObject, List<float>> playerUnitData = new Dictionary<GameObject, List<float>>();

    public Dictionary<GameObject, List<float>> enemyUnitData = new Dictionary<GameObject, List<float>>();
}
