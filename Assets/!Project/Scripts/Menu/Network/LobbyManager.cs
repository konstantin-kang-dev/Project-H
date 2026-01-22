using FishNet.Object;
using FishNet.Connection;
using FishNet.Managing.Scened;
using System.Collections.Generic;
using FishNet.Transporting;
using UnityEngine;
using FishNet.Managing;

public class LobbyManager : NetworkBehaviour
{
    public static LobbyManager Instance { get; private set; }
    [SerializeField] private NetworkManager _networkManager;

    private void Awake()
    {
        Instance = this;
    }

    public void StartHost()
    {
        _networkManager.ServerManager.StartConnection();

        _networkManager.ClientManager.StartConnection();
        Debug.Log("Host started");
    }

    public void StartClient(string ip = "127.0.0.1")
    {
        _networkManager.ClientManager.StartConnection(ip);
        Debug.Log($"Client connecting to {ip}");
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        SceneManager.OnClientLoadedStartScenes += OnClientLoadedStartScenes;
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        SceneManager.OnClientLoadedStartScenes -= OnClientLoadedStartScenes;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        _networkManager.ClientManager.OnClientConnectionState += OnClientConnectionStateChange;
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        _networkManager.ClientManager.OnClientConnectionState -= OnClientConnectionStateChange;
    }

    private void OnClientLoadedStartScenes(NetworkConnection conn, bool asServer)
    {
        if (!asServer)
        {
            SceneLoadData sld = new SceneLoadData("Menu");
            sld.ReplaceScenes = ReplaceOption.None;
            _networkManager.SceneManager.LoadGlobalScenes(sld);
        }

        Debug.Log($"Client {conn.ClientId} loaded start scenes");
    }

    void OnClientConnectionStateChange(ClientConnectionStateArgs args)
    {

        Debug.Log($"[LobbyManager] Updated connection state: {args.ConnectionState}");
    }
}