using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

using System;
using System.Collections;
using System.Collections.Generic;

[HelpURL("https://geom.io/bakery/wiki/index.php?title=Manual#Bakery_Light_Filter")]
[DisallowMultipleComponent]
public class BakeryLightFilter : MonoBehaviour
{
    public Texture2D texture;
    
    [HideInInspector]
    public int lmid = 0;
}



