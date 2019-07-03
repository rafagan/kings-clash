using System;
using System.Linq;
using UnityEngine;

public static class BuildRepairStates {
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
            case IA_Messages.BUILD:
                FSM.ChangeState(new SearchingToBuild());
                break;
            case IA_Messages.REPAIR:
                FSM.ChangeState(new SearchingToRepair());
                break;
            case IA_Messages.DYING:
                FSM.ChangeState(new Dying());
                break;
            }
        }
    }

    public class Borning : AbstractState<BaseUnit> {
        public override string Name {
            get { return ""; }
            set { }
        }

        public override IState<BaseUnit> Process() {
            return new Idle();
        }
    }

    public class Idle : AbstractState<BaseUnit> {
        public override string Name {
            get { return "Idle"; }
            set { }
        }

        public override void Enter() {
            Mobile.StopMoving();

            if (MyAnimator != null) {
                MyAnimator.LogicState = UnitAnimator.AnimStateType.Idle;
            }
        }

        public override IState<BaseUnit> Process() {
            return this;
        }

        public override void Leave() { }

        public override void ReceiveMessage(IA_Messages iaMessage) { }
    }

    public class Moving : GeneralStates.GeneralMoving {
        protected override IState<BaseUnit> GetIdle() {
            return new Idle();
        }
    }

    public class Searching : AbstractState<BaseUnit> {
        public override string Name {
            get { return "Searching Structure"; }
            set { }
        }

        public override void Enter() {}

        public override IState<BaseUnit> Process() {
            var structure = BuilderRange.GetNearestStructure();
            if (structure != null) {
                if (structure.GetUnitComponent<StructureComponent>().built) {
                    Constructor.StructureBeingRepaired = structure;
                    return new SearchingToRepair();
                }
                Constructor.StructureBeingBuilt = structure;
                return new SearchingToBuild();
            }
            return new Idle();
        }

        public override void Leave() { }
        public override void ReceiveMessage(IA_Messages iaMessage) { }
    }

    public class SearchingToBuild : AbstractState<BaseUnit> {
        public override string Name {
            get { return "Searching to Build"; }
            set { }
        }

        public override void Enter() {
            Mobile.MoveTo(Constructor.StructureBeingBuilt.transform.position);
        }

        public override IState<BaseUnit> Process() {
            if (Mobile.ReachedDestination())
                return new Searching();
            if (Constructor.IsInRangeOfBuild(Constructor.StructureBeingBuilt)) {
                if (Constructor.StructureBeingBuilt.IsDead)
                    return new Searching();
                return new Building();
            }

            return this;
        }

        public override void Leave() { }
        public override void ReceiveMessage(IA_Messages iaMessage) {}
    }

    public class SearchingToRepair : AbstractState<BaseUnit> {
        public override string Name {
            get { return "Searching To Repair"; }
            set { }
        }

        public override void Enter() {
            Mobile.MoveTo(Constructor.StructureBeingRepaired.transform.position);
        }

        public override IState<BaseUnit> Process() {
            if (Mobile.ReachedDestination())
                return new Searching();
            if (Constructor.IsInRangeOfBuild(Constructor.StructureBeingRepaired)) {
                if (Constructor.StructureBeingRepaired.IsDead)
                    return new Searching();
                return new Repairing();
            }

            return this;
        }

        public override void Leave() { }
        public override void ReceiveMessage(IA_Messages iaMessage) { }
    }

    public class Building : AbstractState<BaseUnit> {
        private AttributesComponent _structureAtt;
        private StructureComponent _structure;

        public override string Name {
            get { return "Building"; }
            set { }
        }

        public override void Enter() {
            Mobile.StopMoving();

            AbilityLibrary.SelectAbilityWithTarget(AbilityLibrary.GetAbility<AbilityBuildStructure>(), Constructor.StructureBeingBuilt);
            _structureAtt = Constructor.StructureBeingBuilt.GetUnitComponent<AttributesComponent>();
            _structure = Constructor.StructureBeingBuilt.GetUnitComponent<StructureComponent>();
            _structure.AddBuildUnit(Entity);
        }

        public override IState<BaseUnit> Process() {
            if (Math.Abs(_structureAtt.CurrentLife - _structureAtt.MaxLife) < Mathf.Epsilon) {
                return new Searching();
            }
            return this;
        }

        public override void Leave() {
            _structure.RemoveBuildUnit(Entity);
            Constructor.StructureBeingBuilt = null;
        }
        public override void ReceiveMessage(IA_Messages iaMessage) { }
    }

    public class Repairing : AbstractState<BaseUnit> {
        private AttributesComponent _structureAtt;
        private StructureComponent _structure;

        public override string Name {
            get { return "Repairing"; }
            set { }
        }

        public override void Enter() {
            Mobile.StopMoving();
            AbilityLibrary.SelectAbilityWithTarget(AbilityLibrary.GetAbility<AbilityBuildStructure>(), Constructor.StructureBeingBuilt);
            _structureAtt = Constructor.StructureBeingRepaired.GetUnitComponent<AttributesComponent>();
            _structure = Constructor.StructureBeingRepaired.GetUnitComponent<StructureComponent>();
            _structure.AddBuildUnit(Entity);
        }

        public override IState<BaseUnit> Process() {
            if (Math.Abs(_structureAtt.CurrentLife - _structureAtt.MaxLife) < Mathf.Epsilon) {
                return new Searching();
            }

            return this;
        }

        public override void Leave() {
            _structure.RemoveBuildUnit(Entity);
            Constructor.StructureBeingRepaired = null;
        }
        public override void ReceiveMessage(IA_Messages iaMessage) { }
    }

    public class Dying : AbstractState<BaseUnit> {
        public override string Name {
            get { return "Dying"; }
            set { }
        }

        public override void Enter() { }
        public override void Leave() { }
        public override void ReceiveMessage(IA_Messages iaMessage) { }

        public override IState<BaseUnit> Process() {
            return this;
        }
    }
}
