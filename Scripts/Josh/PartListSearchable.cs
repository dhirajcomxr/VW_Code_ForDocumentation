using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
[ExecuteInEditMode]
public class PartListSearchable : MonoBehaviour
{

    public string searchTerm;
    public PartSequenceEximProcessor table;
    public List<PartSequenceEximProcessor.PartSequence> results;
    public SearchableListWidget listWidget;
    public PartIdToStepFunction idToStepFunction;
    [Space(10)]
    [SerializeField] GameObject sureDialog;
    [SerializeField] Text areYouSureText;
    string lastSearchTerm;
    int tableSize = 0;
    List<string> listResults;
    bool isEditor = false;
  
    // Start is called before the first frame update
    private void Reset()
    {
    //    table = FindObjectOfType<PartSequenceEximProcessor>();
        listWidget = FindObjectOfType<SearchableListWidget>();
        idToStepFunction = FindObjectOfType<PartIdToStepFunction>();
        ResetResults();
    }
    string lastSearch = "";
    public void SearchFor(string term)
    {
        if (lastSearch != term)
        {
            searchTerm = term;
            if (term.Length < 1)
            {
                Debug.Log("Init Reset");
                ResetResults();
            }
            if (searchTerm.Length > 0)
            {
                if (searchTerm != lastSearchTerm)
                {
                    results = idToStepFunction.GetResultsFor(searchTerm);
                    lastSearchTerm = searchTerm;
                    UpdateUiList();
                }
            }
            else
            if (tableSize > 0)
            {
                if (results.Count < tableSize)
                    ResetResults();
            }
            else
                ResetResults();
            lastSearch = term;
        }
    }
    void UpdateUiList()
    {
        if (!isEditor)
        {
            listResults = new List<string>();
            for (int i = 0; i < results.Count; i++)
            {
                listResults.Add(results[i].name);
            }
            listWidget.LoadResults(listResults);
        }
    }
    public void OnListSelected(int ip)
    {
        if (results.Count < 1)
            ResetResults();

        Debug.Log("SELECTED:" + ip+":"+results.Count);
            Debug.Log(":"+ results[ip].name);
        if (areYouSureText)
            areYouSureText.text = "View R&R steps for " + results[ip].name+"?";
        if (sureDialog)
            sureDialog.SetActive(true);

      //  table.PlaySequenceFor(results[ip]);
       // table.PlaySequence(results[ip]);
        idToStepFunction.PlaySequence(results[ip]);
      //  idToStepFunction.PlaySequenceFor(results[ip].name);
    }
    public void ResetSeq() => idToStepFunction.ResetStepSequence();
  public  void ResetResults()
    {

        Debug.Log("RESET!");
        if (table)
        {
            searchTerm = "";
            ResetSeq();
            results = idToStepFunction.GetFullSequence();
         
            tableSize = results.Count;
            UpdateUiList();
        }
    }
#if UNITY_EDITOR
    // Update is called once per frame
    void Update()
    {
        isEditor = false;
        SearchFor(searchTerm);
    }
#endif

}
