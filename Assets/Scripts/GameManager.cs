using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class GameManager : MonoBehaviour
{
    public GameObject swordsmenPrefab;
    public GameObject spearmenPrefab;
    public GameObject archersPrefab;
    public GameObject cavalryPrefab;
    public MapGraph mapGraph;
    //Declaration of variables and collections for GameManager.
    public UIController uIController;
    public List<Unit> allUnits = new List<Unit>();
    public List<Unit> turnUnits = new List<Unit>();
    public List<GameObject> playerUnits = new List<GameObject>();
    public List<GameObject> enemyUnits = new List<GameObject>();
    public Dictionary<Unit, bool> turnUnitsDict = new Dictionary<Unit, bool>();
    public Dictionary<Unit, bool> unitMovedDict = new Dictionary<Unit, bool>();
    public Dictionary<Unit, bool> unitAttackedDict = new Dictionary<Unit, bool>();
    public string currentUnitTurn;
    public int levelCode = 1;
    public int turnNumber = 0;
    public bool mapSetupComplete = false;
    public bool startupComplete = false;
    public bool runOnce = false;
    public bool turnComplete = false;

    public bool loading = false;
    public bool loadComplete = false;
    public bool levelStarted = false;
    public bool turnSetupComplete = false;
    public bool turnSetupInProgress = false;
    public bool turnInProgress = false;

    private void Awake()
    {
        foreach (GameObject terrainTile in GameObject.FindGameObjectsWithTag("Terrain"))
        {
            mapGraph.graphCostDict.Add(terrainTile.GetComponent<MapNode>(), terrainTile.transform.position);
            foreach (GameObject playerUnit in GameObject.FindGameObjectsWithTag("Player Unit"))
            {
                if (playerUnit.transform.position.x == terrainTile.transform.position.x && playerUnit.transform.position.y == terrainTile.transform.position.y)
                {
                    Debug.Log("Player Unit tile SETUP");
                    mapGraph.tileOccupationDict.Add(terrainTile.GetComponent<MapNode>(), playerUnit.GetComponent<Unit>());
                    playerUnit.GetComponent<Unit>().currentMapNode = terrainTile.GetComponent<MapNode>();
                    break;
                }
            }
            foreach (GameObject enemyUnit in GameObject.FindGameObjectsWithTag("Enemy Unit"))
            {
                if (enemyUnit.transform.position.x == terrainTile.transform.position.x && enemyUnit.transform.position.y == terrainTile.transform.position.y)
                {
                    Debug.Log("Enemy Unit tile SETUP");
                    mapGraph.tileOccupationDict.Add(terrainTile.GetComponent<MapNode>(), enemyUnit.GetComponent<Unit>());
                    enemyUnit.GetComponent<Unit>().currentMapNode = terrainTile.GetComponent<MapNode>();
                    break;
                }
            }
            if (!mapGraph.tileOccupationDict.Keys.Contains(terrainTile.GetComponent<MapNode>()))
            {
                Debug.Log("Tile occupation set to NULL");
                mapGraph.tileOccupationDict.Add(terrainTile.GetComponent<MapNode>(), null);
            }
        }
        GameObject.Find("GameManager").GetComponent<GameManager>().mapSetupComplete = true;
        //Populates the respective collections with player and enemy units.
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
        if (!startupComplete)
        {
            foreach (GameObject playerUnit in GameObject.FindGameObjectsWithTag("Player Unit"))
            {
                playerUnit.GetComponent<Unit>().CheckCurrentNode();
            }
            foreach (GameObject enemyUnit in GameObject.FindGameObjectsWithTag("Enemy Unit"))
            {
                enemyUnit.GetComponent<Unit>().CheckCurrentNode();
            }
            startupComplete = true;
        }
        //Starts the level.
        if (!runOnce && mapSetupComplete && startupComplete && !loading && levelStarted)
        {
            runOnce = true;
            currentUnitTurn = "Enemy Unit";
        }

        if (loading && !loadComplete)
        {
            return;
        }

        if (uIController.paused)
        {
            return;
        }

        //Enemy turn loop.
        else if (currentUnitTurn == "Enemy Unit")
        {
            //Sets up the needed variables for enemy turn.
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
                //Resets and sets up variables for each enemy unit.
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
            //Runs the actual enemy turn, unit by unit.
            if (turnSetupComplete && !turnInProgress && !turnComplete)
            {
                turnInProgress = true;
                while (!turnComplete)
                {
                    foreach (Unit unit in turnUnits)
                    {
                        //Debug.Log("NodeCostDict Check at Update: " + unit.NodeCostDict.Keys.Count);
                        unit.CheckCanMove();
                        /*while (unit.transform.position.x != unit.path.Last().transform.position.x && unit.transform.position.y != unit.path.Last().transform.position.y)
                        {
                            unit.Move();
                        }*/
                        unit.gameObject.transform.position = unit.path.Last().transform.position;
                        unit.CheckCurrentNode();
                        unit.Attack(unit.AttackChoice(), unit.WeaponDamage);
                    }
                    turnComplete = true;
                }
            }
            //Resets turn variables and sets the current turn to the player.
            if (turnComplete)
            {
                turnSetupComplete = false;
                turnSetupInProgress = false;
                turnInProgress = false;
                currentUnitTurn = "Player Unit";
                turnComplete = false;
            }
        }
        //Player turn loop.
        else if (currentUnitTurn == "Player Unit" && startupComplete)
        {
            //Sets up variables for player turn.
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
                    //var unitNode = GameObject.Find("MapGraph").GetComponent<MapGraph>().tileOccupationDict.Where(x => x.Value.Equals(unit.GetComponent<Unit>())).Select(x => x.Key);
                    //Checks unit current tile.
                    if (unit.GetComponent<Unit>().currentMapNode != null)
                    {
                        if (unit.transform.position != unit.GetComponent<Unit>().currentMapNode.transform.position)
                        {
                            unit.GetComponent<Unit>().CheckCurrentNode();
                        }
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
                //Opens UI for player turn.
                uIController.playerCanvas.SetActive(true);
                uIController.unitPanel.SetActive(true);
                uIController.endTurnButton.SetActive(true);
                turnSetupComplete = true;
                
            }
            if (turnSetupComplete && !turnComplete)
            {
                //Tells the UIController that it is the player turn and they can use the UI and raycasts.
                uIController.playerTurn = true;
            }
            //Resets turn variables and changes turn to enemy turn.
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

    //Ends the player turn by setting turn complete to true and disables player UI.
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
