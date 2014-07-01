using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OmegaMUD.Telnet;
using System.Reflection;

namespace OmegaMUD.Parsing
{
    public class StatParseState : ParseState
    {
        private string stats;

        public StatParseState()
        {
            Next(StatState);
        }

        protected ParseState StatState(MUDToken token, Player player)
        {
            if (token.TokenType == MUDTokenType.Color && token.String == "\x1B[0;37;40m")
            {
                ParseStats(player);
                return new ParseState();
            }
            else  if (token.TokenType == MUDTokenType.NewLine)
            {
                stats += " ";
                return this;
            }
            else if (token.TokenType == MUDTokenType.Text)
            {
                stats += token.String;
                return this;
            }
            else if (token.TokenType == MUDTokenType.Color)
            {
                return this;
            }

            return new ParseState();
        }

        private void ParseStats(Player player)
        {
            var groups = player.Model.StatRegex.MatchNamedCaptures(stats);

            Type type = typeof(Player);
            foreach (var group in groups)
            {
                if (!group.Value.Success)
                    continue;

                PropertyInfo pi = type.GetProperty(group.Key);
                if (pi.PropertyType == typeof(Int32))
                    pi.SetValue(player, Int32.Parse(group.Value.Value), null);
                else if (pi.PropertyType == typeof(Int64))
                    pi.SetValue(player, Int64.Parse(group.Value.Value), null);
                else
                    pi.SetValue(player, group.Value.Value, null);
            }

            player.UpdateGameStatus(Commands.GameStatusUpdate.StatsParsed);
        }
    }
}
