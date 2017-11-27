using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OmegaMUD
{
    public class ItemExitData : ExitData
    {
        public ItemExitData(Room room, Direction direction, int exitRoom, ExitType exitType, int p1, int p2, int p3, int p4)
            : base(room, direction, exitRoom, exitType, p1, p2, p3, p4)
        {
        }

        public int ItemRequired { get { return Parameter1; } }
        public int FailedPassageMessage { get { return Parameter2; } }
        public int PassageMessage { get { return Parameter3; } }

        public override ExitUsageRequirements CanUseExit(MajorModelEntities model, PathfindingSettings settings)
        {
            var reqs = new ExitUsageRequirements();
            if (settings.PartyCharacters.Any(p => !p.Items.Any(i => i.Number == ItemRequired)))
            {
                // At least one member does not have the required item
                reqs.Method = ExitMethod.NeedItem;
                reqs.RequiredItemNumber = ItemRequired;
                return reqs;
            }

            // All party members have the required item, we're good to go.
            reqs.Method = ExitMethod.Normal;
            return reqs;
        }
    }
}
