using DiscordSharp.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luigibot
{
    public class BaseOwnerModule : IModule
    {
        private IntegrationProcessManager mainEntry;
        private DateTime InitializeTime;

        public BaseOwnerModule(IntegrationProcessManager main)
        {
            mainEntry = main;
            InitializeTime = DateTime.Now;
            Name = "base";
            Description = "The base set of modules that cannot be enabled or disabled by the user.";
        }

        public override void Install(CommandsManager manager)
        {
            manager.AddCommand(new CommandStub("selfdestruct", "Shuts the bot down.", "", PermissionType.Owner, cmdArgs =>
            {
                //mainEntry.Exit();
            }), this);
            manager.AddCommand(new CommandStub("stopintegration", "Stops an integrations.", "", PermissionType.Owner, 1, cmdArgs =>
            {
                if (cmdArgs.Args.Count > 0)
                {
                    if (cmdArgs.FromIntegration.ToLower().Trim() == cmdArgs.Args[0].ToLower().Trim())
                    {
                        manager.Integration.SendMessage("You can't stop the integration you're running me from, silly!", cmdArgs.Channel);
                        return;
                    }
                    mainEntry.StopIntegration(cmdArgs.Args[0]);
                }
                else
                    manager.Integration.SendMessage("Stop what?", cmdArgs.Channel);
            }));
            manager.AddCommand(new CommandStub("startintegration", "Starts an integrations.", "", PermissionType.Owner, 1, cmdArgs =>
            {
                if (cmdArgs.Args.Count > 0)
                {
                    if (cmdArgs.FromIntegration.ToLower().Trim() == cmdArgs.Args[0].ToLower().Trim())
                    {
                        manager.Integration.SendMessage("what kind of paradox are you trying to create", cmdArgs.Channel);
                        return;
                    }
                    mainEntry.StartIntegration(cmdArgs.Args[0]);
                }
                else
                    manager.Integration.SendMessage("Start what?", cmdArgs.Channel);
            }));
            manager.AddCommand(new CommandStub("giveperm", "Gives the perm to the specified user (bot scope)", "", PermissionType.Owner, 2, e =>
            {
                //giveperm Admin <@2309208509852>
                if (e.Args.Count > 1)
                {
                    string permString = e.Args[0];
                    PermissionType type = PermissionType.User;
                    switch (permString.ToLower())
                    {
                        case "admin":
                            type = PermissionType.Admin;
                            break;
                        case "mod":
                            type = PermissionType.Mod;
                            break;
                        case "none":
                            type = PermissionType.None;
                            break;
                        case "user":
                            type = PermissionType.User;
                            break;
                    }
                    string id = e.Args[1].Trim(new char[] { '<', '@', '>' });
                    manager.AddPermission(id, type);
                    manager.Integration.SendMessage($"Given permission {type.ToString().Substring(type.ToString().IndexOf('.') + 1)} to <@{id}>!", e.Channel);
                }
            }), this);

            manager.AddCommand(new CommandStub("disablemodule", "Disables a module by name", "The module name is case insensitive.", PermissionType.Owner, 1, cmdArgs =>
            {
                if (cmdArgs.Args[0].Length > 0)
                {
                    if (!manager.ModuleEnabled(cmdArgs.Args[0]))
                    {
                        manager.Integration.SendMessage("Module already disabled!", cmdArgs.Channel);
                        return;
                    }
                    try
                    {
                        manager.DisableModule(cmdArgs.Args[0]);
                        manager.Integration.SendMessage($"Disabled {cmdArgs.Args[0]}.", cmdArgs.Channel);
                    }
                    catch (Exception ex)
                    { manager.Integration.SendMessage($"Couldn't disable module! {ex.Message}", cmdArgs.Channel); }
                }
                else
                {
                    manager.Integration.SendMessage("What module?", cmdArgs.Channel);
                }
            }), this);

            manager.AddCommand(new CommandStub("enablemodule", "Disables a module by name", "The module name is case insensitive.", PermissionType.Owner, 1, cmdArgs =>
            {
                if (cmdArgs.Args[0].Length > 0)
                {
                    if (manager.ModuleEnabled(cmdArgs.Args[0]))
                    {
                        manager.Integration.SendMessage("Module already enabled!", cmdArgs.Channel);
                        return;
                    }
                    try
                    {
                        manager.EnableModule(cmdArgs.Args[0]);
                        manager.Integration.SendMessage($"Enabled {cmdArgs.Args[0]}.", cmdArgs.Channel);
                    }
                    catch (Exception ex)
                    { manager.Integration.SendMessage($"Couldn't enable module! {ex.Message}", cmdArgs.Channel); }
                }
                else
                {
                    manager.Integration.SendMessage("What module?", cmdArgs.Channel);
                }
            }), this);

            manager.AddCommand(new CommandStub("modules", "Lists all the modules and whether or not they're enabled.", "",
                PermissionType.Owner, cmdArgs =>
                {
                    string msg = $"**Modules**";
                    foreach (var kvp in manager.Modules)
                    {
                        msg += $"\n`{kvp.Key.Name}` - {kvp.Value.ToString()}";
                    }
                    manager.Integration.SendMessage(msg, cmdArgs.Channel);
                }));

            manager.AddCommand(new CommandStub("changeprefix", "Changes the command prefix to a specified character.", "", PermissionType.Owner, 1, cmdArgs =>
            {
                //if (cmdArgs.Args.Count > 0)
                //{
                //    char oldPrefix = mainEntry.config.CommandPrefix;
                //    try
                //    {
                //        char newPrefix = cmdArgs.Args[0][0];
                //        mainEntry.config.CommandPrefix = newPrefix;
                //        manager.Client.SendMessage($"Command prefix changed to **{mainEntry.config.CommandPrefix}** successfully!");
                //    }
                //    catch (Exception)
                //    {
                //        manager.Client.SendMessage($"Unable to change prefix to `{cmdArgs.Args[0][0]}`. Falling back to `{oldPrefix}`.");
                //        mainEntry.config.CommandPrefix = oldPrefix;
                //    }
                //}
                //else
                //    manager.Client.SendMessage("What prefix?");
            }));

            manager.AddCommand(new CommandStub("flush", "Flushes various internal DiscordSharp caches.", "Flushes either `offline` or `messages`. \n  `offline` as parameter will flush offline users from the current server.\n  `messages` will flush the internal message log.", PermissionType.Owner, 1, cmdArgs =>
            {
                if (cmdArgs.FromIntegration.ToLower().Trim() == "discord")
                {
                    manager.Integration.SendMessage($"Coming soon!", cmdArgs.Channel);
                    //if (cmdArgs.Args.Count > 0)
                    //{
                    //    if (cmdArgs.Args[0].ToLower().Trim() == "offline")
                    //    {
                    //        int flushedCount = manager.Client.ClearOfflineUsersFromServer(cmdArgs.Channel.Parent);
                    //        cmdArgs.Channel.SendMessage($"Flushed {flushedCount} offliners from {cmdArgs.Channel.Parent.Name}.");
                    //    }
                    //    else if (cmdArgs.Args[0].ToLower().Trim() == "messages")
                    //    {
                    //        int flushedCount = manager.Client.ClearInternalMessageLog();
                    //        cmdArgs.Channel.SendMessage($"Flushed {flushedCount} messages from the internal message log.");
                    //    }
                    //}
                    //else
                    //{
                    //    cmdArgs.Channel.SendMessage("Flush what? The toilet?");
                    //}
                }
            }), this);

            manager.AddCommand(new CommandStub("about", "Shows bot information", "", cmdArgs =>
            {
                string message = manager.Integration.BoldText("About Luigibot");
                message += $"\nOwner: Mike Santiago\n";
                message += $"Libraries: DiscordSharp {typeof(DiscordSharp.DiscordClient).Assembly.GetName().Version.ToString()}\n";
                message += $"     SlackAPI w/ WebSocketSharp {typeof(SlackAPI.SlackClient).Assembly.GetName().Version.ToString()}\n";
                var uptime = (DateTime.Now - InitializeTime);
                message += $"Uptime: {uptime.Days} days, {uptime.Hours} hours, {uptime.Minutes} minutes\n";
                message += $"Runtime: ";
                if (Type.GetType("Mono.Runtime") != null)
                    message += "Mono\n";
                else
                    message += ".Net\n";

                message += $"OS: {OperatingSystemDetermination.GetUnixName()}\n";
                message += $"Current Integration: {manager.Integration.IntegrationName} ({manager.Integration.IntegrationDescription})\n";

                message += "Commands: " + manager.Commands.Count + "\n";

                manager.Integration.SendMessage(message, cmdArgs.Channel);
            }));
        }
    }
}
