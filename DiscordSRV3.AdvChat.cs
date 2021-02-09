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
    public class DiscordSRV3 : Plugin_Simple
    {
        Player fakeGuest = new Player("Discord");

        public override string creator { get { return "Jerry Wang & SpicyCombo"; } }
        public override string MCGalaxy_Version { get { return "1.9.2.8"; } }
        public override string name { get { return "DiscordSRV3.AdvChatSingle"; } }

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
            SingleSocketMessageToDiscord("**:octagonal_sign: Server has started! / Plugin has been loaded!**");
            Client.LogoutAsync();
            OnChatEvent.Unregister(HandleChat);
            OnChatFromEvent.Unregister(HandleChatFrom);
            OnPlayerConnectEvent.Unregister(HandlePlayerConnect);
            OnPlayerDisconnectEvent.Unregister(HandlePlayerDisconnect);
        }

        private DiscordSocketClient Client;

        public DiscordSRV3()
        {
            Client = new DiscordSocketClient();

            Client.Log += Log;
            Client.Ready += ReadyAsync;
            Client.MessageReceived += MessageReceivedAsync;
        }

        void SocketMessageToDiscord(ChatScope scope, string socketmessage, object arg, ChatMessageFilter filter)
        {
            ChatMessageFilter scopeFilter = Chat.scopeFilters[(int)scope];

            if (scopeFilter(fakeGuest, arg) && (filter == null || filter(fakeGuest, arg)))
            {
                Client.GetGuild(1234567890).GetTextChannel(1234567890).SendMessageAsync(socketmessage);
            }
        }

        void SingleSocketMessageToDiscord(string socketmessage)
        {
            {
                Client.GetGuild(1234567890).GetTextChannel(1234567890).SendMessageAsync(socketmessage);
            }
        }

        public async Task MainAsync()
        {
            var token = "config token here";

            await Client.LoginAsync(TokenType.Bot, token);
            await Client.StartAsync();
            await Client.SetActivityAsync(new Game("with " + PlayerInfo.NonHiddenCount() + " players", ActivityType.Playing)).ConfigureAwait(false);
        }

        private Task ReadyAsync()
        {
            Logger.Log(LogType.SystemActivity, "DiscordSRV3 > " + $"{Client.CurrentUser} is connected!");
            SingleSocketMessageToDiscord("**:white_check_mark: Server has started! / Plugin has been loaded!**");
            return Task.CompletedTask;
        }

        public string Nickname { get; private set; }

        private async Task MessageReceivedAsync(SocketMessage message)
        {
            ulong[] channelIds =
            {
                767133494186082314
            };

            if (!(message is SocketUserMessage msg))
                return;

            var UNick = (message.Author as SocketGuildUser).Nickname;
            var chatColor = "%5";
            var chatPrefix = "(Discord)";
            var authorColor = "%e";

            if (message.Author.Id == Client.CurrentUser.Id) return;

            if (!channelIds.Contains(message.Channel.Id)) return;

            if (UNick == null)
            {
                Logger.Log(LogType.SystemActivity, "DiscordSRV3 > " + message.Author.Username + ": " + message.Content);
                Chat.Message(ChatScope.Global, chatColor + chatPrefix + " " + authorColor + message.Author.Username + ": %f" + message.Content, null, null, true);
            }
            else
            {
                Logger.Log(LogType.SystemActivity, "DiscordSRV3 > " + UNick + ": " + message.Content);
                Chat.Message(ChatScope.Global, chatColor + chatPrefix + " " + authorColor + UNick + ": %f" + message.Content, null, null, true);
            }

            if (message.Content.FirstOrDefault() != '.') return;

            var leftover = message.Content.Split(' ').FirstOrDefault();
            var result = leftover?.ToLower().Replace(".", string.Empty);

            if (result != "who") return;
            //write code here vvv

            var final = string.Empty;

            var usersEmbedBuilder = new EmbedBuilder()
.WithDescription($"**There are " + PlayerInfo.NonHiddenCount() + " players online.**" +
         $"")
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
                Logger.LogError("DiscordSRV3 > Error setting discord relay status", ex);
            }
        }

        void HandlePlayerConnect(Player p)
        {
            UpdateStatus();
        }

        void HandlePlayerDisconnect(Player p, string reason)
        {
            UpdateStatus();
        }

        private Task Log(LogMessage msg)
        {
            Logger.Log(LogType.SystemActivity, "DiscordSRV3 > " + msg.ToString());
            return Task.CompletedTask;
        }
    }
}
