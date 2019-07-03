using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections;

public class AttackerRangeView : AbstractUnitComponent {
    private BaseUnit _nearestEnemy;
    public CapsuleCollider Collider;
    private AttackComponent _attack;
    private AttributesComponent _attributes;

    private class EnemiesOrderedByPos : IComparer<BaseUnit> {
        private readonly BaseUnit _gm;

        public EnemiesOrderedByPos(BaseUnit gm) {
            _gm = gm;
        }

        public int Compare(BaseUnit x, BaseUnit y) {
            var distX = _gm.transform.position - x.transform.position;
            var distY = _gm.transform.position - y.transform.position;

            if (x.ThreatLevel > y.ThreatLevel)
                return -1;
            if (x.ThreatLevel < y.ThreatLevel)
                return 1;
            if (x.ThreatLevel == y.ThreatLevel)
                if (distX.sqrMagnitude < distY.sqrMagnitude)
                    return -1;
            if (distX.sqrMagnitude > distY.sqrMagnitude)
                return 1;
            return 0;
        }
    }

    //.NET 3.5 / Mono 2.6 não possui suporte a SortedSet, então tive
    //Que criar um SortedDictionary com valores dummy
    public SortedDictionary<BaseUnit, bool> EnemiesInFOV;

    void Awake() {
        Collider = gameObject.GetComponent<CapsuleCollider>();
    }

    void Start() {
        EnemiesInFOV = new SortedDictionary<BaseUnit, bool>(new EnemiesOrderedByPos(baseUnit));

        _attack = baseUnit.GetUnitComponent<AttackComponent>();
        _attributes = baseUnit.GetUnitComponent<AttributesComponent>();

        Collider.radius = _attributes.FieldOfView;
        Collider.height = _attributes.HeightOfView;
    }

    void OnTriggerStay(Collider collided) {
        var bu = collided.gameObject.GetComponent<BaseUnit>();
        if (bu == null) return;

        if(bu.UnitType != ObjectType.CHARACTER && bu.UnitType != ObjectType.STRUCTURE) return;
        if (!baseUnit.CheckIfIsMyEnemy(bu)) return;

        var isDead = (bu.IsDead || !bu.gameObject.activeInHierarchy);
        if (isDead) {
            RemoveEnemy(bu);
            return;
        }

        var att = bu.GetUnitComponent<AttributesComponent>();
        if(att.IsInvisible) {
            RemoveEnemy(bu);
            return;
        }

        if (att.IsFlying && !_attack.CanAttackFlyingUnities) {
            RemoveEnemy(bu);
            return;
        }

        if (!att.IsFlying && !att.IsInvisible) {
            AddEnemy(bu);
        }
    }

    void OnTriggerEnter(Collider collided) {
        var bu = collided.gameObject.GetComponent<BaseUnit>();
        if (bu == null) return;

        if (!ValidadeCollision(bu)) return;

        AddEnemy(bu);
    }

    void OnTriggerExit(Collider collided) {
        var bu = collided.gameObject.GetComponent<BaseUnit>();
        if (bu == null)
            return;

        RemoveEnemy(bu);
    }

    private void UpdateNearestEnemy() {
        _nearestEnemy = EnemiesInFOV.Count > 0 ? EnemiesInFOV.Keys.First() : null;
    }

    private bool ValidadeCollision(BaseUnit bu) {
        var attributes = bu.GetUnitComponent<AttributesComponent>();
        if (attributes == null)
            return false;

        if (!(attributes.UnitType == ObjectType.CHARACTER || attributes.UnitType == ObjectType.STRUCTURE))
            return false;

        //Testa se as unidades são do mesmo time ou não
        if (!baseUnit.CheckIfIsMyEnemy(bu)) return false;

        if (attributes.IsInvisible) return false;

        if (attributes.IsFlying && !_attack.CanAttackFlyingUnities) return false;

        var isDead = (bu.IsDead || !bu.gameObject.activeInHierarchy);
        if (isDead) return false;

        return true;
    }

    public BaseUnit GetNearestEnemy() {
        return _nearestEnemy;
    }

    public void AddEnemy(BaseUnit bu) {
        try {
            EnemiesInFOV.Add(bu, false);
        #pragma warning disable 168
        } catch (ArgumentException e) { }
        #pragma warning restore 168
        UpdateNearestEnemy();
    }

    private void RemoveEnemy(BaseUnit bu) {
        var before = EnemiesInFOV.Count;
        EnemiesInFOV.Remove(bu);
        var after = EnemiesInFOV.Count;

        if (before > after) UpdateNearestEnemy();
    }

    public override void GUIPriority() {
        
    }

    public override void UserInputPriority() {
        
    }

    public override void Reset() {
        
    }
}
