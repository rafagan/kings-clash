using UnityEngine;
using System.Collections;

public static class NoCollectStates {

    public class Global : AbstractState<BaseUnit> {
        public override string Name {
            get { return ""; }
            set { }
        }

        public override void Enter() { }

        public override IState<BaseUnit> Process() {
            return this;
        }

        public override void Leave() { }

        public override void ReceiveMessage(IA_Messages iaMessage) {
            switch (iaMessage) {
            case IA_Messages.MOVING:
                FSM.ChangeState(new Moving());
                break;
            case IA_Messages.DYING:
                FSM.ChangeState(new Dying());
                break;
            }
        }
    }

    //O estado Idle no NoCollectStates representa literalmente que o Blacksmith não tem ideia do
    //que coletar e não fará nada até que seja ordenado
    public class Idle : GeneralStates.GeneralIdle {
        public override string Name {
            get { return "NoCollectStates - Idle"; }
            set { }
        }
    }

    public class Moving : GeneralStates.GeneralMoving {
        protected override IState<BaseUnit> GetIdle() {
            return new Idle();
        }
    }

    public class Borning : GeneralStates.GeneralBorning {
        protected override IState<BaseUnit> GetFirstState() {
            return new Idle();
        }
    }

    public class Dying : GeneralStates.GeneralDying { }
}
