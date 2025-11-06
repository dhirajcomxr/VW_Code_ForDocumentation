using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
[System.Serializable]
public class ImageLabelList : ImageLabelCollection
{
    [SerializeField]  public ImageLabelListElement rowElement;
    [SerializeField] UnityEvent<int> OnSelect;
    [SerializeField] GameObject[] disableIfListEmpty, enableIfListEmpty;
    [SerializeField] bool disableIfNoRowElements = true, disableParentIfNoElement = true;
    [SerializeField] bool debug = false;
    [SerializeField] int totalBtns = 0;
    int totalImgsLoaded = 0;
    //    public List<ImageLabelListElement> rowElements;
    int count;
    public int GetCount() => count;
    public void Load(Sprite[] images, bool labelFromName)
    {
        Load(images);
        if (labelFromName)
        {
            ImageLabelListElement[] elementArray = GetComponentsInChildren<ImageLabelListElement>();
            if(elementArray.Length>0)
            {
                foreach (ImageLabelListElement item in elementArray)
                {
                    if(item.label!=null && item.rowImage.sprite != null)
                    //if (item.label != null)
                    item.label.text=(item.rowImage.sprite.name);
                }
            }
        }
    }

    public override void Load(string[] labels, string[] vin, string[] datetime)
    {
        CleanList();
        totalBtns = 0;
        if (labels != null && vin!=null && datetime != null) 
        {
            for(int i = 0; i < labels.Length; i++)
            {
                int curIndex = i;
                ImageLabelListElement rowEl = Instantiate(rowElement, transform);
                InitButton(rowEl, curIndex);
                if (labels.Length > 0)
                {
                    rowEl.label.text = labels[i].Replace("_", "/");
                    //Debug.Log("LABEL 2:" + vin[i]);
                    rowEl.vin.text = vin[i].Replace("_", "/");
                    rowEl.dateTime.text = datetime[i].Replace("_", "/");
                }
                //i += 2;
                //Debug.Log("i------ " + i);
            }
        }

        bool nullLabels = (labels == null);
        if (!nullLabels)
            count = labels.Length;
        // Debug.Log("Condition:" +condition + " count: " + count);
        ToggleGoOnElementCount(transform.parent.gameObject, disableParentIfNoElement, nullLabels ? 0 : labels.Length);
        ToggleGoOnElementCount(gameObject, disableIfNoRowElements, nullLabels ? 0 : labels.Length);
        ToggleOnElementCount();
        if (debug)
            Debug.Log("Loaded with " + count + " elements");
    }

    //Ticket details shown here
    public override void Load(Sprite[] images, string[] labels) 
    {
        CleanList();
        int totalLoadedL = -1;
         totalBtns = 0;
        if (images != null)
        {
            for (int i = 0; i < images.Length; i++)
            {
                ImageLabelListElement rowEl = Instantiate(rowElement, transform);
                string nextLabel="";
                if (images != null)
                {
                    if (images[i] != null)
                    {
                        totalImgsLoaded++;
                        rowEl.rowImage.sprite = images[i];
                        rowEl.rowImage.preserveAspect = true;
                        nextLabel = images[i].name.Length > 0 ? images[i].name.Replace("_", "/") : "";
                        if (labels != null)
                            if (labels[i].Length > 0)
                                nextLabel = labels[i];
                    }
                    else
                        if (labels != null)
                        if (labels[i].Length > 0)
                            rowEl.SetLabel(labels[i]);
                }
                if (labels != null)
                    if(i<labels.Length)
                    if (labels[i] != null)
                        nextLabel = labels[i];
                rowEl.SetLabel(nextLabel);

                //          rowElements.Add(rowEl);
            }
        }
        else
            if (labels != null)
            for (int i = 0; i < labels.Length; i++)
            {
                int curIndex = i;
                ImageLabelListElement rowEl = Instantiate(rowElement, transform);
                InitButton(rowEl, curIndex);
                
                //if (rowEl.button != null)
                //{
                //    totalBtns++;
                //    rowEl.button.onClick.AddListener(() => Select(curIndex)); // OnSelect.Invoke(curIndex));
                //}
                if (labels.Length > 0)
                {
                    rowEl.label.text = labels[i].Replace("_","/");
                    //Debug.Log("LABEL 2:" + labels[i + 1]);
                    //rowEl.vin.text = labels[i + 1].Replace("_", "/");
                    //rowEl.dateTime.text = labels[i+2].Replace("_", "/");
                }
                //i += 2;
                //Debug.Log("i------ " + i);
            }
        bool nullLabels = ( labels == null);
        if(!nullLabels)
        count = labels.Length;
       // Debug.Log("Condition:" +condition + " count: " + count);
        ToggleGoOnElementCount(transform.parent.gameObject, disableParentIfNoElement, nullLabels ? 0 : labels.Length);
        ToggleGoOnElementCount(gameObject, disableIfNoRowElements,nullLabels ? 0 : labels.Length);
        ToggleOnElementCount();
        if(debug)
            Debug.Log("Loaded with " + count + " elements");
    }


    void ToggleOnElementCount()
    {
        int totalElements = transform.childCount;
        
        if (totalElements > 0)
        {
            Toggle(enableIfListEmpty, false);// disable if not
            Toggle(disableIfListEmpty, true);// enable if not
        }
        else
        {
            Toggle(enableIfListEmpty, true);
            Toggle(disableIfListEmpty, false);
        }
        void Toggle(GameObject[] gos,bool state)
        {
            foreach (var item in gos)
            {
                if (item != null)
                    item.SetActive(state);
            }
        }
    }
    void InitButton(ImageLabelListElement rowEl,int curIndex) {
        if (rowEl.button != null)
        {
            totalBtns++;
            rowEl.button.onClick.AddListener(() => Select(curIndex)); // OnSelect.Invoke(curIndex));
            //Debug.LogError(rowEl.label);
        }
    }
    public void Select(int id)
    {
        if (debug)
            Debug.Log("Select " + id);

        //Steps s = FindObjectOfType<Steps>();
        //if (s != null && s.steps.Count<id-5)
        //{    
            OnSelect?.Invoke(id);
        //}
        //else
        //{
        //    Debug.LogError("ASSEMBLY STEP... IGNORE");
        //}
    }
    public void LoadLabels(string[] labels1,string[] labels2)
    {
        CleanList();
        if (rowElement.labels.Length == 2)
        {
      
            for (int i = 0; i < labels1.Length; i++)
            {
                ImageLabelListElement rowEl = Instantiate(rowElement, transform);
                rowEl.SetLabels(labels1[i], labels2[i]);
                InitButton(rowEl, i);
            }
        }
    }
    public override void Load(string[] labels)
    {
        Load(null,labels);
    }

    void ToggleGoOnElementCount(GameObject obj,bool condition,int totalElements)
    {
        if (condition)
            obj.SetActive(totalElements > 0);
     //   Debug.Log("Total:" + totalElements);
    }
    
    public override void Load(Sprite[] images)
    {
        Load(images, null);
    }
    public override void LoadLabels(params string[] labels)
    {
        base.LoadLabels(labels);
    }
    void SetImageLabelFor(ImageLabelListElement listElement,Sprite img,string label)
    {

        if (img != null)
        {
            listElement.rowImage.sprite = img;
            listElement.rowImage.preserveAspect = true;
        }
                listElement.SetLabel(label);
    }
    void CleanList()
    {
      if(transform.childCount>0)
            for (int i = 0; i < transform.childCount; i++)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
   //     rowElements = new List<ImageLabelListElement>();
     
    }

}
