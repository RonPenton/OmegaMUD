using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using System.Windows.Media;

namespace OmegaMUD
{
    public partial class MainWindow
    {
        //private void ParseParagraph(MudParagraph lastParagraph)
        //{
        //    if (!isInMajorMUD && lastParagraph.LineString() == "[MAJORMUD]: e")
        //    {
        //        isInMajorMUD = true;
        //        StatusText.Text = "Entered MajorMUD!";
        //    }
        //    else if (isInMajorMUD && lastParagraph.LineString().StartsWith("Your character has been saved."))
        //    {
        //        isInMajorMUD = false;
        //        StatusText.Text = "Exited MajorMUD!";
        //    }

        //    if (isInMajorMUD)
        //    {
        //        TestRoomState(lastParagraph);
        //        if (roomState == RoomState.Confirmed)
        //        {
        //            ParseRoom();
        //        }

        //        if (isLooking == true)
        //        {
        //            if ((lastParagraph.LineString() == "The door is closed in that direction!" ||
        //                (lastParagraph.LineString().StartsWith("There are no exits "))))
        //            {
        //                isLooking = false;
        //            }
        //        }

        //        if (isMoving == true)
        //        {
        //            if (lastParagraph.LineString() == "The door is closed!" ||
        //                lastParagraph.LineString() == "The gate is closed!" ||
        //                lastParagraph.LineString() == "There is no exit in that direction!")
        //            {
        //                isMoving = false;
        //            }
        //        }
        //    }
        //}

    }
}
