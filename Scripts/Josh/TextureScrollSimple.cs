using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureScrollSimple : MonoBehaviour
{

   public float scrollSpeed  = 0.5f, offset ;
    Renderer r;
    private void Start()
    {
        r = GetComponent<Renderer>();
    }
    void Update()
    {
        offset += (Time.deltaTime * scrollSpeed) / 10.0f;
        r.material.SetTextureOffset("_MainTex",new Vector2(offset, 0));

    }
}
