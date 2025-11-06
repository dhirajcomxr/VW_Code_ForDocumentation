using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
[ExecuteInEditMode]
public class PartIdToStepFunction : MonoBehaviour
{
    [SerializeField] string partId,partResult,displayName;
    [SerializeField] bool searchNow = false;
    [SerializeField] PartHighlighter highlighter;
    [SerializeField] Text explodedViewLabel, stepsViewButtonText,partResultText,sureText;
    [SerializeField] InputField searchInput;
    [SerializeField] GameObject oneTimeUse;
    [SerializeField] public GameObject[] stepsView, explodedView;
    [SerializeField] StepsManager stepsManager;
    [SerializeField] PartListTable partList;
    [SerializeField] PartSequenceEximProcessor partSequence;
    GameObject selectedObject;
    // Start is called before the first frame update
    VWReferencesManager refManager;
    PartListTable.PartData partData;
    public void GoToStepsView()
    {
        ToggleGos(stepsView, true);
        ToggleGos(explodedView, false);
       

      //  if (stepsManager != null)
        //    stepsManager.OnNext();
    }
  
    public void GoToExplodedView()
    {
   
        ToggleGos(stepsView, false);
        ToggleGos(explodedView, true);
        if (selectedObject != null)
            SearchForPart(selectedObject);
    }
    public string PartName => partResult;
    void ToggleGos(GameObject[] gos,bool state)
    {
        foreach (var item in gos)
        {if (item != null)
                item.SetActive(state);
            else
                Debug.Log("NULL");
        }
    }
  
    // Update is called once per frame
#if UNITY_EDITOR
    void Update()
    {
        if (searchNow)
            if (partList != null && partSequence != null)
            {
                searchNow = false;
                SearchPartId(partId);           
            }

    }
#endif
    public string GetSelectedPartId() => partId;
    public string GetPartResult() => partResult;
    public void SearchForPart(GameObject g)
    {
        if (g != null)
        {
            Debug.Log("Search: " + g.name, g);
            highlighter.RemoveAllHighlights();
            highlighter.Highlight(g);
            selectedObject = g;
            SearchPartId(g.name);
        }
        else
        {
            highlighter.RemoveHighLight();
            selectedObject = null;
        }

        SetStepButton(g != null);
    }

    void SetStepButton(bool state)
    {
        stepsViewButtonText.transform.parent.gameObject.SetActive(state);
        if (explodedViewLabel)
        {
            explodedViewLabel.transform.parent.gameObject.SetActive(state);
            explodedViewLabel.text = displayName;
        }
        if (partResultText)
            partResultText.text = displayName;
        if(searchInput)
        {
            searchInput.onValueChanged.Invoke(partResult);
            searchInput.onEndEdit.Invoke(partResult);
        }
        if (sureText)
            sureText.text = "Are you sure you would want to"+System.Environment.NewLine+" view R&R for " + displayName+" ?";
        stepsViewButtonText.text = "View R&R for " + displayName;
      
        if (oneTimeUse)
            oneTimeUse.SetActive(partData.isOneTime);
    }
    public List<PartSequenceEximProcessor.PartSequence> GetFullSequence()
        => partSequence.GetFullSequence();
    public void ResetStepSequence()
    {
        partResult = "";
        partSequence.ResetSequence();
        UpdateRefManager();

    }
    public List<PartSequenceEximProcessor.PartSequence> GetResultsFor(string term) 
        => partSequence.GetResultsFor(term);
    public void PlaySequenceFor(string part_id)
    {
        partId = part_id;
        partData = partList.GetPartDataFor(partId);
        UpdateDisplayName(partData.linkedObjectName,partData.name);
    }
    void UpdateDisplayName(string result,string name)
    {
        partResult = partData.linkedObjectName;
        displayName = partData.name;
        UpdateRefManager();
    }
    void UpdateRefManager()
    {
        if (!refManager)
            refManager = FindObjectOfType<VWReferencesManager>();
        if (refManager)
            refManager.UpdateName();
    }
    public void PlaySequence(PartSequenceEximProcessor.PartSequence seq)
    {
        partSequence.PlaySequence(seq);
        partResult = seq.name;
        UpdateRefManager();
    }
    public void PlaySequence(string pResult)
    {
        if (pResult.Length > 0)
            partSequence.StepSequenceFromName(pResult);
    }
    public void SearchPartId(string search)
    {
        partId = search;
       partData = partList.GetPartDataFor(partId);
       // partResult = partList.GetLinkedObjectNameFor(partId);
        UpdateDisplayName(partData.linkedObjectName, partData.name);
        PlaySequence(partResult);
        //if (partResult.Length > 0)
        //{
        //    partSequence.StepSequenceFromName(partResult);
        //}
     //   displayName = partData.name;
       // partResult = partData.name;
    }
   
    private void Reset()
    {
        partList = FindObjectOfType<PartListTable>();
        partSequence = FindObjectOfType<PartSequenceEximProcessor>();
        stepsManager = FindObjectOfType<StepsManager>();
        highlighter = FindObjectOfType<PartHighlighter>();
    }
}
