using System;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.IO;

namespace Proxy
{
	/// <summary>
	/// Summary description for TCPSocketListener.
	/// </summary>
	public class TCPSocketListener
	{
		/// <summary>
		/// Variables that are accessed by other classes indirectly.
		/// </summary>
		private Socket m_clientSocket = null;
        private Socket m_proxySocket = null;
		private bool m_stopClient=false;
		private Thread m_clientListenerThreadPhone=null;
        private Thread m_clientListenerThreadProxy= null;
		private bool m_markedForDeletion=false;
        private int m_iIdxProxy;
        private String m_ProxysIP;
        private int m_ProxyiPort;
        private DateTime m_lastReceiveDateTime;
        private DateTime m_currentReceiveDateTime;
        private System.Threading.Timer _t;


		/// <summary>
		/// Client Socket Listener Constructor.
		/// </summary>
		/// <param name="clientSocket">Phone connection</param>
        /// <param name="sIP">Proxy address</param>
        /// <param name="iPort">Port </param>
        /// <param name="iIdxProxy">Index from main list proxy of main form</param>
        public TCPSocketListener(Socket clientSocket, String sIP, int iPort, int iIdxProxy)
		{
			m_clientSocket = clientSocket;
            m_iIdxProxy = iIdxProxy;
            m_ProxysIP = sIP;
            m_ProxyiPort = iPort;
            m_proxySocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		}

		/// <summary>
		/// Client SocketListener Destructor.
		/// </summary>
		~TCPSocketListener()
		{
			StopSocketListener();
		}

		/// <summary>
		/// Method that starts SocketListener Thread.
		/// </summary>
		public bool StartSocketListener()
		{
            if (m_clientSocket != null && m_proxySocket != null)
            {
                try
                {
                    m_proxySocket.Connect(m_ProxysIP, m_ProxyiPort);
                    if (!m_proxySocket.Connected)
                        return false;    
                }
                catch (Exception)
                {
                }
                // create thread to data listen from phone
                m_clientListenerThreadPhone = new Thread(new ThreadStart(SocketListenerThreadPhone));
                m_clientListenerThreadPhone.Start();
                // create thread to data listen from proxy
                m_clientListenerThreadProxy = new Thread(new ThreadStart(SocketListenerThreadProxy));
                m_clientListenerThreadProxy.Start();

                return true;
            }
            else
                return false;
		}

		/// <summary>
		/// Thread method that does the communication to the client phone. This 
		/// thread tries to receive from phone and forward to proxy
		/// </summary>
		private void SocketListenerThreadPhone()
		{
			int size=0;
			Byte [] byteBuffer = new Byte[1024];
			while (!m_stopClient)
			{
				try
				{
                    m_lastReceiveDateTime = DateTime.Now;
                    m_currentReceiveDateTime = DateTime.Now;
                    _t = new System.Threading.Timer(new TimerCallback(CheckTimeOut), null, 60000, 60000);

					size = m_clientSocket.Receive(byteBuffer);
                    m_currentReceiveDateTime = DateTime.Now;
                    if (m_proxySocket.Connected)
                        m_proxySocket.Send(byteBuffer, size, 0);
                    else
                        StopSocketListener();
				}
				catch (SocketException )
				{
					m_stopClient=true;
					m_markedForDeletion=true;
				}
			}
		}
        /// <summary>
        /// Thread method that does the communication to the client proxy. This 
        /// thread tries to receive from proxy and forward to phone
        /// </summary>
        private void SocketListenerThreadProxy()
        {
            int size = 0;
            Byte[] byteBuffer = new Byte[1024];
            while (!m_stopClient)
            {
                try
                {
                    size = m_proxySocket.Receive(byteBuffer);
                    if (m_clientSocket.Connected)
                        m_clientSocket.Send(byteBuffer, size, 0);
                    else
                        StopSocketListener();
                }
                catch (SocketException)
                {
                    m_stopClient = true;
                    m_markedForDeletion = true;
                }
            }
        }

		/// <summary>
		/// Method that stops Client SocketListening Thread.
		/// </summary>
		public void StopSocketListener()
		{
			if (m_clientSocket!= null)
			{
				m_stopClient=true;
				m_clientSocket.Close();
                m_proxySocket.Close();

				// Wait for one second for the the thread to stop.
                m_clientListenerThreadPhone.Join(1000);
                m_clientListenerThreadProxy.Join(1000);
				
				// If still alive; Get rid of the thread.
                if (m_clientListenerThreadPhone.IsAlive)
                    m_clientListenerThreadPhone.Abort();
                if (m_clientListenerThreadProxy.IsAlive)
                    m_clientListenerThreadProxy.Abort();

                m_clientListenerThreadPhone = null;
                m_clientListenerThreadProxy = null;
				m_clientSocket=null;
                m_proxySocket =null;
				m_markedForDeletion=true;
			}
		}

		/// <summary>
		/// Method that returns the state of this object i.e. whether this
		/// object is marked for deletion or not.
		/// </summary>
		/// <returns></returns>
		public bool IsMarkedForDeletion()
		{
			return m_markedForDeletion;
		}
        /// <summary>
		/// Method that returns index of proxy from main form
		/// object is marked for deletion or not.
		/// </summary>
		/// <returns>Index proxy</returns>
        public int GetIndexProxy()
        {
            return m_iIdxProxy;
        }
        /// <summary>
        /// Method that checks whether there are any client calls for the
        /// last x seconds or not. If not this client SocketListener will
        /// be closed.
        /// </summary>
        /// <param name="o"></param>
        private void CheckTimeOut(object o)
        {
            
            if (m_lastReceiveDateTime.Equals(m_currentReceiveDateTime))
            {
                StopSocketListener();
                _t.Dispose();
            }
            else
            {
                m_lastReceiveDateTime = m_currentReceiveDateTime;
            }
        }
		
	}
}
