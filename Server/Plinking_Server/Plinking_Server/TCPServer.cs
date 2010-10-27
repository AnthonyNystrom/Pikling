using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections;
using System.IO;
using System.Windows.Forms;

namespace Plinking_Server
{
    /// <summary>
    /// TCPServer is the Server class. When "StartServer" method is called
    /// this Server object tries to connect to a IP Address specified on a port
    /// configured. Then the server start listening for client socket requests.
    /// As soon as a requestcomes in from any client then a Client Socket 
    /// Listening thread will be started. That thread is responsible for client
    /// communication.
    /// </summary>
    public class TCPServer
    {
        /// <summary>
        /// Default Constants.
        /// </summary>
        public static IPAddress DEFAULT_SERVER = IPAddress.Parse("127.0.0.1");
        public static int DEFAULT_PORT = 81;
        public static IPEndPoint DEFAULT_IP_END_POINT =
            new IPEndPoint(DEFAULT_SERVER, DEFAULT_PORT);

        /// <summary>
        /// Local Variables Declaration.
        /// </summary>
        private TcpListener m_server = null;
        private bool m_stopServer = false;
        private bool m_stopPurging = false;
        private Thread m_serverThread = null;
        private Thread m_purgingThread = null;
        private ArrayList m_socketListenersList = null;
        /// <summary>
        /// Constructors.
        /// </summary>
        public TCPServer()
        {
            Init(DEFAULT_IP_END_POINT);
        }
        public TCPServer(IPAddress serverIP)
        {
            Init(new IPEndPoint(serverIP, DEFAULT_PORT));
        }

        public int GetNumberConnections()
        {
            if (m_socketListenersList!=null)
                return m_socketListenersList.Count;
            else
                return 0;
        }

        public TCPServer(int port)
        {
            Init(new IPEndPoint(DEFAULT_SERVER, port));
        }

        public TCPServer(IPAddress serverIP, int port)
        {
            Init(new IPEndPoint(serverIP, port));
        }

        public TCPServer(IPEndPoint ipNport)
        {
            Init(ipNport);
        }

        /// <summary>
        /// Destructor.
        /// </summary>
        ~TCPServer()
        {
            StopServer();
        }

        /// <summary>
        /// Init method that create a server (TCP Listener) Object based on the
        /// IP Address and Port information that is passed in.
        /// </summary>
        /// <param name="ipNport"></param>
        private void Init(IPEndPoint ipNport)
        {
            try
            {
                m_server = new TcpListener(ipNport);

            }
            catch (Exception)
            {
                m_server = null;
            }
        }

        /// <summary>
        /// Method that starts TCP/IP Server.
        /// </summary>
        public void StartServer()
        {
            if (m_server != null)
            {
                // Create a ArrayList for storing SocketListeners before
                // starting the server.
                m_socketListenersList = new ArrayList();

                // Start the Server and start the thread to listen client 
                // requests.
                m_server.Start();
                m_serverThread = new Thread(new ThreadStart(ServerThreadStart));
                m_serverThread.Start();

                // Create a low priority thread that checks and deletes client
                // SocktConnection objcts that are marked for deletion.
                m_purgingThread = new Thread(new ThreadStart(PurgingThreadStart));
                m_purgingThread.Priority = ThreadPriority.Lowest;
                m_purgingThread.Start();
            }
        }

        /// <summary>
        /// Method that stops the TCP/IP Server.
        /// </summary>
        public void StopServer()
        {
            if (m_server != null)
            {
                // It is important to Stop the server first before doing
                // any cleanup. If not so, clients might being added as
                // server is running, but supporting data structures
                // (such as m_socketListenersList) are cleared. This might
                // cause exceptions.

                // Stop the TCP/IP Server.
                m_stopServer = true;
                m_server.Stop();

                // Wait for one second for the the thread to stop.
                m_serverThread.Join(2000);

                // If still alive; Get rid of the thread.
                if (m_serverThread.IsAlive)
                {
                    m_serverThread.Abort();
                }
                m_serverThread = null;

                m_stopPurging = true;
                m_purgingThread.Join(2000);
                if (m_purgingThread.IsAlive)
                {
                    m_purgingThread.Abort();
                }
                m_purgingThread = null;

                // Free Server Object.
                m_server = null;

                // Stop All clients.
                StopAllSocketListers();

            }
        }


        /// <summary>
        /// Method that stops all clients and clears the list.
        /// </summary>
        private void StopAllSocketListers()
        {
            foreach (TCPSocketListener socketListener
                         in m_socketListenersList)
            {
                socketListener.StopSocketListener();                
            }
            // Remove all elements from the list.
            m_socketListenersList.Clear();
            m_socketListenersList = null;
        }

        /// <summary>
        /// TCP/IP Server Thread that is listening for clients.
        /// </summary>
        private void ServerThreadStart()
        {
            // Client Socket variable;
            Socket clientSocket = null;
            TCPSocketListener socketListener = null;
            while (!m_stopServer)
            {
                try
                {
                    // Wait for any client requests and if there is any 
                    // request from any client accept it (Wait indefinitely).
                    clientSocket = m_server.AcceptSocket();
                    // Create a SocketListener object for the client.
                    socketListener = new TCPSocketListener(clientSocket);

                    // Start a communicating with the client in a different
                    // thread.
                    if (socketListener.StartSocketListener())
                    {
                        // Add the socket listener to an array list in a thread 
                        // safe fashon.
                        //Monitor.Enter(m_socketListenersList);
                        lock (m_socketListenersList)
                        {
                            m_socketListenersList.Add(socketListener);
                            //Program.MainForm.AddConnection(clientSocket.RemoteEndPoint.ToString());
                        }
                        
                    }
                    else
                        clientSocket.Disconnect(false);
                }
                catch (SocketException)
                {
                    m_stopServer = true;
                }
            }
            Console.WriteLine("ServerThreadStart Closed");
        }

        /// <summary>
        /// Thread method for purging Client Listeneres that are marked for
        /// deletion (i.e. clients with socket connection closed). This thead
        /// is a low priority thread and sleeps for 10 seconds and then check
        /// for any client SocketConnection obects which are obselete and 
        /// marked for deletion.
        /// </summary>
        private void PurgingThreadStart()
        {
            try{
                while (!m_stopPurging)
                {
                    ArrayList deleteList = new ArrayList();

                    // Check for any clients SocketListeners that are to be
                    // deleted and put them in a separate list in a thread sage
                    // fashon.
                    //Monitor.Enter(m_socketListenersList);
                    lock (m_socketListenersList)
                    {
                        foreach (TCPSocketListener socketListener
                                     in m_socketListenersList)
                        {
                            if (socketListener.IsMarkedForDeletion())
                            {
                                // remove the counter connection for this proxy
                                //Program.MainForm.RemoveConnection(socketListener.m_clientSocket.RemoteEndPoint.ToString());
                                deleteList.Add(socketListener);
                                socketListener.StopSocketListener();
                            }
                        }

                        // Delete all the client SocketConnection ojects which are
                        // in marked for deletion and are in the delete list.
                        for (int i = 0; i < deleteList.Count; ++i)
                        {
                            m_socketListenersList.Remove(deleteList[i]);
                        }
                    }


                    deleteList = null;
                    Thread.Sleep(10000);
                }
            }
            catch (Exception Ex)
            {
            }
        }
    }
}
