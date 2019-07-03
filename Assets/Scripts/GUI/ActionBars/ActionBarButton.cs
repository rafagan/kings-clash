using UnityEngine;
using System.Collections;
using System;

public class ActionBarButton : MonoBehaviour {
    public ActionBarRow BarRow;
    public ActionBarInfo Info;
    public UILabel HotKeyLabel;
    public UILabel CooldownLabel;
    public UISprite CooldownSprite;
    public UISprite Icon;

    void DoAction() {
        if (InterfaceManager.GetSelectedBaseUnit() == null) {
            return;
        }
        if (Info._ability != null) {

            InterfaceManager.Manager.SetSelectedAbility(Info._ability.AbilityIndex);
        }
        if (Info._pool != null) {
            if (Info._pool.Prototype.GetComponent<AttributesComponent>().UnitType == ObjectType.CHARACTER)
                InterfaceManager.GetSelectedBaseUnit().GetUnitComponent<ProducerComponent>().TrainUnit(Info._pool.PoolUniqueID);
            else if (Info._pool.Prototype.GetComponent<AttributesComponent>().UnitType == ObjectType.STRUCTURE)
                Info._pool.PreviewStructure.Spawn(InterfaceManager.GetSelectedBaseUnit());
        }
    }

    //NGUI OnPress Event
    void OnPress(bool isPressed) {
        // Left Clicking an item, thus activating it
        if (Input.GetMouseButtonUp(0) && (Info != null)) {
            IsClicked();
        }
    }

    //NGUI OnTooltip Event
    void OnTooltip(bool show) {
        if (show) {
            string Tip;
            if (Info != null) {
                Tip = "" + Info.Name;
                Tip += "\n" + Info.Description + "\n";
                if (Info._ability != null) {
                    if (Info._ability.reloadTime > 0)
                        Tip += "\n" + "Cooldown: " + Info._ability.reloadTime;
                    if (Info._ability.inCooldown == true) {
                        Tip += "\n" + "Cooldown Remaining: " + Mathf.Round((Info._ability.currentTime)).ToString();
                    }
                }
            }
            else {
                Tip = "Empty Button";
            }
            UITooltip.ShowText(Tip);
            return;

        }
        UITooltip.ShowText(null);
    }
    void Start() {
        Disable();
    }

    public void Disable() {
        HotKeyLabel.enabled = false;
        CooldownLabel.enabled = false;
        CooldownSprite.enabled = false;
        Icon.enabled = false;
        Info.Disabled = true;
    }

    public void Enable() {
        HotKeyLabel.enabled = true;
        CooldownLabel.enabled = true;
        CooldownSprite.enabled = true;
        Icon.enabled = true;
        Info.Disabled = false;
    }

    void Update() {

        if (Info != null) {
            if (Info.Disabled) {
                SetFilled(1F);
                return;
            }
            if (ActionBarSettings.Instance.HotKeyDictionary.ContainsKey(Info.HotKey)) {
                HotKeyLabel.text = ActionBarSettings.Instance.HotKeyDictionary[Info.HotKey];
            }  else {
                HotKeyLabel.text = ""; //Assign Text to Blank since it has no Hotkey
            }
            if (Info._ability != null) {
                if (Info._ability.inCooldown) {
                    CooldownSprite.fillAmount = Mathf.Lerp(0F, 1f, Info._ability.currentTime / Info._ability.reloadTime);
                    if (ActionBarSettings.Instance.DisplayCooldownTimer == true) {
                        CooldownLabel.text = Mathf.Round((Info._ability.currentTime)).ToString();
                    }
                    else {
                        CooldownLabel.text = "";
                    }
                }
                else {
                    if (CooldownSprite.fillAmount > 0F) {
                        if (!Info.Disabled) {
                            CooldownSprite.fillAmount = 0F;
                        }
                    }
                    CooldownLabel.text = "";
                }
            }
            else {
                CooldownSprite.fillAmount = 0F;
                CooldownLabel.text = "";
            }
            if (Input.GetKeyUp(Info.HotKey)) {
                IsClicked();
            }
        }
    }

    //Called when the Item/Spell is clicked
    void IsClicked() {
        if (Info != null) {
            if (Info.Disabled == false) {
                if (Info._ability != null) {
                    if (Info._ability.inCooldown) {
                        NGUITools.PlaySound(ActionBarSettings.Instance.ButtonClickSound_Disable);
                        return;
                    }
                }
                //Call the primary select function (Activates the spell/item, User defines what happens there)
                DoAction();
                NGUITools.PlaySound(ActionBarSettings.Instance.ButtonClickSound_Success);
            }
            else {
                NGUITools.PlaySound(ActionBarSettings.Instance.ButtonClickSound_Disable);
            }
        }
        else {
            NGUITools.PlaySound(ActionBarSettings.Instance.ButtonClickSound_Disable);
        }
    }

    //Assign new Item/Spell to the Button
    public void SetInfo(ActionBarInfo ButtonInformation) {
        if (ButtonInformation == null) {
            RemoveInfo();
            return;
        }
        Info = ButtonInformation;
        CooldownSprite.fillDirection = UISprite.FillDirection.Radial360;
        SetTexture(ActionBarSettings.Instance.Atlas[Info.Atlas], Info.Icon);
        Enable();
    }
    //Set the Texture of the Button
    public void SetTexture(UIAtlas Atlas, string SpriteName) {
        Icon.atlas = Atlas;
        Icon.spriteName = Atlas.GetSprite(SpriteName) == null ? "Empty" : SpriteName;
    }

    //Sets the fill amount on the cooldown Sprite
    public void SetFilled(float FillAmount) {
        CooldownSprite.fillAmount = FillAmount;
    }
    //Remove Item from button
    public void RemoveInfo() {
        Icon.atlas = null;
        Icon.spriteName = "";
        CooldownLabel.text = "";
        CooldownSprite.fillAmount = 0F;
        Info = null;
    }




}
