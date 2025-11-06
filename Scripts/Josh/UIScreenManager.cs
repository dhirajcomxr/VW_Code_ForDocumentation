using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Linq;
using mixpanel;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class UIScreenManager : MonoBehaviour
{
    public TutorialSlideShow tutorialSlideShow;
    [SerializeField] AssetLoader assetLoader;
    [SerializeField] bool enableDebug = true;
    public ScreenLinker linker;
    public Dialog dialog;
    public GameObject headerPanel,headerOptions, optionsPanel,loadingScreen, loadingAnimated,
        expandedOptionsPanel,homeConfirmation,appOptions;
   
    [SerializeField]  Button loadingCancelButton;
    public Text title;
    public Text loadingText,toastTxt,debugTxt;
    public Image loadingFillImage;
    public delegate void FloatUpdate(float val);
    public FloatUpdate CheckPercent;
    int openAfterLoading = -1;
    [Header("Screen Settings")]

    bool renameOnIntialise=true;
    public int startwithScreen;
   [SerializeField]
    List<int> lastScreens;

    int maxRewind = 8, homeScreen=15;
  [SerializeField]  int currentScreen = -1;
    public UIScreen[] screens;
   public UIScreen lastSelectedScreen;


    [Space(10)]
    [Header("Button Settings")]
    [SerializeField] GameObject externalButtonSource;
    UIButton[] buttons;
    [SerializeField] UIButtonToScreen[] buttonScreenNames;
    
    [System.Serializable]
    public class Dialog
    {        
        public GameObject dialog,buttonHolder;
        public Text titleTxt, messageTxt;
        public Button mainButton;
        public List<Button> buttons;
        public List<UiButtonAction> buttonActions;
        public void Show(string title,string message) {
            Get(title, message);
        }
        public void Get(string title,string message)
        {
            Get(title, message, "OK", Close);
        }
        public Dialog ShowDirect(string title,string message,string btnName,UnityAction action)
        {
            Get(title, message, btnName, action);
            Get(title, message, new List<NamedActions>{new NamedActions(btnName, () =>
             {
                 action.Invoke();
                 Close();
             }) });
            return this;
        }
        public Dialog Show(string title, string message, string btnName, UnityAction action)
        {
            Get(title, message, new List<NamedActions> { new NamedActions("Cancel", Close) , new NamedActions(btnName, action)});
            return this;
        }
        public void Show(string title,string message,List<NamedActions> namedActions)
        {
            Get(title, message, namedActions);
        }
        Dialog Get(string title, string message, string btnName, UnityAction action)
        {
            Get(title, message, new List<NamedActions> {new NamedActions(btnName,action) });
            return this;
        }
        Dialog Get(string title, string message, List<NamedActions> namedActions)
        {
            if(titleTxt)
            titleTxt.text = title;
            if(messageTxt)
            messageTxt.text = message;
            foreach (var item in buttons)
            {
                Destroy(item.gameObject);
            }
            buttons = new List<Button>();
            foreach (var item in namedActions)
            {
                Button btn = Instantiate(mainButton, buttonHolder.transform);
                Text btnTxt = btn.GetComponentInChildren<Text>();
                btnTxt.text = item.name;
                btn.onClick.AddListener(item.action);
                buttons.Add(btn);
            }
            dialog.SetActive(true);
            return this;
        }

        public void Close()
        {
            dialog.SetActive(false);
        }

    }
    [System.Serializable]
   public class UiButtonAction
    {
        public string name = "";
        public Button button;
        public UnityAction action;
        public UiButtonAction(string btnName,Button btn, UnityAction onClick)
        {
            name = btnName;
            button = btn;
            button.name = name;
            Text txt = button.GetComponentInChildren<Text>();
            if(txt)
            txt.text = name;
            action = onClick;
            button.onClick.AddListener(action);
        }
    }
    [System.Serializable]
    public class NamedActions
    {
        public string name;
        public UnityAction action;
        public NamedActions(string btnName,UnityAction btnAction)
        {
            name = btnName;
            action = btnAction;
        }
    }
    [System.Serializable]
    class UIButtonToScreen
    {
        public string action;
        public UIButton button;
        public UIScreen screen;
        public int index;
     
        public UIButtonToScreen(UIScreen s,UIButton uib)
        {
            screen = s;
            button = uib;
            string sName = "ERROR";
            if (screen != null)
                sName = screen.name;
            else
                Debug.Log("Error: screen doesn't Exist for "+uib.name, uib.gameObject);
            action = button.name+" >> "+ sName;
            index = uib.index;
        }
    }
    // Start is called before the first frame update
   
    void Start()
    {
       Screen.SetResolution(1280, 800, true);
        enableDebug = (linker.GetApiManager().GetServerType() == AppApiManager.ServerType.Development);
          
        //  Toast("<color=blue> Set Resolution to .1280,800 </color>: "+ QualitySettings.GetQualityLevel());
        //foreach (var item in Screen.resolutions)
        //{
        //    Debug.Log("<color=blue> W " + item.width + ": H " + item.height + "</color>");
        //}
  //    dialog.Show("Test", "Test Message");
    }
    AssetLoader loader;

    public AppApiManager APIM;
    // Update is called once per frame
    void Update()
    {
       // APIM.SessionID();
    }
    #region EditorTestFunctions
#if UNITY_EDITOR
    public void _EditorGoNext()
    {if (IsLoading())
            SetLoadingState(false);
        else
        {
            currentScreen++;
            if (currentScreen >= screens.Length)
                currentScreen = 0;
            _EditorGoScreenSelect(currentScreen);
        }
    }
   public void _EditorGoPrev()
    {
        if (IsLoading())
            SetLoadingState(false);
        else
        {
            currentScreen--;
            if (currentScreen < 0)
                currentScreen = screens.Length - 1;
            _EditorGoScreenSelect(currentScreen);
        }
    }
   public void _EditorGoScreenSelect(int sc)
    {
        if (screens[sc] != null) { 
        foreach (var item in screens)
        {
            if(item!=null)
            item.gameObject.SetActive(false);
        }
                SetHeaderState(screens[sc].enableHeader);
                SetOptionState(screens[sc].enableOptions);
            SetExpandedOptionsState(screens[sc].expandOptions);
            SetAppOptionsState(screens[sc].showAppOptions);
          
                //       lastSelectedScreen.gameObject.SetActive(false);
                screens[sc].gameObject.SetActive(true);
                title.text = screens[sc].screenName;
                lastSelectedScreen = screens[sc];
            currentScreen = sc;
        }
    }
    public void OnAppError()
    {
      Texture2D errorTex=  ScreenCapture.CaptureScreenshotAsTexture();
        
    }
#endif
    #endregion
    private void Reset()
    {
        _InitializeScreens();
       // _InitialiseButtons();
        SelectScreen(0);
    }
    private void OnEnable()
    {
     //   AddButtonListeners();
     //   Loading();
        SelectScreen(startwithScreen);
    }
    private void OnDisable()
    {
       // RemoveButtonListeners();
    }
    #region Remove
    void RemoveButtonListeners()
    {
        if (buttonScreenNames.Length > 0)
            for (int i = 0; i < buttonScreenNames.Length; i++)
            {
                Debug.Log("RemoveButtonListeners.................." + buttonScreenNames[i].button.index);
                buttonScreenNames[i].button.button.onClick.RemoveListener(delegate { SelectScreen(buttonScreenNames[i].button.index); });
            }
    }
  public  void _InitialiseButtons()
    {
        UIButton[] buttonsS = GetComponentsInChildren<UIButton>();

        UIButton[] extButtons =externalButtonSource.GetComponentsInChildren<UIButton>();
        UIButton[] buttons = buttonsS.Concat(extButtons).ToArray();
        if (buttons.Length > 0)
        {

            buttonScreenNames = new UIButtonToScreen[buttons.Length];
            for (int i = 0; i < buttons.Length; i++)
            {
                //  buttons[i].button.onClick.AddListener(delegate { SelectScreen(buttons[i].index); });
                if (buttons[i] != null)
                    buttonScreenNames[i] = new UIButtonToScreen(screens[buttons[i].index], buttons[i]);
                else
                    DebugLog("Button:" + i + " is null");
            }
        }
    }
    void AddButtonListeners()
    {
        if (buttonScreenNames.Length > 0)
            for (int i = 0; i < buttonScreenNames.Length; i++)
            {
                int k = i;
                int n = buttonScreenNames[k].index;
           //     DebugLog("ADDED TO:" + i);
                buttonScreenNames[i].button.button.onClick.AddListener(delegate {
            //        OnButtonClick(buttonScreenNames[k].button);
                    //DebugLog("CLICK" + n, buttonScreenNames[k].button.gameObject);
                    //SelectScreen(n);
                });
            }
    }

    void OnButtonClick(UIButton btn)
    {
        DebugLog("Open Screen:" + btn.index, btn.gameObject);
        //   SelectScreen(btn.index);
    }

    #endregion
    public void Home()
    {
        lastScreens.Clear();
        SelectScreen(homeScreen);
    }
    public void DebugToScreen(string msg)
    {
        if (enableDebug)
        {
            if (debugTxt)
            {
                debugTxt.gameObject.SetActive(false);
                debugTxt.gameObject.SetActive(true);
                debugTxt.text = msg;
            }
            DebugLog("<color=white>[DEBUG-SCREEN]</color>" + msg);
        }
    }
    public void Toast(string message)
    {
        GameObject par = toastTxt.transform.parent.gameObject;
        par.SetActive(false);
        par.SetActive(true);
        toastTxt.text = message;
        DebugLog("<color=white>[Toast]</color>"+message);
    }
    public void Loading(float perc)
    { 
    
        DebugLog("Loading.." + perc+" AFL: "+openAfterLoading);
        loadingAnimated.SetActive(perc == 0f);
        //loadingAnimated.SetActive(perc==1f);
        if (perc <1f)//on 0.999 scene was not loaded properly ,hence  change to 1
        {
            if (!loadingScreen.activeSelf)
            {
                loadingScreen.SetActive(true);
            }
            loadingFillImage.fillAmount = perc;
            if (perc > 0)
                loadingText.text = "Loading " + (perc * 100f).ToString("F1") + " % ";
            //else
            //{
            // //   loadingText.text = "Loading...";
            //}
        }
        else if (IsLoading())
        {    
            SetLoadingState(false);
        }

    }
    public bool IsLoading() => (loadingScreen != null) ? loadingScreen.activeSelf : false;
    public void Loading()
    {
        SetLoadingState(true);
    }

   public IEnumerator LoadingAnimationClose(float waitTime)
    {
      
        yield return new WaitForSeconds(waitTime);
            print("WaitAndPrint " + Time.time);
            loadingAnimated.SetActive(false);         
    }
    public void SetLoadingState(bool state,UnityAction onCancel=null)
    {
        //Debug.Log("Loading Animation");
        loadingAnimated.SetActive(state);
        if (openAfterLoading >= 0)
        {
            DebugLog("LOADING: " + state + " NEXT: " + screens[openAfterLoading].name);
        }         
        else
        {       
            DebugLog("AFTER LOAD IS: " + openAfterLoading);

        }
            
        if (loadingScreen)
        {
            loadingScreen.SetActive(state);
            loadingCancelButton.gameObject.SetActive(onCancel!=null);
            if (!state)
            {
                if (openAfterLoading > -1)
                {
                    Debug.Log("Set LOading State.................." + openAfterLoading);
                    SelectScreen(openAfterLoading);
                    openAfterLoading = -1;
                }
            }
            else
            if (onCancel != null)
            {
                loadingCancelButton.gameObject.SetActive(true);
                loadingCancelButton.onClick.RemoveAllListeners();
                loadingCancelButton.onClick.AddListener(onCancel);
            }
        }
    }

    public void OpenAfterLoading(int next)
    {
        if (IsLoading())
            openAfterLoading = next;
        else
        {
            DebugLog("IS NOT LOADING FOR: " + next);
            Debug.Log("OpenAfterLoading.................." + next);
            SelectScreen(next);
        }
    }
    public void SelectScreenByButton(int sid)
    {
     //   DebugLog("Called for:" + sid);
        SelectScreen(sid);
    }
    #region Screen Section
    public void SetTitle(string msgTitle) => title.text = msgTitle;
    public void SetAppOptionsState(bool state)
    {
        if (appOptions)
            appOptions.SetActive(state);
    }
    public void SetHeaderState(bool state)
    {
        if (headerPanel != null)
            headerPanel.SetActive(state);
        else
            DebugLog("HEADER is null");
    }
    public void SetOptionState(bool state)
    {
      //  headerOptions.SetActive(state);
        optionsPanel.SetActive(state);
    }
    public void SetExpandedOptionsState(bool state)
    {
        expandedOptionsPanel.SetActive(state);
    }
    public int GetCurrentScreenIndex() => currentScreen;
    public void HomeConfirmationDialog()
    {
        homeConfirmation.SetActive(true);
    }
    public void Back()
    {
        Mixpanel.Track("Back_Clicked");
        if (currentScreen == 31)
        {
            Mixpanel.Track("Harness Exited");
        }
        if (currentScreen == 43)
        {
            FindAnyObjectByType<DiagnosticTreeManager>().OnBackFromDT();
        }
        Debug.Log("  Last Screen " + lastScreens.Count);
        if (lastScreens.Count > 1 && currentScreen != 9)
        {
            int lastScreenId = lastScreens[lastScreens.Count - 2];
            lastScreens.RemoveRange(lastScreens.Count - 2, 2);
            Debug.Log("Back........" + lastScreenId);
            SelectScreen(lastScreenId);
            if (assetLoader.bundleLoaded)
            {
                Debug.Log("Dhiraj : BundleLoaded make false");
                assetLoader.bundleLoaded = false;

                //Dhiraj Added Start
                //                assetLoader.bundleLoader.Clear();
                //                assetLoader.bundleLoader = null;
                //End


            }
        }
        else if (currentScreen == 9)
        {
            // int lastScreenId = lastScreens[lastScreens.Count - 2];
            // lastScreens.RemoveRange(lastScreens.Count - 2, 2);
            // Debug.Log("Back........" + lastScreenId);
            SelectScreen(30);
            /*            if (assetLoader.bundleLoaded)
                        {
                            Debug.Log("Dhiraj : BundleLoaded make false");
                            assetLoader.bundleLoaded = false;

                            //Dhiraj Added Start
                            //                assetLoader.bundleLoader.Clear();
                            //                assetLoader.bundleLoader = null;
                            //End


                        }*/
        }

        APIM.currentTicket.TicketID = "";

    }
    public void _InitializeScreens()
    {
    UIScreen[] allScreens=   GetComponentsInChildren<UIScreen>();
        int GetMaxScreens()
        {
            int max = -1;
            foreach (var sc in allScreens)
            {
                if (sc.index > max)
                    max = sc.index;
            }
            return (max+1);
        }
        if (allScreens.Length > 0)
        {
            screens = new UIScreen[GetMaxScreens()];

            for (int i = 0; i < allScreens.Length; i++)
            {
                //    screenArray[allScreens[i].index] = allScreens[i].gameObject;
                if (allScreens[i] != null)
                {
                    if (screens[allScreens[i].index] == null)
                    {
                        screens[allScreens[i].index] = allScreens[i];
                    }
                    else
                        DebugLog("Cannot Overwrite screen index:" + allScreens[i].index);

                }
                else
                    DebugLog("NULL AT:" + i);
            }
        }
        if (screens.Length > 0)
            DebugLog("Finished Initialising Ui Screen Manager");
        else
            DebugLog("No UIScreen Objects Found!");
    }
    void _DisableAllScreens()
    {if(screens.Length>0)
        foreach (var item in screens)
        {
                if(item!=null)
            item.gameObject.SetActive(false);
        }
    }

    public Dialog GetDialog() => dialog;

   
    //Screen Enable based on screen number
    public void SelectScreen(int screenNumber)
    {
        Debug.Log(screenNumber);
        if(screenNumber == 30 && tutorialSlideShow.curSprite==null)
        {
            Mixpanel.Track("Tutorial_Skipped");
        }
        // Debug.Log("lastScreen ID......" +lastSelectedScreen.index);
        //if (linker != null)
        //    linker.UpdateTicket();
        if (lastSelectedScreen != null)
        {
            //Debug.Log("[SCREEN] last screen:" + lastSelectedScreen.index, lastSelectedScreen.gameObject);
            lastSelectedScreen.Close();
        }
        for (int i = 0; i < lastScreens.Count; i++)
        {
            if (screens[lastScreens[i]].gameObject.activeSelf)
            {
                DebugLog("<color=red>[FOUND]</color> Screen Open at" + screens[lastScreens[i]].name);
                screens[lastScreens[i]].Close();
            }
        }
        if (screens[screenNumber] != null)
        {

           // Debug.Log("lastScreen ID......" + lastSelectedScreen);
            lastSelectedScreen = screens[screenNumber];
            //DebugLog("<color=brown>[SELECT:" + screenNumber+"]</color>, "+screens[screenNumber].name);
            title.text = screens[screenNumber].screenName;
            currentScreen = screenNumber;
            screens[screenNumber].Open();
            SetHeaderState(screens[screenNumber].enableHeader);
            SetOptionState(screens[screenNumber].enableOptions);
            SetExpandedOptionsState(screens[screenNumber].expandOptions);
            SetAppOptionsState(screens[screenNumber].showAppOptions);
           
          
            _SetupPath(screenNumber);
        }
        else
        {
            DebugLog("Cannot find screen:" + screenNumber);
        }
            

    }
    public void SelectScreenIfNotSelected(int screenNum)
    {
        Debug.Log("SelectScreenIfNotSelected.................."+screenNum);
        if (currentScreen != screenNum)
            SelectScreen(screenNum);
    }
     void SelectRecScreen(int screenNumber)
    {
        for (int i = 0; i < screens.Length; i++)
        {
            if (screens[i] != null)
                screens[i].gameObject.SetActive(i == screenNumber);
            else
                Debug.LogError("SCREEN:" + screenNumber + " does not exist!",screens[i].gameObject);
        }
        lastSelectedScreen = screens[screenNumber];
        currentScreen = screenNumber;
        _SetupPath(screenNumber);  
    }
    void _SetupPath(int lastNode)
    {
 
        if (lastScreens == null)
            lastScreens = new List<int>();
        if (lastScreens.Count < maxRewind)
            lastScreens.Add(lastNode);
        else
        {
            for (int i = 1; i < maxRewind; i++)
            {
                lastScreens[i - 1] = lastScreens[i];
            }
            lastScreens[maxRewind-1] = lastNode;
        }
    }

    void DebugLog(string msg)
    {
        if (enableDebug)
            Debug.Log(msg);
    }
    void DebugLog(string msg,GameObject g)
    {
        if (enableDebug)
            Debug.Log(msg, g);
    }
    #endregion

}
#if UNITY_EDITOR
[CustomEditor(typeof(UIScreenManager))]
[CanEditMultipleObjects]
public class UIScreenManagerEditor : Editor
{
    int sel;
    public override void OnInspectorGUI()
    {
        UIScreenManager manager = (UIScreenManager)target;
        DrawDefaultInspector();
        EditorGUILayout.Space(10);
        EditorGUILayout.HelpBox("Update References after changes", MessageType.None);
        if (GUILayout.Button("Update References"))
        {
            EnableAllScreens(manager.transform);
            manager._InitializeScreens();
         //   manager._InitialiseButtons();
            manager._EditorGoScreenSelect(manager.startwithScreen);
        }
        EditorGUILayout.Space(10);
        EditorGUILayout.HelpBox("Screen Player", MessageType.None);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("<<"))
            manager._EditorGoPrev();
        if (GUILayout.Button(">>"))
            manager._EditorGoNext();

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.HelpBox("Input Id and Click Select to select screen in editor.", MessageType.None);
        EditorGUILayout.BeginHorizontal();
        
         sel = EditorGUILayout.IntField(sel);
        if (GUILayout.Button("Select "))
            manager._EditorGoScreenSelect(sel);
       
        EditorGUILayout.EndHorizontal();
    }
    void EnableAllScreens(Transform par)
    {
        int total = par.childCount;
        for (int i = 0; i < total; i++)
        {
            par.GetChild(i).gameObject.SetActive(true);
        }
    }
 
}
#endif
