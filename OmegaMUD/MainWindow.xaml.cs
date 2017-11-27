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
using OmegaMUD.Telnet;
using System.Reflection;
using System.Threading;

namespace OmegaMUD
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        OmegaMUDEntities OmegaModel = new OmegaMUDEntities();

        public MainWindow()
        {
            InitializeComponent();


            //var model = new MajorModelEntities();
            //var rs = model.Rooms.ToList();
            //var exits = (from r in rs
            //             from e in r.GetExits()
            //             where (e.ExitType == ExitType.Door ||
            //                   e.ExitType == ExitType.Gate) &&
            //                   e.Parameter2 < 0
            //             select new { Room = r, Chance = e.Parameter2 }).ToList();

            //DateTime now = DateTime.Now;
            //var model = new MajorModelEntities();
            //model.PreCacheMap();
            //var diff = DateTime.Now - now;

            //now = DateTime.Now;
            //var path = Pathfinder.Pathfind(model, new RoomNumber(1, 1), new RoomNumber(9, 972));
            //diff = DateTime.Now - now;


            //now = DateTime.Now;
            //path = Pathfinder.Pathfind(model, new RoomNumber(1, 1), new RoomNumber(1, 69));
            //diff = DateTime.Now - now;


            //DataUtilities.FillInMapCoordinates(new MajorModelEntities());
            //var rs = majorModel.Rooms.ToList();
            //var exits = (from r in rs
            //             from e in r.GetExits()
            //             where e.ExitType == Room.ExitType.Text
            //             select e.Parameter3).Distinct().ToList();
            //var messages = (from m in majorModel.Messages
            //                where exits.Contains(m.Number)
            //                select m).ToList();


            //var es = (from r in rs
            //             from e in r.GetExits()
            //             where e.ExitType == Room.ExitType.Text
            //             select e).ToList();
            //var e1s = es.Where(x => x.Parameter3 == messages[0].Number).ToList();
            //var e2s = es.Where(x => x.Parameter3 == messages[19].Number).ToList();
            //var e3s = es.Where(x => x.Parameter3 == messages[20].Number).ToList();
            //var e4s = es.Where(x => x.Parameter3 == messages[58].Number).ToList();

        }

        #region UIEvents

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            //if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
            //    return;
            //if (e.Key == Key.C && (e.KeyboardDevice.Modifiers & ModifierKeys.Control) != 0)
            //    return;
            //InputBox.Focus();
        }


        private void Window_Closed(object sender, EventArgs e)
        {
            foreach (var player in Players)
            {
                player.Disconnect();
            }

            Application.Current.Shutdown();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var players = from player in OmegaModel.Players.ToList()
                          where !Players.Any(x => x.IsEqual(player))
                          orderby player.BBSName, player.PlayerName
                          select player;

            foreach (var player in players)
            {
                var item = new MenuItem();
                item.Header = String.Format("{0} - {1}", player.BBSName, player.PlayerName);
                item.Tag = player;
                item.Click += new RoutedEventHandler(playerOpen_Click);
                CharacterMenu.Items.Add(item);
            }
        }

        void playerOpen_Click(object sender, RoutedEventArgs e)
        {
            Player player = (sender as MenuItem).Tag as Player;

            PlayerInterfaceControl i = new PlayerInterfaceControl(player.PlayerName);
            TabItem tab = new TabItem();
            tab.Content = i;
            tab.Header = String.Format("{0} - {1}", player.BBSName, player.PlayerName);
            InterfaceTabControl.Items.Add(tab);
            InterfaceTabControl.SelectedItem = tab;

            CharacterMenu.Items.Remove(sender);
        }


        private IEnumerable<Player> Players
        {
            get
            {
                return from TabItem tab in InterfaceTabControl.Items
                       let i = tab.Content as PlayerInterfaceControl
                       select i.Player;
            }
        }


        #endregion

        private void NewCharacter_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
