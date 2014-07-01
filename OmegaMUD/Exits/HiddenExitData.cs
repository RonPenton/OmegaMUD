using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OmegaMUD
{
    public class HiddenExitData : ExitData
    {
        public HiddenExitData(Room room, Direction direction, int exitRoom, ExitType exitType, int p1, int p2, int p3, int p4)
            : base(room, direction, exitRoom, exitType, p1, p2, p3, p4)
        {
        }

        public int TypeOfExit { get { return Parameter1; } }
        public int PassageOpeningMessage { get { return Parameter3; } }
        public int ExitDescriptionMessage { get { return Parameter4; } }

        /// <summary>
        /// Gets the exit name for the hidden exit, if it were to be displayed to the user in the current room.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public string ExitName(MajorModelEntities model)
        {
            var message = model.Messages.SingleOrDefault(x => x.Number == this.ExitDescriptionMessage);
            string text = null;
            if (message != null)
                text = message.Line_1;
            else
                text = model.SecretPassageText;

            return text.Replace("%s", model.SpecialExitNames[(int)Direction]);
        }

        public override bool RemoveMatch(Player player, List<string> exits)
        {
            var hidden = this as HiddenExitData;
            string exitName = hidden.ExitName(player.Model);

            foreach (var exit in exits.ToArray())
            {
                if (exit.Equals(exitName, StringComparison.InvariantCultureIgnoreCase))
                {
                    exits.Remove(exit);
                    return true;
                }
            }

            // always return true. Exit may be hidden after all.
            return true;
        }

        public override bool IsCurrentlyPassable(MajorModelEntities model, SeenRoom room)
        {
            string exitName = ExitName(model);
            if (room.Exits.Any(x => x.Equals(exitName, StringComparison.InvariantCultureIgnoreCase)))
                return true;
            return false;
        }
    }
}
