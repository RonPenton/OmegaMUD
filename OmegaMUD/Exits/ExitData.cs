using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;
using OmegaMUD.Commands;

namespace OmegaMUD
{
    public enum ExitType
    {
        Normal = 0,
        Spell = 1,
        Key = 2,
        Item = 3,
        Toll = 4,
        Action = 5,
        Hidden = 6,
        Door = 7,
        MapChange = 8,
        Trap = 9,
        Text = 10,
        Gate = 11,
        RemoteAction = 12,
        Class = 13,
        Race = 14,
        Level = 15,
        Timed = 16,
        Ticket = 17,
        UserCount = 18,
        BlockGuard = 19,
        Alignment = 20,
        Delay = 21,
        Cast = 22,
        Ability = 23,
        SpellTrap = 24
    }
    public enum ExitMethod
    {
        Normal,
        CannotPass,
        NeedItem,
        NeedMoney,
        NeedAlignment,
        Bash,
        Pick,
        UseItem,
        PickOrBash
    }

    public class ExitUsageRequirements
    {
        public ExitMethod Method { get; set; }
        public int RequiredItemNumber { get; set; }
        public int RequiredMoneyAmount { get; set; }
        public int ExitWeight { get; set; }

        public ExitUsageRequirements()
        {
            ExitWeight = 1;
        }
    }

    public class ExitData
    {
        public ExitData(Room room, Direction direction, int exitRoom, ExitType exitType, int p1, int p2, int p3, int p4)
        {
            Room = room;
            Direction = direction;
            ExitRoom = exitRoom;
            ExitType = exitType;
            Parameter1 = p1;
            Parameter2 = p2;
            Parameter3 = p3;
            Parameter4 = p4;
        }
        public static ExitData Create(Room room, Direction direction, int exitRoom, int exitType, int p1, int p2, int p3, int p4)
        {
            ExitType type = (ExitType)exitType;
            if (type == ExitType.MapChange)
                return new MapChangeExitData(room, direction, exitRoom, type, p1, p2, p3, p4);
            else if (type == ExitType.Hidden)
                return new HiddenExitData(room, direction, exitRoom, type, p1, p2, p3, p4);
            else if (type == ExitType.Key)
                return new KeyExitData(room, direction, exitRoom, type, p1, p2, p3, p4);
            else if (type == ExitType.Item)
                return new ItemExitData(room, direction, exitRoom, type, p1, p2, p3, p4);
            else if (type == ExitType.Toll)
                return new TollExitData(room, direction, exitRoom, type, p1, p2, p3, p4);
            else if (type == ExitType.Door)
                return new DoorExitData(room, direction, exitRoom, type, p1, p2, p3, p4);
            else if (type == ExitType.Trap)
                return new TrapExitData(room, direction, exitRoom, type, p1, p2, p3, p4);
            else if (type == ExitType.Text)
                return new TextExitData(room, direction, exitRoom, type, p1, p2, p3, p4);
            else if (type == ExitType.Gate)
                return new GateExitData(room, direction, exitRoom, type, p1, p2, p3, p4);
            else if (type == ExitType.RemoteAction)
                return new RemoteActionExitData(room, direction, exitRoom, type, p1, p2, p3, p4);
            else if (type == ExitType.Class)
                return new ClassExitData(room, direction, exitRoom, type, p1, p2, p3, p4);
            else if (type == ExitType.Race)
                return new RaceExitData(room, direction, exitRoom, type, p1, p2, p3, p4);
            else if (type == ExitType.Level)
                return new LevelExitData(room, direction, exitRoom, type, p1, p2, p3, p4);
            return new ExitData(room, direction, exitRoom, type, p1, p2, p3, p4);
        }

        public Room Room { get; protected set; }
        public Direction Direction { get; protected set; }
        public int ExitRoom { get; protected set; }
        public ExitType ExitType { get; protected set; }
        public int Parameter1 { get; protected set; }
        public int Parameter2 { get; protected set; }
        public int Parameter3 { get; protected set; }
        public int Parameter4 { get; protected set; }

        public virtual bool RemoveMatch(Player player, List<string> exits)
        {
            if (exits.Any(x => x.Equals(player.Model.PlainExitNames[(int)Direction], StringComparison.InvariantCultureIgnoreCase)))
            {
                exits.Remove(player.Model.PlainExitNames[(int)Direction]);
                return true;
            }
            else
            {
                return false;
            }
        }

        public virtual RoomNumber AdjacentRoomNumber
        {
            get
            {
                return new RoomNumber(Room.Map_Number, ExitRoom);
            }
        }

        public virtual OutputCommand GetOutputCommand(ExitUsageRequirements requirements)
        {
            return new MovementOutputCommand(this);
        }

        public virtual string GetMovementCommand(Player player)
        {
            return player.Model.GetCommand((Command)Direction);
        }

        public virtual ExitUsageRequirements CanUseExit(MajorModelEntities model, PathfindingSettings settings)
        {
            return new ExitUsageRequirements();
        }

        /// <summary>
        /// This method determines if the exit is currently passable. This is primarily to detect if 
        ///     1) Hidden exits have been exposed -or-
        ///     2) Key/Door/Gate exits have been opened
        /// This will make no attempt to determine if the player has enough money or is the right 
        /// level/class/race/item to enter the exit. Those parameters should have already been verified via 
        /// the pathfinding algorithm.
        /// </summary>
        /// <param name="room"></param>
        /// <returns></returns>
        public virtual bool IsCurrentlyPassable(MajorModelEntities model, SeenRoom room)
        {
            if (room.Exits.Any(x => x.Equals(model.PlainExitNames[(int)Direction], StringComparison.InvariantCultureIgnoreCase)))
                return true;
            return false;
        }
    }
}
