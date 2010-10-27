using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Net.Mail;
using Microsoft.Office.Interop.Word;
using System.Windows.Forms;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace Plinking_Server
{
    class DocCreator
    {
        String _sServerSmtp = "smtpout.secureserver.net";

        /// <summary>
        /// Create doc file with name pliking.doc into the application folder
        /// </summary>
        /// <param name="sStrSource">Text Language source</param>
        /// <param name="sStrDest">Text Language dest</param>
        /// <param name="sFileName">File name doc to create</param>
        /// <returns>true if the doc file is created</param>        
        public bool CreateDocFile(String sStrSource, String sStrDest, String sFileName)
        {
            object missing = System.Reflection.Missing.Value;
            object Visible=true;
            object start1 = 0;
            object end1 = 0;
            bool bRet = false;


            ApplicationClass WordApp = new ApplicationClass();
            Microsoft.Office.Interop.Word.Document adoc = WordApp.Documents.Add(ref missing, ref missing, ref missing, ref missing);
            Range rng = adoc.Range(ref start1, ref missing);
            String sFolder = sFileName.Substring(0, sFileName.LastIndexOf('\\'));
 
            try
            {              
                Object oSaveWithDocument = true;

                adoc.Tables.Add(rng, 3, 1, ref missing, ref missing);
                Range rngPic = adoc.Tables[1].Range;

                rngPic.InlineShapes.AddPicture(System.Windows.Forms.Application.StartupPath + "\\logo.jpg",
                    ref missing, ref oSaveWithDocument, ref missing);
                
                rng.Font.Name = "Verdana";
                rng.Font.Size = 12;
                rng.InsertAfter(sStrSource);
                rng.InsertAfter(sStrDest);

                foreach (Microsoft.Office.Interop.Word.Section section in adoc.Sections)
                {
                    object fieldEmpty = Microsoft.Office.Interop.Word.WdFieldType.wdFieldEmpty;
                    section.Headers[Microsoft.Office.Interop.Word.WdHeaderFooterIndex.wdHeaderFooterPrimary].Range.Font.Name = "Verdana";
                    section.Headers[Microsoft.Office.Interop.Word.WdHeaderFooterIndex.wdHeaderFooterPrimary].Range.Font.Size = 14;
                    section.Headers[Microsoft.Office.Interop.Word.WdHeaderFooterIndex.wdHeaderFooterPrimary].Range.Font.Bold = 1;
                    section.Headers[Microsoft.Office.Interop.Word.WdHeaderFooterIndex.wdHeaderFooterPrimary].Range.InsertAfter("http://www.7touchgroup.com/");
                    section.Headers[Microsoft.Office.Interop.Word.WdHeaderFooterIndex.wdHeaderFooterPrimary]
                    .Range.ParagraphFormat.Alignment = Microsoft.Office.Interop.Word.WdParagraphAlignment.wdAlignParagraphCenter;
                }

                object filename = sFileName;
                adoc.SaveAs(ref filename, ref missing, ref missing, ref missing, ref missing, ref missing,
                ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing);
                //WordApp.Visible = true;
                bRet = true;
                adoc.Close(ref oSaveWithDocument, ref missing, ref missing);
            }
            catch (Exception ex)
            {
                Program.MainForm.AddLog("Exception:" + ex.Message, sFolder);
            }
            return bRet;
        }
        /// <summary>
        /// Create txt file with name pliking.txt into the application folder
        /// </summary>
        /// <param name="sStrSource">Text Language source</param>
        /// <param name="sStrDest">Text Language dest</param>
        /// <param name="sFileName">File name txt to create</param>
        /// <returns>true if the txt file is created</param>        
        public bool CreateTxtFile(String sStrSource, String sStrDest, String sFileName)
        {
            bool bret=false;
            String sFolder = sFileName.Substring(0, sFileName.LastIndexOf('\\'));
            try
            {
                TextWriter tr1 = new StreamWriter(sFileName);
                tr1.WriteLine(sStrSource);
                tr1.WriteLine("");
                tr1.WriteLine("");
                tr1.WriteLine("");
                tr1.WriteLine(sStrDest);
                tr1.Close();
                bret = true;
            }
            catch (Exception ex)
            {
                Program.MainForm.AddLog("Exception:" + ex.Message, sFolder);
            }
            return bret;
        }

        public bool CreatePdfFile(String sStrSource, String sStrDest, String sFileName)
        {
            bool bRet = false;
            // step 1: creation of a document-object
            iTextSharp.text.Document document = new iTextSharp.text.Document();

            try
            {
                // step 2:
                // we create a writer that listens to the document
                // and directs a PDF-stream to a file
                PdfWriter.GetInstance(document, new FileStream(sFileName, FileMode.Create));

                // step 3: we open the document
                document.Open();

                // step 4: we add content to the document
                BaseFont bfComic = BaseFont.CreateFont("c:\\windows\\fonts\\comic.ttf", BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                iTextSharp.text.Font font = new iTextSharp.text.Font(bfComic, 12);
                //document.Add(new iTextSharp.text.Paragraph(sStrSource, font));
                document.Add(new iTextSharp.text.Paragraph(sStrDest, font));
                bRet = true;
            }
            catch (DocumentException de)
            {
                Console.Error.WriteLine(de.Message);
            }
            catch (IOException ioe)
            {
                Console.Error.WriteLine(ioe.Message);
            }
            
            document.Close();
            return bRet;
        }



        /// <summary>
        /// Create pdf file with name pliking.pdf into the application folder
        /// </summary>
        /// <param name="sStrSource">Text Language source</param>
        /// <param name="sStrDest">Text Language dest</param>
        /// <param name="sFileName">File name pdf to create</param>
        /// <returns>true if the pdf file is created</param>        
        public bool CreatePdfFileOld(String sStrSource, String sStrDest,  String sFileName)
        {/*
            bool bret=false;
            String sFolder="";
            try
            {
                sFolder = sFileName.Substring(0, sFileName.LastIndexOf('\\'));

                PdfDocument _document = new PdfDocument();
                _document = new PdfDocument();
                _document.Info.Title = "Pikling";
                _document.Info.Author = "http://www.7touchgroup.com/";
                _document.Info.Subject = "";
                _document.Info.Keywords = "";

                PdfPage page = _document.AddPage();
                XGraphics gfxG = XGraphics.FromPdfPage(page);
                
                XImage image = XImage.FromFile(System.Windows.Forms.Application.StartupPath + "\\logo.jpg");
                // Left position in point
                double x = (250 - image.PixelWidth * 72 / image.HorizontalResolution) / 2;
                gfxG.DrawImage(image, x, 0);
                DrawTitle(page, gfxG, "http://www.7touchgroup.com/");
                XPdfFontOptions options = new XPdfFontOptions(PdfFontEncoding.Unicode, PdfFontEmbedding.Always);
                string facename = "Verdana";
                XFont fontRegular = new XFont(facename, 14, XFontStyle.Regular, options);

                String sStr = sStrSource;
                int iY = 200;
                int i;
                for (i = 0; i < 2; i++)
                {
                    if (i == 1)
                    {   sStr = sStrDest;
                        iY += 30;
                    }

                    String sStrLine = "";
                    string[] myFields = sStr.Split(' ');
                    
                    int iCharForRow = 60;
                    foreach (String sf in myFields)
                    {
                        if (sf.Length + sStrLine.Length >= iCharForRow || sf.LastIndexOf('\r') >= 0)
                        {
                            gfxG.DrawString(sStrLine, fontRegular, XBrushes.DarkSlateGray, 20, iY);
                            sStrLine = sf + " ";
                            iY += 20;
                        }
                        else
                            sStrLine += sf + " ";
                    }
                    if (sStrLine != String.Empty)
                        gfxG.DrawString(sStrLine, fontRegular, XBrushes.DarkSlateGray, 20, iY);
                }
                // Save the document...
                _document.Save(sFileName);
                // show the pdf
                //Process.Start(sFileName);
                _document.Close();
                bret = true;
            }
            catch (Exception ex)
            {
                Program.MainForm.AddLog("CreatePdfFile Exception:" + ex.Message, sFolder);
            }
            return bret;*/
            return true;
        }
        /// <summary>
        /// Send an email
        /// </summary>
        /// <param name="sEmailAddr">Email to</param>
        /// <param name="sFileImg">file name image to attach. If empty do not attach nothing</param>
        /// <param name="sFileDoc">file name doc to attach. If empty do not attach nothing</param>
        /// <param name="sFilePdf">file name pdf to attach. If empty do not attach nothing</param>
        /// <param name="sFileTxt">file name txt to attach. If empty do not attach nothing</param>
        /// <returns>true if the email was sent</param>        
        public bool SendEmailTo(String sEmailAddr, String sFileImg, String sFileDoc, String sFilePdf, String sFileTxt)
        {
            bool bret = false;
            String sFolder = "";
            try
            {
                sFolder = sFileImg.Substring(0, sFileImg.LastIndexOf('\\'));
                // create mail message object
                System.Net.Mail.MailMessage message = new System.Net.Mail.MailMessage(
                   "noreply@pikling.com",
                   sEmailAddr,
                   "Pikling Notification",
                   "See the attached doc");

                /*MailAddress copy = new MailAddress("nystrom.anthony@gmail.com");
                message.CC.Add(copy);*/
                /*MailAddress copy = new MailAddress("mantovani.alex@gmail.com");
                message.CC.Add(copy);*/

                if (sFileImg != String.Empty)
                {
                    Attachment data = new Attachment(sFileImg);
                    message.Attachments.Add(data);
                }
                System.Windows.Forms.Application.DoEvents();
                if (sFileDoc != String.Empty)
                {
                    Attachment data = new Attachment(sFileDoc);
                    message.Attachments.Add(data);
                }
                System.Windows.Forms.Application.DoEvents();
                if (sFilePdf != String.Empty)
                {
                    Attachment data = new Attachment(sFilePdf);
                    message.Attachments.Add(data);
                }
                System.Windows.Forms.Application.DoEvents();
                //Send the message.
                SmtpClient client = new SmtpClient(_sServerSmtp);
                // Add credentials if the SMTP server requires them.
                System.Net.NetworkCredential cr = new System.Net.NetworkCredential("noreply@pikling.com", "tony6472");
                client.UseDefaultCredentials = false;
                client.Credentials = cr;
                client.Send(message);
                bret = true;
            }
            catch (Exception ex)
            {
                Program.MainForm.AddLog("Exception:" + ex.Message, sFolder);
            }
            return bret;
        }

        /// <summary>
        /// Draws the page title and footer.
        /// </summary>
        private void DrawTitle(PdfSharp.Pdf.PdfPage page, XGraphics gfx, string title)
        {
            XRect rect = new XRect(new XPoint(), gfx.PageSize);
            rect.Inflate(-10, -15);
            XFont font = new XFont("Verdana", 14, XFontStyle.Bold);
            gfx.DrawString(title, font, XBrushes.MidnightBlue, rect, XStringFormats.TopCenter);

            //_document.Outlines.Add(title, page, true);
        }
    }
}
