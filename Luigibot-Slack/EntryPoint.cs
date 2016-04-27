using LuigibotCommon.Integrations;
using LuigibotSlack;

public class EntryPoint : IEntryPoint
{
    public IIntegration CreateIntegration()
    {
        SlackIntegration slack = new SlackIntegration(System.IO.File.ReadAllText("slack_token.txt"));
        return slack;
    }
}
