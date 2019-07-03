using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections;

public class DropPointManager : MonoBehaviour {
    private static HashSet<BaseUnit> _dropPoints;
    public static HashSet<BaseUnit> DropPoints 
    { get { return _dropPoints; } set { _dropPoints = value; } }

    public static BaseUnit GetNearestDropPoint(Vector3 position) {
        if (_dropPoints.Count == 0) return null;

        var target = _dropPoints.First();
        var currentDistance = (target.transform.position - position).sqrMagnitude;
        foreach (var dropPoint in _dropPoints) {
            var distance = (dropPoint.transform.position - position).sqrMagnitude;
            if (!(distance < currentDistance)) { continue;}
            currentDistance = distance;
            target = dropPoint;
        }

        return target;
    }
}
