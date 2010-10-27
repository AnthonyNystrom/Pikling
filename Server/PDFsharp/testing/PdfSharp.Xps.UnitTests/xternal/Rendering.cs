using System;
using System.Diagnostics;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using System.IO;
using System.Xml;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Controls;
using PdfSharp.Pdf;
using PdfSharp.Xps.UnitTests.Helpers;
using PdfSharp.Xps.XpsModel;
using PdfSharp.Xps.Rendering;
using IOPath = System.IO.Path;

namespace PdfSharp.Xps.UnitTests.Xternal
{
  #if true
  /// <summary>
  /// 
  /// </summary>
  [TestClass]
  public class Rendering : TestBase
  {
    public TestContext TestContext { get; set; }

    [TestInitialize]
    public void TestInitialize()
    {
    }

    [TestCleanup]
    public void TestCleanup()
    {
    }

    [TestMethod]
    public void TestExternalXpsFiles()
    {
      string currentDirectory = Directory.GetCurrentDirectory();
#if true
      string[] files = Directory.GetFiles("../../../../../testing/PdfSharp.Xps.UnitTests/xternal", "*.xps", SearchOption.AllDirectories);
#else
      string[] files = Directory.GetFiles("../../../XPS-TestDocuments", "*.xps", SearchOption.AllDirectories);
#endif
      
      if (files.Length == 0)
        throw new Exception("No sample file found.");

      foreach (string filename in files)
      {
        // No negative tests here
        if (filename.Contains("\\ConformanceViolations\\"))
          continue;

        Debug.WriteLine(filename);
        try
        {
          int docIndex = 0;
          XpsDocument xpsDoc = XpsDocument.Open(filename);
          foreach (FixedDocument doc in xpsDoc.Documents)
          {
            PdfDocument pdfDoc = new PdfDocument();
            PdfRenderer renderer = new PdfRenderer();

            int pageIndex = 0;
            foreach (FixedPage page in doc.Pages)
            {
              Debug.WriteLine(String.Format("  doc={0}, page={1}", docIndex, pageIndex));
              PdfPage pdfPage = renderer.CreatePage(pdfDoc, page);
              renderer.RenderPage(pdfPage, page);
              pageIndex++;
            }

            string pdfFilename = IOPath.GetFileNameWithoutExtension(filename);
            if (docIndex != 0)
              pdfFilename += docIndex.ToString();
            pdfFilename += ".pdf";
            pdfFilename = IOPath.Combine(IOPath.GetDirectoryName(filename), pdfFilename);

            pdfDoc.Save(pdfFilename);
            docIndex++;
          }
        }
        catch (Exception ex)
        {
          Debug.WriteLine(ex.Message);
          GetType();
        }
      }
    }
  }
#endif
}