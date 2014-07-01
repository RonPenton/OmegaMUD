using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OmegaMUD.Commands;

namespace OmegaMUD
{
    public class DoorExitData : ExitData
    {
        public DoorExitData(Room room, Direction direction, int exitRoom, ExitType exitType, int p1, int p2, int p3, int p4)
            : base(room, direction, exitRoom, exitType, p1, p2, p3, p4)
        {
        }

        public int State { get { return Parameter1; } }
        public int ChanceToPickOrBash { get { return Parameter2; } }
        public int StaysOpenFor { get { return Parameter3; } }
        public int KeyRequired { get { return Parameter4; } }

        public override ExitUsageRequirements CanUseExit(MajorModelEntities model, PathfindingSettings settings)
        {
            return CanUseExit(model, settings, ChanceToPickOrBash, KeyRequired);
        }

        public static ExitUsageRequirements CanUseExit(MajorModelEntities model, PathfindingSettings settings, int chanceToPickOrBash, int keyRequired)
        {
            var reqs = new ExitUsageRequirements();
            int chanceOfPicking = settings.PartyCharacters.Max(p => p.Picklocks) + chanceToPickOrBash;
            int chanceOfBashing = settings.PartyCharacters.Max(p => p.Strength) + chanceToPickOrBash;
            if (chanceOfPicking > settings.JustPickOrBashThreshold || chanceOfBashing > settings.JustPickOrBashThreshold)
            {
                // Good chance of picking or bashing this lock, pick/bash it even if the user has the item to save 
                // the number of uses on it.
                reqs.Method = ExitMethod.PickOrBash;
                return reqs;
            }
            if (keyRequired != 0 && settings.PartyCharacters.Any(p => p.Items.Any(x => x.Number == keyRequired)))
            {
                // Low or no chance of picking lock, but a party member has the item, so use it.
                reqs.Method = ExitMethod.UseItem;
                reqs.RequiredItemNumber = keyRequired;
                return reqs;
            }
            if (chanceOfPicking > 0 || chanceOfBashing > 0)
            {
                // Low but some chance of picking the lock, user does not have the item
                reqs.Method = ExitMethod.PickOrBash;
                return reqs;
            }
            if (keyRequired == 0)
            {
                // There is no way through this exit; you cannot pick it, and there is no key to obtain.
                reqs.Method = ExitMethod.CannotPass;
                return reqs;
            }

            // User cannot pick, user has no item. Note the restriction so you can offer him a way around.
            reqs.Method = ExitMethod.NeedItem;
            reqs.RequiredItemNumber = keyRequired;
            return reqs;
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
