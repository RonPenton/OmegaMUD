using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OmegaMUD.Parsing
{
    public static class LineParser
    {
        public static void ParseLine( MudParagraph paragraph, Player player)
        {
            if (paragraph.IsMatch(player.Model.BashSuccessRegex, "BashSuccessRegex"))
                player.UpdateGameStatus(Commands.GameStatusUpdate.BashSuccess);
            else if (paragraph.IsMatch(player.Model.BashFailRegex, "BashFailRegex"))
                player.UpdateGameStatus(Commands.GameStatusUpdate.BashFail);
            else if (paragraph.IsMatch(player.Model.PickSuccessRegex, "PickSuccessRegex"))
                player.UpdateGameStatus(Commands.GameStatusUpdate.PickSuccess);
            else if (paragraph.IsMatch(player.Model.PickFailRegex, "PickFailRegex"))
                player.UpdateGameStatus(Commands.GameStatusUpdate.PickFail);
            else if (paragraph.IsMatch(player.Model.DoorOpenRegex, "DoorOpenRegex"))
                player.UpdateGameStatus(Commands.GameStatusUpdate.DoorOpened);
        }
    }
}
