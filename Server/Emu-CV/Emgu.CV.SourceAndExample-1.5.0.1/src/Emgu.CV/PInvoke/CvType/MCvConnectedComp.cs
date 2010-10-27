﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Emgu.CV.Structure
{
   /// <summary>
   /// Managed structure equivalent to CvConnectedComp
   /// </summary>
   [StructLayout(LayoutKind.Sequential)]
   public struct MCvConnectedComp
   {
      /// <summary>
      /// area of the segmented component
      /// </summary>
      public double area;

      /// <summary>
      /// scalar value
      /// </summary>
      public MCvScalar value;

      /// <summary>
      /// ROI of the segmented component
      /// </summary>
      public System.Drawing.Rectangle rect;

      /// <summary>
      /// Pointer to the CvSeq
      /// </summary>
      public IntPtr contour;
   }
}
