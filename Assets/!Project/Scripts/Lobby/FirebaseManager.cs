using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using Steamworks;
using System;
using System.Collections.Generic;
using UnityEngine;

public class FirebaseManager : MonoBehaviour
{
    private DatabaseReference _database;
    private bool _isInitialized = false;

    void Start()
    {
        InitializeFirebase();
    }

    void InitializeFirebase()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                _database = FirebaseDatabase.DefaultInstance.RootReference;
                _isInitialized = true;
                Debug.Log("[FirebaseManager] Firebase initialized");
            }
            else
            {
                Debug.LogError($"[FirebaseManager] Firebase init failed: {task.Result}");
            }
    
        });
    }

    public void CreateLobby(LobbyData lobby, string steamId, Action onSuccess = null)
    {
        if (!_isInitialized) return;

        var lobbyDict = new Dictionary<string, object>
        {
            { "lobbyId", lobby.LobbyId },
            { "maxPlayers", lobby.MaxPlayers },
            { "currentPlayers", lobby.CurrentPlayers },
            { "hostName", lobby.HostName },
            { "steamId", steamId },
            { "timestamp", ServerValue.Timestamp }
        };

        _database.Child("lobbies").Child(lobby.LobbyId.ToString()).SetValueAsync(lobbyDict)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    Debug.Log($"[FirebaseManager] Lobby created by: {lobby.HostName}");
                    onSuccess?.Invoke();
                }
            });
    }

    public void GetLobbies(Action<List<LobbyData>> callback)
    {
        if (!_isInitialized) return;

        _database.Child("lobbies").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("[FirebaseManager] Failed to get lobbies");
                callback?.Invoke(new List<LobbyData>());
                return;
            }

            DataSnapshot snapshot = task.Result;
            List<LobbyData> lobbies = new List<LobbyData>();

            foreach (DataSnapshot lobbySnap in snapshot.Children)
            {
                try
                {
                    LobbyData lobby = new LobbyData
                    {
                        LobbyId = int.Parse(lobbySnap.Child("lobbyId").Value.ToString()),
                        MaxPlayers = int.Parse(lobbySnap.Child("maxPlayers").Value.ToString()),
                        CurrentPlayers = int.Parse(lobbySnap.Child("currentPlayers").Value.ToString()),
                        HostName = lobbySnap.Child("hostName").Value.ToString(),
                        HostSteamId = ulong.Parse(lobbySnap.Child("steamId").Value.ToString())
                    };

                    lobbies.Add(lobby);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[FirebaseManager] Failed to parse lobby: {e.Message}");
                }
            }

            callback?.Invoke(lobbies);
        });
    }

    public void UpdatePlayerCount(int lobbyId, int count)
    {
        if (!_isInitialized) return;

        _database.Child("lobbies").Child(lobbyId.ToString()).Child("currentPlayers").SetValueAsync(count);
    }

    public void RemoveLobby(int lobbyId)
    {
        if (!_isInitialized) return;

        _database.Child("lobbies").Child(lobbyId.ToString()).RemoveValueAsync();
    }
}

[Serializable]
public struct LobbyData
{
    public int LobbyId;
    public int MaxPlayers;
    public int CurrentPlayers;
    public string HostName;
    public ulong HostSteamId;
}