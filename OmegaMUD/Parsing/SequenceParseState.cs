using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OmegaMUD.Telnet;

namespace OmegaMUD.Parsing
{
    /// <summary>
    /// A class that represents a "string" of tokens in a row
    /// </summary>
    public class SequenceParseState : ParseState
    {
        Func<ParseState> success;
        Func<ParseState> fail;
        Func<MUDToken, Player, bool>[] checks;
        int index = 0;

        public SequenceParseState(
            Func<ParseState> success,
            Func<ParseState> fail,
            params Func<MUDToken, Player, bool>[] checks)
        {
            this.success = success;
            this.fail = fail;
            this.checks = checks;
            this.Execute = CheckSequence;
        }


        private ParseState CheckSequence(MUDToken token, Player player)
        {
            if (checks[index](token, player))
            {
                index++;
                if (index == checks.Length)
                    return success();
                return this;
            }
            return fail();
        }
    }
}
