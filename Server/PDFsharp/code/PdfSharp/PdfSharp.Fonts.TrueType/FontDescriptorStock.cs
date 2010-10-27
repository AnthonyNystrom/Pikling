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
using System.Collections;
using PdfSharp.Drawing;
using PdfSharp.Fonts.TrueType;

namespace PdfSharp.Fonts.TrueType
{
#if true
/// <summary>
  /// Global table of TrueType fontdescriptor objects.
  /// </summary>
  class FontDescriptorStock
  {
    private FontDescriptorStock()
    {
      this.table = new Hashtable();
    }

    /// <summary>
    /// Gets the FontDescriptor identified by the specified FontSelector. Returns null if no
    /// such objects exists.
    /// </summary>
    public FontDescriptor FindDescriptor(FontSelector selector)
    {
      if (selector == null)
        return null;

      FontDescriptor descriptor = this.table[selector] as FontDescriptor;
      return descriptor;
    }

    ///// <summary>
    ///// Gets the FontDescriptor identified by the specified FontSelector. If no such objects 
    ///// exists, a new FontDescriptor is created and added to the stock.
    ///// </summary>
    //public FontDescriptor CreateDescriptor(FontSelector selector)
    //{
    //  if (selector == null)
    //    throw new ArgumentNullException("selector");

    //  FontDescriptor descriptor = this.table[selector] as FontDescriptor;
    //  if (descriptor == null)
    //  {
    //    descriptor = new TrueTypeDescriptor(selector);
    //    this.table.Add(selector, descriptor);
    //  }
    //  return descriptor;
    //}

    /// <summary>
    /// Gets the FontDescriptor identified by the specified FontSelector. If no such objects 
    /// exists, a new FontDescriptor is created and added to the stock.
    /// </summary>
    public FontDescriptor CreateDescriptor(XFont font)
    {
      if (font == null)
        throw new ArgumentNullException("font");

      FontSelector selector = new FontSelector(font);
      FontDescriptor descriptor = this.table[selector] as FontDescriptor;
      if (descriptor == null)
      {
        lock (typeof(FontDescriptorStock))
        {
          // may be created by other thread meanwhile
          descriptor = this.table[selector] as FontDescriptor;
          if (descriptor == null)
          {
            descriptor = new TrueTypeDescriptor(font, font.privateFontCollection);
            this.table.Add(selector, descriptor);
          }
        }
      }
      return descriptor;
    }

    /// <summary>
    /// Gets the FontDescriptor identified by the specified FontSelector. If no such objects 
    /// exists, a new FontDescriptor is created and added to the stock.
    /// </summary>
    public FontDescriptor CreateDescriptor(XFontFamily family, XFontStyle style)
    {
      if (family == null)
        throw new ArgumentNullException("family");

      FontSelector selector = new FontSelector(family, style);
      FontDescriptor descriptor = this.table[selector] as FontDescriptor;
      if (descriptor == null)
      {
        lock (typeof(FontDescriptorStock))
        {
          // may be created by other thread meanwhile
          descriptor = this.table[selector] as FontDescriptor;
          if (descriptor == null)
          {
            XFont font = new XFont(family.Name, 10, style);
            descriptor = new TrueTypeDescriptor(font, font.privateFontCollection);
            if (this.table.ContainsKey(selector))
              GetType();
            else
              this.table.Add(selector, descriptor);
          }
        }
      }
      return descriptor;
    }

    public FontDescriptor CreateDescriptor(string idName, byte[] fontData)
    {
      FontSelector selector = new FontSelector(idName);
      FontDescriptor descriptor = this.table[selector] as FontDescriptor;
      if (descriptor == null)
      {
        lock (typeof(FontDescriptorStock))
        {
          // may be created by other thread meanwhile
          descriptor = this.table[selector] as FontDescriptor;
          if (descriptor == null)
          {
            descriptor = new TrueTypeDescriptor(idName, fontData);
            this.table.Add(selector, descriptor);
          }
        }
      }
      return descriptor;
    }

    public static FontDescriptorStock Global
    {
      get
      {
        if (FontDescriptorStock.global == null)
        {
          lock (typeof(FontDescriptorStock))
          {
            if (FontDescriptorStock.global == null)
              FontDescriptorStock.global = new FontDescriptorStock();
          }
        }
        return FontDescriptorStock.global;
      }
    }
    static FontDescriptorStock global;

    Hashtable table;

    /// <summary>
    /// A collection of information that uniquely idendifies a particular font.
    /// Used to map XFont to PdfFont.
    /// There is a one to one relationship between a FontSelector and a TrueType/OpenType file.
    /// </summary>
    internal class FontSelector
    {
      public FontSelector(XFont font)
      {
        this.name = font.Name;
        this.style = font.Style;
      }

      public FontSelector(XFontFamily family, XFontStyle style)
      {
        this.name = family.Name;
        this.style = style;
      }

      public FontSelector(string idName)
      {
        this.name = idName;
        this.style = XFontStyle.Regular;
      }

      public string Name
      {
        get { return this.name; }
      }
      string name;

      public XFontStyle Style
      {
        get { return this.style; }
      }
      XFontStyle style;

      public static bool operator ==(FontSelector selector1, FontSelector selector2)
      {
        if (selector1 != null)
          selector1.Equals(selector2);
        return selector2 == null;
      }

      public static bool operator !=(FontSelector selector1, FontSelector selector2)
      {
        return !(selector1 == selector2);
      }

      public override bool Equals(object obj)
      {
        FontSelector selector = obj as FontSelector;
        if (obj != null && this.name == selector.name)
          return this.style == selector.style;
        return false;
      }

      public override int GetHashCode()
      {
        return this.name.GetHashCode() ^ this.style.GetHashCode();
      }

      /// <summary>
      /// Returns a string for diagnostic purposes only.
      /// </summary>
      public override string ToString()
      {
        string variation = "";
        switch (this.style)
        {
          case XFontStyle.Regular:
            variation = "(Regular)";
            break;

          case XFontStyle.Bold:
            variation = "(Bold)";
            break;

          case XFontStyle.Italic:
            variation = "(Italic)";
            break;

          case XFontStyle.Bold | XFontStyle.Italic:
            variation = "(BoldItalic)";
            break;
        }
        return this.name + variation;
      }
    }
  }
#else
  /// <summary>
  /// Global table of TrueType fontdescriptor objects.
  /// </summary>
  class FontDescriptorStock
  {
    private FontDescriptorStock()
    {
      this.table = new Hashtable();
    }

    /// <summary>
    /// Gets the FontDescriptor identified by the specified FontSelector. Returns null if no
    /// such objects exists.
    /// </summary>
    public FontDescriptor FindDescriptor(FontSelector selector)
    {
      if (selector == null)
        return null;

      FontDescriptor descriptor = this.table[selector] as FontDescriptor;
      return descriptor;
    }

    ///// <summary>
    ///// Gets the FontDescriptor identified by the specified FontSelector. If no such objects 
    ///// exists, a new FontDescriptor is created and added to the stock.
    ///// </summary>
    //public FontDescriptor CreateDescriptor(FontSelector selector)
    //{
    //  if (selector == null)
    //    throw new ArgumentNullException("selector");

    //  FontDescriptor descriptor = this.table[selector] as FontDescriptor;
    //  if (descriptor == null)
    //  {
    //    descriptor = new TrueTypeDescriptor(selector);
    //    this.table.Add(selector, descriptor);
    //  }
    //  return descriptor;
    //}

    /// <summary>
    /// Gets the FontDescriptor identified by the specified FontSelector. If no such objects 
    /// exists, a new FontDescriptor is created and added to the stock.
    /// </summary>
    public FontDescriptor CreateDescriptor(XFont font)
    {
      if (font == null)
        throw new ArgumentNullException("font");

      FontSelector selector = new FontSelector(font);
      FontDescriptor descriptor = this.table[selector] as FontDescriptor;
      if (descriptor == null)
      {
        lock (typeof(FontDescriptorStock))
        {
          // may be created by other thread meanwhile
          descriptor = this.table[selector] as FontDescriptor;
          if (descriptor == null)
          {
            descriptor = new TrueTypeDescriptor(font, font.privateFontCollection);
            this.table.Add(selector, descriptor);
          }
        }
      }
      return descriptor;
    }

    /// <summary>
    /// Gets the FontDescriptor identified by the specified FontSelector. If no such objects 
    /// exists, a new FontDescriptor is created and added to the stock.
    /// </summary>
    public FontDescriptor CreateDescriptor(XFontFamily family, XFontStyle style)
    {
      if (family == null)
        throw new ArgumentNullException("family");

      FontSelector selector = new FontSelector(family, style);
      FontDescriptor descriptor = this.table[selector] as FontDescriptor;
      if (descriptor == null)
      {
        lock (typeof(FontDescriptorStock))
        {
          // may be created by other thread meanwhile
          descriptor = this.table[selector] as FontDescriptor;
          if (descriptor == null)
          {
            XFont font = new XFont(family.Name, 10, style);
            descriptor = new TrueTypeDescriptor(font, font.privateFontCollection);
            if (this.table.ContainsKey(selector))
              GetType();
            else
              this.table.Add(selector, descriptor);
          }
        }
      }
      return descriptor;
    }

    public FontDescriptor CreateDescriptor(string idName, byte[] fontData)
    {
      FontSelector selector = new FontSelector(idName);
      FontDescriptor descriptor = this.table[selector] as FontDescriptor;
      if (descriptor == null)
      {
        lock (typeof(FontDescriptorStock))
        {
          // may be created by other thread meanwhile
          descriptor = this.table[selector] as FontDescriptor;
          if (descriptor == null)
          {
            descriptor = new TrueTypeDescriptor(idName, fontData);
            this.table.Add(selector, descriptor);
          }
        }
      }
      return descriptor;
    }

    public static FontDescriptorStock Global
    {
      get
      {
        if (FontDescriptorStock.global == null)
        {
          lock (typeof(FontDescriptorStock))
          {
            if (FontDescriptorStock.global == null)
              FontDescriptorStock.global = new FontDescriptorStock();
          }
        }
        return FontDescriptorStock.global;
      }
    }
    static FontDescriptorStock global;

    Hashtable table;

    /// <summary>
    /// A collection of information that uniquely idendifies a particular font.
    /// Used to map XFont to PdfFont.
    /// There is a one to one relationship between a FontSelector and a TrueType/OpenType file.
    /// </summary>
    internal class FontSelector
    {
      public FontSelector(XFont font)
      {
        this.name = font.Name;
        this.style = font.Style;
      }

      public FontSelector(XFontFamily family, XFontStyle style)
      {
        this.name = family.Name;
        this.style = style;
      }

      public FontSelector(string idName)
      {
        this.name = idName;
        this.style = XFontStyle.Regular;
      }

      public string Name
      {
        get { return this.name; }
      }
      string name;

      public XFontStyle Style
      {
        get { return this.style; }
      }
      XFontStyle style;

      public static bool operator ==(FontSelector selector1, FontSelector selector2)
      {
        if (selector1 != null)
          selector1.Equals(selector2);
        return selector2 == null;
      }

      public static bool operator !=(FontSelector selector1, FontSelector selector2)
      {
        return !(selector1 == selector2);
      }

      public override bool Equals(object obj)
      {
        FontSelector selector = obj as FontSelector;
        if (obj != null && this.name == selector.name)
          return this.style == selector.style;
        return false;
      }

      public override int GetHashCode()
      {
        return this.name.GetHashCode() ^ this.style.GetHashCode();
      }

      /// <summary>
      /// Returns a string for diagnostic purposes only.
      /// </summary>
      public override string ToString()
      {
        string variation = "";
        switch (this.style)
        {
          case XFontStyle.Regular:
            variation = "(Regular)";
            break;

          case XFontStyle.Bold:
            variation = "(Bold)";
            break;

          case XFontStyle.Italic:
            variation = "(Italic)";
            break;

          case XFontStyle.Bold | XFontStyle.Italic:
            variation = "(BoldItalic)";
            break;
        }
        return this.name + variation;
      }
    }
  }
#endif
}