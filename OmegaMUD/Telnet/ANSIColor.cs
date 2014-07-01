using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace OmegaMUD.Telnet
{
    public class AnsiColor
    {
        public bool Bright { get; set; }
        public AnsiColorCode ForegroundCode { get; set; }
        public AnsiColorCode BackgroundCode { get; set; }


        public void Update(MUDToken token)
        {
            if (token.TokenType != MUDTokenType.Color)
                throw new InvalidOperationException();

            foreach (int param in token.Arguments)
            {
                if (param == 0)
                {
                    Bright = false;
                    ForegroundCode = AnsiColorCode.White;
                    BackgroundCode = AnsiColorCode.Black;
                }

                if (param == 1)
                    Bright = true;
                else if (param == 22)
                    Bright = false;

                else if (param >= 30 && param <= 37)
                {
                    ForegroundCode = (AnsiColorCode)(param - 30);
                }
                else if (param >= 40 && param <= 47)
                {
                    BackgroundCode = (AnsiColorCode)(param - 40);
                }
                else if (param == 49)
                {
                    BackgroundCode = AnsiColorCode.Black;
                }
                else if (param == 39)
                {
                    ForegroundCode = AnsiColorCode.White;
                }
            }
        }

        public static Color GetAnsiColor(AnsiColorCode code, bool bright)
        {
            if (!bright)
            {
                switch (code)
                {
                    case AnsiColorCode.Black: return Color.FromRgb(0, 0, 0);
                    case AnsiColorCode.Red: return Color.FromRgb(128, 0, 0);
                    case AnsiColorCode.Green: return Color.FromRgb(0, 128, 0);
                    case AnsiColorCode.Yellow: return Color.FromRgb(128, 128, 0);
                    case AnsiColorCode.Blue: return Color.FromRgb(0, 0, 128);
                    case AnsiColorCode.Magenta: return Color.FromRgb(128, 0, 128);
                    case AnsiColorCode.Cyan: return Color.FromRgb(0, 128, 128);
                    case AnsiColorCode.White: return Color.FromRgb(192, 192, 192);
                    default: return Colors.White;
                }
            }
            else
            {
                switch (code)
                {
                    case AnsiColorCode.Black: return Color.FromRgb(128, 128, 128);
                    case AnsiColorCode.Red: return Color.FromRgb(255, 0, 0);
                    case AnsiColorCode.Green: return Color.FromRgb(0, 255, 0);
                    case AnsiColorCode.Yellow: return Color.FromRgb(255, 255, 0);
                    case AnsiColorCode.Blue: return Color.FromRgb(0, 0, 255);
                    case AnsiColorCode.Magenta: return Color.FromRgb(255, 0, 255);
                    case AnsiColorCode.Cyan: return Color.FromRgb(0, 255, 255);
                    case AnsiColorCode.White: return Color.FromRgb(255, 255, 255);
                    default: return Colors.White;
                }
            }
        }

        public Color ForegroundColor 
        {
            get { return GetAnsiColor(ForegroundCode, Bright); }
        }
        public Color BackgroundColor
        {
            get { return GetAnsiColor(BackgroundCode, false); }
        }
    }

    public enum AnsiColorCode
    {
        Black = 0,
        Red = 1,
        Green = 2,
        Yellow = 3,
        Blue = 4,
        Magenta = 5,
        Cyan = 6,
        White = 7
    }
}
