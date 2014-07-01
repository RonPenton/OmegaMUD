using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OmegaMUD.Commands
{
    public class MovementOutputCommand : OutputCommand
    {
        ExitData exitData;
        RoomNumber startingRoom;
        RoomNumber destinationRoom;
        bool sentCommand = false;

        public MovementOutputCommand(ExitData exit)
        {
            this.exitData = exit;
            this.startingRoom = exit.Room.RoomNumber;
            this.destinationRoom = exit.AdjacentRoomNumber;
        }

        public override string GetOutput(Player player, GameStatusUpdate update)
        {
            // don't bother sending new output until a room is parsed.
            if (update != GameStatusUpdate.RoomParsed)
                return null;

            string direction = exitData.GetMovementCommand(player);
            if (destinationRoom == null && startingRoom == null)
            {
                // no source or destination assigned, so we're not verifying the location.
                // Simply return the movement command and be done.
                IsDone = true;
                return direction;
            }
            else if (startingRoom != null &&
                     player.RoomDetectionState == Parsing.RoomDetectionState.HaveMatch &&
                     player.LastSeenRoom.Match.RoomNumber == startingRoom)
            {
                // we're in the starting room, return the direction.
                sentCommand = true;
                return direction;
            }

            if (destinationRoom != null &&
            player.RoomDetectionState == Parsing.RoomDetectionState.HaveMatch &&
            player.LastSeenRoom.Match.RoomNumber == destinationRoom)
            {
                // we're at the destination.
                IsDone = true;
                return null;
            }

            return null;
        }
    }

}
