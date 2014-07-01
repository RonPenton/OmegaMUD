using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OmegaMUD
{
    public class TrapExitData : ExitData
    {
        public TrapExitData(Room room, Direction direction, int exitRoom, ExitType exitType, int p1, int p2, int p3, int p4)
            : base(room, direction, exitRoom, exitType, p1, p2, p3, p4)
        {
        }

        public int MaxDamage { get { return Parameter1; } }
        public int TrapType { get { return Parameter2; } }
        public int MessageOnPassage { get { return Parameter3; } }
        public int MessageOnFail { get { return Parameter4; } }
    }
}
