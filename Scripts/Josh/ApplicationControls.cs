using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class ApplicationControls : MonoBehaviour
{
    [SerializeField] bool scanButtons = true;
    [SerializeField] KeyCode reloadButton;

    // Start is called before the first frame update
    void Start()
    {
        
    }
    private void OnLevelWasLoaded(int level)
    {

    }
   public void LoadScene(int level)
    {
        SceneManager.LoadSceneAsync(level, LoadSceneMode.Additive);
    }
    public void LoadSceneDirect(int level)
    {
        SceneManager.LoadScene(level, LoadSceneMode.Single);
    }
    public void UnloadScene(int level)
    {
        SceneManager.UnloadSceneAsync(level);
    }
    public void UnloadLastLoadedScene()
    {
        SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().buildIndex);
    }
    // Update is called once per frame
    void Update()
    {
        if (scanButtons)
            ScanButtons();
    }
    void ScanButtons()
    {
        if (Input.GetKeyDown(reloadButton))
        {
            this.enabled = false;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
        }
    }

}