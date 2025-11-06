using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ImageLabelListElement : MonoBehaviour
{
    int index = -1;
    public Image rowImage;
    public Button button;
    public Text label, vin, dateTime;
    public Text counterLabel;
    public string counterTextPrefix;
    public Text[] labels;
    private void Reset()
    {
        rowImage = GetComponent<Image>();
        button = GetComponent<Button>();
        if (!button)
            button = GetComponentInChildren<Button>();
        if (!label)
            label = GetComponentInChildren<Text>();
    }
    public void SetButtonIndexFunction(int listIndex,System.Action<int> SelectButton)
    {
        index = listIndex;
        Debug.Log("Added index " + index, gameObject);
        if (button != null)
            button.onClick.AddListener(() => SelectButton(index));
    }
    public ImageLabelListElement(Image img,Text labelText)
    {
        rowImage = img;
        if (labelText != null)
        label = labelText;
    }
    public ImageLabelListElement(Image img,Text[] labelText)
    {
        rowImage = img;
        labels = labelText;      
    }
    public void SetLabel(string newLabel)
    {
        if (labels != null)
            if (labels.Length > 0)
                labels[0].text = newLabel;
        if (counterLabel)
            counterLabel.text = counterTextPrefix + transform.GetSiblingIndex();
    }
    public void SetLabels(params string[] newLabels)
    {
        for (int i = 0; i < newLabels.Length; i++)
        {
            if (labels != null)
                if (labels[i] != null)
                {
             //       Debug.Log("Label:" + i);
                    labels[i].text = newLabels[i];
                }
        }
    }
    public ImageLabelListElement(Image img,Text[] labelTexts,Sprite sprite,string[] labelVals)
    {
        rowImage = img;
        labels = labelTexts;
        rowImage.sprite = sprite;
        if(labelTexts!=null)
        for (int  i = 0;  i <labels.Length;  i++)
        {
            labels[i].text = labelVals[i];
        }
       

    }
}
