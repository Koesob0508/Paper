using System.IO;
using UnityEngine;
using Newtonsoft.Json;

public class ResourceManager
{
    public T Load<T>(string path) where T : Object
    {
        if (typeof(T) == typeof(GameObject))
        {
            string name = path;
            int index = name.LastIndexOf('/');
            if (index >= 0)
            {
                name = name.Substring(index + 1);
            }

            GameObject go = Managers.Pool.GetOriginal(name);
            if (go != null) { return go as T; }
        }

        return Resources.Load<T>(path);
    }

    public void SaveJsonToFile<T>(T data, string fileName)
    {
        // persistentDataPath 경로 설정
        string path = Application.persistentDataPath + "/" + fileName + ".json";

        // JSON 데이터 직렬화
        string jsonData = JsonConvert.SerializeObject(data, Formatting.Indented);

        // 파일에 저장
        File.WriteAllText(path, jsonData);
    }

    public T LoadJsonFromFile<T>(string fileName)
    {
        // persistentDataPath 내 파일 경로 설정
        string path = Application.persistentDataPath + "/" + fileName + ".json";

        if (File.Exists(path))
        {
            
            // 파일에서 JSON 데이터 읽기
            string jsonData = File.ReadAllText(path);

            // JSON 데이터 역직렬화
            return JsonUtility.FromJson<T>(jsonData);
            //return JsonConvert.DeserializeObject<T>(jsonData);
        }

        Debug.LogWarning($"File not found: {path}");
        return default;
    }

    public GameObject Instantiate(string path, Transform parent = null)
    {
        GameObject original = Load<GameObject>($"{path}");

        if (original == null)
        {
            Debug.LogWarning($"Failed to load prefab : {path}");
            return null;
        }

        if (original.GetComponent<Poolable>() != null) { return Managers.Pool.Pop(original, parent).gameObject; }

        GameObject go = Object.Instantiate(original, parent);

        int index = go.name.IndexOf("(Clone)");

        if (index > 0)
        {
            go.name = go.name.Substring(0, index);
        }

        return go;
    }

    public T Instantiate<T>(string path, Transform parent = null)
    {
        GameObject original = Load<GameObject>($"{path}");

        if (original == null)
        {
            Debug.LogWarning($"Failed to load prefab : {path}");
            return default;
        }

        if (original.GetComponent<Poolable>() != null) { return Managers.Pool.Pop(original, parent).gameObject.GetComponent<T>(); }
        GameObject go = Object.Instantiate(original, parent);

        int index = go.name.IndexOf("(Clone)");
        if (index > 0)
        {
            go.name = go.name.Substring(0, index);
        }

        return go.GetComponent<T>();
    }
    public void Destroy(GameObject go)
    {
        if (go == null) return;

        Poolable poolable = go.GetComponent<Poolable>();

        if (poolable != null)
        {
            Managers.Pool.Push(poolable);
            return;
        }

        Object.Destroy(go);
    }
}