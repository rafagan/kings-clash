using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {
	public float MinDistanceToBorder;
	public float ScrollSpeed;
	public float ZoomSpeed = 25.0f;
	public int ZoomMin = 20;
	public int ZoomMax = 100;
	public float PanSpeed = 50.0f;
	public int PanAngleMin = 25;
	public int PanAngleMax = 80;
	public Transform CameraPosition;
    public Transform SpectatorPosition;
	public Transform UpperBound;
	public Transform BottonBound;
    public bool EnableEdgeScrolling = true;

	private float screenHeight;
	private float screenWidth;
	[SerializeField]
	private Vector3[] cameraPositions;
	private Vector3 nullPosition;
	private bool canGoLeft = true;
	private bool canGoRight = true;
	private bool canGoUp = true;
	private bool canGoDown = true;

	void Start() {
		nullPosition = new Vector3(0, -999, 0);
		cameraPositions = new Vector3[10];
		for (int i = 0; i < 10; i++)
			cameraPositions[i] = nullPosition;

		screenHeight = Screen.height;
		screenWidth = Screen.width;
	}

	void Update() {
		CheckBounds();
		EdgeScrolling();
		WASDScrolling();
		AddToBookmark();
		GoToBookmark();
	}

	private void EdgeScrolling() {
        if(!EnableEdgeScrolling) return;

		Vector2 _mousePosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);

		float _yBorder = screenHeight - _mousePosition.y;
		float _xBorder = screenWidth - _mousePosition.x;

		Vector3 _movement = Vector3.zero;

		if(canGoUp && _yBorder < MinDistanceToBorder)
			_movement = new Vector3(0, 0, 1);
		else if (canGoDown && _mousePosition.y < MinDistanceToBorder)
			_movement = new Vector3(0, 0, -1);

		if(canGoRight && _xBorder < MinDistanceToBorder)
			_movement += new Vector3(1, 0, 0);
		else if (canGoLeft && _mousePosition.x < MinDistanceToBorder)
			_movement += new Vector3(-1, 0, 0);

		CameraPosition.Translate(_movement * Time.deltaTime * ScrollSpeed, Space.Self);
	}
	
	private void CheckBounds() {
        if (CameraPosition.localPosition.x >= BottonBound.localPosition.x)
        {
			canGoRight = false;
        }
        else if (CameraPosition.localPosition.x < BottonBound.localPosition.x)
        {
			canGoRight = true;
		}
        if (CameraPosition.localPosition.z <= BottonBound.localPosition.z)
        {
			canGoDown = false;
        }
        else if (CameraPosition.localPosition.z > BottonBound.localPosition.z)
        {
			canGoDown = true;
		}
        if (CameraPosition.localPosition.x <= UpperBound.localPosition.x)
        {
			canGoLeft = false;
        }
        else if (CameraPosition.localPosition.x > UpperBound.localPosition.x)
        {
			canGoLeft = true;
		}
        if (CameraPosition.localPosition.z >= UpperBound.localPosition.z)
        {
			canGoUp = false;
        }
        else if (CameraPosition.localPosition.z < UpperBound.localPosition.z)
        {
			canGoUp = true;
		}
	}

	private void WASDScrolling() {
		float translationX = 0;
		float translationY = 0;

		if (canGoLeft && (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)))
			translationX = -1.0f;
		else if (canGoRight && (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)))
			translationX = 1.0f;
		if (canGoUp && (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)))
			translationY = 1.0f;
		else if (canGoDown && (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)))
			translationY = -1.0f;

		CameraPosition.Translate((translationX) * Time.deltaTime * ScrollSpeed, 0, 
		                    (translationY) * Time.deltaTime * ScrollSpeed);	
	}

	private void Zooming() {
		var _translation = Vector3.zero;
		var _zoomDelta = Input.GetAxis("Mouse ScrollWheel") * ZoomSpeed * Time.deltaTime;
		if (_zoomDelta != 0)
			_translation -= Vector3.up * ZoomSpeed * ZoomSpeed;

		CameraPosition.position += _translation;
	}

	private void AddToBookmark() {
		if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) &&
		    Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) {
			int _key = ReturnAlphaNumber();

			if (_key != 99)
				cameraPositions[_key] = CameraPosition.position;
		}
	}

	private void GoToBookmark() {
		if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) {
			int _key = ReturnAlphaNumber();

			if (_key != 99 && cameraPositions[_key] != nullPosition)
				CameraPosition.position = cameraPositions[_key];
		}
	}
	
	public void FocusUnit(BaseUnit unit) {
		if (unit != null)
			CameraPosition.position = new Vector3(unit.transform.position.x, CameraPosition.position.y, unit.transform.position.z);
	}

    public void FocusPosition(Vector3 position)
    {
        CameraPosition.position = new Vector3(position.x, CameraPosition.position.y, position.z);
    }

	private int ReturnAlphaNumber() {
		if (Input.GetKeyUp(KeyCode.Alpha1))
			return 1;
		else if (Input.GetKeyUp(KeyCode.Alpha2))
			return 2;
		else if (Input.GetKeyUp(KeyCode.Alpha3))
			return 3;
		else if (Input.GetKeyUp(KeyCode.Alpha4))
			return 4;
		else if (Input.GetKeyUp(KeyCode.Alpha5))
			return 5;
		else if (Input.GetKeyUp(KeyCode.Alpha6))
			return 6;
		else if (Input.GetKeyUp(KeyCode.Alpha7))
			return 7;
		else if (Input.GetKeyUp(KeyCode.Alpha8))
			return 8;
		else if (Input.GetKeyUp(KeyCode.Alpha9))
			return 9;
		else if (Input.GetKeyUp(KeyCode.Alpha0))
			return 0;
		else
			return 99;
	}
}
