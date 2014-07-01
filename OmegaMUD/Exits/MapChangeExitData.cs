using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OmegaMUD
{
    public class MapChangeExitData : ExitData
    {
        public MapChangeExitData(Room room, Direction direction, int exitRoom, ExitType exitType, int p1, int p2, int p3, int p4)
            : base(room, direction, exitRoom, exitType, p1, p2, p3, p4)
        {
        }

        public int MapNumber { get { return Parameter1; } set { Parameter1 = value; } }

        public override RoomNumber AdjacentRoomNumber
        {
            get
            {
                return new RoomNumber(MapNumber, ExitRoom);
            }
        }
    }
}
