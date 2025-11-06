using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoTileTexture : MonoBehaviour {

    public Vector2 scale = new Vector2(1f,100f);

    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        renderer.material.mainTextureScale = new Vector2(transform.lossyScale.x * scale.x, transform.lossyScale.y * scale.y);
    }
}
