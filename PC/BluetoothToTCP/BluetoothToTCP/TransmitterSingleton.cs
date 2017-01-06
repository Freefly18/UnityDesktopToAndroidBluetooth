using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Dolphins.Salaam;
// test
//using UnityEngine;

namespace ServerClient
{
    class TransmitterSingleton
    {
        private static TransmitterSingleton instance;
        private TransmitterSingleton()
        {
			// test
			/*var x = Dns.GetHostEntry(Dns.GetHostName());
			foreach (var y in x.AddressList)
			{
				//Debug.Log(y.ToString());
			}*/

            // Set the TcpListener on port 13000.
            port = 13000;
			//localAddr = Dns.GetHostEntry(Dns.GetHostName()).AddressList[0];
			TcpClientList = new List<TcpClient>();
        }
        ~TransmitterSingleton()
        {
            stopServer();
            clientStopListening();
        }
        public static TransmitterSingleton Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new TransmitterSingleton();
                }
                return instance;
            }
        }

        /************************************************************/

		public delegate void GetData(string data);
		public GetData serverGetsDataDelegate;
		public GetData clientGetsDataDelegate;
		public bool mustExitThreads = false;
		public SalaamBrowser salaamBrowser;
		public string connectionState = "Offline";

        private Int32 port;
        //private IPAddress localAddr;
		private IPAddress serverAddr;
        private TcpListener tcpListener;
        private Thread listenThread;
        private Thread serverListenThread;
        private bool serverIsRunning = false;
        private List<TcpClient> TcpClientList;
        private TcpClient client;
		private SalaamService salaamService;
		private string clientMessage = "";
		private IPEndPoint serverEndPoint;
		private int nbThreadRunning = 0;
		private int nbRCLThreadRunning = 0;

		// Restart data
		private Thread restartThread;
		private GetData restartGetsDataDelegate;
		private string restartIPText;
		private string restartServiceType;
		private bool restartIsRunning = false;

        public void sendDataToServer(string data)
        {
			// test
			////Debug.Log("sendDataToServer");

            if (client != null && client.Client != null && client.Connected)
            {
				try
				{
					NetworkStream clientStream = client.GetStream();
					if (clientStream == null)
					{
						//Debug.Log("Client Stream is null even though the client is connected");
						connectionState = "Could not send data: Bad Client Stream";
						return;
					}
					ASCIIEncoding encoder = new ASCIIEncoding();
					byte[] buffer = encoder.GetBytes(data + "\0");
					clientStream.Write(buffer, 0, buffer.Length);
					clientStream.Flush();
					//clientStream.Close();
				}
				catch (SocketException e)
				{
					//Debug.Log(e.Message);
				}
				catch (IOException e)
				{
					//Debug.Log(e.Message);
				}
			}
			else
			{
				connectionState = "Could not send data: no client connected";
				if (serverEndPoint != null)
				{
					if (nbRCLThreadRunning == 0)
					{
						Thread aThread = new Thread(new ThreadStart(RetryConnectionLater));
						aThread.Name = "Client: RetryConnectionLater";
						aThread.Start();
					}
				}
			}
		}
		
		public void sendDataToClient(string data)
		{
			for (int i=TcpClientList.Count-1; i>=0; i--)
			{
                if (!TcpClientList[i].Connected)
                {
                    TcpClientList.RemoveAt(i);
                }
            }

            foreach (TcpClient client in TcpClientList)
            {
				try
				{
	                NetworkStream clientStream = client.GetStream();
	                ASCIIEncoding encoder = new ASCIIEncoding();
	                byte[] buffer = encoder.GetBytes(data + "\0");
	                clientStream.Write(buffer, 0, buffer.Length);
	                clientStream.Flush();
	                //clientStream.Close();
				}
				catch (SocketException ex)
				{
					//Debug.Log("Could not send data to client: " + ex.Message);
				}
            }
        }

		public void startServer(GetData getDataCallback, string serviceType = "_test._tcp", string name = "My Application")
        {
            if (!serverIsRunning)
            {
				connectionState = "Starting Salaam Service";

                mustExitThreads = false;
                serverGetsDataDelegate = getDataCallback;

				salaamService = new SalaamService(serviceType, name, 2000);
				//salaamService.Message = "NotReady;";
				salaamService.Message = "NotReady";
				//salaamService.BroadcastAddress = localAddr;
				salaamService.Register();

				try
				{
					//connectionState = "Server is starting at " + localAddr.ToString() + ":" + port.ToString();
					connectionState = "Server is starting on port " + port.ToString();
					// test
					//Debug.Log("Server is starting on port " + port.ToString());

					tcpListener = new TcpListener(IPAddress.Any, port);
					listenThread = new Thread(new ThreadStart(ListenForClients));
					listenThread.Name = "Server: ListenForClients";
					listenThread.Start();
				}
				catch (SocketException ex)
				{
					connectionState = "Server failed to start";
					// test
					//Debug.Log(ex.ToString());
					
					Console.WriteLine("SocketException: {0}", ex);
				}
            }
        }
		
		public void stopServer()
		{
			if (serverIsRunning)
			{
				//listenThread.Abort();
				mustExitThreads = true;
				foreach (TcpClient client in TcpClientList)
				{
					client.Close();
				}
				tcpListener.Server.Close();
				salaamService.Unregister();
				serverIsRunning = false;
				connectionState = "Server stopped";
			}
		}

		public void clientStartListening(GetData getDataCallback, string serviceType = "_test._tcp")
		{
			// test
			//Debug.Log("clientStartListening");

            if (client == null || client.Client == null || !client.Connected)
            {
				clientGetsDataDelegate = getDataCallback;
				connectionState = "Waiting for server...";

				salaamBrowser = new SalaamBrowser();
				salaamBrowser.ReceiveFromLocalMachine = true;
				salaamBrowser.Start(serviceType);
				salaamBrowser.ClientMessageChanged += delegate(object sender, SalaamClientEventArgs e)
				{
					// test
					////Debug.Log("Server message has changed: " + e.Client.Message + " (" + e.Client.Address.ToString() + ")");

					clientMessage = e.Client.Message;

					// test
					//Debug.Log("Server message: " + clientMessage);

					if (clientMessage == "Ready" && (client == null || client.Client == null || !client.Connected))
					{
						connectionState = "Server is ready, connecting...";
						// test
						//Debug.Log("Server is ready, connecting...");

						mustExitThreads = false;
						client = new TcpClient();
						serverAddr = e.Client.Address;
						serverEndPoint = new IPEndPoint(e.Client.Address, port);
						try
						{
							client.Connect(serverEndPoint);
						}
						catch (SocketException ex)
						{
							// test
							//Debug.Log(ex.ToString());
							connectionState = ex.Message + " (" + serverEndPoint.ToString() + ")";
							
							if (nbRCLThreadRunning == 0)
							{
								Thread aThread = new Thread(new ThreadStart(RetryConnectionLater));
								aThread.Name = "Client: RetryConnectionLater";
								aThread.Start();
							}
							
							return;
						}
						
						try
						{
							serverListenThread = new Thread(new ThreadStart(ListenForServer));
							serverListenThread.Name = "Client: ListenForServer";
							serverListenThread.Start();
							connectionState = "Server online at " + e.Client.Address.ToString();
						}
						catch (SocketException ex)
						{
							//Debug.Log(ex.ToString());
						}
					}
				};
				salaamBrowser.ClientAppeared += delegate(object sender, SalaamClientEventArgs e)
				{
					// test
					////Debug.Log("salaamBrowser.ClientAppeared : " + e.Client.Message + " (" + e.Client.Address.ToString() + ")");

					clientMessage = e.Client.Message;

					connectionState = "Server found at " + e.Client.Address.ToString();

					// test
					//Debug.Log("Server message: " + clientMessage);
					
					if (clientMessage == "Ready" && (client == null || client.Client == null || !client.Connected))
					{
						connectionState = "Server is ready, connecting...";
						// test
						//Debug.Log("Server is ready, connecting...");

						mustExitThreads = false;
						client = new TcpClient();
						serverAddr = e.Client.Address;
						serverEndPoint = new IPEndPoint(e.Client.Address, port);
						try
						{
							client.Connect(serverEndPoint);
						}
						catch (SocketException ex)
						{
							// test
							//Debug.Log(ex.ToString());
							connectionState = ex.Message + " (" + serverEndPoint.ToString() + ")";
							
							if (nbRCLThreadRunning == 0)
							{
								Thread aThread = new Thread(new ThreadStart(RetryConnectionLater));
								aThread.Name = "Client: RetryConnectionLater";
								aThread.Start();
							}
							
							return;
						}

						try
						{
							serverListenThread = new Thread(new ThreadStart(ListenForServer));
							serverListenThread.Name = "Client: ListenForServer";
							serverListenThread.Start();
							connectionState = "Server online at " + e.Client.Address.ToString();
						}
						catch (SocketException ex)
						{
							//Debug.Log(ex.ToString());
						}
					}
				};
				salaamBrowser.ClientDisappeared += delegate(object sender, SalaamClientEventArgs e)
				{
					if (serverAddr == null || !e.Client.Address.Equals(serverAddr))
					{
						// If there was no server IP or if we lost Salaam connection to a different IP,
						// do nothing
						return;
					}

					if (client != null)
					{
						connectionState = "Server is now offline";
						// test
						//Debug.Log("Server has disappeared");

						mustExitThreads = true;
						client.Close();
					}
				};
			}
        }

		// Assumes that ipText is a valid IP Address
		public void clientStartListeningAtIP(GetData getDataCallback, string ipText)
		{
			//Debug.Log("clientStartListening (custom IP)");
			
			if (client == null || client.Client == null || !client.Connected)
			{
				connectionState = "Trying to connect at " + ipText;
				// test
				//Debug.Log("Trying to connect at " + ipText);
				
				mustExitThreads = false;
				client = new TcpClient();
				serverAddr = IPAddress.Parse(ipText);
				serverEndPoint = new IPEndPoint(serverAddr, port);
				try
				{
					client.Connect(serverEndPoint);
				}
				catch (SocketException ex)
				{
					// test
					//Debug.Log(ex.ToString());
					connectionState = "Could not reach specified IP Address";

					/*if (nbRCLThreadRunning == 0)
					{
						Thread aThread = new Thread(new ThreadStart(RetryConnectionLater));
						aThread.Name = "Client: RetryConnectionLater";
						aThread.Start();
					}*/
					
					return;
				}
				
				try
				{
					serverListenThread = new Thread(new ThreadStart(ListenForServer));
					serverListenThread.Name = "Client: ListenForServer";
					serverListenThread.Start();
					connectionState = "Server online at " + serverAddr.ToString();
				}
				catch (SocketException ex)
				{
					//Debug.Log(ex.ToString());
				}
			}
		}

        public void clientStopListening()
		{
			if (restartThread != null)
			{
				restartThread.Abort();
				restartIsRunning = false;
			}

            if (client != null)
            {
                //serverListenThread.Abort();
                mustExitThreads = true;
                client.Close();
                //client = null;
            }
			if (salaamBrowser != null)
			{
				salaamBrowser.Stop();
			}

			connectionState = "Offline";
        }

		public bool isValidIPAddress(string ipText)
		{
			IPAddress add;
			return IPAddress.TryParse(ipText, out add);
		}

		// Returns false if we are already restarting
		public bool restartClientListening(GetData getDataCallback, string ipText = "", string serviceType = "_test._tcp")
		{
			if (restartIsRunning)
			{
				return false;
			}
			
			clientStopListening();

			restartGetsDataDelegate = getDataCallback;
			restartIPText = ipText;
			restartServiceType = serviceType;

			restartThread = new Thread(new ThreadStart(RestartAfterEnd));
			restartThread.Name = "Client: RestartAfterEnd";
			restartThread.Start();
			connectionState = "Waiting for last connection attempt to end (" + nbThreadRunning.ToString() + ")";

			return true;
		}





		// Waits for all past threads to end (may take up to a second after we stop listening)
		// then starts client listening
		private void RestartAfterEnd()
		{
			restartIsRunning = true;

			while (nbThreadRunning > 0)
			{
				Thread.Sleep(100);
			}

			restartIsRunning = false;

			if (restartIPText == "")
			{
				clientStartListening(restartGetsDataDelegate, restartServiceType);
			}
			else
			{
				clientStartListeningAtIP(restartGetsDataDelegate, restartIPText);
			}
		}

		private void RetryConnectionLater()
		{
			nbRCLThreadRunning++;
			Thread.Sleep(1000);

			if (mustExitThreads)
			{
				nbRCLThreadRunning--;
				return;
			}

			if (client == null || client.Client == null || !client.Connected)
			{
				connectionState = "Retrying connection...";
				// test
				//Debug.Log("Retrying connection...");
				
				mustExitThreads = false;
				client = new TcpClient();
				try
				{
					// If serverEndPoint is unreachable, client.Connect may get stuck
					// During a Connect, we pretend the thread is not running
					// That way, other methods can create new instances of the thread
					nbRCLThreadRunning--;
					client.Connect(serverEndPoint);
					nbRCLThreadRunning++;
				}
				catch (SocketException ex)
				{
					// test
					//Debug.Log(ex.ToString());
					connectionState = ex.Message + " (" + serverEndPoint.ToString() + ")";

					Thread aThread = new Thread(new ThreadStart(RetryConnectionLater));
					aThread.Name = "Client: RetryConnectionLater";
					aThread.Start();

					nbRCLThreadRunning--;
					return;
				}
				
				try
				{
					serverListenThread = new Thread(new ThreadStart(ListenForServer));
					serverListenThread.Name = "Client: ListenForServer";
					serverListenThread.Start();
					connectionState = "Server online at " + serverEndPoint.ToString();
				}
				catch (SocketException ex)
				{
					//Debug.Log(ex.ToString());
				}
			}

			nbRCLThreadRunning--;
		}
		
		private void ListenForClients()
        {
			nbThreadRunning++;
            tcpListener.Start();
            serverIsRunning = true;
			connectionState = "Server running and ready";
			//salaamService.Message = "Ready;" + localAddr.ToString();
			salaamService.Message = "Ready";

			// test
			//Debug.Log("serverIsRunning==true");

            while (true && !mustExitThreads)
            {
                try
                {
                    //blocks until a client has connected to the server
                    TcpClient client = this.tcpListener.AcceptTcpClient();

                    //create a thread to handle communication 
                    //with connected client
                    Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClientComm));
                    clientThread.Name = "Server: HandleClientComm";
                    clientThread.Start(client);
                    TcpClientList.Add(client);
                }
                catch
                {
                    //a socket error has occured
					connectionState = "Error while handling a client";
                    break;
                }
            }

            this.tcpListener.Stop();
			nbThreadRunning--;
        }

        private void HandleClientComm(object client)
        {
			nbThreadRunning++;
            TcpClient tcpClient = (TcpClient)client;
            NetworkStream clientStream = tcpClient.GetStream();

            byte[] message = new byte[4096];
            string data = "";
            int bytesRead;

            while (true && !mustExitThreads)
            {
                bytesRead = 0;

                try
                {
                    //blocks until a client sends a message
                    bytesRead = clientStream.Read(message, 0, 4096);
                }
                catch
                {
                    //a socket error has occured
                    break;
                }

                if (bytesRead == 0)
                {
                    //the client has disconnected from the server
                    break;
                }

                //message has successfully been received
                ASCIIEncoding encoder = new ASCIIEncoding();
                string receivedData = encoder.GetString(message, 0, bytesRead);
                if (receivedData.EndsWith("\0"))
                {
                    data += receivedData.Substring(0, receivedData.Length - 1);
                    serverGetsDataDelegate.Invoke(data);
                    data = "";
                }
                else
                {
                    data += receivedData;
                }
            }
			nbThreadRunning--;
        }

        private void ListenForServer()
        {
			nbThreadRunning++;

            while (true && !mustExitThreads)
            {
                if (!client.Connected)
                {
                    continue;
                }

                NetworkStream clientStream = client.GetStream();

                if (!clientStream.CanRead)
                {
                    continue;
                }

                byte[] message = new byte[4096];
                string data = "";
                int bytesRead;

                while (true && !mustExitThreads)
                {
                    bytesRead = 0;

                    try
                    {
                        //blocks until the server sends a message
                        bytesRead = clientStream.Read(message, 0, 4096);
                    }
                    catch
                    {
                        //a socket error has occured
                        break;
                    }

                    if (bytesRead == 0)
                    {
                        //the server has disconnected
                        break;
                    }

                    //message has successfully been received
                    ASCIIEncoding encoder = new ASCIIEncoding();
                    string receivedData = encoder.GetString(message, 0, bytesRead);
                    if (receivedData.EndsWith("\0"))
                    {
                        data += receivedData.Substring(0, receivedData.Length - 1);
                        break;
                    }
                    else
                    {
                        data += receivedData;
                    }
                }
                if (data.Length > 0)
                {
                    clientGetsDataDelegate.Invoke(data);
                }
            }

			connectionState = "Communication ended with server";
			mustExitThreads = true;
			client.Close();

			nbThreadRunning--;
		}
	}
}












