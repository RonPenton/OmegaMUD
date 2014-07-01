using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media.Media3D;
using System.Text.RegularExpressions;

namespace OmegaMUD
{
    public static class Utilities
    {
        public static string AssembleDescriptionString(params string[] fragments)
        {
            StringBuilder builder = new StringBuilder();
            foreach (var fragment in fragments)
            {
                builder.Append(fragment.Trim()).Append(" ");
            }

            return builder.ToString().TrimEnd();
        }


        public static ScrollViewer FindScrollViewerDescendant(DependencyObject control)
        {
            if (control is ScrollViewer) return (control as ScrollViewer);

            int childCount = VisualTreeHelper.GetChildrenCount(control);
            for (int i = 0; i < childCount; i++)
            {
                ScrollViewer result = FindScrollViewerDescendant(VisualTreeHelper.GetChild(control, i));
                if (result != null) return result;
            }

            return null;
        }

        public static Dictionary<string, Group> MatchNamedCaptures(this Regex regex, string input)
        {
            var namedCaptureDictionary = new Dictionary<string, Group>();
            GroupCollection groups = regex.Match(input).Groups;
            string[] groupNames = regex.GetGroupNames();
            foreach (string groupName in groupNames)
            {
                int val;
                if (!Int32.TryParse(groupName, out val))
                    namedCaptureDictionary.Add(groupName, groups[groupName]);
            }
            return namedCaptureDictionary;
        }
    }

    public static class ParagraphExtensions
    {
        public static string LineString(this Paragraph paragraph)
        {
            StringBuilder builder = new StringBuilder();
            foreach (Run run in paragraph.Inlines)
                builder.Append(run.Text);
            return builder.ToString();
        }

        public static Color StartingColor(this Paragraph paragraph)
        {
            if (paragraph.Inlines.Count == 0)
                return Colors.Black;
            return ((SolidColorBrush)(paragraph.Inlines.FirstInline as Run).Foreground).Color;
        }


    }

    public static class DataUtilities
    {
        //public static void FillInMapCoordinates(MajorModelEntities model)
        //{
        //    HashSet<Room> markedRooms = new HashSet<Room>();
        //    Queue<Room> queue = new Queue<Room>();

        //    var roomPool = (from r in model.Rooms
        //                select r).ToDictionary(x => x.RoomNumber);

        //    var firstRoom = roomPool[new RoomNumber(1, 1)];
        //    firstRoom.X = firstRoom.Y = firstRoom.Z = 0;
        //    queue.Enqueue(firstRoom);
        //    markedRooms.Add(firstRoom);

        //    while (queue.Count > 0)
        //    {
        //        var room = queue.Dequeue();
        //        var exits = from exit in room.GetExits()
        //                    where exit.ExitType != Room.ExitType.RemoteAction
        //                    select exit;

        //        foreach (var exit in exits)
        //        {
        //            Room adjacent = roomPool[exit.AdjacentRoomNumber];
        //            if (!markedRooms.Contains(adjacent))
        //            {
        //                var offset = AdjacentRoomOffset(exit.Direction);
        //                adjacent.X = room.X + (int)offset.X;
        //                adjacent.Y = room.Y + (int)offset.Y;
        //                adjacent.Z = room.Z + (int)offset.Z;
        //                markedRooms.Add(adjacent);
        //                queue.Enqueue(adjacent);
        //            }
        //        }
        //    }

        //    model.SaveChanges();
        //}

        //static Point3D AdjacentRoomOffset(Direction direction)
        //{
        //    Point3D p = new Point3D();
        //    switch (direction)
        //    {
        //        case Direction.North:
        //        case Direction.Northeast:
        //        case Direction.Northwest:
        //            p.Y = -1;
        //            break;
        //        case Direction.South:
        //        case Direction.Southeast:
        //        case Direction.Southwest:
        //            p.Y = 1;
        //            break;
        //        default:
        //            break;
        //    }

        //    switch (direction)
        //    {
        //        case Direction.East:
        //        case Direction.Northeast:
        //        case Direction.Southeast:
        //            p.X = 1;
        //            break;
        //        case Direction.West:
        //        case Direction.Northwest:
        //        case Direction.Southwest:
        //            p.X = -1;
        //            break;
        //        default:
        //            break;
        //    }

        //    switch (direction)
        //    {
        //        case Direction.Up:
        //            p.Z = 1;
        //            break;
        //        case Direction.Down:
        //            p.Z = -1;
        //            break;
        //        default:
        //            break;
        //    }

        //    return p;
        //}
    }

}
