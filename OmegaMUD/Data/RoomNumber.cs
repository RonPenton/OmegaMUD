using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OmegaMUD
{
    public struct RoomNumber
    {
        public int Map;
        public int Room;

        public RoomNumber(int map, int room)
        {
            Map = map;
            Room = room;
        }

        public static bool operator ==(RoomNumber x, RoomNumber y)
        {
            if (x.Room == y.Room && x.Map == y.Map)
                return true;
            return false;
        }

        public static bool operator !=(RoomNumber x, RoomNumber y)
        {
            return !(x == y);
        }

        public override int GetHashCode()
        {
            return (Map >> 16 | Room).GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is RoomNumber))
                return false;
            return this == (RoomNumber)obj;
        }

        public override string ToString()
        {
            return String.Format("{0}:{1}", Map, Room);
        }
    }
}
