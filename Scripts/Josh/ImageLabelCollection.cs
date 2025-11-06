using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class ImageLabelCollection:MonoBehaviour
{
   public virtual void Load(Sprite[] images, string[] labels)
    {
        Debug.Log("Got images" + images.Length + " and Labels " + labels);
    }

    public virtual void Load(string[] labels, string[] vin, string[] datetime)
    {
        Debug.Log("Labels " + labels + " and VIN " + vin +" DateTime: "+ datetime);
    }
    public virtual void Load(Sprite[] images) => Load(images, null);
    public virtual void Load(string[] labels) => Load(null, labels);
    public virtual void LoadLabels(params string[] labels)
    {
        Load(null, labels);
        Debug.Log("Got Labels " + labels);
    }
}
//}
//public interface IImageLabelCollection
//{
//     void Load(Sprite[] images, string[] labels);
//}
//public interface ImageCollection
//{
//    void Load(params Sprite[] images);
//}
//public interface LabelCollection
//{
//    void Load(params string[] labels);
//}

