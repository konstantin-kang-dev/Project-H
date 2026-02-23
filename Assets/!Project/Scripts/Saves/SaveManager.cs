using System;
using System.IO;
using UnityEngine;

namespace Saves
{
    public class GameSave
    {
        public PlayerSave PlayerSave;
        public SettingsSave SettingsSave;
    }

    public class SaveManager
    {
        private static readonly string SavePath = Path.Combine(Application.persistentDataPath, "save.json");

        public static GameSave GameSave { get; private set; } = new GameSave();
        public static void LoadAll()
        {
            GameSave = Load<GameSave>();
        }

        public static void SaveAll()
        {
            Save(GameSave);
        }

        public static void Save<T>(T data)
        {
            try
            {
                string json = JsonUtility.ToJson(data, true);
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
                    return JsonUtility.FromJson<T>(json);
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


