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
    private Observer observer;

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

    public void Observe()
    {
        root = new GameObject { name = "Env Root" };

        map = GenerateMap(PositionsByLength(1));

        foreach (var manager in map)
        {
            manager.Init();
        }

        observer = Managers.Resource.Instantiate("Prefabs/Observer").GetComponent<Observer>();
        observer.Init(map[0].Agent, map[0].Target);
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
}