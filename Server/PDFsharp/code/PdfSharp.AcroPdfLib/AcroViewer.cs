#region PDFsharp Viewing - A .NET wrapper of the Adobe ActiveX control
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
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using AcroPDFLib;
using PdfSharp.Pdf;

namespace PdfSharp.Viewing
{
  // DELETE: 06-12-31
#if old
  /// <summary>
  /// Wraps the Adobe Acrobat Web Browser ActiveX control. Requires Acrobat Reader 7.0 to be installed.
  /// Since Acrobat 7 it is legal to use this ActiveX control for viewing and printing PDF files even with
  /// only Acrobat Viewer installed.
  /// For more information see Acrobat Interapplication Communication Reference. You can download this file
  /// here: http://partners.adobe.com/public/developer/en/acrobat/sdk/pdf/iac/IACReference.pdf.
  /// </summary>
  [DefaultEvent("OnError"), AxHost.Clsid("{CA8A9780-280D-11CF-A24D-444553540000}"), DesignTimeVisible(true)]
  class AcroViewer : AxHost
  {
    public event EventHandler OnError;
    public event EventHandler OnMessage;

    public AcroViewer() : base("CA8A9780-280D-11CF-A24D-444553540000")
    {
    }

    /// <summary>
    /// Goes to the previous view on the view stack, if the previous view exists. The previous view
    /// may be in a different document.
    /// </summary>
    public virtual void GoBackwardStack()
    {
      if (this.ocx == null)
        throw new AxHost.InvalidActiveXStateException("goBackwardStack", AxHost.ActiveXInvokeKind.MethodInvoke);

      this.ocx.goBackwardStack();
    }

    /// <summary>
    /// Goes to the next view on the view stack, if the next view exists. The next view may be in a
    /// different document.
    /// </summary>
    public virtual void GoForwardStack()
    {
      if (this.ocx == null)
        throw new AxHost.InvalidActiveXStateException("goForwardStack", AxHost.ActiveXInvokeKind.MethodInvoke);

      this.ocx.goForwardStack();
    }

    /// <summary>
    /// Goes to the first page in the document, maintaining the current location within the page
    /// and zoom level.
    /// </summary>
    public virtual void GotoFirstPage()
    {
      if (this.ocx == null)
        throw new AxHost.InvalidActiveXStateException("gotoFirstPage", AxHost.ActiveXInvokeKind.MethodInvoke);

      this.ocx.gotoFirstPage();
    }

    /// <summary>
    /// Goes to the last page in the document, maintaining the current location within the page
    /// and zoom level.
    /// </summary>
    public virtual void GotoLastPage()
    {
      if (this.ocx == null)
        throw new AxHost.InvalidActiveXStateException("gotoLastPage", AxHost.ActiveXInvokeKind.MethodInvoke);

      this.ocx.gotoLastPage();
    }

    /// <summary>
    /// Goes to the next page in the document, if it exists. Maintains the current location within the
    /// page and zoom level.
    /// </summary>
    public virtual void GotoNextPage()
    {
      if (this.ocx == null)
        throw new AxHost.InvalidActiveXStateException("gotoNextPage", AxHost.ActiveXInvokeKind.MethodInvoke);

      this.ocx.gotoNextPage();
    }

    /// <summary>
    /// Goes to the previous page in the document, if it exists. Maintains the current location
    /// within the page and zoom level.
    /// </summary>
    public virtual void GotoPreviousPage()
    {
      if (this.ocx == null)
        throw new AxHost.InvalidActiveXStateException("gotoPreviousPage", AxHost.ActiveXInvokeKind.MethodInvoke);

      this.ocx.gotoPreviousPage();
    }

    /// <summary>
    /// Opens and displays the specified document within the browser.
    /// </summary>
    public virtual bool LoadFile(string fileName)
    {
      if (this.ocx == null)
        throw new AxHost.InvalidActiveXStateException("LoadFile", AxHost.ActiveXInvokeKind.MethodInvoke);

      return this.ocx.LoadFile(fileName);
    }

    /// <summary>
    /// (This function is not documented by Adobe)
    /// </summary>
    public virtual void PostMessage(object strArray)
    {
      if (this.ocx == null)
        throw new AxHost.InvalidActiveXStateException("postMessage", AxHost.ActiveXInvokeKind.MethodInvoke);
    
      this.ocx.postMessage(strArray);
    }

    /// <summary>
    /// Prints the document according to the options selected in a user dialog box. The options
    /// include embedded printing (printing within a bounding rectangle on a given page), as well
    /// as interactive printing to a specified printer. Printing is complete when this method returns.
    /// </summary>
    public virtual void Print()
    {
      if (this.ocx == null)
        throw new AxHost.InvalidActiveXStateException("Print", AxHost.ActiveXInvokeKind.MethodInvoke);

      this.ocx.Print();
    }

    /// <summary>
    /// Prints the entire document without displaying a user dialog box. The current printer, page
    /// settings, and job settings are used. Printing is complete when this method returns.
    /// </summary>
    public virtual void PrintAll()
    {
      if (this.ocx == null)
        throw new AxHost.InvalidActiveXStateException("printAll", AxHost.ActiveXInvokeKind.MethodInvoke);

      this.ocx.printAll();
    }

    /// <summary>
    /// Prints the entire document without displaying a user dialog box, and the pages are shrunk,
    /// if necessary, to fit into the imageable area of a page in the printer. The current printer, page
    /// settings, and job settings are used. Printing is complete when this method returns.
    /// </summary>
    /// <param name="shrinkToFit">Determines whether to scale the imageable area when printing the document. A value of 0 indicates that no scaling should be used, and a positive value indicates that the pages are shrunk, if necessary, to fit into the imageable area of a page in the printer.</param>
    public virtual void PrintAllFit(bool shrinkToFit)
    {
      if (this.ocx == null)
        throw new AxHost.InvalidActiveXStateException("printAllFit", AxHost.ActiveXInvokeKind.MethodInvoke);

      this.ocx.printAllFit(shrinkToFit);
    }

    /// <summary>
    /// Prints the specified pages without displaying a user dialog box. The current printer, page
    /// settings, and job settings are used. Printing is complete when this method returns.
    /// </summary>
    /// <param name="from">The page number of the first page to be printed. The first page in a document is page 0.</param>
    /// <param name="to">The page number of the last page to be printed.</param>
    public virtual void PrintPages(int from, int to)
    {
      if (this.ocx == null)
        throw new AxHost.InvalidActiveXStateException("printPages", AxHost.ActiveXInvokeKind.MethodInvoke);

      this.ocx.printPages(from, to);
    }

    /// <summary>
    /// Prints the specified pages without displaying a user dialog box. The current printer, page
    /// settings, and job settings are used. Printing is complete when this method returns.
    /// </summary>
    /// <param name="from">The page number of the first page to be printed. The first page in a document is page 0.</param>
    /// <param name="to">The page number of the last page to be printed.</param>
    /// <param name="shrinkToFit">Specifies whether the pages will be shrunk, if necessary, to fit into the imageable area of a page in the printer.</param>
    public virtual void PrintPagesFit(int from, int to, bool shrinkToFit)
    {
      if (this.ocx == null)
        throw new AxHost.InvalidActiveXStateException("printPagesFit", AxHost.ActiveXInvokeKind.MethodInvoke);

      this.ocx.printPagesFit(from, to, shrinkToFit);
    }

    /// <summary>
    /// Prints the document according to the options selected in a user dialog box. The options
    /// include embedded printing (printing within a bounding rectangle on a given page), as well
    /// as interactive printing to a specified printer. Printing is complete when this method returns.
    /// </summary>
    public virtual void PrintWithDialog()
    {
      if (this.ocx == null)
        throw new AxHost.InvalidActiveXStateException("printWithDialog", AxHost.ActiveXInvokeKind.MethodInvoke);

      this.ocx.printWithDialog();
    }

    /// <summary>
    /// Highlights the text selection within the specified bounding rectangle on the current page.
    /// </summary>
    /// <param name="left">The distance in points from the left side of the page.</param>
    /// <param name="top">The distance in points from the top of the page.</param>
    /// <param name="right">The width of the bounding rectangle.</param>
    /// <param name="bottom">The height of the bounding rectangle.</param>
    public virtual void SetCurrentHighlight(int left, int top, int right, int bottom)
    {
      if (this.ocx == null)
        throw new AxHost.InvalidActiveXStateException("setCurrentHighlight", AxHost.ActiveXInvokeKind.MethodInvoke);

      this.ocx.setCurrentHighlight(left, top, right, bottom);
    }

    /// <summary>
    /// Goes to the specified page in the document. Maintains the current location within the page
    /// and zoom level.
    /// </summary>
    /// <param name="page">The page number of the destination page. The first page in a document is page 0.</param>
    public virtual void SetCurrentPage(int page)
    {
      if (this.ocx == null)
        throw new AxHost.InvalidActiveXStateException("setCurrentPage", AxHost.ActiveXInvokeKind.MethodInvoke);

      this.ocx.setCurrentPage(page);
    }

    /// <summary>
    /// Sets the layout mode for a page view according to the specified value.
    /// </summary>
    public virtual void SetLayoutMode(PdfPageLayout layoutMode)
    {
      if (this.ocx == null)
        throw new AxHost.InvalidActiveXStateException("setLayoutMode", AxHost.ActiveXInvokeKind.MethodInvoke);

      this.ocx.setLayoutMode(layoutMode.ToString());
    }

    /// <summary>
    /// Changes the page view to the named destination in the specified string.
    /// </summary>
    /// <param name="namedDest">The named destination to which the viewer will go.</param>
    public virtual void SetNamedDest(string namedDest)
    {
      if (this.ocx == null)
        throw new AxHost.InvalidActiveXStateException("setNamedDest", AxHost.ActiveXInvokeKind.MethodInvoke);

      this.ocx.setNamedDest(namedDest);
    }

    /// <summary>
    /// Sets the page mode according to the specified value.
    /// </summary>
    public virtual void SetPageMode(PdfPageMode pageMode)
    {
      if (this.ocx == null)
        throw new AxHost.InvalidActiveXStateException("setPageMode", AxHost.ActiveXInvokeKind.MethodInvoke);

      string mode = "";
      switch (pageMode)
      {
        case PdfPageMode.UseNone:
          mode = "none";
          break;

        case PdfPageMode.UseOutlines:
          mode = "bookmarks";
          break;

        case PdfPageMode.UseThumbs:
          mode = "thumbs";
          break;

        //case PdfPageMode.FullScreen:
        //  mode = "fullscreen";  // TODO: not documented by Adobe, value guessed...
        //  break;
        //
        //case PdfPageMode.UseOC:
        //  mode = "oc";  // TODO: not documented by Adobe, value guessed...
        //  break;
        //
        //case PdfPageMode.UseAttachments:
        //  mode = "attachments";  // TODO: not documented by Adobe, value guessed...
        //  break;
      }
      this.ocx.setPageMode(mode);
    }

    /// <summary>
    /// Determines whether scrollbars will appear in the document view.
    /// </summary>
    public virtual bool ShowScrollbars
    {
      set
      {
        if (this.ocx == null)
          throw new AxHost.InvalidActiveXStateException("setShowScrollbars", AxHost.ActiveXInvokeKind.MethodInvoke);

        this.ocx.setShowScrollbars(value);
      }
    }

    /// <summary>
    /// Determines whether a toolbar will appear in the viewer.
    /// </summary>
    public virtual bool ShowToolbar
    {
      set
      {
        if (this.ocx == null)
          throw new AxHost.InvalidActiveXStateException("setShowToolbar", AxHost.ActiveXInvokeKind.MethodInvoke);

        this.ocx.setShowToolbar(value);
      }
    }

    /// <summary>
    /// Sets the view of a page according to the specified string.
    /// </summary>
    public virtual void SetView(string viewMode)
    {
      if (this.ocx == null)
        throw new AxHost.InvalidActiveXStateException("setView", AxHost.ActiveXInvokeKind.MethodInvoke);

      this.ocx.setView(viewMode);
    }

    /// <summary>
    /// Sets the view rectangle according to the specified coordinates.
    /// </summary>
    /// <param name="left">The upper left horizontal coordinate.</param>
    /// <param name="top">The vertical coordinate in the upper left corner.</param>
    /// <param name="width">The horizontal width of the rectangle.</param>
    /// <param name="height">The vertical height of the rectangle.</param>
    public virtual void SetViewRect(double left, double top, double width, double height)
    {
      if (this.ocx == null)
        throw new AxHost.InvalidActiveXStateException("setViewRect", AxHost.ActiveXInvokeKind.MethodInvoke);

      this.ocx.setViewRect((float)left, (float)top, (float)width, (float)height);
    }

    /// <summary>
    /// Sets the view of a page according to the specified string. Depending on the view mode, the
    /// page is either scrolled to the right or scrolled down by the amount specified in offset.
    /// </summary>
    /// <param name="viewMode"></param>
    /// <param name="offset">The horizontal or vertical coordinate positioned either at the left or top edge.</param>
    public virtual void SetViewScroll(string viewMode, double offset)
    {
      if (this.ocx == null)
        throw new AxHost.InvalidActiveXStateException("setViewScroll", AxHost.ActiveXInvokeKind.MethodInvoke);

      this.ocx.setViewScroll(viewMode, (float)offset);
    }

    /// <summary>
    /// Sets the magnification according to the specified value.
    /// </summary>
    public virtual double Zoom
    {
      set
      {
        if (this.ocx == null)
          throw new AxHost.InvalidActiveXStateException("Zoom", AxHost.ActiveXInvokeKind.MethodInvoke);

        this.ocx.setZoom((float)value);
      }
    }

    /// <summary>
    /// Sets the magnification according to the specified value, and scrolls the page view both
    /// horizontally and vertically according to the specified amounts.
    /// </summary>
    /// <param name="percent">The desired zoom factor, expressed as a percentage (for example, 1.0 represents a magnification of 100%).</param>
    /// <param name="left">The horizontal coordinate positioned at the left edge.</param>
    /// <param name="top">The vertical coordinate positioned at the top edge.</param>
    public virtual void SetZoomScroll(double percent, double left, double top)
    {
      if (this.ocx == null)
        throw new AxHost.InvalidActiveXStateException("setZoomScroll", AxHost.ActiveXInvokeKind.MethodInvoke);
      this.ocx.setZoomScroll((float)percent, (float)left, (float)top);
    }

    protected override void AttachInterfaces()
    {
      try
      {
        this.ocx = (IAcroAXDocShim)base.GetOcx();
      }
      catch (Exception)
      {
      }
      Debug.Assert(this.ocx != null, "ActiveX PDF viewer control not created. Adobe Acrobat or Acrobat Reader 7.0 are probably not installed on your computer.");
    }

    protected override void CreateSink()
    {
      try
      {
        this.eventMulticaster = new AcroViewerEventMulticaster(this);
        this.cookie = new AxHost.ConnectionPointCookie(this.ocx, this.eventMulticaster, typeof(_IAcroAXDocShimEvents));
      }
      catch (Exception)
      {
      }
    }

    protected override void DetachSink()
    {
      try
      {
        this.cookie.Disconnect();
      }
      catch (Exception)
      {
      }
    }


    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), DispId(30)]
    protected virtual object messageHandler
    {
      get
      {
        if (this.ocx == null)
          throw new AxHost.InvalidActiveXStateException("messageHandler", AxHost.ActiveXInvokeKind.PropertyGet);

        return this.ocx.messageHandler;
      }
      set
      {
        if (this.ocx == null)
          throw new AxHost.InvalidActiveXStateException("messageHandler", AxHost.ActiveXInvokeKind.PropertySet);

        this.ocx.messageHandler = value;
      }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), DispId(1)]
    protected virtual string src
    {
      get
      {
        if (this.ocx == null)
          throw new AxHost.InvalidActiveXStateException("src", AxHost.ActiveXInvokeKind.PropertyGet);

        return this.ocx.src;
      }
      set
      {
        if (this.ocx == null)
          throw new AxHost.InvalidActiveXStateException("src", AxHost.ActiveXInvokeKind.PropertySet);

        this.ocx.src = value;
      }
    }

    internal void RaiseOnOnError(object sender, EventArgs e)
    {
      if (this.OnError != null)
        this.OnError(sender, e);
    }

    internal void RaiseOnOnMessage(object sender, EventArgs e)
    {
      if (this.OnMessage != null)
        this.OnMessage(sender, e);
    }

    private AxHost.ConnectionPointCookie cookie;
    private AcroViewerEventMulticaster eventMulticaster;
    private IAcroAXDocShim ocx;
  }
#endif
}
