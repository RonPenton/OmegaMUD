using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OmegaMUD.Telnet;
using System.Text.RegularExpressions;

namespace OmegaMUD.Parsing
{
    public class InventoryParseState : ParseState
    {
        private string items;
        private string keys;
        private string weight;

        public InventoryParseState(MUDToken token, Player player)
        {
            items += token.String;
            Next(InventoryState);
        }

        protected ParseState InventoryState(MUDToken token, Player player)
        {
            if (token.TokenType == MUDTokenType.NewLine)
            {
                return this;
            }
            else if (token.TokenType == MUDTokenType.Text)
            {
                items += " " + token.String;
                return this;
            }
            else if (token.TokenType == MUDTokenType.CursorBackward && token.Arguments[0] == 79)
            {
                return new SequenceParseState(
                    () => Next(KeyState),
                    () => new ParseState(),
                    (t, p) => t.TokenType == MUDTokenType.EraseLine,
                    (t, p) => t.TokenType == MUDTokenType.Color && t.String == "\x1B[0;37;40m");
            }

            return new ParseState();
        }

        protected ParseState KeyState(MUDToken token, Player player)
        {
            if (token.TokenType == MUDTokenType.NewLine)
            {
                return this;
            }
            else if (token.TokenType == MUDTokenType.Text)
            {
                keys += " " + token.String;
                return this;
            }
            else if (token.TokenType == MUDTokenType.Color && token.String == "\x1b[0;32m")
            {
                return Next(WealthState);
            }

            return new ParseState();
        }

        protected ParseState WealthState(MUDToken token, Player player)
        {
            if (token.TokenType == MUDTokenType.Text)
            {
                return new SequenceParseState(
                    () => Next(EncumbranceState),
                    () => new ParseState(),
                    (t, p) => t.TokenType == MUDTokenType.Color,
                    (t, p) => t.TokenType == MUDTokenType.Text,
                    (t, p) => t.TokenType == MUDTokenType.NewLine,
                    (t, p) => t.TokenType == MUDTokenType.Color);
            }
            return new ParseState();
        }

        protected ParseState EncumbranceState(MUDToken token, Player player)
        {
            if (token.TokenType == MUDTokenType.Text)
            {
                weight += token.String;
                return this;
            }
            else if (token.TokenType == MUDTokenType.Color)
            {
                return this;
            }
            else if (token.TokenType == MUDTokenType.NewLine)
            {
                ParseInventory(player);
            }
            return new ParseState();
        }

        private void ParseInventory(Player player)
        {
            var itemMatches = player.Model.InventoryRegex.Matches(items);
            if (itemMatches[0].Groups["name"].Value == player.Model.InventoryNoItemString)
            {
                itemMatches = null;
            }

            MatchCollection keyMatches = null;
            if (!keys.Trim().Equals(player.Model.InventoryNoKeyString.Trim(), StringComparison.InvariantCultureIgnoreCase))
            {
                keyMatches = player.Model.InventoryKeyRegex.Matches(keys);
            }

            player.PopulateInventory(itemMatches, keyMatches, player.Model);

            var weightMatch = player.Model.InventoryWeightRegex.Match(weight);
            player.MaxEncumbrance = Int32.Parse(weightMatch.Groups["max"].Value);
            player.Encumbrance = Int32.Parse(weightMatch.Groups["current"].Value);

            player.UpdateGameStatus(Commands.GameStatusUpdate.InventoryParsed);
        }
    }
}
