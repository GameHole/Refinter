using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Refinter;
namespace Tests
{
    interface ITestInterface:IInterface
    {

    }
    class TestInterfaceImpl : ITestInterface { }
    class TestUseMono : MonoBehaviour
    {
        public ITestInterface test;
    }
    interface ITestA { }
    class TestInterfaceClass : IInterface , ITestA { }

    public class TestReflection
    {
        GameObject go;
        TestUseMono testMono;
        [UnitySetUp]
        public IEnumerator TestReflectionSetUp()
        {
            go = new GameObject("test");
            testMono = go.AddComponent<TestUseMono>();
            go.AddComponent<Reflection>();
            yield return null;
        }
        [UnityTest]
        public IEnumerator TestLoadAndInjectInterface()
        {
            var testInter = Reflection.Get<ITestInterface>();
            Assert.AreNotEqual(null, testInter);
            Assert.AreNotEqual(null, testMono.test);
            Assert.AreEqual(testInter, testMono.test);
            foreach (var item in typeof(TestInterfaceClass).GetInterfaces())
            {
                Debug.Log(item);
            }
            yield return null;
        }
        [UnityTearDown]
        public IEnumerator TestReflectionTearDown()
        {
            //Reflection.Dispose();
            Object.Destroy(go);
            yield return null;
        }
    }
}
