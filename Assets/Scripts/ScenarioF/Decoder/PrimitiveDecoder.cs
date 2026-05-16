using ScenarioFlow;
using ScenarioFlow.Scripts.SFText;

public class PrimitiveDecoder : IReflectable
{
    [DecoderMethod]
    public string ConvertToString(string input)
    {
        return input.Replace(SFText.LineBreakSymbol, "\n");
    }
}
