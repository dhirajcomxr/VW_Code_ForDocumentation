using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
[System.Serializable]
public class ImageSlideshow :ImageLabelCollection
{
    public bool auto = false,parentOffIfNull=false,disableSelfIfNull;
    public float autoDelay = 3f;

  
    public Image display;
    [Tooltip("Assign is label has to be changed with image.")]
    public Text labelText;
    public Sprite[] images;
    [SerializeField] List<GameObject> loadedList;
    float nextTime = 1;
    int cur = -1;
   
    // Start is called before the first frame update
    void Reset()
    {
        Init();
    }
  
    void CheckAssignments()
    {
        if (display == null)
            display = GetComponent<Image>();
     
    }
    void Init()
    {
        display = GetComponent<Image>();
        Next();
    }
    
    public override void Load(Sprite[] sprites)
    {
        Debug.Log("sp:" + sprites.Length, gameObject);
        CheckAssignments();
        images = sprites;
        //     if (images.Length <= 0)
        if (disableSelfIfNull)
            gameObject.SetActive(sprites.Length>0);
        if (parentOffIfNull)       
            transform.parent.gameObject.SetActive(images.Length > 0);
        else
            transform.parent.gameObject.SetActive(true);
       if(!auto)
            if(sprites.Length>0)
            {
                display.sprite = sprites[0];

                if (labelText != null)
                    labelText.text = sprites[0].name;
                for (int i = 1; i < sprites.Length; i++)
                {
                    GameObject go = new GameObject("IMG:" + i);
                    Image img =go.AddComponent<Image>();
                 //   img.transform.SetPositionAndRotation(display);


                }
            }
    }
   public void Next()
    {
        if (!display)
            Init();
        cur++;
        nextTime = autoDelay;
        if (cur >= images.Length)
            cur = 0;
        if(cur<images.Length)
        if(images[cur]!=null)
        display.sprite = images[cur];
        transform.parent.gameObject.SetActive(images.Length > 0);
    }
    // Update is called once per frame
    void Update()
    {
        if (auto)
        {
            nextTime -= Time.deltaTime;
            if (nextTime <= 0)
                Next();
        }
    }

  
}
