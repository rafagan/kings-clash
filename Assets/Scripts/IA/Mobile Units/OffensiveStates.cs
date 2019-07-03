using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections;

public static class OffensiveStates {

    public class Global : AbstractState<BaseUnit> {
        public override string Name {
            get { return ""; }
            set { }
        }

        public override IState<BaseUnit> Process() {
            return this;
        }

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
            }
        }
    }

    public class Idle : AbstractState<BaseUnit> {
        public override string Name{
            get { return "Idle"; }
            set { }
        }

        public override void Enter(){
            Mobile.StopMoving();
            if (MyAnimator != null)
                MyAnimator.LogicState = UnitAnimator.AnimStateType.Idle;
        }

        public override IState<BaseUnit> Process() {
            //Se estão me atacando, mas eu não vejo quem é...
            //Devo partir para o Searching, mas como eu testo isso?

            if (Entity.GetUnitComponent<Debugger>().IA_DebugEnabled) {
                if (AttackerRange.GetNearestEnemy() != null) {
                    Debug.Log(AttackerRange.GetNearestEnemy());
                    Debug.Log(AttackerRange.GetNearestEnemy().IsDead);
                    Debug.Log(AttackerRange.GetNearestEnemy().gameObject.activeInHierarchy);
                }
            }

            if (AttackerRange.EnemiesInFOV.Count > 0)
                return new Pursuing();

            return this;
        }
    }

    public class Attacking : GeneralStates.GeneralAttacking {
        protected override IState<BaseUnit> EnemyIsDeadState() {
            return new Searching();
        }

        protected override IState<BaseUnit> EnemyIsOutOfRangeState() {
            return new Pursuing();
        }
    }

    //O personagem somente entrará nesse estado caso esteja em Idle ou Searching e em seguida aviste um inimigo
    public class Pursuing : AbstractState<BaseUnit> {
        private float _timeToWalk = 1.0f;
        private float _inertia = 5.0f;
        private const float TIME_INERTIA = 5.0f;
        private BaseUnit _enemy;

        public override string Name
        {
            get { return "Pursuing"; }
            set { }
        }

        public override void Enter()
        {
            var enemiesInFov = AttackerRange.EnemiesInFOV;

            if (enemiesInFov.Count == 0) {
                FSM.ChangeState(new Searching());
                return;
            }
            _enemy = AttackerRange.GetNearestEnemy();

//            if (MyAnimator != null)
//                MyAnimator.LogicState = UnitAnimator.AnimStateType.Walk;
        }

        public override IState<BaseUnit> Process() {
            if (_enemy == null || _enemy.IsDead) {
                return new Searching();
            }

            if (Attack.IsInRangeOfAbility(_enemy) || Attack.IsInFrontOfMe(_enemy)) {
                Attack.EnemyBeingAttacked = AttackerRange.GetNearestEnemy();
                return new Attacking();
            }

            if (_timeToWalk < 1) {
                _timeToWalk += Time.deltaTime;
                return this;
            }
            _timeToWalk = 0;

            Mobile.MoveTo(_enemy.transform.position);
            
            _inertia -= Time.deltaTime;
            if(_inertia < 0) {
                _inertia = TIME_INERTIA;
                _enemy = AttackerRange.EnemiesInFOV.Keys.ToList()[1] ?? AttackerRange.GetNearestEnemy();
            }
            return this;
        }

        public override void Leave() {}
        public override void ReceiveMessage(IA_Messages iaMessage) { }
    }

    public class Searching : AbstractState<BaseUnit>{
        private const float SECS_TO_SEARCH = 5.0f;
        private float _currentTime;
        private float _nextTime;

        public override string Name
        {
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
                } else {
                    return new Pursuing();
                }
            }
            if(_currentTime > _nextTime)
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
