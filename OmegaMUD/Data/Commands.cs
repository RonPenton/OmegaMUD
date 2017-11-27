using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OmegaMUD
{
    public enum Command
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
        Down = 9,
        Look = 10
    }

    public partial class MajorModelEntities
    {

        private Dictionary<Command, HashSet<string>> _commandSet;
        private Dictionary<Command, List<string>> _commandList;

        private void LoadCommands()
        {
            _commandSet = new Dictionary<Command, HashSet<string>>();
            _commandList = new Dictionary<Command, List<string>>();
            foreach (Command command in Enum.GetValues(typeof(Command)))
            {
                string key = "Command" + command.ToString();
                var values = this.Settings.Single(x => x.Key == key).Value.Split('|');
                _commandSet[command] = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
                _commandList[command] = new List<string>();
                foreach (var value in values)
                {
                    _commandSet[command].Add(value);
                    _commandList[command].Add(value);
                }
            }
        }

        public bool IsCommand(Command command, string token)
        {
            if (_commandSet == null)
                LoadCommands();
            return _commandSet[command].Contains(token);
        }

        /// <summary>
        /// Determines if the given token is a direction command. If so, it returns the direction. If not, it returns null.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public Direction? IsDirectionCommand(string token)
        {            
            if (_commandSet == null)
                LoadCommands();

            var directionCommands = Enum.GetValues(typeof(Command)).Cast<Command>().Where(x => x <= Command.Down).ToList();
            foreach (var command in directionCommands)
            {
                if (_commandSet[command].Contains(token))
                    return (Direction)(int)command;
            }

            return null;
        }

        public string GetCommand(Command command)
        {
            if (_commandSet == null)
                LoadCommands();

            return _commandList[command].First();
        }
    }
}
