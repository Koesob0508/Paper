using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Managers : MonoBehaviour
{
    static Managers s_instance;
    public static Managers Instance { get { Init(); return s_instance; } }

    private DataManager _data = new DataManager();
    private ResourceManager _resource = new ResourceManager();
    private SceneManagerEx _scene = new SceneManagerEx();
    private PoolManager _pool = new PoolManager();

    public static DataManager Data { get { return Instance._data; } }
    public static ResourceManager Resource { get { return Instance._resource; } }
    public static SceneManagerEx Scene { get { return Instance._scene; } }
    public static PoolManager Pool { get { return Instance._pool; } }

    public List<EnvManager> Envs = new List<EnvManager>();

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

            //Data.LoadData();
            s_instance._data.Init();

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
