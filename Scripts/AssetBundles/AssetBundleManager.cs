using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class AssetBundleManager : MonoBehaviour {

    public string assetBundleUrl = "";

    // Start is called before the first frame update
    void Start() {
        StartCoroutine(LoadBundle());
    }

    // Update is called once per frame
    void Update() {

    }

    IEnumerator LoadBundle() {
        var uwr = UnityWebRequestAssetBundle.GetAssetBundle(assetBundleUrl);
        yield return uwr.SendWebRequest();

        // Get an asset from the bundle and instantiate it.
        AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(uwr);
        var loadAsset = bundle.LoadAssetAsync<GameObject>("main.prefab");
        yield return loadAsset;

        Instantiate(loadAsset.asset);
    }
}
