using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Modules.Rendering.Outline;

public interface IOutlinable
{
    List<OutlineComponent> Outlines { get; }
    void SetHighlight(bool value);
}