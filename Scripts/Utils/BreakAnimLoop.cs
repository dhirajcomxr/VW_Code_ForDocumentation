using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakAnimLoop : MonoBehaviour {

    public bool isStopped = false;

    public void StopAnim() {
        if (isStopped) {
            GetComponent<Animator>().enabled = false;
        }
    }
}
