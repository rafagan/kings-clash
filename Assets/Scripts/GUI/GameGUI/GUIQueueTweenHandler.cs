using UnityEngine;
using System.Collections;

public class GUIQueueTweenHandler : MonoBehaviour {

    private UIPlayTween _play;
    private UITweener _tween;

	// Use this for initialization
    void Start() {
        _tween = GetComponent<UITweener>();
    }
	
	// Update is called once per frame
    void Update() {
        if (_tween == null)
            return;
        if (InterfaceManager.GetSelectedBaseUnit() == null) {
            _tween.Play(false);
            return;
        }
        if (InterfaceManager.GetSelectedBaseUnit().GetComponent<ProducerComponent>() != null) {
            if (InterfaceManager.GetSelectedBaseUnit().GetComponent<StructureComponent>().built)
                _tween.Play(true);
        }
    }
}
