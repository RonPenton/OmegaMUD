using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace OmegaMUD.Telnet
{
    public class MUDServerConnection
    {
        private TcpClient connection = new TcpClient();
        private byte[] buffer = new byte[8192];
        ANSIParser ansiParser;
        TelnetParser telnetParser;

        public bool TextGrouping { get; set; }

        public event disconnectionEventHandler disconnected;
        public delegate void disconnectionEventHandler();
        public event serverMessageEventHandler serverMessage;
        public delegate void serverMessageEventHandler(List<MUDToken> runs);


        public MUDServerConnection(string address, int port, string connectionName)
        {
            this.ansiParser = new ANSIParser(connectionName);
            this.connection.Connect(address, port);

            if (this.connection.Connected)
            {
                this.telnetParser = new TelnetParser(this.connection);
                connection.Client.BeginReceive(this.buffer, 0, this.buffer.Length, SocketFlags.None, new AsyncCallback(this.handleServerMessage), null);
            }
        }


        void handleServerMessage(IAsyncResult result)
        {
            int receivedCount;
            try
            {
                receivedCount = connection.Client.EndReceive(result);
            }
            catch
            {
                //if there was any issue reading the server text, ignore the message (what else can we do?)
                return;
            }

            //0 bytes received means the server disconnected
            if (receivedCount == 0)
            {
                this.Disconnect();
                return;
            }

            //list of bytes which aren't telnet sequences
            //ultimately, this will be the original buffer minus any telnet messages from the server
            List<byte> contentBytes = this.telnetParser.HandleAndRemoveTelnetBytes(this.buffer, receivedCount);

            //now we've filtered-out and responded accordingly to any telnet data.
            //next, convert the actual MUD content of the message from ASCII to Unicode
            string message = AsciiDecoder.AsciiToUnicode(contentBytes.ToArray(), contentBytes.Count);

            //run the following on the main thread so that calling code doesn't have to think about threading
            if (this.serverMessage != null)
            {
                List<MUDToken> runs = this.ansiParser.Translate(message, this, telnetParser, TextGrouping);
                this.serverMessage(runs);
            }

            //now that we're done with this message, listen for the next message
            connection.Client.BeginReceive(this.buffer, 0, this.buffer.Length, SocketFlags.None, new AsyncCallback(this.handleServerMessage), null);
        }

        #region outgoing text

        public void SendText(string text)
        {
            //if not connected, do nothing
            if (!this.connection.Connected) return;

            //add carriage return and line feed
            text = text + "\r\n";

            //convert from Unicode to ASCII
            Encoder encoder = System.Text.Encoding.ASCII.GetEncoder();
            char[] charArray = text.ToCharArray();
            int count = encoder.GetByteCount(charArray, 0, charArray.Length, true);
            byte[] outputBuffer = new byte[count];
            encoder.GetBytes(charArray, 0, charArray.Length, outputBuffer, 0, true);

            //send to server
            this.connection.Client.Send(outputBuffer);
        }

        #endregion

        #region disconnect

        internal void Disconnect()
        {
            //if not connected, do nothing
            if (!this.connection.Connected) return;

            //close the connection
            this.connection.Close();

            //initialize a new object
            this.connection = new TcpClient();

            //fire disconnection notification event on main UI thread
            if (this.disconnected != null)
            {
                App.Current.Dispatcher.BeginInvoke(new Action(delegate
                {
                    this.disconnected.Invoke();
                }));
            }
        }

        #endregion
    }
}
