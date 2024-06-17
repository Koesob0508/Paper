using Codice.Client.BaseCommands.WkStatus.Printers;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Managers : MonoBehaviour
{
    public int Count;
    public bool IsObserveMode = false;
    static Managers s_instance;
    public static Managers Instance { get { Init(); return s_instance; } }

    private ResourceManager _resource = new ResourceManager();
    private SceneManagerEx _scene = new SceneManagerEx();
    private PoolManager _pool = new PoolManager();

    private DataManager _data = new DataManager();
    private EnvironManager _env = new EnvironManager();
    private PathManager _path = new PathManager();

    public static ResourceManager Resource { get { return Instance._resource; } }
    public static SceneManagerEx Scene { get { return Instance._scene; } }
    public static PoolManager Pool { get { return Instance._pool; } }
    public static DataManager Data { get { return Instance._data; } }
    public static EnvironManager Env { get { return Instance._env; } }
    public static PathManager Path { get { return Instance._path; } }

    private void Start()
    {
        Init();
    }

    private static void Init()
    {
        if(s_instance == null)
        {
            GameObject go = GameObject.Find("@Managers");

            if(go == null)
            {
                go = new GameObject { name = "@Managers" };
                go.AddComponent<Managers>();
            }

            DontDestroyOnLoad(go);
            s_instance = go.GetComponent<Managers>();

            s_instance._data.Init();
            if(s_instance.IsObserveMode)
            {
                s_instance._env.Observe();
            }
            else
            {
                s_instance._env.Init(Instance.Count);
            }

            //foreach(EnvManager Env in Instance.Envs)
            //{
            //    Env.InitializeLevel();
            //    Env.StartEpisode();
            //}
        }
    }

    public static void Clear()
    {

    }
}
