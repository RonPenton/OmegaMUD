using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OmegaMUD.Commands;

namespace OmegaMUD
{
    public class GateExitData : ExitData
    {
        public GateExitData(Room room, Direction direction, int exitRoom, ExitType exitType, int p1, int p2, int p3, int p4)
            : base(room, direction, exitRoom, exitType, p1, p2, p3, p4)
        {
        }

        public int State { get { return Parameter1; } }
        public int ChanceToPickOrBash { get { return Parameter2; } }
        public int StaysOpenFor { get { return Parameter3; } }
        public int KeyRequired { get { return Parameter4; } }

        public override ExitUsageRequirements CanUseExit(MajorModelEntities model, PathfindingSettings settings)
        {
            return DoorExitData.CanUseExit(model, settings, ChanceToPickOrBash, KeyRequired);
        }

        public override bool RemoveMatch(Player player, List<string> exits)
        {
            return KeyExitData.RemoveMatch(player, exits, this);
        }

        public override bool IsCurrentlyPassable(MajorModelEntities model, SeenRoom room)
        {
            var regex = KeyExitData.GetBarrierExitRegex(model, this, BarrierState.Open);
            if (room.Exits.Any(x => regex.IsMatch(x)))
                return true;
            return false;
        }

        public override OutputCommand GetOutputCommand(ExitUsageRequirements requirements)
        {
            return new BarrierMovementOutputCommand(this, requirements);
        }
    }
}
