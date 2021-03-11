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

namespace DiscordEssentials
{
    public class DiscordEssentials : Plugin
    {
        Player fakeGuest = new Player("Discord");

        public override string creator { get { return "Jerry Wang & SpicyCombo"; } }
        public override string MCGalaxy_Version { get { return "1.9.2.8"; } }
        public override string name { get { return "DiscordEssentials"; } }

        /*
        Hello there! Thank you for using the DiscordEssentials plugin! Now, before you set this up, please download
        everything from https://github.com/SpicyCombo/MCGalaxy-Things/tree/main/uploads/Discord.Net and put all 
        the .dll s at where MCGalaxyCLI.exe or MCGalaxyGUI.exe lives. Need support? Add SpicyCombo#1665 on Discord!
        */

        DateTime now = DateTime.Now;

        // General - These are the general settings that you can modify for the bot's partly behavior
        static string chatPrefix = "(Discord) "; // The prefix that's shown everytime in front of the chat, or in console when the plugin does something.
        static string prefixColor = "%5"; // The color of the prefix when it's shown in-game.
        static string authorColor = "%a"; // The default color of the Discord user when they are chatting.
        static string botToken = "get-your-token-from-discord"; // Here you configure your bot's token. Get one at https://discord.com/developers
        static string logPath = "plugins/DiscordPlugin/"; // Path for bot logging

        // Chat - These are the settings used for the chatting system.
        static bool chatFeature = true;
        static string chatMode = "advanced"; // Discord chat mode. But sadly, there is currently only the advanced chatmode.
        static string chatChannelID = "818654531784933376"; // If the chat feature is enabled, you will have to change 1234567890 to the channel id you want the bot to send messages to.
        Color EmbedColor = Color.Gold; // This is the color of your embed when you run .who command. Your current choices are:
        // Blue, DarkBlue, DarkerGrey, DarkGreen, DarkGrey, DarkMagenta, DarkOrange, DarkPurple, DarkRed, DarkReal, Default (Black), Gold, Green, LighterGrey, LightGrey, lightOrange, Magenta, Orange, Purple, Red, Real 

        // Bug Logger - Setting to enable if you want enable bug logging.
        static bool bugFeature = false;
        static string bugChannelID = "818277449309749248"; // If the bug report feature is enabled, you will have to change 1234567890 to the channel id you want the bot to send messages to.

        public override void Load(bool startup)
        {
            BugHandler();
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
                SocketIngameMessageToDiscord(socketmessage);
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

        void BugHandler()
        {
            Logger.LogHandler += HandleSendBug;
        }

        void HandleSendBug(LogType type, string error)
        {
            if (bugFeature == true)
            {
                if (type != LogType.Error) return;
                try { SocketBugMessageToDiscord("**An error has occurred on server " + Server.Config.Name + "!!**" + "```\n" + error + "\n```" + "\nPlease report this error to UnknownShadow200 for further examination."); } catch { }
            }
        }

        public async Task MainAsync()
        {
            var token = botToken;

            await Client.SetStatusAsync(UserStatus.Idle);
            await Client.LoginAsync(TokenType.Bot, token);
            await Client.StartAsync();
            SetActivityStatus();
        }

        public string Nickname { get; private set; }

        private async Task MessageReceivedAsync(SocketMessage message)
        {
            ulong[] channelID = { Convert.ToUInt64(chatChannelID) };

            if (!(message is SocketUserMessage))
                return;

            var DiscordUserNickname = (message.Author as SocketGuildUser).Nickname;

            if (chatFeature == true)
            {
                if (message.Author.Id == Client.CurrentUser.Id) return;

                if (!channelID.Contains(message.Channel.Id)) return;

                string DiscordMessageDisplay = message.Author.Username + ": " + message.Content;
                var DiscordMessage = message.Content;

                // Request from icanttellyou+, owner of The Build
                // To replace a character from Discord, simply use the code:
                // DiscordMessage = DiscordMessage.Replace("discord character", "ingame character")

                if (DiscordUserNickname == null)
                {
                    HandleLog(now.Year + "." + now.Month + "." + now.Day + " " + now.Hour + ":" + now.Minute + ":" + now.Second + chatPrefix + message.Author.Username + ": " + message.Content);
                    Logger.Log(LogType.SystemActivity, message.Author.Username + ": " + message.Content);
                    Chat.Message(ChatScope.Global, prefixColor + chatPrefix + authorColor + message.Author.Username + ": %f" + message.Content, null, null, true);
                }
                else
                {
                    HandleLog(now.Year + "." + now.Month + "." + now.Day + " " + now.Hour + ":" + now.Minute + ":" + now.Second + chatPrefix + DiscordUserNickname + ": " + message.Content);
                    Logger.Log(LogType.SystemActivity, chatPrefix + DiscordUserNickname + ": " + message.Content);
                    Chat.Message(ChatScope.Global, DiscordUserNickname + ": " + message.Content, null, null, true);
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

        void HandleCommand(Player p, string cmd, string args, CommandData data)
        {
            cmd = cmd.ToLower();
            if (!(cmd == "hide" || cmd == "possess" || cmd == "ohide")) return;

            SetActivityStatus();
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
                    // Start adding emotes after line.
                    // example for replacing characters: `msg = msg.Replace("ingame character", ":discordemote:");`

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
                    p.Message("%SIngame -> " + prefixColor + chatPrefix + "%f" + message);
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

            // Player name, join and disconnect
            msg = msg.Replace("+ λFULL", ":green_square: + **" + source.FullName + "**").Replace("+ λNICK", ":green_square: - **" + source.ColoredName + "**");
            msg = msg.Replace("- λFULL", ":red_square: - **" + source.FullName + "**").Replace("- λNICK", ":red_square: - **" + source.ColoredName + "**");
            msg = msg.Replace("λFULL:", "**" + source.FullName + ":**").Replace("λNICK:", "**" + source.ColoredName + ":**");
            msg = msg.Replace("λFULL", "**" + source.FullName + "**").Replace("λNICK", "**" + source.ColoredName + "**");

            // Color token removal
            msg = Colors.Escape(msg);
            msg = Colors.StripUsed(msg);
            // Start adding Emotes after this line.

            IngameMessageToDiscord(scope, msg, arg, filter);
        }

        /*
         * was gonna add external json, but i think i should just release this version of the plugin early ;p
         
        public struct ConfigJson
        {
            [JsonProperty("token")]
            public string Token { get; private set; }
            [JsonProperty("prefix")]
            public string Prefix { get; private set; }
        }
        */

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
