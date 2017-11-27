using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OmegaMUD
{
    public class LevelExitData : ExitData
    {
        public LevelExitData(Room room, Direction direction, int exitRoom, ExitType exitType, int p1, int p2, int p3, int p4)
            : base(room, direction, exitRoom, exitType, p1, p2, p3, p4)
        {
        }

        public int MinimumLevel { get { return Parameter1; } }
        public int MaximumLevel { get { return Parameter2; } }
        public int FailureMessage { get { return Parameter3; } }

        public override ExitUsageRequirements CanUseExit(MajorModelEntities model, PathfindingSettings settings)
        {
            var reqs = new ExitUsageRequirements();

            if (settings.PartyCharacters.Any(x => x.Level < MinimumLevel) || settings.PartyCharacters.Any(x => x.Level > MaximumLevel))
            {
                // Detected a disallowed level.
                reqs.Method = ExitMethod.CannotPass;
                return reqs;
            }

            // All party members can pass
            reqs.Method = ExitMethod.Normal;
            return reqs;
        }
    }
}
