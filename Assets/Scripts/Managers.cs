using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Managers : MonoBehaviour
{
    static Managers s_instance;
    public static Managers Instance { get { Init(); return s_instance; } }

    public EnvManager _env;

    public static EnvManager Env { get { return Instance._env; } }
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

            // s_instance._env.Init();
            Env.LoadData();
            Env.InitializeLevel();
            Env.StartEpisode();
        }
    }

    public static void Clear()
    {

    }
}
