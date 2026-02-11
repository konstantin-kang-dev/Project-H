using UnityEngine;
using System.Collections;

public class GlobalInputManager
{
    public static IInput Input;

    public static void Init()
    {
        Input = new DefaultInput();
    }
}