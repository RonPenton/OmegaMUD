using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OmegaMUD.Telnet;
using System.Xml.Linq;

namespace OmegaMUD.Parsing
{
    public class TokenSequence
    {
        public List<MUDToken> Tokens { get; private set; }
        public TokenSequence(string tokenString)
        {
            Tokens = new List<MUDToken>();
            XDocument doc = XDocument.Parse("<xml>" + tokenString + "</xml>");

            foreach (XElement element in doc.Root.Elements())
            {
                Tokens.Add(new MUDToken(element));
            }
        }

        public bool Check(MUDToken token)
        {
            return Tokens[0].Equals(token);
        }

        public TokenSequenceParseState GetSequence(Func<ParseState> success, Func<ParseState> fail)
        {
            return new TokenSequenceParseState(success, fail, this);
        }
    }

    public class TokenSequenceParseState : ParseState
    {
        Func<ParseState> success;
        Func<ParseState> fail;
        int index = 1;
        TokenSequence sequence;

        public TokenSequenceParseState(
            Func<ParseState> success,
            Func<ParseState> fail,
            TokenSequence sequence)
        {
            this.success = success;
            this.fail = fail;
            this.sequence = sequence;
            this.Execute = CheckSequence;
        }

        private ParseState CheckSequence(MUDToken token, Player player)
        {
            if (token.Equals(sequence.Tokens[index]))
            {
                index++;
                if (index == sequence.Tokens.Count)
                    return success();
                return this;
            }
            return fail();
        }
    }
}
