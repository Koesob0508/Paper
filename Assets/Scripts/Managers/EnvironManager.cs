using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Unity.Mathematics;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.UIElements;

public class EnvironManager
{
    private List<MapManager> map;
    private GameObject root;

    public List<MapManager> Map { get { return map; } }
    public GameObject Root { get { return root; } }

    public void Init(int _length = 1)
    {
        root = new GameObject { name = "Env Root" };
        
        map = GenerateMap(PositionsByLength(_length));

        foreach(var manager in map)
        {
            manager.Init();
        }
    }

    private List<MapManager> GenerateMap(List<(float, float)> _positions)
    {
        List<MapManager> result = new List<MapManager>();

        foreach( (float x, float y) in _positions)
        {
            GameObject root = new GameObject { name = $"leaf {x}, {y}" };
            root.transform.position = new Vector3(x * 7, 0, y * 7);
            root.transform.SetParent(Root.transform);

            MapManager map = new MapManager(root);
            
            result.Add(map);
        }

        return result;
    }

    private List<(float, float)> PositionsByLength(int count)
    {
        List<(float, float)> result = new List<(float, float)>();

        float first = (count % 2 == 0) ? -((count / 2) - 0.5f) : -(count / 2);

        for (int y = 0; y < count; y++)
        {
            for (int x = 0; x < count; x++)
            {
                result.Add(((x + first), (y + first)));
            }
        }

        return result;
    }

    // Pool Index
    // 0 : Training
    // 1 : Test
    public EnvData Sample(int poolIndex = 0, int envIndex = 0)
    {
        EnvData result = new EnvData();
        switch(poolIndex)
        {
            case 0:
                Managers.Data.TrainingEnvs.TryGetValue(envIndex, out result);
                break;
            case 1:
                break;
            default:
                break;
        }
        return result;
    }

    public void UpdateRecord(string _guid, float _time, List<Vector2> _trajectory)
    {
        TrajectoryData data = new TrajectoryData();
        data.GUID = _guid;
        data.BestTime = _time;
        data.BestTrajectoryX = new List<float>();
        data.BestTrajectoryY = new List<float>();

        foreach(var trajectory in _trajectory)
        {
            data.BestTrajectoryX.Add(trajectory.x);
            data.BestTrajectoryY.Add(trajectory.y);
        }

        if(_trajectory.Count < 5)
        {
            for(int i = 0; i < 5 - _trajectory.Count; i++)
            {
                data.BestTrajectoryX.Add(0f);
                data.BestTrajectoryY.Add(0f);
            }
        }

        if(Managers.Data.Trajectory.TryGetValue(_guid, out var _t))
        {
            if(_t.BestTime > _time)
            {
                Managers.Data.Trajectory[_guid] = data;
                TrajectoryLoader loader = new TrajectoryLoader();
                loader.Records = Managers.Data.Trajectory.Values.ToList();
                Managers.Data.UpdateTrajectory(loader);
                Managers.Data.SaveJsonTo(loader, "TrajectoryData");
            }
        }
        else
        {
            Managers.Data.Trajectory.Add(_guid, data);
            TrajectoryLoader loader = new TrajectoryLoader();
            loader.Records = Managers.Data.Trajectory.Values.ToList();
            Managers.Data.UpdateTrajectory(loader);
            Managers.Data.SaveJsonTo(loader, "TrajectoryData");
        }
    }
}