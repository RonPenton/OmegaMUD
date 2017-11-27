using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OmegaMUD.Telnet;
using OmegaMUD.Parsing;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows;
using System.Reflection;
using OmegaMUD.Commands;
using OmegaMUD.Data;

namespace OmegaMUD
{
    /// <summary>
    /// A class representing a player that Omega is controlling
    /// </summary>
    public partial class Player : IItemContainer
    {
        /// <summary>
        /// The current state of the parsing machine
        /// </summary>
        public ParseState CurrentParseState { get; set; }

        private string lastConfirmedCommand;
        /// <summary>
        /// The last command that the user entered that we have received back from MajorMUD.
        /// In theory this should be filled in with the currently-executing command at any
        /// given time.
        /// </summary>
        public string LastConfirmedCommand
        {
            get { return lastConfirmedCommand; }
            set
            {
                lastConfirmedCommand = value;
                Interface.DebugText("Player command returned: " + value);
            }
        }

        private bool isInMUD { get; set; }
        /// <summary>
        /// Whether the player is in majormud at the moment or not.
        /// </summary>
        public bool IsInMajorMUD
        {
            get
            {
                return isInMUD;
            }
            set
            {
                isInMUD = value;
                serverConnection.TextGrouping = isInMUD;
            }
        }

        /// <summary>
        /// The current palette that the user is using
        /// </summary>
        public MudPalette Palette { get; set; }

        /// <summary>
        /// The last command that the player set.
        /// </summary>
        public string LastCommand { get; set; }

        /// <summary>
        /// A queue of commands that the player has sent
        /// </summary>
        public Queue<string> SentCommands { get; private set; }

        /// <summary>
        /// The current color of the users screen.
        /// </summary>
        public AnsiColor CurrentColor { get; set; }

        /// <summary>
        /// The last room that the user has seen.
        /// </summary>
        public SeenRoom LastSeenRoom { get; set; }

        /// <summary>
        /// The first unknown room that the user found themselves in. We need to keep track of this
        /// because it is how room webs are calculated. Some rooms may have exits that lead to places
        /// that don't follow euclidian geometry; namely if you move south then move north you may not
        /// end up in the same place where you started from. This happens in the Labyrinth, and I'm sure
        /// other places as well. So the room anchor will simply be the first room where you got a hazy 
        /// match and the web will be calculated outwards from there.
        /// </summary>
        public SeenRoom SeenRoomAnchor { get; set; }

        /// <summary>
        /// The current status of room detection for the player.
        /// </summary>
        public RoomDetectionState RoomDetectionState { get; set; }

        /// <summary>
        /// The regular expression to verify the players statline
        /// </summary>
        public Regex StatlineRegex { get; set; }


        /// <summary>
        /// Constructs a player.
        /// </summary>
        public Player()
        {
        }

        /// <summary>
        /// Constructs a player.
        /// </summary>
        public Player(PlayerInterfaceControl playerInterface)
        {
            CurrentParseState = new ParseState();
            Palette = new MudPalette0();
            SentCommands = new Queue<string>();
            CurrentColor = new AnsiColor() { ForegroundCode = AnsiColorCode.White, BackgroundCode = AnsiColorCode.Black };
            StatlineRegex = new Regex("\\[HP=(?<hp>-?\\d+)/(?<mhp>\\d+),MA=(?<mp>\\d+)/(?<mmp>\\d+)]:( \\(Resting\\) )? ");
            Interface = playerInterface;
            this.Money = new Wallet();

            StartupMessage();
        }

        public PlayerInterfaceControl Interface { get; private set; }
        public MajorModelEntities Model { get; set; }

        public PathfindingSettings GetPathfindingSettings()
        {
            PathfindingSettings settings = new PathfindingSettings();
            settings.PartyCharacters.Add(this);
            return settings;
        }

        #region Statisticals
        
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public int Level { get; set; }
        public int Lives { get; set; }
        public int CharacterPoints { get; set; }
        public long Experience { get; set; }

        public int Strength { get; set; }
        public int Agility { get; set; }
        public int Intellect { get; set; }
        public int Health { get; set; }
        public int Willpower { get; set; }
        public int Charm { get; set; }

        public int ArmourClass { get; set; }
        public int DamageResistance { get; set; }
        public int Perception { get; set; }
        public int Spellcasting { get; set; }
        public int MagicResistance { get; set; }
        public int Stealth { get; set; }
        public int Thievery { get; set; }
        public int Traps { get; set; }
        public int Picklocks { get; set; }
        public int Tracking { get; set; }
        public int MartialArts { get; set; }

        public int HitPoints { get; set; }
        public int Power { get; set; }
        public int MaxHitPoints { get; set; }
        public int MaxPower { get; set; }

        public Class Class { get; set; }
        public Race Race { get; set; }
        public string RaceName
        {
            get
            {
                if (Race != null)
                    return Race.Name;
                return null;
            }
            set
            {
                Race = Model.Races.Single(x => x.Name == value);
            }
        }
        public string ClassName
        {
            get
            {
                if (Class != null)
                    return Class.Name;
                return null;
            }
            set
            {
                Class = Model.Classes.Single(x => x.Name == value);
            }
        }

        List<Item> _items = new List<Item>();
        public IEnumerable<Item> Items { get { return _items; } }
        public Wallet Money { get; private set; }
        public void AddItem(Item item) { _items.Add(item); }
        public void RemoveItem(Item item) { _items.Remove(item); }
        public void ClearInventory() { _items.Clear(); Money.Clear(); }
        public void EquipItem(string type, int? usesLeft, Item item) {  }
        public void UnequipItem(Item item) {  }

        public int MaxEncumbrance { get; set; }
        public int Encumbrance { get; set; }

        #endregion

        #region UI

        private void StartupMessage()
        {
            Run run = new Run("OmegaMUD Version: " + Assembly.GetExecutingAssembly().GetName().Version.ToString() + "\r\n");
            run.Foreground = new SolidColorBrush(Colors.White);
            run.Background = new SolidColorBrush(Colors.CornflowerBlue);
            Interface.OutputConsole.AddLine(run);
        }

        #endregion

        #region Communication

        private MUDServerConnection serverConnection;

        public void Connect()
        {
            string address = "192.168.1.102";
            int port = 23;

            try
            {
                Interface.StatusText.Text = "Attempting a connection to " + address + ", port " + port.ToString() + "...";
                this.serverConnection = new MUDServerConnection(address, port, PlayerName);
            }
            catch (Exception)
            {
                Interface.StatusText.Text = "Connection failed.  Please verify your internet connectivity and server information, then try again.";
                return;
            }

            Interface.StatusText.Text = "Connected.";
            this.serverConnection.serverMessage += new MUDServerConnection.serverMessageEventHandler(serverConnection_serverMessage);
            this.serverConnection.disconnected += new MUDServerConnection.disconnectionEventHandler(serverConnection_disconnected);
            this.Interface.ConnectButton.IsChecked = true;
        }

        void serverConnection_disconnected()
        {
            Interface.StatusText.Text = "Disconnected.";
            serverConnection = null;
            this.Interface.ConnectButton.IsChecked = false;
        }

        void serverConnection_serverMessage(List<MUDToken> tokens)
        {
            App.Current.Dispatcher.BeginInvoke(new Action(delegate
            {
                foreach (MUDToken token in tokens)
                {
                    ProcessToken(token);
                }
            }));
        }

        public bool IsConnected
        {
            get { return serverConnection != null; }
        }

        private void ProcessToken(MUDToken token)
        {
            if (token.TokenType == MUDTokenType.Text)
            {
                Run run = new Run(token.Content.ToString());
                run.Foreground = new SolidColorBrush(CurrentColor.ForegroundColor);
                run.Background = new SolidColorBrush(CurrentColor.BackgroundColor);
                this.Interface.OutputConsole.AddRun(run);

                // parse the line now that new text has been added to it. 
                LineParser.ParseLine(this.Interface.OutputConsole.LastParagraph(), this);
            }
            else if (token.TokenType == MUDTokenType.Color)
            {
                CurrentColor.Update(token);
            }
            else if (token.TokenType == MUDTokenType.NewLine)
            {
                this.Interface.OutputConsole.AddNewParagraph();
            }
            else if (token.TokenType == MUDTokenType.EraseLine)
            {
                this.Interface.OutputConsole.ClearLastParagraph();
            }
            else if (token.TokenType == MUDTokenType.CursorBackward)
            {
                // do nothing for now. Maybe forever.
            }

            CurrentParseState = CurrentParseState.Execute(token, this);
        }


        #endregion


        #region Output Commands

        private Queue<OutputCommand> _outputCommands = new Queue<OutputCommand>();
        public void UpdateGameStatus(GameStatusUpdate update)
        {
            if (_outputCommands.Count > 0)
            {
                string command = _outputCommands.First().GetOutput(this, update);
                if (command != null)
                    SendCommand(command);
                
                if (_outputCommands.First().IsDone)
                {
                    _outputCommands.Dequeue();

                    // If the command was null, send the status to the next command in the chain.
                    if (command == null)
                        UpdateGameStatus(update);
                }
            }
        }
        public void EnqueueCommands(IEnumerable<OutputCommand> commands)
        {
            foreach (var command in commands)
                _outputCommands.Enqueue(command);
        }

        #endregion


        public void SendCommand(string command)
        {
            if (command == ".")
                command = LastCommand;
            else
                LastCommand = command;
            this.serverConnection.SendText(command);
            SentCommands.Enqueue(command);
            if (SentCommands.Count > 30)
                SentCommands.Dequeue();
        }

        public void Disconnect()
        {
            if (this.serverConnection != null)
            {
                this.serverConnection.Disconnect();
                serverConnection = null;
            }
        }

        public bool IsEqual(Player player)
        {
            if (PlayerName == player.PlayerName && BBSName == player.BBSName)
                return true;
            return false;
        }

        public void SetDestination(RoomNumber roomNumber)
        {
            if (RoomDetectionState == RoomDetectionState.HaveMatch)
            {
                var path = Pathfinder.Pathfind(Model, LastSeenRoom.Match.RoomNumber, roomNumber, GetPathfindingSettings());
                if (path != null)
                {
                    this.Interface.DebugText(String.Format("Path found to {0}, {1} steps. Executing.", roomNumber, path.Count));
                    EnqueueCommands(path);
                    UpdateGameStatus(GameStatusUpdate.RoomParsed);
                }
            }
        }

    }
}
