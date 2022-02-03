using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIController : MonoBehaviour
{
    public GameManager gameManager;
    private List<GameObject> highlightedObjects = new List<GameObject>();
    public bool playerTurn = false;

    private void Update()
    {
        if (gameManager.currentUnitTurn == "Player Unit")
        {
            playerTurn = true;
        }
    }
    public void EndTurnButton()
    {
        gameManager.turnTaken = true;
    }
    public void InfoButton(Unit unit)
    {

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
        foreach (MapNode terrainTile in unit.GetComponent<Dijkstra>().PossibleMoves())
        {
            if (!terrainTile.occupyingObject.CompareTag("Player Unit") && !terrainTile.occupyingObject.CompareTag("Enemy Unit"))
            {
                terrainTile.GetComponent<SpriteRenderer>().color = Color.blue;
                highlightedObjects.Add(terrainTile.gameObject);
            }
        }
    }
    public void AttackHighlight(Unit unit, int range)
    {
        foreach (GameObject enemyUnit in GameObject.FindGameObjectsWithTag("Enemy Unit"))
        {
            float distance = Mathf.Abs(unit.transform.position.x - enemyUnit.transform.position.x) + Mathf.Abs(unit.transform.position.y - enemyUnit.transform.position.y);
            if (distance <= range)
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
            highlightedObjects.Remove(terrainTile);
        }
    }

    public void PopulateUnitPanels(Unit unit)
    {
        GameObject.Find("Unit Panel Name").GetComponent<TextMeshPro>().text = "Name: " + unit.UnitName;
        GameObject.Find("Unit Panel Type").GetComponent<TextMeshPro>().text = "Type: " + unit.UnitType;
        GameObject.Find("Unit Panel HP").GetComponent<TextMeshPro>().text = "HP: " + unit.CurrentHP.ToString();
        GameObject.Find("Unit Panel Stamina").GetComponent<TextMeshPro>().text = "Stamina: " + unit.CurrentStamina.ToString();

        GameObject.Find("Info Panel Name").GetComponent<TextMeshPro>().text = "Name: " + unit.UnitName;
        GameObject.Find("Info Panel Type").GetComponent<TextMeshPro>().text = "Type: " + unit.UnitType;
        GameObject.Find("Info Panel HP").GetComponent<TextMeshPro>().text = "HP: " + unit.CurrentHP.ToString();
        GameObject.Find("Info Panel Stamina").GetComponent<TextMeshPro>().text = "Stamina: " + unit.CurrentStamina.ToString();
        GameObject.Find("Info Panel Attack").GetComponent<TextMeshPro>().text = "Attack: " + unit.WeaponDamage.ToString();
        GameObject.Find("Info Panel Advantage").GetComponent<TextMeshPro>().text = "Adv: " + string.Join(", ", unit.Advantages);
        GameObject.Find("Info Panel Disadvantage").GetComponent<TextMeshPro>().text = "Disadv: " + string.Join(", ", unit.Disadvantages);
    }

    public void UnitPanelsDefault()
    {
        GameObject.Find("Unit Panel Name").GetComponent<TextMeshPro>().text = "Name:";
        GameObject.Find("Unit Panel Type").GetComponent<TextMeshPro>().text = "Type: ";
        GameObject.Find("Unit Panel HP").GetComponent<TextMeshPro>().text = "HP:";
        GameObject.Find("Unit Panel Stamina").GetComponent<TextMeshPro>().text = "Stamina: ";

        GameObject.Find("Info Panel Name").GetComponent<TextMeshPro>().text = "Name:";
        GameObject.Find("Info Panel Type").GetComponent<TextMeshPro>().text = "Type:";
        GameObject.Find("Info Panel HP").GetComponent<TextMeshPro>().text = "HP:";
        GameObject.Find("Info Panel Stamina").GetComponent<TextMeshPro>().text = "Stamina:";
        GameObject.Find("Info Panel Attack").GetComponent<TextMeshPro>().text = "Attack:";
        GameObject.Find("Info Panel Advantage").GetComponent<TextMeshPro>().text = "Adv:";
        GameObject.Find("Info Panel Disadvantage").GetComponent<TextMeshPro>().text = "Disadv:";
    }
}