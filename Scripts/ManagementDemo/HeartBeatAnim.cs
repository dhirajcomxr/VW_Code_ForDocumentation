using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeartBeatAnim : MonoBehaviour {
    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }

    private void OnEnable() {
        if (GetComponent<iTween>() != null) {
            DestroyImmediate(GetComponent<iTween>());
        }
        transform.localScale = Vector3.one;
        iTween.PunchScale(this.gameObject, iTween.Hash("amount", Vector3.one * 0.1f, "time", 1f, "delay", 0f, "looptype", "loop"));
    }
}
