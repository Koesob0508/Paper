using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class DataTest
{

    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
    [UnityTest]
    public IEnumerator ReadTest()
    {
        Managers.Data.Init();
        Managers.Data.TrainingEnvs.TryGetValue(1, out EnvData value);
        Assert.AreEqual("Straight", value.Name);

        yield return null;
    }
}
