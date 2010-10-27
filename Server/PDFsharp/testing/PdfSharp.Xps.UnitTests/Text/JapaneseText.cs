using System;
using System.Text;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using System.IO;
using System.Xml;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Controls;
using PdfSharp.Xps.UnitTests.Helpers;

namespace PdfSharp.Xps.UnitTests.Text
{
  /// <summary>
  /// Test glyphs.
  /// </summary>
  [TestClass]
  public class JapaneseText : TestBase
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
    public void TestSmoothBezierCurveAbbreviatedSyntax()
    {
      RenderVisual("5.1.6.2 Japanese vertical text", new XamlPresenter(GetType(), "Japanese vertical text.xaml").CreateContent);
    }
  }
}