using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DemoUIManager : MonoBehaviour {

    public GameObject homeScreen;

    public GameObject rnrModel;
    public GameObject stepsUI;

    public GameObject explodedViewModel;
    public GameObject explodedUI;

    public GameObject dtUI;

    // Start is called before the first frame update
    void Start() {

    }

    private void OnEnable() {
        //homeScreen.SetActive(true);
    }

    // Update is called once per frame
    void Update() {

    }

    public void OnRnRSelect() {
        homeScreen.SetActive(false);
        rnrModel.SetActive(true);
        stepsUI.SetActive(true);
    }

    public void OnExplodedViewSelect() {
        homeScreen.SetActive(false);
        rnrModel.SetActive(false);
        explodedViewModel.SetActive(true);
        explodedUI.SetActive(true);
    }

    public void OnDTSelect() {
        rnrModel.SetActive(false);
        homeScreen.SetActive(false);
        dtUI.SetActive(true);
        SceneManager.LoadScene(1, LoadSceneMode.Additive);
    }

    public void OnHomeClick() {
        if (SceneManager.sceneCount > 1) {
            Debug.Log(SceneManager.sceneCount);
            SceneManager.UnloadSceneAsync(1);
        }
        SceneManager.LoadScene(0);
    }
    public void ResetClick()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void AppQuit() {
        Application.Quit();
    }
}
