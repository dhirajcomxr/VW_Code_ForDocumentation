using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode]
public class AppApiControl : MonoBehaviour
{
    [SerializeField] AppApiManager.ServerType serverType;
    [SerializeField] AppApiManager apiManager;
    // Start is called before the first frame update
    private void Reset()
    {
        apiManager = FindObjectOfType<AppApiManager>();
        if(apiManager!=null)
        serverType = apiManager.serverType;
    }
    void Start()
    {
      
    }
#if UNITY_EDITOR
    // Update is called once per frame
    void Update()
    {
        if (serverType != apiManager.GetServerType())
            apiManager.SetServerType(serverType);
    }
#endif
}
