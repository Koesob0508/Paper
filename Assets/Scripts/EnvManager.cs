using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

[Serializable]
public struct LevelSetting
{
    public int Level;
    public List<Vector2> Obstacles;
    public Vector2 TargetPosition;
    public int TargetDirection;
    public Vector2 StartPosition;
    public int StartDirection;
}

[Serializable]
public struct LevelPool
{
    public int Level;
    public List<LevelSetting> LevelSettings;
}

[Serializable]
public struct EnvPool
{
    public List<LevelPool> Pool;
}

public class EnvManager : MonoBehaviour
{
    public GameObject Floor;
    public GameObject Obstacle;
    public GameObject Target;
    public GameObject Agent;

    public EnvPool Pool;

    public void OnClickSave()
    {
        string jsonData = ObjectToJson(Pool);
        CreateJsonFile(Application.dataPath, "PoolData", jsonData);
    }

    public void OnClickLoad()
    {
        Pool = LoadJsonFile<EnvPool>(Application.dataPath, "PoolData");
    }

    public void OnClickGenerate()
    {
        LevelSetting levelSetting = new LevelSetting();
        levelSetting.Level = 7;
        levelSetting.Obstacles = new List<Vector2>
        {
            new Vector2(-2, 2),
            new Vector2(-1, 2),
            new Vector2(-2, 1),
            new Vector2(-1, 1),
            new Vector2(1, 2),
            new Vector2(1, 1),
            new Vector2(2, 2),
            new Vector2(2, 1),
            new Vector2(-2, -1),
            new Vector2(2, -1),
            new Vector2(-1, -2),
            new Vector2(0, -2),
            new Vector2(1, -2)
        };
        levelSetting.TargetPosition = new Vector2(-3, 3);
        levelSetting.TargetDirection = 1;
        levelSetting.StartPosition = new Vector2(3, -3);
        levelSetting.StartDirection = 3;
        GenerateLevel(levelSetting);
    }

    private string ObjectToJson(object obj)
    {
        return JsonUtility.ToJson(obj);
    }

    private void CreateJsonFile(string createPath, string fileName, string jsonData)
    {
        FileStream fileStream = new FileStream(string.Format("{0}/{1}.json", createPath, fileName), FileMode.Create);
        byte[] data = Encoding.UTF8.GetBytes(jsonData);
        fileStream.Write(data, 0, data.Length);
        fileStream.Close();
    }

    private T LoadJsonFile<T>(string loadPath, string fileName)
    {
        FileStream fileStream = new FileStream(string.Format("{0}/{1}.json", loadPath, fileName), FileMode.Open);
        byte[] data = new byte[fileStream.Length];
        fileStream.Read(data, 0, data.Length);
        fileStream.Close(); string jsonData = Encoding.UTF8.GetString(data);
        return JsonUtility.FromJson<T>(jsonData);
    }

    private void GenerateLevel(LevelSetting _level)
    {
        // 일단 Floor를 깝니다.
        GameObject Floor = Instantiate(this.Floor, Vector3.zero, Quaternion.identity, transform);

        // Level에 맞게 x, z축으로 스케일 업
        Floor.transform.localScale = new Vector3(_level.Level, 1, _level.Level);

        // 가장 자리에 Obstacle을 순서대로 배치. 
        // North
        GameObject North = Instantiate(Obstacle, Vector3.zero, Quaternion.identity, transform);
        North.transform.localScale = new Vector3(_level.Level + 1, 1, 1);
        North.transform.position = new Vector4(0, 0.5f, _level.Level / 2 + 1);
        // East
        GameObject East = Instantiate(Obstacle, Vector3.zero, Quaternion.identity, transform);
        East.transform.localScale = new Vector3(1, 1, _level.Level + 1);
        East.transform.position = new Vector4(_level.Level / 2 + 1, 0.5f, 0);
        // South
        GameObject South = Instantiate(Obstacle, Vector3.zero, Quaternion.identity, transform);
        South.transform.localScale = new Vector3(_level.Level + 1, 1, 1);
        South.transform.position = new Vector4(0, 0.5f, -_level.Level / 2 - 1);
        // West
        GameObject West = Instantiate(Obstacle, Vector3.zero, Quaternion.identity, transform);
        West.transform.localScale = new Vector3(1, 1, _level.Level + 1);
        West.transform.position = new Vector4(-_level.Level / 2 - 1, 0.5f, 0);

        // Obstacle을 읽어들여서 해당 위치에 배치
        foreach (Vector2 position in _level.Obstacles)
        {
            GameObject Obstacle = Instantiate(this.Obstacle, new Vector3(position.x, 0.5f, position.y), Quaternion.identity, transform);
        }

        // Target Position을 읽어들여 해당 위치에 배치
        GameObject Target = Instantiate(this.Target, new Vector3(_level.TargetPosition.x, 0.5f, _level.TargetPosition.y), Quaternion.identity, transform);

        // Target Direction을 읽어들여 회전 조정
        switch (_level.TargetDirection)
        {
            case 0: // North
                break;
            case 1: // East
                Target.transform.rotation = Quaternion.Euler(new Vector3(0, 90, 0));
                break;
            case 2: // South
                Target.transform.rotation = Quaternion.Euler(new Vector3(0, 180, 0));
                break;
            case 3: // West
                Target.transform.rotation = Quaternion.Euler(new Vector3(0, -90, 0));
                break;
        }

        // Start Position을 읽어들여 해당 위치에 배치
        GameObject Start = Instantiate(this.Agent, new Vector3(_level.StartPosition.x, 0.5f, _level.StartPosition.y), Quaternion.identity, transform);

        // Start Direction을 읽어들여 회전 조정
        switch (_level.StartDirection)
        {
            case 0: // North
                break;
            case 1: // East
                Start.transform.rotation = Quaternion.Euler(new Vector3(0, 90, 0));
                break;
            case 2: // South
                Start.transform.rotation = Quaternion.Euler(new Vector3(0, 180, 0));
                break;
            case 3: // West
                Start.transform.rotation = Quaternion.Euler(new Vector3(0, -90, 0));
                break;
        }
    }
}