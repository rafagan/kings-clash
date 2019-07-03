using UnityEngine;
using System.Collections;

public class CursorMark : MonoBehaviour
{
    public Projector MouseProjector;
    public float AnimationSpeed;

    private bool isActive;

	void Start () {
	    if (MouseProjector == null)
	    {
	        MouseProjector = GetComponentInChildren<Projector>();
	    }

        gameObject.SetActive(false);
	}

    public void PlaceMark(Vector3 target)
    {
        gameObject.SetActive(true);
        transform.position = new Vector3(target.x, 10, target.z);
        StartCoroutine(Animate());
    }

    private void Reset()
    {
        MouseProjector.orthographicSize = 1;
        gameObject.SetActive(false);
    }

    private IEnumerator Animate()
    {
        while (MouseProjector.orthographicSize >= 0.1f)
        {
            MouseProjector.orthographicSize -= AnimationSpeed;
            yield return null;
        }

        Reset();
    }
}
