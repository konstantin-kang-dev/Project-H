using Newtonsoft.Json;
using System;
using System.IO;
using UnityEngine;

namespace Saves
{
    [Serializable]
    public class GameSave
    {
        public PlayerSave PlayerSave;
        public SettingsSave SettingsSave;

        public GameSave()
        {
            PlayerSave = new PlayerSave();
            SettingsSave = new SettingsSave();
        }
    }

    public class SaveManager
    {
        private static readonly string SavePath = Path.Combine(Application.persistentDataPath, "save.json");

        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore
        };

        public static Action<GameSave> OnSaveUpdated;
        public static Action<GameSave> OnSaveLoaded;
        public static GameSave GameSave { get; private set; } = new GameSave();

        public static void LoadAll()
        {
            GameSave = Load<GameSave>();
            if (GameSave == null)
            {
                GameSave = new GameSave();
            }
            else
            {
                if (GameSave.PlayerSave == null) GameSave.PlayerSave = new PlayerSave();
                if (GameSave.SettingsSave == null) GameSave.SettingsSave = new SettingsSave();
            }

            OnSaveLoaded?.Invoke(GameSave);
        }

        public static void SaveAll()
        {
            Save(GameSave);
            OnSaveUpdated?.Invoke(GameSave);
        }

        public static void Save<T>(T data)
        {
            try
            {
                string json = JsonConvert.SerializeObject(data, Settings);
                File.WriteAllText(SavePath, json);
            }
            catch (Exception e)
            {
                Debug.LogError($"Save failed: {e.Message}");
            }
        }

        public static T Load<T>() where T : new()
        {
            try
            {
                if (File.Exists(SavePath))
                {
                    string json = File.ReadAllText(SavePath);
                    return JsonConvert.DeserializeObject<T>(json, Settings);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Load failed: {e.Message}");
            }

            return new T();
        }

        public static bool HasSave() => File.Exists(SavePath);

        public static void DeleteSave()
        {
            if (File.Exists(SavePath))
                File.Delete(SavePath);
        }
    }
}