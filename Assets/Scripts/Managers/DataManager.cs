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

public class DataManager
{
    public List<EnvData> Pool;

    public Dictionary<int, EnvData> TrainingEnvs { get; private set; }
    public Dictionary<int, EnvData> TestEnvs { get; private set; }
    public Dictionary<int, EnvData> SecondEnvs { get; private set; }
    public Dictionary<string, TrajectoryData> Trajectory { get; private set; }
    public Dictionary<string, TrajectoryData> LastTrajectory { get; private set; }
    public void Init()
    {
        TrainingEnvs = LoadJson<EnvLoader, int, EnvData>("EnvData").MakeDictionary();
        TestEnvs = LoadJson<EnvLoader, int, EnvData>("TestData").MakeDictionary();
        SecondEnvs = LoadJson<EnvLoader, int, EnvData>("SecondEnv").MakeDictionary();
        Trajectory = LoadJsonFrom<TrajectoryLoader, string, TrajectoryData>("TrajectoryData").MakeDictionary();
    }

    public void UpdateTrajectory(TrajectoryLoader _loader)
    {
        Trajectory = _loader.MakeDictionary();
    }

    private Loader LoadJson<Loader, Key, Value>(string path) where Loader : ILoader<Key, Value>
    {
        TextAsset textAsset = Managers.Resource.Load<TextAsset>($"Data/{path}");
        return JsonConvert.DeserializeObject<Loader>(textAsset.text);
    }

    public Loader LoadJsonFrom<Loader, Key, Value>(string path) where Loader : ILoader<Key, Value>
    {
        return Managers.Resource.LoadJsonFromFile<Loader>($"Data/{path}");
    }

    public void SaveJsonTo<T>(T data, string fileName)
    {
        Managers.Resource.SaveJsonToFile(data, $"Data/{fileName}");
    }
}
