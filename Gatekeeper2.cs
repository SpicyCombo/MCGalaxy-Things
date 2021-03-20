using System;
using MCGalaxy;
using MCGalaxy.Commands;
using MCGalaxy.Events.PlayerEvents;

namespace myServer
{

    public sealed class GateKeeper : Plugin
    {
        public override string name { get { return "GateKeeper"; } }
        public override string creator { get { return "crush_"; } }
        public string version { get { return "18.04.20"; } }
        public override string MCGalaxy_Version { get { return "1.9.1.9"; } }

        public override void Load(bool startup)
        {
            OnPlayerCommandEvent.Register(OnPlrCommand, Priority.Low);
        }

        public override void Unload(bool shutdown)
        {
            OnPlayerCommandEvent.Unregister(OnPlrCommand);
        }


        void OnPlrCommand(Player subject, string cmd, string argString, CommandData data)
        {
            if (subject.group.Permission >= LevelPermission.Operator) { return; }
            string[] args = argString.SplitSpaces();

            if (cmd.CaselessEq("tp") || cmd.CaselessEq("teleport") ||
            cmd.CaselessEq("tpp") && data.Context != CommandContext.MessageBlock)
            {

                if (subject.GetMotd().CaselessContains("-tp"))
                {
                    subject.Message("%STeleportation isn't allowed in the level you are currently in.");
                    subject.cancelcommand = true;
                    return;
                }

                if (args.Length == 1)
                {
                    Player tpObject = PlayerInfo.FindMatches(subject, args[0]);

                    if (tpObject != null && tpObject.GetMotd().CaselessContains("-tp"))
                    {
                            subject.Message("%STeleportation isn't allowed in the level that player is in as the map " +  tpObject.level.name " has hacks disabled. Using %a/Goto%S instead.");
                        subject.cancelcommand = true;
                        return;

                    }
                }
            }

        }

    }

}
