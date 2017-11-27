using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OmegaMUD.Telnet
{
    enum Telnet : byte
    {
        //escape
        InterpretAsCommand = 255,

        //commands
        SubnegotiationEnd = 240,
        NoOperation = 241,
        DataMark = 242,
        Break = 243,
        InterruptProcess = 244,
        AbortOutput = 245,
        AreYouThere = 246,
        EraseCharacter = 247,
        EraseLine = 248,
        GoAhead = 249,
        SubnegotiationBegin = 250,

        //negotiation
        WILL = 251,
        WONT = 252,
        DO = 253,
        DONT = 254,

        //options (common)
        SuppressGoAhead = 3,
        Status = 5,
        Echo = 1,
        TimingMark = 6,
        TerminalType = 24,
        TerminalSpeed = 32,
        RemoteFlowControl = 33,
        LineMode = 34,
        EnvironmentVariables = 36,
        NAWS = 31,

        //options (MUD-specific)
        MSDP = 69,
        MXP = 91,
        MCCP1 = 85,
        MCCP2 = 86,
        MSP = 90
    };

    class TelnetParser
    {
        private System.Net.Sockets.TcpClient tcpClient;


        public TelnetParser(System.Net.Sockets.TcpClient tcpClient)
        {
            this.tcpClient = tcpClient;
        }


        public List<byte> HandleAndRemoveTelnetBytes(byte[] buffer, int receivedCount)
        {
            //list to hold any bytes which aren't telnet bytes (which will be most of the bytes)
            List<byte> contentBytes = new List<byte>();

            //we'll scan for telnet control sequences.  anything NOT a telnet control sequence will be added to the contentBytes list for later processing.
            int currentIndex = 0;
            while (currentIndex < receivedCount)
            {
                //search for an IAC, which may signal the beginning of a telnet message
                while (currentIndex < receivedCount && buffer[currentIndex] != (byte)Telnet.InterpretAsCommand)
                {
                    contentBytes.Add(buffer[currentIndex]);
                    currentIndex++;
                }

                //if at the end of the data, stop.  otherwise we've encountered an IAC and there should be at least one more byte here
                if (++currentIndex == receivedCount) break;

                //read the next byte
                byte secondByte = buffer[currentIndex];

                //if another IAC, then this was just sequence IAC IAC, which is the escape sequence to represent byte value 255 (=IAC) in the content stream
                if (secondByte == (byte)Telnet.InterpretAsCommand)
                {
                    //write byte value 255 to the content stream and move on
                    contentBytes.Add(secondByte);
                }

                //otherwise we have a "real" telnet sequence, where the second byte is a command or negotiation
                else
                {
                    //DO
                    if (secondByte == (byte)Telnet.DO ||
                        secondByte == (byte)Telnet.DONT ||
                        secondByte == (byte)Telnet.WILL ||
                        secondByte == (byte)Telnet.WONT)
                    {
                        //what are we being told to do?
                        currentIndex++;
                        if (currentIndex == receivedCount) break;
                        byte thirdByte = buffer[currentIndex];

                        if (secondByte == (byte)Telnet.WILL && thirdByte == (byte)Telnet.SuppressGoAhead)
                            this.sendTelnetBytes((byte)Telnet.DO, thirdByte);
                    }

                    //subnegotiations
                    else if (secondByte == (byte)Telnet.SubnegotiationBegin)
                    {
                        List<byte> subnegotiationBytes = new List<byte>();

                        //read until an IAC followed by an SE
                        while (currentIndex < receivedCount - 1 &&
                            !(buffer[currentIndex] == (byte)Telnet.InterpretAsCommand && buffer[currentIndex] == (byte)Telnet.SubnegotiationEnd))
                        {
                            subnegotiationBytes.Add(buffer[currentIndex]);
                            currentIndex++;
                        }

                        byte[] subnegotiationBytesArray = subnegotiationBytes.ToArray();

                        //append the content of the subnegotiation to the incoming message report string
                        AsciiDecoder.AsciiToUnicode(subnegotiationBytesArray, subnegotiationBytes.Count);
                    }
                }
                //move up to the next byte in the data
                currentIndex++;
            }

            return contentBytes;
        }


        public void sendTelnetBytes(params byte[] bytes)
        {
            //if not connected, do nothing
            if (!this.tcpClient.Connected) return;

            //send IAC
            this.tcpClient.Client.Send(new byte[] { (byte)Telnet.InterpretAsCommand });

            //send the specified bytes
            this.tcpClient.Client.Send(bytes);
        }
    }
}
