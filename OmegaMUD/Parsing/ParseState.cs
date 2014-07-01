using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OmegaMUD.Telnet;
using System.Text.RegularExpressions;

namespace OmegaMUD.Parsing
{
    //set statline full custom [HP=%h/%H,MA=%m/%M]:%r %B


    public delegate ParseState ParseFunction(MUDToken token, Player player);

    public class ParseState
    {
        /// <summary>
        /// The current parsing function.
        /// </summary>
        public ParseFunction Execute { get; protected set; }

        //bool reset = false;

        /// <summary>
        /// Constructs an empty parse state.
        /// </summary>
        public ParseState()
        {
            Execute = InitialState;
        }

        /// <summary>
        /// The initial state function which determines which path to follow.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private ParseState InitialState(MUDToken token, Player player)
        {
            if (!player.IsInMajorMUD)
            {
                if (player.Model.MudEnterSequence.Check(token))
                {
                    return player.Model.MudEnterSequence.GetSequence(
                        () => { player.IsInMajorMUD = true; player.Interface.DebugText("Game Entered."); return new ParseState(); },
                        () => new ParseState());
                }
                else
                {
                    return this;
                }
            }

            if (token.TokenType == MUDTokenType.Text && token.String.StartsWith( player.Model.ExitString))
            {
                player.IsInMajorMUD = false;
                player.LastConfirmedCommand = null;
                player.Interface.DebugText("Game Exited.");
                return new ParseState();
            }
            if (token.String == "\x1B[0;37;40m")
            {
                return Next(Reset);
            }
            else if (token.TokenType == MUDTokenType.CursorBackward && token.Arguments[0] == 79)
            {
                return Next(Backwards);
            }

            return this;
        }

        protected ParseState Reset(MUDToken token, Player player)
        {
            if (token.TokenType == MUDTokenType.CursorBackward && token.Arguments[0] == 79)
            {
                return Next(Backwards);
            }
            if (token.TokenType == MUDTokenType.Color && token.String == "\x1B[0;32m")
            {
                return Next(ResetGreen);
            }
            return new ParseState();
        }

        protected ParseState ResetGreen(MUDToken token, Player player)
        {
            if (token.TokenType == MUDTokenType.Text && token.String == player.Model.ExperienceStartString)
            {
                return new ExperienceParseState(token, player);
            }
            return new ParseState();
        }



        protected ParseState Backwards(MUDToken token, Player player)
        {
            if (token.TokenType == MUDTokenType.EraseLine)
                return Next(BackwardsErase);
            return new ParseState();
        }

        protected ParseState BackwardsErase(MUDToken token, Player player)
        {
            if (token.TokenType == MUDTokenType.Color && token.String == "\x1B[0;37m")
                return Next(White);
            else if (token.TokenType == MUDTokenType.Color && token.String == player.Palette.RoomNameColor)
                return new RoomParseState();
            else if (token.TokenType == MUDTokenType.Text && token.String == player.Model.DarkString)
            {
                RoomParseState.HandleDarkMovement(player);
                player.UpdateGameStatus(Commands.GameStatusUpdate.RoomParsed);
                return new ParseState();
            }
            else if (token.TokenType == MUDTokenType.Text && token.String.StartsWith(player.Model.InventoryString, StringComparison.InvariantCultureIgnoreCase))
            {
                return new InventoryParseState(token, player);
            }
            else if (token.TokenType == MUDTokenType.Color && token.String == "\x1B[0m")
                return Next(Zerom);

            return new ParseState();
        }

        protected ParseState Zerom(MUDToken token, Player player)
        {
            if (token.TokenType == MUDTokenType.Color && token.String == "\x1B[32m")
                return new StatParseState();
            return new ParseState();
        }

        protected ParseState White(MUDToken token, Player player)
        {
            if (token.TokenType == MUDTokenType.Text)
            {
                var match = player.StatlineRegex.Match(token.String);
                if (match.Success)
                {
                    player.Interface.DebugText("Statline Parsed: " + match.Value);
                    player.HitPoints = Int32.Parse(match.Groups["hp"].Value);
                    player.MaxHitPoints = Int32.Parse(match.Groups["mhp"].Value);
                    player.Power = Int32.Parse(match.Groups["mp"].Value);
                    player.MaxPower = Int32.Parse(match.Groups["mmp"].Value);

                    return new SequenceParseState(
                        () => Next(AfterStatus),
                        () => new ParseState(),
                        (t, p) => t.TokenType == MUDTokenType.Color && t.String == "\x1B[1m");
                }

                match = player.Model.FollowRegex.Match(token.String);
                if (match.Success)
                {
                    var direction = player.Model.IsDirectionCommand(match.Groups["dir"].Value);

                    return new SequenceParseState(
                        () => new RoomParseState(direction),
                        () => new ParseState(),
                        (t, p) => t.TokenType == MUDTokenType.NewLine,
                        (t, p) => t.TokenType == MUDTokenType.Color && t.String == "\x1B[0;37;40m",
                        (t, p) => t.TokenType == MUDTokenType.CursorBackward && t.Arguments[0] == 79,
                        (t, p) => t.TokenType == MUDTokenType.EraseLine,
                        (t, p) => t.TokenType == MUDTokenType.Color && t.String == player.Palette.RoomNameColor);
                }

                //match = player.Model.DragRegex.Match(token.String);
                //if( match.Success)
                //{
                //    return new SequenceParseState(
                //        () => new RoomParseState(),
                //        () => new ParseState(),
                //        (t, p) => t.TokenType == MUDTokenType.NewLine,
                //        (t, p) => t.TokenType == MUDTokenType.CursorBackward && t.Arguments[0] == 79,
                //        (t, p) => t.TokenType == MUDTokenType.EraseLine,
                //        (t, p) => t.TokenType == MUDTokenType.Color && t.String == player.Palette.RoomNameColor);
                //}


            }
            return new ParseState();
        }

        protected ParseState AfterStatus(MUDToken token, Player player)
        {
            if (token.TokenType == MUDTokenType.Text && player.SentCommands.Contains(token.String))
            {
                return new SequenceParseState(
                    () => { player.LastConfirmedCommand = token.String; return new ParseState(); },
                    () => new ParseState(),
                    (t, p) => t.TokenType == MUDTokenType.NewLine);
            }
            else if (token.TokenType == MUDTokenType.NewLine && player.SentCommands.Contains(""))
            {
                player.LastConfirmedCommand = "";
                return new ParseState();
            }
            else if (token.TokenType == MUDTokenType.CursorBackward && token.Arguments[0] == 79)
            {
                return Next(AfterStatusBack);
                //return new SequenceParseState(
                //    () => Next(AfterStatus),
                //    () => new ParseState(),
                //    (t, p) => t.TokenType == MUDTokenType.EraseLine,
                //    (t, p) => t.TokenType == MUDTokenType.Text,
                //    (t, p) => t.TokenType == MUDTokenType.NewLine);
            }
            return new ParseState();
        }

        protected ParseState AfterStatusBack(MUDToken token, Player player)
        {
            if (token.TokenType == MUDTokenType.EraseLine)
                return Next(AfterStatusErase);
            return Backwards(token, player);
        }

        protected ParseState AfterStatusErase(MUDToken token, Player player)
        {
            if (token.TokenType == MUDTokenType.Text)
                return Next(AfterStatusText);
            return BackwardsErase(token, player);
        }

        protected ParseState AfterStatusText(MUDToken token, Player player)
        {
            if (token.TokenType == MUDTokenType.NewLine)
                return Next(AfterStatus);
            return new ParseState();
        }


        protected ParseState Next(ParseFunction next)
        {
            Execute = next;
            return this;
        }
    }
}
