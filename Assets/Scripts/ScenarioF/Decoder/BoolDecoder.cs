using ScenarioFlow;
using System.Diagnostics;
using System;
using UnityEngine;


public class BoolDecoder : IReflectable
{
    [DecoderMethod]
    public bool ConvertToBool(string input)
    {
        if(input.Equals("true", System.StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }
        else if (input.Equals("false", System.StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }
        UnityEngine.Debug.Log("BoolDecoder: Invalid input for boolean conversion: " + input);
        return false;
    }
}
