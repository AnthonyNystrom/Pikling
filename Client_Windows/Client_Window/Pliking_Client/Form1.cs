using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Threading;
using System.Drawing.Imaging;
using System.Net.Sockets;

namespace Pliking_Client
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private string StrRight(string param, int length)
        {
            //start at the index based on the lenght of the sting minus
            //the specified lenght and assign it a variable
            string result = param.Substring(param.Length - length, length);
            //return the result of the operation
            return result;
        }

        private void butLoadImg_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileOpen = new OpenFileDialog();
            //fileOpen.Filter = "Jpg Files (*.jpg)|*.jpg|Bmp files (*.bmp)|*.bmp";
            fileOpen.Filter = "Jpg Files (*.jpg)|*.jpg";
            fileOpen.FilterIndex = 0;
            fileOpen.RestoreDirectory = true;
            if (fileOpen.ShowDialog() == DialogResult.OK)
            {
                txtResult.Text = "";
                pictureBox1.Image = Image.FromFile(fileOpen.FileName);
                String sFiletmp = Application.StartupPath + "\\" + "tmp.jpg";
                pictureBox1.Image.Save(sFiletmp, ImageFormat.Jpeg);
                lblFile.Text = sFiletmp;
                /*String sFileBmp;
                if (StrRight(fileOpen.FileName, 3) == "jpg") // is a jpg ?
                {   // convert in bmp. Modi work with bmp
                    Image img = Image.FromFile(fileOpen.FileName);
                    sFileBmp = Application.StartupPath + "\\" + "tmp.bmp";
                    img.Save(sFileBmp, ImageFormat.Bmp);
                }
                else
                    sFileBmp = fileOpen.FileName;
                lblFile.Text = sFileBmp;*/
                
                butSend.Enabled = true;
            }
        }
        public static void Receive(Socket socket, byte[] buffer, int offset, int size, int timeout)
        {
            int startTickCount = Environment.TickCount;
            int received = 0;  // how many bytes is already received
            do
            {
                if (Environment.TickCount > startTickCount + timeout)
                    throw new Exception("Timeout.");
                try
                {
                    received += socket.Receive(buffer, offset + received, size - received, SocketFlags.None);
                }
                catch (SocketException ex)
                {
                    if (ex.SocketErrorCode == SocketError.WouldBlock ||
                        ex.SocketErrorCode == SocketError.IOPending ||
                        ex.SocketErrorCode == SocketError.NoBufferSpaceAvailable)
                    {
                        // socket buffer is probably empty, wait and try again
                        Thread.Sleep(30);
                    }
                    else
                        throw ex;  // any serious error occurr
                }
            } while (received < size);
        }
        private void butSend_Click(object sender, EventArgs e)
        {
            try{
                txtResult.Text = "";
                this.Cursor = Cursors.WaitCursor;
                int TIME_OUT = 30000;
                System.IO.FileInfo fi = new System.IO.FileInfo(@lblFile.Text);
                Byte[] byBuffer = new Byte[fi.Length];
                Byte[] byRx = new Byte[1024];

                TcpClient myclient = new TcpClient();
                myclient.Connect(txtIp.Text, Convert.ToInt32(txtPort.Text));

                System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
                Byte[] byBufLan = encoding.GetBytes(txtLang.Text);
                Byte[] byPrefix = new Byte[1];
                byPrefix[0] = 0;
                myclient.Client.Send(byPrefix, 1, SocketFlags.None);
                Receive(myclient.Client, byBuffer, 0, 1, TIME_OUT);

                myclient.Client.Send(byBufLan, 5, SocketFlags.None);
                Receive(myclient.Client, byBuffer, 0, 10, TIME_OUT);
                String sIDProc = encoding.GetString(byBuffer, 0, 10);
                
                long iLen = fi.Length;
                byBuffer[3] = (byte)(iLen>>24);
                byBuffer[2] = (byte)((iLen>>16) & 0xFF);
                byBuffer[1] = (byte)((iLen >> 8) & 0xFF);
                byBuffer[0] = (byte)(iLen & 0xFF);
                // send header
                myclient.Client.Send(byBuffer,4, SocketFlags.None);
                Receive(myclient.Client, byRx, 0, 4, TIME_OUT);
                if (byRx[0]!=byBuffer[0] || byRx[1]!=byBuffer[1] || byRx[2]!=byBuffer[2] || byRx[3]!=byBuffer[3])
                    throw new Exception("Bad Answer from server");

                FileStream fileStream = new FileStream(@lblFile.Text, FileMode.Open);
                fileStream.Read(byBuffer, (int)0, (int)iLen);
                fileStream.Close();
                myclient.Client.Send(byBuffer, (int)iLen, SocketFlags.None);
                Receive(myclient.Client, byRx,0,1,TIME_OUT);
                Receive(myclient.Client, byRx, 0, 4, TIME_OUT);
                iLen = byRx[3];iLen <<= 8;
                iLen |= byRx[2];iLen <<= 8;
                iLen |= byRx[1];iLen <<= 8;
                iLen |= byRx[0];
                if (iLen > 0)
                {
                    Byte[] byData = new Byte[iLen + 1];
                    Receive(myclient.Client, byData, (int)0, (int)iLen, (int)TIME_OUT);
                    txtResult.Text  = System.Text.ASCIIEncoding.ASCII.GetString(byData);                    
                }
                else
                    txtResult.Text = "OCR FAILED";

                byBuffer[0]=1;
                myclient.Client.Send(byBuffer, (int)1, SocketFlags.None);
                // wait len data
                Receive(myclient.Client, byRx, 0, 5, TIME_OUT);
                iLen = byRx[4];iLen <<= 8;
                iLen |= byRx[3];iLen <<= 8;
                iLen |= byRx[2];iLen <<= 8;
                iLen |= byRx[1];
                // translator used byRx[0]
                if (iLen > 0)
                {
                    Byte[] byData = new Byte[iLen + 1];
                    // receive the data answer
                    Receive(myclient.Client, byData, (int)0, (int)iLen, (int)TIME_OUT);
                    txtResultDest.Text  = System.Text.ASCIIEncoding.ASCII.GetString(byData);
                    UTF8Encoding temp = new UTF8Encoding(true);
                    txtResultDest.Text = temp.GetString(byData);
                }
                else
                    txtResultDest.Text = "OCR FAILED";
                byBuffer[0]=1;
                myclient.Client.Send(byBuffer, (int)1, SocketFlags.None);
                Thread.Sleep(1000);

                // invio email
                byPrefix[0] = 1;
                myclient.Client.Send(byPrefix, 1, SocketFlags.None);
                Receive(myclient.Client, byBuffer, 0, 1, TIME_OUT);
                sIDProc += "|PDF|fala70@gmail.com";
                iLen = sIDProc.Length;
                byBuffer[1] = (byte)((iLen >> 8) & 0xFF);
                byBuffer[0] = (byte)(iLen & 0xFF);
                myclient.Client.Send(byBuffer, 2, SocketFlags.None);
                Receive(myclient.Client, byBuffer, 0, 2, TIME_OUT);

                Byte[] byDataEmail = encoding.GetBytes(sIDProc);
                myclient.Client.Send(byDataEmail, (int)iLen, SocketFlags.None);
                Receive(myclient.Client, byBuffer, 0, 1, TIME_OUT);
                
                Thread.Sleep(1000);
                myclient.Close();
                

            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            this.Cursor = Cursors.Default;
        }

        private void txtResult_TextChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {

        }
    }
}
