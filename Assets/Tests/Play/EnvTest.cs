using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class EnvTest
{
    [UnityTest]
    public IEnumerator SampleRandomEnvData()
    {
        Assert.IsNotNull(Managers.Env.Sample(0, -1));

        yield return null;
    }

    [UnityTest]
    public IEnumerator SampleFirstEnvData()
    {
        Assert.AreEqual("Straight", Managers.Env.Sample(0, 0).Name);

        yield return null;
    }

    [UnityTest]
    public IEnumerator EnvCount()
    {
        Managers.Env.Init(3);

        // 숫자만큼 값이 존재하는지?
        Assert.AreEqual(9, Managers.Env.Map.Count);

        yield return null;
    }

    [UnityTest]
    public IEnumerator EnvGenerate()
    {
        Managers.Env.Init(5);
        Assert.AreEqual(25, Managers.Env.Map.Count);
        yield return null;
    }
}
