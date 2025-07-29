using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SearchBarSetup : MonoBehaviour
{
    public TMP_InputField searchInputField;

    void Start()
    {
        if (searchInputField != null)
        {
            searchInputField.lineType = TMP_InputField.LineType.SingleLine;
            searchInputField.contentType = TMP_InputField.ContentType.Standard;
            searchInputField.characterLimit = 100; // Optional
        }
    }
}
