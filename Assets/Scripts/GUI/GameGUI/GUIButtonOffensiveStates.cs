using UnityEngine;

public class GUIButtonOffensiveStates : MonoBehaviour {

    public UIToggle Button;
    public UISprite SpriteBackground;
    public UISprite SpriteCheckMark;

    void Update() {
        if (NotValidSelection()) {
            Disable();
            return;
        }
        Enable();
    }

    private bool NotValidSelection() {
        var selectedUnities = GameManager.Manager.unitsManager.SelectedUnits;
        var unitiesToSendInput = InterfaceManager.Manager.SameUnitsToSendInput;

        if (unitiesToSendInput.Count == 0 || selectedUnities.Count == 0) return true;

        return unitiesToSendInput[0].UnitType != ObjectType.CHARACTER;
    }

    void Enable() {
        Button.enabled = true;
        SpriteBackground.enabled = true;
        SpriteCheckMark.enabled = true;
        GetComponent<Collider>().enabled = true;
    }

    void Disable() {
        Button.enabled = false;
        SpriteBackground.enabled = false;
        SpriteCheckMark.enabled = false;
        GetComponent<Collider>().enabled = false;
    }

    void OnClick() {
        var selectedUnities = GameManager.Manager.unitsManager.SelectedUnits;
        foreach (var unity in selectedUnities) {
            var ia = unity.GetUnitComponent<IA_HFSM_MobileUnitManager>();
            ia.CurrentStateMachine.SendMessageToState(IA_Messages.OFFENSIVE);
        }
    }
}
