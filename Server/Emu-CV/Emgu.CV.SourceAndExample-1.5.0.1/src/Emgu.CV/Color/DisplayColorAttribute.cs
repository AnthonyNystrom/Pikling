using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace Emgu.CV
{
   [AttributeUsage(AttributeTargets.Property)]
   internal sealed class DisplayColorAttribute : System.Attribute
   {
      public DisplayColorAttribute(int blue, int green, int red)
      {
         _displayColor = Color.FromArgb(red, green, blue);
      }

      private Color _displayColor;

      public Color DisplayColor
      {
         get { return _displayColor; }
         set { _displayColor = value; }
      }
   }
}
