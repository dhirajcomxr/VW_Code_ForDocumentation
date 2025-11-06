using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

    [DisallowMultipleComponent]
    public class UIScreen : MonoBehaviour
    {
        public string screenName;
        public int index;
        [Tooltip("use these options to activate or Deactivate header or options when this screen is active.")]

        public bool enableHeader = true, enableOptions = true, expandOptions = false, showAppOptions = false;
        public UnityEvent onOpen, onClose;
        //public Scrollbar ticketScrollbar;

        //private void Awake()
        //{
        //    ticketScrollbar.value = 1f;
        //    ticketScrollbar.size = 0.6f;
        //}
        private void OnEnable()
        {
         
        }
        private void Reset()
        {
            index = transform.GetSiblingIndex();
            screenName = gameObject.name;
        }
        public void SetScreenName(string name) => screenName = name;

        //Invoke unity action "open"
        public void Open()
        {
            onOpen.Invoke();
            gameObject.SetActive(true);
        }

    //Invoke unity action "close"
    public void Close()
        {
            onClose.Invoke();
            gameObject.SetActive(false);
        }
    }
#if UNITY_EDITOR
[CustomEditor(typeof(UIScreen))]
[CanEditMultipleObjects]
public class UIScreenEditor : Editor
{
    UIScreenManager manager;
    UIScreen curScreen;
    public override void OnInspectorGUI()
    {
        if(!curScreen)
         curScreen = (UIScreen)target;
        if (!manager)
            manager = FindObjectOfType<UIScreenManager>();
        if (GUILayout.Button("Select",GUILayout.Height(30)))
            manager._EditorGoScreenSelect(curScreen.index);
        EditorGUILayout.HelpBox("Click to select and show this screen", MessageType.None);
        DrawDefaultInspector();
    }
}
#endif