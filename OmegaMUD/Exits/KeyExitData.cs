using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using OmegaMUD.Commands;

namespace OmegaMUD
{
    public class KeyExitData : ExitData
    {
        public KeyExitData(Room room, Direction direction, int exitRoom, ExitType exitType, int p1, int p2, int p3, int p4)
            : base(room, direction, exitRoom, exitType, p1, p2, p3, p4)
        {
        }

        public int KeyRequired { get { return Parameter1; } }
        public int Status { get { return Parameter2; } }
        public int PicklockDifficulty { get { return Parameter3; } }
        public int StaysOpenFor { get { return Parameter4; } }

        public override ExitUsageRequirements CanUseExit(MajorModelEntities model, PathfindingSettings settings)
        {
            var reqs = new ExitUsageRequirements();
            int chanceOfPicking = settings.PartyCharacters.Max(p => p.Picklocks) + PicklockDifficulty;
            if (chanceOfPicking > settings.JustPickOrBashThreshold)
            {
                // Good chance of picking this lock, pick it even if the user has the item to save 
                // the number of uses on it.
                reqs.Method = ExitMethod.Pick;
                return reqs;
            }
            if (KeyRequired != 0 && settings.PartyCharacters.Any(p => p.Items.Any(x => x.Number == KeyRequired)))
            {
                // Low or no chance of picking lock, but a party member has the item, so use it.
                reqs.Method = ExitMethod.UseItem;
                reqs.RequiredItemNumber = KeyRequired;
                return reqs;
            }
            if (chanceOfPicking > 0)
            {
                // Low but some chance of picking the lock, user does not have the item
                reqs.Method = ExitMethod.Pick;
                return reqs;
            }
            if (KeyRequired == 0)
            {
                // There is no way through this exit; you cannot pick it, and there is no key to obtain.
                reqs.Method = ExitMethod.CannotPass;
                return reqs;
            }

            // User cannot pick, user has no item. Note the restriction so you can offer him a way around.
            reqs.Method = ExitMethod.NeedItem;
            reqs.RequiredItemNumber = KeyRequired;
            return reqs;
        }

        public override bool RemoveMatch(Player player, List<string> exits)
        {
            return KeyExitData.RemoveMatch(player, exits, this);
        }
        public static bool RemoveMatch(Player player, List<string> exits, ExitData exit)
        {
            var regex = GetBarrierExitRegex(player.Model, exit, BarrierState.Open, BarrierState.Closed);
            foreach (var exitString in exits.ToArray())
            {
                if (regex.IsMatch(exitString))
                {
                    exits.Remove(exitString);
                    return true;
                }
            }
            return false;
        }

        public static Regex GetBarrierExitRegex(MajorModelEntities model, ExitData exit, params BarrierState[] barrierStates)
        {
            string objectName = model.DoorNames[(int)exit.Direction];
            if (exit.ExitType == ExitType.Gate)
                objectName = model.GateNames[(int)exit.Direction];

            string barrierSegment = String.Join("|", barrierStates.Select(x => model.BarrierStateNames[(int)x]));
            barrierSegment = String.Format("({0}) ", barrierSegment);
            return new Regex(barrierSegment + objectName + " " + model.SpecialExitNames[(int)exit.Direction], RegexOptions.IgnoreCase);
        }

        public override bool IsCurrentlyPassable(MajorModelEntities model, SeenRoom room)
        {
            var regex = GetBarrierExitRegex(model, this, BarrierState.Open);
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
