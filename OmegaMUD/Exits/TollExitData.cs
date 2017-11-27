using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OmegaMUD
{
    public class TollExitData : ExitData
    {
        public TollExitData(Room room, Direction direction, int exitRoom, ExitType exitType, int p1, int p2, int p3, int p4)
            : base(room, direction, exitRoom, exitType, p1, p2, p3, p4)
        {
        }

        public int TollAmountRequired { get { return Parameter1; } }

        public override ExitUsageRequirements CanUseExit(MajorModelEntities model, PathfindingSettings settings)
        {
            var reqs = new ExitUsageRequirements();
            var moneyRequired = model.TollMultiplier * TollAmountRequired;

            if (settings.PartyCharacters.Any(x => x.GetTotalMoney(model) < moneyRequired))
            {
                // someone doesn't have enough money, or hasn't disclosed that they have the money.
                // So, mark the exit as needing money.
                reqs.Method = ExitMethod.NeedMoney;
                reqs.RequiredMoneyAmount = moneyRequired;
                return reqs;
            }

            // All party members have the required money, we're good to go.
            reqs.Method = ExitMethod.Normal;
            return reqs;
        }
    }
}
