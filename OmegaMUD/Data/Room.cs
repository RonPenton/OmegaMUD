using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Documents;
using System.Text.RegularExpressions;

namespace OmegaMUD
{
    public partial class Room
    {
        public override string ToString()
        {
            return String.Format("{0}:{1} {2}", Map_Number, Room_Number, Name);
        }

        public enum RoomType
        {
            Normal = 0,
            Shop = 1,
            Arena = 2,
            Lair = 3,
            Hotel = 4,
            Colliseum = 5,
            Jail = 6,
            Library = 7
        }

        public RoomNumber RoomNumber { get { return new RoomNumber(Map_Number, Room_Number); } }

        /// <summary>
        /// Gets a list of all of the adjacent room numbers for the current room.
        /// </summary>
        /// <returns></returns>
        public List<RoomNumber> GetAdjacentRoomNumbers()
        {
            var list = new List<RoomNumber>();
            foreach (var exit in GetExits().Where(x => x.ExitType != ExitType.RemoteAction))
                list.Add(exit.AdjacentRoomNumber);
            return list;
        }

        public bool DoExitsMatch(Player player, IEnumerable<string> exitStrings)
        {
            List<string> list = new List<string>(exitStrings);
            List<ExitData> roomExits = GetExits().ToList();

            foreach (var exit in roomExits)
            {
                if (exit.RemoveMatch(player, list) == false)
                    return false;
            }

            // if there are no exits listed, then return true, because any room that had obvious exits would
            //  have already been eliminated by this point.
            if (list.Count == 1 && list[0].Equals(player.Model.NoExitsText, StringComparison.InvariantCultureIgnoreCase))
                return true;

            // No exits left, return true.
            if (list.Count == 0)
                return true;

            // Still some exits left that weren't able to be eliminated, this room cannot be a match.
            return false;
        }

        public IEnumerable<ExitData> GetExits()
        {
            if (Exit_0 != 0) yield return ExitData.Create(this, Direction.North, Exit_0, Type_0, Para1_0, Para2_0, Para3_0, Para4_0);
            if (Exit_1 != 0) yield return ExitData.Create(this, Direction.South, Exit_1, Type_1, Para1_1, Para2_1, Para3_1, Para4_1);
            if (Exit_2 != 0) yield return ExitData.Create(this, Direction.East, Exit_2, Type_2, Para1_2, Para2_2, Para3_2, Para4_2);
            if (Exit_3 != 0) yield return ExitData.Create(this, Direction.West, Exit_3, Type_3, Para1_3, Para2_3, Para3_3, Para4_3);
            if (Exit_4 != 0) yield return ExitData.Create(this, Direction.Northeast, Exit_4, Type_4, Para1_4, Para2_4, Para3_4, Para4_4);
            if (Exit_5 != 0) yield return ExitData.Create(this, Direction.Northwest, Exit_5, Type_5, Para1_5, Para2_5, Para3_5, Para4_5);
            if (Exit_6 != 0) yield return ExitData.Create(this, Direction.Southeast, Exit_6, Type_6, Para1_6, Para2_6, Para3_6, Para4_6);
            if (Exit_7 != 0) yield return ExitData.Create(this, Direction.Southwest, Exit_7, Type_7, Para1_7, Para2_7, Para3_7, Para4_7);
            if (Exit_8 != 0) yield return ExitData.Create(this, Direction.Up, Exit_8, Type_8, Para1_8, Para2_8, Para3_8, Para4_8);
            if (Exit_9 != 0) yield return ExitData.Create(this, Direction.Down, Exit_9, Type_9, Para1_9, Para2_9, Para3_9, Para4_9);
        }


        public ExitData GetExit(Direction direction)
        {
            return GetExits().SingleOrDefault(x => x.Direction == direction);
        }
    }



}
