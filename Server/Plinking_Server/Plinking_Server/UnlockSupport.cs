using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Leadtools;

namespace Plinking_Server
{
    public static class Support
    {
        public const string MedicalServerKey = "";

        public static bool KernelExpired
        {
            get
            {
                if (RasterSupport.KernelExpired)
                {
                    MessageBox.Show(
                       null,
                       "This library has expired.  Contact LEAD Technologies, Inc. at (704) 332-5532 to order a new version.",
                       "LEADTOOLS for .NET Evalutation Notice",
                       MessageBoxButtons.OK,
                       MessageBoxIcon.Stop);
                    return true;
                }
                else
                    return false;
            }
        }

        public static void Unlock(bool check)
        {
#if LTV15_CONFIG
         RasterSupport.Unlock(RasterSupportType.Abc, "");
         RasterSupport.Unlock(RasterSupportType.AbicRead, "");
         RasterSupport.Unlock(RasterSupportType.AbicSave, "");
         RasterSupport.Unlock(RasterSupportType.Barcodes1D, "");
         RasterSupport.Unlock(RasterSupportType.Barcodes1DSilver, "");
         RasterSupport.Unlock(RasterSupportType.BarcodesDataMatrixRead, "");
         RasterSupport.Unlock(RasterSupportType.BarcodesDataMatrixWrite, "");
         RasterSupport.Unlock(RasterSupportType.BarcodesPdfRead, "");
         RasterSupport.Unlock(RasterSupportType.BarcodesPdfWrite, "");
         RasterSupport.Unlock(RasterSupportType.BarcodesQRRead, "");
         RasterSupport.Unlock(RasterSupportType.BarcodesQRWrite, "");
         RasterSupport.Unlock(RasterSupportType.Bitonal, "");
         RasterSupport.Unlock(RasterSupportType.Cmw, "");
         RasterSupport.Unlock(RasterSupportType.Dicom, "");
         RasterSupport.Unlock(RasterSupportType.Document, "");
         RasterSupport.Unlock(RasterSupportType.ExtGray, "");
         RasterSupport.Unlock(RasterSupportType.Icr, "");
         RasterSupport.Unlock(RasterSupportType.J2k, "");
         RasterSupport.Unlock(RasterSupportType.Jbig2, "");
         RasterSupport.Unlock(RasterSupportType.Pro, "");
         RasterSupport.Unlock(RasterSupportType.Medical, "");
         RasterSupport.Unlock(RasterSupportType.MedicalNet, "");
         RasterSupport.Unlock(RasterSupportType.Mobile, "");
         RasterSupport.Unlock(RasterSupportType.Nitf, "");
         RasterSupport.Unlock(RasterSupportType.Ocr, "");
         RasterSupport.Unlock(RasterSupportType.OcrPdfOutput, "");
         RasterSupport.Unlock(RasterSupportType.PdfAdvanced, "");
         RasterSupport.Unlock(RasterSupportType.PdfRead, "");
         RasterSupport.Unlock(RasterSupportType.PdfSave, "");
         RasterSupport.Unlock(RasterSupportType.Vector, "");
#endif
#if LTV16_CONFIG
         RasterSupport.Unlock(RasterSupportType.Abc, "");
         RasterSupport.Unlock(RasterSupportType.AbicRead, "");
         RasterSupport.Unlock(RasterSupportType.AbicSave, "");
         RasterSupport.Unlock(RasterSupportType.Barcodes1D, "");
         RasterSupport.Unlock(RasterSupportType.Barcodes1DSilver, "");
         RasterSupport.Unlock(RasterSupportType.BarcodesDataMatrixRead, "");
         RasterSupport.Unlock(RasterSupportType.BarcodesDataMatrixWrite, "");
         RasterSupport.Unlock(RasterSupportType.BarcodesPdfRead, "");
         RasterSupport.Unlock(RasterSupportType.BarcodesPdfWrite, "");
         RasterSupport.Unlock(RasterSupportType.BarcodesQRRead, "");
         RasterSupport.Unlock(RasterSupportType.BarcodesQRWrite, "");
         RasterSupport.Unlock(RasterSupportType.Bitonal, "");
         RasterSupport.Unlock(RasterSupportType.Cmw, "");
         RasterSupport.Unlock(RasterSupportType.Dicom, "");
         RasterSupport.Unlock(RasterSupportType.Document, "");
         RasterSupport.Unlock(RasterSupportType.DocumentWriters, "");
         RasterSupport.Unlock(RasterSupportType.DocumentWritersPdf, "");
         RasterSupport.Unlock(RasterSupportType.ExtGray, "");
         RasterSupport.Unlock(RasterSupportType.Forms, "");
         RasterSupport.Unlock(RasterSupportType.IcrPlus, "");
         RasterSupport.Unlock(RasterSupportType.IcrProfessional, "");
         RasterSupport.Unlock(RasterSupportType.J2k, "");
         RasterSupport.Unlock(RasterSupportType.Jbig2, "");
         RasterSupport.Unlock(RasterSupportType.Jpip, "");
         RasterSupport.Unlock(RasterSupportType.Pro, "");
         RasterSupport.Unlock(RasterSupportType.LeadOmr, "");
         RasterSupport.Unlock(RasterSupportType.MediaWriter, "");
         RasterSupport.Unlock(RasterSupportType.Medical, "");
         RasterSupport.Unlock(RasterSupportType.Medical3d, "");
         RasterSupport.Unlock(RasterSupportType.MedicalNet, "");
         RasterSupport.Unlock(RasterSupportType.MedicalServer, MedicalServerKey);
         RasterSupport.Unlock(RasterSupportType.Mobile, "");
         RasterSupport.Unlock(RasterSupportType.Nitf, "");
         RasterSupport.Unlock(RasterSupportType.OcrAdvantage, "");
         RasterSupport.Unlock(RasterSupportType.OcrAdvantagePdfLeadOutput, "");
         RasterSupport.Unlock(RasterSupportType.OcrArabic, "");
         RasterSupport.Unlock(RasterSupportType.OcrPlus, "");
         RasterSupport.Unlock(RasterSupportType.OcrPlusPdfOutput, "");
         RasterSupport.Unlock(RasterSupportType.OcrPlusPdfLeadOutput, "");
         RasterSupport.Unlock(RasterSupportType.OcrProfessional, "");
         RasterSupport.Unlock(RasterSupportType.OcrProfessionalAsian, "");
         RasterSupport.Unlock(RasterSupportType.OcrProfessionalPdfOutput, "");
         RasterSupport.Unlock(RasterSupportType.OcrProfessionalPdfLeadOutput, "");
         RasterSupport.Unlock(RasterSupportType.PdfAdvanced, "");
         RasterSupport.Unlock(RasterSupportType.PdfRead, "");
         RasterSupport.Unlock(RasterSupportType.PdfSave, "");
         RasterSupport.Unlock(RasterSupportType.PrintDriver, "");
         RasterSupport.Unlock(RasterSupportType.Vector, "");
#endif

            if (check)
            {
                /*
                Array a = Enum.GetValues(typeof(RasterSupportType));
                foreach(RasterSupportType i in a)
                {
                   if(i != RasterSupportType.Vector && i != RasterSupportType.MedicalNet)
                   {
                      if(RasterSupport.IsLocked(i))
                         Messager.ShowWarning(null, string.Format("Locked: {0}", i));
                   }
                }
                */
            }
        }
    }
}
