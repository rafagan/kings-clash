using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System;


public class ActionBarRow : MonoBehaviour {
    public System.Collections.Generic.List<ActionBarInfo> ActionList;

    public string[] ItemNames;
    public GameObject ActionBarButtonPrefab;
    public enum RowArrangement { Horizontal, Vertical };
    public Vector2 ButtonSize;
    GameObject ActionBarButtonClone;
    public int Columns;
    public int ColumnPadding;
    public int Rows;
    public int RowPadding;
    public RowArrangement Arrangement;
    UIGrid ActionBarGrid;
    public bool CloneOnPickup = false;
    public bool DisplayOneStack = true;
    public KeyCode[] Hotkey;
    [HideInInspector]
    public List<ActionBarButton> ButtonList = new List<ActionBarButton>();

    public bool CompleteRowWithEmptyButtons;
    public void SetNames() {
        ItemNames = new string[ActionList.Count];
        for (int i = 0; i < ActionList.Count; i++) {
            ItemNames[i] = ActionList[i].Icon;
        }
    }

    void Start() {
        if (ActionBarButtonPrefab != null) {
            //Error checking in case user forgets to assign size values
            if ((ButtonSize.x == 0F) || (ButtonSize.y == 0F)) {
                Debug.LogWarning("Button Size is set to 0.  It will not be Visible");
            }
            //Avoids modifying the original Prefab
            ActionBarButtonClone = ActionBarButtonPrefab;

            //Assign the Grid Component in order to arrange the objects.
            ActionBarGrid = (UIGrid)this.gameObject.AddComponent<UIGrid>();
            ActionBarGrid.maxPerLine = Columns;
            if (Arrangement == RowArrangement.Horizontal) { ActionBarGrid.arrangement = UIGrid.Arrangement.Horizontal; }
            else { ActionBarGrid.arrangement = UIGrid.Arrangement.Vertical; }
            ActionBarGrid.cellWidth = ButtonSize.x + ColumnPadding;
            ActionBarGrid.cellHeight = ButtonSize.y + RowPadding;
            ActionBarGrid.sorted = false;
            ActionBarGrid.hideInactive = true;
            SpawnChilds();
        }
        else {
            Debug.LogWarning("Voce precisa referenciar o prefab do ActionBarButton no ActionBarRow!");
        }
    }

    public void UpdateActionButtons() {
        for (int i = 0; i < ActionList.Count; i++) {
            ButtonList[i].SetInfo(ActionList[i]);
        }
        for (int i = ActionList.Count; i < ButtonList.Count - 1; i++) {
            ButtonList[i].Disable();
        }
    }

    public void DisableButtons() {
        foreach (var button in ButtonList) {
            button.Disable();
        }
    }

    public void SpawnChilds() {
        //Spawn Actions
        for (int i = 0; i < Columns * Rows; i++) {
            //Assign Button Size
            ActionBarButtonClone.transform.GetChild(0).transform.localScale = new Vector3(ButtonSize.x, ButtonSize.y, 1F);
            //Add Button as a child to the Action Bar Row
            ActionBarButton button = NGUITools.AddChild(this.gameObject, ActionBarButtonClone).GetComponentInChildren<ActionBarButton>();
            //Assign MISC Information to Items/Spells
            button.BarRow = GetComponent<ActionBarRow>();	//Assign Parent Row
            ButtonList.Add(button); //Add button to list
            ActionBarGrid.Reposition();
        }
    }

}