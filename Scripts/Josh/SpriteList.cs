using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
[CreateAssetMenu(fileName = "SpriteList", menuName = "Objects/SpriteList", order = 1)]
public class SpriteList : ScriptableObject
{
   public bool remoteLoad = false;
    public Sprite[] sprites;
    public Dictionary<string, Sprite> spriteSet;
    public Sprite GetSprite(string name)
    {
      return  spriteSet[name];
    }
}
//#if UNITY_EDITOR
//[CustomEditor(typeof(SpriteList))]
//[CanEditMultipleObjects]
//public class SpriteListEditor : Editor
//{
//    string loadFolder;
//    bool toolSpriteOptions = false;
//    public void LoadFromDir(SpriteList sl,string dir)
//    {
//        sl.sprites = LoadIconFromAssetPath(dir);
//        if (sl.sprites.Length > 0)
//        {
//            sl.spriteSet = new Dictionary<string, Sprite>();
//            for (int i = 0; i < sl.sprites.Length; i++)
//            {
//                Sprite curSprite = sl.sprites[i];
//                if (curSprite != null)
//                    sl.spriteSet.Add(curSprite.name, curSprite);
//            }
//        }
//        Sprite[] LoadIcons(string resPath)
//        {
//            Sprite[] Icons; // icons array
//            object[] loadedIcons = Resources.LoadAll(resPath, typeof(Sprite));
//            Icons = new Sprite[loadedIcons.Length];
//            //this
//            for (int x = 0; x < loadedIcons.Length; x++)
//            {
//                Icons[x] = (Sprite)loadedIcons[x];
//            }
//            //or this
//            //loadedIcons.CopyTo (Icons,0);
//            return Icons;
//        }
//        Sprite[] LoadIconFromAssetPath(string path)
//        {
//            Sprite[] icons;
//            var objects = AssetDatabase.LoadAllAssetsAtPath(path);
//            Debug.Log("OB:" + objects.Length);
//           var sprites = objects.Where(q => q is Sprite).Cast<Sprite>();
       
//            icons = sprites.ToArray();
//            return icons;
//        }

     
//    }
//    public override void OnInspectorGUI()
//    {
//        DrawDefaultInspector();
//        SpriteList spriteList = (SpriteList)target;
//        if (spriteList.sprites.Length < 1)
//        {
//            loadFolder = EditorGUILayout.TextField("Load From Folder:", loadFolder, GUILayout.MinWidth(50));
//            if (GUILayout.Button("Populate Images"))
//                LoadFromDir(spriteList, loadFolder);
//        }
//    }
//}
//#endif
