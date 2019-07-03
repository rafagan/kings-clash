
using System.Collections;

/*
 *	Descrição: Uma máquina de estados simples, com suporte a estados globais e retorno
 *	ao estado ligeiramente anterior.
 *	Não possui suporte a enfileiramento ou empilhamento de estados, e nem suporte a goals
 */

public class StateMachine<TEntityType> {
    private AbstractState<TEntityType> _currentState;
    private AbstractState<TEntityType> _previousState;
    private AbstractState<TEntityType> _globalState;

    public TEntityType Owner { get; private set; }
    public AttackerRangeView AttackerRange { get; set; }
    public AttackComponent Attack { get; set; }
    public UnityEngine.AI.NavMeshAgent MyNavMesh { get; set; }
    public MobileComponent Mobile { get; set; }
    public CollectorComponent Collector { get; set; }
    public CollectorRangeView CollectorRange { get; set; }
    public StructureComponent Structure { get; set; }
    public BuilderRangeView BuilderRange { get; set; }
    public ConstructorComponent Constructor { get; set; }
    public AbilityLibraryComponent AbilityLibrary { get; set; }
    public UnitAnimator MyAnimator { get; set; }
    public AttributesComponent Attributes { get; set; }

    public AbstractState<TEntityType> CurrentState
    {
        get { return _currentState; }
        set { ChangeState(value); }
    }

    public AbstractState<TEntityType> PreviousState { get; private set; }

    public AbstractState<TEntityType> GlobalState
    {
        get { return _globalState; }
        set {
            _globalState.Leave();
            _globalState = value;
            _globalState.Init(this);
            _globalState.Enter();
        }
    }

    public void Init(TEntityType owner,
        AbstractState<TEntityType> fisrtState,
        AbstractState<TEntityType> globalState = null)
    {
        _currentState = fisrtState;
        _globalState = globalState;
        Owner = owner;

        if (_globalState != null) {
            _globalState.Init(this);
            _globalState.Enter();
        }
        _currentState.Init(this);
        _currentState.Enter();
    }

    public void Update() {
        if (GlobalState != null)
            GlobalState.Process();

        var next = _currentState.Process();
        if (_currentState != next)
            ChangeState(next);
    }

    public void FixedUpdate() {
        if (GlobalState != null)
            GlobalState.FixedProcess();

        var next = _currentState.FixedProcess();
        if (_currentState != next)
            ChangeState(next);
    }

    public void ChangeState(IState<TEntityType> nextState)
    {
        _currentState.Leave();
        _previousState = _currentState;
        _currentState = (AbstractState<TEntityType>) nextState;
        _currentState.Init(this);
        _currentState.Enter();
    }

    public void ChangeStateToPrevious() {
        _currentState.Leave();
        var tmp = _currentState;
        _currentState = _previousState;
        _previousState = tmp;
        _currentState.Init(this);
        _currentState.Enter();
    }

    public bool IsInState(IState<TEntityType> state) {
        return _currentState.Name == state.Name;
    }

    public void SendMessageToState(IA_Messages iaMessage) {
        if(_globalState != null) _globalState.ReceiveMessage(iaMessage);
        _currentState.ReceiveMessage(iaMessage);
    }
}
