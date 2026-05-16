using ScenarioFlow;


public class CharacterImageDecoder : IReflectable
{
    [DecoderMethod]
    public CharacterImageManager.Character ConvertToString(string input)
    {
        return CharacterImageManager.GetCharacterFromString(input);
    }
}
