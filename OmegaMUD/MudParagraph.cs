using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using OmegaMUD.Telnet;
using System.Windows.Media;
using System.Text.RegularExpressions;

namespace OmegaMUD
{
    public class MudParagraph : Paragraph
    {
        public MudParagraph()
        {
            Margin = new System.Windows.Thickness(0);
        }

        HashSet<string> _matchedKeys = new HashSet<string>();


        public bool IsMatch(string str, string key)
        {
            return IsMatch((s) => s.Contains(str), key);
        }

        public bool IsMatch(Regex regex, string key)
        {
            return IsMatch((s) => regex.IsMatch(s), key);
        }

        private bool IsMatch(Func<string, bool> comparison, string key)
        {
            if (_matchedKeys.Contains(key))
                return false;
            if (comparison(LineString))
            {
                _matchedKeys.Add(key);
                return true;
            }
            return false;
        }

        public string LineString
        {
            get
            {
                StringBuilder builder = new StringBuilder();
                foreach (Run run in this.Inlines)
                {
                    builder.Append(run.Text);
                }
                return builder.ToString();
            }
        }

        //public static Color StartingColor(this Paragraph paragraph)
        //{
        //    if (paragraph.Inlines.Count == 0)
        //        return Colors.Black;
        //    return ((SolidColorBrush)(paragraph.Inlines.FirstInline as Run).Foreground).Color;
        //}



    }
}
