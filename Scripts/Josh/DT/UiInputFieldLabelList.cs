using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UiInputFieldLabelList : MonoBehaviour
{
  [SerializeField]  UiInputFieldLabel inputLabel;
    [SerializeField] List<UiInputFieldLabel> inputLabelList;
   public void Setup(string[] names,string inputFieldPrompt)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
          
        }
        inputLabelList = new List<UiInputFieldLabel>();
        for (int i = 0; i < names.Length; i++)
        {
            UiInputFieldLabel lab = Instantiate(inputLabel, transform);
            lab.label.text = names[i];
            lab.inputField.placeholder.GetComponent<Text>().text = inputFieldPrompt; 
            inputLabelList.Add(lab);
        }
    }
    public List<string> GetAllInputs()
    {
        int totalInputs = inputLabelList.Count;
        List<string> result= new List<string>(totalInputs);

        for (int i = 0; i < totalInputs; i++)
        {
            string cur = "";
            InputField inputField = inputLabelList[i].inputField;
            if(inputField)
            if (inputField.text.Length > 0)
                cur = inputField.text;
            result.Add(cur);
        }
        return result;
    }
    public void Setup(string labelPrefix,int numberOfInputs)
    {
        List<string> nameList = new List<string>();
        for (int i = 0; i < numberOfInputs; i++)
        {
            nameList.Add(labelPrefix + " " + i);
        }
        Setup(nameList.ToArray(), "Enter Value");
    }
}
