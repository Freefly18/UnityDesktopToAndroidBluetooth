// Based on sample code from
// 32feet.NET - Personal Area Networking for .NET
// By In The Hand Ltd. & Alan J. McFarlane.

using System;
using System.IO;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using InTheHand.Net;
using InTheHand.Net.Sockets;
using InTheHand.Net.Bluetooth;

namespace BluetoothToTCP
{
    public class BTTransmitterSingleton
    {
        private static BTTransmitterSingleton instance;
        private BTTransmitterSingleton()
        {

        }
        ~BTTransmitterSingleton()
        {

        }
        public static BTTransmitterSingleton Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new BTTransmitterSingleton();
                }
                return instance;
            }
        }

        /************************************************************/

        public string connectionState = "Offline";
        public delegate void GetData(string data);
        public event GetData getDataDelegate;

        readonly Guid OurServiceClassId = new Guid("{f8a8bae3-3eba-493f-89e9-c221964b449b}");
        readonly string OurServiceName = "BluetoothToTCP";
        //
        volatile bool _closing;
        TextWriter _connWtr;
        BluetoothListener _lsnr;

        public void StartBluetooth()
        {
            //Socket s = new Socket(AddressFamily32.Bluetooth, SocketType.Stream, BluetoothProtocolType.RFComm);

            connectionState = "Starting Bluetooth Transmitter";
            try
            {
                new BluetoothClient();
            }
            catch (Exception ex)
            {
                connectionState = "Bluetooth init failed: " + ex;
                Console.WriteLine(connectionState);
                //Debug.LogError(msg);
                //throw new InvalidOperationException(connectionState, ex);
                return;
            }
            StartListener();
        }

        public bool Send(string message)
        {
            if (_connWtr == null)
            {
                connectionState = "No connection.";
                return false;
            }
            try
            {
                _connWtr.WriteLine(message);
                _connWtr.Flush();
                return true;
            }
            catch (Exception ex)
            {
                connectionState = "Connection lost! (" + ex.Message + ")";
                ConnectionCleanup();
                return false;
            }
        }



        private void StartListener()
        {
            var lsnr = new BluetoothListener(OurServiceClassId);
            lsnr.ServiceName = OurServiceName;
            lsnr.Start();
            _lsnr = lsnr;
            ThreadPool.QueueUserWorkItem(ListenerAccept_Runner, lsnr);
        }

        void ListenerAccept_Runner(object state)
        {
            var lsnr = (BluetoothListener)_lsnr;
            connectionState = "Listening for client";
            while (true)
            {
                var conn = lsnr.AcceptBluetoothClient();
                var peer = conn.GetStream();
                SetConnection(peer, false, conn.RemoteEndPoint);
                ReadMessagesToEnd(peer);
            }
        }

        private void SetConnection(Stream peerStream, bool outbound, BluetoothEndPoint remoteEndPoint)
        {
            if (_connWtr != null)
            {
                Console.WriteLine("Already Connected!");
                //Debug.Log("Already Connected!");
                return;
            }
            _closing = false;
            var connWtr = new StreamWriter(peerStream);
            connWtr.NewLine = "\0";
            _connWtr = connWtr;
            connectionState = (outbound ? "Connected to " : "Connection from ") + remoteEndPoint.Address;
            //Debug.Log((outbound ? "Connected to " : "Connection from ") + remoteEndPoint.Address);
        }

        private void ReadMessagesToEnd(Stream peer)
        {
            var rdr = new StreamReader(peer);
            string lines = "";
            while (true)
            {
                string line;
                try
                {
                    line = rdr.ReadLine();
                }
                catch (IOException ioex)
                {
                    if (_closing)
                    {
                        // Ignore the error that occurs when we're in a Read
                        // and _we_ close the connection.
                    }
                    else
                    {
                        connectionState = "Connection was closed hard (read).  " + ioex.Message;
                        //Debug.Log("Connection was closed hard (read).  " + ioex.Message);
                    }
                    break;
                }
                if (line == null)
                {
                    connectionState = "Connection was closed (read).";
                    //Debug.Log("Connection was closed (read).");
                    break;
                }
                //Console.WriteLine(line);
                //Debug.Log(line);

                lines += line;
                if (lines.EndsWith("\0"))
                {
                    string data = lines.Substring(0, lines.Length - 1);
                    lines = "";
                    if (getDataDelegate != null)
                        getDataDelegate(data);
                }
                else
                {
                    lines += "\r\n";
                }
            }
            ConnectionCleanup();
        }

        private void ConnectionCleanup()
        {
            _closing = true;
            var wtr = _connWtr;
            //_connStrm = null;
            _connWtr = null;
            if (wtr != null)
            {
                try
                {
                    wtr.Close();
                }
                catch (Exception)
                {
                    connectionState = "Problem while cleaning up";
                    //Debug.Log("ConnectionCleanup close ex: " + ex.Message);
                }
            }
        }
    }
}
