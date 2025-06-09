using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    string name_ = "This is sample sentence";
    public string[] list;
    public string text;

    private void Start()
    {
        split(); Trim();
    }

    void split()
    {
        list = name_.Split();
        Debug.Log(list);
    }
    void Trim()
    {
        text = "   Trim   ";
        text = text.Trim();
        Debug.Log(text);
    }
    void ToWer()
    {
        text = "upper";
        Debug.Log(text.ToUpper());
        text = "LOWER";
        Debug.Log(text.ToLower());
    }
}
