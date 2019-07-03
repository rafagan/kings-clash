using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class IA_Fighter : AbstractUnitComponent, IStateMachine
{
    private IA_HFSM_MobileUnitManager _manager;

    public enum StateTypes { OFFENSIVE, DEFENSIVE, PASSIVE };
    public StateTypes StateSelected = StateTypes.OFFENSIVE;
    public StateMachine<BaseUnit> CurrentFsm;

    protected StateTypes PreviousState = StateTypes.OFFENSIVE;
    protected Dictionary<StateTypes, StateMachine<BaseUnit>> Fsms;
    protected Dictionary<StateTypes, AbstractState<BaseUnit>> Factory, GlobalFactory;

    void Awake() {
        Fsms = new Dictionary<StateTypes, StateMachine<BaseUnit>> {
            {StateTypes.OFFENSIVE, new StateMachine<BaseUnit>()},
            {StateTypes.DEFENSIVE, new StateMachine<BaseUnit>()},
            {StateTypes.PASSIVE, new StateMachine<BaseUnit>()}
        };
        Factory = new Dictionary<StateTypes, AbstractState<BaseUnit>> {
            {StateTypes.OFFENSIVE, new OffensiveStates.Borning()},
            {StateTypes.DEFENSIVE, new DefensiveStates.Borning()},
            {StateTypes.PASSIVE, new PassiveStates.Borning()}
        };
        GlobalFactory = new Dictionary<StateTypes, AbstractState<BaseUnit>> {
            {StateTypes.OFFENSIVE, new OffensiveStates.Global()},
            {StateTypes.DEFENSIVE, new DefensiveStates.Global()},
            {StateTypes.PASSIVE, new PassiveStates.Global()}
        };
    }

	void Start () {
	    if (baseUnit == null)
	        baseUnit = transform.parent.GetComponent<BaseUnit>();

        if (baseUnit.GetUnitComponent<AttackComponent>() == null) {
            Destroy(this);
            return;
        }

        _manager = baseUnit.GetComponentInChildren<IA_HFSM_MobileUnitManager>();
        foreach (var sm in Fsms) {
            sm.Value.AttackerRange = baseUnit.GetComponentInChildren<AttackerRangeView>();
            sm.Value.Attack = baseUnit.GetComponentInChildren<AttackComponent>();
            sm.Value.MyNavMesh = baseUnit.GetComponentInChildren<UnityEngine.AI.NavMeshAgent>();
            sm.Value.Mobile = baseUnit.GetComponentInChildren<MobileComponent>();
            sm.Value.Collector = baseUnit.GetComponentInChildren<CollectorComponent>();
            sm.Value.CollectorRange = baseUnit.GetComponentInChildren<CollectorRangeView>();
            sm.Value.Structure = baseUnit.GetUnitComponent<StructureComponent>();
            sm.Value.MyAnimator = baseUnit.GetUnitComponent<UnitAnimator>();
            sm.Value.Attributes = baseUnit.GetUnitComponent<AttributesComponent>();
        }
        ChangeFsm();
	}

    void Update () {
        CurrentFsm.Update();

        if (PreviousState != StateSelected)
            ChangeFsm();
	}

    void FixedUpdate() {
        CurrentFsm.FixedUpdate();
    }

    protected virtual void ChangeFsm() {
        PreviousState = StateSelected;
        CurrentFsm = Fsms[StateSelected];
        CurrentFsm.Init(baseUnit, Factory[StateSelected], GlobalFactory[StateSelected]);
    }

    public override void GUIPriority() {}

    public override void UserInputPriority() {}

    public virtual void SendMessageToState(IA_Messages iaMessage) {
        switch (iaMessage){
            case IA_Messages.PASSIVE:
                StateSelected = StateTypes.PASSIVE;
                return;
            case IA_Messages.DEFENSIVE:
                StateSelected = StateTypes.DEFENSIVE;
                return;
            case IA_Messages.OFFENSIVE:
                StateSelected = StateTypes.OFFENSIVE;
                return;
            case IA_Messages.CRUDE_GATHERING_PLASMO:
            case IA_Messages.CRUDE_GATHERING_TREE:
            case IA_Messages.NODE_GATHERING_PLASMO:
            case IA_Messages.GATHERING_LOGGED_TREE:
                _manager.ChangeStateMachine(MobileFSMType.FSM_GATHERER);
                _manager.CurrentStateMachine.SendMessageToState(iaMessage);
                return;
            case IA_Messages.BUILD:
            case IA_Messages.REPAIR:
                _manager.ChangeStateMachine(MobileFSMType.FSM_BUILDER);
                _manager.CurrentStateMachine.SendMessageToState(iaMessage);
                break;
        }

        if (CurrentFsm == null)
            ChangeFsm();
        
        CurrentFsm.SendMessageToState(iaMessage);
    }

    public IState<BaseUnit> CurrentState() {
        return CurrentFsm.CurrentState;
    }

    public virtual MonoBehaviour GetMeAsAComponent() {
        return this;
    }
    
    public override void Reset() {}
}
