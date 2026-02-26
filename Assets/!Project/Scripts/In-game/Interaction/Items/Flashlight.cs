using UnityEngine;
using System.Collections;
using FishNet.Object;

public class Flashlight : BasicPickableItem
{
    [SerializeField] Light _light;
    
    [Client]
    protected override void CLIENT_HandleInteractState(bool prev, bool next, bool asServer)
    {
        base.CLIENT_HandleInteractState(prev, next, asServer);

        if (asServer) return;

        _light.enabled = next;
    }
}