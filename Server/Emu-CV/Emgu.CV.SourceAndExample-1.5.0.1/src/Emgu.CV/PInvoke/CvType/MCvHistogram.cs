using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Emgu.CV.Structure
{
   /// <summary>
   /// Managed structure equivalent to CvMat
   /// </summary>
   [StructLayout(LayoutKind.Sequential)]
   public struct MCvHistogram
   {
      /// <summary>
      /// 
      /// </summary>
      public int type;

      /// <summary>
      /// Pointer to CvArr
      /// </summary>
      public IntPtr bins;

      /// <summary>
      /// For uniform histograms 
      /// </summary>
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)CvEnum.GENERAL.CV_MAX_DIM)]
      public RangeF[] thresh;

      /// <summary>
      /// For non-uniform histograms
      /// </summary>
      public IntPtr thresh2;

      /// <summary>
      /// Embedded matrix header for array histograms
      /// </summary>
      public MCvMatND mat;
   }
}
