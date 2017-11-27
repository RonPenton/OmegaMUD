using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OmegaMUD.Telnet;
using System.Text.RegularExpressions;

namespace OmegaMUD.Parsing
{
    public class RoomParseState : ParseState
    {
        private string roomName;
        private string description;
        private string items;
        private string people;
        private string exits;

        private Direction? followingDirection;


        public RoomParseState()
        {
            this.Execute = InitialState;
        }

        public RoomParseState(Direction? followingDirection)
        {
            this.Execute = InitialState;
            this.followingDirection = followingDirection;
        }

        // Room Parsing
        // Enter Game:  Back->Erase->Color->Text->Newline
        // Hit enter:   Back->Erase->Color->Text->Newline
        // Look:        Reset->Back->Erase->Color->Text->Newline
        // Move <dir>:  Reset->Back->Erase->Color->Text->Newline
        // Look <dir>:  Back->Erase->Color->Text->Newline

        #region State Processing

        protected ParseState InitialState(MUDToken token, Player player)
        {
            if (token.TokenType == MUDTokenType.Text)
            {
                roomName += token.Content.ToString();
                return this;
            }
            else if (token.TokenType == MUDTokenType.NewLine)
            {
                return Next(AfterName);
            }
            return new ParseState();
        }

        protected ParseState AfterName(MUDToken token, Player player)
        {
            if (token.TokenType == MUDTokenType.CursorBackward && token.Arguments[0] == 79)
            {
                return new SequenceParseState(
                    () => Next(Description),
                    () => new ParseState(),
                    (t, p) => t.TokenType == MUDTokenType.EraseLine,
                    (t, p) => t.TokenType == MUDTokenType.Color && t.String == player.Palette.RoomDescriptionColor);
            }
            else if (token.TokenType == MUDTokenType.Color && token.String == player.Palette.RoomItemsColor)
                return Next(Items);
            else if (token.TokenType == MUDTokenType.Color && token.String == player.Palette.RoomPeopleColor)
                return Next(People);
            else if (token.TokenType == MUDTokenType.Color && token.String == player.Palette.RoomExitsColor)
                return Next(Exits);
            return new ParseState();
        }

        protected ParseState Description(MUDToken token, Player player)
        {
            if (token.TokenType == MUDTokenType.Text)
            {
                description += token.String;
                return this;
            }
            else if (token.TokenType == MUDTokenType.NewLine)
            {
                description += " ";
                return this;
            }
            else if (token.TokenType == MUDTokenType.CursorBackward && token.Arguments[0] == 79)
            {
                // bank info
                return new SequenceParseState(
                    () => Next(AfterDescription),
                    () => new ParseState(),
                    (t, p) => t.TokenType == MUDTokenType.EraseLine,
                    (t, p) => t.TokenType == MUDTokenType.Color && t.String == p.Palette.RoomBankColor,
                    (t, p) => t.TokenType == MUDTokenType.NewLine,
                    (t, p) => t.TokenType == MUDTokenType.Text,
                    (t, p) => t.TokenType == MUDTokenType.NewLine,
                    (t, p) => t.TokenType == MUDTokenType.Text,
                    (t, p) => t.TokenType == MUDTokenType.NewLine,
                    (t, p) => t.TokenType == MUDTokenType.Text,
                    (t, p) => t.TokenType == MUDTokenType.NewLine,
                    (t, p) => t.TokenType == MUDTokenType.Text,
                    (t, p) => t.TokenType == MUDTokenType.NewLine,
                    (t, p) => t.TokenType == MUDTokenType.Text,
                    (t, p) => t.TokenType == MUDTokenType.NewLine);
            }
            else if (token.TokenType == MUDTokenType.Color)
            {
                return AfterDescription(token, player);
            }
            return new ParseState();
        }

        protected ParseState AfterDescription(MUDToken token, Player player)
        {
            if (token.TokenType == MUDTokenType.Color && token.String == player.Palette.RoomItemsColor)
                return Next(Items);
            else if (token.TokenType == MUDTokenType.Color && token.String == player.Palette.RoomPeopleColor)
                return Next(People);
            else if (token.TokenType == MUDTokenType.Color && token.String == player.Palette.RoomExitsColor)
                return Next(Exits);
            return new ParseState();
        }

        protected ParseState Items(MUDToken token, Player player)
        {
            if (token.TokenType == MUDTokenType.Text)
            {
                items += token.String;
                return this;
            }
            else if (token.TokenType == MUDTokenType.NewLine)
            {
                items += " ";
                return this;
            }
            else if (token.TokenType == MUDTokenType.Color)
            {
                if (token.TokenType == MUDTokenType.Color && token.String == player.Palette.RoomPeopleColor)
                    return Next(People);
                else if (token.TokenType == MUDTokenType.Color && token.String == player.Palette.RoomExitsColor)
                    return Next(Exits);
            }
            return new ParseState();
        }

        protected ParseState People(MUDToken token, Player player)
        {
            if (token.TokenType == MUDTokenType.Text)
            {
                people += token.String;

                if (token.String == ".")
                {
                    return new SequenceParseState(
                        () => Next(Exits),
                        () => new ParseState(),
                        (t, p) => t.TokenType == MUDTokenType.NewLine,
                        (t, p) => t.TokenType == MUDTokenType.Color && t.String == p.Palette.RoomPeopleClearColor,
                        (t, p) => t.TokenType == MUDTokenType.Color && t.String == p.Palette.RoomExitsColor);
                }
                return this;
            }
            else if (token.TokenType == MUDTokenType.NewLine)
            {
                people += " ";
                return this;
            }
            else if (token.TokenType == MUDTokenType.Color)
            {
                return this;
            }
            return new ParseState();
        }

        protected ParseState Exits(MUDToken token, Player player)
        {
            if (token.TokenType == MUDTokenType.Text)
            {
                exits += token.String;

                if (!token.String.EndsWith(","))
                {
                    ParseRoomInfo(player);
                    return new ParseState();
                }
                return this;
            }
            else if (token.TokenType == MUDTokenType.NewLine)
            {
                exits += " ";
                return this;
            }
            else if (token.TokenType == MUDTokenType.Color)
            {
                if (token.TokenType == MUDTokenType.Color && token.String == player.Palette.RoomPeopleColor)
                    return Next(People);
                else if (token.TokenType == MUDTokenType.Color && token.String == player.Palette.RoomExitsColor)
                    return Next(Exits);
            }
            return new ParseState();
        }

        #endregion


        #region Data Parsing

        private void ParseRoomInfo(Player player)
        {
            player.Interface.DebugText("Room Parsed: " + roomName + ", Last Command: " + player.LastConfirmedCommand);

            SeenRoom room = new SeenRoom();
            room.Name = roomName.Trim();

            if (description != null)
                room.Description = description.Trim();

            if (items != null)
            {
                var itemMatches = player.Model.RoomItemRegex.Matches(items);
                room.PopulateInventory(itemMatches, null, player.Model);
            }
            if (people != null)
            {
                var playerMatches = player.Model.RoomPeopleRegex.Matches(people);
                room.People.AddRange(playerMatches.Cast<Match>().Select(x => x.Groups["name"].Value));
            }

            var exitMatches = player.Model.RoomExitRegex.Matches(exits);
            room.Exits.AddRange(exitMatches.Cast<Match>().Select(x => x.Groups["name"].Value));

            UpdateRoomState(player, room);

            player.UpdateGameStatus(Commands.GameStatusUpdate.RoomParsed);
        }

        #endregion


        #region Room Detection

        private void UpdateRoomState(Player player, SeenRoom room)
        {
            RoomDisplayCommand command;
            Direction? direction;

            // parse the users last command to see if we can tell which direction the user is moving.
            ParseUserCommand(player, out command, out direction);
            if (followingDirection != null)
            {
                command = RoomDisplayCommand.Move;
                direction = followingDirection;
            }

            //if (command != RoomDisplayCommand.Look)
            //{
            //    player.UpdateItems(
            //}

            if (command == RoomDisplayCommand.Look)
            {
                Looking(player, room, direction);
            }
            else if( command == RoomDisplayCommand.Move || command == RoomDisplayCommand.Refresh || command == RoomDisplayCommand.Unknown)
            {
                MovedOrRefresh(player, room, command, direction);
            }
        }

        private void MovedOrRefresh(Player player, SeenRoom room, RoomDisplayCommand command, Direction? direction)
        {
            if (player.RoomDetectionState == RoomDetectionState.HaveMatch && command == RoomDisplayCommand.Move && direction != null)
            {
                // user is moving, verify that the new room we are in is in fact what we think it is.
                VerifyNewRoom(player.LastSeenRoom, direction.Value, room, player);
            }
            else
            {
                // first find a potential match for the room.
                FindPotentialRoomMatch(room, player);

                if (room.PotentialMatches.Count == 1)
                {
                    // we found a definite match, simply set the current room and be done with it.
                    SetCurrentRoom(RoomDetectionState.HaveMatch, room, player);
                }
                else if (room.PotentialMatches.Count == 0)
                {
                    // found a room that we have no idea where it is, so we're officially lost.
                    SetCurrentRoom(RoomDetectionState.NoClue, null, player);
                }
                else
                {
                    if (player.RoomDetectionState == RoomDetectionState.HaveMatches && direction != null)
                    {
                        player.LastSeenRoom.AdjacentRooms[(int)direction] = room;
                        SetCurrentRoom(RoomDetectionState.HaveMatches, room, player);
                        CalculateRoomWeb(player);
                    }
                    else if (command == RoomDisplayCommand.Refresh && player.RoomDetectionState == RoomDetectionState.HaveMatch && room.HasMatch(player.LastSeenRoom.Match.RoomNumber))
                    {
                        // player refreshed, so if we had a match don't overwrite it with an ambiguity.
                        return;
                    }
                    else if (player.RoomDetectionState == RoomDetectionState.HaveMatch && command == RoomDisplayCommand.Move && direction == null)
                    {
                        // We couldn't tell what direction the user moved, so look at the adjacent rooms and see if there's a single match.
                        var adjacentRooms = player.LastSeenRoom.Match.GetAdjacentRoomNumbers();

                        var narrowedList = room.PotentialMatches.Where(p => adjacentRooms.Any(a => p.RoomNumber == a));
                        if (narrowedList.Count() == 1)
                        {
                            room.PotentialMatches.RemoveAll(x => x.RoomNumber != narrowedList.First().RoomNumber);
                            // found a definite match, let's assume the user moved there.
                            SetCurrentRoom(RoomDetectionState.HaveMatch, room, player);
                            return;
                        }
                    }
                    else
                    {
                        SetCurrentRoom(RoomDetectionState.HaveMatches, room, player);
                    }
                }
            }
        }

        private void Looking(Player player, SeenRoom room, Direction? direction)
        {
            if (player.RoomDetectionState == RoomDetectionState.HaveMatches && direction != null)
            {
                // user is looking through an exit, and they aren't entirely sure where they are. 
                // So we'll try to use this information to come up with an idea, hopefully narrow down the choices.
                FindPotentialRoomMatch(room, player);

                // we're looking and we have a hazy idea of where we are, so add the room to the current web of data
                // and recalculate.
                player.LastSeenRoom.AdjacentRooms[(int)direction] = room;
                CalculateRoomWeb(player);
            }
        }

        /// <summary>
        /// Called when the user moves from one room to another. This simply verifies that the new room
        /// is what we expect it to be.
        /// </summary>
        /// <param name="lastSeenRoom"></param>
        /// <param name="movingDirection"></param>
        /// <param name="room"></param>
        private void VerifyNewRoom(SeenRoom lastSeenRoom, Direction movingDirection, SeenRoom room, Player player)
        {
            FindPotentialRoomMatch(room, player);
            if (room.PotentialMatches.Count == 1)
            {
                // only one match for the current room. Go ahead and set it.
                SetCurrentRoom(RoomDetectionState.HaveMatch, room, player);
                return;
            }
            if (room.PotentialMatches.Count == 0)
            {
                // no matches, not sure what to do.
                SetCurrentRoom(RoomDetectionState.NoClue, null, player);
                return;
            }

            // ambiguous match found, so attempt to figure out what room we're in by following the
            // exit that the user used.
            var exitData = lastSeenRoom.Match.GetExits().SingleOrDefault(x => x.Direction == movingDirection);
            Room matchedRoom = null;
            if (exitData != null)
            {
                matchedRoom = room.GetMatch(exitData.AdjacentRoomNumber);
            }

            if (matchedRoom == null)
            {
                // no match. Dunno what happened. User possibly teleported into an ambiguous room.
                SetCurrentRoom(RoomDetectionState.HaveMatches, room, player);
            }
            else
            {
                // we have a match. Remove all candidates that aren't the matched room
                room.PotentialMatches.RemoveAll(x => x != matchedRoom);
                SetCurrentRoom(RoomDetectionState.HaveMatch, room, player);
            }
        }

        private void CalculateRoomWeb(Player player)
        {
            // do a sanity check to make sure we need to do a calculation.
            if (player.SeenRoomAnchor == null || player.RoomDetectionState != RoomDetectionState.HaveMatches)
                return;

            // loop through the elimination cycle until there are no more eliminations to be made.
            while (EliminatePotentials(player.SeenRoomAnchor) == true) { }

            if (player.LastSeenRoom.PotentialMatches.Count == 0)
            {
                // something really messed up happened, so clean the slate.
                SetCurrentRoom(RoomDetectionState.NoClue, player.LastSeenRoom, player);
            }
            else if (player.LastSeenRoom.PotentialMatches.Count == 1)
            {
                // we found a match! Brilliant!
                SetCurrentRoom(RoomDetectionState.HaveMatch, player.LastSeenRoom, player);
            }
        }


        /// <summary>
        /// Eliminates potential rooms from a web of rooms. Returns true if potentials were eliminated, false otherwise.
        /// </summary>
        /// <param name="room"></param>
        /// <returns></returns>
        private bool EliminatePotentials(SeenRoom room)
        {
            bool eliminated = false;

            // eliminate potentials on the current room first.
            foreach (Direction direction in Enum.GetValues(typeof(Direction)))
            {
                var adjacent = room.AdjacentRooms[(int)direction];
                if (adjacent != null)
                {
                    var potentials = (from x in room.PotentialMatches
                                  let x1 = x.GetExit(direction).AdjacentRoomNumber
                                  where adjacent.PotentialMatches.Any(y => y.RoomNumber == x1)
                                  select x).ToList();

                    if (potentials.Count < room.PotentialMatches.Count)
                        eliminated = true;
                    room.PotentialMatches = potentials;
                }
            }

            // once the current room's potentials have been minimized according to local data, reduce the potentials
            // on all adjacent rooms based on what we already know.
            foreach (Direction direction in Enum.GetValues(typeof(Direction)))
            {
                var adjacent = room.AdjacentRooms[(int)direction];
                if (adjacent != null)
                {
                    var adjacentPotentials = (from x in room.PotentialMatches
                                              let x1 = x.GetExit(direction).AdjacentRoomNumber
                                              let y = adjacent.PotentialMatches.Single(z => z.RoomNumber == x1)
                                              select y).ToList();

                    if (adjacentPotentials.Count < adjacent.PotentialMatches.Count)
                        eliminated = true;
                    adjacent.PotentialMatches = adjacentPotentials;
                }
            }

            // Lastly, recurse through the adjacent rooms to repeat the process.
            foreach (Direction direction in Enum.GetValues(typeof(Direction)))
            {
                var adjacent = room.AdjacentRooms[(int)direction];
                if (adjacent != null)
                {
                    if (EliminatePotentials(adjacent))
                        eliminated = true;
                }
            }

            return eliminated;
        }

        private static bool IsInWeb(SeenRoom start, SeenRoom toCheck)
        {
            foreach (Direction direction in Enum.GetValues(typeof(Direction)))
            {
                var adjacent = start.AdjacentRooms[(int)direction];
                if (adjacent == toCheck)
                    return true;
                if (adjacent != null && IsInWeb(adjacent, toCheck))
                    return true;
            }

            return false;
        }

        private void FindPotentialRoomMatch(SeenRoom seenRoom, Player player)
        {
            seenRoom.PotentialMatches = (from room in player.Model.Rooms
                                         where room.Name == seenRoom.Name
                                         select room).ToList();
            if (seenRoom.PotentialMatches.Count <= 1)
                return;

            // check descriptions if available
            seenRoom.PotentialMatches = (from room in seenRoom.PotentialMatches
                                         where DoesRoomDescriptionMatch(room, seenRoom)
                                         select room).ToList();
            if (seenRoom.PotentialMatches.Count <= 1)
                return;

            // check exits
            seenRoom.PotentialMatches = (from room in seenRoom.PotentialMatches
                                         where room.DoExitsMatch(player, seenRoom.Exits)
                                         select room).ToList();

            // check NPC's, maybe bosses

            // check placed objects
        }

        private bool DoesRoomDescriptionMatch(Room candidate, SeenRoom seenRoom)
        {
            // no description, simply return all candidates as the user likely just has short mode on
            if (seenRoom.Description == null)
                return true;

            return Utilities.AssembleDescriptionString(candidate.Desc_0, candidate.Desc_1, candidate.Desc_2, candidate.Desc_3, candidate.Desc_4, candidate.Desc_5, candidate.Desc_6) ==
                seenRoom.Description.ToString();
        }

        private static void SetCurrentRoom(RoomDetectionState state, SeenRoom room, Player player)
        {
            if (state == RoomDetectionState.HaveMatches)
            {
                if (player.SeenRoomAnchor == null)
                {
                    // we had a hazy idea before, and the new room has done nothing to correct that notion.
                    // set the anchor point so a web can be calculated.
                    player.SeenRoomAnchor = room;
                }
                else if (!IsInWeb(player.SeenRoomAnchor, room))
                {
                    // The newly seen room isn't in the web, so we might have teleported or some other accident happened. Clear
                    // the anchor so we don't try running a calculation on old data.
                    player.SeenRoomAnchor = room;
                }
            }

            player.LastSeenRoom = room;
            player.RoomDetectionState = state;

            switch (state)
            {
                case RoomDetectionState.HaveMatch:
                    player.SeenRoomAnchor = null;
                    player.Interface.MapControl.SetRoom(room.Match);
                    player.Interface.DebugText("Room detected: " + room.PotentialMatches[0].Map_Number.ToString() + ":" + room.PotentialMatches[0].Room_Number.ToString());
                    break;
                case RoomDetectionState.HaveMatches:
                    player.Interface.DebugText("Potential Room Matches: " + room.PotentialMatches.Count().ToString());
                    break;
                case RoomDetectionState.NoClue:
                    player.SeenRoomAnchor = null;
                    player.Interface.DebugText("No room matches found!");
                    break;
            }
        }

        #endregion


        /// <summary>
        /// No light in the room, figure out if we're moving and in which direction.
        /// </summary>
        /// <param name="player"></param>
        public static void HandleDarkMovement(Player player)
        {
            RoomDisplayCommand command;
            Direction? direction;

            // parse the users last command to see if we can tell which direction the user is moving.
            ParseUserCommand(player, out command, out direction);

            if (command == RoomDisplayCommand.Move && direction != null && player.RoomDetectionState == RoomDetectionState.HaveMatch)
            {
                var exitData = player.LastSeenRoom.Match.GetExits().SingleOrDefault(x => x.Direction == direction);
                if (exitData != null)
                {
                    var roomNumber = exitData.AdjacentRoomNumber;
                    var room = player.Model.GetRoom(roomNumber);
                    SeenRoom seen = new SeenRoom();
                    seen.PotentialMatches = new List<Room>() { room };
                    SetCurrentRoom(RoomDetectionState.HaveMatch, seen, player);
                }
            }
        }

        public static void ParseUserCommand(Player player, out RoomDisplayCommand command, out Direction? direction)
        {
            command = RoomDisplayCommand.Unknown;
            direction = null;

            if (player.LastConfirmedCommand != null)
            {
                var tokens = player.LastConfirmedCommand.ToLowerInvariant().Split(new char[0], StringSplitOptions.RemoveEmptyEntries);

                if (tokens.Length > 1 && player.Model.IsCommand(Command.Look, tokens[0]))
                {
                    command = RoomDisplayCommand.Look;
                    direction = player.Model.IsDirectionCommand(tokens[1]);
                    player.Interface.DebugText("Player looking " + player.Model.PlainExitNames[(int)direction]);
                }
                else if (tokens.Length == 0 || (tokens.Length == 1 && player.Model.IsCommand(Command.Look, tokens[0])))
                {
                    command = RoomDisplayCommand.Refresh;
                    player.Interface.DebugText("Player refreshing room view.");
                }
                else if (tokens.Length > 0)
                {
                    command = RoomDisplayCommand.Move;
                    direction = player.Model.IsDirectionCommand(tokens[0]);

                    if (direction == null && player.RoomDetectionState == RoomDetectionState.HaveMatch)
                    {
                        var exits = (from e in player.LastSeenRoom.Match.GetExits()
                                     from me in player.Model.Messages
                                     from msg in me.Messages()
                                     where e.ExitType == ExitType.Text &&
                                           me.Number == e.Parameter1 &&
                                           msg.Equals(player.LastConfirmedCommand, StringComparison.InvariantCultureIgnoreCase)
                                     select e).ToList();
                        if (exits.Count == 1)
                        {
                            direction = exits[0].Direction;
                        }
                    }

                    if (direction != null)
                    {
                        player.Interface.DebugText("Player moving " + player.Model.PlainExitNames[(int)direction]);
                    }
                    else
                    {
                        player.Interface.DebugText("Player moved, but I don't know what direction.");
                    }
                }
            }
        }
    }

    public enum RoomDetectionState
    {
        NoClue,
        HaveMatches,
        HaveMatch
    }


}
