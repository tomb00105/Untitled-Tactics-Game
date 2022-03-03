using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveBehaviour : MonoBehaviour
{
    GameManager gameManager;
    Dictionary<int, Scene> levelCodeDict = new Dictionary<int, Scene>();

    public void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        levelCodeDict.Add(1, SceneManager.GetSceneByName("Test Level"));
    }
    private LevelSave CreateSaveGameObject()
    {
        GameManager gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        LevelSave save = new LevelSave();
        
        save.levelCode = gameManager.levelCode;
        foreach (Unit unit in gameManager.allUnits)
        {
            save.allUnits.Add(unit.gameObject.name);
        }
        foreach (Unit unit in gameManager.turnUnits)
        {
            save.turnUnits.Add(unit.gameObject.name);
        }
        foreach (KeyValuePair<Unit, bool> unit in gameManager.turnUnitsDict)
        {
            save.turnUnitsDict.Add(unit.Key.gameObject.name, unit.Value);
        }
        foreach (KeyValuePair<Unit, bool> unit in gameManager.unitMovedDict)
        {
            save.unitMovedDict.Add(unit.Key.gameObject.name, unit.Value);
        }
        foreach (KeyValuePair<Unit, bool> unit in gameManager.unitAttackedDict)
        {
            save.unitAttackedDict.Add(unit.Key.gameObject.name, unit.Value);
        }
        save.currentUnitTurn = gameManager.currentUnitTurn;
        save.turnNumber = gameManager.turnNumber;

        foreach (GameObject playerUnit in GameObject.FindGameObjectsWithTag("Player Unit"))
        {
            Unit playerUnitScript = playerUnit.GetComponent<Unit>();
            float unitType;
            float attackorDefence;
            if (playerUnitScript.UnitType == "Swordsmen")
            {
                unitType = 0;
            }
            else if (playerUnitScript.UnitType == "Spearmen")
            {
                unitType = 1;
            }
            else if (playerUnitScript.UnitType == "Archers")
            {
                unitType = 2;
            }
            else
            {
                unitType = 3;
            }
            if (playerUnitScript.AttackOrDefence)
            {
                attackorDefence = 0;
            }
            else
            {
                attackorDefence = 1;
            }
            save.playerUnitData.Add(playerUnitScript.unitName, new List<float> 
            { 
                unitType, 
                playerUnitScript.MaxHP, playerUnitScript.CurrentHP, 
                playerUnitScript.MaxStamina, playerUnitScript.CurrentStamina,
                playerUnitScript.WeaponDamage, playerUnitScript.WeaponRange,
                attackorDefence,
                playerUnitScript.GrassCost, playerUnitScript.AridCost,
                playerUnitScript.IceCost, playerUnitScript.MountainCost,
                playerUnitScript.RiverCost, playerUnitScript.OceanCost,
                playerUnit.gameObject.transform.position.x, playerUnit.gameObject.transform.position.y
                } );
            if (gameManager.unitMovedDict[playerUnitScript])
            {
                save.playerUnitData[playerUnit.name].Add(1);
            }
            else
            {
                save.playerUnitData[playerUnit.name].Add(0);
            }
            if (gameManager.unitAttackedDict[playerUnitScript])
            {
                save.playerUnitData[playerUnit.name].Add(1);
            }
            else
            {
                save.playerUnitData[playerUnit.name].Add(0);
            }
        }

        foreach (GameObject enemyUnit in GameObject.FindGameObjectsWithTag("Enemy Unit"))
        {
            Unit enemyUnitScript = enemyUnit.GetComponent<Unit>();
            float unitType;
            float attackorDefence;
            if (enemyUnitScript.UnitType == "Swordsmen")
            {
                unitType = 0;
            }
            else if (enemyUnitScript.UnitType == "Spearmen")
            {
                unitType = 1;
            }
            else if (enemyUnitScript.UnitType == "Archers")
            {
                unitType = 2;
            }
            else
            {
                unitType = 3;
            }
            if (enemyUnitScript.AttackOrDefence)
            {
                attackorDefence = 0;
            }
            else
            {
                attackorDefence = 1;
            }
            save.enemyUnitData.Add(enemyUnitScript.unitName, new List<float>
            {
                unitType,
                enemyUnitScript.MaxHP, enemyUnitScript.CurrentHP,
                enemyUnitScript.MaxStamina, enemyUnitScript.CurrentStamina,
                enemyUnitScript.WeaponDamage, enemyUnitScript.WeaponRange,
                attackorDefence,
                enemyUnitScript.GrassCost, enemyUnitScript.AridCost,
                enemyUnitScript.IceCost, enemyUnitScript.MountainCost,
                enemyUnitScript.RiverCost, enemyUnitScript.OceanCost,
                enemyUnit.gameObject.transform.position.x, enemyUnit.gameObject.transform.position.y
                });
            
        }

        return save;
    }

    public void SaveGame()
    {
        Debug.Log("Save In Progress...");
        LevelSave save = CreateSaveGameObject();

        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/levelsave.save");
        bf.Serialize(file, save);
        file.Close();

        Debug.Log("GAME SAVED!");
    }

    IEnumerator LevelLoading(LevelSave save)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Test Level Load", LoadSceneMode.Single);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        gameManager.loading = true;
        gameManager.loadComplete = false;
        gameManager.allUnits.Clear();
        gameManager.turnUnits.Clear();
        gameManager.playerUnits.Clear();
        gameManager.enemyUnits.Clear();
        gameManager.turnUnitsDict.Clear();
        gameManager.unitMovedDict.Clear();
        gameManager.unitAttackedDict.Clear();
        
        gameManager.currentUnitTurn = save.currentUnitTurn;
        if (save.currentUnitTurn == "Player Unit")
        {
            GameObject.Find("UIController").GetComponent<UIController>().playerTurn = true;
        }
        gameManager.turnNumber = save.turnNumber;
        foreach (KeyValuePair<string, List<float>> playerUnit in save.playerUnitData)
        {
            List<float> dataList = playerUnit.Value;
            Transform playerUnitTransform = transform;
            playerUnitTransform.SetPositionAndRotation(new Vector2(dataList[14], dataList[15]), Quaternion.Euler(0, 0, 0));
            GameObject playerObject;
            Unit playerUnitScript;
            
            if (dataList[0] == 0)
            {
                playerObject = Instantiate(gameManager.swordsmenPrefab, parent: GameObject.Find("Units").transform, playerUnitTransform);
                playerUnitScript = playerObject.GetComponent<Unit>();
                playerUnitScript.UnitType = "Swordsmen";
            }
            else if (dataList[0] == 1)
            {
                playerObject = Instantiate(gameManager.spearmenPrefab, parent: GameObject.Find("Units").transform, playerUnitTransform);
                playerUnitScript = playerObject.GetComponent<Unit>();
                playerUnitScript.UnitType = "Spearmen";
            }
            else if (dataList[0] == 2)
            {
                playerObject = Instantiate(gameManager.archersPrefab, parent: GameObject.Find("Units").transform, playerUnitTransform);
                playerUnitScript = playerObject.GetComponent<Unit>();
                playerUnitScript.UnitType = "Archers";
            }
            else if (dataList[0] == 3)
            {
                playerObject = Instantiate(gameManager.cavalryPrefab, parent: GameObject.Find("Units").transform, playerUnitTransform);
                playerUnitScript = playerObject.GetComponent<Unit>();
                playerUnitScript.UnitType = "Cavalry";
            }
            else
            {
                playerObject = null;
                playerUnitScript = null;
                Debug.Log("CANNOT INSTANTIATE - UNIT TYPE DOES NOT EXIST");
            }
            playerObject.name = playerUnit.Key;
            playerUnitScript.unitName = playerUnit.Key;
            foreach (GameObject mapNode in GameObject.FindGameObjectsWithTag("Terrain"))
            {
                if (playerUnitTransform.position == mapNode.transform.position)
                {
                    playerObject.transform.position = mapNode.transform.position;
                    playerUnitScript.currentMapNode = mapNode.GetComponent<MapNode>();
                    GameObject.Find("MapGraph").GetComponent<MapGraph>().tileOccupationDict[mapNode.GetComponent<MapNode>()] = playerUnitScript;
                }
            }
            playerUnitScript.MaxHP = dataList[1];
            playerUnitScript.CurrentHP = dataList[2];
            playerUnitScript.MaxStamina = dataList[3];
            playerUnitScript.CurrentStamina = dataList[4];
            playerUnitScript.WeaponDamage = dataList[5];
            playerUnitScript.WeaponRange = dataList[6];
            if (dataList[7] == 0)
            {
                playerUnitScript.AttackOrDefence = true;
            }
            else
            {
                playerUnitScript.AttackOrDefence = false;
            }
            playerUnitScript.GrassCost = dataList[8];
            playerUnitScript.AridCost = dataList[9];
            playerUnitScript.IceCost = dataList[10];
            playerUnitScript.MountainCost = dataList[11];
            playerUnitScript.RiverCost = dataList[12];
            playerUnitScript.OceanCost = dataList[13];
            if (dataList[16] == 0)
            {
                gameManager.unitMovedDict.Add(playerObject.GetComponent<Unit>(), false);
            }
            else
            {
                gameManager.unitMovedDict.Add(playerObject.GetComponent<Unit>(), true);
            }
            if (dataList[17] == 0)
            {
                gameManager.unitAttackedDict.Add(playerObject.GetComponent<Unit>(), false);
            }
            else
            {
                gameManager.unitAttackedDict.Add(playerObject.GetComponent<Unit>(), true);
            }
            gameManager.allUnits.Add(playerObject.GetComponent<Unit>());
            gameManager.playerUnits.Add(playerObject);
            playerObject.tag = "Player Unit";
        }
        foreach (KeyValuePair<string, List<float>> enemyUnit in save.enemyUnitData)
        {
            List<float> dataList = enemyUnit.Value;
            Transform enemyUnitTransform = transform;
            enemyUnitTransform.SetPositionAndRotation(new Vector2(dataList[14], dataList[15]), Quaternion.Euler(0, 0, 0));
            GameObject enemyObject;
            Unit enemyUnitScript;
            
            if (dataList[0] == 0)
            {
                enemyObject = Instantiate(gameManager.swordsmenPrefab, parent: GameObject.Find("Units").transform, enemyUnitTransform);
                enemyUnitScript = enemyObject.GetComponent<Unit>();
                enemyUnitScript.UnitType = "Swordsmen";
            }
            else if (dataList[0] == 1)
            {
                enemyObject = Instantiate(gameManager.spearmenPrefab, parent: GameObject.Find("Units").transform, enemyUnitTransform);
                enemyUnitScript = enemyObject.GetComponent<Unit>();
                enemyUnitScript.UnitType = "Spearmen";
            }
            else if (dataList[0] == 2)
            {
                enemyObject = Instantiate(gameManager.archersPrefab, parent: GameObject.Find("Units").transform, enemyUnitTransform);
                enemyUnitScript = enemyObject.GetComponent<Unit>();
                enemyUnitScript.UnitType = "Archers";
            }
            else if (dataList[0] == 3)
            {
                enemyObject = Instantiate(gameManager.cavalryPrefab, parent: GameObject.Find("Units").transform, enemyUnitTransform);
                enemyUnitScript = enemyObject.GetComponent<Unit>();
                enemyUnitScript.UnitType = "Cavalry";
            }
            else
            {
                enemyObject = null;
                enemyUnitScript = null;
                Debug.Log("CANNOT INSTANTIATE - UNIT TYPE DOES NOT EXIST");
            }
            enemyObject.name = enemyUnit.Key;
            enemyUnitScript.unitName = enemyUnit.Key;
            foreach (GameObject mapNode in GameObject.FindGameObjectsWithTag("Terrain"))
            {
                if (enemyUnitTransform.position == mapNode.transform.position)
                {
                    enemyObject.transform.position = mapNode.transform.position;
                    enemyUnitScript.currentMapNode = mapNode.GetComponent<MapNode>();
                    GameObject.Find("MapGraph").GetComponent<MapGraph>().tileOccupationDict[mapNode.GetComponent<MapNode>()] = enemyUnitScript;
                }
            }
            enemyUnitScript.MaxHP = dataList[1];
            enemyUnitScript.CurrentHP = dataList[2];
            enemyUnitScript.MaxStamina = dataList[3];
            enemyUnitScript.CurrentStamina = dataList[4];
            enemyUnitScript.WeaponDamage = dataList[5];
            enemyUnitScript.WeaponRange = dataList[6];
            if (dataList[7] == 0)
            {
                enemyUnitScript.AttackOrDefence = true;
            }
            else
            {
                enemyUnitScript.AttackOrDefence = false;
            }
            enemyUnitScript.GrassCost = dataList[8];
            enemyUnitScript.AridCost = dataList[9];
            enemyUnitScript.IceCost = dataList[10];
            enemyUnitScript.MountainCost = dataList[11];
            enemyUnitScript.RiverCost = dataList[12];
            enemyUnitScript.OceanCost = dataList[13];

            gameManager.allUnits.Add(enemyObject.GetComponent<Unit>());
            gameManager.enemyUnits.Add(enemyObject);
            enemyObject.tag = "Enemy Unit";
        }
        foreach (string unit in save.allUnits)
        {
            gameManager.allUnits.Add(GameObject.Find(unit).GetComponent<Unit>());
        }
        foreach (string unit in save.turnUnits)
        {
            gameManager.turnUnits.Add(GameObject.Find(unit).GetComponent<Unit>());
        }
        foreach (KeyValuePair<string, bool> unit in save.turnUnitsDict)
        {
            gameManager.turnUnitsDict.Add(GameObject.Find(unit.Key).GetComponent<Unit>(), unit.Value);
        }
        gameManager.loadComplete = true;
    }
    public bool LoadGame()
    {
        if (File.Exists(Application.persistentDataPath + "/levelsave.save"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/levelsave.save", FileMode.Open);
            LevelSave save = (LevelSave)bf.Deserialize(file);
            file.Close();

            Scene scene = SceneManager.GetSceneByName("Test Level");
            //CODE TO FIND AND LOAD LEVEL TO BE ADDED
            //if (levelCodeDict.ContainsKey(save.levelCode))
            //{
            StartCoroutine(LevelLoading(save));
            //}
            /*else
            {
                Debug.Log("LEVEL DOES NOT EXIST: " + save.levelCode.ToString());
                return false;
            }*/
        }
        return true;
    }
}