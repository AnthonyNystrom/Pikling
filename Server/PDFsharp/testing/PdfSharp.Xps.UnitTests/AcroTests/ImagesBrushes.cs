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
  /// Test image brushes.
  /// </summary>
  [TestClass]
  public class ImagesBrushes : TestBase
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
    public void ImagesBrushes_None()
    {
      RenderVisual("ImagesBrushes None", CreateImagesBrushes_None, true);
    }

    Visual CreateImagesBrushes_None()
    {
      const double width = 240;
      const double height = 120;

      DrawingContext dc;
      DrawingVisual dv = PrepareDrawingVisual(out dc, false);
      ImageSource image;
      ImageBrush brush;
      Pen pen;

      image = VisualsHelper.GetBitmapSource("Resources.Test01.png");
      brush = new ImageBrush(image);
      brush.TileMode = TileMode.None;

      //dc.PushOpacity(0.66);
      pen = new Pen(Brushes.Green, 3);
      pen.DashStyle = DashStyles.DashDotDot;
      dc.DrawRectangle(brush, null, new Rect(20, 20, width, height));
      //dc.Pop();

      image = VisualsHelper.GetBitmapSource("Resources.Test02.png");
      brush = new ImageBrush(image);
      brush.TileMode = TileMode.None;
      pen = new Pen(Brushes.Green, 3);
      pen.DashStyle = DashStyles.DashDotDot;
      dc.DrawRectangle(brush, null, new Rect(20, 220, width, height));

      dc.Close();
      return dv;
    }

    [TestMethod]
    public void ImagesBrushesBrush_Tile()
    {
      RenderVisual("ImagesBrushes Tile", CreateImagesBrushes_Tile, true);
    }

    Visual CreateImagesBrushes_Tile()
    {
      const double width = 240;
      const double height = 120;

      DrawingContext dc;
      DrawingVisual dv = PrepareDrawingVisual(out dc, false);

      ImageSource image = VisualsHelper.GetBitmapSource("Resources.Test02.png");
      ImageBrush brush = new ImageBrush(image);
      brush.RelativeTransform = new RotateTransform(45);
      brush.ViewportUnits = BrushMappingMode.Absolute;
      brush.Viewport = new Rect(0, 0, 24, 12);
      brush.TileMode = TileMode.Tile;
      //brush.AlignmentX = AlignmentX.

      dc.DrawRectangle(brush, null, new Rect(0, 0, width, height));
      dc.Close();

      return dv;
    }

    [TestMethod]
    public void ImagesBrushesBrush_FlipX()
    {
      RenderVisual("ImagesBrushes FlipX", CreateImagesBrushes_FlipX, true);
    }

    Visual CreateImagesBrushes_FlipX()
    {
      const double width = 240;
      const double height = 120;

      DrawingContext dc;
      DrawingVisual dv = PrepareDrawingVisual(out dc, false);

      ImageSource image = VisualsHelper.GetBitmapSource("Resources.Test02.png");
      ImageBrush brush = new ImageBrush(image);
      brush.ViewportUnits = BrushMappingMode.Absolute;
      brush.Viewport = new Rect(0, 0, 24, 12);
      brush.TileMode = TileMode.FlipX;

      dc.DrawRectangle(brush, null, new Rect(0, 0, width, height));
      dc.Close();

      return dv;
    }

    [TestMethod]
    public void ImagesBrushesBrush_FlipY()
    {
      RenderVisual("ImagesBrushes FlipY", CreateImagesBrushes_FlipY, true);
    }

    Visual CreateImagesBrushes_FlipY()
    {
      const double width = 240;
      const double height = 120;

      DrawingContext dc;
      DrawingVisual dv = PrepareDrawingVisual(out dc, false);

      ImageSource image = VisualsHelper.GetBitmapSource("Resources.Test02.png");
      ImageBrush brush = new ImageBrush(image);
      brush.ViewportUnits = BrushMappingMode.Absolute;
      brush.Viewport = new Rect(0, 0, 24, 12);
      brush.TileMode = TileMode.FlipY;

      dc.DrawRectangle(brush, null, new Rect(0, 0, width, height));
      dc.Close();

      return dv;
    }

    [TestMethod]
    public void ImagesBrushesBrush_FlipXY()
    {
      RenderVisual("ImagesBrushes FlipXY", CreateImagesBrushes_FlipXY, true);
    }

    Visual CreateImagesBrushes_FlipXY()
    {
      const double width = 240;
      const double height = 120;

      DrawingContext dc;
      DrawingVisual dv = PrepareDrawingVisual(out dc, false);

      ImageSource image = VisualsHelper.GetBitmapSource("Resources.Test02.png");
      ImageBrush brush = new ImageBrush(image);
      brush.ViewportUnits = BrushMappingMode.Absolute;
      brush.Viewport = new Rect(0, 0, 24, 12);
      brush.TileMode = TileMode.FlipXY;

      dc.DrawRectangle(brush, null, new Rect(0, 0, width, height));
      dc.Close();

      return dv;
    }
  }
}