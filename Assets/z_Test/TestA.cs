using System.Collections.Generic;
using UnityEngine;
using Refinter;
using System;

namespace Default
{
    interface BBB : IInterface
    {

    }
    class TTT: BBB
    {

    }
    class TTT1 : BBB
    {

    }
    [Important(2)]
    class TTT2 : BBB
    {

    }
    //[Important(1)]
    public class TestA:MonoBehaviour/*, BBB*/
    {
        BBB BBB;
        private void Start()
        {
            Debug.Log(BBB);
            Refinter.Reflection.Set<BBB>(new TTT());
            Debug.Log(BBB);
        }
    }
    
}
