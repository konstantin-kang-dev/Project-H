using UnityEngine;
using System.Collections;

public interface IProgressBarVisuals
{
    void Init();
    void UpdateProgress(float progress, bool doInstantly);
}