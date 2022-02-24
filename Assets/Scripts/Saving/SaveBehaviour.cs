using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveBehaviour : MonoBehaviour
{
    Dictionary<int, Scene> levelCodeDict = new Dictionary<int, Scene>();

    public void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }
    private LevelSave CreateSaveGameObject()
    {
        GameManager gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        LevelSave save = new LevelSave();
        
        save.levelCode = gameManager.levelCode;
        save.allUnits = gameManager.allUnits;
        save.turnUnits = gameManager.turnUnits;
        save.turnUnitsDict = gameManager.turnUnitsDict;
        save.unitMovedDict = gameManager.unitMovedDict;
        save.unitAttackedDict = gameManager.unitAttackedDict;
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
            save.playerUnitData.Add(playerUnit, new List<float> 
            { 
                unitType, 
                playerUnitScript.MaxHP, playerUnitScript.CurrentHP, 
                playerUnitScript.MaxStamina, playerUnitScript.CurrentStamina,
                playerUnitScript.WeaponDamage, playerUnitScript.WeaponRange,
                attackorDefence,
                playerUnitScript.GrassCost, playerUnitScript.AridCost,
                playerUnitScript.IceCost, playerUnitScript.MountainCost,
                playerUnitScript.RiverCost, playerUnitScript.OceanCost,
                playerUnit.transform.position.x, playerUnit.transform.position.y
                } );
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
            save.enemyUnitData.Add(enemyUnit, new List<float>
            {
                unitType,
                enemyUnitScript.MaxHP, enemyUnitScript.CurrentHP,
                enemyUnitScript.MaxStamina, enemyUnitScript.CurrentStamina,
                enemyUnitScript.WeaponDamage, enemyUnitScript.WeaponRange,
                attackorDefence,
                enemyUnitScript.GrassCost, enemyUnitScript.AridCost,
                enemyUnitScript.IceCost, enemyUnitScript.MountainCost,
                enemyUnitScript.RiverCost, enemyUnitScript.OceanCost,
                enemyUnit.transform.position.x, enemyUnit.transform.position.y
                });
        }

        return save;
    }

    public void SaveGame()
    {
        LevelSave save = CreateSaveGameObject();

        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/levelsave.save");
        bf.Serialize(file, save);
        file.Close();

        Debug.Log("GAME SAVED!");
    }

    public bool LoadGame()
    {
        if (File.Exists(Application.persistentDataPath + "/levelsave.save"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/levelsave.save", FileMode.Open);
            LevelSave save = (LevelSave)bf.Deserialize(file);
            file.Close();

            Scene scene = levelCodeDict[save.levelCode];
            //CODE TO FIND AND LOAD LEVEL TO BE ADDED
            if (levelCodeDict.ContainsKey(save.levelCode))
            {
                SceneManager.LoadSceneAsync(scene.buildIndex, LoadSceneMode.Single);
            }
            else
            {
                Debug.Log("LEVEL DOES NOT EXIST: " + save.levelCode.ToString());
                return false;
            }

            GameManager gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
            gameManager.loading = true;
            gameManager.loadComplete = false;

            gameManager.allUnits = save.allUnits;
            gameManager.turnUnits = save.turnUnits;
            gameManager.turnUnitsDict = save.turnUnitsDict;
            gameManager.unitMovedDict = save.unitMovedDict;
            gameManager.unitAttackedDict = save.unitAttackedDict;
            gameManager.currentUnitTurn = save.currentUnitTurn;
            if (save.currentUnitTurn == "Player Unit")
            {
                GameObject.Find("UIController").GetComponent<UIController>().playerTurn = true;
            }
            gameManager.turnNumber = save.turnNumber;

            foreach (KeyValuePair<GameObject, List<float>> playerUnit in save.playerUnitData)
            {
                Transform playerUnitTransform = transform;
                playerUnitTransform.SetPositionAndRotation(new Vector3(playerUnit.Value[14], playerUnit.Value[15], 0), Quaternion.identity);
                GameObject playerObject;
                Unit playerUnitScript;
                List<float> dataList = playerUnit.Value;
                if (dataList[0] == 0)
                {
                    playerObject = Instantiate(gameManager.swordsmenPrefab, playerUnitTransform);
                    playerUnitScript = playerObject.GetComponent<Unit>();
                    playerUnitScript.UnitType = "Swordsmen";
                }
                else if (dataList[0] == 1)
                {
                    playerObject = Instantiate(gameManager.spearmenPrefab, playerUnitTransform);
                    playerUnitScript = playerObject.GetComponent<Unit>();
                    playerUnitScript.UnitType = "Spearmen";
                }
                else if (dataList[0] == 2)
                {
                    playerObject = Instantiate(gameManager.archersPrefab, playerUnitTransform);
                    playerUnitScript = playerObject.GetComponent<Unit>();
                    playerUnitScript.UnitType = "Archers";
                }
                else
                {
                    playerObject = Instantiate(gameManager.cavalryPrefab, playerUnitTransform);
                    playerUnitScript = playerObject.GetComponent<Unit>();
                    playerUnitScript.UnitType = "Cavalry";
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

                playerObject.tag = "Player Unit";
            }
            foreach (KeyValuePair<GameObject, List<float>> enemyUnit in save.enemyUnitData)
            {
                Transform enemyUnitTransform = transform;
                enemyUnitTransform.SetPositionAndRotation(new Vector3(enemyUnit.Value[14], enemyUnit.Value[15], 0), Quaternion.identity);
                GameObject enemyObject;
                Unit enemyUnitScript;
                List<float> dataList = enemyUnit.Value;
                if (dataList[0] == 0)
                {
                    enemyObject = Instantiate(gameManager.swordsmenPrefab, enemyUnitTransform);
                    enemyUnitScript = enemyObject.GetComponent<Unit>();
                    enemyUnitScript.UnitType = "Swordsmen";
                }
                else if (dataList[0] == 1)
                {
                    enemyObject = Instantiate(gameManager.spearmenPrefab, enemyUnitTransform);
                    enemyUnitScript = enemyObject.GetComponent<Unit>();
                    enemyUnitScript.UnitType = "Spearmen";
                }
                else if (dataList[0] == 2)
                {
                    enemyObject = Instantiate(gameManager.archersPrefab, enemyUnitTransform);
                    enemyUnitScript = enemyObject.GetComponent<Unit>();
                    enemyUnitScript.UnitType = "Archers";
                }
                else
                {
                    enemyObject = Instantiate(gameManager.cavalryPrefab, enemyUnitTransform);
                    enemyUnitScript = enemyObject.GetComponent<Unit>();
                    enemyUnitScript.UnitType = "Cavalry";
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

                enemyObject.tag = "Enemy Unit";
            }
            gameManager.loadComplete = true;
        }
        return true;
    }
}