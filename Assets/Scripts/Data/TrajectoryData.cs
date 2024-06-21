using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public struct TrajectoryData
{
    public string GUID;
    public float BestTime;
    public List<float> BestTrajectoryX;
    public List<float> BestTrajectoryY;
}

public class TrajectoryLoader : ILoader<string, TrajectoryData>
{
    public List<TrajectoryData> Records = new List<TrajectoryData>();

    public Dictionary<string, TrajectoryData> MakeDictionary()
    {
        Dictionary<string, TrajectoryData> RecordDictionary = new Dictionary<string, TrajectoryData>();

        foreach (TrajectoryData record in Records)
        {
            RecordDictionary.Add(record.GUID, record);
        }

        return RecordDictionary;
    }
}