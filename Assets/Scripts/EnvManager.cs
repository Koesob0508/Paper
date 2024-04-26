using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using Newtonsoft.Json;

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
    #endregion

    #region Wall
    public GameObject Floor;
    public GameObject North;
    public GameObject East;
    public GameObject South;
    public GameObject West;
    #endregion Wall

    #region Obstacle
    List<GameObject> Obstacles;
    #endregion Obstacle

    public EnvPool Pool;

    public int EnvIndex = 0;

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
        if (Obstacles == null)
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
        }
    }

    private void GenerateLevel(EnvData env)
    {
        int ObstacleIndex = 0;

        foreach (List<int> row in env.Env)
        {
            foreach (int status in row)
            {
                if (status == 0)
                {
                    Obstacles[ObstacleIndex].SetActive(false);
                }
                else if (status == -1)
                {
                    Obstacles[ObstacleIndex].SetActive(true);
                }

                ObstacleIndex++;
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

    #endregion
}