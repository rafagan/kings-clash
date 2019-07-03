using UnityEngine;
using System.Collections;

public static class PassiveStates
{
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
            case IA_Messages.MOVE_TO_ATTACK:
                FSM.ChangeState(new MovingToAttack());
                break;
            case IA_Messages.DYING:
                FSM.ChangeState(new Dying());
                break;
            case IA_Messages.IDLE:
                FSM.ChangeState(new Idle());
                break;
            }
        }
    }

    public class Attacking : GeneralStates.GeneralAttacking {
        protected override IState<BaseUnit> EnemyIsDeadState() {
            return new Idle();
        }

        protected override IState<BaseUnit> EnemyIsOutOfRangeState() {
            return new Idle();
        }
    }

    public class MovingToAttack : GeneralStates.GeneralMovingToAttack {
        protected override IState<BaseUnit> GetIdle() {
            return new Idle();
        }

        protected override IState<BaseUnit> GetAttacking() {
            return new Attacking();
        }

        protected override IState<BaseUnit> GetSearching() {
            return new Idle();
        }
    }

    public class Idle : GeneralStates.GeneralIdle {
        public override string Name {
            get { return "PassiveStates - Idle"; }
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
