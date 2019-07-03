using UnityEngine;
using System.Collections;

public static class TreeGathererStates {

    public class Global : AbstractState<BaseUnit> {
        public Vector3 ReturnPoint;

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
            if (iaMessage == IA_Messages.MOVING) {
                FSM.ChangeState(new Moving());
                return;
            }

            switch (iaMessage) {
            case IA_Messages.NODE_GATHERING_PLASMO:
            case IA_Messages.CRUDE_GATHERING_PLASMO:
                FSM.ChangeState(new PlasmoGathererStates.SearchingPlasmoRock());
                break;
            case IA_Messages.CRUDE_GATHERING_TREE:
            case IA_Messages.GATHERING_LOGGED_TREE:
                FSM.ChangeState(new SearchingTree());
                break;
            case IA_Messages.STOCK:
                FSM.ChangeState(new Stocking());
                break;
            case IA_Messages.DYING:
                FSM.ChangeState(new NoCollectStates.Dying());
                break;
            }
        }
    }

    //Estado utilizado apenas como listener de novos estados a serem iniciados
    public class Idle : AbstractState<BaseUnit> {
        public override string Name {
            get { return "Idle"; }
            set { }
        }

        public override void Enter() {
            Collector.CrudeResourceBeingCollected = null;
            Collector.ResourceBeingCollected = null;
            Mobile.StopMoving();

            if (MyAnimator != null)
                MyAnimator.LogicState = UnitAnimator.AnimStateType.Idle;
        }

        public override IState<BaseUnit> Process() {
            return this;
        }

        public override void Leave() { }

        public override void ReceiveMessage(IA_Messages iaMessage) {}
    }

    public class SearchingTree : AbstractState<BaseUnit> {
        public override string Name 
        {
            get { return "Searching Tree"; }
            set { }
        }

        public override void Enter() {
            //Se estiver com as mãos ocupadas, primeiro leva embora!
            if (Collector.MyLumber && Collector.MyLumber.gameObject.activeInHierarchy) {
                FSM.ChangeState(new Stocking());
                return;
            }

            //Se estiver coletando um node, troca de buscador
            if (Collector.ResourceBeingCollected != null) {
                FSM.ChangeState(new SearchingLoggedTree());
                return;
            }
            Mobile.MoveTo(Collector.CrudeResourceBeingCollected.transform.position);
        }

        public override IState<BaseUnit> Process() {
            var nearest = CollectorRange.NearestTree;
            var goal = Collector.CrudeResourceBeingCollected;
            var isInFov = Entity.IsInFOV(goal);

            //Se chegou ao destino e não encontrou nada, redefine um target
            if (Mobile.ReachedDestination())
                return NewStateWhenNotFound();

            //Não esta no range ainda (observe que daqui pra baixo o objeto esta no range!)
            if (!isInFov)
                return this;

            //Casos em que chegou ao destino e o recurso não existe mais
            if (goal.IsDead)
                return NewStateWhenNotFound();

            //Destino esta no campo de visão e é o mais próximo
            if (nearest == goal) {
                Mobile.StopMoving();
                return new CuttingDown();
            }
            
            //Esta no campo de visão, esta colidindo mas não foi registrado como o mais próximo
            if (nearest != null && nearest != goal) {
                CollectorRange.NearestTree = Collector.CrudeResourceBeingCollected;
                return new CuttingDown();
            }
            return this;
        }

        private IState<BaseUnit> NewStateWhenNotFound() {
            if (CollectorRange.NearestLoggedTree != null)
                return new Gathering(); //Coleta LoggedTree
            else if (CollectorRange.NearestTree != null)
                return new CuttingDown(); //Derruba Tree
            return new Idle(); //Não tem nada pra fazer, espera novas ordens
        }

        public override void Leave() { }

        public override void ReceiveMessage(IA_Messages iaMessage) { }
    }

    public class SearchingLoggedTree : AbstractState<BaseUnit> {
        private BaseUnit _goal;

        public override string Name {
            get { return "Searching LoggedTree"; }
            set { }
        }

        public override void Enter() {
            Mobile.MoveTo(Collector.ResourceBeingCollected.transform.position);
            _goal = Collector.ResourceBeingCollected;
        }

        public override IState<BaseUnit> Process() {
            var nearest = CollectorRange.NearestLoggedTree;
            var isInFov = Entity.IsInFOV(_goal);

            //Se chegou ao destino e não encontrou nada, redefine um target
            if (Mobile.ReachedDestination())
                return NewStateWhenNotFound();

            //Não esta no range ainda
            if (!isInFov)
                return this;

            //Esta no range, porém não existe mais
            if (_goal.IsDead) {
                return NewStateWhenNotFound();
            }
            if (nearest == _goal) {
                Mobile.StopMoving();
                return new Gathering();
            }

            //Esta no campo de visão, esta colidindo mas não foi registrado como o mais próximo
            if (nearest != null && nearest != _goal) {
                CollectorRange.NearestLoggedTree = Collector.ResourceBeingCollected;
                return new Gathering();
            }
            return this;
        }

        private IState<BaseUnit> NewStateWhenNotFound() {
            if (CollectorRange.NearestLoggedTree != null)
                return new Gathering(); //Coleta LoggedTree
            else if (CollectorRange.NearestTree != null)
                return new CuttingDown(); //Derruba Tree
            return new Idle(); //Não tem nada pra fazer, espera novas ordens
        }

        public override void Leave() { }

        public override void ReceiveMessage(IA_Messages iaMessage) { }
    }

    public class CuttingDown : AbstractState<BaseUnit> {
        private float _timeToCut, _targetTime = 0.0f;

        public override string Name {
            get { return "CuttingDown"; }
            set { }
        }

        public override void Enter() {
            var resource = Collector.CrudeResourceBeingCollected;
            if (resource == null || resource.IsDead) {
                resource = Collector.CrudeResourceBeingCollected = CollectorRange.NearestTree;
                if (resource == null || resource.IsDead)
                    FSM.ChangeState(new Idle());
            }
            SetCollecting();
            if (!Collector.IsInRange(resource))
                Mobile.MoveTo(Collector.CrudeResourceBeingCollected.transform.position);
        }

        public override IState<BaseUnit> Process() {
            SetCollecting();

            var resource = Collector.CrudeResourceBeingCollected;
            if (resource == null || resource.IsDead) {
                if (CollectorRange.NearestLoggedTree != null)
                    return new Gathering(); //As vezes fica sobrando 1
                if (CollectorRange.NearestTree != null) {
                    Collector.CrudeResourceBeingCollected = CollectorRange.NearestTree;
                } else
                    return new Idle();
            }

            if (!Collector.IsInRange(Collector.CrudeResourceBeingCollected)) {
                return this;
            }

            _timeToCut = Time.time;
            if (_timeToCut > _targetTime) {
                _targetTime = Time.time + Collector.ReloadInSecs;
                Collector.CrudeResourceBeingCollected.GetUnitComponent<CrudeResourceComponent>().Gather();
            }

            return this;
        }

        private void SetCollecting() {
            if (CollectorRange.NearestLoggedTree == null) return;
            Mobile.StopMoving();
            FSM.ChangeState(new Gathering());
        }

        public override void Leave() { }

        public override void ReceiveMessage(IA_Messages iaMessage) { }
    }

    public class Gathering : AbstractState<BaseUnit> {

        public override string Name {
            get { return "Gathering"; }
            set { }
        }

        public override void Enter() {
            var resource = Collector.ResourceBeingCollected;
            if (resource == null || resource.IsDead) {
                resource = Collector.ResourceBeingCollected = CollectorRange.NearestLoggedTree;
                if (resource == null || resource.IsDead)
                    FSM.ChangeState(new Idle());
            }

            if (!Collector.IsInRange(resource)) {
                Mobile.MoveTo(Collector.ResourceBeingCollected.transform.position);
            }
        }

        public override IState<BaseUnit> Process() {
            if (Collector.ResourceBeingCollected == null || Collector.ResourceBeingCollected.IsDead) {
                Mobile.StopMoving();

                if (Collector.MyLumber != null) {
                    return new Stocking();
                }

                //Tenta coletar uma nova árvore derrubada caso exista
                if (Collector.ResourceBeingCollected == null || Collector.ResourceBeingCollected.IsDead)
                    Collector.ResourceBeingCollected = CollectorRange.NearestLoggedTree;
                if (Collector.ResourceBeingCollected != null)
                    return this;

                //Tenta derrubar uma nova árvore caso exista
                if (Collector.CrudeResourceBeingCollected == null || Collector.CrudeResourceBeingCollected.IsDead)
                    Collector.CrudeResourceBeingCollected = CollectorRange.NearestPlasmoRock;
                if (Collector.CrudeResourceBeingCollected != null)
                    return new CuttingDown();
                return new Idle();
            }

            //Se parou de se mover e ainda não alcançou o destino, reenvia a ordem de movimentação
            if (!Collector.IsInRange(Collector.ResourceBeingCollected)) {
                Mobile.MoveTo(Collector.ResourceBeingCollected.transform.position);
            }

            if (Collector.IsInRange(Collector.ResourceBeingCollected))
                return new Stocking();
            return this;
        }

        public override void Leave() { }

        public override void ReceiveMessage(IA_Messages iaMessage) { }
    }

    public class Stocking : AbstractState<BaseUnit> {
        private BaseUnit _dropPoint;
        private bool _stopCollect;

        public override string Name
        {
            get { return "Stocking"; }
            set { }
        }

        public override void Enter() {
            ((Global)FSM.GlobalState).ReturnPoint = Entity.transform.position;

            if (Collector.DropPointInUse != null) {
                _dropPoint = Collector.DropPointInUse;
                _stopCollect = true;
            } else {
                SetDropPoint();
                Mobile.MoveTo(Collector.DropPointInUse.transform.position);
                Collector.MyLumber.gameObject.SetActive(true);

                if (Collector.ResourceBeingCollected != null) {
                    var loggedTree = Collector.ResourceBeingCollected.GetUnitComponent<PoolItemComponent>();
                    loggedTree.MyPoolManager.Despawn(loggedTree);
                }
            }

            if (Collector.DropPointInUse == null) {
                FSM.ChangeState(new Idle());
                return;
            }

            Collector.ResourceBeingCollected = null;
            Collector.CrudeResourceBeingCollected = null;
        }

        public override IState<BaseUnit> Process() {
            if (Mobile.ReachedDestination() || _dropPoint == null) {
                SetDropPoint();
                Mobile.MoveTo(Collector.DropPointInUse.transform.position);
            }

            if (DropPointIsNext()) {
                Collector.MyLumber.Stock();
                return !_stopCollect ? (IState<BaseUnit>)new GoingBackToGatherPoint() : new NoCollectStates.Idle();
            }

            var isInFov = Entity.IsInFOV(_dropPoint);
            if (!isInFov)
                return this;

            var isInRange = Collector.IsInRange(_dropPoint);
            if (!isInRange)
                return this;

            Collector.MyLumber.Stock();
            return !_stopCollect ? (IState<BaseUnit>) new GoingBackToGatherPoint() : new NoCollectStates.Idle();
        }

        public override void Leave() {
            Collector.DropPointInUse = null;
        }

        public override void ReceiveMessage(IA_Messages iaMessage) {}

        private void SetDropPoint() {
            _dropPoint = DropPointManager.GetNearestDropPoint(Entity.transform.position);
            if(_dropPoint == null) return;
            Collector.DropPointInUse = _dropPoint;
        }

        private bool DropPointIsNext() {
            var direction = _dropPoint.transform.position - Entity.transform.position;
            RaycastHit hit;
            var ray = Physics.Raycast(Entity.transform.position, direction, out hit, Collector.GatherRange);
            return ray && hit.transform && hit.transform.gameObject == _dropPoint.gameObject;
        }
    }

    public class GoingBackToGatherPoint : AbstractState<BaseUnit> {
        private BaseUnit _dropPoint;
        private Vector3 _returnPoint;

        public override string Name {
            get { return "GoingBackToGatherPoint"; }
            set { }
        }

        public override void Enter() {
            _returnPoint = ((Global)FSM.GlobalState).ReturnPoint;
            Mobile.MoveTo(_returnPoint);
        }

        public override IState<BaseUnit> Process() {
            var isInFov = Entity.IsInFOV(_returnPoint);
            if (!isInFov)
                return this;

            if (CollectorRange.NearestLoggedTree != null) {
                Collector.ResourceBeingCollected = CollectorRange.NearestLoggedTree;
                return new Gathering(); //Coleta LoggedTree
            } else if (CollectorRange.NearestTree != null) {
                Collector.CrudeResourceBeingCollected = CollectorRange.NearestTree;
                return new CuttingDown(); //Derruba Tree
            }

            var isInRange = Collector.IsInRange(_returnPoint) && Mobile.ReachedDestination();
            if (!isInRange)
                return this;

            return new Idle(); //Não tem nada pra fazer, espera novas ordens
        }

        public override void Leave() {
            ((Global)FSM.GlobalState).ReturnPoint = Vector3.zero;
        }

        public override void ReceiveMessage(IA_Messages iaMessage) { }
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
