using FishNet.Managing;
using FishNet.Managing.Transporting;
using FishNet.Transporting;
using FishNet.Transporting.Multipass;
using FishNet.Transporting.Tugboat;
using System.Linq;
using UnityEngine;

public class TransportSwitcher : MonoBehaviour
{
    [SerializeField] private TransportManager _transportManager;
    [SerializeField] private Multipass _multipass;
    [SerializeField] public Transport EditorTransport;
    [SerializeField] public Transport SteamTransport;

    [field: SerializeField] public bool IsLocalMode { get; private set; } = false;

    private void Awake()
    {
        if (IsLocalMode)
        {
            SwitchTransportToEditor();
        }
        else
        {
            SwitchTransportToSteam();
        }
    }

    public void SwitchTransportToSteam()
    {
        //_transportManager.Transport = SteamTransport;
        _multipass.SetClientTransport<FishySteamworks.FishySteamworks>();
        Debug.Log($"[TransportSwitcher] Switched transport to Steam");
    }

    public void SwitchTransportToEditor()
    {
        //_transportManager.Transport = EditorTransport;
        _multipass.SetClientTransport<Tugboat>();
        Debug.Log($"[TransportSwitcher] Switched transport to Editor");
    }
}