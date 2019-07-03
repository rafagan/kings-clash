using UnityEngine;
using System.Collections;
using System.Runtime.Remoting.Channels;

public class RTSController : MonoBehaviour {
	enum ClickState {Idle, SingleClick, DoubleClick, Hold};
	
	public Texture2D SelectionBoxTexture;
	public bool CanControl = false;

	//Variáveis para testes de cliques
	private ClickState clickState;
	//not used - private float currentClickTime = 0;
	private float lastClickTime = 0;
	public float MaxTimeToDoubleClick = 1.0f;
	public float TimeToHoldClick = 1.5f;
	
	private GameManager manager;
	private PlayerManager player;
	private Vector2 initialMousePosition;
	private Vector2 finalMousePosition;
	private int focusIndex;

	void Start() {
		if (manager == null)
			manager = GameManager.Manager;
		
		if (player == null)
			player = transform.GetComponent<PlayerManager>();

		focusIndex = 0;
		clickState = ClickState.Idle;
	}

    void Update() {
       CheckClickState();
		
		if(CanControl && GUIUtility.hotControl == 0) {
			if (clickState == ClickState.SingleClick && !Input.GetKey(KeyCode.LeftShift))
				SelectSingleUnit();
            else if (clickState == ClickState.DoubleClick && !Input.GetKey(KeyCode.LeftShift))
                SelectSameUnitsInScreen();
			else if (clickState == ClickState.SingleClick && Input.GetKey(KeyCode.LeftShift))
				AddOrRemoveUnitInSelection();
		}
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

	void OnGUI() {
		if(clickState == ClickState.Hold && CanControl && GUIUtility.hotControl == 0)
			DrawSelectionBox();
	}
	
	void CheckClickState() {
		if (clickState == ClickState.Idle) {
			if (Input.GetMouseButtonDown(0)) {
			    if (CheckUIHover() == false)
			    {
			        if (Time.fixedTime <= lastClickTime + MaxTimeToDoubleClick)
			        {
                        clickState = ClickState.DoubleClick;
			        }
			        else
			            clickState = ClickState.SingleClick;

			        initialMousePosition = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
			        lastClickTime = Time.fixedTime;
			    }
			}
		}
		
		if (clickState == ClickState.SingleClick) {
			if (Input.GetMouseButton(0) && Time.fixedTime >= lastClickTime + TimeToHoldClick) {
			    if (CheckUIHover() == false)
			    {
			        clickState = ClickState.Hold;
			    }
			}
		}
		
		if (clickState != ClickState.Idle) {
		    if (Input.GetMouseButtonUp(0))
		    {
		        if (CheckUIHover() == false)
		        {
		            clickState = ClickState.Idle;
		        }
		    }
		}
	}

	private void DrawSelectionBox() {
		if(Input.GetMouseButton(0) && GUIUtility.hotControl == 0) {
			finalMousePosition = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
			GUI.DrawTexture(new Rect(initialMousePosition.x, initialMousePosition.y, 
			                        finalMousePosition.x - initialMousePosition.x, finalMousePosition.y - initialMousePosition.y), 
			                		SelectionBoxTexture);
		}

		if(Input.GetMouseButtonUp(0) && GUIUtility.hotControl == 0) {
			if(!Input.GetKey(KeyCode.LeftShift))
				manager.unitsManager.ClearSelectedList();

			float xMin = Mathf.Min(initialMousePosition.x, finalMousePosition.x);
			float yMin = Mathf.Min(initialMousePosition.y, finalMousePosition.y);
			float width = Mathf.Abs(initialMousePosition.x - finalMousePosition.x);
			float height = Mathf.Abs(initialMousePosition.y - finalMousePosition.y);

			CheckUnitsInBox(new Rect(xMin, yMin, width, height));
		}
	}

	private void CheckUnitsInBox(Rect selection) {
		#region Check Characters in Box Selection
		foreach(BaseUnit _unit in manager.unitsManager.CharactersInScene) {
			Vector3 _unitPosition = Camera.main.WorldToScreenPoint(_unit.transform.position);
			Vector2 _convertedUnitPosition = new Vector2(_unitPosition.x, Screen.height - _unitPosition.y);

			if(selection.Contains(_convertedUnitPosition))
				manager.unitsManager.AddToSelectedList(_unit);
			
			//Filtra a multipla selecao, para que selecione apenas as unidades correspondentes ao player role
			for (int i = manager.unitsManager.SelectedUnits.Count - 1; i >= 0; i--) {
				var _selectedUnit = manager.unitsManager.SelectedUnits[i];
			    if (PlayerManager.Player.PlayerRoles.Contains(PlayerRole.SPECTATOR) == false)
			    {
                    if (_selectedUnit.IsEnemy || !player.CheckPlayerRole(_selectedUnit.UnitRole))
                        manager.unitsManager.RemoveOfSelectedList(_selectedUnit);	
			    }	
			}
		}
		#endregion
		
		if (manager.unitsManager.SelectedUnits.Count > 0) return;
		
		#region Check Structures in Box Selection
		foreach(BaseUnit _unit in manager.unitsManager.StructuresInScene) {
			Vector3 _unitPosition = Camera.main.WorldToScreenPoint(_unit.transform.position);
			Vector2 _convertedUnitPosition = new Vector2(_unitPosition.x, Screen.height - _unitPosition.y);

			if(selection.Contains(_convertedUnitPosition)) {
				manager.unitsManager.AddToSelectedList(_unit);
			}
			
			//Filtra a multipla selecao, para que selecione apenas as unidades correspondentes ao player role
			for (int i = manager.unitsManager.SelectedUnits.Count - 1; i >= 0; i--) {
				var _selectedUnit = manager.unitsManager.SelectedUnits[i];
			    if (PlayerManager.Player.PlayerRoles.Contains(PlayerRole.SPECTATOR) == false)
			    {
                    if (_selectedUnit.IsEnemy || !player.CheckPlayerRole(_selectedUnit.UnitRole))
                        manager.unitsManager.RemoveOfSelectedList(_selectedUnit);	
			    }	
			}
		}
		#endregion
	}

    private BaseUnit GetRayTarget() {

		Ray _ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit _hit;

        if (Physics.Raycast(_ray.origin, _ray.direction, out _hit, Mathf.Infinity)) {
            //if (_hit.transform.gameObject.layer == 27)
            //    return null;
            var _target = _hit.transform.GetComponent<BaseUnit>();
            if (_target != null) return _target;
        }
        
		return null;
	}

	private bool SelectSingleUnit() {
		BaseUnit _unit = GetRayTarget();
		if (_unit != null) {
			manager.unitsManager.ClearSelectedList();
			manager.unitsManager.AddToSelectedList(_unit);
	
			return true;
		} else {
			manager.unitsManager.ClearSelectedList();
		}

		return false;
	}
	
	private bool AddOrRemoveUnitInSelection() {
		Ray _raycast = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit _hit = new RaycastHit();

		if (Physics.Raycast(_raycast, out _hit, Mathf.Infinity)) {
			BaseUnit _unit = _hit.transform.GetComponent<BaseUnit>();
			
			//Verifica se a unidade é inimiga ou se não pertence a Role
			if (_unit == null || _unit.IsEnemy || !PlayerManager.Player.PlayerRoles.Contains(_unit.UnitRole)) return false;
			if (_unit.UnitType != ObjectType.CHARACTER && _unit.UnitType != ObjectType.STRUCTURE) { Debug.Log("Aqio"); return false;}
			
			//Verifica se na lista de seleção contém unidades de outro tipo e aproveita e verifica se a unidade já está selecionada
			if (manager.unitsManager.SelectedUnits.Count > 0) {
				foreach (BaseUnit _unitAlreadySelected in manager.unitsManager.SelectedUnits) {
					if (manager.unitsManager.SelectedUnits.Contains(_unit)) {
						manager.unitsManager.RemoveOfSelectedList(_unit);
						return true;
					}
					else if (_unitAlreadySelected.UnitType != _unit.UnitType)
						return false;			
				}
			}
						
			manager.unitsManager.AddToSelectedList(_unit);
		}

		return false;
	}

	private bool SelectSameUnitsInScreen() {
		Ray _raycast = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit _hit = new RaycastHit();

		if (Physics.Raycast(_raycast, out _hit, 5000)) {
			if (_hit.transform.gameObject.layer == 8) {
				manager.unitsManager.ClearSelectedList();

				string _name = _hit.transform.GetComponent<AttributesComponent>().UnitName;
				Rect _screen = new Rect(0, 0, Screen.width, Screen.height);

				foreach(BaseUnit _unit in manager.unitsManager.CharactersInScene) {
					if (!_unit.IsEnemy && _unit.UnitName == _name) {
						Vector3 _unitPosition = Camera.main.WorldToScreenPoint(_unit.transform.position);
						Vector2 _convertedUnitPosition = new Vector2(_unitPosition.x, Screen.height - _unitPosition.y);

						if(_screen.Contains(_convertedUnitPosition) && PlayerManager.Player.CheckPlayerRole(_unit.UnitRole)) {
							manager.unitsManager.AddToSelectedList(_unit);
						}
					}
				}
				return true;
			}
		}
		return false;
	}

	private void FocusInUnit() {
		if (Input.GetKeyDown(KeyCode.F) && manager.unitsManager.SelectedUnits.Count > 0) {
			if (focusIndex > manager.unitsManager.SelectedUnits.Count - 1)
				focusIndex = 0;

			Camera.main.transform.position = manager.unitsManager.SelectedUnits[focusIndex].transform.position;
			focusIndex++;
		}
	}

	private void HealthBarSwitch() {
		if (Input.GetKeyDown(KeyCode.Backspace))
			manager.ShowHealthBar = !manager.ShowHealthBar;
	}
	
	public void ClearMousePositions() {
		initialMousePosition = Vector2.zero;
		finalMousePosition = Vector2.zero;
	}
}
