using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OmegaMUD.Commands
{
    public enum GameStatusUpdate
    {
        StatlineParsed,
        RoomParsed,
        ExperienceParsed,
        StatsParsed,
        InventoryParsed,
        BashSuccess,
        BashFail,
        PickSuccess,
        PickFail,
        DoorOpened
    }


    public abstract class OutputCommand
    {
        /// <summary>
        /// Whether or not the command is done and should be removed from the command queue.
        /// </summary>
        public bool IsDone { get; protected set; }

        /// <summary>
        /// Retrieves a textual command that should be sent in response to a game status update. 
        /// Null signifies that nothing should be sent.
        /// </summary>
        /// <param name="update"></param>
        /// <returns></returns>
        public abstract string GetOutput(Player player, GameStatusUpdate update);
    }
}