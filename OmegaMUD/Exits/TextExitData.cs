using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OmegaMUD
{
    public class TextExitData : ExitData
    {
        public TextExitData(Room room, Direction direction, int exitRoom, ExitType exitType, int p1, int p2, int p3, int p4)
            : base(room, direction, exitRoom, exitType, p1, p2, p3, p4)
        {
        }

        public int CommandMessage { get { return Parameter1; } }
        public int DescriptionMessage { get { return Parameter3; } }

        public override string GetMovementCommand(Player player)
        {
            return player.Model.Messages.Single(x => x.Number == CommandMessage).Line_1;
        }

        public override bool RemoveMatch(Player player, List<string> exits)
        {
            return true;
        }
    }
}
