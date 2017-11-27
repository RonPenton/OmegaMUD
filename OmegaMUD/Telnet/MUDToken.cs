using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace OmegaMUD.Telnet
{
    public class MUDToken
    {
        public StringBuilder Content { get; private set; }
        public MUDTokenType TokenType { get; set; }
        public List<int> Arguments { get; private set; }
        public String String { get { return Content.ToString(); } }

        public MUDToken()
        {
            Content = new StringBuilder();
            Arguments = new List<int>();
        }


        public MUDToken(XElement element)
        {
            TokenType = (MUDTokenType)Enum.Parse(typeof(MUDTokenType), element.Name.LocalName);
            var value = element.Attribute("v");

            if( value == null )
                return;
            
            Content = new StringBuilder();
            if (TokenType != MUDTokenType.Text)
                Content.Append("\x1B");

            Content.Append(value.Value);
        }

        public override bool Equals(object obj)
        {
            MUDToken token = obj as MUDToken;
            if (token == null)
                return false;

            if (token.TokenType != TokenType)
                return false;

            if (token.Content != null && Content != null)
                return token.Content.ToString() == Content.ToString();

            return true;
        }
    }

    public enum MUDTokenType
    {
        Text,
        NewLine,
        Color,
        EraseLine,
        CursorBackward
    }

}
