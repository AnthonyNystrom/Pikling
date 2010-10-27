using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Drawing;

namespace Emgu.CV.Structure
{
   /// <summary>
   /// Result of cvHaarDetectObjects
   /// </summary>
   [StructLayout(LayoutKind.Sequential)]
   public struct MCvAvgComp
   {
      /// <summary>
      /// Bounding rectangle for the object (average rectangle of a group)
      /// </summary>
      public Rectangle rect;

      /// <summary>
      /// Number of neighbor rectangles in the group
      /// </summary>
      public int neighbors;
   }
}
