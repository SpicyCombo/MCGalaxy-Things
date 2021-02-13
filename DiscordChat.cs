//reference System.Core.dll
//reference System.Net.dll
//reference System.dll
//reference Discord.Net.Core.dll
//reference Discord.Net.WebSocket.dll
//reference Discord.Net.Rest.dll

using System;
using System.Threading.Tasks;
using MCGalaxy;
using MCGalaxy.Events.PlayerEvents;
using MCGalaxy.Events.ServerEvents;
using Discord.WebSocket;
using Discord;
using System.Linq;
using System.Net;

namespace DiscordSRV3
{
    public class DiscordChat : Plugin_Simple
    {
        Player fakeGuest = new Player("Discord");

        public override string creator { get { return "Jerry Wang & SpicyCombo"; } }
        public override string MCGalaxy_Version { get { return "1.9.2.8"; } }
        public override string name { get { return "DiscordChat"; } }

        DateTime now = DateTime.Now;

        // Settings - DiscordChat
        // These are the settings that you can modify for the plugin to function differently.
        static string chatPrefix = "(Discord) "; // The prefix that's shown everytime in front of the chat, or in console when the plugin does something.
        static string prefixColor = "%5"; // The color of the prefix when it's shown in-game.
        static string authorColor = "%a"; // The default color of the Discord user when they are chatting.
        static string botToken = "oopsie-whoopsie-fucky-wucky"; // Here you configure your bot's token.
        static string logPath = "plugins/DiscordPlugin/";
        // Set your GuildID and ChannelID on line 86
        // Set the channelID(s) you want the who command to be listened on at line 111

        public override void Load(bool startup)
        {
            Command.Register(new CmdDiscordBroadcast());
            ForceEnableTLS();
            MainAsync().GetAwaiter().GetResult();
            OnChatEvent.Register(HandleChat, Priority.Low);
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
        }

        private static DiscordSocketClient Client;

        public DiscordChat()
        {
            Client = new DiscordSocketClient();

            Client.Log += Log;
            Client.Ready += ReadyAsync;
            Client.MessageReceived += MessageReceivedAsync;
        }


        void IngameMessageToDiscord(ChatScope scope, string socketmessage, object arg, ChatMessageFilter filter)
        {
            ChatMessageFilter scopeFilter = Chat.scopeFilters[(int)scope];

            if (scopeFilter(fakeGuest, arg) && (filter == null || filter(fakeGuest, arg)))
            {
                SocketMessageToDiscord(socketmessage);
            }
        }

        static void SocketMessageToDiscord(string socketmessage)
        {
            {
                Client.GetGuild(123456789).GetTextChannel(123456789).SendMessageAsync(socketmessage);
            }
        }

        public async Task MainAsync()
        {
            var token = botToken;

            await Client.LoginAsync(TokenType.Bot, token);
            await Client.StartAsync();
            await Client.SetActivityAsync(new Game("with " + PlayerInfo.NonHiddenCount() + " players", ActivityType.Playing)).ConfigureAwait(false);
        }

        private Task ReadyAsync()
        {
            Logger.Log(LogType.SystemActivity, "(Discord) " + Client.CurrentUser + " is connected!");
            return Task.CompletedTask;
        }

        public string Nickname { get; private set; }

        private async Task MessageReceivedAsync(SocketMessage message)
        {
            ulong[] channelIds =
            {
                796071862576611398
            };

            if (!(message is SocketUserMessage))
                return;

            var UNick = (message.Author as SocketGuildUser).Nickname;

            if (message.Author.Id == Client.CurrentUser.Id) return;

            if (!channelIds.Contains(message.Channel.Id)) return;

            if (UNick == null)
            {
                HandleLog(now.Year + "." + now.Month + "." + now.Day + " " + now.Hour + ":" + now.Minute + ":" + now.Second + chatPrefix + message.Author.Username + ": " + message.Content);
                Logger.Log(LogType.SystemActivity, chatPrefix + message.Author.Username + ": " + message.Content);
                Chat.Message(ChatScope.Global, prefixColor + chatPrefix + authorColor + message.Author.Username + ": %f" + message.Content, null, null, true);
            }
            else
            {
                HandleLog(now.Year + "." + now.Month + "." + now.Day + " " + now.Hour + ":" + now.Minute + ":" + now.Second + chatPrefix + UNick + ": " + message.Content);
                Logger.Log(LogType.SystemActivity, chatPrefix + UNick + ": " + message.Content);
                Chat.Message(ChatScope.Global, prefixColor + chatPrefix + authorColor + UNick + ": %f" + message.Content, null, null, true);
            }

            if (message.Content.FirstOrDefault() != '.') return;

            var leftover = message.Content.Split(' ').FirstOrDefault();
            var result = leftover.ToLower().Replace(".", string.Empty);

            if (result != "who") return;

            var usersEmbedBuilder = new EmbedBuilder()
.WithDescription("**There are " + PlayerInfo.NonHiddenCount() + " players online.**")
.WithColor(Color.Gold);

            await message.Channel.SendMessageAsync(embed: usersEmbedBuilder.Build());
        }

        void HandleChatFrom(ChatScope scope, Player source, string msg,
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
            // Start adding emotes after this line.
            // example for replacing characters: `msg = msg.Replace("ingame character", ":discordemote:");`

            IngameMessageToDiscord(scope, msg, arg, filter);
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
                    SocketMessageToDiscord(message);
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

        
        void UpdateStatus()
        {
            try
            {
                Client.SetActivityAsync(new Game("with " + PlayerInfo.NonHiddenCount() + " players", ActivityType.Playing)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Logger.LogError(chatPrefix + "Error setting discord relay status", ex);
            }
        }

        static void ForceEnableTLS()
        {
            // Force enable TLS 1.1/1.2, otherwise checking for updates on Github doesn't work
            try { ServicePointManager.SecurityProtocol |= (SecurityProtocolType)0x300; } catch { }
            try { ServicePointManager.SecurityProtocol |= (SecurityProtocolType)0xC00; } catch { }
        }

        void HandlePlayerConnect(Player p)
        {
            UpdateStatus();
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
            UpdateStatus();
        }

        private Task Log(LogMessage msg)
        {
            HandleLog(msg.ToString());
            return Task.CompletedTask;
        }
    }
}
