using System.Linq;
using UnityEngine;
using System.Collections;

public static class PlasmoGathererStates {

    public class Global : AbstractState<BaseUnit> {
        public override string Name {
            get { return ""; }
            set { }
        }

        public override void Enter() {}

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
                FSM.ChangeState(new SearchingPlasmoRock());
                break;
            case IA_Messages.GATHERING_LOGGED_TREE:
            case IA_Messages.CRUDE_GATHERING_TREE:
                FSM.ChangeState(new TreeGathererStates.SearchingTree());
                break;
            case IA_Messages.DYING:
                FSM.ChangeState(new NoCollectStates.Dying());
                break;
            }
        }
    }

    public class Idle : AbstractState<BaseUnit>{
        public override string Name
        {
            get { return "Idle"; }
            set { }
        }

        public override void Enter(){
            Collector = Entity.GetUnitComponent<CollectorComponent>();
            Collector.CrudeResourceBeingCollected = null;
            Collector.ResourceBeingCollected = null;
            Mobile.StopMoving();

            if (MyAnimator != null)
                MyAnimator.LogicState = UnitAnimator.AnimStateType.Idle;
        }

        public override IState<BaseUnit> Process(){
            return this;
        }

        public override void Leave() { }

        public override void ReceiveMessage(IA_Messages iaMessage){}
    }

    public class SearchingPlasmoRock : AbstractState<BaseUnit> {
        public override string Name
        {
            get { return "Searching PlasmoRock"; }
            set { }
        }

        public override void Enter() {
            if (Collector.ResourceBeingCollected != null) {
                FSM.ChangeState(new SearchingPlasmoCrumb());
                return;
            }

            Mobile.MoveTo(Collector.CrudeResourceBeingCollected.transform.position);
            if (MyAnimator != null)
                MyAnimator.LogicState = UnitAnimator.AnimStateType.Walk;
        }

        public override IState<BaseUnit> Process() {
            var nearest = CollectorRange.NearestPlasmoRock;
            var goal = Collector.CrudeResourceBeingCollected;
            var isInFov = Entity.IsInFOV(goal);

            //Se chegou ao destino e não encontrou nada, redefine um target
            if (Mobile.ReachedDestination())
                return NewStateWhenNotFound();

            //Não esta no range ainda (observe que daqui pra baixo o objeto esta no range!)
            if (!isInFov)
                return this;

            //Casos em que chegou ao destino e o recurso não existe mais
            if (goal.IsDead) {
                return NewStateWhenNotFound();
            }

            //Destino esta no campo de visão e é o mais próximo
            if (nearest == goal) {
                Entity.GetComponent<UnityEngine.AI.NavMeshAgent>().ResetPath();
                return new Crushing();
            }

            //Esta no campo de visão, esta colidindo mas não foi registrado como o mais próximo
            if (nearest != null && nearest != goal) {
                CollectorRange.NearestPlasmoRock = Collector.CrudeResourceBeingCollected;
                return new Crushing();
            }
            return this;
        }

        private IState<BaseUnit> NewStateWhenNotFound() {
            if (CollectorRange.NearestPlasmoCrumb != null)
                return new Collecting(); //Coleta Plasmo
            else if (CollectorRange.NearestPlasmoRock != null)
                return new Crushing(); //Quebra pedra de plasmo
            return new Idle(); //Não tem nada pra fazer, espera novas ordens
        }

        public override void Leave() { }

        public override void ReceiveMessage(IA_Messages iaMessage) {}
    }

    public class SearchingPlasmoCrumb : AbstractState<BaseUnit> {

        public override string Name {
            get { return "Searching PlasmoNode"; }
            set { }
        }

        public override void Enter() {
            Mobile.MoveTo(Collector.ResourceBeingCollected.transform.position);
            if (MyAnimator != null)
                MyAnimator.LogicState = UnitAnimator.AnimStateType.Walk;
        }

        public override IState<BaseUnit> Process() {
            var nearest = CollectorRange.NearestPlasmoCrumb;
            var goal = Collector.ResourceBeingCollected;
            var isInFov = Entity.IsInFOV(goal);

            //Se chegou ao destino e não encontrou nada, redefine um target
            if (Mobile.ReachedDestination())
                return NewStateWhenNotFound();

            //Não esta no range ainda
            if (!isInFov)
                return this;

            //Casos em que chegou ao destino e o recurso não existe mais
            if (goal.IsDead) {
                return NewStateWhenNotFound();
            }

            if (nearest == goal) {
                Entity.GetComponent<UnityEngine.AI.NavMeshAgent>().ResetPath();
                return new Collecting();
            } 
            
            //Esta no campo de visão, esta colidindo mas não foi registrado como o mais próximo
            if (nearest != null && nearest != goal) {
                CollectorRange.NearestPlasmoCrumb = Collector.ResourceBeingCollected;
                return new Collecting();
            }
            return this;
        }

        private IState<BaseUnit> NewStateWhenNotFound() {
            if (CollectorRange.NearestPlasmoCrumb != null)
                return new Collecting(); //Coleta Plasmo
            else if (CollectorRange.NearestPlasmoRock != null)
                return new Crushing(); //Quebra pedra de plasmo
            return new Idle(); //Não tem nada pra fazer, espera novas ordens
        }

        public override void Leave() { }

        public override void ReceiveMessage(IA_Messages iaMessage) {}
    }

    public class Crushing : AbstractState<BaseUnit>{
        private float _timeToCrush, _targetTime = 0.0f;

        public override string Name
        {
            get { return "Crushing"; }
            set { }
        }

        public override void Enter()
        {
            var resource = Collector.CrudeResourceBeingCollected;
            if (resource == null || resource.IsDead)
                resource = Collector.CrudeResourceBeingCollected = CollectorRange.NearestPlasmoRock;

            SetCollecting();
            if (!Collector.IsInRange(resource)) {
                Mobile.MoveTo(resource.transform.position);
            }
        }

        public override IState<BaseUnit> Process() {
            SetCollecting();

            var resource = Collector.CrudeResourceBeingCollected;
            if(resource == null || resource.IsDead) {
                if (CollectorRange.NearestPlasmoCrumb != null)
                    return new Collecting(); //As vezes fica sobrando 1
                if (CollectorRange.NearestPlasmoRock != null) {
                    Collector.CrudeResourceBeingCollected = CollectorRange.NearestPlasmoRock;
                } else
                    return new Idle();
            }

            if (!Collector.IsInRange(Collector.CrudeResourceBeingCollected)) {
                return this;
            }

            _timeToCrush = Time.time;
            if (_timeToCrush > _targetTime) {
                _targetTime = Time.time + Collector.ReloadInSecs;
                Collector.CrudeResourceBeingCollected.GetUnitComponent<CrudeResourceComponent>().Gather();
            }

            return this;
        }

        public override void Leave() { }

        public override void ReceiveMessage(IA_Messages iaMessage){}

        private void SetCollecting() {
            if (CollectorRange.NearestPlasmoCrumb == null) return;
            Mobile.StopMoving();
            FSM.ChangeState(new Collecting());
        }
    }

    public class Collecting : AbstractState<BaseUnit>
    {
        public override string Name
        {
            get { return "Collecting"; }
            set { }
        }

        public override void Enter() {
            var resource = Collector.ResourceBeingCollected;
            if (resource == null || resource.IsDead)
                resource = Collector.ResourceBeingCollected = CollectorRange.NearestPlasmoCrumb;

            if (!Collector.IsInRange(resource)) {
                Mobile.MoveTo(resource.transform.position);
            }
        }

        public override IState<BaseUnit> Process() {
            //Entra aqui caso não existam nodes a serem coletados
            if (Collector.ResourceBeingCollected == null || Collector.ResourceBeingCollected.IsDead) {
                Mobile.StopMoving();

                //Tenta coletar um novo plasmoNode caso exista
                if (Collector.ResourceBeingCollected == null || Collector.ResourceBeingCollected.IsDead)
                    Collector.ResourceBeingCollected = CollectorRange.NearestPlasmoCrumb;
                if (Collector.ResourceBeingCollected != null)
                    return this;

                //Tenta coletar um novo plasmoCrumb caso exista
                if (Collector.CrudeResourceBeingCollected == null || Collector.CrudeResourceBeingCollected.IsDead)
                    Collector.CrudeResourceBeingCollected = CollectorRange.NearestPlasmoRock;
                if (Collector.CrudeResourceBeingCollected != null)
                    return new Crushing();
                return new Idle();
            }

            if (!Collector.IsInRange(Collector.ResourceBeingCollected)) {
                Mobile.MoveTo(Collector.ResourceBeingCollected.transform.position);
            }

            if (Collector.IsInRange(Collector.ResourceBeingCollected)) {
                Collector.ResourceBeingCollected = CollectorRange.NearestPlasmoCrumb;
                var amount = Collector.ResourceBeingCollected.GetUnitComponent<PlasmoComponent>().GatherAll();
                PlayerManager.Player.PlayerResources.AddResource(ResourceType.Plasmo, amount);
            }

            return this;
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
