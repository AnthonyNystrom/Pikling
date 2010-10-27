using System;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
using System.Data.Sql;
using System.Data.SqlClient;

namespace Plinking_Server
{
    /// <summary>
    /// TCPSocketListener is the direct socket comunication server with client. into is implemented the protocol.
    /// </summary>
    public class TCPSocketListener
    {
        /// <summary>
        /// Variables that are accessed by other classes indirectly.
        /// </summary>
        public Socket m_clientSocket = null;
        /// <summary>
        /// Working Variables.
        /// </summary>
        private DateTime m_lastReceiveDateTime;
        private DateTime m_currentReceiveDateTime;
        private bool m_stopClient = false;
        private Thread m_clientListenerThreadPhone = null;
        private bool m_markedForDeletion = false;
        private System.Threading.Timer _t;
        String sFolder = "";

        /// <summary>
        /// Client Socket Listener Constructor.
        /// </summary>
        /// <param name="clientSocket">Phone connection</param>
        public TCPSocketListener(Socket clientSocket)
        {
            m_clientSocket = clientSocket;

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
            if (m_clientSocket != null)
            {
                // create thread to data listen from phone
                m_clientListenerThreadPhone = new Thread(new ThreadStart(SocketListenerThreadPhone));
                m_clientListenerThreadPhone.Start();

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
            int size = 0;
            Byte[] byteBuffer = new Byte[1024 * 100];
            Byte[] byBuffer = new Byte[1024];
            FileStream fout = null;
            int iCounter = 0;
            int iFileLen = 0;
            int iEmailDataLen = 0;
            String sEmailData = "";
            String sFileBmp = "";
            String sFileJpg = "";
            String sFileSrc = "";
            String sFileDest = "";
            String sLang = "";
            String sName = "";
            String sSrc = "";
            String sDst = "";
            String sHeader = "";
            int iStato = -1;
            int iLenRes = 0;            
            System.Text.Encoding enc;
            byte []byHeader=new byte[1024];
            int iHeaderSize=0;

            m_lastReceiveDateTime = DateTime.Now;
            m_currentReceiveDateTime = DateTime.Now;
            _t = new System.Threading.Timer(new TimerCallback(CheckTimeOut), null, 60000, 60000);

            while (!m_stopClient)
            {
                try
                {
                    if (iStato==-1)
                        size = m_clientSocket.Receive(byteBuffer, (int)0, (int)1, SocketFlags.None);
                    else
                        size = m_clientSocket.Receive(byteBuffer, (int)0, (int)1024, SocketFlags.None);
                    if (size > 0)
                    {
                        m_currentReceiveDateTime = DateTime.Now;
                        switch (iStato)
                        {
                            case -1:
                                if (byteBuffer[0] != 'p')
                                {
                                    iCounter = 0;
                                    if (byteBuffer[0] == 0)
                                        iStato = 0;
                                    else
                                        iStato = 5;
                                    m_clientSocket.Send(byteBuffer, 1, SocketFlags.None);
                                }
                                else
                                {
                                    size = m_clientSocket.Receive(byteBuffer, (int)1, (int)3, SocketFlags.None);
                                    // check if a ping request
                                    if (size == 3 && byteBuffer[0] == 'p' && byteBuffer[1] == 'i' && byteBuffer[2] == 'n' && byteBuffer[3] == 'g')
                                        m_clientSocket.Send(byteBuffer, size+1, SocketFlags.None);
                                    else
                                    {
                                        Program.MainForm.AddLog(String.Format("BAD PROTOCOL.STATE {0}, SIZE{1}", iStato, size.ToString()), sFolder);
                                        m_stopClient = true;
                                        m_markedForDeletion = true;
                                    }
                                }
                                break;
                            case 0: // receive header settings
                                if (size >= 2 && iHeaderSize==0)
                                {
                                    iHeaderSize |= byteBuffer[1];
                                    iHeaderSize <<= 8;
                                    iHeaderSize |= byteBuffer[0];
                                    size -= 2;
                                    if (size != 0)
                                    {
                                        Array.Copy(byteBuffer, 2, byHeader, 0, size);
                                        iCounter += size;
                                    }
                                }
                                else if (size>0 && iCounter<=iHeaderSize){
                                    Array.Copy(byteBuffer, 0, byHeader, iCounter, size);
                                    iCounter += size;
                                }
                                if (iCounter == iHeaderSize)
                                {
                                    byHeader[iHeaderSize] = 0;
                                    enc = System.Text.Encoding.ASCII;
                                    sHeader = enc.GetString(byHeader);
                                    sHeader = sHeader.Substring(0, size);
                                    Program.MainForm.AddLog(sHeader, sFolder);
                                    string[] sInfo = sHeader.Split('|');
                                    sLang = sInfo[8] + "|" + sInfo[9];

                                    sFolder = Program.MainForm.GetNewFolder(ref sName);
                                    Program.MainForm.AddLog(sHeader, sFolder);
                                    sFileBmp = sFolder + "\\pikling.bmp";
                                    sFileJpg = sFolder + "\\pikling.jpg";
                                    sFileSrc = sFolder + "\\source.dat";
                                    sFileDest = sFolder + "\\dest.dat";
                                    fout = new FileStream(sFileJpg, FileMode.Create);
                                    Program.MainForm.AddLog("************ NEW PROCESS *****************", sFolder);
                                    Program.MainForm.AddLog("ID:" + sName, sFolder);


                                    System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
                                    byBuffer = encoding.GetBytes(sName);
                                    m_clientSocket.Send(byBuffer, sName.Length, SocketFlags.None);
                                    iStato++;
                                    iCounter = 0;
                                }
                                else if (iCounter >= iHeaderSize)
                                {
                                    Program.MainForm.AddLog(String.Format("BAD PROTOCOL.STATE {0}, SIZE{1}", iStato, size.ToString()), sFolder);
                                    m_stopClient = true;
                                    m_markedForDeletion = true;
                                }

                                    break;
                            case 1: // receive size image
                                if (size == 4)
                                {
                                    iFileLen = byteBuffer[3];
                                    iFileLen <<= 8;
                                    iFileLen |= byteBuffer[2];
                                    iFileLen <<= 8;
                                    iFileLen |= byteBuffer[1];
                                    iFileLen <<= 8;
                                    iFileLen |= byteBuffer[0];
                                    m_clientSocket.Send(byteBuffer, 4, 0);
                                    Program.MainForm.AddLog("Size jpg:" + iFileLen.ToString(), sFolder);
                                    iStato++;
                                    iCounter = 0;
                                }
                                else
                                {
                                    Program.MainForm.AddLog(String.Format("BAD PROTOCOL.STATE {0}, SIZE{1}", iStato, size.ToString()), sFolder);
                                    m_stopClient = true;
                                    m_markedForDeletion = true;
                                }
                                break;
                            case 2: // raw image
                                //Program.MainForm.AddLog("Received:" + iCounter.ToString());
                                iCounter += size;
                                fout.Write(byteBuffer, 0, size);
                                if (iCounter == iFileLen && !m_stopClient)
                                {
                                    Program.MainForm.AddLog("File completed", sFolder);
                                    byteBuffer[0] = 1;
                                    m_clientSocket.Send(byteBuffer, 1, 0);
                                    fout.Close();
                                    Program.MainForm.NewImage(sFileJpg, sFolder);
                                    Image img = Image.FromFile(sFileJpg);
                                    img.Save(sFileBmp, ImageFormat.Bmp);

                                    LeadWorker ocrWorker = new LeadWorker(Program.MainForm._ocrEngine, Program.MainForm._codecs);
                                    sSrc = ocrWorker.Start(sFolder, sFileBmp, getLangFrom(sLang));
                                    sDst="";
                                    Byte byTranslator = 0;
                                    try
                                    {
                                        if (sSrc != "")
                                        {
                                            TranslatedTranslator tr = new TranslatedTranslator();
                                            sDst = tr.Translate(sSrc, new TranslateDir(getLangFrom(sLang), getLangTo(sLang)));
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        byTranslator = 1;
                                        Program.MainForm.AddLog(String.Format("Translated translation failed tray with google, Exception:{0}", ex.Message), sFolder);
                                        GoogleTranslator gt = new GoogleTranslator();
                                        try
                                        {
                                            sDst = gt.Translate(sSrc, new TranslateDir(getLangFrom(sLang), getLangTo(sLang)));
                                        }
                                        catch (Exception ex2)
                                        {
                                            Program.MainForm.AddLog(String.Format("Google translation failed Exception:{0}", ex2.Message), sFolder);
                                        }
                                    }
                                    
                                    iLenRes = Encoding.UTF8.GetBytes(sSrc).Length;
                                    byteBuffer[4] = (byte)(iLenRes >> 24);
                                    byteBuffer[3] = (byte)((iLenRes >> 16) & 0xFF);
                                    byteBuffer[2] = (byte)((iLenRes >> 8) & 0xFF);
                                    byteBuffer[1] = (byte)(iLenRes & 0xFF);
                                    byteBuffer[0] = byTranslator;
                                    m_clientSocket.Send(byteBuffer, 5, 0);
                                    if (iLenRes > 0)
                                    {
                                        TextWriter tw = new StreamWriter(sFileSrc, true, System.Text.Encoding.UTF8);
                                        tw.WriteLine(sSrc);
                                        tw.Close();

                                        byBuffer = Encoding.UTF8.GetBytes(sSrc);
                                        m_clientSocket.Send(byBuffer, iLenRes, 0);
                                        Program.MainForm.AddLog("SENT SOURCE:" + sSrc, sFolder);
                                    }
                                    Program.MainForm.AddLog(String.Format("Len result ocr source:{0}", iLenRes), sFolder);
                                    iStato++;

                                }
                                else if (iCounter > iFileLen && !m_stopClient)
                                {
                                    Program.MainForm.AddLog(String.Format("BAD PROTOCOL.STATE {0}, RAW DATA IMAGE TOO LARGE ", iStato), sFolder);
                                    m_stopClient = true;
                                    m_markedForDeletion = true;
                                }

                                break;
                            case 3: // result upload 1   
                                if (size == 1)
                                {
                                    if (byteBuffer[0] != 1)
                                    {
                                        Program.MainForm.AddLog(String.Format("BAD ANSWER FROM CLIENT. STATE: {0}", iStato), sFolder);
                                        m_stopClient = true;
                                        m_markedForDeletion = true;
                                    }
                                    else
                                    {   // go ahead with dest lang string
                                        //iLenRes = sResultLangDest.Length;
                                        iLenRes = Encoding.UTF8.GetBytes(sDst).Length;
                                        byteBuffer[3] = (byte)(iLenRes >> 24);
                                        byteBuffer[2] = (byte)((iLenRes >> 16) & 0xFF);
                                        byteBuffer[1] = (byte)((iLenRes >> 8) & 0xFF);
                                        byteBuffer[0] = (byte)(iLenRes & 0xFF);
                                        m_clientSocket.Send(byteBuffer, 4, 0);
                                        if (iLenRes > 0)
                                        {
                                            //System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
                                            //byBuffer = encoding.GetBytes(sResultLangDest);
                                            TextWriter tw = new StreamWriter(sFileDest, true, System.Text.Encoding.UTF8);
                                            tw.WriteLine(sDst);
                                            tw.Close();

                                            byBuffer = Encoding.UTF8.GetBytes(sDst);
                                            m_clientSocket.Send(byBuffer, iLenRes, 0);
                                            Program.MainForm.AddLog("SENT DEST:" + sDst, sFolder);
                                        }
                                        sHeader += "|" + sSrc + "|" + sDst + "|"+sName;
                                        string[] sInfo = sHeader.Split('|');
                                        UpdateDB(sInfo);

                                        Program.MainForm.AddLog(String.Format("Len result ocr dest:{0}", iLenRes), sFolder);
                                        iStato++;
                                    }
                                }
                                else
                                {
                                    Program.MainForm.AddLog(String.Format("BAD PROTOCOL.STATE {0}, SIZE{1}", iStato, size.ToString()), sFolder);
                                    m_stopClient = true;
                                    m_markedForDeletion = true;
                                }
                                break;
                            case 4: // result upload 2  
                                Program.MainForm.AddLog("HANSHAKE PROTOCOL COMPLETED*************", sFolder);
                                iStato = -1;
                                break;

                            case 5:
                                if (size == 2)
                                {
                                    iEmailDataLen = byteBuffer[1];
                                    iEmailDataLen <<= 8;
                                    iEmailDataLen |= byteBuffer[0];
                                    m_clientSocket.Send(byteBuffer, 2, 0);
                                    iStato++;
                                }
                                else
                                {
                                    Program.MainForm.AddLog(String.Format("BAD PROTOCOL.STATE {0}, SIZE{1}", iStato, size.ToString()), sFolder);
                                    m_stopClient = true;
                                    m_markedForDeletion = true;
                                }
                                break;

                            case 6:
                                byteBuffer[iEmailDataLen] = 0;
                                enc = System.Text.Encoding.ASCII;
                                sEmailData = enc.GetString(byteBuffer);
                                sEmailData = sEmailData.Substring(0, iEmailDataLen);
                                String sFolderData = sEmailData.Substring(0, 10);
                                String sFile = Application.StartupPath + "\\" + sFolderData + "\\sending.txt";
                                bool exists = System.IO.Directory.Exists(Application.StartupPath + "\\" + sFolderData);
                                if (File.Exists(sFile))
                                    File.Delete(sFile);
                                Program.MainForm.AddLog(String.Format("Request email {0} folder {1}", sEmailData, sFolderData), sFolderData);
                                if (exists)
                                {
                                    byteBuffer[0] = 1;
                                    TextWriter tw = new StreamWriter(sFile);
                                    tw.WriteLine(sEmailData);
                                    tw.Close();
                                }
                                else
                                {
                                    Program.MainForm.AddLog(String.Format("Request email failed folder not found {0}", sFolderData), sFolderData);
                                    byteBuffer[0] = 0;
                                    m_clientSocket.Send(byteBuffer, 1, 0);
                                    Thread.Sleep(2000);
                                    m_stopClient = true;
                                    m_markedForDeletion = true;
                                }
                                iStato = -1;
                                m_clientSocket.Send(byteBuffer, 1, 0);
                                break;
                        }
                    }
                    else
                    {
                        m_stopClient = true;
                        m_markedForDeletion = true;
                    }
                }
                catch (Exception Ex)
                {
                    m_stopClient = true;
                    m_markedForDeletion = true;
                    /*Program.MainForm.AddLog(String.Format("EXCEPTION state{0}:{1}", iStato, Ex.Message), sFolder);
                    Program.MainForm.AddLog("************ END PROCESS *****************", sFolder);*/
                }
            }
            _t.Dispose();
        }
        public String getLangFrom(String sLang)
        {
            int iIndex = sLang.LastIndexOf('|');
            return sLang.Substring(0, iIndex);
        }
        public String getLangTo(String sLang)
        {
            int iIndex = sLang.LastIndexOf('|');
            return sLang.Substring(iIndex + 1, sLang.Length - (iIndex + 1));
        }



        /// <summary>
        /// Method that stops Client SocketListening Thread.
        /// </summary>
        public void StopSocketListener()
        {

            try
            {
                if (m_clientSocket != null)
                {
                    m_stopClient = true;
                    m_clientSocket.Close();
                    // Wait for one second for the the thread to stop.
                    m_clientListenerThreadPhone.Join(1000);

                    // If still alive; Get rid of the thread.
                    if (m_clientListenerThreadPhone.IsAlive)
                        m_clientListenerThreadPhone.Abort();

                    m_clientListenerThreadPhone = null;
                    m_clientSocket = null;
                    m_markedForDeletion = true;
                }
            }
            catch (Exception Ex)
            {
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
        /// Method that checks whether there are any client calls for the
        /// last x seconds or not. If not this client SocketListener will
        /// be closed.
        /// </summary>
        /// <param name="o"></param>
        private void CheckTimeOut(object o)
        {

            if (m_lastReceiveDateTime.Equals(m_currentReceiveDateTime))
            {
                Program.MainForm.AddLog("TIMEOUT CLOSE THE CONNECTION", sFolder);
                StopSocketListener();
                _t.Dispose();
            }
            else
            {
                m_lastReceiveDateTime = m_currentReceiveDateTime;
            }
        }
        void UpdateDB(String []sInfo)
        {
           string phonebrand = sInfo[0]; // ex. Apple
           string phonemodel = sInfo[1]; // ex. iPhone
           string phoneserial = sInfo[2]; //if can get
           string phonenumber = sInfo[3];  //if can get
           string OSVersion = sInfo[4]; //example 3.0
           string IPAddress = "test"; //mobile client
           DateTime date = DateTime.Today; //todays date as
           DateTime time = DateTime.Now;
           string GeoA = sInfo[5]; // Geo cord lat if supplied in this way
           string GeoB = sInfo[6]; //Geo cord long if supplied in this way
           string userid = sInfo[7] ; //email
           string langin = sInfo[8]; //Language in
           string langout = sInfo[9]; //Language out
           string textin = sInfo[10]; //text to translate
           string textout = sInfo[11]; //translated text
           string product = "Pikling"; //whatever product is connecting
           string imageinpath = sInfo[12]; //network unc path to this
/*users image dir --> \\192.168.2.222\pikling\users\[email
address]\[each transaction]\imagein <-- will need to create upon each
connection or write to.*/
           string imageoutpath = sInfo[12]; //network unc path to this
/*users image dir --> \\192.168.2.222\pikling\users\[email
address]\[each transaction]\imageout <-- will need to create upon each
connection or write to.*/
           string docpdcoutpath = sInfo[12]; //network unc path to this
/*users image dir --> \\192.168.2.222\pikling\users\[email
address]\[each transaction]\docout <-- will need to create upon each
connection or write to.*/

           SqlConnection conn = new SqlConnection("Password=tony6472rene91970;Persist Security Info=True;User ID=user;Initial Catalog=7TMobileGeneral;Data Source=192.168.3.222");
           conn.Open();
           SqlTransaction trans = conn.BeginTransaction();

           SqlCommand cmd = new SqlCommand("EXEC NewInsertCommand @userid, @product, @phonebrand, @phonemodel, @OSVersion, @phoneserial, @phonenumber, @IPAddress, @date, @time, @GeoA, @GeoB", conn);
           cmd.Transaction = trans;

           SqlConnection conn2 = new SqlConnection("Password=tony6472rene91970;Persist Security Info=True;User ID=user;Initial Catalog=pikling;Data Source=192.168.3.222");
           conn2.Open();
           SqlTransaction trans2 = conn2.BeginTransaction();

           SqlCommand cmd2 = new SqlCommand("EXEC NewInsertCommand @userid, @langin, @langout, @textin, @textout, @phonemodel, @phonebrand, @imageinpath, @imageoutpath, @docpdfoutpath, @IPAddress, @date, @time, @phoneserial, @phonenumber, @OSVersion, @GeoA, @GeoB",conn2);
           cmd2.Transaction = trans2;


           try
           {    cmd.Parameters.Clear();
                cmd.Parameters.Add(new SqlParameter("@userid", userid));
                cmd.Parameters.Add(new SqlParameter("@product", product));
                cmd.Parameters.Add(new SqlParameter("@phonebrand", phonebrand));
                cmd.Parameters.Add(new SqlParameter("@phonemodel", phonemodel));
                cmd.Parameters.Add(new SqlParameter("@OSVersion", OSVersion));
                cmd.Parameters.Add(new SqlParameter("@phoneserial", phoneserial));
                cmd.Parameters.Add(new SqlParameter("@phonenumber", phonenumber));
                cmd.Parameters.Add(new SqlParameter("@IPAddress", IPAddress));
                cmd.Parameters.Add(new SqlParameter("@date", date));
                cmd.Parameters.Add(new SqlParameter("@time", time));
                cmd.Parameters.Add(new SqlParameter("@GeoA", GeoA));
                cmd.Parameters.Add(new SqlParameter("@GeoB", GeoB));

                cmd.ExecuteNonQuery();
                trans.Commit();
                trans = conn.BeginTransaction();
                cmd.Transaction = trans;

           }
           finally
           {
               if (trans != null) 
                   trans.Commit();
               conn.Close();
               trans.Dispose();
               conn.Dispose();
               trans = null;
               conn = null;
           }
           try
           {   cmd2.Parameters.Clear();
               cmd2.Parameters.Add(new SqlParameter("@userid", userid));
               cmd2.Parameters.Add(new SqlParameter("@langin", langin));
               cmd2.Parameters.Add(new SqlParameter("@langout", langout));
               cmd2.Parameters.Add(new SqlParameter("@textin", textin));
               cmd2.Parameters.Add(new SqlParameter("@textout", textout));
               cmd2.Parameters.Add(new SqlParameter("@phonemodel", phonemodel));
               cmd2.Parameters.Add(new SqlParameter("@phonebrand", phonebrand));
               cmd2.Parameters.Add(new SqlParameter("@imageinpath", imageinpath));
               cmd2.Parameters.Add(new SqlParameter("@imageoutpath",imageoutpath));
               cmd2.Parameters.Add(new SqlParameter("@docpdfoutpath",docpdcoutpath));
               cmd2.Parameters.Add(new SqlParameter("@IPAddress", IPAddress));
               cmd2.Parameters.Add(new SqlParameter("@date", date));
               cmd2.Parameters.Add(new SqlParameter("@time", time));
               cmd2.Parameters.Add(new SqlParameter("@phoneserial", phoneserial));
               cmd2.Parameters.Add(new SqlParameter("@phonenumber", phonenumber));
               cmd2.Parameters.Add(new SqlParameter("@OSVersion", OSVersion));
               cmd2.Parameters.Add(new SqlParameter("@GeoA", GeoA));
               cmd2.Parameters.Add(new SqlParameter("@GeoB", GeoB));

               cmd2.ExecuteNonQuery();
               trans2.Commit();
               trans2 = conn2.BeginTransaction();
               cmd2.Transaction = trans2;
           }
           finally
           {
               if (trans2 != null) 
                   trans2.Commit();
               conn2.Close();
               trans2.Dispose();
               conn2.Dispose();
               trans2 = null;
               conn2 = null;
           }
        }
    }    
}
