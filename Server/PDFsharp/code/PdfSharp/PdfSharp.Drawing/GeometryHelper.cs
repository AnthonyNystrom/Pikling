#region PDFsharp - A .NET library for processing PDF
//
// Authors:
//   Stefan Lange (mailto:Stefan.Lange@pdfsharp.com)
//
// Copyright (c) 2005-2008 empira Software GmbH, Cologne (Germany)
//
// http://www.pdfsharp.com
// http://sourceforge.net/projects/pdfsharp
//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included
// in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.Diagnostics;
using System.Globalization;
using System.Collections.Generic;
using System.IO;
#if GDI
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
#endif
#if WPF
using System.Windows;
using System.Windows.Media;
#endif
using PdfSharp.Internal;
using PdfSharp.Pdf;
using PdfSharp.Drawing.Pdf;
using PdfSharp.Pdf.Advanced;

namespace PdfSharp.Drawing
{
  /// <summary>
  /// Helper class for Geometry paths.
  /// </summary>
  static class GeometryHelper
  {
#if WPF
    /// <summary>
    /// Appends a Bézier segment from a curve.
    /// </summary>
    public static BezierSegment CreateCurveSegment(XPoint pt0, XPoint pt1, XPoint pt2, XPoint pt3, double tension3)
    {
      return new BezierSegment(
        new System.Windows.Point(pt1.X + tension3 * (pt2.X - pt0.X), pt1.Y + tension3 * (pt2.Y - pt0.Y)),
        new System.Windows.Point(pt2.X - tension3 * (pt3.X - pt1.X), pt2.Y - tension3 * (pt3.Y - pt1.Y)),
        new System.Windows.Point(pt2.X, pt2.Y), true);
    }
#endif

#if WPF
    /// <summary>
    /// Creates a path geometry form a polygon.
    /// </summary>
    public static PathGeometry CreatePolygonGeometry(System.Windows.Point[] points, XFillMode fillMode, bool closed)
    {
      PolyLineSegment seg = new PolyLineSegment();
      int count = points.Length;
      // For correct drawing the start point of the segment must not be the same as the first point
      for (int idx = 1; idx < count; idx++)
        seg.Points.Add(new System.Windows.Point(points[idx].X, points[idx].Y));
      seg.IsStroked = true;
      PathFigure fig = new PathFigure();
      fig.StartPoint = new System.Windows.Point(points[0].X, points[0].Y);
      fig.Segments.Add(seg);
      fig.IsClosed = closed;
      PathGeometry geo = new PathGeometry();
      geo.FillRule = fillMode == XFillMode.Winding ? FillRule.Nonzero : FillRule.EvenOdd;
      geo.Figures.Add(fig);
      return geo;
    }
#endif

#if WPF
    /// <summary>
    /// Creates the arc segment from paramters of the GDI+ DrawArc function.
    /// </summary>
    public static ArcSegment CreateArcSegment(double x, double y, double width, double height, double startAngle,
      double sweepAngle, out System.Windows.Point startPoint)
    {
      // Normalize the angles
      double α = startAngle;
      if (α < 0)
        α = α + (1 + Math.Floor((Math.Abs(α) / 360))) * 360;
      else if (α > 360)
        α = α - Math.Floor(α / 360) * 360;
      Debug.Assert(α >= 0 && α <= 360);

      if (Math.Abs(sweepAngle) >= 360)
        sweepAngle = Math.Sign(sweepAngle) * 360;
      double β = startAngle + sweepAngle;
      if (β < 0)
        β = β + (1 + Math.Floor((Math.Abs(β) / 360))) * 360;
      else if (β > 360)
        β = β - Math.Floor(β / 360) * 360;

      if (α == 0 && β < 0)
        α = 360;
      else if (α == 360 && β > 0)
        α = 0;

      // Scanling factor
      double δx = width / 2;
      double δy = height / 2;

      // Center of ellipse
      double x0 = x + δx;
      double y0 = y + δy;

      double cosα, cosβ, sinα, sinβ;
      if (width == height)
      {
        // Circular arc needs no correction.
        α = α * Calc.Deg2Rad;
        β = β * Calc.Deg2Rad;
      }
      else
      {
        // Elliptic arc needs the angles to be adjusted such that the scaling transformation is compensated.
        α = α * Calc.Deg2Rad;
        sinα = Math.Sin(α);
        if (Math.Abs(sinα) > 1E-10)
        {
          if (α < Math.PI)
            α = Math.PI / 2 - Math.Atan(δy * Math.Cos(α) / (δx * sinα));
          else
            α = 3 * Math.PI / 2 - Math.Atan(δy * Math.Cos(α) / (δx * sinα));
        }
        //α = Calc.πHalf - Math.Atan(δy * Math.Cos(α) / (δx * sinα));
        β = β * Calc.Deg2Rad;
        sinβ = Math.Sin(β);
        if (Math.Abs(sinβ) > 1E-10)
        {
          if (β < Math.PI)
            β = Math.PI / 2 - Math.Atan(δy * Math.Cos(β) / (δx * sinβ));
          else
            β = 3 * Math.PI / 2 - Math.Atan(δy * Math.Cos(β) / (δx * sinβ));
        }
        //β = Calc.πHalf - Math.Atan(δy * Math.Cos(β) / (δx * sinβ));
      }

      sinα = Math.Sin(α);
      cosα = Math.Cos(α);
      sinβ = Math.Sin(β);
      cosβ = Math.Cos(β);

      startPoint = new System.Windows.Point(x0 + δx * cosα, y0 + δy * sinα);
      System.Windows.Point destPoint = new System.Windows.Point(x0 + δx * cosβ, y0 + δy * sinβ);
      System.Windows.Size size = new System.Windows.Size(δx, δy);
      bool isLargeArc = Math.Abs(sweepAngle) >= 180;
      SweepDirection sweepDirection = sweepAngle > 0 ? SweepDirection.Clockwise : SweepDirection.Counterclockwise;
      bool isStroked = true;
      ArcSegment seg = new ArcSegment(destPoint, size, 0, isLargeArc, sweepDirection, isStroked);
      return seg;
    }
#endif

    /// <summary>
    /// Creates between 1 and 5 Béziers curves from parameters specified like in GDI+.
    /// </summary>
    public static List<XPoint> BezierCurveFromArc(double x, double y, double width, double height, double startAngle, double sweepAngle,
      PathStart pathStart, ref XMatrix matrix)
    {
      List<XPoint> points = new List<XPoint>();

      // Normalize the angles
      double α = startAngle;
      if (α < 0)
        α = α + (1 + Math.Floor((Math.Abs(α) / 360))) * 360;
      else if (α > 360)
        α = α - Math.Floor(α / 360) * 360;
      Debug.Assert(α >= 0 && α <= 360);

      double β = sweepAngle;
      if (β < -360)
        β = -360;
      else if (β > 360)
        β = 360;

      if (α == 0 && β < 0)
        α = 360;
      else if (α == 360 && β > 0)
        α = 0;

      // Is it possible that the arc is small starts and ends in same quadrant?
      bool smallAngle = Math.Abs(β) <= 90;

      β = α + β;
      if (β < 0)
        β = β + (1 + Math.Floor((Math.Abs(β) / 360))) * 360;

      bool clockwise = sweepAngle > 0;
      int startQuadrant = Quatrant(α, true, clockwise);
      int endQuadrant = Quatrant(β, false, clockwise);

      if (startQuadrant == endQuadrant && smallAngle)
        AppendPartialArcQuadrant(points, x, y, width, height, α, β, pathStart, matrix);
      else
      {
        int currentQuadrant = startQuadrant;
        bool firstLoop = true;
        do
        {
          if (currentQuadrant == startQuadrant && firstLoop)
          {
            double ξ = currentQuadrant * 90 + (clockwise ? 90 : 0);
            AppendPartialArcQuadrant(points, x, y, width, height, α, ξ, pathStart, matrix);
          }
          else if (currentQuadrant == endQuadrant)
          {
            double ξ = currentQuadrant * 90 + (clockwise ? 0 : 90);
            AppendPartialArcQuadrant(points, x, y, width, height, ξ, β, PathStart.Ignore1st, matrix);
          }
          else
          {
            double ξ1 = currentQuadrant * 90 + (clockwise ? 0 : 90);
            double ξ2 = currentQuadrant * 90 + (clockwise ? 90 : 0);
            AppendPartialArcQuadrant(points, x, y, width, height, ξ1, ξ2, PathStart.Ignore1st, matrix);
          }

          // Don't stop immediately if arc is greater than 270 degrees
          if (currentQuadrant == endQuadrant && smallAngle)
            break;
          smallAngle = true;

          if (clockwise)
            currentQuadrant = currentQuadrant == 3 ? 0 : currentQuadrant + 1;
          else
            currentQuadrant = currentQuadrant == 0 ? 3 : currentQuadrant - 1;

          firstLoop = false;
        } while (true);
      }
      return points;
    }

    /// <summary>
    /// Calculates the quadrant (0 through 3) of the specified angle. If the angle lies on an edge
    /// (0, 90, 180, etc.) the result depends on the details how the angle is used.
    /// </summary>
    static int Quatrant(double φ, bool start, bool clockwise)
    {
      Debug.Assert(φ >= 0);
      if (φ > 360)
        φ = φ - Math.Floor(φ / 360) * 360;

      int quadrant = (int)(φ / 90);
      if (quadrant * 90 == φ)
      {
        if ((start && !clockwise) || (!start && clockwise))
          quadrant = quadrant == 0 ? 3 : quadrant - 1;
      }
      else
        quadrant = clockwise ? ((int)Math.Floor(φ / 90)) % 4 : (int)Math.Floor(φ / 90);
      return quadrant;
    }

    /// <summary>
    /// Appends a Bézier curve for an arc within a full quadrant.
    /// </summary>
    static void AppendPartialArcQuadrant(List<XPoint> points, double x, double y, double width, double height, double α, double β, PathStart pathStart, XMatrix matrix)
    {
      Debug.Assert(α >= 0 && α <= 360);
      Debug.Assert(β >= 0);
      if (β > 360)
        β = β - Math.Floor(β / 360) * 360;
      Debug.Assert(Math.Abs(α - β) <= 90);

      // Scanling factor
      double δx = width / 2;
      double δy = height / 2;

      // Center of ellipse
      double x0 = x + δx;
      double y0 = y + δy;

      // We have the following quarters:
      //     |
      //   2 | 3
      // ----+-----
      //   1 | 0
      //     |
      // If the angles lie in quarter 2 or 3, their values are subtracted by 180 and the
      // resulting curve is reflected at the center. This algorythm works as expected (simply tried out).
      // There may be a mathematical more elegant solution...
      bool reflect = false;
      if (α >= 180 && β >= 180)
      {
        α -= 180;
        β -= 180;
        reflect = true;
      }

      double cosα, cosβ, sinα, sinβ;
      if (width == height)
      {
        // Circular arc needs no correction.
        α = α * Calc.Deg2Rad;
        β = β * Calc.Deg2Rad;
      }
      else
      {
        // Elliptic arc needs the angles to be adjusted such that the scaling transformation is compensated.
        α = α * Calc.Deg2Rad;
        sinα = Math.Sin(α);
        if (Math.Abs(sinα) > 1E-10)
          α = Calc.πHalf - Math.Atan(δy * Math.Cos(α) / (δx * sinα));
        β = β * Calc.Deg2Rad;
        sinβ = Math.Sin(β);
        if (Math.Abs(sinβ) > 1E-10)
          β = Calc.πHalf - Math.Atan(δy * Math.Cos(β) / (δx * sinβ));
      }

      double κ = 4 * (1 - Math.Cos((α - β) / 2)) / (3 * Math.Sin((β - α) / 2));
      sinα = Math.Sin(α);
      cosα = Math.Cos(α);
      sinβ = Math.Sin(β);
      cosβ = Math.Cos(β);

      //XPoint pt1, pt2, pt3;
      if (!reflect)
      {
        // Calculation for quarter 0 and 1
        switch (pathStart)
        {
          case PathStart.MoveTo1st:
            points.Add(matrix.Transform(new XPoint(x0 + δx * cosα, y0 + δy * sinα)));
            break;

          case PathStart.LineTo1st:
            points.Add(matrix.Transform(new XPoint(x0 + δx * cosα, y0 + δy * sinα)));
            break;

          case PathStart.Ignore1st:
            break;
        }
        points.Add(matrix.Transform(new XPoint(x0 + δx * (cosα - κ * sinα), y0 + δy * (sinα + κ * cosα))));
        points.Add(matrix.Transform(new XPoint(x0 + δx * (cosβ + κ * sinβ), y0 + δy * (sinβ - κ * cosβ))));
        points.Add(matrix.Transform(new XPoint(x0 + δx * cosβ, y0 + δy * sinβ)));
      }
      else
      {
        // Calculation for quarter 2 and 3
        switch (pathStart)
        {
          case PathStart.MoveTo1st:
            points.Add(matrix.Transform(new XPoint(x0 - δx * cosα, y0 - δy * sinα)));
            break;

          case PathStart.LineTo1st:
            points.Add(matrix.Transform(new XPoint(x0 - δx * cosα, y0 - δy * sinα)));
            break;

          case PathStart.Ignore1st:
            break;
        }
        points.Add(matrix.Transform(new XPoint(x0 - δx * (cosα - κ * sinα), y0 - δy * (sinα + κ * cosα))));
        points.Add(matrix.Transform(new XPoint(x0 - δx * (cosβ + κ * sinβ), y0 - δy * (sinβ - κ * cosβ))));
        points.Add(matrix.Transform(new XPoint(x0 - δx * cosβ, y0 - δy * sinβ)));
      }
    }

    /// <summary>
    /// Creates between 1 and 5 Béziers curves from parameters specified like in WPF.
    /// </summary>
    public static List<XPoint> BezierCurveFromArc(XPoint point1, XPoint point2, double rotationAngle,
      XSize size, bool isLargeArc, bool clockwise, PathStart pathStart)
    {
#if DEBUG_
      if (size == new XSize(115, 115))
        Debugger.Break();
#endif
      // See also http://www.charlespetzold.com/blog/blog.xml from January 2, 2008
      double δx = size.Width;
      double δy = size.Height;
      Debug.Assert(δx * δy > 0);
      double factor = δy / δx;
      bool isCounterclockwise = !clockwise;

      // Adjust for different radii and rotation angle
      XMatrix matrix = new XMatrix();
      matrix.RotateAppend(-rotationAngle);
      matrix.ScaleAppend(δy / δx, 1);
      XPoint pt1 = matrix.Transform(point1);
      XPoint pt2 = matrix.Transform(point2);

      // Get info about chord that connects both points
      XPoint midPoint = new XPoint((pt1.X + pt2.X) / 2, (pt1.Y + pt2.Y) / 2);
      XVector vect = pt2 - pt1;
      double halfChord = vect.Length / 2;

      // Get vector from chord to center
      XVector vectRotated;

      // (comparing two Booleans here!)
      if (isLargeArc == isCounterclockwise)
        vectRotated = new XVector(-vect.Y, vect.X);
      else
        vectRotated = new XVector(vect.Y, -vect.X);

      vectRotated.Normalize();

      // Distance from chord to center 
      double centerDistance = Math.Sqrt(δy * δy - halfChord * halfChord);
      if (double.IsNaN(centerDistance))
        centerDistance = 0;

      // Calculate center point
      XPoint center = midPoint + centerDistance * vectRotated;

      // Get angles from center to the two points
      double α = Math.Atan2(pt1.Y - center.Y, pt1.X - center.X);
      double β = Math.Atan2(pt2.Y - center.Y, pt2.X - center.X);

      // (another comparison of two Booleans!)
      if (isLargeArc == (Math.Abs(β - α) < Math.PI))
      {
        if (α < β)
          α += 2 * Math.PI;
        else
          β += 2 * Math.PI;
      }

      // Invert matrix for final point calculation
      matrix.Invert();
      double sweepAngle = β - α;

      // Let the algorithm of GDI+ DrawArc to Bézier curves do the rest of the job
      return BezierCurveFromArc(center.X - δx * factor, center.Y - δy, 2 * δx * factor, 2 * δy,
        α / Calc.Deg2Rad, sweepAngle / Calc.Deg2Rad, pathStart, ref matrix);
    }
  }
}