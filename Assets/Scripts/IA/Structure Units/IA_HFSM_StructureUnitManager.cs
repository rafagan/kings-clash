using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections;

public enum StructureFSMType { FSM_FIGHTER }

public class IA_HFSM_StructureUnitManager : AbstractUnitComponent {
    private IStateMachine _currentSM;
    private IStateMachine _fighter;

    public IStateMachine CurrentStateMachine {
        get { return _currentSM; }
        set { _currentSM = value; }
    }
    public IStateMachine Fighter {
        get { return _fighter ?? (_fighter = GetComponent<IA_StructureFighter>() ?? gameObject.AddComponent<IA_StructureFighter>()); }
        private set { _fighter = value; }
    }

    public Dictionary<StructureFSMType, IStateMachine> PriorityOrder;

    void Awake() {
        PriorityOrder = new Dictionary<StructureFSMType, IStateMachine>();
    }

    void Start() {
        PriorityOrder.Add(StructureFSMType.FSM_FIGHTER, Fighter);
        _currentSM = PriorityOrder[StructureFSMType.FSM_FIGHTER];

        //Já estou considerando aqui futuras HFSM
        foreach (var current in PriorityOrder.Where(current => current.Value != null && current.Value != _currentSM)) {
            current.Value.GetMeAsAComponent().enabled = false;
        }
    }

    public void ChangeStateMachine(StructureFSMType type) {
        _currentSM.GetMeAsAComponent().enabled = false;
        _currentSM = PriorityOrder[type];
        _currentSM.GetMeAsAComponent().enabled = true;
    }

    public override void GUIPriority() {

    }

    public override void UserInputPriority() {

    }
    
    public override void Reset() {}
}
