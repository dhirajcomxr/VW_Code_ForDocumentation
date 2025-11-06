using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StepsManager : MonoBehaviour {

    #region PUBLIC_VARS
    private static StepsManager _instance;
    public static StepsManager Instance { get { return _instance; } }

    public Text stepDesc;
    [ColorUsage(true,true)]
    public Color highlightColor;
    public GameObject fixedCamera;
    public GameObject mainCamera;
    public ExteriorCam mainCamEx;
    public GameObject freeCamPivot;
    public GameObject caution_popup;
    public ImageLabelList caution_txt;
   
    public GameObject toolPopup;
    public Text toolName_txt;
    public ImageLabelList toolList;
    //public Sprite defaultToolSprite;
    public Image toolImage;
    public ImageLabelCollection slideShow;
    public GameObject torquePopup;
    public Text torqueValueTxt;
    public GameObject oneTimeUse;
    public GameObject infoPanel;
    public GameObject nextBtn;
    public GameObject prevBtn;

    public GameObject restartBtnContainer;
    #endregion

    #region EVENTS
    public delegate void OnNextClick();
    public static event OnNextClick NextStep;

    public delegate void OnPrevClick();
    public static event OnPrevClick PreviousStep;
    #endregion

    private void Awake() {
        if (_instance != null && _instance != this) {
            Destroy(this.gameObject);
        }
        else {
            _instance = this;
        }
    }
    public void PanRight()
    {
        if (mainCamEx)
            mainCamEx.PanRight();
    }
    public Text description { get { return (stepDesc); } }

    public void OnNext() {
        NextStep?.Invoke();
    }

    public void OnPrevious() {
        PreviousStep?.Invoke();
    }
    private void OnValidate()
    {
        if (mainCamEx == null)
            mainCamEx = FindObjectOfType<ExteriorCam>();
    }
}
