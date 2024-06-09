using UnityEngine;

public class Util
{
    public static T GetOrAddComponent<T>(GameObject go) where T : Component
    {
        T component = go.GetComponent<T>();

        if(component == null)
        {
            component = go.AddComponent<T>();
        }

        return component;
    }
}