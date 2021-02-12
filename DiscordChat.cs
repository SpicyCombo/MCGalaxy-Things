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

namespace DiscordSRV3
{
    public class DiscordChat : Plugin_Simple
    {
        Player fakeGuest = new Player("Discord");

        public override string creator { get { return "Jerry Wang & SpicyCombo"; } }
        public override string MCGalaxy_Version { get { return "1.9.2.8"; } }
        public override string name { get { return "DiscordChat"; } }

        DateTime now = DateTime.Now;

        // Settings - DiscordSRV3
        // These are the settings that you can modify for the plugin to function differently.
        string chatPrefix = "(Discord) "; // The prefix that's shown everytime in front of the chat, or in console when the plugin does something.
        string prefixColor = "%5"; // The color of the prefix when it's shown in-game.
        string authorColor = "%2"; // The default color of the Discord user when they are chatting.
        string botToken = "get-your-token-from-discord"; // Here you configure your bot's token.
        string logPath = "plugins/DiscordPlugin/";

        public override void Load(bool startup)
        {
            MainAsync().GetAwaiter().GetResult();
            OnChatEvent.Register(HandleChat, Priority.Low);
            OnChatFromEvent.Register(HandleChatFrom, Priority.Low);
            OnPlayerConnectEvent.Register(HandlePlayerConnect, Priority.High);
            OnPlayerDisconnectEvent.Register(HandlePlayerDisconnect, Priority.High);
        }

        public override void Unload(bool shutdown)
        {
            Client.LogoutAsync();
            OnChatEvent.Unregister(HandleChat);
            OnChatFromEvent.Unregister(HandleChatFrom);
            OnPlayerConnectEvent.Unregister(HandlePlayerConnect);
            OnPlayerDisconnectEvent.Unregister(HandlePlayerDisconnect);
        }

        private DiscordSocketClient Client;

        public DiscordChat()
        {
            Client = new DiscordSocketClient();

            Client.Log += Log;
            Client.Ready += ReadyAsync;
            Client.MessageReceived += MessageReceivedAsync;
        }


        void SocketMessageToDiscord(ChatScope scope, string socketmessage, object arg, ChatMessageFilter filter)
        {
            ChatMessageFilter scopeFilter = Chat.scopeFilters[(int)scope];

            try {
                if (scopeFilter(fakeGuest, arg) && (filter == null || filter(fakeGuest, arg)))
                {
                    Client.GetGuild(123456789).GetTextChannel(1234567890).SendMessageAsync(socketmessage);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(chatPrefix + "Error sending Discord message: ", ex);
                HandleLog(now.Hour + ":" + now.Minute + ":" + now.Second + " " + "Error occurred while sending Discord message:" + ex);
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
            Logger.Log(LogType.SystemActivity, chatPrefix + Client.CurrentUser + "is connected!");
            HandleLog(now.Hour + ":" + now.Minute + ":" + now.Second + " " + "Bot user: " + Client.CurrentUser);
            return Task.CompletedTask;
        }

        public string Nickname { get; private set; }

        private async Task MessageReceivedAsync(SocketMessage message)
        {
            ulong[] channelIds =
            {
                767133494186082314
            };

            if (!(message is SocketUserMessage))
                return;

            var UNick = (message.Author as SocketGuildUser).Nickname;

            if (message.Author.Id == Client.CurrentUser.Id) return;

            if (!channelIds.Contains(message.Channel.Id)) return;

            if (UNick == null)
            {
                HandleLog(now.Year + "." + now.Month + "." + now.Day + " " + now.Hour + ":" + now.Minute + ":" + now.Second + " " + chatPrefix + message.Author.Username + ": " + message.Content);
                Logger.Log(LogType.SystemActivity, chatPrefix + message.Author.Username + ": " + message.Content);
                Chat.Message(ChatScope.Global, prefixColor + chatPrefix + authorColor + message.Author.Username + ": %f" + message.Content, null, null, true);
            }
            else
            {
                HandleLog(now.Year + "." + now.Month + "." + now.Day + " " + now.Hour + ":" + now.Minute + ":" + now.Second + " " + chatPrefix + UNick + ": " + message.Content);
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

            SocketMessageToDiscord(scope, msg, arg, filter);
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

            SocketMessageToDiscord(scope, msg, arg, filter);
        }

        void UpdateStatus()
        {
            try
            {
                Client.SetActivityAsync(new Game("with " + PlayerInfo.NonHiddenCount() + " players", ActivityType.Playing)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Logger.LogError(chatPrefix + "Error setting discord relay status: ", ex);
                HandleLog(now.Hour + ":" + now.Minute + ":" + now.Second + " " + "Error occurred while configuring remote status:" + ex);
            }
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
