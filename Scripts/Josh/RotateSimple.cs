using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateSimple : MonoBehaviour
{
  public Vector3 axis;
  public float speed = 1f;
  
    void Update()
    {
        transform.Rotate(axis * speed * Time.deltaTime);
    }
}
