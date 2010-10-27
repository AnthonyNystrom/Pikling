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
using System.IO;
//#if GDI
//using System.Drawing;
//using System.Drawing.Drawing2D;
//using System.Drawing.Imaging;
//#endif
#if WPF
using System.Windows;
using System.Windows.Media;
#endif
using PdfSharp.Internal;
using PdfSharp.Pdf;
using PdfSharp.Drawing.Pdf;
using PdfSharp.Fonts.TrueType;
using PdfSharp.Pdf.Advanced;

namespace PdfSharp.Drawing
{
#if WPF
  /// <summary>
  /// The Get WPF Value flags.
  /// </summary>
  enum GWV
  {
    GetCellAscent,
    GetCellDescent,
    GetEmHeight,
    GetLineSpacing,
    IsStyleAvailable
  }

  /// <summary>
  /// Helper class for fonts.
  /// </summary>
  static class FontHelper
  {
    /// <summary>
    /// Creates a typeface.
    /// </summary>
    public static Typeface CreateTypeface(XFontFamily family, XFontStyle style)
    {
      FontStyle fontStyle = FontStyleFromStyle(style);
      FontWeight fontWeight = FontWeightFromStyle(style);
      Typeface typeface = new Typeface(family.wpfFamily, fontStyle, fontWeight, FontStretches.Normal);
      return typeface;
    }

    /// <summary>
    /// Simple hack to make it work...
    /// </summary>
    public static FontStyle FontStyleFromStyle(XFontStyle style)
    {
      switch (style & XFontStyle.BoldItalic)  // mask out Underline and Strikeout
      {
        case XFontStyle.Regular:
          return FontStyles.Normal;

        case XFontStyle.Bold:
          return FontStyles.Normal;

        case XFontStyle.Italic:
          return FontStyles.Italic;

        case XFontStyle.BoldItalic:
          return FontStyles.Italic;
      }
      return FontStyles.Normal;
    }

    /// <summary>
    /// Simple hack to make it work...
    /// </summary>
    public static FontWeight FontWeightFromStyle(XFontStyle style)
    {
      switch (style)
      {
        case XFontStyle.Regular:
          return FontWeights.Normal;

        case XFontStyle.Bold:
          return FontWeights.Bold;

        case XFontStyle.Italic:
          return FontWeights.Normal;

        case XFontStyle.BoldItalic:
          return FontWeights.Bold;
      }
      return FontWeights.Normal;
    }

    public static int GetWpfValue(XFontFamily family, XFontStyle style, GWV value)
    {
      FontDescriptor descriptor = FontDescriptorStock.Global.CreateDescriptor(family, style);
      XFontMetrics metrics = descriptor.FontMetrics;

      switch (value)
      {
        case GWV.GetCellAscent:
          return (int)metrics.Ascent;

        case GWV.GetCellDescent:
          return (int)Math.Abs(metrics.Descent);

        case GWV.GetEmHeight:
          return (int)metrics.CapHeight;

        case GWV.GetLineSpacing:
          return (int)(metrics.Ascent + Math.Abs(metrics.Descent) + metrics.Leading);

        case GWV.IsStyleAvailable:
          // TODOWPF: 
          System.Collections.Generic.List<Typeface> typefaces = new System.Collections.Generic.List<Typeface>(family.wpfFamily.GetTypefaces());
          foreach (Typeface typeface in typefaces)
          {
            Debugger.Break();
            //typeface.Style = FontStyles.
          }
          // TODOWPF
          return 1;
      }
      return 0;
    }
  }
#endif
}