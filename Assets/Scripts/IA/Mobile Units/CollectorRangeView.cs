using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections;

public class CollectorRangeView : AbstractUnitComponent {
    public CapsuleCollider Collider;
    private AttributesComponent _attributes;

    private class ResourceOrderedByPos : IComparer<GameObject> {
        private GameObject _gm;
        private CollectorComponent _collector;

        public ResourceOrderedByPos(GameObject gm, CollectorComponent collector) {
            _gm = gm;
            _collector = collector;
        }

        public int Compare(GameObject x, GameObject y) {
            var distX = _gm.transform.position - x.transform.position;
            var distY = _gm.transform.position - y.transform.position;

            if (distX.sqrMagnitude < distY.sqrMagnitude || _collector.CrudeResourceBeingCollected == x)
                return -1;
            else if (distX.sqrMagnitude > distY.sqrMagnitude)
                return 1;
            return 0;
        }
    }

    //.NET 3.5 / Mono 2.6 não possui suporte a SortedSet, então tive que criar um SortedDictionary com valores dummy
    // ReSharper disable InconsistentNaming
    public SortedDictionary<GameObject,bool> PlasmoCrumbsInFOV;
    public SortedDictionary<GameObject, bool> TreesInFOV;
    public SortedDictionary<GameObject, bool> LoggedTreesInFOV;
    public SortedDictionary<GameObject, bool> PlasmoInFOV;
    // ReSharper restore InconsistentNaming

    public BaseUnit NearestPlasmoRock {
        get { return _nearestPlasmoRock; }
        set { _nearestPlasmoRock = value; }
    }
    public BaseUnit NearestLoggedTree {
        get { return _nearestLoggedTree; }
        set { _nearestLoggedTree = value; }
    }
    public BaseUnit NearestTree {
        get { return _nearestTree; }
        set { _nearestTree = value; }
    }
    public BaseUnit NearestPlasmoCrumb {
        get { return _nearestPlasmoCrumb; }
        set { _nearestPlasmoCrumb = value; }
    }

    private BaseUnit _nearestPlasmoRock = null;
    private BaseUnit _nearestLoggedTree = null;
    private BaseUnit _nearestTree = null;
    private BaseUnit _nearestPlasmoCrumb = null;

    private CollectorComponent _collector;

    void Awake() {
        Collider = gameObject.GetComponent<CapsuleCollider>();
    }

    void Start() {
        _attributes = baseUnit.GetUnitComponent<AttributesComponent>();
        _collector = baseUnit.GetUnitComponent<CollectorComponent>();

        Collider.radius = _attributes.FieldOfView;
        Collider.height = _attributes.HeightOfView;

        PlasmoCrumbsInFOV = new SortedDictionary<GameObject, bool>(new ResourceOrderedByPos(gameObject, _collector));
        TreesInFOV = new SortedDictionary<GameObject, bool>(new ResourceOrderedByPos(gameObject, _collector));
        PlasmoInFOV = new SortedDictionary<GameObject, bool>(new ResourceOrderedByPos(gameObject, _collector));
        LoggedTreesInFOV = new SortedDictionary<GameObject, bool>(new ResourceOrderedByPos(gameObject, _collector));
    }

    void Update() {
        if (NearestPlasmoCrumb != null && !NearestPlasmoCrumb.gameObject.activeInHierarchy) {
            PlasmoInFOV.Remove(NearestPlasmoCrumb.gameObject);
            UpdateNearest(ref PlasmoInFOV, ref _nearestPlasmoCrumb);
        }
        if (NearestPlasmoRock != null && !NearestPlasmoRock.gameObject.activeInHierarchy) {
            PlasmoCrumbsInFOV.Remove(NearestPlasmoRock.gameObject);
            UpdateNearest(ref PlasmoCrumbsInFOV, ref _nearestPlasmoRock);
        }
        if (NearestTree != null && !NearestTree.gameObject.activeInHierarchy) {
            TreesInFOV.Remove(NearestTree.gameObject);
            UpdateNearest(ref TreesInFOV, ref _nearestTree);
        }
        if (NearestLoggedTree != null && !NearestLoggedTree.gameObject.activeInHierarchy) {
            LoggedTreesInFOV.Remove(NearestLoggedTree.gameObject);
            UpdateNearest(ref LoggedTreesInFOV, ref _nearestLoggedTree);
        }
    }

    void OnTriggerEnter(Collider collided) {
        var bu = collided.gameObject.GetComponent<BaseUnit>();
        if (bu == null)
            return;

        var attributes = bu.GetUnitComponent<AttributesComponent>();
        if (attributes == null)
            return;

        if (attributes.UnitType != ObjectType.CRUDERESOURCE && attributes.UnitType != ObjectType.RESOURCE)
            return;

        switch (attributes.UnitType) {
            case ObjectType.CRUDERESOURCE: {
                var crudeResource = bu.GetUnitComponent<CrudeResourceComponent>();
                switch (crudeResource.ResourceType) {
                    case CrudeResourceType.CrudePlasmo:
                        try {
                            PlasmoCrumbsInFOV.Add(collided.gameObject, false);
                        }
                        catch (ArgumentException) { }
                        UpdateNearest(ref PlasmoCrumbsInFOV, ref _nearestPlasmoRock);
                        break;
                    case CrudeResourceType.CrudeTree:
                        try {
                            TreesInFOV.Add(collided.gameObject, false);
                        }
                        catch (ArgumentException) { }
                        UpdateNearest(ref TreesInFOV, ref _nearestTree);
                        break;
                }
            }
            break;
            case ObjectType.RESOURCE: {
                var resource = bu.GetComponent<AbstractResource>();
                switch (resource.ResourceName) {
                    case ResourceType.Plasmo:
                        try {
                            PlasmoInFOV.Add(collided.gameObject, false);
                        } catch (ArgumentException) { }
                        UpdateNearest(ref PlasmoInFOV, ref _nearestPlasmoCrumb);
                        break;
                    case ResourceType.Tree:
                        try {
                            LoggedTreesInFOV.Add(collided.gameObject, false);
                        } catch (ArgumentException) { }
                        UpdateNearest(ref LoggedTreesInFOV, ref _nearestLoggedTree);
                        break;
                }
            }
            break;
        }
    }

    void OnTriggerExit(Collider collided) {
        var bu = collided.gameObject.GetComponent<BaseUnit>();
        if (bu == null)
            return;

        var attributes = bu.GetUnitComponent<AttributesComponent>();
        if (attributes == null)
            return;

        if (attributes.UnitType != ObjectType.CRUDERESOURCE && attributes.UnitType != ObjectType.RESOURCE)
            return;

        switch (attributes.UnitType) {
            case ObjectType.CRUDERESOURCE: {
                var crudeResource = bu.GetUnitComponent<CrudeResourceComponent>();
                switch (crudeResource.ResourceType) {
                    case CrudeResourceType.CrudePlasmo:
                        PlasmoCrumbsInFOV.Remove(collided.gameObject);
                        UpdateNearest(ref PlasmoCrumbsInFOV, ref _nearestPlasmoRock);
                        break;
                    case CrudeResourceType.CrudeTree:
                        TreesInFOV.Remove(collided.gameObject);
                        UpdateNearest(ref TreesInFOV, ref _nearestTree);
                        break;
                }
            }
                break;
            case ObjectType.RESOURCE: {
                var resource = bu.GetComponent<AbstractResource>();
                switch (resource.ResourceName) {
                    case ResourceType.Plasmo:
                        PlasmoInFOV.Remove(collided.gameObject);
                        UpdateNearest(ref PlasmoInFOV, ref _nearestPlasmoCrumb);
                        break;
                    case ResourceType.Tree:
                        LoggedTreesInFOV.Remove(collided.gameObject);
                        UpdateNearest(ref LoggedTreesInFOV, ref _nearestLoggedTree);
                        break;
                }
            }
            break;
        }
    }

    private void UpdateNearest(ref SortedDictionary<GameObject,bool> fovSet, ref BaseUnit nearest) {
        nearest = fovSet.Count > 0 ? fovSet.Keys.AsEnumerable().ToArray()[0].GetComponent<BaseUnit>() : null;
    }

    public override void GUIPriority() {
        
    }

    public override void UserInputPriority() {
        
    }

    public override void Reset() {
        
    }
}
