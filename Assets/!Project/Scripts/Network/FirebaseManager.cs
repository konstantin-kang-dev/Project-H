using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using Steamworks;
using System;
using System.Collections.Generic;
using UnityEngine;

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager Instance;

    private DatabaseReference _database;
    private bool _isInitialized = false;

    private void Awake()
    {
        Instance = this;
    }

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
#if UNITY_EDITOR
                bool isClone = UnityEditor.EditorApplication.applicationPath.Contains("_clone_");
                if (isClone)
                {
                    FirebaseDatabase.DefaultInstance.SetPersistenceEnabled(false);
                }
#endif

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

        var lobbyRef = _database.Child("lobbies").Child(lobby.LobbyId.ToString());

        var lobbyDict = new Dictionary<string, object>
    {
        { "lobbyId", lobby.LobbyId },
        { "maxPlayers", lobby.MaxPlayers },
        { "currentPlayers", lobby.CurrentPlayers },
        { "chosenDifficulty", (int)lobby.ChosenDifficulty },
        { "hostName", lobby.HostName },
        { "steamId", steamId },
        { "timestamp", ServerValue.Timestamp }
    };

        lobbyRef.OnDisconnect().RemoveValue().ContinueWithOnMainThread(_ =>
        {
            lobbyRef.SetValueAsync(lobbyDict).ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    Debug.Log($"[FirebaseManager] Lobby created by: {lobby.HostName}");
                    onSuccess?.Invoke();
                }
            });
        });
    }

    public void LoadLobbies(Action<List<LobbyData>> callback)
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

            if(snapshot.ChildrenCount > 0)
            {
                foreach (DataSnapshot lobbySnap in snapshot.Children)
                {
                    try
                    {
                        LobbyData lobby = new LobbyData
                        {
                            LobbyId = int.Parse(lobbySnap.Child("lobbyId").Value.ToString()),
                            MaxPlayers = int.Parse(lobbySnap.Child("maxPlayers").Value.ToString()),
                            CurrentPlayers = int.Parse(lobbySnap.Child("currentPlayers").Value.ToString()),
                            ChosenDifficulty = (DifficultyType)int.Parse(lobbySnap.Child("chosenDifficulty").Value.ToString()),
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
            }

            Debug.Log($"[FirebaseManager] Loaded lobbies: {lobbies.Count}");

            callback?.Invoke(lobbies);
        });
    }

    public void UpdateLobbyData(LobbyData lobbyData)
    {
        if (!_isInitialized) return;

        var lobbyDict = new Dictionary<string, object>
        {
            { "lobbyId", lobbyData.LobbyId },
            { "maxPlayers", lobbyData.MaxPlayers },
            { "currentPlayers", lobbyData.CurrentPlayers },
            { "chosenDifficulty", (int)lobbyData.ChosenDifficulty},
            { "hostName", lobbyData.HostName },
            { "steamId", lobbyData.HostSteamId.ToString() },
            { "timestamp", ServerValue.Timestamp }
        };
        _database.Child("lobbies").Child(lobbyData.LobbyId.ToString()).UpdateChildrenAsync(lobbyDict);
    }

    public void RemoveLobby(int lobbyId)
    {
        if (!_isInitialized) return;

        var lobbyRef = _database.Child("lobbies").Child(lobbyId.ToString());
        lobbyRef.OnDisconnect().Cancel();
        lobbyRef.RemoveValueAsync();
    }
}

[Serializable]
public struct LobbyData
{
    public int LobbyId;
    public int MaxPlayers;
    public int CurrentPlayers;
    public DifficultyType ChosenDifficulty;
    public string HostName;
    public ulong HostSteamId;

}