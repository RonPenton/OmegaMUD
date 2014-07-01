using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OmegaMUD.Commands;

namespace OmegaMUD
{
    public class PathfindingSettings
    {
        /// <summary>
        /// A list of all players in the party
        /// </summary>
        public List<Player> PartyCharacters { get; private set; }

        /// <summary>
        /// The threshold at which the user just decides to pick or bash a door rather than use
        /// the required item. So if set to 10, then the user will always pick/bash if they have a
        /// more-than-10% chance of picking or bashing. If less than 10% then the user will attempt
        /// to use the key instead. Unless they do not have the key, in which case they'll resort
        /// back to picking/bashing. This option is here simply to help people avoid using up all
        /// of their key uses if they don't want to.
        /// </summary>
        public int JustPickOrBashThreshold { get; set; }


        public PathfindingSettings()
        {
            PartyCharacters = new List<Player>();
        }
    }

    public class Pathfinder
    {
        class RoomInfo
        {
            public Room Room { get; set; }
            public int Distance { get; set; }
            public RoomInfo Previous { get; set; }
            public C5.IPriorityQueueHandle<RoomInfo> Handle;
            public ExitData PreviousExit { get; set; }
            public ExitUsageRequirements Requirements { get; set; }
        }

        class RoomInfoComparer : IComparer<RoomInfo>
        {
            public int Compare(RoomInfo x, RoomInfo y)
            {
                return x.Distance.CompareTo(y.Distance);
            }
        }


        public static List<OutputCommand> Pathfind(MajorModelEntities model, RoomNumber start, RoomNumber finish, PathfindingSettings settings)
        {
            var dictionary = new Dictionary<RoomNumber, RoomInfo>();
            var queue = new C5.IntervalHeap<RoomInfo>(new RoomInfoComparer());

            var firstRoom = model.GetRoom(start);
            var firstInfo = new RoomInfo() { Room = firstRoom, Distance = 0 };
            queue.Add(ref firstInfo.Handle, firstInfo);
            dictionary[firstRoom.RoomNumber] = firstInfo;
            RoomInfo destination = null;

            while (!queue.IsEmpty)
            {
                var info = queue.DeleteMin();

                // we found the finish, so exit.
                if (info.Room.RoomNumber == finish)
                {
                    destination = info;
                    break;
                }

                var exits = from exit in info.Room.GetExits()
                            where exit.ExitType != ExitType.RemoteAction
                            select exit;

                foreach (var exit in exits)
                {
                    // query the exit to see if it can be used in our current state.
                    var requirements = exit.CanUseExit(model, settings);
                    if (requirements.Method == ExitMethod.CannotPass)
                        continue;

                    var adjacentNumber = exit.AdjacentRoomNumber;
                    RoomInfo adjacent = null;
                    if (!dictionary.TryGetValue(adjacentNumber, out adjacent))
                    {
                        adjacent = new RoomInfo();
                        adjacent.Room = model.GetRoom(adjacentNumber);
                        adjacent.Distance = Int32.MaxValue;
                        dictionary[adjacentNumber] = adjacent;
                    }


                    int travelCost = 1; // TODO: set to 1 for now, adjust later as heuristics play out.
                    if (info.Distance + travelCost < adjacent.Distance)
                    {
                        adjacent.Distance = info.Distance + travelCost;
                        adjacent.Previous = info;
                        adjacent.PreviousExit = exit;
                        adjacent.Requirements = requirements;

                        if (adjacent.Handle == null)
                            queue.Add(ref adjacent.Handle, adjacent);
                        else
                            queue.Replace(adjacent.Handle, adjacent);
                    }
                }
            }


            if (destination != null)
            {
                List<OutputCommand> list = new List<OutputCommand>();

                var x = destination;
                while (x.Previous != null)
                {
                    list.Add(x.PreviousExit.GetOutputCommand(x.Requirements));
                    x = x.Previous;
                }

                list.Reverse();
                return list;
            }
            else return null;
        }
    }

}
