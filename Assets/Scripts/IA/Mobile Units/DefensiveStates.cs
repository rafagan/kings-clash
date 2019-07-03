using System.Linq;
using UnityEngine;
using System.Collections;

public static class DefensiveStates
{
    public class Global : AbstractState<BaseUnit> {
        public override string Name {
            get { return ""; }
            set { }
        }

        public override IState<BaseUnit> Process() {
            return this;
        }

        public override void ReceiveMessage(IA_Messages iaMessage) {
            //Caso a unidade for uma estrutura (Tower), testa se a mesma se encontra construída
            if (Structure != null && !Structure.built)
                return;

            switch (iaMessage) {
            case IA_Messages.MOVING:
                if (Entity.UnitType != ObjectType.STRUCTURE) {
                    FSM.ChangeState(new Moving());
                }
                break;
            case IA_Messages.MOVE_TO_ATTACK:
                //Se for uma estrutura, não se move para atacar
                if (Entity.UnitType == ObjectType.STRUCTURE) {
                    FSM.ChangeState(new Attacking());
                    break;
                } FSM.ChangeState(new MovingToAttack());
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

    public class Idle : AbstractState<BaseUnit>
    {
        public override string Name {
            get { return "Idle"; }
            set { }
        }

        public override void Enter() {
            if (MyAnimator != null)
                MyAnimator.LogicState = UnitAnimator.AnimStateType.Idle;
            if(Mobile != null)
                Mobile.StopMoving();
        }

        public override IState<BaseUnit> Process() {
            //O Idle da FSM defensiva faz o papel do Idle e Pursuit da FSM Ofensiva
            if (AttackerRange.EnemiesInFOV.Count > 0) {
                var enemy = AttackerRange.GetNearestEnemy();
                if (Attack.IsInRangeOfAbility(enemy))
                    return new Attacking();
            }
            return this;
        }
    }


    public class Attacking : GeneralStates.GeneralAttacking {
        protected override IState<BaseUnit> EnemyIsDeadState() {
            return new Searching();
        }

        protected override IState<BaseUnit> EnemyIsOutOfRangeState() {
            return new Searching();
        }
    }

    public class Searching : AbstractState<BaseUnit>{
        private const float SECS_TO_SEARCH = 5.0f;
        private float _currentTime;
        private float _nextTime;

        public override string Name {
            get { return "Searching"; }
            set { }
        }

        public override void Enter() {
            _currentTime = Time.time;
            _nextTime = Time.time + SECS_TO_SEARCH;

            if (MyAnimator != null)
                MyAnimator.LogicState = UnitAnimator.AnimStateType.Idle;
        }

        public override IState<BaseUnit> Process() {
            _currentTime = Time.time;

            if (AttackerRange.EnemiesInFOV.Count > 0) {
                var enemy = AttackerRange.GetNearestEnemy();

                if (enemy.IsDead) return this;

                if (Attack.IsInRangeOfAbility(enemy) || Attack.IsInFrontOfMe(enemy)) {
                    Attack.EnemyBeingAttacked = enemy;
                    return new Attacking();
                }
            }

            if (_currentTime > _nextTime)
                return new Idle();
            return this;
        }
    }

    //Atua como um PURSUING de longa distância
    public class MovingToAttack : GeneralStates.GeneralMovingToAttack {
        protected override IState<BaseUnit> GetIdle() {
            return new Idle();
        }

        protected override IState<BaseUnit> GetAttacking() {
            return new Attacking();
        }

        protected override IState<BaseUnit> GetSearching() {
            return new Searching();
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
