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
using System.ComponentModel;
#if GDI
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Runtime.InteropServices;
#endif
#if WPF
using System.Windows.Media;
#endif
using PdfSharp.Internal;

#pragma warning disable 1591

namespace PdfSharp.Drawing
{
#if GDI
  ///<summary>
  /// Represents a PrivateFontCollection.
  /// </summary>
  public class XPrivateFontCollection
  {
    public XPrivateFontCollection()
    {
      privateFontCollection = null;
      privateFonts = null;
    }

    public PrivateFontCollection PrivateFontCollection
    {
      get { return privateFontCollection; }
      set { privateFontCollection = value; }
    }
    PrivateFontCollection privateFontCollection;
    List<XPrivateFont> privateFonts;

    void Initialize()
    {
      if (privateFontCollection == null)
        privateFontCollection = new PrivateFontCollection();
      if (privateFonts == null)
        privateFonts = new List<XPrivateFont>();
    }

    /*public void AddMemoryFont(IntPtr memory, int length,
      string fontName, bool bold, bool italic)
    {
    }*/

    public void AddMemoryFont(byte[] data, int length,
      string fontName, bool bold, bool italic)
    {
      Initialize();
      // Do it w/o unsafe code (do it like VB programmers do): 
      IntPtr ip = Marshal.AllocCoTaskMem(length);
      Marshal.Copy(data, 0, ip, length);
      privateFontCollection.AddMemoryFont(ip, length);
      Marshal.FreeCoTaskMem(ip);
      byte[] data2 = new byte[length];
      Array.Copy(data, data2, length);
      XPrivateFont pf = new XPrivateFont(fontName, bold, italic, data2, length);
      privateFonts.Add(pf);
    }

    public XPrivateFont FindFont(string fontName, bool bold, bool italic)
    {
      if (privateFonts != null)
      {
        for (int i = 0; i < 2; ++i)
        {
          // We make 3 passes.
          // On second pass, we ignore Bold.
          // On third pass, we ignore Bold and Italic
          foreach (XPrivateFont pf in privateFonts)
          {
            if (string.Compare(pf.FontName, fontName, true) == 0)
            {
              switch (i)
              {
                case 0:
                  if (pf.Bold == bold && pf.Italic == italic)
                    return pf;
                  break;
                case 1:
                  if (/*pf.Bold == bold &&*/ pf.Italic == italic)
                    return pf;
                  break;
                case 2:
                  //if (pf.Bold == bold && pf.Italic == italic)
                  return pf;
              }
            }
          }
        }
      }
      return null;
    }
  }

  /// <summary>
  /// THHO4THHO: TODO!
  /// </summary>
  public class XPrivateFont
  {
    public XPrivateFont(string fontName,
      bool bold,
      bool italic,
      byte[] data,
      int length)
    {
      this.FontName = fontName;
      this.Bold = bold;
      this.Italic = italic;
      this.Data = data;
      this.Length = length;
    }

    internal string FontName;
    internal bool Bold;
    internal bool Italic;
    internal byte[] Data;
    internal int Length;

    public int GetFontData(ref byte[] data,
       int length)
    {
      if (length == this.Length)
      {
        // Copy the data:
        //Data.CopyTo(data, 0);
        Array.Copy(Data, data, length);
      }
      return this.Length;
    }
  }
#endif

#if WPF && !GDI
  /// <summary>
  /// Stub - yni.
  /// </summary>
  public class XPrivateFontCollection
  {
  // TODOWPF:
  }
#endif
}