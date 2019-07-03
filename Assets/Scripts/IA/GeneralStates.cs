using System;
using UnityEngine;
using System.Collections;

public static class GeneralStates {

    public abstract class GeneralMoving : AbstractState<BaseUnit> {
        private bool _isInRange;
        private const int TIME_TO_GET_OUT = 3;
        private float _currentTime;
        protected IState<BaseUnit> Idle; 

        public override string Name {
            get { return "Moving"; }
            set { }
        }

        protected abstract IState<BaseUnit> GetIdle(); 
        public override void Enter() {
            Idle = GetIdle();
            if (MyAnimator != null)
                MyAnimator.LogicState = UnitAnimator.AnimStateType.Walk;
            Mobile.MoveToDestiny();
        }

        public override IState<BaseUnit> Process() {
            if (Mobile.IsInRangeOfFOV(Mobile.Destiny)) {
                _isInRange = true;
                RaycastHit hit;

                var hasHit = Physics.Raycast(Entity.transform.position, Entity.transform.forward, out hit, 8);

                if (hasHit && hit.transform.gameObject.GetComponent<BaseUnit>() != null)
                    return Idle;
            }
            if (_isInRange) {
                _currentTime += Time.deltaTime;
                if (_currentTime > TIME_TO_GET_OUT)
                    return Idle;
            }

            if (Mobile.ReachedDestination())
                return Idle;
            return this;
        }

        public override void Leave() { }

        public override void ReceiveMessage(IA_Messages iaMessage) { }
    }

    public abstract class GeneralBorning : AbstractState<BaseUnit> {
        public override string Name {
            get { return ""; }
            set { }
        }

        protected abstract IState<BaseUnit> GetFirstState(); 
        public override IState<BaseUnit> Process() {
            return GetFirstState();
        }
    }

    public abstract class GeneralIdle : AbstractState<BaseUnit> {
        public override string Name {
            get { return "Idle"; }
            set { }
        }

        public override void Enter() {
            if(Mobile != null)Mobile.StopMoving();

            if (MyAnimator != null) 
                MyAnimator.LogicState = UnitAnimator.AnimStateType.Idle;
        }

        public override IState<BaseUnit> Process() {
            return this;
        }

        public override void Leave() { }

        public override void ReceiveMessage(IA_Messages iaMessage) { }
    }

    public abstract class GeneralDying : AbstractState<BaseUnit> {
        public override string Name {
            get { return "Dying"; }
            set { }
        }

        public override void Enter() {
            if (MyAnimator != null)
                MyAnimator.LogicState = UnitAnimator.AnimStateType.Dead;
            Debug.Log("Morri");
        }
    }

    public abstract class GeneralAttacking : AbstractState<BaseUnit> {
        private float _timeToAttack, _targetTime;
        private IState<BaseUnit> _nextState;

        public override string Name {
            get { return "Attacking"; }
            set { }
        }

        public override void Enter() {
            if (Attack.EnemyBeingAttacked == null)
                Attack.EnemyBeingAttacked = AttackerRange.GetNearestEnemy();

            if (MyAnimator != null) {
                MyAnimator.LogicState = UnitAnimator.AnimStateType.Attack;
            }
            if(Mobile != null) 
                Mobile.StopMoving();
        }

        protected abstract IState<BaseUnit> EnemyIsDeadState(); 
        protected abstract IState<BaseUnit> EnemyIsOutOfRangeState(); 
        public override IState<BaseUnit> Process() {
            Debug.DrawRay(Entity.transform.position,
                Attack.EnemyBeingAttacked.transform.position - Entity.transform.position,Color.magenta);

            //Retorna novo estado apenas se não existir animator, 
            //ou se a animação de ataque não estiver mais rolando
            if (_nextState != null)
//                if (MyAnimator == null || !MyAnimator.IsAttacking) //Teste do Breno 2
                    return _nextState;

            if (Attack.EnemyBeingAttacked.IsDead)
                _nextState = EnemyIsDeadState();
            else if (!Attack.IsInRangeOfAbility(Attack.EnemyBeingAttacked))
                if (!Attack.IsInFrontOfMe(Attack.EnemyBeingAttacked))
                    _nextState = EnemyIsOutOfRangeState();

            _timeToAttack = Time.time;
            if (_timeToAttack > _targetTime) {
//                if (MyAnimator != null && !MyAnimator.NullState) //Teste do Breno 1
//                    return this;

                MyAnimator.LogicState = UnitAnimator.AnimStateType.Attack;
                _targetTime = Time.time + Attack.ReloadTime;
                Attack.PrepareAttack(Attack.EnemyBeingAttacked);
            }

            return this;
        }

        public override void Leave() {
            Attack.EnemyBeingAttacked = null;
            Attack.AttackPoint = Vector3.zero;
        }

        public override void ReceiveMessage(IA_Messages iaMessage) { }
    }

    //Atua como um PURSUING de longa distância
    public abstract class GeneralMovingToAttack : AbstractState<BaseUnit> {
        private Vector3 _currentDestiny;
        public override string Name {
            get { return "Moving To Attack"; }
            set { }
        }

        protected abstract IState<BaseUnit> GetIdle();
        protected abstract IState<BaseUnit> GetAttacking();
        protected abstract IState<BaseUnit> GetSearching();

        public override void Enter() {
            if (Attack.EnemyBeingAttacked != null) {
                _currentDestiny = Attack.EnemyBeingAttacked.transform.position;
            } else if (Attack.AttackPoint != Vector3.zero) {
                _currentDestiny = Attack.AttackPoint;
            } else {
                FSM.ChangeState(GetIdle());
                return;
            }

            if (MyAnimator != null)
                MyAnimator.LogicState = UnitAnimator.AnimStateType.Walk;
            if (Mobile != null)
                Mobile.MoveTo(_currentDestiny);
        }

        public override IState<BaseUnit> Process() {
            if (Attack.AttackPoint == Vector3.zero)
                return GetIdle();

            if (AttackerRange.EnemiesInFOV.Count > 0) {
                if (Attack.IsInRangeOfAbility(_currentDestiny) || Attack.IsInFrontOfMe(Attack.EnemyBeingAttacked))
                    return GetAttacking();
                if (Mobile.ReachedDestination())
                    return GetSearching();
            }
            return this;
        }

        public override void Leave() { }

        public override void ReceiveMessage(IA_Messages iaMessage) { }
    }
}
