using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class SearchableListElement :MonoBehaviour
{
    public Button button;
    public Text label;
    public SearchableListElement(SearchableListElement copy)
    {
        this.button = copy.button;
        this.label = copy.label;
    }
}
