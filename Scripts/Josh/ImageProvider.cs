using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Events;
public class ImageProvider : MonoBehaviour
{
  

    public static ImageProvider Get()
    {
        GameObject g = new GameObject("Image Provider");
        Debug.Log("Invoke Image Provider:", g);
      return g.AddComponent<ImageProvider>();
        
    }
    public void GetTexture(string pathName,UnityAction<Texture2D> returnFn)
    {
        if (File.Exists(pathName))
            returnFn(loadTexture(pathName));
    }
    public static bool Exists(string pathName)
    {
        return File.Exists(pathName);
    }
    public ImageProvider downloadImage(string url, string pathToSaveImage,UnityAction<Texture2D> returnFn)
    {
        //    WWW www = new WWW(url);
        UnityWebRequest www = new UnityWebRequest(url);
        StartCoroutine(_downloadImage(www, pathToSaveImage,returnFn));
        return this;
    }
    //public void downloadImage(string url, string pathToSaveImage)
    //{
    ////    WWW www = new WWW(url);
    //    UnityWebRequest www = new UnityWebRequest(url);
    //    StartCoroutine(_downloadImage(www, pathToSaveImage));
    //}

    private IEnumerator _downloadImage(UnityWebRequest www, string savePath,UnityAction<Texture2D> returnFn)
    {
        yield return www.SendWebRequest();
        //Check if we failed to send
        if (string.IsNullOrEmpty(www.error))
        {
            UnityEngine.Debug.Log("Success");
            //Save Image
            saveImage(savePath, www.downloadHandler.data);
            returnFn(GetTexture(www.downloadHandler.data));
        }
        else
        {
            Debug.Log("Error: " + www.error);
        }
    }

    public ImageProvider saveImage(string path, byte[] imageBytes)
    {
        //Create Directory if it does not exist
        if (!Directory.Exists(Path.GetDirectoryName(path)))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
        }

        try
        {
            File.WriteAllBytes(path, imageBytes);
            Debug.Log("Saved Data to: " + path.Replace("/", "\\"));
        }
        catch (Exception e)
        {
            Debug.LogWarning("Failed To Save Data to: " + path.Replace("/", "\\"));
            Debug.LogWarning("Error: " + e.Message);
        }
        return this;
    }
    //void saveImage(string path, byte[] imageBytes)
    //{
    //    //Create Directory if it does not exist
    //    if (!Directory.Exists(Path.GetDirectoryName(path)))
    //    {
    //        Directory.CreateDirectory(Path.GetDirectoryName(path));
    //    }

    //    try
    //    {
    //        File.WriteAllBytes(path, imageBytes);
    //        Debug.Log("Saved Data to: " + path.Replace("/", "\\"));
    //    }
    //    catch (Exception e)
    //    {
    //        Debug.LogWarning("Failed To Save Data to: " + path.Replace("/", "\\"));
    //        Debug.LogWarning("Error: " + e.Message);
    //    }
    //}
    Texture2D GetTexture(byte[] imgBytes)
    {
        Texture2D result = null;
        if (imgBytes.Length > 0)
            result.LoadImage(imgBytes);
        return result;
    }
    public Texture2D loadTexture(string path)
    {
        Texture2D result = null;
        byte[] imgBytes = loadImage(path);
        result = GetTexture(imgBytes);
            return result;
    }
    public byte[] loadImage(string path)
    {
        byte[] dataByte = null;

        //Exit if Directory or File does not exist
        if (!Directory.Exists(Path.GetDirectoryName(path)))
        {
            Debug.LogWarning("Directory does not exist");
            return null;
        }

        if (!File.Exists(path))
        {
            Debug.Log("File does not exist");
            return null;
        }

        try
        {
            dataByte = File.ReadAllBytes(path);
            Debug.Log("Loaded Data from: " + path.Replace("/", "\\"));
        }
        catch (Exception e)
        {
            Debug.LogWarning("Failed To Load Data from: " + path.Replace("/", "\\"));
            Debug.LogWarning("Error: " + e.Message);
        }

        return dataByte;
    }
}
