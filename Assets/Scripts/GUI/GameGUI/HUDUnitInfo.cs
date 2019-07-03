using UnityEngine;
using System.Collections;

public class HUDUnitInfo : MonoBehaviour {

    private UILabel _life;
    private UILabel _name;
    private UISprite _sprite;
    private UISprite _mask;
    //private UISprite _unitPortrait;


    void Start() {
        _life = transform.Find("LifeL").GetComponent<UILabel>();
        _name = transform.Find("NameL").GetComponent<UILabel>();
        _sprite = transform.Find("Portrait").GetComponent<UISprite>();
        _mask = transform.Find("Background").GetComponent<UISprite>();
    }

    void Update() {
        if (InterfaceManager.GetSelectedBaseUnit() == null) {
            _life.enabled = false;
            _name.enabled = false;
            _sprite.enabled = false;
            _mask.enabled = false;
            return;
        }
        _life.enabled = true;
        _name.enabled = true;
        _sprite.enabled = true;
        _mask.enabled = true;

        _name.text = InterfaceManager.GetSelectedBaseUnit().UnitName;
        _life.text = "Life " + (int)InterfaceManager.GetSelectedBaseUnit().GetUnitComponent<AttributesComponent>().CurrentLife;
        _sprite.spriteName = InterfaceManager.GetSelectedBaseUnit().UnitIconName;

    }
}