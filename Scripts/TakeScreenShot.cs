using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TakeScreenShot : MonoBehaviour
{

    [SerializeField] GameObject showSreenshot;
    public ApplicationErrorScreen applicationErrorScreen;
   public void ScreenShot()
    {
        StartCoroutine(CaptureScreenShotAsTxture());
    }

    //Capture SS as texture to send it on the Dashborad
    IEnumerator CaptureScreenShotAsTxture()
    {
    yield return new WaitForEndOfFrame();
    applicationErrorScreen.errorScreenshot = ScreenCapture.CaptureScreenshotAsTexture();
        Debug.Log("TEXTURE...." + applicationErrorScreen.errorScreenshot.name);
        Invoke("Open", 0.5f);
        Texture2D tex = new Texture2D(applicationErrorScreen.errorScreenshot.width, applicationErrorScreen.errorScreenshot.height, TextureFormat.RGB24, false);

        tex.ReadPixels(new Rect(0, 0, applicationErrorScreen.errorScreenshot.width, applicationErrorScreen.errorScreenshot.height), 0, 0);
        tex.Apply();
        Debug.Log(tex + " texture........");
        showSreenshot.GetComponent<RawImage>().texture = tex;
        //showSreenshot.GetComponent<Image>().sprite = Sprite.Create(tex);
        yield return null;

    }
    public void Open()
    {
        applicationErrorScreen.OnError();
    }
}
