using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class IA_Patrol : AbstractUnitComponent, IStateMachine
{
    private int _currentPath = 0;
    private MobileComponent _mobile;
    private ClickController _click;
    private List<Vector3> _path;
    private Queue<Vector3> _temporaryPath;

	void Start () {
        _click = PlayerManager.Player.clickController;
        _path = new List<Vector3>();
        _temporaryPath = new Queue<Vector3>();
	    _mobile = baseUnit.GetUnitComponent<MobileComponent>();

	    StartCoroutine(PatrolWander());
	}
	
	void Update () {

        if (CannotUse() || !baseUnit.IsSelected) {
	        return;
	    }

        var position = _click.GetClickPosition(MouseButton.Right);
        if (position == Vector3.zero)
            return;
        
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
            AddPath(position);
        } else if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) {
            AddPath(position);
            _temporaryPath.Enqueue(position);
        } else {
            ClearPath();
        }
	}

    private bool CannotUse() {
        return baseUnit.IsEnemy || baseUnit.IsDead || baseUnit.IsResource;
    }
    private void ClearPath() {
        _path.Clear();
        _currentPath = 0;
    }
    private void AddPath(Vector3 position) {
        if (_path.Count == 0) {
            _path.Add(transform.position);
        }
        _path.Add(position);
    }

    void OnDisable() {
        ClearPath();
        StopAllCoroutines();
    }

    void OnEnable() {
        //Assegura que a unidade já foi instanciada uma vez
        if (_path != null) {
            StartCoroutine(PatrolWander());
        }
    }

    public IEnumerator PatrolWander() {
        do {
            while (_path.Count < 1 || CannotUse()) {
                yield return new WaitForEndOfFrame();
            }

            if (_mobile.ReachedDestination()) {
                _currentPath = (_currentPath + 1) % _path.Count;
                _mobile.MoveTo(_path[_currentPath]);

                if (_path.Count == 1) {
                    ClearPath();
                } else if (_temporaryPath.Count > 0 && _path[_currentPath] == _temporaryPath.Peek()) {
                    _temporaryPath.Dequeue();
                    _path.RemoveAt(_currentPath);
                    --_currentPath;
                }
            }
            yield return new WaitForEndOfFrame();
        } while (true);
    }

    public void SendMessageToState(IA_Messages iaMessage) {

    }

    public MonoBehaviour GetMeAsAComponent() {
        return this;
    }

    public IState<BaseUnit> CurrentState() {
        return null;
    }

    public override void GUIPriority() {
     
     }
    
    public override void UserInputPriority() {
	
	}
	
	public override void Reset() {}
}
