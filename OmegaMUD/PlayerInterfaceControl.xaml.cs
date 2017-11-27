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
    /// Interaction logic for PlayerInterfaceControl.xaml
    /// </summary>
    public partial class PlayerInterfaceControl : UserControl
    {
        public Player Player { get; private set; }

        public PlayerInterfaceControl(string playerName)
        {
            InitializeComponent();
            Player = new Player(this);

            //TODO: For now simply give it the generic model.
            Player.Model = new MajorModelEntities();

            Thread thread = new System.Threading.Thread(new ThreadStart(() => Player.Model.PreCache()));
            thread.Start();

            OutputConsole.CommandSender = Player.SendCommand;
            OutputConsole.StatusUpdater = s => this.StatusText.Text = s;
            MapControl.Player = Player;
        }

        public void DebugText(string text)
        {
            Run run = new Run(text);
            run.Foreground = new SolidColorBrush(Colors.White);
            DebugConsole.AddLine(run);
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            if (Player.IsConnected)
            {
                Player.Disconnect();
            }
            else
            {
                Player.Connect();
            }
        }

        private void OptionsButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MoveButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
        }

    }
}
