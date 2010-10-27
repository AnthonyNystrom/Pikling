using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Emgu.CV.UI
{
   /// <summary>
   /// A view for histogram
   /// </summary>
   public partial class HistogramViewer : Form
   {
      /// <summary>
      /// A histogram viewer
      /// </summary>
      public HistogramViewer()
      {
         InitializeComponent();
      }

      /// <summary>
      /// Display the histograms of the specific image
      /// </summary>
      /// <param name="image">The image to retrieve hostigram from</param>
      public static void Show(IImage image)
      {
         Show(image, 256);
      }

      /// <summary>
      /// Display the histograms of the specific image
      /// </summary>
      /// <param name="image">The image to retrieve hostigram from</param>
      /// <param name="numberOfBins">The numebr of bins in the histogram</param>
      public static void Show(IImage image, int numberOfBins)
      {
         HistogramViewer viewer = new HistogramViewer();
         viewer.HistogramCtrl.GenerateHistograms(image, numberOfBins);
         viewer.HistogramCtrl.Refresh();
         viewer.Show();
      }

      /// <summary>
      /// Display the specific histogram
      /// </summary>
      /// <param name="hist">The histogram to be displayed</param>
      /// <param name="title">The name of the histogram</param>
      public static void Show(Histogram hist, string title)
      {
         HistogramViewer viewer = new HistogramViewer();
         viewer.HistogramCtrl.AddHistogram(title, Color.Black, hist);
         viewer.HistogramCtrl.Refresh();
         viewer.Show();
      }

      /// <summary>
      /// Get the histogram control of this viewer
      /// </summary>
      public HistogramCtrl HistogramCtrl
      {
         get
         {
            return histogramCtrl1;
         }
      }
   }
}
