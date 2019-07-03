
using System.Collections;

public abstract class AbstractState<T> : IState<T> {
    protected T Entity { get; set; }
    public StateMachine<T> FSM;

    protected AttackerRangeView AttackerRange;
    protected AttackComponent Attack;
    protected UnityEngine.AI.NavMeshAgent MyNavMesh;
    protected MobileComponent Mobile;
    protected CollectorComponent Collector;
    protected CollectorRangeView CollectorRange;
    protected StructureComponent Structure;
    protected BuilderRangeView BuilderRange;
    protected ConstructorComponent Constructor;
    protected AbilityLibraryComponent AbilityLibrary;
    protected UnitAnimator MyAnimator;
    protected AttributesComponent Attributes;

    public virtual string Name { get { return ""; } set {} }

    public void Init(StateMachine<T> fsm) {
        FSM = fsm;
        Entity = FSM.Owner;
        AttackerRange = FSM.AttackerRange;
        Attack = FSM.Attack;
        MyNavMesh = FSM.MyNavMesh;
        Mobile = FSM.Mobile;
        Collector = FSM.Collector;
        CollectorRange = FSM.CollectorRange;
        BuilderRange = FSM.BuilderRange;
        Structure = FSM.Structure;
        Constructor = FSM.Constructor;
        MyAnimator = FSM.MyAnimator;
        AbilityLibrary = FSM.AbilityLibrary;
        Attributes = FSM.Attributes;

        if (Name == "")
            return;
        if (Entity is BaseUnit)
            if ((Entity as BaseUnit).GetUnitComponent<Debugger>().IA_DebugEnabled) {
                DebugManager.IA_PrintInfo(Name);
            }
    }
    public virtual void Enter() {}

    public virtual IState<T> Process() {
        return this;
    }

    public virtual IState<T> FixedProcess() {
        return this;
    }

    public virtual void Leave() {}

    public virtual void ReceiveMessage(IA_Messages iaMessage) {}
}
