using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct EnvData
{
    public int GUID;
    public string Name;
    public List<int> Directions;
    public List<List<int>> Env;
}

public class EnvLoader : ILoader<int, EnvData>
{
    public List<EnvData> Envs = new List<EnvData>();

    public Dictionary<int, EnvData> MakeDictionary()
    {
        Dictionary<int, EnvData> EnvDictionary = new Dictionary<int, EnvData>();

        foreach (EnvData data in Envs)
        {
            EnvDictionary.Add(data.GUID, data);
        }

        return EnvDictionary;
    }
}
