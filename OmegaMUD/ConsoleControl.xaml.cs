using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OmegaMUD
{
    /// <summary>
    /// Interaction logic for ConsoleControl.xaml
    /// </summary>
    public partial class ConsoleControl : UserControl
    {
        public ConsoleControl()
        {
            InitializeComponent();
        }

        public Action<string> CommandSender { get; set; }
        public Action<string> StatusUpdater { get; set; }

        private void InputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                DoEnter(e);
            }
        }

        void DoEnter(KeyEventArgs e)
        {
            if ((e.KeyboardDevice.Modifiers & ModifierKeys.Shift) != 0)
            {
                ReplaceInputSelection("\r\n");
            }
            else
            {
                try
                {
                    var commands = this.InputBox.Text.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                    foreach (var command in commands)
                    {
                        if (CommandSender != null)
                            CommandSender(command);
                    }
                }
                catch
                {
                    if (StatusUpdater != null)
                        StatusUpdater("Failed to send the command.  Your internet service may have been interrupted, or the server might have shut down.");
                }

                //clear text for next command entry
                this.InputBox.Clear();
            }
        }

        void ReplaceInputSelection(string replacement)
        {
            string left = InputBox.Text.Substring(0, InputBox.SelectionStart);
            string right = InputBox.Text.Substring(InputBox.SelectionStart + InputBox.SelectionLength);
            InputBox.Text = left + replacement + right;
            InputBox.Select(left.Length + replacement.Length, 0);
        }


        public void AddLine(params Run[] runs)
        {
            bool skipScroll = SkipScroll();
            MudParagraph newParagraph = new MudParagraph();
            newParagraph.Inlines.AddRange(runs);
            this.consoleFlowDocument.Blocks.Add(newParagraph);
            if (!skipScroll)
                ScrollToEnd();
        }

        public void AddRun(Run run)
        {
            bool skipScroll = SkipScroll();
            var lastParagraph = consoleFlowDocument.Blocks.LastBlock as MudParagraph;
            lastParagraph.Inlines.Add(run);
            if (!skipScroll)
                ScrollToEnd();
        }

        public MudParagraph LastParagraph()
        {
            return consoleFlowDocument.Blocks.LastBlock as MudParagraph;
        }

        public void AddNewParagraph()
        {
            bool skipScroll = SkipScroll();
            MudParagraph newParagraph = new MudParagraph();
            consoleFlowDocument.Blocks.Add(newParagraph);
            if (!skipScroll)
                ScrollToEnd();
        }

        public void ClearLastParagraph()
        {
            var lastParagraph = consoleFlowDocument.Blocks.LastBlock as MudParagraph;
            lastParagraph.Inlines.Clear();
        }

        private bool SkipScroll()
        {
            var viewer = Utilities.FindScrollViewerDescendant(this.consoleBox);
            if (viewer != null && viewer.ScrollableHeight - viewer.VerticalOffset == 0)
                return false;
            return true;
        }

        private void ScrollToEnd()
        {
            var descendantScrollViewer = Utilities.FindScrollViewerDescendant(this.consoleBox);
            if (descendantScrollViewer != null)
            {
                descendantScrollViewer.ScrollToEnd();
            }
        }
    }
}
