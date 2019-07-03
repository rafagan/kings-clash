using UnityEngine;
using System.Collections;

public class ExplorerStates : MonoBehaviour {

    public class Global : AbstractState<BaseUnit> {
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

    public class Idle : GeneralStates.GeneralIdle { }

    public class Dying : GeneralStates.GeneralDying { }
}
