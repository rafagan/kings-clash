using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class IA_StructureFighter : AbstractUnitComponent, IStateMachine {

    protected IA_HFSM_StructureUnitManager Manager;

    public enum StateTypes { OFFENSIVE, PASSIVE };
    public StateTypes StateSelected = StateTypes.OFFENSIVE;
    public StateMachine<BaseUnit> CurrentFsm;

    protected StateTypes PreviousState = StateTypes.OFFENSIVE;
    protected Dictionary<StateTypes, StateMachine<BaseUnit>> Fsms;
    protected Dictionary<StateTypes, AbstractState<BaseUnit>> Factory, GlobalFactory;

	void Awake() {
        Fsms = new Dictionary<StateTypes, StateMachine<BaseUnit>> {
            {StateTypes.OFFENSIVE, new StateMachine<BaseUnit>()},
            {StateTypes.PASSIVE, new StateMachine<BaseUnit>()}
        };
        Factory = new Dictionary<StateTypes, AbstractState<BaseUnit>> {
            {StateTypes.OFFENSIVE, new DefensiveStates.Borning()},
            {StateTypes.PASSIVE, new PassiveStates.Borning()}
        };
        GlobalFactory = new Dictionary<StateTypes, AbstractState<BaseUnit>> {
            {StateTypes.OFFENSIVE, new DefensiveStates.Global()},
            {StateTypes.PASSIVE, new PassiveStates.Global()}
        };
	}

    void Start() {
        if (baseUnit == null)
            baseUnit = transform.parent.GetComponent<BaseUnit>();

        Manager = baseUnit.GetComponentInChildren<IA_HFSM_StructureUnitManager>();
        foreach (var sm in Fsms) {
            sm.Value.AttackerRange = baseUnit.GetComponentInChildren<AttackerRangeView>();
            sm.Value.Attack = baseUnit.GetComponentInChildren<AttackComponent>();
            sm.Value.MyNavMesh = baseUnit.GetComponentInChildren<UnityEngine.AI.NavMeshAgent>();
            sm.Value.Mobile = baseUnit.GetComponentInChildren<MobileComponent>();
            sm.Value.Collector = baseUnit.GetComponentInChildren<CollectorComponent>();
            sm.Value.CollectorRange = baseUnit.GetComponentInChildren<CollectorRangeView>();
            sm.Value.Structure = baseUnit.GetComponentInChildren<StructureComponent>();
            sm.Value.MyAnimator = baseUnit.GetComponentInChildren<UnitAnimator>();
        }

        //A unidade pode atacar?
        if (!baseUnit.GetUnitComponent<AttributesComponent>().CanAttack)
            Destroy(this);

        ChangeFsm();
    }

    void Update() {
        CurrentFsm.Update();

        if (PreviousState != StateSelected)
            ChangeFsm();
    }

    public override void GUIPriority() {
        if (!baseUnit.IsSelected || baseUnit.IsEnemy) return;
        GUI.Label(new Rect(2.0f, 200.0f, 200.0f, 25.0f), "UNIT STATE:");

        var states = new string[] { "Offensive", "Passive" };

        PreviousState = StateSelected;
        StateSelected = (StateTypes)GUI.SelectionGrid(
            new Rect(2.0f, 230.0f, 225.0f, 25.0f), (int)StateSelected, states, 2, "toggle");

        foreach (var unit in InterfaceManager.Manager.SameUnitsToSendInput) {
            var fighter = unit.GetUnitComponent<IA_StructureFighter>();
            if (fighter == null) continue;
            fighter.StateSelected = StateSelected;
        }
    }

    protected virtual void ChangeFsm() {
        PreviousState = StateSelected;
        CurrentFsm = Fsms[StateSelected];
        CurrentFsm.Init(baseUnit, Factory[StateSelected], GlobalFactory[StateSelected]);
    }

    public void SendMessageToState(IA_Messages iaMessage) {
        CurrentFsm.SendMessageToState(iaMessage);
    }

    public override void UserInputPriority() { }

    public virtual MonoBehaviour GetMeAsAComponent() {
        return this;
    }

    public IState<BaseUnit> CurrentState() {
        return CurrentFsm.CurrentState;
    }

    public override void Reset() {}
}
