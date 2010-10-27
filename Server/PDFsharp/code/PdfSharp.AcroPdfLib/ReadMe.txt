Viewing And Printing PDF Files
------------------------------

PDFsharp is designed to dynamically create and process PDF files on the fly. But it cannot render 
the content of a PDF page to a printer. Because I got a lot of questions how to view or print PDF
I wrote this tiny sample. It has actually nothing to do with the PDFsharp library, but I think it
is a useful add-on. It based on an ActiveX component of Adobe.

The Adobe Reader installs an ActiveX control that is used by the Internet Explorer to view PDF.
This control already exists in earlier version of Adobe Reader, but until Acrobat 7 it is explicit
legal to use it as a PDF viewer in external applications. This sample implements the WinForms
control PdfSharp.Viewing.PdfAcroViewer which can be used to view and print PDF files. The only
requirement is to have Adobe Acrobat Reader installed.

For more information see 'Acrobat Interapplication Communication Reference', which is found here:
http://partners.adobe.com/public/developer/en/acrobat/sdk/pdf/iac/IACReference.pdf

