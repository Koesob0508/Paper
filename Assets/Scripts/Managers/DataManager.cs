using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

public interface ILoader<Key, Value>
{
    Dictionary<Key, Value> MakeDictionary();
}

public class EnvLoader : ILoader<int, EnvData>
{
    public List<EnvData> Envs = new List<EnvData>();

    public Dictionary<int, EnvData> MakeDictionary()
    {
        Dictionary<int, EnvData> EnvDictionary = new Dictionary<int, EnvData>();

        foreach(EnvData data in Envs)
        {
            EnvDictionary.Add(data.GUID, data);
        }

        return EnvDictionary;
    }
}

public class DataManager
{
    public EnvPool Pool;

    public Dictionary<int, EnvData> TrainingEnvs { get; private set; } = new Dictionary<int, EnvData>();

    public void Init()
    {
        TrainingEnvs = LoadJson<EnvLoader, int, EnvData>("EnvData").MakeDictionary();
    }

    private Loader LoadJson<Loader, Key, Value>(string path) where Loader : ILoader<Key, Value>
    {
        TextAsset textAsset = Managers.Resource.Load<TextAsset>($"Data/{path}");
        return JsonConvert.DeserializeObject<Loader>(textAsset.text);
    }

    private T LoadJsonFile<T>(string loadPath, string fileName)
    {
        FileStream fileStream = new FileStream(string.Format("{0}/{1}.json", loadPath, fileName), FileMode.Open);
        byte[] data = new byte[fileStream.Length];
        fileStream.Read(data, 0, data.Length);
        fileStream.Close(); string jsonData = Encoding.UTF8.GetString(data);
        return JsonConvert.DeserializeObject<T>(jsonData);
    }

    public void LoadData()
    {
        Pool = LoadJsonFile<EnvPool>(Application.dataPath, "PoolData");
    }
}
