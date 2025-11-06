using System.Collections.Generic;
using UnityEngine;

public class Highlighter : MonoBehaviour {

    public bool selfDestruct = false;
    public List<Color> initColor;
    Color highlightColor;
    StepsManager steps;

    private void OnEnable() {
        steps = StepsManager.Instance;
        highlightColor = steps.highlightColor;

        initColor = new List<Color>();

        if (GetComponent<Highlighter>() != this) {
            Destroy(GetComponent<Highlighter>());
        }

        if (this.GetComponent<Renderer>() != null) {
            // save the current colors
            foreach (Material m in GetComponent<Renderer>().materials) {
                initColor.Add(m.color);
            }
            // highlight this object since it has a mesh renderer 
            iTween.ColorTo(this.gameObject, iTween.Hash("color", highlightColor, "time", 0.25f, "LoopType", "pingpong", "includechildren", false));
        }
        else {
            // since this object does not have a renderer
            // attach this script to all child renderers
            foreach (MeshRenderer mesh in GetComponentsInChildren<MeshRenderer>(false)) {


                if (mesh.gameObject.GetComponent<Highlighter>() != null) {
                    mesh.gameObject.GetComponent<Highlighter>().enabled = true;
                }
                else {
                    mesh.gameObject.AddComponent<Highlighter>();
                }
            }
        }
        if (selfDestruct) {
            Invoke("ResetAndDestroy", 2f);
        }
    }

    private void OnDisable() {
        ResetAndDestroy();
    }

    private void ResetAndDestroy() {
        if (GetComponent<MeshRenderer>() != null) {
            // remove highlighter for this object and reset colors
            if (GetComponent<iTween>() != null) {
                foreach (iTween itween in GetComponents<iTween>()) {
                    DestroyImmediate(itween);
                }
                for (int i = 0; i < GetComponent<MeshRenderer>().materials.Length; i++) {
                    GetComponent<MeshRenderer>().materials[i].color = initColor[i];
                }
            }
        }
        else {
            // remove highlighter from all child objects
            // that instance of the highlighter will reset the colors for that mesh
            foreach (MeshRenderer mesh in GetComponentsInChildren<MeshRenderer>()) {
                if (mesh.gameObject.GetComponent<Highlighter>() != null) {
                    mesh.gameObject.GetComponent<Highlighter>().enabled = false;
                }
            }
        }
        Destroy(this);
    }
}