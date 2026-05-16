using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMain : MonoBehaviour
{
    [SerializeField] private TestEnum testEnum;

    [SerializeField] TestMainSO testMainSO;

    

    public enum TestEnum
    {
        HasState,
        NoState,
    }

    private void Start()
    {
        if(testMainSO.effectClass.UseState is StateAttackUpByAttribute targetState)
        {
            Debug.Log(testMainSO.effectClass.Effect.value);
            Debug.Log(testMainSO.effectClass.Effect.cardEffect.ToString());
        } 
    }
}
