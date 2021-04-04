//reference System.Core.dll
//reference System.Net.dll
//reference System.dll
//reference Discord.Net.Core.dll
//reference Discord.Net.WebSocket.dll
//reference Discord.Net.Rest.dll

using System;
using System.Net;
using System.Threading.Tasks;
using MCGalaxy;
using MCGalaxy.Events.PlayerEvents;
using MCGalaxy.Events.ServerEvents;
using Discord.WebSocket;
using Discord;
using System.Collections.Generic;
using System.Linq;
using MCGalaxy.UI;

/* 

 **********************
 * Credits & Licenses *
 **********************

Some code are used and copied from other open source projects. See below 
for licenses and URLs of those open source repos, projects. And of course,
I will have my MIT license included here as well. Also extra thanks to
everyone in the ClassiCube community that brough up ideas for the plugin.

==========================================================================================
The SpicyCombo/MCGalaxy-Things Github Repo (https://github.com/SpicyCombo/MCGalaxy-Things)
==========================================================================================

MIT License

Copyright (c) 2021 SpicyCombo

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

================================================================
MCSharp (MCGalaxy, https://github.com/UnknownShadow200/MCGalaxy)
================================================================

    Copyright 2010 MCSharp team (Modified for use with MCZall/MCLawl/MCGalaxy)
    
    Dual-licensed under the Educational Community License, Version 2.0 and
    the GNU General Public License, Version 3 (the "Licenses"); you may
    not use this file except in compliance with the Licenses. You may
    obtain a copy of the Licenses at
    
    http://www.opensource.org/licenses/ecl2.php
    http://www.gnu.org/licenses/gpl-3.0.html
    
    Unless required by applicable law or agreed to in writing,
    software distributed under the Licenses are distributed on an "AS IS"
    BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express
    or implied. See the Licenses for the specific language governing
    permissions and limitations under the Licenses.

** Credits for
MCGalaxy Logging

========================================================
Discord.Net (https://github.com/Discord-Net/Discord.Net)
========================================================

The MIT License (MIT)

Copyright (c) 2015-2019 Discord.Net Contributors

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

** Credits for
Library used for Discord Connections
Modified code & Custom compiled libraries
 */

namespace DiscordEssentials
{
    public class DiscordEssentials : Plugin
    {
        Player fakeGuest = new Player("Discord");

        public override string creator { get { return "Jerry Wang & SpicyCombo"; } }
        public override string MCGalaxy_Version { get { return "1.9.2.8"; } }
        public override string name { get { return "DiscordEssentials"; } }

        /*
        Special thanks to UnknownShadow200 and Revenor on Github for helping with
        Discord.Net and MCGalaxy code. I couldn't have finished parts of this plugin
        without their help.
        */

        /*
        Hello there! Thank you for using the DiscordEssentials plugin! Now, before you set this up, please download
        everything from https://github.com/SpicyCombo/MCGalaxy-Things/tree/main/uploads/Discord.Net and put all 
        the .dll s at where MCGalaxyCLI.exe or MCGalaxyGUI.exe lives. Need support? Add SpicyCombo#1665 on Discord!
        */

        DateTime now = DateTime.Now;

        // General - These are the general settings that you can modify for the bot's partly behavior
        static string chatPrefix = "(Discord) "; // The prefix that's shown everytime in front of the chat, or in console when the plugin does something.
        static string prefixColor = "%5"; // The color of the prefix when it's shown in-game.
        string authorColor = "%a"; // The default color of the Discord user when they are chatting.
        string botToken = "get-your-token-from-discord"; // Here you configure your bot's token. Get one at https://discord.com/developers
        string logPath = "plugins/DiscordPlugin/"; // Path for bot logging
        UserStatus BotStatus = UserStatus.Online; // Can change the status of the bot to Online, Idle, Offline, DoNotDisturb, AFK, Invisible.

        // Chat - These are the settings used for the chatting system.
        static bool chatFeature = true;
        static string chatMode = "advanced"; // Discord chat mode. But sadly, there is currently only the advanced chatmode.
        static string chatChannelID = "1234567890"; // If the chat feature is enabled, you will have to change 1234567890 to the channel id you want the bot to send messages to.
        Color EmbedColor = Color.Gold; // This is the color of your embed when you run .who command. Your current choices are:
        // Blue, DarkBlue, DarkerGrey, DarkGreen, DarkGrey, DarkMagenta, DarkOrange, DarkPurple, DarkRed, DarkReal, Default (Black), Gold, Green, LighterGrey, LightGrey, lightOrange, Magenta, Orange, Purple, Red, Real 

        // Bug Logger - Setting to enable if you want enable bug logging.
        static bool bugFeature = false;
        static string bugChannelID = "1234567890"; // If the bug report feature is enabled, you will have to change 1234567890 to the channel id you want the bot to send messages to.

        // Discord Console - Console on Discord, you can view logs through Discord!
        static bool consoleFeature = false;
        static string consoleChannelID = "1234567890";

        public override void Load(bool startup)
        {
            Logger.LogHandler += LogHandler;
            Command.Register(new CmdDiscordBroadcast());
            ForceEnableTLS();
            MainAsync().GetAwaiter().GetResult();
            OnChatEvent.Register(HandleChat, Priority.Low);
            OnPlayerCommandEvent.Register(HandleCommand, Priority.Low);
            OnChatFromEvent.Register(HandleChatFrom, Priority.Low);
            OnPlayerConnectEvent.Register(HandlePlayerConnect, Priority.High);
            OnPlayerDisconnectEvent.Register(HandlePlayerDisconnect, Priority.High);
        }

        public override void Unload(bool shutdown)
        {
            Command.Unregister(Command.Find("DiscordBroadcast"));
            Client.LogoutAsync();
            OnChatEvent.Unregister(HandleChat);
            OnChatFromEvent.Unregister(HandleChatFrom);
            OnPlayerConnectEvent.Unregister(HandlePlayerConnect);
            OnPlayerDisconnectEvent.Unregister(HandlePlayerDisconnect);
            OnPlayerCommandEvent.Unregister(HandleCommand);
        }

        private static DiscordSocketClient Client;

        public DiscordEssentials()
        {
            Client = new DiscordSocketClient();

            Client.Log += Log;
            Client.MessageReceived += MessageReceivedAsync;
        }


        void IngameMessageToDiscord(ChatScope scope, string socketmessage, object arg, ChatMessageFilter filter)
        {
            ChatMessageFilter scopeFilter = Chat.scopeFilters[(int)scope];

            if (scopeFilter(fakeGuest, arg) && (filter == null || filter(fakeGuest, arg)))
            {
                MessageFilter(socketmessage);
            }
        }

        static void SocketIngameMessageToDiscord(string socketmessage)
        {
            {
                ulong convertedChannelID = Convert.ToUInt64(chatChannelID);
                var ClientChannel = Client.GetChannel(convertedChannelID) as IMessageChannel;
                ClientChannel.SendMessageAsync(socketmessage);
            }
        }

        static void SocketBugMessageToDiscord(string socketmessage)
        {
            {
                ulong convertedChannelID = Convert.ToUInt64(bugChannelID);
                var ClientChannel = Client.GetChannel(convertedChannelID) as IMessageChannel;
                ClientChannel.SendMessageAsync(socketmessage);

            }
        }

        static void SocketConsoleMessageToDiscord(string socketmessage)
        {
            {
                ulong convertedChannelID = Convert.ToUInt64(consoleChannelID);
                var ClientChannel = Client.GetChannel(convertedChannelID) as IMessageChannel;
                ClientChannel.SendMessageAsync(socketmessage);

            }
        }


        void LogHandler(LogType type, string log)
        {
            HandleBug(type, log);
            HandleConsole(type, log);
            HandleIRCChat(type, log);
            HandleIRCActivities(type, log);
        }

        void HandleBug(LogType type, string log) {
                if (bugFeature == true)
                {
                    if (type != LogType.Error) return;
                    try { SocketBugMessageToDiscord("**An error has occurred on server " + Server.Config.Name + "!!**" + "```\n" + log + "\n```" + "\nPlease report this error to UnknownShadow200 for further examination."); } catch { }
                }
        }

        static void HandleConsole(LogType type, string message)
        {
            if (!Server.Config.ConsoleLogging[(int)type]) return;

            message = Colors.Escape(message);
            message = Colors.StripUsed(message);

            switch (type)
            {
                case LogType.Error:
                    try { SocketConsoleMessageToDiscord("!!!Error! See " + FileLogger.ErrorLogPath + " for more information.") ; } catch { }
                        break;
                case LogType.BackgroundActivity:
                    break;
                default:
                    string now = DateTime.Now.ToString("(HH:mm:ss) ");
                    try { SocketConsoleMessageToDiscord(now + message); } catch { }
                    break;
            }
        }

        void HandleIRCChat(LogType type, string log)
        {
            if (chatFeature == true)
            {
                if (type != LogType.IRCChat) return;
                try { MessageFilter(log); } catch { }
            }
        }

        void HandleIRCActivities(LogType type, string log)
        {
            if (chatFeature == true)
            {
                if (type != LogType.IRC) return;
                try { MessageFilter(log); } catch { }
            }
        }


        public async Task MainAsync()
        {
            var token = botToken;

            await Client.SetStatusAsync(BotStatus);
            await Client.LoginAsync(TokenType.Bot, token);
            await Client.StartAsync();
            SetActivityStatus();
        }

        private async Task MessageReceivedAsync(SocketMessage message)
        {
            if (!(message is SocketUserMessage)) return;
            DiscordToConsole(message);
            DiscordToIngame(message);
        }

        async void DiscordToIngame(SocketMessage message)
        {
            ulong[] channelID = { Convert.ToUInt64(chatChannelID) };

            var DiscordUserNickname = (message.Author as SocketGuildUser).Nickname;
            var DiscordMessage = message.Content;

            if (chatFeature == true)
            {
                if (message.Author.Id == Client.CurrentUser.Id) return;

                if (!channelID.Contains(message.Channel.Id)) return;

                // Request from icanttellyou+, owner of The Build
                // To replace a character from Discord, simply use the code:
                // DiscordMessage = DiscordMessage.Replace("discord character", "ingame character")
                // To replace a character in a nickname of a user, use
                // DiscordUsername = DiscordUsername.Replace("discord character", "ingame character")
                // To replace a character in a nickname of a user, use
                // DiscordUserNickname = DiscordUserNickname.Replace("discord character", "ingame character")

                if (DiscordUserNickname == null)
                {
                    IngameChatHandler(message.Author.Username, DiscordMessage);
                }
                else
                {
                    IngameChatHandler(DiscordUserNickname, DiscordMessage);
                }

                if (message.Content.FirstOrDefault() != '.') return;

                var leftover = message.Content.Split(' ').FirstOrDefault();
                var result = leftover.ToLower().Replace(".", string.Empty);

                if (result != "who") return;
                var names = new List<string>();

                Player[] online = PlayerInfo.Online.Items;
                foreach (Player pl in online)
                {
                    names.Add(pl.DisplayName);
                }

                var PlayersCount = PlayerInfo.NonHiddenCount();
                var ConvertedNames = names.Join(", ");

                ConvertedNames = Colors.Escape(ConvertedNames);
                ConvertedNames = Colors.StripUsed(ConvertedNames);

                if (PlayersCount == 0)
                {
                    var usersEmbedBuilder = new EmbedBuilder()
    .WithDescription("**There are " + PlayerInfo.NonHiddenCount() + " players online.**")
    .WithColor(EmbedColor);

                    await message.Channel.SendMessageAsync(embed: usersEmbedBuilder.Build());
                }
                else
                {


                    var usersEmbedBuilder = new EmbedBuilder()
.WithDescription("**There are " + PlayerInfo.NonHiddenCount() + " players online.**\n" + "```" + ConvertedNames + "```")
.WithColor(EmbedColor);

                    await message.Channel.SendMessageAsync(embed: usersEmbedBuilder.Build());
                }
            }
            else if (chatFeature == false)
            {
                // Do nothing.
            }
        }

        void DiscordToConsole(SocketMessage message)
        {
            ulong[] channelID = { Convert.ToUInt64(consoleChannelID) };
            if (consoleFeature == true) return;
            if (!channelID.Contains(message.Channel.Id)) return;

            try
            {
                if (message.Content.Equals("/"))
                {
                    UIHelpers.RepeatCommand();
                }
                else if (message.Content.Length > 0 && message.Content[0] == '/')
                {
                    UIHelpers.HandleCommand(message.Content.Substring(1));
                }
                else
                {
                    UIHelpers.HandleChat(message.Content);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }
        }

        void HandleCommand(Player p, string cmd, string args, CommandData data)
        {
            cmd = cmd.ToLower();
            if (!(cmd == "hide" || cmd == "possess" || cmd == "ohide")) return;

            SetActivityStatus();
        }

        void IngameChatHandler(string DiscordUser, string Message)
        {
            // This is a member that handles sending chat messages in-game.

            HandleLog(now.Year + "." + now.Month + "." + now.Day + " " + now.Hour + ":" + now.Minute + ":" + now.Second + chatPrefix + DiscordUser + ": " + Message);
            Logger.Log(LogType.SystemActivity, DiscordUser + ": " + Message);
            Chat.Message(ChatScope.Global, prefixColor + chatPrefix + authorColor + DiscordUser + ": %f" + Message, null, null, true);
        }

        void MessageFilter(string msg)
        {
            // This is the filter you use to replace in-game characters to show differently in Discord.
            // To replace a character, use the code: `msg = msg.Replace("ingame character", ":discordemote:");`

            SocketIngameMessageToDiscord(msg);
        }

        void HandleChatFrom(ChatScope scope, Player source, string msg,
                        object arg, ref ChatMessageFilter filter, bool discord)
        {
            if (chatFeature == true)
            {
                if (chatMode == "advanced")
                {
                    fakeGuest.group = Group.DefaultRank;
                    if (filter != null && !filter(fakeGuest, arg)) return;

                    msg = msg.Replace("+ λFULL", ":green_square: + **" + source.FullName + "**").Replace("+ λNICK", ":green_square: - **" + source.ColoredName + "**");
                    msg = msg.Replace("- λFULL", ":red_square: - **" + source.FullName + "**").Replace("- λNICK", ":red_square: - **" + source.ColoredName + "**");
                    msg = msg.Replace("λFULL:", "**" + source.FullName + ":**").Replace("λNICK:", "**" + source.ColoredName + ":**");
                    msg = msg.Replace("λFULL", "**" + source.FullName + "**").Replace("λNICK", "**" + source.ColoredName + "**");

                    msg = Colors.Escape(msg);
                    msg = Colors.StripUsed(msg);
                    
                    IngameMessageToDiscord(scope, msg, arg, filter);
                }
                else if (chatMode == "simple")
                {
                    // do nothing. let playerdisconnect, playerconnect and playerchat event handlers do the work.
                }
                else
                {
                    Logger.Log(LogType.SystemActivity, chatPrefix + "You asshole. Why are you setting the mode to seomthing that doesn't even exist?");
                }
            }
        }


        public class CmdDiscordBroadcast : Command2
        {
            public override string name { get { return "DiscordBroadcast"; } }
            public override string type { get { return "other"; } }
            public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }

            public override void Use(Player p, string message, CommandData data)
            {
                string[] args = message.SplitSpaces(2);

                if (message.Length == 0)
                {
                    p.Message(prefixColor + chatPrefix + "%STip of the day: try actually typing something."); return;
                }
                else
                {
                    SocketIngameMessageToDiscord(message);
                    Logger.Log(LogType.BackgroundActivity ,chatPrefix + "%SIngame -> " + prefixColor + chatPrefix + "%f" + message);
                }
            }

            public override void Help(Player p)
            {
                p.Message("%T/DiscordBroadcast [Message]");
                p.Message("%SBroadcasts a message to Discord as the bot..");
            }
        }

        void HandleChat(ChatScope scope, Player source, string msg,
                        object arg, ref ChatMessageFilter filter, bool discord)
        {
            fakeGuest.group = Group.DefaultRank;
            if (filter != null && !filter(fakeGuest, arg)) return;

            msg = msg.Replace("+ λFULL", ":green_square: + **" + source.FullName + "**").Replace("+ λNICK", ":green_square: - **" + source.ColoredName + "**");
            msg = msg.Replace("- λFULL", ":red_square: - **" + source.FullName + "**").Replace("- λNICK", ":red_square: - **" + source.ColoredName + "**");
            msg = msg.Replace("λFULL:", "**" + source.FullName + ":**").Replace("λNICK:", "**" + source.ColoredName + ":**");
            msg = msg.Replace("λFULL", "**" + source.FullName + "**").Replace("λNICK", "**" + source.ColoredName + "**");

            msg = Colors.Escape(msg);
            msg = Colors.StripUsed(msg);

            IngameMessageToDiscord(scope, msg, arg, filter);
        }

        void SetActivityStatus()
        {
            try
            {
                Client.SetActivityAsync(new Game("with " + PlayerInfo.NonHiddenCount() + " players", ActivityType.Playing)).ConfigureAwait(false);
                // You may change "ActivityType.Playing" to "ActivityType.Listening" or "ActivityType.Watching"
            }
            catch (Exception ex)
            {
                Logger.LogError(chatPrefix + "Error setting discord relay status", ex);
            }
        }

        static void ForceEnableTLS()
        {
            // I copied this section from MCGalaxy's source code in case if the plugin gives TLS errors
            try { ServicePointManager.SecurityProtocol |= (SecurityProtocolType)0x300; } catch { }
            try { ServicePointManager.SecurityProtocol |= (SecurityProtocolType)0xC00; } catch { }
        }

        void HandlePlayerConnect(Player p)
        {
            SetActivityStatus();
        }

        void HandleLog(string log)
        {
            System.IO.FileInfo filedir = new System.IO.FileInfo(logPath + "chat-" + now.Year + "-" + now.Month + "-" + now.Day + ".txt");
            filedir.Directory.Create();

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(logPath + "chat-" + now.Year + "-" + now.Month + "-" + now.Day + ".txt", true))
            {
                file.WriteLine(now.Year + "." + now.Month + "." + now.Day + " " + log);
            }
        }

        void HandlePlayerDisconnect(Player p, string reason)
        {
            SetActivityStatus();
        }

        private Task Log(LogMessage msg)
        {
            HandleLog(msg.ToString());
            return Task.CompletedTask;
        }
    }
}
