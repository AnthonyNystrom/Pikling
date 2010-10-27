using System;
using System.Collections.Generic;
using System.Text;
using Emgu.CV.Structure;

namespace Emgu.CV
{
   ///<summary>
   /// A color type
   ///</summary>
   public interface IColor
   {
      /// <summary>
      /// The equivalent MCvScalar value
      /// </summary>
      MCvScalar MCvScalar
      {
         get;
         set;
      }

      /// <summary>
      /// Get the dimension of the color type
      /// </summary>
      int Dimension
      {
         get;
      }
   }
}
