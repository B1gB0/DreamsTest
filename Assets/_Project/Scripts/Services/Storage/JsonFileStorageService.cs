using System.IO;
using UnityEngine;

namespace Game.Services.Storage
{
    public class JsonFileStorageService : IStorageService
    {
        public void Save<T>(string key, T data) where T : class
        {
            string path = GetPath(key);
            string json = JsonUtility.ToJson(data);
            File.WriteAllText(path, json);
        }

        public T Load<T>(string key) where T : class
        {
            string path = GetPath(key);
            if (!File.Exists(path))
            {
                return null;
            }
            
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<T>(json);
        }

        public bool Has(string key)
        {
            return File.Exists(GetPath(key));
        }

        public void Delete(string key)
        {
            string path = GetPath(key);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        private static string GetPath(string key)
        {
            return Path.Combine(Application.persistentDataPath, $"{key}.json");
        }
    }
}
