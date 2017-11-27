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

namespace OmegaMUD
{
    /// <summary>
    /// Interaction logic for RoomControl.xaml
    /// </summary>
    public partial class RoomControl : UserControl
    {
        public bool Drawn { get; private set; }
        public Player Player { get; private set; }
        public Room Room { get; private set; }
        Line[] Lines;

        public static int Size = 22;

        public RoomControl(Player player)
        {
            InitializeComponent();

            this.Player = player;
            Lines = new Line[8];
            Lines[(int)Direction.North] = North;
            Lines[(int)Direction.East] = East;
            Lines[(int)Direction.South] = South;
            Lines[(int)Direction.West] = West;
            Lines[(int)Direction.Northeast] = NorthEast;
            Lines[(int)Direction.Northwest] = NorthWest;
            Lines[(int)Direction.Southeast] = SouthEast;
            Lines[(int)Direction.Southwest] = SouthWest;

            Clear();
        }

        public void DrawRoom(Room room, bool isCurrentRoom)
        {
            Clear();
            Room = room;
            Shape shape;
            if (room.Perm_NPC != 0)
                shape = Circle;
            else
                shape = Square;
            shape.Visibility = System.Windows.Visibility.Visible;

            this.ToolTip = String.Format("{0} ({1}:{2})", room.Name, room.Map_Number, room.Room_Number);

            if (room.Type == (int)Room.RoomType.Lair)
            {
                shape.RenderTransform = new RotateTransform(45);
                shape.Fill = new SolidColorBrush(Colors.Crimson);
            }
            else if (room.Type == (int)Room.RoomType.Shop)
            {
                shape.Fill = new SolidColorBrush(Colors.DarkGreen);
                shape.RenderTransform = new RotateTransform(0);
            }
            else 
            {
                shape.Fill = new SolidColorBrush(Colors.Gray);
                shape.RenderTransform = new RotateTransform(0);
            }

            if (isCurrentRoom)
            {
                shape.Stroke = new SolidColorBrush(Colors.LightCyan);
            }
            else
            {
                shape.Stroke = new SolidColorBrush(new Color() { G = 68, A = 255 });
            }
            Drawn = true;
        }

        public void DrawExit(Room room, ExitData exit, bool isCurrentRoom)
        {
            if (exit.Direction == Direction.Up)
            {
                Up.Visibility = System.Windows.Visibility.Visible;
            }
            else if(exit.Direction == Direction.Down)
            {
                Down.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                Lines[(int)exit.Direction].Visibility = System.Windows.Visibility.Visible;
            }
        }

        public void Clear()
        {
            Drawn = false;
            for (int i = 0; i < Lines.Length; i++)
                Lines[i].Visibility = System.Windows.Visibility.Hidden;
            Circle.Visibility = System.Windows.Visibility.Hidden;
            Square.Visibility = System.Windows.Visibility.Hidden;
            Up.Visibility = System.Windows.Visibility.Hidden;
            Down.Visibility = System.Windows.Visibility.Hidden;
            Room = null;
            this.ToolTip = null;
        }

        private void UserControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (Room != null)
                this.Player.SetDestination(Room.RoomNumber);
        }
    }
}
