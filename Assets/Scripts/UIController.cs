using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIController : MonoBehaviour
{
    public GameObject endTurnButton;
    public GameObject unitPanel;
    public GameObject infoPanel;
    public GameObject terrainPanel;
    public GameObject movePanel;
    public GameObject attackPanel;

    public GameManager gameManager;
    public List<GameObject> highlightedObjects = new List<GameObject>();
    public Unit selectedInfoUnit;
    public MapNode selectedInfoMapNode;
    public MapNode selectedMoveMapNode;
    public Unit selectedAttackUnit;
    public bool playerTurn = false;

    private void Update()
    {
        if (!playerTurn)
        {
            return;
        }

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

        if (movePanel.activeInHierarchy)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);
                RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);
                if (hit.collider != null)
                {
                    if (highlightedObjects.Contains(hit.collider.gameObject) && hit.collider.gameObject.CompareTag("Terrain"))
                    {
                        selectedMoveMapNode = hit.collider.gameObject.GetComponent<MapNode>();
                        PopulateMovePanel(selectedMoveMapNode, selectedInfoUnit);
                    }
                }
                else
                {
                    Debug.Log("Not a valid space to move to!");
                }
            }
        }

        if (attackPanel.activeInHierarchy)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);
                RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos2D, Vector2.zero);
                foreach (RaycastHit2D hit in hits)
                {
                    if (hit.collider != null)
                    {
                        if (highlightedObjects.Contains(hit.collider.gameObject) && hit.collider.gameObject.CompareTag("Enemy Unit"))
                        {
                            selectedAttackUnit = hit.collider.gameObject.GetComponent<Unit>();
                            PopulateAttackPanel(selectedAttackUnit, selectedInfoUnit);
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

    public void EndTurnButton()
    {
        gameManager.turnTaken = true;
        gameManager.EndTurn("Player Unit");
    }

    public void TurnOnUI(List<string> uIElements)
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
    }
    public void MoveHighlight(Unit unit)
    {
        highlightedObjects.Clear();
        unit.GetComponent<Dijkstra>().DijkstraCalc(unit.NodeCostDict);
        foreach (MapNode terrainTile in unit.GetComponent<Dijkstra>().PossibleMoves())
        {
            if (!terrainTile.occupyingObject.CompareTag("Player Unit") && !terrainTile.occupyingObject.CompareTag("Enemy Unit"))
            {
                terrainTile.GetComponent<SpriteRenderer>().color = Color.blue;
                highlightedObjects.Add(terrainTile.gameObject);
            }
        }
    }
    public void AttackHighlight(Unit unit)
    {
        highlightedObjects.Clear();
        foreach (GameObject enemyUnit in GameObject.FindGameObjectsWithTag("Enemy Unit"))
        {
            float distance = Mathf.Abs(unit.transform.position.x - enemyUnit.transform.position.x) + Mathf.Abs(unit.transform.position.y - enemyUnit.transform.position.y);
            if (distance <= unit.WeaponRange)
            {
                enemyUnit.GetComponent<Unit>().currentMapNode.GetComponent<SpriteRenderer>().color = Color.red;
                highlightedObjects.Add(enemyUnit.GetComponent<Unit>().currentMapNode.gameObject);
            }
        }
    }
    public void RemoveHighlight()
    {
        foreach (GameObject terrainTile in highlightedObjects)
        {
            terrainTile.GetComponent<SpriteRenderer>().color = Color.white;
        }
        highlightedObjects.Clear();
    }

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

    public void UnitPanelsDefault()
    {
        unitPanel.transform.Find("Unit Panel Name").GetComponent<TextMeshProUGUI>().text = "Name:";
        unitPanel.transform.Find("Unit Panel Type").GetComponent<TextMeshProUGUI>().text = "Type: ";
        unitPanel.transform.Find("Unit Panel HP").GetComponent<TextMeshProUGUI>().text = "HP:";
        unitPanel.transform.Find("Unit Panel Stamina").GetComponent<TextMeshProUGUI>().text = "Stamina: ";

        unitPanel.transform.Find("Unit Panel Move Button").gameObject.SetActive(true);
        unitPanel.transform.Find("Unit Panel Attack Button").gameObject.SetActive(true);

        infoPanel.transform.Find("Info Panel Name").GetComponent<TextMeshProUGUI>().text = "Name:";
        infoPanel.transform.Find("Info Panel Type").GetComponent<TextMeshProUGUI>().text = "Type:";
        infoPanel.transform.Find("Info Panel HP").GetComponent<TextMeshProUGUI>().text = "HP:";
        infoPanel.transform.Find("Info Panel Stamina").GetComponent<TextMeshProUGUI>().text = "Stamina:";
        infoPanel.transform.Find("Info Panel Attack").GetComponent<TextMeshProUGUI>().text = "Attack:";
        infoPanel.transform.Find("Info Panel Advantage").GetComponent<TextMeshProUGUI>().text = "Adv:";
        infoPanel.transform.Find("Info Panel Disadvantage").GetComponent<TextMeshProUGUI>().text = "Disadv:";
    }

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

    public void PopulateTerrainPanel(MapNode terrainTile)
    {
        terrainPanel.transform.Find("Terrain Panel Type").GetComponent<TextMeshProUGUI>().text = "Type: " + terrainTile.terrainType;
        if (terrainTile.isOccupied)
        {
            terrainPanel.transform.Find("Terrain Panel Unit").GetComponent<TextMeshProUGUI>().text = "Unit: " + terrainTile.occupyingObject.GetComponent<Unit>().UnitType;
        }
        else
        {
            terrainPanel.transform.Find("Terrain Panel Unit").GetComponent<TextMeshProUGUI>().text = "Unit: None";
        }
    }
    public void TerrainPanelDefault()
    {
        terrainPanel.transform.Find("Terrain Panel Type").GetComponent<TextMeshProUGUI>().text = "Type:";
        terrainPanel.transform.Find("Terrain Panel Unit").GetComponent<TextMeshProUGUI>().text = "Unit:";
    }

    public void PopulateMovePanel(MapNode terrainTile, Unit unit)
    {
        movePanel.transform.Find("Move Panel Terrain").GetComponent<TextMeshProUGUI>().text = "Terrain: " + terrainTile.terrainType;
        movePanel.transform.Find("Move Panel Stamina").GetComponent<TextMeshProUGUI>().text = "Stamina: " + unit.CurrentStamina.ToString();
        movePanel.transform.Find("Move Panel Cost").GetComponent<TextMeshProUGUI>().text = "Cost: " + unit.GetComponent<Dijkstra>().dijkstraDict[terrainTile].ToString();
    }

    public void MovePanelDefault()
    {
        movePanel.transform.Find("Move Panel Terrain").GetComponent<TextMeshProUGUI>().text = "Terrain:";
        movePanel.transform.Find("Move Panel Stamina").GetComponent<TextMeshProUGUI>().text = "Stamina:";
        movePanel.transform.Find("Move Panel Cost").GetComponent<TextMeshProUGUI>().text = "Cost:";
    }

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

    public void PopulateAttackPanel(Unit target, Unit unit)
    {
        attackPanel.transform.Find("Attack Panel Target").GetComponent<TextMeshProUGUI>().text = "Target: " + target.UnitName;
        attackPanel.transform.Find("Attack Panel Target HP").GetComponent<TextMeshProUGUI>().text = "Target HP: " + target.CurrentHP.ToString();
        attackPanel.transform.Find("Attack Panel Base Damage").GetComponent<TextMeshProUGUI>().text = "Base Damage: " + unit.WeaponDamage.ToString();
    }

    public void AttackPanelDefault()
    {
        attackPanel.transform.Find("Attack Panel Target").GetComponent<TextMeshProUGUI>().text = "Target:";
        attackPanel.transform.Find("Attack Panel Target HP").GetComponent<TextMeshProUGUI>().text = "Target HP:";
        attackPanel.transform.Find("Attack Panel Base Damage").GetComponent<TextMeshProUGUI>().text = "Base Damage:";
    }

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
            if (selectedInfoUnit.UnitType == "Swordsmen" || selectedInfoUnit.UnitType == "Spearmen")
            {
                gameManager.unitMovedDict[selectedInfoUnit] = true;
            }
            PopulateUnitPanels(selectedInfoUnit);
            attackPanel.SetActive(false);
            AttackPanelDefault();
            unitPanel.SetActive(true);
            selectedAttackUnit = null;
        }
    }

    public void AttackPanelBackButton()
    {
        RemoveHighlight();
        attackPanel.SetActive(false);
        AttackPanelDefault();
        selectedAttackUnit = null;
        unitPanel.SetActive(true);
    }
}