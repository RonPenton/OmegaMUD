using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OmegaMUD
{
    public partial class Message
    {
        public IEnumerable<string> Messages()
        {
            if (!String.IsNullOrWhiteSpace(this.Line_1))
                yield return this.Line_1;
            if (!String.IsNullOrWhiteSpace(this.Line_2))
                yield return this.Line_2;
            if (!String.IsNullOrWhiteSpace(this.Line_3))
                yield return this.Line_3;
        }
    }
}
