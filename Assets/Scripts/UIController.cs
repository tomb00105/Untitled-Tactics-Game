using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class UIController : MonoBehaviour
{
    //References to UI elements.
    public GameObject playerCanvas;
    public GameObject endTurnButton;
    public GameObject unitPanel;
    public GameObject infoPanel;
    public GameObject terrainPanel;
    public GameObject movePanel;
    public GameObject attackPanel;
    public GameObject pauseMenuPanel;
    public GameObject exitMenuPanel;
    public GameObject exitToMainMenuPanel;
    public GameObject exitToDesktopPanel;

    //Initialisation of variables.
    public GameManager gameManager;
    private MapGraph mapGraph;
    public List<GameObject> highlightedObjects = new List<GameObject>();
    public Unit selectedInfoUnit;
    public MapNode selectedInfoMapNode;
    public MapNode selectedMoveMapNode;
    public Unit selectedAttackUnit;
    public bool playerTurn = false;
    public bool paused = false;

    private void Awake()
    {
        mapGraph = GameObject.Find("MapGraph").GetComponent<MapGraph>();
    }
    private void Update()
    {
        //Makes sure raycasts are not usable if not the player turn.
        if (!playerTurn)
        {
            return;
        }
        if (pauseMenuPanel.activeInHierarchy || exitMenuPanel.activeInHierarchy || exitToMainMenuPanel.activeInHierarchy || exitToDesktopPanel.activeInHierarchy)
        {
            paused = true;
            return;
        }
        else
        {
            paused = false;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!pauseMenuPanel.activeInHierarchy)
            {
                pauseMenuPanel.SetActive(true);
                endTurnButton.SetActive(false);
            }
        }

        //Allows player to select units via raycast.
        if (unitPanel.activeInHierarchy || terrainPanel.activeInHierarchy)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector2 rayPos = new Vector2(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
                RaycastHit2D[] hits = Physics2D.RaycastAll(rayPos, Vector2.zero);
                if (hits.Count() == 0)
                {
                    Debug.Log("Raycast hit nothing!");
                    /*if (hit.collider.gameObject.CompareTag("Terrain"))
                    {
                        selectedInfoMapNode = hit.collider.gameObject.GetComponent<MapNode>();
                        PopulateTerrainPanel(selectedInfoMapNode);
                        UnitPanelsDefault();
                        unitPanel.SetActive(false);
                        terrainPanel.SetActive(true);
                    }*/
                }
                else
                {
                    foreach (RaycastHit2D hit in hits)
                    {
                        if (hit.transform.gameObject.CompareTag("Player Unit") || hit.transform.gameObject.CompareTag("Enemy Unit"))
                        {
                            Debug.Log("Raycast hit: " + hit.transform.gameObject.name.ToString());
                            selectedInfoUnit = hit.transform.gameObject.GetComponent<Unit>();
                            PopulateUnitPanels(selectedInfoUnit);
                            /*TerrainPanelDefault();
                            terrainPanel.SetActive(false);
                            unitPanel.SetActive(true);*/
                        }
                        else
                        {
                            Debug.Log("Raycast hit: " + hit.transform.gameObject.name.ToString());
                        }
                    }
                    
                }
            }
        }
        //Allows player to select a space to move to via raycast.
        if (movePanel.activeInHierarchy)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector2 rayPos = new Vector2(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
                RaycastHit2D[] hits = Physics2D.RaycastAll(rayPos, Vector2.zero);
                if (hits.Count() != 0)
                {
                    foreach (RaycastHit2D hit in hits)
                    {
                        if (highlightedObjects.Contains(hit.collider.gameObject) && hit.collider.gameObject.CompareTag("Terrain"))
                        {
                            Debug.Log("Move Raycast hit: " + hit.transform.gameObject.name.ToString());
                            selectedMoveMapNode = hit.collider.gameObject.GetComponent<MapNode>();
                            PopulateMovePanel(selectedMoveMapNode, selectedInfoUnit);
                        }
                        else
                        {
                            Debug.Log("Move Raycast hit: " + hit.transform.gameObject.name.ToString());
                        }
                    }
                    
                }
                else
                {
                    Debug.Log("Not a valid space to move to!");
                }
            }
        }
        //Allows player to select an enemy to attack via raycast.
        if (attackPanel.activeInHierarchy)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector2 rayPos = new Vector2(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
                RaycastHit2D[] hits = Physics2D.RaycastAll(rayPos, Vector2.zero);
                foreach (RaycastHit2D hit in hits)
                {
                    if (hits.Count() != 0)
                    {
                        if (highlightedObjects.Contains(hit.collider.gameObject) && hit.collider.gameObject.CompareTag("Enemy Unit"))
                        {
                            Debug.Log("Attack Raycast hit: " + hit.transform.gameObject.name.ToString());
                            selectedAttackUnit = hit.collider.gameObject.GetComponent<Unit>();
                            PopulateAttackPanel(selectedAttackUnit, selectedInfoUnit);
                        }
                        else
                        {
                            Debug.Log("Attack Raycast hit: " + hit.transform.gameObject.name.ToString());
                        }
                    }
                    else
                    {
                        Debug.Log("Not a valid target!");
                    }
                }
            }
        }
    }
    //End Turn Button function.
    public void EndTurnButton()
    {
        UnitPanelsDefault();
        MovePanelDefault();
        AttackPanelDefault();
        RemoveHighlight();
        unitPanel.transform.Find("Unit Panel Info Button").gameObject.SetActive(false);
        unitPanel.transform.Find("Unit Panel Move Button").gameObject.SetActive(false);
        unitPanel.transform.Find("Unit Panel Attack Button").gameObject.SetActive(false);
        unitPanel.SetActive(false);
        infoPanel.SetActive(false);
        movePanel.SetActive(false);
        attackPanel.SetActive(false);
        gameManager.EndTurn("Player Unit");
    }

    /*public void TurnOnUI(List<string> uIElements)
    {
        foreach (string uIElement in uIElements)
        {
            GameObject.Find(uIElement).SetActive(true);
        }
    }
    public void TurnOffUI(List<string> uIElements)
    {
        foreach (string uIElement in uIElements)
        {
            GameObject.Find(uIElement).SetActive(false);
        }
    }*/ //Deprecated as not funcitonal.

    //Highlights the tiles which a player unit can move to.
    public void MoveHighlight(Unit unit)
    {
        highlightedObjects.Clear();
        unit.GetComponent<Dijkstra>().DijkstraCalc();
        foreach (MapNode terrainTile in unit.GetComponent<Dijkstra>().PossibleMoves())
        {
            terrainTile.GetComponent<SpriteRenderer>().color = Color.blue;
            highlightedObjects.Add(terrainTile.gameObject);

        }
    }

    //Highlights the tiles and enemy sprites of enemies the player unit can attack.
    public void AttackHighlight(Unit unit)
    {
        highlightedObjects.Clear();
        foreach (GameObject enemyUnit in GameObject.FindGameObjectsWithTag("Enemy Unit"))
        {
            float distance = Mathf.Abs(unit.transform.position.x - enemyUnit.transform.position.x) + Mathf.Abs(unit.transform.position.y - enemyUnit.transform.position.y);
            Debug.Log("Distance to enemy: " + distance.ToString());
            Debug.Log("Weapon range: " + unit.WeaponRange.ToString());
            if (distance <= unit.WeaponRange)
            {
                enemyUnit.GetComponent<Unit>().currentMapNode.GetComponent<SpriteRenderer>().color = Color.red;
                enemyUnit.GetComponent<SpriteRenderer>().color = Color.green;
                highlightedObjects.Add(enemyUnit.gameObject);
                highlightedObjects.Add(enemyUnit.GetComponent<Unit>().currentMapNode.gameObject);
            }
        }
    }

    //Removes any highlights and takes the objects out of the list of highlighted objects.
    public void RemoveHighlight()
    {
        foreach (GameObject terrainTile in highlightedObjects)
        {
            terrainTile.GetComponent<SpriteRenderer>().color = Color.white;
        }
        highlightedObjects.Clear();
    }

    //Populates the unit and unit info UI elements with the information of the selected unit.
    public void PopulateUnitPanels(Unit unit)
    {
        unitPanel.transform.Find("Unit Panel Name").GetComponent<TextMeshProUGUI>().text = "Name: " + unit.UnitName;
        unitPanel.transform.Find("Unit Panel Type").GetComponent<TextMeshProUGUI>().text = "Type: " + unit.UnitType;
        unitPanel.transform.Find("Unit Panel HP").GetComponent<TextMeshProUGUI>().text = "HP: " + unit.CurrentHP.ToString();
        unitPanel.transform.Find("Unit Panel Stamina").GetComponent<TextMeshProUGUI>().text = "Stamina: " + unit.CurrentStamina.ToString();

        infoPanel.transform.Find("Info Panel Name").GetComponent<TextMeshProUGUI>().text = "Name: " + unit.UnitName;
        infoPanel.transform.Find("Info Panel Type").GetComponent<TextMeshProUGUI>().text = "Type: " + unit.UnitType;
        infoPanel.transform.Find("Info Panel HP").GetComponent<TextMeshProUGUI>().text = "HP: " + unit.CurrentHP.ToString();
        infoPanel.transform.Find("Info Panel Stamina").GetComponent<TextMeshProUGUI>().text = "Stamina: " + unit.CurrentStamina.ToString();
        infoPanel.transform.Find("Info Panel Attack").GetComponent<TextMeshProUGUI>().text = "Attack: " + unit.WeaponDamage.ToString();
        if (unit.Advantages != null)
        {
            infoPanel.transform.Find("Info Panel Advantage").GetComponent<TextMeshProUGUI>().text = "Adv: " + string.Join(", ", unit.Advantages);
        }
        if (unit.Disadvantages != null)
        {
            infoPanel.transform.Find("Info Panel Disadvantage").GetComponent<TextMeshProUGUI>().text = "Disadv: " + string.Join(", ", unit.Disadvantages);

        }

        unitPanel.transform.Find("Unit Panel Info Button").gameObject.SetActive(true);

        if (unit.CompareTag("Enemy Unit"))
        {
            unitPanel.transform.Find("Unit Panel Move Button").gameObject.SetActive(false);
            unitPanel.transform.Find("Unit Panel Attack Button").gameObject.SetActive(false);
        }
        else
        {
            if (!gameManager.unitMovedDict[unit])
            {
                unitPanel.transform.Find("Unit Panel Move Button").gameObject.SetActive(true);
            }
            else
            {
                unitPanel.transform.Find("Unit Panel Move Button").gameObject.SetActive(false);
            }
            if (!gameManager.unitAttackedDict[unit])
            {
                unitPanel.transform.Find("Unit Panel Attack Button").gameObject.SetActive(true);
            }
            else
            {
                unitPanel.transform.Find("Unit Panel Attack Button").gameObject.SetActive(false);
            }
        }
    }

    //Sets the unit and unit info UI elements to their default values.
    public void UnitPanelsDefault()
    {
        unitPanel.transform.Find("Unit Panel Name").GetComponent<TextMeshProUGUI>().text = "Name:";
        unitPanel.transform.Find("Unit Panel Type").GetComponent<TextMeshProUGUI>().text = "Type: ";
        unitPanel.transform.Find("Unit Panel HP").GetComponent<TextMeshProUGUI>().text = "HP:";
        unitPanel.transform.Find("Unit Panel Stamina").GetComponent<TextMeshProUGUI>().text = "Stamina: ";

        unitPanel.transform.Find("Unit Panel Info Button").gameObject.SetActive(false);
        unitPanel.transform.Find("Unit Panel Move Button").gameObject.SetActive(false);
        unitPanel.transform.Find("Unit Panel Attack Button").gameObject.SetActive(false);

        infoPanel.transform.Find("Info Panel Name").GetComponent<TextMeshProUGUI>().text = "Name:";
        infoPanel.transform.Find("Info Panel Type").GetComponent<TextMeshProUGUI>().text = "Type:";
        infoPanel.transform.Find("Info Panel HP").GetComponent<TextMeshProUGUI>().text = "HP:";
        infoPanel.transform.Find("Info Panel Stamina").GetComponent<TextMeshProUGUI>().text = "Stamina:";
        infoPanel.transform.Find("Info Panel Attack").GetComponent<TextMeshProUGUI>().text = "Attack:";
        infoPanel.transform.Find("Info Panel Advantage").GetComponent<TextMeshProUGUI>().text = "Adv:";
        infoPanel.transform.Find("Info Panel Disadvantage").GetComponent<TextMeshProUGUI>().text = "Disadv:";
    }

    //Functionality for unit and unit UI panel buttons.
    public void UnitPanelInfoButton()
    {
        unitPanel.SetActive(false);
        infoPanel.SetActive(true);
    }

    public void UnitPanelMoveButton()
    {
        MoveHighlight(selectedInfoUnit);
        unitPanel.SetActive(false);
        movePanel.SetActive(true);
    }

    public void UnitPanelAttackButton()
    {
        AttackHighlight(selectedInfoUnit);
        unitPanel.SetActive(false);
        attackPanel.SetActive(true);
    }

    public void InfoPanelBackButton()
    {
        infoPanel.SetActive(false);
        unitPanel.SetActive(true);
    }

    //Populates the terrain panel with the information of the selected tile. NOT CURRENTLY IN USE.
    public void PopulateTerrainPanel(MapNode terrainTile)
    {
        terrainPanel.transform.Find("Terrain Panel Type").GetComponent<TextMeshProUGUI>().text = "Type: " + terrainTile.terrainType;
        if (mapGraph.tileOccupationDict[terrainTile] != null)
        {
            terrainPanel.transform.Find("Terrain Panel Unit").GetComponent<TextMeshProUGUI>().text = "Unit: " + mapGraph.tileOccupationDict[terrainTile].UnitType;
        }
        else
        {
            terrainPanel.transform.Find("Terrain Panel Unit").GetComponent<TextMeshProUGUI>().text = "Unit: None";
        }
    }
    //Sets the terrain panel fields to their default values. NOT CURRENTLY IN USE.
    public void TerrainPanelDefault()
    {
        terrainPanel.transform.Find("Terrain Panel Type").GetComponent<TextMeshProUGUI>().text = "Type:";
        terrainPanel.transform.Find("Terrain Panel Unit").GetComponent<TextMeshProUGUI>().text = "Unit:";
    }
    
    //Populates the move panel UI element with the information of the selected tile and the selected unit.
    public void PopulateMovePanel(MapNode terrainTile, Unit unit)
    {
        movePanel.transform.Find("Move Panel Terrain").GetComponent<TextMeshProUGUI>().text = "Terrain: " + terrainTile.terrainType;
        movePanel.transform.Find("Move Panel Stamina").GetComponent<TextMeshProUGUI>().text = "Stamina: " + unit.CurrentStamina.ToString();
        movePanel.transform.Find("Move Panel Cost").GetComponent<TextMeshProUGUI>().text = "Cost: " + unit.GetComponent<Dijkstra>().dijkstraDict[terrainTile].ToString();
    }

    //Sets move panel UI elements values to default.
    public void MovePanelDefault()
    {
        movePanel.transform.Find("Move Panel Terrain").GetComponent<TextMeshProUGUI>().text = "Terrain:";
        movePanel.transform.Find("Move Panel Stamina").GetComponent<TextMeshProUGUI>().text = "Stamina:";
        movePanel.transform.Find("Move Panel Cost").GetComponent<TextMeshProUGUI>().text = "Cost:";
    }

    //Functionality for Move panel buttons
    public void MovePanelMoveButton()
    {
        if (selectedMoveMapNode == null)
        {
            Debug.Log("No MapNode chosen!");
            return;
        }
        else
        {
            selectedInfoUnit.path = selectedInfoUnit.GetComponent<Dijkstra>().BuildPath(selectedInfoUnit.currentMapNode, selectedMoveMapNode);
            selectedInfoUnit.Move();
            RemoveHighlight();
            gameManager.unitMovedDict[selectedInfoUnit] = true;
            PopulateUnitPanels(selectedInfoUnit);
            movePanel.SetActive(false);
            MovePanelDefault();
            unitPanel.SetActive(true);
            selectedMoveMapNode = null;
        }
    }
    
    public void MovePanelBackButton()
    {
        RemoveHighlight();
        movePanel.SetActive(false);
        MovePanelDefault();
        selectedMoveMapNode = null;
        unitPanel.SetActive(true);
    }

    //Populates attack panel UI element with information of selected unit.
    public void PopulateAttackPanel(Unit target, Unit unit)
    {
        attackPanel.transform.Find("Attack Panel Target").GetComponent<TextMeshProUGUI>().text = "Target: " + target.UnitName;
        attackPanel.transform.Find("Attack Panel Target HP").GetComponent<TextMeshProUGUI>().text = "Target HP: " + target.CurrentHP.ToString();
        attackPanel.transform.Find("Attack Panel Base Damage").GetComponent<TextMeshProUGUI>().text = "Base Damage: " + unit.WeaponDamage.ToString();
    }
    //Sets attack panel UI element variables to default.
    public void AttackPanelDefault()
    {
        attackPanel.transform.Find("Attack Panel Target").GetComponent<TextMeshProUGUI>().text = "Target:";
        attackPanel.transform.Find("Attack Panel Target HP").GetComponent<TextMeshProUGUI>().text = "Target HP:";
        attackPanel.transform.Find("Attack Panel Base Damage").GetComponent<TextMeshProUGUI>().text = "Base Damage:";
    }

    //Functionality for attack panel buttons.
    public void AttackPanelAttackButton()
    {
        if (selectedAttackUnit == null)
        {
            Debug.Log("No target selected!");
        }
        else
        {
            selectedInfoUnit.Attack(selectedAttackUnit, selectedInfoUnit.WeaponDamage);
            gameManager.unitAttackedDict[selectedInfoUnit] = true;
            //Use if you want swordsmen and swordsmen to not be able to move after attacking.
            /*if (selectedInfoUnit.UnitType == "Swordsmen" || selectedInfoUnit.UnitType == "Spearmen")
            {
                gameManager.unitMovedDict[selectedInfoUnit] = true;
            }*/
            if (selectedInfoUnit.CurrentHP > 0)
            {
                PopulateUnitPanels(selectedInfoUnit);
            }
            else if (selectedInfoUnit.CurrentHP <= 0)
            {
                UnitPanelsDefault();
            }
            attackPanel.SetActive(false);
            AttackPanelDefault();
            unitPanel.SetActive(true);
            selectedAttackUnit = null;
            RemoveHighlight();
        }
    }

    public void AttackPanelBackButton()
    {
        if (highlightedObjects.Count != 0)
        {
            RemoveHighlight();
        }
        attackPanel.SetActive(false);
        AttackPanelDefault();
        selectedAttackUnit = null;
        unitPanel.SetActive(true);
    }

    public void PauseMenuResumeButton()
    {
        pauseMenuPanel.SetActive(false);
        endTurnButton.SetActive(true);
    }

    public void PauseMenuSettingsButton()
    {
        Debug.Log("NOT IMPLEMENTED");
    }

    public void PauseMenuExitButton()
    {
        exitMenuPanel.SetActive(true);
        pauseMenuPanel.SetActive(false);
    }

    public void ExitMenuToMainMenuButton()
    {
        exitToMainMenuPanel.SetActive(true);
        exitMenuPanel.SetActive(false);
    }

    

    public void ExitMenuToDesktopButton()
    {
        exitToDesktopPanel.SetActive(true);
        exitMenuPanel.SetActive(false);
    }

    public void ExitMenuBackButton()
    {
        pauseMenuPanel.SetActive(true);
        exitMenuPanel.SetActive(false);
    }

    public void ExitToMainMenuAcceptButton()
    {
        StartCoroutine(LoadMainMenu());
    }

    IEnumerator LoadMainMenu()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Main Menu", LoadSceneMode.Single);

        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }

    public void ExitToMainMenuBackButton()
    {
        exitMenuPanel.SetActive(true);
        exitToMainMenuPanel.SetActive(false);
    }

    public void ExitToDesktopAcceptButton()
    {
        Application.Quit();
    }

    public void ExitToDesktopBackButton()
    {
        exitMenuPanel.SetActive(true);
        exitToDesktopPanel.SetActive(false);
    }

}