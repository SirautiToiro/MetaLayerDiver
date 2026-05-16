using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITestClass
{
    public void TestMethod();
}

[System.Serializable]
public class TestClass1 : ITestClass
{
    [SerializeField] private int testInput;

    public TestClass1()
    {

    }

    public void TestMethod()
    {
        Debug.Log("TestClass1: " + testInput);
    }
}

[System.Serializable]
public class TestClass2 : ITestClass
{
    [SerializeField] private int testInput;

    public TestClass2()
    {

    }

    public void TestMethod()
    {
        Debug.Log("TestClass1: " + testInput);
    }
}
