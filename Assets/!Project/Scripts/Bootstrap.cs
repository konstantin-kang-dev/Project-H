using Cysharp.Threading.Tasks;
using FishNet.Managing;
using Steamworks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Bootstrap : MonoBehaviour
{
    [SerializeField] NetworkManager _networkManager;

    float _timeout = 1f;

    private void Start()
    {
        Init();
    }

    async void Init()
    {
        LoadingManager.Instance.ShowLoading(LoadingWindowType.Screen, "Initializing Steam...");

        while (!SteamAPI.IsSteamRunning())
        {
            Debug.LogError($"[Bootstrap] Steam is not running...");

            await UniTask.WaitForSeconds(_timeout);
        }

        if (!SteamAPI.Init())
        {
            Debug.LogError("[NetworkLobbyManager] SteamAPI.Init() failed");
            return;
        }
        Debug.Log($"[NetworkLobbyManager] Steam ID: {SteamUser.GetSteamID()}");

        Debug.Log("[NetworkLobbyManager] SteamAPI initialized");
        Debug.Log($"[NetworkLobbyManager] Steam Name: {SteamFriends.GetPersonaName()}");
        Debug.Log($"[NetworkLobbyManager] Steam ID: {SteamUser.GetSteamID()}");

        await UniTask.WaitForSeconds(1f);

        if (_networkManager == null) throw new System.Exception($"[Bootstrap] Network manager is null.");

        SaveManager.LoadAll();
        SaveManager.GameData.SteamId = SteamUser.GetSteamID().ToString();
        SaveManager.GameData.PlayerName = SteamFriends.GetPersonaName();
        SaveManager.SaveAll();

        GlobalInputManager.Init();

        SceneManager.LoadScene("Menu");
        await UniTask.WaitForSeconds(1f);
        LoadingManager.Instance.SetLoadingProgress(1f);
    }
}
