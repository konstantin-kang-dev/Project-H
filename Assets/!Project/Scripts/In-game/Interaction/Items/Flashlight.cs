using UnityEngine;
using System.Collections;

public class Flashlight : BasicPickableItem
{
    [SerializeField] Light _light;
    protected override void CLIENT_HandleInteractState(bool prev, bool next, bool asServer)
    {
        base.CLIENT_HandleInteractState(prev, next, asServer);

        if (asServer) return;

        _light.enabled = next;
    }
}