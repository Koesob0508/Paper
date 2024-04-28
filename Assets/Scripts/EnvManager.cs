using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using Newtonsoft.Json;
using Random = UnityEngine.Random;

[Serializable]
public struct EnvData
{
    public string Name;
    public List<int> Directions;
    public List<List<int>> Env;
}

[Serializable]
public struct EnvPool
{
    public List<EnvData> Env;
}

public class EnvManager : MonoBehaviour
{
    #region Prefab
    public GameObject preFloor;
    public GameObject preObstacle;
    public GameObject preTarget;
    public GameObject preAgent;
    public GameObject preGuide;
    #endregion

    #region Wall
    public GameObject Floor;
    public GameObject North;
    public GameObject East;
    public GameObject South;
    public GameObject West;
    #endregion Wall

    #region Obstacle
    public List<GameObject> Obstacles;
    #endregion Obstacle

    #region Prob
    public int StartIndex;
    public int EndIndex;

    public GameObject Agent;
    public GameObject Target;
    #endregion

    public EnvPool Pool;

    public int EnvIndex = 0;

    public int[,] graph = new int[25, 25];
    public int[,] next = new int[25, 25];
    public List<GameObject> Guides = new List<GameObject>();

    private string ObjectToJson(object obj)
    {
        return JsonConvert.SerializeObject(obj);
    }

    private T LoadJsonFile<T>(string loadPath, string fileName)
    {
        FileStream fileStream = new FileStream(string.Format("{0}/{1}.json", loadPath, fileName), FileMode.Open);
        byte[] data = new byte[fileStream.Length];
        fileStream.Read(data, 0, data.Length);
        fileStream.Close(); string jsonData = Encoding.UTF8.GetString(data);
        return JsonConvert.DeserializeObject<T>(jsonData);
    }

    private void CreateJsonFile(string createPath, string fileName, string jsonData)
    {
        FileStream fileStream = new FileStream(string.Format("{0}/{1}.json", createPath, fileName), FileMode.Create);
        byte[] data = Encoding.UTF8.GetBytes(jsonData);
        fileStream.Write(data, 0, data.Length);
        fileStream.Close();
    }

    private void InitializeLevel()
    {
        if (Agent == null)
        {
            Obstacles = new List<GameObject>();

            // 일단 Floor를 깝니다.
            Floor = Instantiate(preFloor, Vector3.zero, Quaternion.identity, transform);
            Floor.transform.localScale = new Vector3(5, 1, 5);

            #region Wall
            // North
            North = Instantiate(preObstacle, Vector3.zero, Quaternion.identity, transform);
            North.transform.localScale = new Vector3(6, 1, 1);
            North.transform.position = new Vector4(0, 0.5f, 3);

            // East
            East = Instantiate(preObstacle, Vector3.zero, Quaternion.identity, transform);
            East.transform.localScale = new Vector3(1, 1, 6);
            East.transform.position = new Vector4(3, 0.5f, 0);

            // South
            South = Instantiate(preObstacle, Vector3.zero, Quaternion.identity, transform);
            South.transform.localScale = new Vector3(6, 1, 1);
            South.transform.position = new Vector4(0, 0.5f, -3);

            // West
            West = Instantiate(preObstacle, Vector3.zero, Quaternion.identity, transform);
            West.transform.localScale = new Vector3(1, 1, 6);
            West.transform.position = new Vector4(-3, 0.5f, 0);
            #endregion Wall

            #region Obstacles
            for (int idxY = 2; idxY > -3; idxY--)
            {
                for (int idxX = -2; idxX < 3; idxX++)
                {
                    GameObject _ = Instantiate(preObstacle, new Vector3(idxX, 0.5f, idxY), Quaternion.identity, transform);
                    Obstacles.Add(_);
                }
            }
            #endregion Obstacles

            #region Prob
            Agent = Instantiate(preAgent, Vector3.zero, Quaternion.identity, transform);
            Agent.SetActive(false);
            Target = Instantiate(preTarget, Vector3.zero, Quaternion.identity, transform);
            Target.SetActive(false);
            #endregion Prbo
        }
    }

    private void GenerateLevel(EnvData env)
    {
        if (Agent == null)
        {
            Debug.LogWarning("Initialize가 되지 않아 먼저 Initialize를 진행했습니다.");
            InitializeLevel();
        }

        int ObstacleIndex = 0;
        List<int> spaceList = new List<int>();

        GenerateGraph();

        foreach (List<int> row in env.Env)
        {
            foreach (int status in row)
            {
                if (status == 0)
                {
                    Obstacles[ObstacleIndex].SetActive(false);
                    spaceList.Add(ObstacleIndex);
                }
                else if (status == -1)
                {
                    Obstacles[ObstacleIndex].SetActive(true);

                    SetGraphValue(ObstacleIndex, int.MaxValue);
                }

                ObstacleIndex++;
            }
        }

        // spaceList.Count 내에서 랜덤하게 하나 뽑기
        int randomIndex = Random.Range(0, spaceList.Count);
        StartIndex = spaceList[randomIndex];
        Vector3 startPoint = Obstacles[StartIndex].transform.position;
        spaceList.RemoveAt(randomIndex);

        // 마지막 위치 뽑기
        randomIndex = Random.Range(0, spaceList.Count);
        EndIndex = spaceList[randomIndex];
        Vector3 endPoint = Obstacles[EndIndex].transform.position;

        // 방향 정하기
        randomIndex = Random.Range(0, env.Directions.Count);
        Vector3 startDirection = Vector3.zero;
        if (env.Directions.Count != 0)
        {
            switch (env.Directions[randomIndex])
            {
                case 0:
                    break;
                case 1:
                    startDirection = new Vector3(0f, 90f, 0f);
                    break;
                case 2:
                    startDirection = new Vector3(0f, 180f, 0f);
                    break;
                case 3:
                    startDirection = new Vector3(0f, 270f, 0f);
                    break;
            }
        }

        // Agent 배치
        Agent.transform.position = startPoint;
        Agent.transform.rotation = Quaternion.Euler(startDirection);
        Agent.SetActive(true);

        // Target 배치
        Target.transform.position = endPoint;
        Target.SetActive(true);
    }

    private void GenerateGraph()
    {
        // 기본적으로 모두 연결되어 있지 않음
        for (int i = 0; i < 25; i++)
        {
            for (int j = 0; j < 25; j++)
            {
                if (i == j)
                {
                    graph[i, j] = 0;
                }
                else
                {
                    graph[i, j] = int.MaxValue;
                }
            }
        }

        // 근접한 타일끼리 연결
        for (int index = 0; index < 25; index++)
        {
            SetGraphValue(index, 1);
        }
    }

    private void SetGraphValue(int index, int value)
    {
        if (index - 5 >= 0)
        {
            graph[index, index - 5] = value;
            graph[index - 5, index] = value;
        }

        if (index + 5 <= 24)
        {
            graph[index, index + 5] = value;
            graph[index + 5, index] = value;
        }

        if (index % 5 != 0)
        {
            graph[index, index - 1] = value;
            graph[index - 1, index] = value;
        }

        if (index % 5 != 4)
        {
            graph[index, index + 1] = value;
            graph[index + 1, index] = value;
        }
    }

    private void CalculatePath(int[,] graph)
    {
        int[,] dist = new int[25, 25];
        dist = (int[,])graph.Clone();
        for (int i = 0; i < 25; i++)
        {
            for (int j = 0; j < 25; j++)
            {
                //dist[i, j] = graph[i, j];

                if (graph[i, j] != int.MaxValue && i != j)
                {
                    next[i, j] = j;
                }
                else
                {
                    next[i, j] = -1; // 경로가 없는 경우 -1
                }
            }
        }

        for (int k = 0; k < 25; k++)
        {
            for (int i = 0; i < 25; i++)
            {
                for (int j = 0; j < 25; j++)
                {
                    if (dist[i, k] != int.MaxValue && dist[k, j] != int.MaxValue && dist[i, k] + dist[k, j] < dist[i, j])
                    {
                        dist[i, j] = dist[i, k] + dist[k, j];
                        next[i, j] = next[i, k];
                    }
                }
            }
        }
    }

    public void FindPath()
    {
        for (int i = 0; i < 25; i++)
        {
            string pathResult = "";
            for (int j = 0; j < 25; j++)
            {
                pathResult += $"[{i} {j} {graph[i, j]}] ";
            }
            Debug.Log(pathResult);
        }

        string result = "";
        for (int i = 0; i < 25; i++)
        {
            for (int j = 0; j < 25; j++)
            {
                result = $"정점 {i}에서 정점 {j}까지의 최단 경로: {i} ";
                int inter = next[i, j];
                while (inter != -1)
                {
                    result += $"-> {inter}";
                    inter = next[inter, j];
                }
                Debug.Log(result);
            }
        }
    }

    #region OnClick

    public void OnClickSave()
    {
        string jsonData = ObjectToJson(Pool);
        CreateJsonFile(Application.dataPath, "PoolData", jsonData);
    }

    public void OnClickLoad()
    {
        Pool = LoadJsonFile<EnvPool>(Application.dataPath, "PoolData");
    }

    public void OnClickInitialize()
    {
        InitializeLevel();
    }

    public void OnClickGenerate()
    {
        GenerateLevel(Pool.Env[EnvIndex]);
    }

    public void OnClickRemove()
    {

    }

    public void OnClickPrev()
    {
        EnvIndex--;

        if (EnvIndex < 0)
        {
            EnvIndex = Pool.Env.Count - 1;
        }
    }

    public void OnClickNext()
    {
        EnvIndex++;

        if (EnvIndex == Pool.Env.Count)
        {
            EnvIndex = 0;
        }
    }

    public void OnClickFindPath()
    {
        CalculatePath(graph);
        FindPath();
    }

    #endregion
}