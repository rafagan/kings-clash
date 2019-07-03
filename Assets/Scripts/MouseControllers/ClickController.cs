using UnityEngine;
using System.Collections;

public enum MouseButton { Left = 0, Right = 1 }

public class ClickController : MonoBehaviour {
    public CursorMark CursorProjector;

	public KeyCode GetKeyCodePressed () {
		var e = System.Enum.GetNames (typeof(KeyCode)).Length;
		for (int i = 0; i < e; i++) {
			if (Input.GetKey ((KeyCode)i))
				return (KeyCode)i;
		}
		return KeyCode.None;
	}
	
	private BaseUnit GetRayTarget() {
		Ray _ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit _hit;

        if (Physics.Raycast(_ray.origin, _ray.direction, out _hit, Mathf.Infinity)) {
            var _target = _hit.transform.GetComponent<BaseUnit>();
            if (_target != null) return _target;
        }
        
		return null;
	}

    public BaseUnit CheckClickOnEnemy() {
        if (Input.GetMouseButtonUp((int)MouseButton.Right) && GUIUtility.hotControl == 0) {
            if (CheckUIHover() == false)
            {
                BaseUnit _target = GetRayTarget();
                if (_target != null && _target.IsEnemy) return _target;
            }
        }
        return null;
    }

    public BaseUnit CheckClickOnAlly() {
        if (Input.GetMouseButtonUp((int)MouseButton.Right) && GUIUtility.hotControl == 0) {
            if (CheckUIHover() == false)
            {
                BaseUnit _unit = GetRayTarget();
                if (_unit != null && _unit.TeamID == PlayerManager.Player.PlayerTeam) return _unit;
            }
        }
        return null;
    }

    public BaseUnit CheckLeftClickOnEnemy()
    {
        if (Input.GetMouseButtonUp((int)MouseButton.Left) && GUIUtility.hotControl == 0)
        {
            if (CheckUIHover() == false)
            {
                BaseUnit _target = GetRayTarget();
                if (_target != null && _target.IsEnemy) return _target;
            }
        }
        return null;
    }

    public BaseUnit CheckLeftClickOnAlly()
    {
        if (Input.GetMouseButtonUp((int)MouseButton.Left) && GUIUtility.hotControl == 0)
        {
            if (CheckUIHover() == false)
            {
                BaseUnit _unit = GetRayTarget();
                if (_unit != null && _unit.TeamID == PlayerManager.Player.PlayerTeam) return _unit;
            }
        }
        return null;
    }

    public Vector3 GetClickPosition(MouseButton button) {
        if (Input.GetMouseButtonDown((int)button) && GUIUtility.hotControl == 0) {
            if (CheckUIHover() == false)
            {
                Ray _ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit _hit;

                if (Physics.Raycast(_ray.origin, _ray.direction, out _hit, Mathf.Infinity))
                {
                    if (_hit.transform.GetComponent<BaseUnit>() == null && _hit.transform.gameObject.layer == 10)
                    {
                        CursorProjector.PlaceMark(_hit.point);
                        return _hit.point;
                    }
                }
            }
        }

		return Vector3.zero;
    }

    public BaseUnit CheckPlasmoNodeClick() {
        if (Input.GetMouseButtonDown((int)MouseButton.Right) && GUIUtility.hotControl == 0) {
            if (CheckUIHover() == false)
            {
                Ray _ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit _hit;

                if (Physics.Raycast(_ray.origin, _ray.direction, out _hit, Mathf.Infinity))
                {
                    var _target = _hit.transform.GetComponent<BaseUnit>();

                    if (_target != null && _target.GetUnitComponent<PlasmoComponent>() != null)
                    {
                        return _target;
                    }
                }
            }
        }
        return null;
    }

    public BaseUnit CheckLoggedTreeClick() {
        if (Input.GetMouseButtonDown((int)MouseButton.Right) && GUIUtility.hotControl == 0) {
            if (CheckUIHover() == false)
            {
                Ray _ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit _hit;

                if (Physics.Raycast(_ray.origin, _ray.direction, out _hit, Mathf.Infinity))
                {
                    var _target = _hit.transform.GetComponent<BaseUnit>();

                    if (_target != null && _target.GetUnitComponent<LoggedTreeComponent>() != null)
                    {
                        return _target;
                    }
                }
            }
        }
        return null;
    }

    public BaseUnit CheckCrudeResourceClick() {
        if (Input.GetMouseButtonDown((int)MouseButton.Right) && GUIUtility.hotControl == 0) {
            if (CheckUIHover() == false)
            {
                Ray _ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit _hit;

                if (Physics.Raycast(_ray.origin, _ray.direction, out _hit, Mathf.Infinity))
                {
                    var _target = _hit.transform.GetComponent<BaseUnit>();
                    if (_target != null && _target.IsResource &&
                        _target.GetUnitComponent<CrudeResourceComponent>() != null)
                        return _target;
                }
            }
        }
        return null;
    }

    private bool CheckUIHover()
    {
        var _rayUI = UICamera.mainCamera.ScreenPointToRay(Input.mousePosition);
        const int UIlayerMask = 1 << 27;
        RaycastHit _hitUI;
        if (Physics.Raycast(_rayUI.origin, _rayUI.direction, out _hitUI, Mathf.Infinity, UIlayerMask))
        {
            return true;
        }
        return false;
    }
}
