using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NumberDisplay : MonoBehaviour {

    public TextMeshPro text;
    public float start = 0f;
    public float end = 0f;
    public float time = 2f;
    public string unit = "ml";

    float curr = 0f;

    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        curr += Time.deltaTime;
        if (curr < time) {
            text.text = Mathf.Lerp(start, end, curr / time).ToString("0") + unit;
        }
        else {
            text.text = end + unit;
        }
    }

    private void OnEnable() {
        curr = 0f;
    }
}
