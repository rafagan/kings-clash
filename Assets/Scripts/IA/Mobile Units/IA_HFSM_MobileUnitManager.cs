using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections;

public enum MobileFSMType { FSM_FIGHTER, FSM_GATHERER, FSM_BUILDER, FSM_EXPLORER }

public class IA_HFSM_MobileUnitManager : AbstractUnitComponent {
    private IStateMachine _currentSM;
    private IStateMachine _fighter;
    private IStateMachine _gatherer;
    private IStateMachine _builder;
    private IStateMachine _explorer;
    private IStateMachine _patrol;

    public IStateMachine CurrentStateMachine {
        get { return _currentSM; }
        set { _currentSM = value; }
    }
    public IStateMachine Fighter {
        get { return _fighter ?? (_fighter = GetComponent<IA_Fighter>() ?? gameObject.AddComponent<IA_Fighter>()); }
        private set { _fighter = value; }
    }
    public IStateMachine Gatherer {
        get { return _gatherer ?? (_gatherer = GetComponent<IA_Gatherer>() ?? gameObject.AddComponent<IA_Gatherer>()); }
        private set { _gatherer = value; }
    }
    public IStateMachine Builder {
        get { return _builder ?? (_builder = GetComponent<IA_BuildRepair>() ?? gameObject.AddComponent<IA_BuildRepair>());}
        private set { _builder = value; }
    }
    public IStateMachine Explorer {
        get { return _explorer ?? (_explorer = GetComponent<IA_Explorer>() ?? gameObject.AddComponent<IA_Explorer>()); }
        private set { _explorer = value; }
    }
    public IStateMachine Patrol {
        get { return _patrol ?? (_patrol = GetComponent<IA_Patrol>() ?? gameObject.AddComponent<IA_Patrol>()); }
        private set { _patrol = value; }
    }

    public Dictionary<MobileFSMType,IStateMachine> PriorityOrder;

    void Awake() {
        PriorityOrder = new Dictionary<MobileFSMType,IStateMachine>();
    }

    void Start() {
        if (baseUnit.UnitClass == Unit.Crow) {
            PriorityOrder.Add(MobileFSMType.FSM_EXPLORER, Explorer);
            _currentSM = PriorityOrder.First().Value;
            return;
        }

        if (!(baseUnit.UnitClass == Unit.Blacksmith || baseUnit.UnitClass == Unit.Monarch)) {
            PriorityOrder.Add(MobileFSMType.FSM_FIGHTER, Fighter);
            _currentSM = PriorityOrder.First().Value;
            return;
        }

        switch (baseUnit.UnitClass) {
            case Unit.Blacksmith:
                PriorityOrder.Add(MobileFSMType.FSM_GATHERER,Gatherer);
                PriorityOrder.Add(MobileFSMType.FSM_BUILDER,Builder);
                PriorityOrder.Add(MobileFSMType.FSM_FIGHTER,Fighter);
                _currentSM = PriorityOrder[MobileFSMType.FSM_GATHERER];
                break;
            case Unit.Monarch:
                PriorityOrder.Add(MobileFSMType.FSM_FIGHTER,Fighter);
                PriorityOrder.Add(MobileFSMType.FSM_GATHERER,Gatherer);
                PriorityOrder.Add(MobileFSMType.FSM_BUILDER,Builder);
                _currentSM = PriorityOrder[MobileFSMType.FSM_FIGHTER];
                break;
        }

        foreach (var current in PriorityOrder.Where(current => current.Value != null && current.Value != _currentSM)) {
            current.Value.GetMeAsAComponent().enabled = false;
        }
    }

    public void ChangeStateMachine(MobileFSMType type) {
        _currentSM.CurrentState().Leave();
        _currentSM.GetMeAsAComponent().enabled = false;
        _currentSM = PriorityOrder[type];
        _currentSM.GetMeAsAComponent().enabled = true;
    }

    public IState<BaseUnit> CurrentState() {
        return _currentSM.CurrentState();
    }
    
    public override void GUIPriority() {
     
    }
     
    public override void UserInputPriority() {
	
    }
    
    public override void Reset() {}
}
