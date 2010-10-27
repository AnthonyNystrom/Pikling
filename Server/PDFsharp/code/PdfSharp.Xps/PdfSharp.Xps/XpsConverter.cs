using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.IO.Packaging;
using PdfSharp.Xps.XpsModel;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Drawing;
using PdfSharp.Drawing.Pdf;
using PdfSharp.Xps.Rendering;
using IOPath = System.IO.Path;

namespace PdfSharp.Xps
{
  /// <summary>
  /// Main class that provides the functionallity to convert an XPS file into a PDF file.
  /// </summary>
  class XpsConverter
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="XpsConverter"/> class.
    /// </summary>
    /// <param name="pdfDocument">The PDF document.</param>
    /// <param name="xpsDocument">The XPS document.</param>
    public XpsConverter(PdfDocument pdfDocument, XpsDocument xpsDocument)
    {
      if (pdfDocument == null)
        throw new ArgumentNullException("pdfDocument");
      if (xpsDocument == null)
        throw new ArgumentNullException("xpsDocument");

      this.pdfDocument = pdfDocument;
      this.xpsDocument = xpsDocument;

      Initialize();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="XpsConverter"/> class.
    /// </summary>
    /// <param name="pdfDocument">The PDF document.</param>
    /// <param name="xpsDocumentPath">The XPS document path.</param>
    public XpsConverter(PdfDocument pdfDocument, string xpsDocumentPath)  // TODO: a constructor with an Uri
    {
      if (pdfDocument == null)
        throw new ArgumentNullException("pdfDocument");
      if (String.IsNullOrEmpty(xpsDocumentPath))
        throw new ArgumentNullException("xpsDocumentPath");

      this.pdfDocument = pdfDocument;
      this.xpsDocument = XpsDocument.Open(xpsDocumentPath);

      Initialize();
    }

    void Initialize()
    {
      this.context = new DocumentRenderingContext(this.pdfDocument);
    }

    DocumentRenderingContext Context
    {
      get { return this.context; }
    }
    DocumentRenderingContext context;

    /// <summary>
    /// HACK
    /// </summary>
    public PdfPage CreatePage(int xpsPageIndex)
    {
      FixedPage fixedPage = this.xpsDocument.GetDocument().GetFixedPage(xpsPageIndex);

      PdfPage page = this.pdfDocument.AddPage();
      page.Width = XUnit.FromPresentation(fixedPage.Width);
      page.Height = XUnit.FromPresentation(fixedPage.Height);
      return page;
    }

    /// <summary>
    /// Renders an XPS document page to the specified PDF page.
    /// </summary>
    /// <param name="page">The target PDF page. The page must belong to the PDF document of this converter.</param>
    /// <param name="xpsPageIndex">The zero-based XPS page number.</param>
    public void RenderPage(PdfPage page, int xpsPageIndex)
    {
      if (page == null)
        throw new ArgumentNullException("page");
      if (!Object.ReferenceEquals(page.Owner, this.pdfDocument))
        throw new InvalidOperationException(PSXSR.PageMustBelongToPdfDocument);
      // Debug.Assert(xpsPageIndex==0, "xpsPageIndex must be 0 at this stage of implementation.");
      try
      {
        FixedPage fpage = this.xpsDocument.GetDocument().GetFixedPage(xpsPageIndex);

        // ZipPackage pack = ZipPackage.Open(xpsFilename) as ZipPackage;
        Uri uri = new Uri("/Documents/1/Pages/1.fpage", UriKind.Relative);
        ZipPackagePart part = this.xpsDocument.Package.GetPart(uri) as ZipPackagePart;
        using (Stream stream = part.GetStream())
        {
          using (StreamReader sr = new StreamReader(stream))
          {
            string xml = sr.ReadToEnd();
#if true && DEBUG
            if (!String.IsNullOrEmpty(this.xpsDocument.Path))
            {
              string xmlPath = IOPath.Combine(IOPath.GetDirectoryName(this.xpsDocument.Path), IOPath.GetFileNameWithoutExtension(this.xpsDocument.Path)) + ".xml";
              using (StreamWriter sw = new StreamWriter(xmlPath))
              {
                sw.Write(xml);
              }
            }
#endif
            //XpsElement el = PdfSharp.Xps.Parsing.XpsParser.Parse(xml);
            PdfRenderer renderer = new PdfRenderer();
            renderer.RenderPage(page, fpage);
          }
        }
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    /// <summary>
    /// Gets the PDF document of this converter.
    /// </summary>
    public PdfDocument PdfDocument
    {
      get { return this.pdfDocument; }
    }
    PdfDocument pdfDocument;

    /// <summary>
    /// Gets the XPS document of this converter.
    /// </summary>
    public XpsDocument XpsDocument
    {
      get { return this.xpsDocument; }
    }
    XpsDocument xpsDocument;

    /// <summary>
    /// Converts the specified PDF file into an XPS file. The new file is stored in the same directory.
    /// </summary>
    public static void Convert(string xpsFilename)
    {
      if (String.IsNullOrEmpty(xpsFilename))
        throw new ArgumentNullException("xpsFilename");

      if (!File.Exists(xpsFilename))
        throw new FileNotFoundException("File not found.", xpsFilename);

      string pdfFilename = xpsFilename;
      if (IOPath.HasExtension(pdfFilename))
        pdfFilename = pdfFilename.Substring(0, pdfFilename.LastIndexOf('.'));
      pdfFilename += ".pdf";

      Convert(xpsFilename, pdfFilename, 0);
    }

    /// <summary>
    /// Implements the PDF file to XPS file conversion.
    /// </summary>
    public static void Convert(string xpsFilename, string pdfFilename, int docIndex)
    {
      XpsDocument xpsDocument = null;
      try
      {
        xpsDocument = XpsDocument.Open(xpsFilename);
        FixedDocument fixedDocument = xpsDocument.GetDocument();
        PdfDocument pdfDocument = new PdfDocument();
        PdfRenderer renderer = new PdfRenderer();

        int pageIndex = 0;
        foreach (FixedPage page in fixedDocument.Pages)
        {
          if (page == null)
            continue;
          Debug.WriteLine(String.Format("  doc={0}, page={1}", docIndex, pageIndex));
          PdfPage pdfPage = renderer.CreatePage(pdfDocument, page);
          renderer.RenderPage(pdfPage, page);
          pageIndex++;

#if DEBUG
          // stop at page...
          if (pageIndex == 50)
            break;
#endif
        }
        pdfDocument.Save(pdfFilename);
      }
      catch (Exception ex)
      {
        Debug.WriteLine(ex.Message);
        if (xpsDocument != null)
          xpsDocument.Close();
        throw ex;
      }
      finally
      {
        if (xpsDocument != null)
          xpsDocument.Close();
      }
    }
  }
}