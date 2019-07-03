//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2013 Tasharen Entertainment
//----------------------------------------------

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Very basic script that will activate or deactivate an object (and all of its children) when clicked.
/// </summary>

[AddComponentMenu("NGUI/Interaction/Button Activate")]
public class UIButtonActivate : MonoBehaviour
{
    public List<GameObject> ActivateTargets = new List<GameObject>();
    public List<GameObject> DesactivateTargets = new List<GameObject>();

    void OnClick () {
        foreach (var target in ActivateTargets) {
            if (target != null) NGUITools.SetActive(target, true);
        }
        foreach (var target in DesactivateTargets) {
            if (target != null) NGUITools.SetActive(target, false);
        }
    }
}