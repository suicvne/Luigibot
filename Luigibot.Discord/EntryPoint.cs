using Luigibot.Discord;
using LuigibotCommon.Integrations;

public class EntryPoint : IEntryPoint
{
    public IIntegration CreateIntegration()
    {
        DiscordIntegration disco = new DiscordIntegration(System.IO.File.ReadAllText("discord_token.txt"));
        return disco;
    }
}
