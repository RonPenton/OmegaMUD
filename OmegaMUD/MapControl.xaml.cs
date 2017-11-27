using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;

namespace OmegaMUD
{
    /// <summary>
    /// Interaction logic for MapControl.xaml
    /// </summary>
    public partial class MapControl : UserControl
    {
        public Player Player { get; set; }
        RoomControl[,] Rooms;
        int xacross;
        int yacross;
        public int DrawDepth { get; set; }

        public MapControl()
        {
            InitializeComponent();
            DrawDepth = 16;
        }


        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            // get the number of rooms that will fit, and add 1.
            xacross = (int)(sizeInfo.NewSize.Width / RoomControl.Size) + 1;
            yacross = (int)(sizeInfo.NewSize.Height / RoomControl.Size) + 1;
            // then make sure the number is odd.
            if (xacross % 2 == 0)
                xacross++;
            if (yacross % 2 == 0)
                yacross++;

            var xoffset = (sizeInfo.NewSize.Width - (xacross * RoomControl.Size)) / 2;
            var yoffset = (sizeInfo.NewSize.Height - (yacross * RoomControl.Size)) / 2;
            Rooms = new RoomControl[xacross, yacross];

            for (int x = 0; x < xacross; x++)
            {
                for (int y = 0; y < yacross; y++)
                {
                    Rooms[x, y] = new RoomControl(Player);
                    Canvas.SetLeft(Rooms[x, y], RoomControl.Size * x + xoffset);
                    Canvas.SetTop(Rooms[x, y], RoomControl.Size * y + yoffset);
                    canvas.Children.Add(Rooms[x, y]);
                }
            }
        }

        RoomNumber RoomNumber;
        public void SetRoom(Room room)
        {
            RoomNumber = room.RoomNumber;

            App.Current.Dispatcher.BeginInvoke(new Action(delegate
            {
                Redraw();
            }));
        }

        private class RoomDrawingInfo
        {
            public int Depth { get; set; }
            public int X { get; set; }
            public int Y { get; set; }
            public RoomDrawingInfo(int depth, int x, int y)
            {
                Depth = depth;
                X = x;
                Y = y;
            }
        }

        private void Redraw()
        {
            ClearRoomControls();

            Dictionary<Room, RoomDrawingInfo> markedRooms = new Dictionary<Room, RoomDrawingInfo>();
            Queue<Room> queue = new Queue<Room>();

            var firstRoom = Player.Model.GetRoom(RoomNumber);

            queue.Enqueue(firstRoom);
            markedRooms[firstRoom] = new RoomDrawingInfo(0, xacross / 2, yacross / 2);

            while (queue.Count > 0)
            {
                var room = queue.Dequeue();
                var info = markedRooms[room];

                // Stop the search if we've reached draw depth.
                if (info.Depth >= DrawDepth)
                    continue;

                RoomControl control = null;
                if (info.X < xacross && info.X >= 0 && info.Y < yacross && info.Y >= 0)
                {
                    control = Rooms[info.X, info.Y];

                    // skip the room (and also its whole subtree) if a different room has already been drawn in the same place. This
                    // usually happens because the map designers aren't aware of euclidian geometry and made the map weird.
                    if (control.Drawn)
                        continue;
                    control.DrawRoom(room, info.Depth == 0);
                }


                var exits = from exit in room.GetExits()
                            where exit.ExitType != ExitType.RemoteAction
                            select exit;

                foreach (var exit in exits)
                {
                    // only follow the exit if it's within the same map, and doesn't go up or down.
                    if (exit.ExitType != ExitType.MapChange &&
                        exit.Direction != Direction.Up &&
                        exit.Direction != Direction.Down)
                    {
                        Room adjacent = Player.Model.GetRoom(exit.AdjacentRoomNumber);
                        if (!markedRooms.ContainsKey(adjacent))
                        {
                            var offset = AdjacentRoomOffset(exit.Direction);
                            markedRooms[adjacent] = new RoomDrawingInfo(info.Depth + 1, info.X + (int)offset.X, info.Y + (int)offset.Y);
                            queue.Enqueue(adjacent);
                        }
                    }

                    // it's ok to draw all exits though.
                    if (control != null)
                        control.DrawExit(room, exit, info.Depth == 0);
                }
            }
        }

        private void ClearRoomControls()
        {
            for (int x = 0; x < xacross; x++)
            {
                for (int y = 0; y < yacross; y++)
                {
                    Rooms[x, y].Clear();
                }
            }
        }

        Point AdjacentRoomOffset(Direction direction)
        {
            Point p = new Point();
            switch (direction)
            {
                case Direction.North:
                case Direction.Northeast:
                case Direction.Northwest:
                    p.Y = -1;
                    break;
                case Direction.South:
                case Direction.Southeast:
                case Direction.Southwest:
                    p.Y = 1;
                    break;
                default:
                    break;
            }

            switch (direction)
            {
                case Direction.East:
                case Direction.Northeast:
                case Direction.Southeast:
                    p.X = 1;
                    break;
                case Direction.West:
                case Direction.Northwest:
                case Direction.Southwest:
                    p.X = -1;
                    break;
                default:
                    break;
            }

            return p;
        }
    }
}
