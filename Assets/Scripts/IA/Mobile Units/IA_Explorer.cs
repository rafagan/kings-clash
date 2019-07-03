using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class IA_Explorer : AbstractUnitComponent, IStateMachine {
//    private IA_HFSM_MobileUnitManager _manager;
    private CollectorComponent _collector;

    public enum StateTypes { EXPLORING };
    public StateTypes StateSelected = StateTypes.EXPLORING;
    public StateMachine<BaseUnit> CurrentFsm;

    private StateTypes _previousState = StateTypes.EXPLORING;
    private Dictionary<StateTypes, StateMachine<BaseUnit>> _fsms;
    private Dictionary<StateTypes, AbstractState<BaseUnit>> _firstStateFactory, _globalFactory;

    void Awake() {
        _fsms = new Dictionary<StateTypes, StateMachine<BaseUnit>> {
            {StateTypes.EXPLORING, new StateMachine<BaseUnit>()},
        };
        _firstStateFactory = new Dictionary<StateTypes, AbstractState<BaseUnit>> {
            {StateTypes.EXPLORING, new ExplorerStates.Borning()},
        };
        _globalFactory = new Dictionary<StateTypes, AbstractState<BaseUnit>> {
            {StateTypes.EXPLORING, new ExplorerStates.Global()}
        };
    }

    void Start() {
        if (baseUnit == null)
            baseUnit = transform.parent.GetComponent<BaseUnit>();

        foreach (var sm in _fsms) {
            sm.Value.MyNavMesh = baseUnit.GetComponentInChildren<UnityEngine.AI.NavMeshAgent>();
            sm.Value.Mobile = baseUnit.GetComponentInChildren<MobileComponent>();
            sm.Value.AbilityLibrary = baseUnit.GetComponentInChildren<AbilityLibraryComponent>();
            sm.Value.MyAnimator = baseUnit.GetComponentInChildren<UnitAnimator>();
        }
//        _manager = baseUnit.GetComponentInChildren<IA_HFSM_MobileUnitManager>();

        ChangeFsm();
    }

    void Update() {
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

    public override void GUIPriority() { }

    public override void UserInputPriority() { }

    public override void Reset() { }
}
