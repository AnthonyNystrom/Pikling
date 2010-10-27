using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Configuration;
using System.Net.Sockets;
using System.Net;
using System.Collections;
using System.Threading;


namespace Proxy
{
    public partial class MainForm : Form
    {
        public const int ICON_RED   = 0;
        public const int ICON_GREEN = 1;
        public const int ICON_BLACK = 2;
        public const int ICON_OFF   = 3;

        bool bStartOn;
        bool bExitThread;
        bool IsConnectionSuccessful;
        bool bWorkingOn;
        bool bDataOK;
        ArrayList m_Socks;
        TCPServer m_sockServer;
        Thread m_PingProxies;
        AutoResetEvent PingEvent;
        AutoResetEvent StopService;
        ManualResetEvent TimeoutObject = new ManualResetEvent(false);
        byte[] m_byBuff = new byte[1024];	// Recieved data buffer

        public MainForm()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            int i;
            m_Socks = new ArrayList();
            PingEvent = new AutoResetEvent(false);
            StopService = new AutoResetEvent(false);


            for (i = 0; i < 4; i++)
            {
                if (ConfigurationSettings.AppSettings["enable_server" + (i + 1)] == "1")
                    listView1.Items[i].Checked = true;
                else
                    listView1.Items[i].Checked = false;

                listView1.Items[i].ImageIndex = ICON_OFF;                
                listView1.Items[i].SubItems[1].Text = ConfigurationSettings.AppSettings["server" + (i + 1).ToString()];
                listView1.Items[i].SubItems[2].Text = ConfigurationSettings.AppSettings["port_server" + (i + 1).ToString()];
                listView1.Items[i].SubItems[3].Text = "0";
            }
            
        }
        /// <summary>
        /// Search first server enabled with less connection
        /// </summary>
        /// <param name="iIdxServer">Index of proxy. ret val</param>
        /// <param name="sIP">Address ip. Ret val</param>
        /// <param name="iPort">iPort. Ret val</param>
        /// <returns>true if a proxy is available</returns>
        public bool GivemeAServer(ref int iIdxServer, ref String sIP, ref int iPort)
        {
            int i;
            int iConnections = 1000;
            int iIndex = -1;
            for (i = 0; i < listView1.Items.Count; i++)
            {
                if (GetIcon(i) == ICON_GREEN)   // proxy enabled ?
                {
                    int ic = GetConnections(i);   // get number of active connections
                    if (ic < iConnections)      // check if minus connections
                    {
                        iConnections = ic;
                        iIndex = i;
                    }
                }
            }
            iIdxServer = iIndex;
            if (iIndex >= 0)
            {
                sIP = GetIP(iIndex);
                String sPort = ConfigurationSettings.AppSettings["port_server" + (iIndex + 1).ToString()];
                iPort = Convert.ToInt32(sPort.ToString());
                return true;
            }
            else
                return false;
        }
        /// <summary>
        /// Get the connections item from the list. To use from out of main UI thread
        /// </summary>
        /// <param name="iIndex">Index of list</param>
        /// <returns>number of connections active</returns>
        delegate int GetConnectionsCallback(int iIndex);
        int GetConnections(int iIndex)
        {
            try
            {
                if (!listView1.InvokeRequired)
                {
                    return Convert.ToInt32(listView1.Items[iIndex].SubItems[3].Text);
                }
                else
                {
                    GetConnectionsCallback d = new GetConnectionsCallback(GetConnections);
                    return (int)Invoke(d, new object[] { iIndex });
                }
            }
            catch (Exception)
            {
            }
            return -1;
        }
        /// <summary>
        /// remove a unit from counter connection proxy
        /// </summary>
        /// <param name="iIndex">Index of list</param>
        delegate void RemoveConnectionCallback(int iIndex);
        public void RemoveConnection(int iIndex)
        {
            try
            {
                if (!listView1.InvokeRequired)
                {
                    listView1.Items[iIndex].SubItems[3].Text = (Convert.ToInt32(listView1.Items[iIndex].SubItems[3].Text) - 1).ToString();
                }
                else
                {
                    RemoveConnectionCallback d = new RemoveConnectionCallback(RemoveConnection);
                    Invoke(d, new object[] { iIndex });
                }
            }
            catch (Exception)
            {
            }
        }
        /// <summary>
        /// Add a unit from counter connection proxy
        /// </summary>
        /// <param name="iIndex">Index of list</param>
        delegate void AddConnectionCallback(int iIndex);
        public void AddConnection(int iIndex)
        {
            try
            {
                if (!listView1.InvokeRequired)
                {
                    listView1.Items[iIndex].SubItems[3].Text = (Convert.ToInt32(listView1.Items[iIndex].SubItems[3].Text) + 1).ToString();
                }
                else
                {
                    AddConnectionCallback d = new AddConnectionCallback(AddConnection);
                    Invoke(d, new object[] { iIndex });
                }
            }
            catch (Exception)
            {
            }
        }

        private void butStartStop_Click(object sender, EventArgs e)
        {
            bStartOn = !bStartOn;
            StartStopService(bStartOn);
        }
        /// <summary>
        /// Start and Stop all service.
        /// </summary>
        /// <param name="bStart">if true start the service</param>
        void StartStopService(bool bStart)
        {
            if (bStart)
            {
                // run the server socket
                String sIP = ConfigurationSettings.AppSettings["public_ip"];
                IPAddress ip = IPAddress.Parse(sIP);
                m_sockServer = new TCPServer(ip, Convert.ToInt32(ConfigurationSettings.AppSettings["public_port"]));
                m_sockServer.StartServer();

                // run thread for proxy connections ping status
                bExitThread = false;
                m_PingProxies = new Thread(new ThreadStart(PingProxies));
                m_PingProxies.Start();
                butStartStop.Text = "Stop";
            }
            else
            {
                m_sockServer.StopServer();
                bExitThread = true;
                PingEvent.Set();
                StopService.WaitOne();
                butStartStop.Text = "Start";

                int i;
                for (i = 0; i < listView1.Items.Count; i++)
                    listView1.Items[i].ImageIndex = ICON_OFF;
            }
        }
        bool Connect(String sIP, int iPort, int iIdx)
        {
            bool bret = false;
            IsConnectionSuccessful = false;
            try
            {
                Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                if (iIdx < 0)
                    m_Socks.Add(sock);
                else
                    m_Socks[iIdx] = sock;
                sock.ReceiveTimeout = 3000;
                sock.Blocking = false;
                AsyncCallback onconnect = new AsyncCallback(OnConnect);
                // Define the Server address and port
                IPEndPoint epServer = new IPEndPoint(IPAddress.Parse(sIP), iPort);
                sock.BeginConnect(epServer, onconnect, sock);
                if (TimeoutObject.WaitOne(2000, false))
                {
                    if (IsConnectionSuccessful)
                        bret = true;
                }
                else
                    sock.Close();
            }
            catch (Exception)
            {
            }
            return bret;
        }

        public void OnConnect(IAsyncResult ar)
        {
            // Socket was the passed in object
            Socket sock = (Socket)ar.AsyncState;

            // Check if we were sucessfull
            try
            {
                if (sock.Connected && bStartOn)
                {
                    sock.EndConnect(ar);
                    IsConnectionSuccessful = true;
                    Console.WriteLine("Connected to:" + sock.RemoteEndPoint.ToString());
                    int iIdx = sock.RemoteEndPoint.ToString().LastIndexOf(':');
                    if (iIdx >= 0)
                    {
                        String sIP = sock.RemoteEndPoint.ToString().Substring(0, iIdx);
                        int i;
                        for (i = 0; i < listView1.Items.Count; i++)
                        {
                            if (GetIP(i).CompareTo(sIP) == 0)
                                SetIcon(i, ICON_GREEN);
                        }
                    }
                }
                else
                    Console.WriteLine("Unable to connect");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception OnConnect:" + ex.ToString());
            }
            finally
            {
                TimeoutObject.Set();
            }

        }

        String GetIP(int iIdxServer)
        {
            return ConfigurationSettings.AppSettings["server" + (iIdxServer + 1).ToString()];
        }

        /// <summary>
        /// Get the icon item from the list. To use from out of main UI thread
        /// </summary>
        /// <param name="iIndex">Index of list</param>
        /// <returns>icon index</returns>
        delegate int GetIconCallback(int iIndex);
        int GetIcon(int iIndex)
        {
            try
            {
                if (!listView1.InvokeRequired)
                {
                    return listView1.Items[iIndex].ImageIndex;
                }
                else
                {
                    GetIconCallback d = new GetIconCallback(GetIcon);
                    return (int)Invoke(d, new object[] { iIndex });
                }
            }
            catch (Exception)
            {
            }
            return -1;
        }
        /// <summary>
        /// Set the icon item of the list. To use from out of main UI thread
        /// </summary>
        /// <param name="iIndex">Index of list</param>
        /// <param name="iIcon">icon index</param>
        delegate void SetIconCallback(int iIndex, int iIcon);
        void SetIcon(int iIndex, int iIcon)
        {
            try
            {
                if (!listView1.InvokeRequired)
                {
                    listView1.Items[iIndex].ImageIndex = iIcon;
                }
                else
                {
                    SetIconCallback d = new SetIconCallback(SetIcon);
                    Invoke(d, new object[] { iIndex, iIcon });
                }
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Thread that check status of proxies
        /// </summary>
        private void PingProxies()
        {
            System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
            byte[] byBuff = new byte[255];
            String sQuestion = "ping";
            byte[] byQuestion = encoding.GetBytes(sQuestion);
            int iPort;

            // create Connections
            int i;
            StringBuilder sPort = new StringBuilder(255);
            for (i = 0; i < listView1.Items.Count; i++)
            {
                try
                {   if (ConfigurationSettings.AppSettings["enable_server" + (i + 1).ToString()] == "1")
                    {
                        String sIP = GetIP(i);
                        String sP = ConfigurationSettings.AppSettings["port_server" + (i + 1).ToString()];
                        iPort = Convert.ToInt32(sP);
                        SetIcon(i, ICON_BLACK);
                        Connect(sIP, iPort, -1);
                    }
                }
                catch (SocketException ex)
                {
                    MessageBox.Show(ex.ToString());
                    Console.WriteLine("PingProxies:" + ex.ToString());
                }
            }

            bWorkingOn = false;
            do
            {
                PingEvent.WaitOne();
                bWorkingOn = true;
                for (i = 0; i < listView1.Items.Count && !bExitThread; i++)
                {
                    try
                    {
                        if (ConfigurationSettings.AppSettings["enable_server" + (i + 1).ToString()] == "1")
                        {
                            Socket sock = (Socket)m_Socks[i];
                            if (sock != null && sock.Connected == false)
                            {
                                SetIcon(i, ICON_BLACK);
                                String sIP = GetIP(i);
                                iPort = Convert.ToInt32(ConfigurationSettings.AppSettings["port_server" + (i + 1).ToString()]);
                                Connect(sIP, iPort, i);
                            }
                            if (GetIcon(i) == ICON_GREEN)
                            {
                                bDataOK = false;
                                sock.Send(byQuestion, sQuestion.Length, 0);
                                int iRx = sock.Receive(byBuff, byBuff.Length, 0);
                                if (iRx == byQuestion.Length)
                                {
                                    int ii;
                                    bool bEqual = true;
                                    for (ii = 0; ii < iRx && bEqual; ii++)
                                    {
                                        if (byQuestion[ii] != byBuff[ii])
                                            bEqual = false;
                                    }
                                    if (!bEqual)
                                    {
                                        sock.Close();
                                        SetIcon(i, ICON_BLACK);
                                    }
                                }
                                else
                                {
                                    sock.Close();
                                    SetIcon(i, ICON_BLACK);
                                }

                            }
                        }
                    }
                    catch (SocketException ex)
                    {
                        Console.WriteLine("PingProxies:" + ex.ToString());
                    }
                }
                bWorkingOn = false;

            } while (!bExitThread);
            StopService.Set();
        }
        /// <summary>
        /// Get the new data and send it out to all other connections. 
        /// Note: If not data was recieved the connection has probably 
        /// died.
        /// </summary>
        /// <param name="ar"></param>
        public void OnRecievedData(IAsyncResult ar)
        {
            // Socket was the passed in object
            Socket sock = (Socket)ar.AsyncState;

            // Check if we got any data
            try
            {
                int nBytesRec = sock.EndReceive(ar);
                if (nBytesRec > 0)
                {
                    if (nBytesRec >= 4)
                    {
                        int iIdx = sock.RemoteEndPoint.ToString().LastIndexOf(':');
                        int iIdxIP = -1;
                        if (iIdx >= 0)
                        {
                            String sIP = sock.RemoteEndPoint.ToString().Substring(0, iIdx);
                            int i;
                            for (i = 0; i < listView1.Items.Count && iIdxIP < 0; i++)
                            {
                                if (GetIP(i).CompareTo(sIP) == 0)
                                    iIdxIP = i;
                            }
                            byte[] bal = new byte[nBytesRec];
                            sock.Receive(bal);
                            if (bal[0] == 'p' &&
                                bal[1] == 'i' &&
                                bal[2] == 'n' &&
                                bal[3] == 'g' && iIdxIP >= 0)
                            {
                                SetIcon(iIdxIP, ICON_GREEN);
                            }
                            else
                                SetIcon(iIdxIP, ICON_BLACK);
                        }

                    }

                    // If the connection is still usable restablish the callback
                    SetupRecieveCallback(sock);
                }
                else
                {
                    Console.WriteLine("Server disconnected:" + sock.RemoteEndPoint.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unusual error during Recieve!" + ex.ToString());
            }
        }
        /// <summary>
        /// Setup the callback for recieved data and loss of conneciton
        /// </summary>
        public void SetupRecieveCallback(Socket sock)
        {
            try
            {
                AsyncCallback recieveData = new AsyncCallback(OnRecievedData);
                sock.BeginReceive(m_byBuff, 0, m_byBuff.Length, SocketFlags.None, recieveData, sock);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Setup Recieve Callback failed!:" + ex.ToString());
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (bStartOn)
                StartStopService(false);
        }

        private void butExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void tmrPing_Tick_1(object sender, EventArgs e)
        {
            try
            {
                int iNConnections = m_sockServer.GetNumberConnections();
                Text = "Proxy Pikling - Connections : " + iNConnections.ToString();

                if (!bWorkingOn)
                    PingEvent.Set();
            }
            catch (Exception ex) { }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void listView1_ItemCheck(object sender, ItemCheckEventArgs e)
        {
        }

        //private void tmrUpdateDB_Tick(object sender, EventArgs e)
        //{
        //    string sProduct = "Pikling"; //ex. pikling, voxxee
        //    string sSocketIP = "69.21.113.100";//ConfigurationSettings.AppSettings["public_ip"]; //Socket server IP
        //    int iSocketPort = 10;//Convert.ToInt32(ConfigurationSettings.AppSettings["public_port"]); //port for connection
        //    int iNThreads = 0; // Current primary socket threads for node
        //    bool bOnline = true; // is online or offline ex. available
        //    // count total current connections
        //    int i;
        //    for (i = 0; i < listView1.Items.Count; i++)
        //    {
        //        if (GetIcon(i) == ICON_GREEN)   // proxy enabled ?
        //            iNThreads += GetConnections(i);   // get number of active connections
        //    }


        //        String sConn = "Password=tony6472rene91970;Persist Security Info=True;User ID=user;Initial Catalog=7tservers;Data Source=192.168.3.222";
        //        SqlConnection conn3 = new SqlConnection(sConn);
                
        //        //SqlTransaction trans3 = conn3.BeginTransaction();
        //        //SqlCommand cmd3 = new SqlCommand("EXEC NewUpdateCommand, @product, @socketIP, @socketPORT, @threads, @online" , conn3);
        //        //cmd3.Transaction = trans3;
        //   try
        //   {
        //       conn3.Open();
        //        System.Data.SqlClient.SqlCommand cmd3 = new System.Data.SqlClient.SqlCommand();
        //        cmd3.CommandType = System.Data.CommandType.Text;
        //        //cmd3.CommandText = "UPDATE TBL7tSERVERS SET product = sProduct, socketIP = sSocketIP, socketPORT = iSocketPort, threads = iNThreads, online = bOnline WHERE socketIP = sSocketIP AND product = sProduct";
        //        cmd3.CommandText = "UPDATE TBL7tSERVERS SET product = " + sProduct + ", socketIP = " + sSocketIP + ", socketPORT = " + iSocketPort + ", threads = " + iNThreads + ", online = " + bOnline + ", lastUpdate = " + DateTime.Now.TimeOfDay + " WHERE socketIP = " + sSocketIP + " AND product = " + sProduct;
        //        MessageBox.Show("UPDATE TBL7tSERVERS SET product = " + sProduct + ", socketIP = " + sSocketIP + ", socketPORT = " + iSocketPort + ", threads = " + iNThreads + ", online = " + bOnline + ", lastUpdate = " + DateTime.Now.TimeOfDay + " WHERE socketIP = " + sSocketIP + " AND product = " + sProduct);
        //       cmd3.Connection = conn3;


        //       //cmd3.Parameters.Clear();
        //       //cmd3.Parameters.Add(new SqlParameter("@product", sProduct));
        //       //cmd3.Parameters.Add(new SqlParameter("@socketIP", sSocketIP));
        //       //cmd3.Parameters.Add(new SqlParameter("@socketPORT", iSocketPort));
        //       //cmd3.Parameters.Add(new SqlParameter("@threads", iNThreads));
        //       //cmd3.Parameters.Add(new SqlParameter("@online", bOnline));
        //       cmd3.ExecuteNonQuery();
        //       //trans3.Commit();
        //       //trans3 = conn3.BeginTransaction();
        //       //cmd3.Transaction = trans3;
        //   }
        //   finally
        //   {
        //       //if (trans3 != null) trans3.Commit();
        //       conn3.Close();
        //       //trans3.Dispose();
        //       conn3.Dispose();
        //       //trans3 = null;
        //       conn3 = null;
        //   }
        //}        
    }   
}
