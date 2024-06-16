using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class ManagerTest
{
    [UnityTest]
    public IEnumerator DataManager()
    {
        Assert.IsNotNull(Managers.Data);

        yield return null;
    }

    [UnityTest]
    public IEnumerator EnvManager()
    {
        Assert.IsNotNull(Managers.Env);
        yield return null;
    }

    [UnityTest]
    public IEnumerator PathManager()
    {
        Assert.IsNotNull(Managers.Path);
        yield return null;
    }
}
