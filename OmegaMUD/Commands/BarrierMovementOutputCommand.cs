using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OmegaMUD.Commands
{
    /// <summary>
    /// A class used to represent movement through a barrier (door/gate)
    /// </summary>
    public class BarrierMovementOutputCommand : OutputCommand
    {
        ExitData exitData;
        RoomNumber startingRoom;
        RoomNumber destinationRoom;
        ExitUsageRequirements requirements;

        public BarrierMovementOutputCommand(ExitData exit, ExitUsageRequirements requirements)
        {
            this.exitData = exit;
            this.startingRoom = exit.Room.RoomNumber;
            this.destinationRoom = exit.AdjacentRoomNumber;
            this.requirements = requirements;
        }

        public override string GetOutput(Player player, GameStatusUpdate update)
        {
            string direction = exitData.GetMovementCommand(player);

            if (update == GameStatusUpdate.RoomParsed &&
                player.RoomDetectionState == Parsing.RoomDetectionState.HaveMatch &&
                player.LastSeenRoom.Match.RoomNumber == startingRoom)
            {
                if (exitData.IsCurrentlyPassable(player.Model, player.LastSeenRoom))
                {
                    return direction;
                }
                else
                {
                    // barrier isn't passable, so commence with the suggested problem-solving method
                    switch (requirements.Method)
                    {
                        case ExitMethod.UseItem:
                            return GetUseKey(player);
                        case ExitMethod.Bash:
                            return GetBash(player);
                        case ExitMethod.Pick:
                            return GetPick(player);
                        case ExitMethod.PickOrBash:
                            if (player.Strength > player.Picklocks)
                                goto case ExitMethod.Bash;
                            else
                                goto case ExitMethod.Pick;
                        default:
                            // should never get here, but hey, shit happens.
                            throw new InvalidOperationException();
                    }
                }
            }
            else if(update == GameStatusUpdate.RoomParsed && 
                    player.RoomDetectionState == Parsing.RoomDetectionState.HaveMatch &&
                    player.LastSeenRoom.Match.RoomNumber == destinationRoom)
            {
                // we're at the destination.
                IsDone = true;
                return null;
            }
            else if (update == GameStatusUpdate.BashFail)
            {
                return GetBash(player);
            }
            else if (update == GameStatusUpdate.BashSuccess)
            {
                return direction;
            }
            else if (update == GameStatusUpdate.PickFail)
            {
                return GetPick(player);
            }
            else if (update == GameStatusUpdate.PickSuccess)
            {
                return GetOpen(player);
            }
            else if (update == GameStatusUpdate.DoorOpened)
            {
                return direction;
            }

            return null;
        }

        private string GetOpen(Player player)
        {
            return player.Model.OpenCommandString.Replace("<direction>", exitData.GetMovementCommand(player));
        }

        private string GetPick(Player player)
        {
            return player.Model.PickCommandString.Replace("<direction>", exitData.GetMovementCommand(player));
        }

        private string GetBash(Player player)
        {
            return player.Model.BashCommandString.Replace("<direction>", exitData.GetMovementCommand(player));
        }

        private string GetUseKey(Player player)
        {
            var item = player.Model.GetItem(requirements.RequiredItemNumber);
            return player.Model.UseKeyCommandString.Replace("<item>", item.Name).Replace("<direction>", exitData.GetMovementCommand(player));
        }
    }


}
