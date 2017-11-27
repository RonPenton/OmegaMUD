using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OmegaMUD
{
    public class RemoteActionExitData : ExitData
    {
        public RemoteActionExitData(Room room, Direction direction, int exitRoom, ExitType exitType, int p1, int p2, int p3, int p4)
            : base(room, direction, exitRoom, exitType, p1, p2, p3, p4)
        {
        }

        public int MessageNumber { get { return Parameter1; } }
        public int NumberOfRounds { get { return Parameter2; } }
        public int ExecutionMessageNumber { get { return Parameter3; } }
        public int ItemRequired { get { return Parameter4; } }

        public override bool RemoveMatch(Player player, List<string> exits)
        {
            return true;
        }
    }
}
