using UnityEngine;

public class GUIMinimap : MonoBehaviour {

	
	 // Use this for initialization
         public Transform PlayerEye;
         public Camera MinimapCam;
         public float distance = 50f;
         public float viewSize = 55f;
        public float absolouteX = 0f;
        public float absolouteY = 0f;
         public float absolouteWidth = 100.0f;
         public float absolouteHeight = 100.0f;
         
         private bool clicked;
        void Start () {

        }
 
         // Update is called once per frame
         void Update () {
             if (MinimapCam == null)
                 return;

             MinimapCam.pixelRect = new Rect(Screen.width - absolouteX, absolouteY, absolouteWidth, absolouteHeight);
             MinimapCam.orthographicSize = viewSize;
                 //if mouse button (left hand side) pressed instantiate a raycast
             bool contain = MinimapCam.pixelRect.Contains(Input.mousePosition);
             if (Input.GetMouseButtonDown(0)&& contain)
                 clicked = true;
             else if (Input.GetMouseButtonUp(0)||!contain)
                 clicked= false;

			 if (!clicked) return;

			 //create a ray cast and set it to the mouses cursor position in game
            Ray ray = MinimapCam.ScreenPointToRay (Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast (ray, out hit, distance)) 
                    PlayerEye.position = new Vector3(hit.point.x,26,hit.point.z);

         }
}
