using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Collections;
using Leadtools.Forms.Ocr;
using Leadtools.Forms.DocumentWriters;
using Leadtools.Codecs;
using Leadtools;
using System.Globalization;

namespace Plinking_Server
{
    public partial class PlikingServerMain : Form
    {
        TCPServer _sockServer;
        ArrayList _arrDataProc = new ArrayList();
        // System.IO
        FileSystemWatcher _watchFiles = new FileSystemWatcher();

        
        // The OCR engine instance used in this demo
        public IOcrEngine _ocrEngine;
        public RasterCodecs _codecs;

        public PlikingServerMain()
        {
            InitializeComponent();
            
            Support.Unlock(false);
            _ocrEngine = OcrEngineManager.CreateEngine(OcrEngineType.Professional, false);
            _ocrEngine.Startup(null, null, null, null);
            RasterCodecs.Startup();
            _codecs = new RasterCodecs();
            LeadInitLanguageSupported();
            // Add some Language Plus Characters.
            StringBuilder sb = new StringBuilder();
            char[] langPlusCharacters = { (char)0x2446, (char)0x2447, (char)0x2448, (char)0x2449, (char)0x0 };
            sb.Append(langPlusCharacters);
            _ocrEngine.SettingManager.SetStringValue("Language.LanguagesPlusCharacters", sb.ToString());

            Thread th = new Thread(new ThreadStart(DoSplash));
            //th.ApartmentState = ApartmentState.STA;
            //th.IsBackground=true;
            th.Start();
            Thread.Sleep(2000);
            th.Abort();
            Thread.Sleep(1000);
        }
        /// <summary>
        /// This will start the activity-monitoring of a folder
        /// </summary>
        private void startActivityMonitoring(string sPath)
        {
            if (sPath.Length < 3)
            {
                MessageBox.Show("You have to enter a folder to monitor.",
                    "Hey..!", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
            else
            {
                // This is the path we want to monitor
                _watchFiles.Path = sPath;
                _watchFiles.Filter = "*.txt";
                _watchFiles.IncludeSubdirectories = true;
                _watchFiles.NotifyFilter = System.IO.NotifyFilters.DirectoryName;
                _watchFiles.NotifyFilter = _watchFiles.NotifyFilter | System.IO.NotifyFilters.FileName;
                _watchFiles.NotifyFilter = _watchFiles.NotifyFilter | System.IO.NotifyFilters.Attributes;
                _watchFiles.Created += new FileSystemEventHandler(eventRaisedFiles);
                _watchFiles.Renamed += new RenamedEventHandler(eventRaisedFiles);
                _watchFiles.Deleted += new FileSystemEventHandler(eventRaisedFiles);

                // And at last.. We connect our EventHandles to the system API (that is all
                // wrapped up in System.IO)
                try
                {
                    _watchFiles.EnableRaisingEvents = true;
                }
                catch (ArgumentException)
                {

                }
            }
        }

        /// <summary>
        /// This just stops the monitoring process.
        /// </summary>
        private void stopActivityMonitoring()
        {
            _watchFiles.EnableRaisingEvents = false;
        }
        /// <summary>
        /// Triggerd when an event is raised from the folder acitivty monitoring.
        /// All types exists in System.IO
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">containing all data send from the event that got executed.</param>
        private void eventRaisedFiles(object sender, System.IO.FileSystemEventArgs e)
        {
            String path = e.FullPath;
            switch (e.ChangeType)
            {
                case WatcherChangeTypes.Deleted:
                    int limit = path.LastIndexOf('\\');
                    string folder = path.Substring(0, limit);
                    if (folder.Length < 1)
                        folder = path.Substring(0, (path.Length - 1));
                    limit = folder.LastIndexOf('\\') + 1;
                    folder = folder.Substring(limit);
                    if (folder.Length < 1)
                        folder = path.Substring(0, (folder.Length - 1));
                    break;
                case WatcherChangeTypes.Changed:
                case WatcherChangeTypes.Renamed:
                case WatcherChangeTypes.Created:
                    limit = path.LastIndexOf('\\');
                    folder = path.Substring(0, limit);
                    if (folder.Length < 1)
                        folder = path.Substring(0, (path.Length - 1));
                    limit = folder.LastIndexOf('\\') + 1;
                    folder = folder.Substring(limit);
                    if (folder.Length < 1)
                        folder = path.Substring(0, (folder.Length - 1));

                    limit = path.LastIndexOf('\\') + 1;
                    string sfile = path.Substring(limit);
                    if (sfile.Length < 1)
                        sfile = path.Substring(0, (path.Length - 1));
                    if (sfile.ToLower() == "sending.txt")
                        SendEmail(Application.StartupPath + "\\" + folder);

                    break;
                default: // Another action
                    break;
            }
        }
        private bool SendEmail(String sFolder)
        {
            bool bret = false;
            try
            {
                String sFile = sFolder + "\\sending.txt";
                String sEmailData = "";
                Thread.Sleep(500);
                TextReader tr = new StreamReader(sFile);
                sEmailData = tr.ReadLine();
                tr.Close();

                String sDataDest = "";
                String sDataSource = "";
                sFile = sFolder + "\\source.dat";
                tr = new StreamReader(sFile, System.Text.Encoding.UTF8);
                sDataSource = tr.ReadLine();
                tr.Close();
                sFile = sFolder + "\\dest.dat";
                tr = new StreamReader(sFile, System.Text.Encoding.UTF8);
                sDataDest = tr.ReadLine();
                tr.Close();

                String sDocType = sEmailData.Substring(11, 3);
                String sToAddr = sEmailData.Substring(15);
                String sFileJpg = sFolder + "\\pikling.jpg";
                String sFileDoc = "";
                String sFilePdf = "";
                String sFileTxt = "";
                DocCreator doccre = new DocCreator();

                switch (sDocType.ToLower())
                {
                    case "pdf":
                        Program.MainForm.AddLog("Preparing file PDF...", sFolder);
                        sFilePdf = sFolder + "\\Pikling.pdf";
                        if (!doccre.CreatePdfFile(sDataSource, sDataDest, sFilePdf))
                        {
                            sFilePdf = "";
                            Program.MainForm.AddLog("Pdf Creator failed", sFolder);
                        }
                        else
                            Program.MainForm.AddLog("Pdf created", sFolder);
                        break;
                    case "all":
                        Program.MainForm.AddLog("Preparing file PDF...", sFolder);
                        sFilePdf = sFolder + "\\Pikling.pdf";
                        if (!doccre.CreatePdfFile(sDataSource, sDataDest, sFilePdf))
                        {
                            sFilePdf = "";
                            Program.MainForm.AddLog("Pdf Creator failed", sFolder);
                        }
                        /*sFilePdf = sFolder + "\\Pikling.doc";
                        Program.MainForm.AddLog("Preparing file DOC...", sFolder);
                        if (!doccre.CreateDocFile(sDataSource, sDataDest, sFileDoc))
                        {
                            sFileDoc = "";
                            Program.MainForm.AddLog("Doc Creator failed", sFolder);
                        }*/
                        break;

                    case "txt":
                        sFileTxt = sFolder + "\\Pikling.txt";
                        Program.MainForm.AddLog("Preparing file TXT...", sFolder);
                        if (!doccre.CreateTxtFile(sDataSource, sDataDest, sFileTxt))
                        {
                            sFileTxt = "";
                            Program.MainForm.AddLog("Txt Creator failed", sFolder);
                        }
                        break;
                    case "doc":
                        sFilePdf = sFolder + "\\Pikling.doc";
                        Program.MainForm.AddLog("Preparing file DOC...", sFolder);
                        if (!doccre.CreateDocFile(sDataSource, sDataDest, sFileDoc))
                        {
                            sFileDoc = "";
                            Program.MainForm.AddLog("Doc Creator failed", sFolder);
                        }
                        break;
                }

                Program.MainForm.AddLog("Sending EMAIL...", sFolder);
                Application.DoEvents();
                if (!doccre.SendEmailTo(sToAddr, sFileJpg, sFileDoc, sFilePdf, sFileTxt))
                    Program.MainForm.AddLog("Send email failed", sFolder);
                else
                {
                    sFile = sFolder + "\\sent.txt";
                    TextWriter tr1 = new StreamWriter(sFile);
                    sEmailData = "Completed";
                    tr1.WriteLine(sEmailData);
                    tr1.Close();
                    bret = true;
                    Program.MainForm.AddLog("EMAIL SENT CORRECTLY", sFolder);
                }
            }
            catch (Exception ex)
            {
                Program.MainForm.AddLog(String.Format("SendEmail Exception :{0}", ex.Message), sFolder);
            }
            return bret;
        }

        public string StrRight(string param, int length)
        {
            //start at the index based on the lenght of the sting minus
            //the specified lenght and assign it a variable
            string result = param.Substring(param.Length - length, length);
            //return the result of the operation
            return result;
        }

        delegate void SetImg1Callback(Image img, String sFolder);
        public void SetImg1(Image img, String sFolder)
        {
            try
            {
                if (!pictureBox1.InvokeRequired)
                {   pictureBox1.Image = img;
                    pictureBox1.Tag = sFolder;
                    AddLog("WIDHT:" + img.Width.ToString() + " HEIGHT:" + img.Height.ToString(), sFolder);
                }
                else
                {
                    SetImg1Callback d = new SetImg1Callback(SetImg1);
                    Invoke(d, new object[] { img, sFolder });
                }
            }
            catch (Exception ex)
            {
                AddLog("Exception SetImg1:" + ex.Message, sFolder);
            }
        }
        delegate void SetImg2Callback(Image img, String sFolder);
        public void SetImg2(Image img, String sFolder)
        {
            try
            {
                if (!pictureBox2.InvokeRequired)
                {   pictureBox2.Image = img;
                    pictureBox2.Tag = sFolder;
                }
                else
                {
                    SetImg2Callback d = new SetImg2Callback(SetImg2);
                    Invoke(d, new object[] { img, sFolder });
                }
            }
            catch (Exception ex)
            {
                AddLog("Exception SetImg2:" + ex.Message, sFolder);
            }
        }
        delegate void SetImg3Callback(Image img, String sFolder);
        public void SetImg3(Image img, String sFolder)
        {
            try
            {
                if (!pictureBox3.InvokeRequired)
                {   pictureBox3.Image = img;
                    pictureBox3.Tag = sFolder;
                }
                else
                {
                    SetImg3Callback d = new SetImg3Callback(SetImg3);
                    Invoke(d, new object[] { img, sFolder });
                }
            }
            catch (Exception ex)
            {
                AddLog("Exception SetImg3:" + ex.Message, sFolder);
            }
        }
        delegate void SetImg4Callback(Image img, String sFolder);
        public void SetImg4(Image img, String sFolder)
        {
            try
            {
                if (!pictureBox4.InvokeRequired)
                {   pictureBox4.Image = img;
                    pictureBox4.Tag = sFolder;
                }
                else
                {
                    SetImg4Callback d = new SetImg4Callback(SetImg4);
                    Invoke(d, new object[] { img, sFolder });
                }
            }
            catch (Exception ex)
            {
                AddLog("Exception SetImg4:" + ex.Message, sFolder);
            }
        }
        delegate void SetImg5Callback(Image img, String sFolder);
        public void SetImg5(Image img, String sFolder)
        {
            try
            {
                if (!pictureBox5.InvokeRequired){
                    pictureBox5.Image = img;
                    pictureBox5.Tag = sFolder;
                }
                else
                {
                    SetImg5Callback d = new SetImg5Callback(SetImg5);
                    Invoke(d, new object[] { img , sFolder});
                }
            }
            catch (Exception ex)
            {
                AddLog("Exception SetImg5:" + ex.Message, sFolder);
            }
        }

        public void NewImage(String sImage, String sFolder)
        {
            try
            {
                SetImg5(pictureBox4.Image, (string)pictureBox4.Tag);
                SetImg4(pictureBox3.Image, (string)pictureBox3.Tag);
                SetImg3(pictureBox2.Image, (string)pictureBox2.Tag);
                SetImg2(pictureBox1.Image, (string)pictureBox1.Tag);
                SetImg1(Image.FromFile(sImage), sFolder);
            }
            catch (Exception ex)
            {
                AddLog("Exception NewImage:" + ex.Message, sFolder);
            }
        }

        private void butLoadImage_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileOpen = new OpenFileDialog();
            fileOpen.Filter = "Jpg Files (*.jpg)|*.jpg|Bmp files (*.bmp)|*.bmp";
            fileOpen.FilterIndex = 0;
            fileOpen.RestoreDirectory = false; //true;
            if (fileOpen.ShowDialog() == DialogResult.OK)
            {
                String sFolder = Application.StartupPath + "\\tmp";
                System.IO.Directory.CreateDirectory(sFolder);
                NewImage(fileOpen.FileName, sFolder);
                LeadWorker ocrWorker = new LeadWorker(_ocrEngine, _codecs);
                String sSrc = ocrWorker.Start(sFolder, fileOpen.FileName, lblOcrFrom.Text);
                AddLog("OCR:" + sSrc, "");
                String sDst = "";
                try
                {   if (sSrc != "")
                    {   TranslatedTranslator tr = new TranslatedTranslator();
                        sDst = tr.Translate(sSrc, new TranslateDir(lblOcrFrom.Text, lblOcrTo.Text));
                    }
                }
                catch (Exception ex)
                {
                    Program.MainForm.AddLog(String.Format("Translated translation failed tray with google, Exception:{0}", ex.Message), "");
                    GoogleTranslator gt = new GoogleTranslator();
                    try
                    {
                        sDst = gt.Translate(sSrc, new TranslateDir(lblOcrFrom.Text, lblOcrTo.Text));
                    }
                    catch (Exception ex2)
                    {
                        Program.MainForm.AddLog(String.Format("Google translation failed Exception:{0}", ex2.Message), "");
                    }
                }
                AddLog("Translated:" + sDst, "");
            }
        }
        private void DoSplash()
        {
            Splash sp = new Splash();
            sp.Show();
            Thread.Sleep(2000);
            sp.Hide();
            sp.Dispose();
        }

        /// <summary>
        /// Delete all thumbs and label controls
        /// </summary>
        private void ResetControls()
        {
            pictureBox1.Image = null;
        }

        /// <summary>
        /// Create a new folder work into app folder. 
        /// </summary>
        /// <param name="sName">Return only the name of folder</param>
        /// <returns>Return the full path of folder created</returns>
        public String GetNewFolder(ref String sName)
        {
            DateTime dateTime = DateTime.Now;
            String sFolderName = Application.StartupPath + "\\" + String.Format("{0:00}", dateTime.Hour) + String.Format("{0:00}", dateTime.Minute) + String.Format("{0:00}", dateTime.Second) + String.Format("{0:0000}", dateTime.Millisecond);
            Directory.CreateDirectory(sFolderName);
            
            int limit = sFolderName.LastIndexOf('\\') + 1;
            sName = sFolderName.Substring(limit, (sFolderName.Length - limit));
            if (sName.Length < 1)
                sName = sFolderName.Substring(0, (sFolderName.Length - 1));

            return sFolderName;
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            stopActivityMonitoring();
            startActivityMonitoring(Application.StartupPath);

            //IPAddress ip = IPAddress.Parse("69.21.114.136");
            //IPAddress ip = IPAddress.Parse("192.168.1.3");
            _sockServer = new TCPServer(new IPEndPoint(IPAddress.Any, 8081));
            _sockServer.StartServer();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            stopActivityMonitoring();
            _codecs.Dispose();
            RasterCodecs.Shutdown();
            _ocrEngine.Shutdown();
            _ocrEngine.Dispose();
            _sockServer.StopServer();
        }
        
        public void AppendNewProcess(String sFolder, String sFileBmp, String sFileJpg, String sLang)
        {
            AddLog("New Process in queue", sFolder);
            DataProc data = new DataProc(sFolder, sFileBmp, sFileJpg, sLang);
            _arrDataProc.Add(data);
        }
        public bool GetDataProc(String sFolder, ref DataProc DataP)
        {
            DataP=null;
            foreach (Object obj in _arrDataProc)
            {
                DataProc d = (DataProc)obj;
                if (d != null && d.getFolder() == sFolder)
                    DataP = d;
            }
            if (DataP!=null)
                return false;
            return true;
        }
        delegate void AddLogCallback(string Message, string sFolder);
        public void AddLog(string Message, string sFolder)
        {
            try
            {
                if (!lstLog.InvokeRequired)
                {
                    DateTime dateTime = DateTime.Now;
                    Message = dateTime.ToShortDateString() + " " + dateTime.Hour + ":" + dateTime.Minute + ":" + dateTime.Second + " " + Message;
                    this.lstLog.Items.Insert(0, Message);
                    if (sFolder != "")
                    {
                        TextWriter tr = new StreamWriter(sFolder+"\\log.txt", true);
                        tr.WriteLine(Message);
                        tr.Close();
                    }
                }
                else
                {
                    AddLogCallback d = new AddLogCallback(AddLog);
                    Invoke(d, new object[] { Message, sFolder });
                }
            }
            catch (Exception)
            {
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            int iNConnections = _sockServer.GetNumberConnections();
            Text = "Pikling Server - Connections : " + iNConnections.ToString();
        }

        private void butClearLog_Click(object sender, EventArgs e)
        {
            lstLog.Items.Clear();
        }

        private void lstLog_SelectedIndexChanged(object sender, EventArgs e)
        {
            try{            
                Clipboard.SetDataObject(lstLog.Text, true);
            }
            catch (Exception)
            {
            }
        }

        private void butGetTextTranslated_Click_1(object sender, EventArgs e)
        {
            String sTran = "";
            String sSource = txtSrc.Text;
            if (radTrans.Checked)
            {
                TranslatedTranslator tr = new TranslatedTranslator();
                try
                {
                    sTran = tr.Translate(sSource, new TranslateDir(txtLanSrc.Text, txLantDest.Text));
                }
                catch (Exception ex)
                {
                    AddLog(String.Format("Exception:{0}", ex.Message),"");
                }
            }
            else
            {
                GoogleTranslator tr = new GoogleTranslator();
                try
                {
                    sTran = tr.Translate(sSource, new TranslateDir(txtLanSrc.Text, txLantDest.Text));
                }
                catch (Exception ex)
                {
                    AddLog(String.Format("Exception:{0}", ex.Message), "");
                }
            }
            txtDest.Text = sTran;
        }
        void LeadInitLanguageSupported()
        {
            // Show languages supported by this engine 
            string[] supportedLanguages = _ocrEngine.LanguageManager.GetSupportedLanguages();

            Console.WriteLine("Supported languages:");
            foreach (string supportedLanguage in supportedLanguages)
            {
                // Get the friendly name of this language using the .NET CultureInfo class 
                CultureInfo ci = new CultureInfo(supportedLanguage);
                Console.WriteLine("  {0} ({1})", supportedLanguage, ci.EnglishName);
            }

            // Check if current culture info language is supported 
            CultureInfo currentCulture = CultureInfo.CurrentCulture;
            string name = currentCulture.TwoLetterISOLanguageName;
            bool supported = _ocrEngine.LanguageManager.IsLanguageSupported(name);
            if (!supported)
            {
                name = currentCulture.Name;
                supported = _ocrEngine.LanguageManager.IsLanguageSupported(name);
            }

            if (supported)
            {
                Console.WriteLine("Current culture is {0}, and it is supported by this OCR engine. Enabling only this language and German now", currentCulture.EnglishName);
                _ocrEngine.LanguageManager.EnableLanguages(new string[] { name, "de" });

                // If this engine does not support enabling multiple languages (currently the LEADTOOLS Advantage OCR engine), then GetEnabledLanguages 
                // will always return an array of 1, make a note of this 
                if (!_ocrEngine.LanguageManager.SupportsEnablingMultipleLanguages)
                    Console.WriteLine("This engine supports enabling only one language at a time, so only the first language we enabled will be used");

                string[] enabledLanguages = _ocrEngine.LanguageManager.GetEnabledLanguages();
                Console.WriteLine("Current enabled languages in the engine are:");
                foreach (string enabledLanguage in enabledLanguages)
                {
                    // Get the friendly name of this language using the .NET CultureInfo class 
                    CultureInfo ci = new CultureInfo(enabledLanguage);
                    Console.WriteLine("  {0} ({1})", enabledLanguage, ci.EnglishName);
                }
            }
            else
                Console.WriteLine("Current culture is {0}, and it is not supported by this OCR engine", currentCulture.EnglishName);
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start((string)pictureBox1.Tag);
            }
            catch (Exception) { }
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            try {
                System.Diagnostics.Process.Start((string)pictureBox2.Tag);
            }
            catch (Exception) { }
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start((string)pictureBox3.Tag);
            }
            catch (Exception) { }

        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start((string)pictureBox4.Tag);
            }
            catch (Exception) { }

        }

        private void pictureBox5_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start((string)pictureBox5.Tag);
        }


        
    }
    
}
