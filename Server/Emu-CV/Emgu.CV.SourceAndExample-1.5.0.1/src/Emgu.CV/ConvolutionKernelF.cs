using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Emgu.CV
{
   /// <summary>
   /// A convolution kernel 
   /// </summary>
   public class ConvolutionKernelF : Matrix<float>
   {
      /// <summary>
      /// The center of the convolution kernel
      /// </summary>
      protected System.Drawing.Point _center;

      /// <summary>
      /// Create a convolution kernel with the specific number of <paramref name="rows"/> and <paramref name="cols"/>
      /// </summary>
      /// <param name="rows">The number of raws for the convolution kernel</param>
      /// <param name="cols">The number of columns for the convolution kernel</param>
      public ConvolutionKernelF(int rows, int cols)
         : base(rows, cols)
      {
         Debug.Assert(!(rows <= 1 || cols <= 1));
         _center = new System.Drawing.Point(-1, -1);
      }

      /// <summary>
      /// Create a convolution kernel using the specific matrix and center
      /// </summary>
      /// <param name="kernel">the values for the convolution kernel</param>
      /// <param name="center">the center of the kernel</param>
      public ConvolutionKernelF(Matrix<float> kernel, System.Drawing.Point center)
         : this(kernel.Data, center)
      {
      }

      /// <summary>
      /// Create a convolution kernel using the specific floating point matrix
      /// </summary>
      /// <param name="kernel">the values for the convolution kernel</param>
      public ConvolutionKernelF(float[,] kernel)
         : this(kernel, new System.Drawing.Point(-1, -1))
      {
      }

      /// <summary>
      /// Create a convolution kernel using the specific floating point matrix and center
      /// </summary>
      /// <param name="kernel">the values for the convolution kernel</param>
      /// <param name="center">the center for the convolution kernel</param>
      public ConvolutionKernelF(float[,] kernel, System.Drawing.Point center)
         : base()
      {
         int rows = kernel.GetLength(0);
         int cols = kernel.GetLength(1);
         Debug.Assert(!(rows == 0 || cols == 0));

         if (rows == 1 || cols == 1)
         {
            float[,] data = new float[Math.Max(2, rows), Math.Max(2, cols)];
            for (int i = 0; i < rows; i++)
               for (int j = 0; j < cols; j++)
                  data[i, j] = kernel[i, j];
            Data = data;
         }
         else
         {
            Data = kernel;
         }

         _center = center;
      }

      ///<summary> Get a filpped copy of the convolution kernel</summary>
      ///<param name="flipType">The type of the flipping</param>
      ///<returns> The flipped copy of <i>this</i> image </returns>
      public ConvolutionKernelF Flip(CvEnum.FLIP flipType)
      {
         int code = 0; //flipType == Emgu.CV.CvEnum.FLIP.VERTICAL

         switch (flipType)
         {
            case (Emgu.CV.CvEnum.FLIP.HORIZONTAL | Emgu.CV.CvEnum.FLIP.VERTICAL):
               code = -1;
               break;
            case Emgu.CV.CvEnum.FLIP.HORIZONTAL:
               code = 1;
               break;
         }

         ConvolutionKernelF res = new ConvolutionKernelF(Height, Width);
         CvInvoke.cvFlip(Ptr, res.Ptr, code);

         res.Center = new System.Drawing.Point(
          (Center.X == -1 ? -1 : ((flipType & Emgu.CV.CvEnum.FLIP.HORIZONTAL) == Emgu.CV.CvEnum.FLIP.HORIZONTAL ? Width - Center.X - 1 : Center.X)),
          (Center.Y == -1 ? -1 : ((flipType & Emgu.CV.CvEnum.FLIP.VERTICAL) == Emgu.CV.CvEnum.FLIP.VERTICAL ? Height - Center.Y - 1 : Center.Y)));
         return res;
      }

      /// <summary>
      /// The center of the convolution kernel
      /// </summary>
      public System.Drawing.Point Center 
      { 
         get { return _center; }
         set { _center = value; }
      }

      /// <summary>
      /// Obtain the transpose of the convolution kernel
      /// </summary>
      /// <returns></returns>
      public new ConvolutionKernelF Transpose()
      {
         return new ConvolutionKernelF(
         base.Transpose(),
         _center);
      }
   }
}
