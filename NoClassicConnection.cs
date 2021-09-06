using MCGalaxy.Events.PlayerEvents;

namespace Core
{

	public class NoClassicConnection : Plugin
	{
		public override string creator { get { return "Not SpicyCombo"; } }
		public override string MCGalaxy_Version { get { return "1.9.0.0"; } }
		public override string name { get { return "NoClassicConnection"; } }

		public override void Load(bool startup)
		{
			OnPlayerFinishConnectingEvent.Register(DoKickClients, Priority.High); //we use this because if not it will show disconnect in chat & relay
		}

		public override void Unload(bool shutdown)
		{
			OnPlayerFinishConnectingEvent.Unregister(DoKickClients);
		}

		void DoKickClients(Player p)
		{
			string app = p.appName;
			
			if (app == null /*&& app.CaselessContains("unknown")*/)
			{
				p.Leave(null, "Please select 'Enhanced' from the launcher.", true);
			}
		}
	}
}