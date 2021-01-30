// Staff Command. I think it's really simple to say, but I hope this will be helpful for you.

using System;

namespace MCGalaxy 
{
	public class CmdStaff : Command2
	{
		
		public override string name { get { return "Staff"; } }
		public override string shortcut { get { return "StaffList"; } }
		public override bool MessageBlockRestricted { get { return false; } }
		public override string type { get { return "other"; } }
		public override bool museumUsable { get { return false; } }
		public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }
		public override void Use(Player p, string message, CommandData data)
		{
        	// Configure the rank names below.
            Command.Find("viewranks").Use(p, "Owner all");
            Command.Find("viewranks").Use(p, "administrator all");
            Command.Find("viewranks").Use(p, "moderator all"
		}
		public override void Help(Player p)
		{
			p.Message("%T/Staff");
            p.Message("%HShows the names of staff members");
		}
	}
}
