using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows;
using System.Windows.Media;
using PdfSharp.Xps.UnitTests.Helpers;

namespace PdfSharp.Xps.UnitTests.AcroTests
{
  /// <summary>
  /// Test linear gradient brushes.
  /// </summary>
  [TestClass]
  public class GradientBrushes : TestBase
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
    public void LinearGradientBrush1()
    {
      RenderVisual("LinearGradientBrush1", CreateLinearGradientBrush1);
    }

    Visual CreateLinearGradientBrush1()
    {
      const double width = 240;
      const double height = 120;

      DrawingContext dc;
      DrawingVisual dv = PrepareDrawingVisual(out dc, false);

      LinearGradientBrush brush;

      //BeginBox(dc, 1, BoxOptions.Tile);
      //brush = new LinearGradientBrush(Colors.DarkBlue, Colors.Orange, 0);
      //dc.DrawEllipse(brush, null, center, radiusX, radiusY);
      //EndBox(dc);

      //BeginBox(dc, 4, BoxOptions.Tile, "dark blue to red");
      Color clrFrom = Colors.DarkBlue;
      //clrFrom.A = 240;
      Color clrTo = Colors.Red;
      clrTo.A = 0;
      brush = new LinearGradientBrush(clrFrom, clrTo, 0);
      brush.SpreadMethod = GradientSpreadMethod.Pad;

      //brush.StartPoint = new Point(0.5, 0.45);
      //brush.GradientStops.Add(new GradientStop(clrFrom, 0));
      //brush.EndPoint = new Point(0.55, 0.55);
      //brush.GradientStops.Add(new GradientStop(clrTo, 1));
      //brush.RelativeTransform = Transform.
      //brush.Opacity = 0.5;
      //dc.DrawEllipse(brush, null, center, radiusX, radiusY);

      //dc.PushOpacity(0.66);
      Pen pen = new Pen(Brushes.Green, 3);
      pen.DashStyle = DashStyles.DashDotDot;
      dc.DrawRectangle(brush, null, new Rect(0, 0, width, height));
      //dc.Pop();
      //EndBox(dc);

      dc.Close();
      return dv;
    }

    [TestMethod]
    public void RadialGradientBrush1()
    {
      RenderVisual("RadialGradientBrush 1", CreateRadialGradientBrush1);
    }

    Visual CreateRadialGradientBrush1()
    {
      const double width = 240;
      const double height = 120;

      DrawingContext dc;
      DrawingVisual dv = PrepareDrawingVisual(out dc, false);

      RadialGradientBrush brush;

      //BeginBox(dc, 1, BoxOptions.Tile);
      //brush = new LinearGradientBrush(Colors.DarkBlue, Colors.Orange, 0);
      //dc.DrawEllipse(brush, null, center, radiusX, radiusY);
      //EndBox(dc);

      dc.PushTransform(new TranslateTransform(20, 20));
      //BeginBox(dc, 4, BoxOptions.Tile, "dark blue to red");
      Color clrFrom = Colors.DarkBlue;
      //clrFrom.A = 240;
      Color clrTo = Colors.Red;
      //clrTo.A = 0;
      brush = new RadialGradientBrush(clrFrom, clrTo);
      brush.SpreadMethod = GradientSpreadMethod.Pad;
      brush.Center = new Point(0.3, 0.3);
      brush.GradientOrigin = new Point(0.5, 0.7);

      //dc.PushOpacity(0.66);
      //Pen pen = new Pen(Brushes.Green, 3);
      //pen.DashStyle = DashStyles.DashDotDot;
      dc.DrawRectangle(brush, null, new Rect(0, 0, width, height));
      //dc.Pop();
      //EndBox(dc);

      dc.Close();
      return dv;
    }

    [TestMethod]
    public void RadialGradientBrush2()
    {
      RenderVisual("RadialGradientBrush 2", CreateRadialGradientBrush2);
    }

    Visual CreateRadialGradientBrush2()
    {
      const double width = 240;
      const double height = 120;

      DrawingContext dc;
      DrawingVisual dv = PrepareDrawingVisual(out dc, false);

      RadialGradientBrush brush;
      Color clrFrom = Colors.Red;
      Color clrTo = Colors.Red;
      clrTo.A = 0;
      brush = new RadialGradientBrush(clrFrom, clrTo);
      brush.SpreadMethod = GradientSpreadMethod.Pad;
      dc.DrawRectangle(brush, null, new Rect(0, 0, width, height));

      dc.Close();
      return dv;
    }

    [TestMethod]
    public void RadialGradientBrush3()
    {
      RenderVisual("RadialGradientBrush 3", CreateRadialGradientBrush3);
    }

    Visual CreateRadialGradientBrush3()
    {
      //<RadialGradientBrush MappingMode="Absolute" Center="250,240" GradientOrigin="250,240" RadiusX="50" RadiusY="40" SpreadMethod="Reflect">
      //  <RadialGradientBrush.GradientStops>
      //    <GradientStop Color="#ff0000" Offset="0.0" />
      //    <GradientStop Color="#ffff00" Offset="0.25" />
      //    <GradientStop Color="#00ff00" Offset="0.5" />
      //    <GradientStop Color="#00ffff" Offset="0.75" />
      //    <GradientStop Color="#0000ff" Offset="1.0" />
      //  </RadialGradientBrush.GradientStops>
      //</RadialGradientBrush>

      const double width = 240;
      const double height = 120;

      DrawingContext dc;
      DrawingVisual dv = PrepareDrawingVisual(out dc, false);

      RadialGradientBrush brush = new RadialGradientBrush();
      brush.GradientStops.Add(new GradientStop(Color.FromRgb(255, 0, 0), 0));
      brush.GradientStops.Add(new GradientStop(Color.FromRgb(255, 255, 0), 0.25));
      brush.GradientStops.Add(new GradientStop(Color.FromRgb(0, 255, 0), 0.5));
      brush.GradientStops.Add(new GradientStop(Color.FromRgb(0, 255, 255), 0.75));
      brush.GradientStops.Add(new GradientStop(Color.FromRgb(0, 0, 255), 1));
      brush.SpreadMethod = GradientSpreadMethod.Reflect;
      brush.MappingMode = BrushMappingMode.RelativeToBoundingBox;
      brush.RadiusX = 0.2;
      brush.RadiusX = 0.15;
      dc.DrawRectangle(brush, null, new Rect(0, 0, width, height));

      dc.Close();
      return dv;
    }
  }
}