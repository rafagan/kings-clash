using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class IA_BuildRepair : AbstractUnitComponent, IStateMachine {
    private IA_HFSM_MobileUnitManager _manager;
    private CollectorComponent _collector;

    public enum StateTypes { BUILDING_AND_REPAIRING };
    public StateTypes StateSelected = StateTypes.BUILDING_AND_REPAIRING;
    public StateMachine<BaseUnit> CurrentFsm;

    private StateTypes _previousState = StateTypes.BUILDING_AND_REPAIRING;
    private Dictionary<StateTypes, StateMachine<BaseUnit>> _fsms;
    private Dictionary<StateTypes, AbstractState<BaseUnit>> _firstStateFactory, _globalFactory;

    void Awake() {
        _fsms = new Dictionary<StateTypes, StateMachine<BaseUnit>> {
            {StateTypes.BUILDING_AND_REPAIRING, new StateMachine<BaseUnit>()},
        };
        _firstStateFactory = new Dictionary<StateTypes, AbstractState<BaseUnit>> {
            {StateTypes.BUILDING_AND_REPAIRING, new BuildRepairStates.Borning()},
        };
        _globalFactory = new Dictionary<StateTypes, AbstractState<BaseUnit>> {
            {StateTypes.BUILDING_AND_REPAIRING, new BuildRepairStates.Global()}
        };
    }

	void Start () {
        if (baseUnit == null)
            baseUnit = transform.parent.GetComponent<BaseUnit>();

        foreach (var sm in _fsms) {
            sm.Value.MyNavMesh = baseUnit.GetComponentInChildren<UnityEngine.AI.NavMeshAgent>();
            sm.Value.Mobile = baseUnit.GetComponentInChildren<MobileComponent>();
            sm.Value.BuilderRange = baseUnit.GetComponentInChildren<BuilderRangeView>();
            sm.Value.Constructor = baseUnit.GetComponentInChildren<ConstructorComponent>();
            sm.Value.AbilityLibrary = baseUnit.GetComponentInChildren<AbilityLibraryComponent>();
            sm.Value.MyAnimator = baseUnit.GetComponentInChildren<UnitAnimator>();
            sm.Value.BuilderRange = baseUnit.GetComponentInChildren<BuilderRangeView>();
        }
        _manager = baseUnit.GetComponentInChildren<IA_HFSM_MobileUnitManager>();

        //A unidade pode construir?
        if (_fsms[0].Constructor == null)
            Destroy(this);
        ChangeFsm();
	}
	
	void Update () {
        CurrentFsm.Update();
        if (_previousState != StateSelected)
            ChangeFsm();
	}

    void FixedUpdate() {
        CurrentFsm.FixedUpdate();
    }

    private void ChangeFsm() {
        _previousState = StateSelected;
        CurrentFsm = _fsms[StateSelected];
        CurrentFsm.Init(baseUnit, _firstStateFactory[StateSelected], _globalFactory[StateSelected]);
    }

    public virtual void SendMessageToState(IA_Messages iaMessage) {
        switch (iaMessage) {
            case IA_Messages.CRUDE_GATHERING_PLASMO:
            case IA_Messages.CRUDE_GATHERING_TREE:
            case IA_Messages.NODE_GATHERING_PLASMO:
            case IA_Messages.GATHERING_LOGGED_TREE:
                _manager.ChangeStateMachine(MobileFSMType.FSM_GATHERER);
                _manager.CurrentStateMachine.SendMessageToState(iaMessage);
                return;
            case IA_Messages.MOVE_TO_ATTACK:
                _manager.ChangeStateMachine(MobileFSMType.FSM_FIGHTER);
                _manager.CurrentStateMachine.SendMessageToState(iaMessage);
                return;
        }

        if (CurrentFsm == null)
            ChangeFsm();

        CurrentFsm.SendMessageToState(iaMessage);
    }

    public MonoBehaviour GetMeAsAComponent() {
        return this;
    }

    public IState<BaseUnit> CurrentState() {
        return CurrentFsm.CurrentState;
    }

    public override void GUIPriority() {}

    public override void UserInputPriority() {}
    
    public override void Reset() {}
}
