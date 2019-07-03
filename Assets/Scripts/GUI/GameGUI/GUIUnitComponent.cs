﻿using System.Collections;
﻿using UnityEngine;

public class GUIUnitComponent : AbstractUnitComponent {


    public float healthBarHeight = 3.0f;

    private AttributesComponent attributes;
    private FOWRenderers fowr;

    private GUIHealthBarHandler _healthBarHandler = null;
    private GUIHitHandler _hitHandler = null;


    void Start() {

        if (HUDUnitsRoot.go == null) {
            Destroy(this);
            return;
        }
        attributes = baseUnit.GetUnitComponent<AttributesComponent>();
        fowr = transform.GetComponent<FOWRenderers>();

        // Adiciona o handler do health bar
        var _healthBarHUD = NGUITools.AddChild(HUDHealthBars.go, HUDHealthBars.prefab);
        if (_healthBarHUD != null) {
            var follow = _healthBarHUD.AddComponent<UIFollowTarget>();
            follow.target = baseUnit.transform;
            follow.positionOffset = new Vector3(0.0f, healthBarHeight, 0.0f);
            _healthBarHandler = _healthBarHUD.GetComponent<GUIHealthBarHandler>();
        }
        // Adiciona o handler do hit
        var _hitHUD = NGUITools.AddChild(HUDHits.go, HUDHits.prefab);
        if (_hitHUD != null) {
            _hitHUD.AddComponent<UIFollowTarget>().target = baseUnit.transform;
            _hitHandler = _hitHUD.GetComponent<GUIHitHandler>();
        }

    }

    public void addTextEffect(object obj, Color c, float stayDuration) {
        if (_hitHandler._mText == null)
            return;
        _hitHandler._mText.Add(obj, c, stayDuration);
    }

    void Update() {
        if (attributes == null || fowr == null)
            return;

        if (_healthBarHandler != null) {
            _healthBarHandler.ChangeValue(attributes.CurrentLife, attributes.MaxLife);
            if (baseUnit.IsEnemy) {
                _healthBarHandler.changeBackgroundColor(new Color(0.90f, 0.21f, 0.0f));
            }
            else {
                _healthBarHandler.changeBackgroundColor(new Color(0.47f, 0.86f, 0.0f));
            }
        }

        if (_healthBarHandler != null)
            _healthBarHandler.SetEnable(ShowHealthBar);

    }

    bool ShowHealthBar {
        get {
            return (baseUnit.MeshGO.activeSelf && !attributes.Invisible && fowr.isVisible && !baseUnit.IsDead);
        }
    }

    public override void GUIPriority() {
    }

    public override void UserInputPriority() {
    }

    public override void Reset() {
    }
}