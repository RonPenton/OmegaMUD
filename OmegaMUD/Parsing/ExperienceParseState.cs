using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OmegaMUD.Telnet;

namespace OmegaMUD.Parsing
{
    public class ExperienceParseState : ParseState
    {
        string line;

        public ExperienceParseState(MUDToken token, Player player)
        {
            line = token.String;
            Next(ExperienceState);
        }

        protected ParseState ExperienceState(MUDToken token, Player player)
        {
            if (token.TokenType == MUDTokenType.Text)
            {
                line += token.String;
                return this;
            }
            else if (token.TokenType == MUDTokenType.Color)
            {
                return this;
            }
            else if (token.TokenType == MUDTokenType.NewLine)
            {
                ParseExperience(player);
            }

            return new ParseState();
        }


        private void ParseExperience(Player player)
        {
            var match = player.Model.ExperienceRegex.Match(line);
            player.Experience = Int64.Parse(match.Groups["Experience"].Value);
            player.Level = Int32.Parse(match.Groups["Level"].Value);
            player.UpdateGameStatus(Commands.GameStatusUpdate.ExperienceParsed);
        }

    }
}
