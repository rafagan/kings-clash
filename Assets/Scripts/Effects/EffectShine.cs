using UnityEngine;
using System.Collections;

public class EffectShine : MonoBehaviour {
    public float FlashRate = 1.0f;
    private Color _originalColor;

    void Awake() {
        _originalColor = GetComponent<Renderer>().material.color;
    }

    void Start() {
        StartCoroutine(Flash());
    }
    void OnEnable() {
        StartCoroutine(Flash());
    }

    private IEnumerator Flash() {
        float t = 0;
        while (t < FlashRate) {
            GetComponent<Renderer>().material.color = Color.Lerp(_originalColor, Color.white, t / FlashRate);
            t += Time.deltaTime;
            yield return null;
        }
        GetComponent<Renderer>().material.color = Color.white;
        StartCoroutine("Return");
    }

    private IEnumerator Return() {
        float t = 0;
        while (t < FlashRate) {
            GetComponent<Renderer>().material.color = Color.Lerp(Color.white, _originalColor, t / FlashRate);
            t += Time.deltaTime;
            yield return null;
        }
        GetComponent<Renderer>().material.color = _originalColor;
        StartCoroutine("Flash");
    }
}