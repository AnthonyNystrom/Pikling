using System;
using System.Text;
using System.Drawing;

namespace Emgu.CV
{
   /// <summary>
   /// IImage interface
   /// </summary>
   public interface IImage : IDisposable, ICloneable
   {
      /// <summary>
      /// Convert this image into Bitmap 
      /// </summary>
      /// <returns></returns>
      Bitmap Bitmap
      {
         get;
      }

      /// <summary>
      /// The size of this image
      /// </summary>
      System.Drawing.Size Size
      {
         get;
      }

      /// <summary>
      /// Returns the min / max location and values for the image
      /// </summary>
      /// <returns>
      /// Returns the min / max location and values for the image
      /// </returns>
      void MinMax(out double[] minValues, out double[] maxValues, out System.Drawing.Point[] minLocations, out System.Drawing.Point[] maxLocations);

      ///<summary> 
      /// Split current IImage into an array of gray scale images where each element 
      /// in the array represent a single color channel of the original image
      ///</summary>
      ///<returns> 
      /// An array of gray scale images where each element 
      /// in the array represent a single color channel of the original image 
      ///</returns>
      IImage[] Split();

      /// <summary>
      /// Get the pointer to the unmanaged memory
      /// </summary>
      IntPtr Ptr
      {
         get;
      }
      
      /// <summary>
      /// Save the image to the specific <paramref name="fileName"/> 
      /// </summary>
      /// <param name="fileName">The file name of the image</param>
      void Save(String fileName);
      
   }
}
