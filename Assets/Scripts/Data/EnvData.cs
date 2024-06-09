using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct EnvData
{
    public int GUID;
    public string Name;
    public List<int> Directions;
    public List<List<int>> Env;
}
