using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WrenchHelper : MonoBehaviour
{

    [Range(0.1f, 2f)]
    public float startDelay = 0.5f;
    public enum axis { x, y, z };
    [Header("This will rotate the child object")]
    public float angle = 0f;
    public float animDuration = 1.5f;
    public axis _axis;
    public bool disableOnCompletion = true;
    Transform[] childTransforms;

    Vector3[] startRotation;


    // Start is called before the first frame update
    void Start()
    {
        //childTransforms = transform.GetComponentsInChildren<Transform>();
        for (int i = 0; i < childTransforms.Length; i++)
        {
            startRotation[i] = childTransforms[i].rotation.eulerAngles;
        }
        //startRotation = transform.GetChild(0).rotation.eulerAngles;

    }
    void Awake()
    {
           int children = transform.childCount;
        childTransforms = new Transform[children];
        for (int i = 0; i < children; ++i)
        {
            //print("For loop: " + transform.GetChild(i));
            childTransforms[i] = transform.GetChild(i);
        }
        startRotation = new Vector3[childTransforms.Length];
    }
    private void OnEnable()
    {

       
     
      
        for (int i = 0; i < childTransforms.Length; i++)
        {
            if (startRotation[i] != Vector3.zero)
            {
                childTransforms[i].rotation = Quaternion.Euler(startRotation[i]);

            }
            else
            {
                startRotation[i] = childTransforms[i].rotation.eulerAngles;

            }
        }


        Invoke("RotateObj", startDelay);
    }

    // Update is called once per frame
    void Update()
    {

    }

    void RotateObj()
    {
        for (int i = 0; i < childTransforms.Length; i++)
        {
            float finalRot = 0f;
            switch (_axis)
            {
                case axis.x:
                    finalRot = startRotation[i].x - angle;
                    break;
                case axis.y:
                    finalRot = startRotation[i].y - angle;
                    break;
                case axis.z:
                    finalRot = startRotation[i].z - angle;
                    break;
            }
            iTween.RotateTo(childTransforms[i].gameObject, iTween.Hash(_axis, finalRot, "time", animDuration, "easeType", iTween.EaseType.easeOutQuad));
        }
        if (disableOnCompletion)
        {
            Invoke("DisableGameobject", animDuration + 0.2f);
        }
    }

    void DisableGameobject()
    {
        for (int i = 0; i < childTransforms.Length; i++)
        {
            childTransforms[i].eulerAngles = startRotation[i];
        }
        gameObject.SetActive(false);



    }
}
