using Cysharp.Threading.Tasks;
using FishNet.Managing;
using GameAudio;
using Saves;
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
        await UniTask.WaitUntil(() => SteamManager.Initialized);

        LoadingManager.Instance.ShowLoading(LoadingWindowType.Screen, "Initializing Steam...");

        Debug.Log($"[NetworkLobbyManager] Steam ID: {SteamUser.GetSteamID()}");
        Debug.Log($"[NetworkLobbyManager] Steam Name: {SteamFriends.GetPersonaName()}");
        Debug.Log($"[NetworkLobbyManager] Steam ID: {SteamUser.GetSteamID()}");

        await UniTask.WaitForSeconds(1f);

        if (_networkManager == null) throw new System.Exception($"[Bootstrap] Network manager is null.");

        GlobalInputManager.Init();

        SaveManager.LoadAll();
        SaveManager.GameSave.PlayerSave.SteamId = SteamUser.GetSteamID().ToString();
        SaveManager.GameSave.PlayerSave.PlayerName = SteamFriends.GetPersonaName();
        SaveManager.SaveAll();

        GraphicsManager.ApplySave(SaveManager.GameSave.SettingsSave.GraphicsSave);
        GlobalInputManager.ApplySave(SaveManager.GameSave.SettingsSave.ControlsSave);
        GlobalAudioManager.Instance.ApplySave(SaveManager.GameSave.SettingsSave.AudioSave);

        SceneManager.LoadScene("Menu");
    }
}
