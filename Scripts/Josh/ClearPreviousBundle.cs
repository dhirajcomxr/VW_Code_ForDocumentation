using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearPreviousBundle : MonoBehaviour
{
    [SerializeField]GameObject abc;

    public void checkBundleexist()
    {

        GameObject gm = FindObjectOfType<BundleLoader>().gameObject;
        abc = gm;
        if (FindObjectOfType<BundleLoader>() != null || abc!=null)
        {
            Invoke("abcv", 4f);
            //Destroy(FindObjectOfType<BundleLoader>());
            //Destroy(abc);
        }
    }
    void abcv()
    {
        abc.GetComponent<BundleLoader>().End();
        Debug.Log("   Bundle Exist ");
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
