using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace OmegaMUD.Telnet
{
    class ANSIControlSequence
    {
        public char Operation { get; private set; }
        public List<int> Arguments { get; private set; }
        public string RawText { get; private set; }

        public ANSIControlSequence(string text)
        {
            RawText = text;
            Arguments = new List<int>();

            Operation = text[text.Length - 1];
            string argsString = text.Substring(2, text.Length - 3);
            string[] argsArray = argsString.Split(';');
            foreach (string argument in argsArray)
            {
                int arg;
                if (Int32.TryParse(argument, out arg))
                    Arguments.Add(arg);
            }
        }
    }

    class ANSIParser
    {
        public ANSIParser(string logpostfix)
        {
            this.logpostfix = logpostfix;
        }

        string logpostfix;
        readonly char ansiControlCharacter = (char)27;
        public readonly string ansiControlRegEx = (char)27 + @"\[" + "[^@-~]*" + "([@-~]|!)";
        private string leftoverText = null;

        //scans incoming text for ANSI control sequences, parses them, and returns a list of styled text runs
        public List<MUDToken> Translate(string text, MUDServerConnection connection, TelnetParser telnetParser, bool textGrouping)
        {
            if (!String.IsNullOrEmpty(text))
                File.AppendAllText("D:\\temp\\output-" + logpostfix + ".txt", "<text>" + text + "</text>");

            if (leftoverText != null)
            {
                text = leftoverText + text;
                leftoverText = null;
            }

            List<MUDToken> tokens = new List<MUDToken>();
            if (string.IsNullOrEmpty(text))
                return tokens;

            MUDToken currentToken = new MUDToken();
            int index = 0;
            MatchCollection matches = Regex.Matches(text, this.ansiControlRegEx);

            while (index < text.Length)
            {
                Match match = matches.OfType<Match>().SingleOrDefault(x => x.Index == index);
                if (match != null)
                {
                    if (currentToken.Content.Length > 0)
                    {
                        tokens.Add(currentToken);
                        currentToken = new MUDToken();
                    }

                    var control = new ANSIControlSequence(match.Value);
                    if (HandleANSIControl(control, currentToken, connection, telnetParser))
                    {
                        tokens.Add(currentToken);
                        currentToken = new MUDToken();
                    }

                    index += match.Length;
                }
                else
                {
                    if (text[index] == ansiControlCharacter)
                    {
                        // must have a truncated ansi control sequence, split by transmission boundaries.
                        // We have no choice but to wait for the next transmission.
                        leftoverText = text.Substring(index);
                        break;
                    }
                    else if (text[index] == '\r')
                    {
                        if (currentToken.Content.Length > 0)
                        {
                            tokens.Add(currentToken);
                            currentToken = new MUDToken();
                        }

                        currentToken.Content.Append("\r\n");
                        currentToken.TokenType = MUDTokenType.NewLine;
                        tokens.Add(currentToken);
                        currentToken = new MUDToken();

                        if (index == text.Length - 1)
                            break;
                        else
                            index++;
                    }
                    else if (text[index] == '\n' && index == 0)
                    {
                        // must have split up an "\r\n" along transmission boundaries, which means it was added at the last
                        // loop. Just ignore this one then.
                        index++;
                        continue;
                    }
                    else if (text[index] == '\b')
                    {
                        // backspace. Christ.
                        if (currentToken.Content.Length > 0)
                            currentToken.Content.Remove(currentToken.Content.Length - 1, 1);
                    }
                    else
                    {
                        currentToken.Content.Append(ReplaceSpecialCharacters(text[index]));
                    }

                    index++;

                }
            }

            if (currentToken.Content.Length > 0)
            {
                if (currentToken.TokenType == MUDTokenType.Text && textGrouping == true && currentToken.String != "(N)onstop, (Q)uit, or (C)ontinue?")
                {
                    // If "text grouping" is enabled then we don't submit any text blocks if they are last. We'll wait for the next non-text
                    // transmission to submit it.
                    if (leftoverText != null)
                    {
                        leftoverText = currentToken.String + leftoverText;
                    }
                    else
                    {
                        leftoverText = currentToken.String;
                    }
                }
                else
                {
                    tokens.Add(currentToken);
                }
            }

            var strings = from token in tokens
                          select String.Format("<{0}>{1}</{0}>", token.TokenType, token.TokenType == MUDTokenType.NewLine ? "\\r\\n" : token.String);
            File.AppendAllLines("D:\\temp\\tokens-" + logpostfix + ".txt", strings);

            return tokens;
        }

        private bool HandleANSIControl(ANSIControlSequence control, MUDToken token, MUDServerConnection connection, TelnetParser telnetParser)
        {
            bool handled = false;

            if (control.Operation == 'm')
            {
                token.TokenType = MUDTokenType.Color;
                handled = true;
            }
            else if (control.Operation == 'n')
            {
                // Query cursor position. Respond with an arbitrary value for now.
                // TODO: figure out if we really need to handle this better.
                if (control.Arguments.Count == 1 && control.Arguments[0] == 6)
                {
                    telnetParser.sendTelnetBytes(ASCIIEncoding.ASCII.GetBytes("\x1b[4;6R"));
                }
                handled = false;
            }
            else if (control.Operation == '!')
            {
                // seriously don't have a clue what this does.
                handled = false;
            }
            else if (control.Operation == 'J')
            {
                // clear text, line, screen. Either way, just ignore it.
                handled = false;
            }
            else if (control.Operation == 'K')
            {
                token.TokenType = MUDTokenType.EraseLine;
                handled = true;
            }
            else if (control.Operation == 'D')
            {
                token.TokenType = MUDTokenType.CursorBackward;
                handled = true;
            }
            else if (control.Operation == 'A' ||
                     control.Operation == 'B' ||
                     control.Operation == 'C' ||
                     control.Operation == 'H')
            {
                // cursor stuff, just ignore it.
                handled = false;
            }
            else
            {
                handled = false;
            }


            if (handled)
            {
                token.Content.Append(control.RawText);
                token.Arguments.AddRange(control.Arguments);
                return true;
            }

            return false;
        }

        

        private char ReplaceSpecialCharacters(char input)
        {
            switch (input)
            {
                //case '\b': return '☺';
                default: return input;
            }
        }

    }
}
