using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections;

public class BuilderRangeView : AbstractUnitComponent {
    private BaseUnit _nearestStructure;
    public CapsuleCollider Collider;
    private AttributesComponent _attributes;

    private class StructuresOrderedByLife : IComparer<BaseUnit> {
        private readonly BaseUnit _gm;

        public StructuresOrderedByLife(BaseUnit gm) {
            _gm = gm;
        }

        public int Compare(BaseUnit x, BaseUnit y) {
            var xIsBuilt = x.GetUnitComponent<StructureComponent>().built;
            var yIsBuilt = y.GetUnitComponent<StructureComponent>().built;

            //Da prioridade para quem não esta construído
            if(xIsBuilt && !yIsBuilt)
                return 1;
            if (yIsBuilt && !xIsBuilt)
                return -1;

            var lifeX = x.GetUnitComponent<AttributesComponent>().CurrentLife;
            var lifeY = y.GetUnitComponent<AttributesComponent>().CurrentLife;

            //Da prioridade para quem tem menor vida
            if (lifeX < lifeY)
                return -1;
            if (lifeX > lifeY)
                return 1;

            var distX = _gm.transform.position - x.transform.position;
            var distY = _gm.transform.position - y.transform.position;

            //Da prioridade para quem esta mais próximo
            if (distX.sqrMagnitude < distY.sqrMagnitude)
                return -1;
            if (distX.sqrMagnitude > distY.sqrMagnitude)
                return 1;
            return 0;
        }
    }

    //.NET 3.5 / Mono 2.6 não possui suporte a SortedSet, então tive
    //Que criar um SortedDictionary com valores dummy
    public SortedDictionary<BaseUnit, bool> StructuresInFOV;

    void Awake() {
        Collider = gameObject.GetComponent<CapsuleCollider>();
    }

    void Start() {
        StructuresInFOV = new SortedDictionary<BaseUnit, bool>(new StructuresOrderedByLife(baseUnit));
        _attributes = baseUnit.GetUnitComponent<AttributesComponent>();

        Collider.radius = _attributes.FieldOfView;
        Collider.height = _attributes.HeightOfView;
    }

    void Update() {
        if (_nearestStructure == null)
            return;

        if (_nearestStructure.IsDead) {
            StructuresInFOV.Remove(_nearestStructure);
            UpdateNearestStructure();
            return;
        }

        var att = _nearestStructure.GetUnitComponent<AttributesComponent>();
        if (Math.Abs(att.CurrentLife - att.MaxLife) < Mathf.Epsilon) {
            StructuresInFOV.Remove(_nearestStructure);
            UpdateNearestStructure();
            return;
        }
    }

    void OnTriggerEnter(Collider collided) {
        var bu = collided.gameObject.GetComponent<BaseUnit>();
        if (bu == null)
            return;

        if (!ValidadeCollision(bu))
            return;

        AddStructure(bu);
        _nearestStructure = bu;
    }

    void OnTriggerExit(Collider collided) {
        var bu = collided.gameObject.GetComponent<BaseUnit>();
        if (bu == null)
            return;

        if (!ValidadeCollision(bu))
            return;

        StructuresInFOV.Remove(bu);
        UpdateNearestStructure();
    }

    public void UpdateNearestStructure() {
        if (StructuresInFOV.Count > 0)
            _nearestStructure = StructuresInFOV.Keys.First();
        else
            _nearestStructure = null;
    }

    private bool ValidadeCollision(BaseUnit bu) {
        var attributes = bu.GetUnitComponent<AttributesComponent>();
        if (attributes == null)
            return false;

        if (attributes.UnitType != ObjectType.STRUCTURE)
            return false;

        //Testa se as unidades são do mesmo time ou não
        if (bu.UnitTeam != baseUnit.UnitTeam)
            return false;

        //Se tiver com a vida cheia, não entra na lista
        if (Math.Abs(attributes.CurrentLife - attributes.MaxLife) < Mathf.Epsilon)
            return false;

        return true;
    }

    public BaseUnit GetNearestStructure() {
        return _nearestStructure;
    }
    public List<KeyValuePair<BaseUnit, bool>> GetStructures() {
        return StructuresInFOV.AsEnumerable().ToList();
    } 

    private void AddStructure(BaseUnit bu) {
        try {
            StructuresInFOV.Add(bu, false);
        #pragma warning disable 168
        } catch (ArgumentException e) { }
        #pragma warning restore 168
    }

    public override void GUIPriority() {
        
    }

    public override void UserInputPriority() {
       
    }

    public override void Reset() {
        
    }
}
