using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
#if GDI
using System.Drawing;
using System.Drawing.Imaging;
#endif
#if WPF
using System.Windows;
using System.Windows.Media;
#endif
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.UnitTests.Helpers;

namespace PdfSharp.UnitTests.Text
{
  /// <summary>
  /// 
  /// </summary>
  [TestClass]
  public class TextStyles : TestBase
  {
    /// <summary>
    /// Gets or sets the test context which provides information about and functionality for the current test run.
    ///</summary>
    public TestContext TestContext { get; set; }

    [TestInitialize()]
    public void TestInitialize()
    {
      BeginPdf();
      BeginImage();
    }

    [TestCleanup()]
    public void TestCleanup()
    {
      EndPdf();
      EndImage();
    }

    [TestMethod]
    public void TestTextStyles()
    {
      Render("TextStyles", RenderTextStyles);
    }

    void RenderTextStyles(XGraphics gfx)
    {
      gfx.TranslateTransform(15, 20);

      string facename = "Times New Roman";
      XFont fontRegular = new XFont(facename, 20);
      XFont fontBold = new XFont(facename, 20, XFontStyle.Bold);
      XFont fontItalic = new XFont(facename, 20, XFontStyle.Italic);
      XFont fontBoldItalic = new XFont(facename, 20, XFontStyle.BoldItalic);

      // The default alignment is baseline left (that differs from GDI+)
      gfx.DrawString("Times (regular)", fontRegular, XBrushes.DarkSlateGray, 0, 30);
      gfx.DrawString("Times (bold)", fontBold, XBrushes.DarkSlateGray, 0, 65);
      gfx.DrawString("Times (italic)", fontItalic, XBrushes.DarkSlateGray, 0, 100);
      gfx.DrawString("Times (bold italic)", fontBoldItalic, XBrushes.DarkSlateGray, 0, 135);
    }
  }
}