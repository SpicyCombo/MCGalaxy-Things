reference System.Core.dll
reference System.Net.dll
reference System.dll
reference Discord.Net.Core.dll
reference Discord.Net.WebSocket.dll
reference Discord.Net.Rest.dll

// Put your token in on line (line)
// Configure how you want the status to look on line (line)
// If you DON'T follow everything said on the setup manual, then it will be your problem.
// But, if any error does occur, then please report them to me.

using System;
using System.Threading.Tasks;
using MCGalaxy;
using MCGalaxy.Events.PlayerEvents;
using MCGalaxy.Events.ServerEvents;
using Discord.WebSocket;
using Discord;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace DiscordSRV3
{
    public class DiscordSRV3 : Plugin_Simple
    {
        Player fakeGuest = new Player("Discord");

        public override string creator { get { return "Jerry Wang & SpicyCombo"; } }
        public override string MCGalaxy_Version { get { return "1.9.2.8"; } }
        public override string name { get { return "DiscordSRV3.AdvChatSingle"; } }

        public DiscordSocketClient Client { get => _client; set => _client = value; }

        public override void Load(bool startup)
        {
            SingleSocketMessageToDiscord("**:white_check_mark: Server has started! / Plugin has been loaded!**");
            // Command.Register(new Cmdnothing2());
            MainAsync().GetAwaiter().GetResult();
            OnChatEvent.Register(HandleChat, Priority.Low);
            OnChatFromEvent.Register(HandleChatFrom, Priority.Low);
            OnPlayerConnectEvent.Register(HandlePlayerConnect, Priority.High);
            OnPlayerDisconnectEvent.Register(HandlePlayerDisconnect, Priority.High);
        }

        public override void Unload(bool shutdown)
        {
            SingleSocketMessageToDiscord("**:octagonal_sign: Server has started! / Plugin has been loaded!**");
            // Command.Register(new Cmdnothing2());
            _client.LogoutAsync();
            OnChatEvent.Unregister(HandleChat);
            OnChatFromEvent.Unregister(HandleChatFrom);
            OnPlayerConnectEvent.Unregister(HandlePlayerConnect);
            OnPlayerDisconnectEvent.Unregister(HandlePlayerDisconnect);
        }

        private DiscordSocketClient _client;

        public DiscordSRV3()
        {
            _client = new DiscordSocketClient();

            _client.Log += Log;
            _client.Ready += ReadyAsync;
            _client.MessageReceived += MessageReceivedAsync;
        }

        /*
        public class Cmdnothing2 : Command2
        {
            public override string name { get { return "nothing2"; } }
            public override string type { get { return "other"; } }
            public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }

            public override void Use(Player p, string message, CommandData data)
            {
                int totalPlayers = 0;
                if (message.Length > 0)
                {
                    Group grp = Matcher.FindRanks(p, message);
                    if (grp == null) return;

                    GroupPlayers rankPlayers = Make(p, data, grp, ref totalPlayers);
                    if (totalPlayers == 0)
                    {
                        p.Message("There are no players of that rank online.");
                    }
                    else
                    {
                        Output(rankPlayers, p, false);
                    }
                    return;
                }

                List<GroupPlayers> allPlayers = new List<GroupPlayers>();
                foreach (Group grp in Group.GroupList)
                {
                    allPlayers.Add(Make(p, data, grp, ref totalPlayers));
                }

                if (totalPlayers == 1)
                {
                    p.Message("**There is 1 player online.**\n");
                }
                else
                {
                    p.Message("**There are " + totalPlayers + " players online.**\n");
                }

                for (int i = allPlayers.Count - 1; i >= 0; i--)
                {
                    Output(allPlayers[i], p, Server.Config.ListEmptyRanks);
                }
            }
            

            struct GroupPlayers { public Group group; public StringBuilder builder; }
            static GroupPlayers Make(Player p, CommandData data, Group group, ref int totalPlayers)
            {
                GroupPlayers list;
                list.group = group;
                list.builder = new StringBuilder();

                Player[] online = PlayerInfo.Online.Items;
                foreach (Player pl in online)
                {
                    if (pl.group != group || !p.CanSee(pl, data.Rank)) continue;

                    totalPlayers++;
                    Append(p, list, pl);
                }
                return list;
            }

            static void Append(Player target, GroupPlayers list, Player p)
            {
                StringBuilder data = list.builder;
                data.Append(' ');
                if (p.voice) { data.Append("+").Append(list.group.Color); }
                data.Append(Colors.StripUsed(target.FormatNick(p)));

                if (p.hidden) data.Append("*-hidden*");
                if (p.muted) data.Append("*-muted*");
                if (p.frozen) data.Append("*-frozen*");
                if (p.Game.Referee) data.Append("*-ref*");
                if (p.IsAfk) data.Append("*-afk*");
                if (p.Unverified) data.Append("*-unverified*");

                string lvlName = Colors.Strip(p.level.name); // for museums
                data.Append(" (").Append(lvlName).Append("),");
            }

            static string GetPlural(string name)
            {
                if (name.Length < 2) return name;

                string last2 = name.Substring(name.Length - 2).ToLower();
                if ((last2 != "ed" || name.Length <= 3) && last2[1] != 's')
                    return name + "s";
                return name;
            }

            static void Output(GroupPlayers list, Player p, bool showWhenEmpty)
            {
                StringBuilder data = list.builder;
                if (data.Length == 0 && !showWhenEmpty) return;
                if (data.Length > 0) data.Remove(data.Length - 1, 1);

                string title = "`:" + list.group.Color + GetPlural(list.group.Name) + ":`";
                p.Message(title + data.ToString());
            }

            public override void Help(Player p)
            {
                p.Message("%T/nothing2");
                p.Message("%HDoes nothing.");
            }
        }
        */

        void SocketMessageToDiscord(ChatScope scope, string socketmessage, object arg, ChatMessageFilter filter)
        {
            ChatMessageFilter scopeFilter = Chat.scopeFilters[(int)scope];

            if (scopeFilter(fakeGuest, arg) && (filter == null || filter(fakeGuest, arg)))
            {
                _client.GetGuild(1234567890).GetTextChannel(1234567890).SendMessageAsync(socketmessage);
            }
        }

        void SingleSocketMessageToDiscord(string socketmessage)
        {
            {
                _client.GetGuild(1234567890).GetTextChannel(1234567890).SendMessageAsync(socketmessage);
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
            Logger.Log(LogType.SystemActivity, "DiscordSRV3 > " + $"Discord{_client.CurrentUser} is connected!");
            return Task.CompletedTask;
        }

        public string Nickname { get; private set; }

        private async Task MessageReceivedAsync(SocketMessage message)
        {
            ulong[] channelIds =
            {
                767133494186082314
            };

            // Check if a user posted the message
            if (!(message is SocketUserMessage msg))
                return;

            var UNick = (message.Author as SocketGuildUser).Nickname;
            var chatColor = "%5";
            var chatPrefix = "(Discord)";
            var authorColor = "%e";

            if (message.Author.Id == _client.CurrentUser.Id) return;

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

            /*
            msg = msg.Replace("░", ":skull:");
            msg = msg.Replace("▒", ":man_running:");
            msg = msg.Replace("▓", ":thumbsup:");
            msg = msg.Replace("│", ":thumbsdown:");
            msg = msg.Replace("┤", ":wave:");
            msg = msg.Replace("╡", ":v:");
            msg = msg.Replace("╢", ":point_up:");
            msg = msg.Replace("╖", ":point_down:");
            msg = msg.Replace("╕", ":point_left");
            msg = msg.Replace("╣", ":point_right");
            msg = msg.Replace("║", ":frowning2:");
            msg = msg.Replace("╗", ":neutral_face:");
            msg = msg.Replace("╝", ":broken_heart:");
            msg = msg.Replace("╜", ":leafy_green:");
            msg = msg.Replace("╛", ":feet:");
            msg = msg.Replace("┐", ":slight_smile:");
            msg = msg.Replace("└", ":transgender_symbol:");
            msg = msg.Replace("┴", ":ghost:");
            msg = msg.Replace("┬", ":star:");
            msg = msg.Replace("├", ":cherry_blossom:");
            msg = msg.Replace("─", ":cloud_lightning:");
            msg = msg.Replace("┼", ":gun:");
            msg = msg.Replace("╞", ":gun:");
            msg = msg.Replace("╟", ":point_up_2:");
            msg = msg.Replace("╚", ":triumph:");
            msg = msg.Replace("╔", ":sunglasses:");
            msg = msg.Replace("╩", ":thinking:");
            msg = msg.Replace("╦", ":smirk:");
            msg = msg.Replace("╠", ":angry:");
            msg = msg.Replace("═", ":open_mouth:");
            msg = msg.Replace("═", ":open_mouth:");
            msg = msg.Replace("╬", "<:na2_facing_right:803018895400501298>");
            msg = msg.Replace("╬", "<:na2_facing_left:803019152733765643>");
            msg = msg.Replace("╨", ":warning:");
            msg = msg.Replace("╤", ":anger:");
            msg = msg.Replace("╤", ":sweat_drops:");
            msg = msg.Replace("╙", ":wastebasket:");
            msg = msg.Replace("╘", ":fist:");
            msg = msg.Replace("╒", "<:na2_ex_emote1:803019223450255370>");
            msg = msg.Replace("╓", ":ok_hand:");
            msg = msg.Replace("╫", "<:na2_joy:803019282887737385>");
            msg = msg.Replace("╪", ":eye:");
            msg = msg.Replace("┘", "<:na2_extreme_sad:803019347811369021>");
            msg = msg.Replace("┌", ":crescent_moon:");
            msg = msg.Replace("█", "<:na2_nothing_face:803019457085308958>");
            msg = msg.Replace("▄", "<:na2_cube1:803019566371438632>");
            msg = msg.Replace("▌", "<:na2_cube2:803019630368653333>");
            msg = msg.Replace("▐", ":fire:");
            msg = msg.Replace("▀", "<:na2_cube3:803019755996315650>");
            msg = msg.Replace("■", "<:na2_cube4:803019806026235984>");
            */

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

            /*
            msg = msg.Replace("░", ":skull:");
            msg = msg.Replace("▒", ":man_running:");
            msg = msg.Replace("▓", ":thumbsup:");
            msg = msg.Replace("│", ":thumbsdown:");
            msg = msg.Replace("┤", ":wave:");
            msg = msg.Replace("╡", ":v:");
            msg = msg.Replace("╢", ":point_up:");
            msg = msg.Replace("╖", ":point_down:");
            msg = msg.Replace("╕", ":point_left");
            msg = msg.Replace("╣", ":point_right");
            msg = msg.Replace("║", ":frowning2:");
            msg = msg.Replace("╗", ":neutral_face:");
            msg = msg.Replace("╝", ":broken_heart:");
            msg = msg.Replace("╜", ":leafy_green:");
            msg = msg.Replace("╛", ":feet:");
            msg = msg.Replace("┐", ":slight_smile:");
            msg = msg.Replace("└", ":transgender_symbol:");
            msg = msg.Replace("┴", ":ghost:");
            msg = msg.Replace("┬", ":star:");
            msg = msg.Replace("├", ":cherry_blossom:");
            msg = msg.Replace("─", ":cloud_lightning:");
            msg = msg.Replace("┼", ":gun:");
            msg = msg.Replace("╞", ":gun:");
            msg = msg.Replace("╟", ":point_up_2:");
            msg = msg.Replace("╚", ":triumph:");
            msg = msg.Replace("╔", ":sunglasses:");
            msg = msg.Replace("╩", ":thinking:");
            msg = msg.Replace("╦", ":smirk:");
            msg = msg.Replace("╠", ":angry:");
            msg = msg.Replace("═", ":open_mouth:");
            msg = msg.Replace("═", ":open_mouth:");
            msg = msg.Replace("╬", "<:na2_facing_right:803018895400501298>");
            msg = msg.Replace("╬", "<:na2_facing_left:803019152733765643>");
            msg = msg.Replace("╨", ":warning:");
            msg = msg.Replace("╤", ":anger:");
            msg = msg.Replace("╤", ":sweat_drops:");
            msg = msg.Replace("╙", ":wastebasket:");
            msg = msg.Replace("╘", ":fist:");
            msg = msg.Replace("╒", "<:na2_ex_emote1:803019223450255370>");
            msg = msg.Replace("╓", ":ok_hand:");
            msg = msg.Replace("╫", "<:na2_joy:803019282887737385>");
            msg = msg.Replace("╪", ":eye:");
            msg = msg.Replace("┘", "<:na2_extreme_sad:803019347811369021>");
            msg = msg.Replace("┌", ":crescent_moon:");
            msg = msg.Replace("█", "<:na2_nothing_face:803019457085308958>");
            msg = msg.Replace("▄", "<:na2_cube1:803019566371438632>");
            msg = msg.Replace("▌", "<:na2_cube2:803019630368653333>");
            msg = msg.Replace("▐", ":fire:");
            msg = msg.Replace("▀", "<:na2_cube3:803019755996315650>");
            msg = msg.Replace("■", "<:na2_cube4:803019806026235984>");
            */

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
