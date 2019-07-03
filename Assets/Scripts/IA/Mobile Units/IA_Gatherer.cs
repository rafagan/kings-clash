using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class IA_Gatherer : AbstractUnitComponent, IStateMachine {
    private IA_HFSM_MobileUnitManager _manager;
    private CollectorComponent _collector;

    public enum StateTypes { NO_COLLECT, PLASMO, TREE };
    public StateTypes StateSelected = StateTypes.NO_COLLECT;
    public StateMachine<BaseUnit> CurrentFsm;

    private StateTypes _previousState = StateTypes.NO_COLLECT;
    private Dictionary<StateTypes, StateMachine<BaseUnit>> _fsms;
    private Dictionary<StateTypes, AbstractState<BaseUnit>> _firstStateFactory,_globalFactory;

    void Awake() {
        _fsms = new Dictionary<StateTypes, StateMachine<BaseUnit>> {
            {StateTypes.NO_COLLECT, new StateMachine<BaseUnit>()},
            {StateTypes.PLASMO, new StateMachine<BaseUnit>()},
            {StateTypes.TREE, new StateMachine<BaseUnit>()}
        };
        _firstStateFactory = new Dictionary<StateTypes, AbstractState<BaseUnit>> {
            {StateTypes.NO_COLLECT, new NoCollectStates.Borning()},
            {StateTypes.PLASMO, new PlasmoGathererStates.SearchingPlasmoRock()},
            {StateTypes.TREE, new TreeGathererStates.SearchingTree()}
        };
        _globalFactory = new Dictionary<StateTypes, AbstractState<BaseUnit>> {
            {StateTypes.NO_COLLECT, new NoCollectStates.Global()},
            {StateTypes.PLASMO, new PlasmoGathererStates.Global()},
            {StateTypes.TREE, new TreeGathererStates.Global()}
        };
    }

	void Start () {
        if (baseUnit == null)
            baseUnit = transform.parent.GetComponent<BaseUnit>();

        foreach (var sm in _fsms) {
            sm.Value.AttackerRange = baseUnit.GetComponentInChildren<AttackerRangeView>();
            sm.Value.Attack = baseUnit.GetComponentInChildren<AttackComponent>();
            sm.Value.MyNavMesh = baseUnit.GetComponentInChildren<UnityEngine.AI.NavMeshAgent>();
            sm.Value.Mobile = baseUnit.GetComponentInChildren<MobileComponent>();
            sm.Value.Collector = baseUnit.GetComponentInChildren<CollectorComponent>();
            sm.Value.CollectorRange = baseUnit.GetComponentInChildren<CollectorRangeView>();
            sm.Value.MyAnimator = baseUnit.GetComponentInChildren<UnitAnimator>();
        }
        _manager = baseUnit.GetComponentInChildren<IA_HFSM_MobileUnitManager>();

        //A unidade pode coletar?
        _collector = baseUnit.GetUnitComponent<CollectorComponent>();
        if (!_collector)
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

    public void SendMessageToState(IA_Messages iaMessage) {
        switch (iaMessage) {
            case IA_Messages.NODE_GATHERING_PLASMO:
            case IA_Messages.CRUDE_GATHERING_PLASMO:
                if (StateSelected != StateTypes.PLASMO) {
                    StateSelected = StateTypes.PLASMO;
                    _collector.CurrentResourceType = ResourceType.Plasmo;
                    return;
                }
                break;
            case IA_Messages.CRUDE_GATHERING_TREE:
            case IA_Messages.GATHERING_LOGGED_TREE:
                if (StateSelected != StateTypes.TREE) {
                    StateSelected = StateTypes.TREE;
                    _collector.CurrentResourceType = ResourceType.Tree;
                    return;
                }
                break;
            case IA_Messages.STOCK:
                break;
            case IA_Messages.MOVE_TO_ATTACK:
                _manager.ChangeStateMachine(MobileFSMType.FSM_FIGHTER);
                _manager.CurrentStateMachine.SendMessageToState(iaMessage);
                return;
            case IA_Messages.BUILD:
            case IA_Messages.REPAIR:
                _manager.ChangeStateMachine(MobileFSMType.FSM_BUILDER);
                _manager.CurrentStateMachine.SendMessageToState(iaMessage);
                return;
        }

        CurrentFsm.SendMessageToState(iaMessage);
    }

    public IState<BaseUnit> CurrentState() {
        return CurrentFsm.CurrentState;
    }

    public MonoBehaviour GetMeAsAComponent() {
        return this;
    }
    
    public override void GUIPriority() {
    
    }
    
    public override void UserInputPriority() {
	
	}
	
	public override void Reset() {}
}
