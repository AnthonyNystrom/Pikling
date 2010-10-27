using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;

namespace Emgu.CV
{
   /// <summary>
   /// Utilities class
   /// </summary>
   public static class Util
   {
      /// <summary>
      /// The ColorPalette of Grayscale for Bitmap Format8bppIndexed
      /// </summary>
      public static readonly System.Drawing.Imaging.ColorPalette GrayscalePalette = GenerateGrayscalePalette();

      private static System.Drawing.Imaging.ColorPalette GenerateGrayscalePalette()
      {
         using (Bitmap image = new Bitmap(1, 1, System.Drawing.Imaging.PixelFormat.Format8bppIndexed))
         {
            System.Drawing.Imaging.ColorPalette palette = image.Palette;
            for (int i = 0; i < 256; i++)
            {
               palette.Entries[i] = System.Drawing.Color.FromArgb(i, i, i);
            }
            return palette;
         }
      }

      /// <summary>
      /// Convert the color pallette to four lookup tables
      /// </summary>
      /// <param name="pallette">The color pallette to transform</param>
      /// <param name="bTable">Lookup table for the B channel</param>
      /// <param name="gTable">Lookup table for the G channel</param>
      /// <param name="rTable">Lookup table for the R channel</param>
      /// <param name="aTable">Lookup table for the A channel</param>
      public static void ColorPaletteToLookupTable(ColorPalette pallette, out Matrix<Byte> bTable, out Matrix<byte> gTable, out Matrix<Byte> rTable, out Matrix<Byte> aTable)
      {
         bTable = new Matrix<byte>(256, 1);
         gTable = new Matrix<byte>(256, 1);
         rTable = new Matrix<byte>(256, 1);
         aTable = new Matrix<byte>(256, 1);
         byte[,] bData = bTable.Data;
         byte[,] gData = gTable.Data;
         byte[,] rData = rTable.Data;
         byte[,] aData = aTable.Data;

         Color[] colors = pallette.Entries;
         for (int i = 0; i < colors.Length; i++)
         {
            Color c = colors[i];
            bData[i,0] = c.B;
            gData[i,0] = c.G;
            rData[i,0] = c.R;
            aData[i,0] = c.A;
         }
      }

      /// <summary>
      /// Returns information about one of or all of the registered modules
      /// </summary>
      /// <param name="pluginName">The list of names and versions of the optimized plugins that CXCORE was able to find and load</param>
      /// <param name="versionName">Information about the module(s), including version</param>
      public static void GetModuleInfo(out String pluginName, out String versionName)
      {
         IntPtr version = IntPtr.Zero;
         IntPtr plugin_info = IntPtr.Zero;
         CvInvoke.cvGetModuleInfo(IntPtr.Zero, ref version, ref plugin_info);

         pluginName = Marshal.PtrToStringAnsi(plugin_info);
         versionName = Marshal.PtrToStringAnsi(version);
      }
      
      /// <summary>
      /// Enable or diable IPL optimization for opencv
      /// </summary>
      /// <param name="enable">true to enable optimization, false to disable</param>
      public static void OptimizeCV(bool enable)
      {
         CvInvoke.cvUseOptimized(enable);
      }
   }
}
