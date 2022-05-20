//reference System.Core.dll

using MCGalaxy.Events.PlayerEvents;
using MCGalaxy.Eco;
using MCGalaxy;
using MCGalaxy.Commands;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Core
{
	public class BuyWarpPlugin : Plugin
	{
		public override string creator { get { return "SpicyCombo (originally from UnknownShadow200)"; } }
		public override string MCGalaxy_Version { get { return "1.9.2.3"; } }
		public override string name { get { return "WarpEcoItem"; } }

		//All defined variables
		Item item;
		public static PlayerExtList plwlist;
		/// <summary>A list of players and the player warps they own.</summary>
		public static PlayerExtList plw2list;
		/// <summary>A list of warps that traces back to their owner.</summary>
		public static PlayerList warpslist;
		/// <summary> A list of all the player warps.</summary>

		string PathDirectory = "text/PlayerWarps/";
		string PathP2W = "text/PlayerWarps/player-to-warps.txt";
		string PathW2P = "text/PlayerWarps/warp-to-player.txt";
		string PathWs = "text/PlayerWarps/all-warps.txt";
		string PathInfo = "text/PlayerWarps/readme.txt";

		public override void Load(bool startup)
		{
			item = new BuyWarpItem();
			Economy.Items.Add(item);
			// reload config in case user has changed price
			Command.Register(new CmdPlayerWarps());
			Economy.Load();
			OnPlayerCommandEvent.Register(HandlePlCommandEvent, Priority.Low);
			InitPWLists();
			plwlist = PlayerExtList.Load(PathP2W);
			plw2list = PlayerExtList.Load(PathW2P);
			warpslist = PlayerList.Load(PathWs);
		}

        #region COMMAND HANDLER
        void HandlePlCommandEvent(Player p, string command, string args, CommandData data) {
			List<string> warplist = warpslist.All();
			WarpList warps = WarpList.Global;

			if (command == "warp")
            {
				string[] parts = args.SplitSpaces();
				if (parts[0] == "delete")
                {
					if (!warplist.Contains(Matcher.FindWarps(null, warps, parts[1]).Name))
                    {
						// do nothing, because no one bought that warp specified for deletion
                    } else
                    {
						// Initialize Player to Warps list
						string data3 = plwlist.FindData(p.name);
						if (data3 == null) data3 = "";
						List<string> datas = data3.Split(',').ToList();
						if (data3 == "" || data3 == null || data3 == " ") plwlist.Remove(p.name);
						// ---------

						p.Message("%SThis warp you're deleting belongs to %b"+ plw2list.FindData(parts[1])+"%S.");

						// first, remove the warp from that player's list
						datas.Remove(parts[1]);
						datas.CaselessRemove("");
						string data2 = datas.Join(",");
						plwlist.Update(p.name, data2);
						plwlist.Save();
						// second, remove the traceback to owner
						plw2list.Remove(parts[1]);
						plw2list.Save();
						// now, remove it from the whole warps list
						warpslist.Remove(parts[1]);
						warpslist.Save();
                    }
                }
            } else { /* do nothing */ }
		}
        #endregion

        public override void Unload(bool shutdown)
		{
			Economy.Items.Remove(item);
			Command.Unregister(Command.Find("PlayerWarps"));
			OnPlayerCommandEvent.Unregister(HandlePlCommandEvent);
		}

		static string[] info = new string[]
		{
			"If you just deleted this file and it got regenerated, well don't worry - it's intended.",
			"You could, though just delete all the things here if you want space.",
			"If there is a file sharing violation error in the console, please unload and then load the plugin again. That will fix it.",
			"Also, please don't be stupid and delete all the files here while the plugin is loaded. It will cause errors!\n",

			"This folder contains all the data for the EcoWarpItem plugin.",
			"Each file stores data like bought warps and / or their owner.",
			"Please do not modify any of the files stored in this folder. \n",

			"[player-to-warps.txt]",
			"Stores the list of warps for a specific player.",
			"Format: PlayerName Warp1,Warp2,Warp3.... \n",

			"[warp-to-player.txt]",
			"Stores the owner of each warp so it can be traced back.",
			"Format: Warp Playername \n",

			"[all-warps.txt]",
			"Sotres all the player warps bought.",
			"Format: Warp1, Warp2, Warp3.... \n",

			"To have players create a warp, they will buy it by using /Buy MakeWarp [Warp Name]",
			"To have the player manage their warp they will have to use /PlayerWarp [subcommand] <arguements>",
			"If staffs want to delete a specific warp, they can delete it the normal way using /warp delete [warp name]\n",
			
			"If this plugin ever runs into a problem, contact me:",
			"Email: admin@basicsurvival.xyz",
			"Discord: SpicyCombo#1665"

		};

		void InitPWLists()
		{
			if (!Directory.Exists(PathDirectory))
			{
				Logger.Log(LogType.GameActivity, "%aIt looks like this is your first time loading PlayerWarps. If you see an error with file violation or similar problems, please unload the plugin and then load it again. For more information, please contact SpicyCombo.");
				Directory.CreateDirectory(PathDirectory);
			}
			if (!File.Exists(PathInfo))
				File.WriteAllLines(PathInfo, info);
			if (!File.Exists(PathP2W))
				File.Create(PathP2W);
			if (!File.Exists(PathW2P))
				File.Create(PathW2P);
			if (!File.Exists(PathWs))
				File.Create(PathWs);
		}
	}


	class BuyWarpItem : SimpleItem
	{
		public BuyWarpItem()
		{
			Aliases = new string[] { "makewarp" };
		}

		public override string Name { get { return "MakeWarp"; } }

		protected override void OnPurchase(Player p, string warp)
		{
			// Initialize Player to Warps list
			string data = BuyWarpPlugin.plwlist.FindData(p.name);
			if (data == null) data = "";
			List<string> datas = data.Split(',').ToList();
			if (data == "" || data == null || data == " ") BuyWarpPlugin.plwlist.Remove(p.name);
			// ---------

			if (warp.Length == 0)
			{
				p.Message("&WName of warp required"); return;
			}

			if (warp.Contains(",") || warp.Contains("|"))
            {
				p.Message("&WOne of the characters you typed is not allowed in warp names!"); return;
            }
			
			if (WarpList.Global.Exists(warp))
			{
				p.Message("&WWarp {0} already exists", warp); return;
			}

			if (!CheckPrice(p)) return;

			CommandData dataAsOwner = new CommandData();
			dataAsOwner.Rank = LevelPermission.Nobody;

			Command.Find("Warp").Use(p, "create " + warp, dataAsOwner);
			p.Message("%SThe warp %b" + warp + "%S is now created!");
			p.Message("%SUse %T/PlayerWarps %Sto manage personal warps.");
			Economy.MakePurchase(p, Price, "&3Warp:" + warp);

			// store it to the player to warps list!!!
			datas.Add(warp);
			datas.CaselessRemove("");
			string data2 = datas.Join(",");
			BuyWarpPlugin.plwlist.Update(p.name, data2);
			BuyWarpPlugin.plwlist.Save();

			// add it to the warp to player list so it's able to be traced back
			BuyWarpPlugin.plw2list.Update(warp, p.name);
			BuyWarpPlugin.plw2list.Save();

			// also, because it's a warp we add it to the warps
			BuyWarpPlugin.warpslist.Add(warp);
			BuyWarpPlugin.warpslist.Save();
		}
	}

	class CmdPlayerWarps : Command2
	{
		public override string name { get { return "PlayerWarps"; } }
		public override string shortcut { get { return "pw"; } }
		public override string type { get { return CommandTypes.Other; } }
		public override CommandPerm[] ExtraPerms
		{
			get
			{
				return new[] { new CommandPerm(LevelPermission.Operator, "can see the list of all player warps."),
					new CommandPerm(LevelPermission.Operator, "can check the owner of a player warp.") };
			}
		}

		public override void Use(Player p, string message, CommandData cmddata)
		{
			// use the global warp list. this is to check whether or not that warp exists.
			WarpList warplist = WarpList.Global;
			List<Warp> AllWarps = warplist.Items.ToList();

			// reminder: BuyWarpPlugin.plw2list is the list to traceback the owner of a warp.

			string[] part = message.SplitSpaces();
			string data = BuyWarpPlugin.plwlist.FindData(p.name);
			if (data == null) data = "";
			List<string> datas = data.Split(',').ToList();

			List<string> allwarpnames = BuyWarpPlugin.warpslist.All();

			if (message.Length == 0)
			{
				p.Message("%cYou didn't specify a subcommand! Displaying help...");
				Help(p);
			}
			else
			{
				if (part[0] == "create")
                {
					Command.Find("Buy").Use(p, "MakeWarp " + part[1]);
                }
				else if (part[0] == "list")
				{
					if (data == null || data == "") { p.Message("You haven't created a warp yet."); }
					else
					{
						p.Message("%SYou have created the following warps:");
						p.Message("%b" + datas.Join(", "));
					}
				}
				else if (part[0] == "delete")
				{
					if (part.Length == 1) { p.Message("Specify a player warp to delete!"); }
					else if (datas.Contains(part[1]))
					{
						string NewOrOldOwner = BuyWarpPlugin.plw2list.FindData(part[1]);
						if (!AllWarps.Contains(warplist.Find(part[1])))
						{
							p.Message("%SOh no! It looks like the player warp you tried to delete doesn't exist.");
						}
						else if (NewOrOldOwner != p.name)
						{
							p.Message("Oopsie whoopsie! Looks like someone else owns this warp now!");
						}
						else
						{
							if (!BuyWarpPlugin.warpslist.Contains(part[1]))
							{

								p.Message("%SThe player warp %b" + part[1] + "%S is now deleted.");
								Command.Find("warp").Use(p, "delete " + part[1]);

							}
						}
						datas.Remove(part[1]);
						datas.CaselessRemove("");
						BuyWarpPlugin.plwlist.Update(p.name, datas.Join(","));
						BuyWarpPlugin.plwlist.Save();

						BuyWarpPlugin.plw2list.Remove(part[1]);
						BuyWarpPlugin.plw2list.Save();

						// save for the list of all warps
						BuyWarpPlugin.warpslist.Remove(part[1]);
						BuyWarpPlugin.warpslist.Save();
					}
					else
					{
						p.Message("%SYou don't own this player warp!");
					}
				}
				else if (part[0] == "all")
				{
					if (!CheckExtraPerm(p, cmddata, 1)) { /* let it do its thing */ }
					else
					{
						// check if the player has the right permission
						if (allwarpnames.Count == 0) { p.Message("No one has bought a warp yet!"); }
						else
						{
							p.Message("%SHere's a list of all bought player warps:");
							p.Message("%b" + allwarpnames.Join(", "));
						}
					}
				}
				else if (part[0] == "owner")
				{

					if (!CheckExtraPerm(p, cmddata, 2)) { /* let it do its thing */ }
					else if (part.Length == 1) { p.Message("%cYou need to specify a warp name to check for its owner!"); }
					else if (!allwarpnames.Contains(part[1])) {
						p.Message("%cThat player warp doesn't exist!");
                    } else
					{
						//no need to explain lol
						p.Message("%SThe owner of the warp %b" + part[1] + "%S is %b" + BuyWarpPlugin.plw2list.FindData(part[1]) + "%S.");
					}
				}
				else
				{ 
					p.Message("&cThat subcommand doesn't exist! Displaying help...");
					Help(p);
				}
			}
		}

		public override void Help(Player p)
		{
			p.Message("&T/PlayerWarps create [name]");
			p.Message("&HCreates a warp with the name [name] at the player's position.");
			p.Message("&HCosts %b" + Economy.GetItem("MakeWarp") + "%H %3" + Server.Config.Currency +"%S.");
			
			p.Message("&T/PlayerWarps list");
			p.Message("&HLists all purchased player warps.");
			p.Message("&T/PlayerWarps delete [warp]");
			p.Message("&HDeletes the player warp specified. No refunds.");
			p.Message("&T/PlayerWarps all");
			p.Message("&HShows all the player warps bought by players.");
			p.Message("&T/PlayerWarps owner [warp]");
			p.Message("&HShows the owner of [warp].");
			p.Message("&H- If you are an staff, you can delete other player's warps just by deleting it the regular way using &T/Warp delete&H.");
		}
	}
}
