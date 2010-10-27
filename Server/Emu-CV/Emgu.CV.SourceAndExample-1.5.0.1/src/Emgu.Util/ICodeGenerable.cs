using System;
using System.Collections.Generic;
using System.Text;

namespace Emgu.Util
{
   /// <summary>
   /// Implement this interface if the object can output code to generate it self.
   /// </summary>
   public interface ICodeGenerable
   {
      /// <summary>
      /// Return the code to generate the object itself from the specific language
      /// </summary>
      /// <param name="language">The programming language to output code</param>
      /// <returns>The code to generate the object from the specific language</returns>
      String ToCode(TypeEnum.ProgrammingLanguage language);
   }
}
