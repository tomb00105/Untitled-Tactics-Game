using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LevelSave
{
    public int levelCode;

    public List<string> allUnits = new List<string>();

    public List<string> turnUnits = new List<string>();

    public Dictionary<string, bool> turnUnitsDict = new Dictionary<string, bool>();

    public Dictionary<string, bool> unitMovedDict = new Dictionary<string, bool>();

    public Dictionary<string, bool> unitAttackedDict = new Dictionary<string, bool>();

    public string currentUnitTurn;

    public int turnNumber;

    public Dictionary<string, List<float>> playerUnitData = new Dictionary<string, List<float>>();

    public Dictionary<string, List<float>> enemyUnitData = new Dictionary<string, List<float>>();
}
