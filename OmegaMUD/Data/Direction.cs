using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OmegaMUD
{
    public enum Direction : int
    {
        North = 0,
        South = 1,
        East = 2,
        West = 3,
        Northeast = 4,
        Northwest = 5,
        Southeast = 6,
        Southwest = 7,
        Up = 8,
        Down = 9
    }
    public enum BarrierState : int
    {
        Open = 0,
        Closed = 1
    }

    public enum RoomDisplayCommand
    {
        Move,
        Refresh,
        Look,
        Unknown
    }

}
