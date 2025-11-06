using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SearchableDTList : MonoBehaviour
{
   public DiagnosticTreeManager DTManager;
   public AppApiManager ApiManager;
    public UIScreenManager ScreenManager;
    public StartScreen3dView startScreen3DView;
   // public InputField SearchBox;
    // Start is called before the first frame update
    public AppApiManager.Diagnostic_Trees[] DiagArray;
    List<string> IssueTile;
   // string ScanOption;
    public SearchableListWidget SearchResult;
    public GameObject[] SearchResultPanel;
  private string SearchValue;

  
   public void GetSearchDT (string searchTerm)
    {
      //  SearchResult.searchBox.text = startScreen3DView.modName.ToString();
        if (SearchResult.searchBox.text!=searchTerm)
        {
            SearchResult.searchBox.text = searchTerm;
            //SearchTerm = searchTerm;
        }
        // ApiManager.GetDTIdForTerm(searchTerm.text, OnResponseGetDT);
        SearchValue = searchTerm;
        Invoke("ResponesOnSearch", 0.3f);

    }

    public void SearchDTs(Text text)
    {
       string SearchTerm = text.text;
        Debug.Log(SearchTerm);

        if (SearchResult.searchBox.text!=SearchTerm)
        {
            SearchResult.searchBox.text = SearchTerm;
            //SearchTerm = searchTerm;
        }
       ApiManager.GetDTIdForTerm(SearchTerm, OnResponseGetDT);

    }
    public void ResponesOnSearch()
    {     
        ApiManager.GetDTIdForTerm(SearchValue, OnResponseGetDT);
    }
    private void OnResponseGetDT(string data)
    {
        Debug.Log("DiagArray     "+DiagArray.Length);       
        DiagArray = AppApiManager.GetServerResponse(data).response_data.diagnostic_trees;       
        IssueTile = new List<string>();
        Debug.Log("Issue Title:  "+IssueTile.Count);
        if (DiagArray!=null)
        {
            for (int i = 0; i < DiagArray.Length; i++)
            {
                IssueTile.Add(DiagArray[i].IssueTitle);
            }
            SearchResult.LoadResults(IssueTile);
        }else if (DiagArray == null)
        {
            SearchResult.LoadResults(IssueTile);
        }
      
        //Debug.Log("Got DT ID: " + data);
        //Debug.Log("Issue Tiltle:  " + serverData.diagnostic_trees[0].IssueTitle);
    }

    //On selecting a DT from the List
    public void OnListSelect(int DTNo)
    {
        TextAsset DTText = new TextAsset(DiagArray[DTNo].DT_JSON);
        // DTManager.LoadData(DTText);
        Debug.Log("<Color=blue>DT Json Scan Type</color>" + DiagArray[DTNo].DT_JSON);
  

        DTManager.DtLink = DiagArray[DTNo].FilesContainer;
        DTManager.LoadDtFile(DTText,IssueTile[DTNo]);
        // player.Load(treeFiles[index]);
        DTManager.Download_DT_Images();
    /*  ScreenManager.SelectScreen(ScreenLinker.ScreenID.DT);
        DTManager.Begin();*/
    }

    public void ClearSearchResult()
    {
        SearchResult.resultList.Clear();
    }

    void Start()
    {
      
    }

    // Update is called once per frame
    void Update()
    {
    
        
    
    }
}
