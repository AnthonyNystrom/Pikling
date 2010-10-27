using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Xml.Serialization;
using Emgu.CV.Reflection;
using Emgu.CV.Structure;
using Emgu.Util;

namespace Emgu.CV
{
   /// <summary>
   /// A wrapper for IplImage
   /// </summary>
   /// <typeparam name="TColor">Color type of this image (either Gray, Bgr, Bgra, Hsv, Hls, Lab, Luv, Xyz or Ycc)</typeparam>
   /// <typeparam name="TDepth">Depth of this image (either Byte, SByte, Single, double, UInt16, Int16 or Int32)</typeparam>
   [Serializable]
   public class Image<TColor, TDepth> 
      : CvArray<TDepth>, IImage, IEquatable<Image<TColor, TDepth>> 
      where TColor : struct, IColor
   {
      private TDepth[, ,] _array;

      /// <summary>
      /// File formats supported by Bitmap. Image are converted to Bitmap then perform file operations if the file type belongs to one of following format.
      /// </summary>
      public static String[] BitmapFormats = new string[] { ".jpg", ".jpeg", ".gif", ".exig", ".png", ".tiff", ".bmp", ".tif" };

      /// <summary>
      /// The dimension of color
      /// </summary>
      private static int _numberOfChannels;

      /// <summary>
      /// Offset of roi
      /// </summary>
      private static readonly int _roiOffset = (int)Marshal.OffsetOf(typeof(MIplImage), "roi");

      #region constructors
      ///<summary>
      ///Create an empty Image
      ///</summary>
      protected Image()
      {
      }

      /// <summary>
      /// Create image from the specific multi-dimensional data, where the 1st dimesion is # of rows (height), the 2nd dimension is # cols (cols) and the 3rd dimension is the channel
      /// </summary>
      /// <param name="data">The multi-dimensional data where the 1st dimesion is # of rows (height), the 2nd dimension is # cols (cols) and the 3rd dimension is the channel </param>
      public Image(TDepth[, ,] data)
      {
         Data = data;
      }

      /// <summary>
      /// Read image from a file
      /// </summary>
      /// <param name="fileName">the name of the file that contains the image</param>
      public Image(String fileName)
      {
         FileInfo fi = new FileInfo(fileName);

         if (Array.Exists(OpencvFileFormats, fi.Extension.ToLower().Equals))
         {  //if the file can be imported from Open CV
            try
            {
               LoadImageUsingOpenCV(fi);
            }
            catch
            {  //give Bitmap a try
               //and if it cannot load the image, exception will be thrown
               LoadFileUsingBitmap(fi);
            }
            return;
         }

         //if the file format is not recognized by OpenCV
         try
         {
            LoadFileUsingBitmap(fi);
         }
         catch
         {  //give OpenCV a try any way, 
            //and if it cannot load the image, exception will be thrown
            LoadImageUsingOpenCV(fi);
         }
      }

      /// <summary>
      /// Load the specific file using Bitmap
      /// </summary>
      /// <param name="fi"></param>
      private void LoadFileUsingBitmap(FileInfo fi)
      {
         using (Bitmap bmp = new Bitmap(fi.FullName))
            Bitmap = bmp;
      }

      /// <summary>
      /// Load the specific file using OpenCV
      /// </summary>
      /// <param name="file"></param>
      private void LoadImageUsingOpenCV(FileInfo file)
      {
         IntPtr ptr;
         System.Drawing.Size size;

         #region read the image into ptr ( of TColor, Byte )
         if (typeof(TColor) == typeof(Gray)) //TColor type is gray, load the image as grayscale
         {
            ptr = CvInvoke.cvLoadImage(file.FullName, Emgu.CV.CvEnum.LOAD_IMAGE_TYPE.CV_LOAD_IMAGE_GRAYSCALE);
            size = CvInvoke.cvGetSize(ptr);
         }
         else //color type is not gray
         {
            //load the image as Bgr color
            ptr = CvInvoke.cvLoadImage(file.FullName, CvEnum.LOAD_IMAGE_TYPE.CV_LOAD_IMAGE_COLOR);

            if (ptr == IntPtr.Zero)
               throw new NullReferenceException(String.Format("Unable to load image from file \"{0}\".", file.FullName));

            MIplImage mptr = (MIplImage)Marshal.PtrToStructure(ptr, typeof(MIplImage));
            size = new Size(mptr.width, mptr.height);

            if (typeof(TColor) != typeof(Bgr)) //TColor type is not Bgr, a conversion is required
            {
               IntPtr tmp = CvInvoke.cvCreateImage(
                   size,
                   (CvEnum.IPL_DEPTH)mptr.depth,
                   3);
               CvInvoke.cvCvtColor(ptr, tmp, GetColorCvtCode(typeof(Bgr), typeof(TColor)));

               CvInvoke.cvReleaseImage(ref ptr);
               ptr = tmp;
            }
         }
         #endregion

         if (typeof(TDepth) != typeof(Byte)) //depth is not Byte, a conversion of depth is required
         {
            IntPtr tmp = CvInvoke.cvCreateImage(
                size,
                CvDepth,
                NumberOfChannels);
            CvInvoke.cvConvertScale(ptr, tmp, 1.0, 0.0);
            CvInvoke.cvReleaseImage(ref ptr);
            ptr = tmp;
         }

         #region use managed memory instead of unmanaged
         AllocateData(size.Height, size.Width, NumberOfChannels);

         CvInvoke.cvCopy(ptr, Ptr, IntPtr.Zero);

         CvInvoke.cvReleaseImage(ref ptr);
         #endregion
      }

      /// <summary>
      /// Obtain the image from the specific Bitmap
      /// </summary>
      /// <param name="bmp">The bitmap which will be converted to the image</param>
      public Image(Bitmap bmp)
      {
         Bitmap = bmp;
      }

      ///<summary>
      ///Create a blank Image of the specified width, height and color.
      ///</summary>
      ///<param name="width">The width of the image</param>
      ///<param name="height">The height of the image</param>
      ///<param name="value">The initial color of the image</param>
      public Image(int width, int height, TColor value)
         : this(width, height)
      {
         SetValue(value);
      }

      ///<summary>
      ///Create a blank Image of the specified width and height. 
      ///</summary>
      ///<param name="width">The width of the image</param>
      ///<param name="height">The height of the image</param>
      public Image(int width, int height)
      {
         AllocateData(height, width, NumberOfChannels);
      }

      /// <summary>
      /// Create a blank Image of the specific size
      /// </summary>
      /// <param name="size">The size of the image</param>
      public Image(System.Drawing.Size size)
         : this(size.Width, size.Height)
      {
      }

      /// <summary>
      /// Get or Set the data for this matrix
      /// </summary>
      /// <remarks> 
      /// The Get function has O(1) complexity. 
      /// If the image contains Byte and width is not a multiple of 4. The second dimension of the array might be larger than the Width of this image.  
      /// This is necessary since the length of a row need to be 4 align for OpenCV optimization. 
      /// The Set function always make a copy of the specific value. If the image contains Byte and width is not a multiple of 4. The second dimension of the array created might be larger than the Width of this image.  
      /// </remarks>
      public TDepth[, ,] Data
      {
         get
         {
            return _array;
         }
         set
         {
            Debug.Assert(value != null, "The Array cannot be null");
            Debug.Assert(value.GetLength(2) == NumberOfChannels, "The number of channels must equal");
            AllocateData(value.GetLength(0), value.GetLength(1), NumberOfChannels);
            int rows = value.GetLength(0);
            int valueRowLength = value.GetLength(1) * value.GetLength(2);
            int arrayRowLength = _array.GetLength(1) * _array.GetLength(2);
            for (int i = 0; i < rows; i++)
               Array.Copy(value, i * valueRowLength, _array, i * arrayRowLength, valueRowLength);
         }
      }

      /// <summary>
      /// Allocate data for the array
      /// </summary>
      /// <param name="rows">The number of rows</param>
      /// <param name="cols">The number of columns</param>
      /// <param name="numberOfChannels">The number of channels of this image</param>
      protected override void AllocateData(int rows, int cols, int numberOfChannels)
      {
         DisposeObject();
         Debug.Assert(!_dataHandle.IsAllocated, "Handle should be free");

         _ptr = CvInvoke.cvCreateImageHeader(new System.Drawing.Size(cols, rows), CvDepth, numberOfChannels);
         GC.AddMemoryPressure(StructSize.MIplImage);

         Debug.Assert(MIplImage.align == 4, "Only 4 align is supported at this moment");

         if (typeof(TDepth) == typeof(Byte) && (cols & 3) != 0)
         {   //if the managed data isn't 4 aligned, make it so
            _array = new TDepth[rows, (cols &(~3)) + 4, numberOfChannels];
         }
         else
         {
            _array = new TDepth[rows, cols, numberOfChannels];
         }

         _dataHandle = GCHandle.Alloc(_array, GCHandleType.Pinned);
         CvInvoke.cvSetData(_ptr, _dataHandle.AddrOfPinnedObject(), _array.GetLength(1) * _array.GetLength(2) * _sizeOfElement);
      }

      ///<summary>
      ///Create a multi-channel image from multiple gray scale images
      ///</summary>
      ///<param name="channels">The image channels to be merged into a single image</param>
      public Image(Image<Gray, TDepth>[] channels)
      {
         Debug.Assert(NumberOfChannels == channels.Length);
         AllocateData(channels[0].Height, channels[0].Width, NumberOfChannels);

         if (NumberOfChannels == 1)
         {
            //if this image only have a single channel
            CvInvoke.cvCopy(channels[0].Ptr, Ptr, IntPtr.Zero);
         }
         else
         {
            for (int i = 0; i < NumberOfChannels; i++)
            {
               Image<Gray, TDepth> c = channels[i];

               Debug.Assert(EqualSize(c), String.Format("The size of the {0}th channel is different from the 1st channel", i + 1));

               CvInvoke.cvSetImageCOI(Ptr, i + 1);
               CvInvoke.cvCopy(c.Ptr, Ptr, IntPtr.Zero);
            }
            CvInvoke.cvSetImageCOI(Ptr, 0);
         }
      }
      #endregion

      #region Implement ISerializable interface
      /// <summary>
      /// Constructor used to deserialize runtime serialized object
      /// </summary>
      /// <param name="info">The serialization info</param>
      /// <param name="context">The streaming context</param>
      public Image(SerializationInfo info, StreamingContext context)
      {
         base.DeserializeObjectData(info, context);
         ROI = (System.Drawing.Rectangle)info.GetValue("Roi", typeof(System.Drawing.Rectangle));
      }

      /// <summary>
      /// A function used for runtime serilization of the object
      /// </summary>
      /// <param name="info">Serialization info</param>
      /// <param name="context">streaming context</param>
      [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
      public override void GetObjectData(SerializationInfo info, StreamingContext context)
      {
         base.GetObjectData(info, context);
         info.AddValue("Roi", ROI);
      }
      #endregion

      #region Image Properties

      /// <summary>
      /// The IplImage structure
      /// </summary>
      public MIplImage MIplImage
      {
         get
         {
            return (MIplImage)Marshal.PtrToStructure(Ptr, typeof(MIplImage));
         }
      }

      ///<summary> 
      /// Get or Set the region of interest for this image. To clear the ROI, set it to System.Drawing.Rectangle.Empty
      ///</summary>
      public System.Drawing.Rectangle ROI
      {
         set
         {
            if (value.Equals(Rectangle.Empty))
            {
               //reset the image ROI
               CvInvoke.cvResetImageROI(Ptr);
            }
            else
            {   //set the image ROI to the specific value
               CvInvoke.cvSetImageROI(Ptr, value);
            }
         }
         get
         {
            //return the image ROI
            return CvInvoke.cvGetImageROI(Ptr);
         }
      }

      /// <summary>
      /// Get the number of channels for this image
      /// </summary>
      public override int NumberOfChannels
      {
         get
         {
            if (_numberOfChannels == 0)
            {
               _numberOfChannels = new TColor().Dimension;
            }
            return _numberOfChannels;
         }
      }

      /// <summary>
      /// Get the underneath managed array
      /// </summary>
      public override System.Array ManagedArray
      {
         get { return _array; }
      }

      /// <summary>
      /// Get the equivalent opencv depth type for this image
      /// </summary>
      public static CvEnum.IPL_DEPTH CvDepth
      {
         get
         {
            Type typeOfDepth = typeof(TDepth);

            if (typeOfDepth == typeof(Single))
               return CvEnum.IPL_DEPTH.IPL_DEPTH_32F;
            else if (typeOfDepth == typeof(Byte))
               return CvEnum.IPL_DEPTH.IPL_DEPTH_8U;
            else if (typeOfDepth == typeof(Double))
               return CvEnum.IPL_DEPTH.IPL_DEPTH_64F;
            else if (typeOfDepth == typeof(SByte))
               return Emgu.CV.CvEnum.IPL_DEPTH.IPL_DEPTH_8S;
            else if (typeOfDepth == typeof(UInt16))
               return Emgu.CV.CvEnum.IPL_DEPTH.IPL_DEPTH_16U;
            else if (typeOfDepth == typeof(Int16))
               return Emgu.CV.CvEnum.IPL_DEPTH.IPL_DEPTH_16S;
            else if (typeOfDepth == typeof(Int32))
               return Emgu.CV.CvEnum.IPL_DEPTH.IPL_DEPTH_32S;
            else
               throw new NotImplementedException("Unsupported image depth");
         }
      }

      ///<summary> 
      ///Indicates if the region of interest has been set
      ///</summary> 
      public bool IsROISet
      {
         get
         {
            return Marshal.ReadIntPtr(Ptr, _roiOffset) != IntPtr.Zero;
         }
      }

      /// <summary>
      /// Get the average value on this image
      /// </summary>
      /// <returns>The average color of the image</returns>
      public TColor GetAverage()
      {
         return GetAverage(null);
      }

      /// <summary>
      /// Get the average value on this image, using the specific mask
      /// </summary>
      /// <param name="mask">The mask for find the average value</param>
      /// <returns>The average color of the masked area</returns>
      public TColor GetAverage(Image<Gray, Byte> mask)
      {
         TColor res = new TColor();
         res.MCvScalar = CvInvoke.cvAvg(Ptr, mask == null ? IntPtr.Zero : mask.Ptr);
         return res;
      }

      ///<summary>Get the sum for each color channel </summary>
      public TColor GetSum()
      {
         TColor res = new TColor();
         res.MCvScalar = CvInvoke.cvSum(Ptr);
         return res;
      }
      #endregion

      #region Coping and Filling
      /// <summary>
      /// Set every pixel of the image to the specific color 
      /// </summary>
      /// <param name="color">The color to be set</param>
      public void SetValue(TColor color)
      {
         SetValue(color.MCvScalar);
      }

      /// <summary>
      /// Set every pixel of the image to the specific color, using a mask
      /// </summary>
      /// <param name="color">The color to be set</param>
      /// <param name="mask">The mask for setting color</param>
      public void SetValue(TColor color, Image<Gray, Byte> mask)
      {
         SetValue(color.MCvScalar, mask);
      }

      /// <summary>
      /// Copy the masked area of this image to destination
      /// </summary>
      /// <param name="dest">the destination to copy to</param>
      /// <param name="mask">the mask for copy</param>
      public void Copy(Image<TColor, TDepth> dest, Image<Gray, Byte> mask)
      {
         CvInvoke.cvCopy(Ptr, dest.Ptr, mask == null ? IntPtr.Zero : mask.Ptr);
      }

      ///<summary> 
      /// Make a copy of the image using a mask, if ROI is set, only copy the ROI 
      /// </summary> 
      /// <param name="mask">the mask for coping</param>
      ///<returns> A copy of the image</returns>
      public Image<TColor, TDepth> Copy(Image<Gray, Byte> mask)
      {
         Image<TColor, TDepth> res = new Image<TColor, TDepth>(Size);
         Copy(res, mask);
         return res;
      }

      /// <summary>
      /// Make a copy of the specific ROI (Region of Interest)
      /// </summary>
      /// <param name="roi">The roi to be copied</param>
      /// <returns>The image of the specific roi</returns>
      public Image<TColor, TDepth> Copy(System.Drawing.Rectangle roi)
      {
         Rectangle currentRoi = ROI; //cache the current roi
         Image<TColor, TDepth> res = new Image<TColor, TDepth>(roi.Size);
         ROI = roi;
         CvInvoke.cvCopy(Ptr, res.Ptr, IntPtr.Zero);
         ROI = currentRoi; //reset the roi
         return res;
      }

      ///<summary> Make a copy of the image, if ROI is set, only copy the ROI</summary>
      ///<returns> A copy of the image</returns>
      public Image<TColor, TDepth> Copy()
      {
         return Copy(null);
      }

      /// <summary> 
      /// Create an image of the same size
      /// </summary>
      /// <remarks>The initial pixel in the image equals zero</remarks>
      /// <returns> The image of the same size</returns>
      public Image<TColor, TDepth> CopyBlank()
      {
         return new Image<TColor, TDepth>(Size);
      }

      /// <summary>
      /// Make a clone of the current image. All image data as well as the COI and ROI are cloned
      /// </summary>
      /// <returns>A clone of the current image. All image data as well as the COI and ROI are cloned</returns>
      public Image<TColor, TDepth> Clone()
      {
         int coi = CvInvoke.cvGetImageCOI(Ptr); //get the COI for current image
         System.Drawing.Rectangle roi = ROI; //get the ROI for current image

         CvInvoke.cvSetImageCOI(Ptr, 0); //clear COI for current image
         ROI = Rectangle.Empty; // clear ROI for current image

         #region create a clone of the current image with the same COI and ROI
         Image<TColor, TDepth> res = Copy();
         CvInvoke.cvSetImageCOI(res.Ptr, coi);
         res.ROI = roi;
         #endregion

         CvInvoke.cvSetImageCOI(Ptr, coi); //reset the COI for the current image
         ROI = roi; // reset the ROI for the current image

         return res;
      }

      /// <summary>
      /// Get a subimage which image data is shared with the current image.
      /// </summary>
      /// <param name="rect">The rectangle area of the sub-image</param>
      /// <returns>A subimage which image data is shared with the current image</returns>
      public Image<TColor, TDepth> GetSubRect(System.Drawing.Rectangle rect)
      {
         Image<TColor, TDepth> subRect = new Image<TColor, TDepth>();
         subRect._array = _array;
         subRect._ptr = CvInvoke.cvCreateImageHeader(rect.Size, CvDepth, NumberOfChannels); 
         
         GC.AddMemoryPressure(StructSize.MIplImage); //This pressure will be released once the return image is disposed. 

         IntPtr matPtr = Marshal.AllocHGlobal(StructSize.MCvMat);
         CvInvoke.cvGetSubRect(_ptr, matPtr, rect);
         CvInvoke.cvGetImage(matPtr, subRect.Ptr);
         Marshal.FreeHGlobal(matPtr);

         return subRect;
      }

      #endregion

      #region Drawing functions
      ///<summary> Draw a box of the specific color and thickness </summary>
      ///<param name="box"> The box to be drawn</param>
      ///<param name="color"> The color of the rectangle </param>
      ///<param name="thickness"> If thickness is less than 1, the rectangle is filled up </param>
      ///<typeparam name="T">The type of Box2D to draw</typeparam>
      public virtual void Draw<T>(MCvBox2D box, TColor color, int thickness) where T : struct, IComparable
      {
         Draw<float>(box, color, thickness);
      }

      ///<summary> Draw an Rectangle of the specific color and thickness </summary>
      ///<param name="rect"> The rectangle to be drawn</param>
      ///<param name="color"> The color of the rectangle </param>
      ///<param name="thickness"> If thickness is less than 1, the rectangle is filled up </param>
      public virtual void Draw(System.Drawing.Rectangle rect, TColor color, int thickness)
      {
         CvInvoke.cvRectangle(
             Ptr,
             rect.Location,
             Point.Add(rect.Location, rect.Size),
             color.MCvScalar,
             (thickness <= 0) ? -1 : thickness,
             CvEnum.LINE_TYPE.EIGHT_CONNECTED,
             0);
      }

      ///<summary> Draw a 2D Cross using the specific color and thickness </summary>
      ///<param name="cross"> The 2D Cross to be drawn</param>
      ///<param name="color"> The color of the cross </param>
      ///<param name="thickness"> Must be &gt; 0 </param>
      public void Draw(Cross2DF cross, TColor color, int thickness) 
      {
         Debug.Assert(thickness > 0, "Thickness should be > 0");
         if (thickness > 0)
         {
            Draw(cross.Horizontal, color, thickness);
            Draw(cross.Vertical, color, thickness);
         }
      }
      ///<summary> Draw a line segment using the specific color and thickness </summary>
      ///<param name="line"> The line segment to be drawn</param>
      ///<param name="color"> The color of the line segment </param>
      ///<param name="thickness"> The thickness of the line segment </param>
      public virtual void Draw(LineSegment2DF line, TColor color, int thickness)
      {
         Debug.Assert(thickness > 0, "Thickness should be > 0");
         if (thickness > 0)
            CvInvoke.cvLine(
                Ptr,
                Point.Round(line.P1),
                Point.Round(line.P2),
                color.MCvScalar,
                thickness,
                CvEnum.LINE_TYPE.EIGHT_CONNECTED,
                0);
      }

      ///<summary> Draw a line segment using the specific color and thickness </summary>
      ///<param name="line"> The line segment to be drawn</param>
      ///<param name="color"> The color of the line segment </param>
      ///<param name="thickness"> The thickness of the line segment </param>
      public virtual void Draw(LineSegment2D line, TColor color, int thickness)
      {
         Debug.Assert(thickness > 0, "Thickness should be > 0");
         if (thickness > 0)
            CvInvoke.cvLine(
                Ptr,
                line.P1,
                line.P2,
                color.MCvScalar,
                thickness,
                CvEnum.LINE_TYPE.EIGHT_CONNECTED,
                0);
      }

      ///<summary> Draw a convex polygon using the specific color and thickness </summary>
      ///<param name="polygon"> The convex polygon to be drawn</param>
      ///<param name="color"> The color of the triangle </param>
      ///<param name="thickness"> If thickness is less than 1, the triangle is filled up </param>
      public virtual void Draw(IConvexPolygonF polygon, TColor color, int thickness) 
      {
         Point[] vertices = Array.ConvertAll<PointF, Point>(polygon.GetVertices(), Point.Round);

         if (thickness > 0)
            DrawPolyline(vertices, true, color, thickness);
         else
         {
            FillConvexPoly(vertices, color);
         }
      }

      /// <summary>
      /// Fill the convex polygon with the specific color
      /// </summary>
      /// <param name="pts">The array of points that define the convex polygon</param>
      /// <param name="color">The color to fill the polygon with</param>
      public void FillConvexPoly(System.Drawing.Point[] pts, TColor color)
      {
         CvInvoke.cvFillConvexPoly(Ptr, pts, pts.Length, color.MCvScalar, Emgu.CV.CvEnum.LINE_TYPE.EIGHT_CONNECTED, 0);
      }

      /// <summary>
      /// Draw the polyline defined by the array of 2D points
      /// </summary>
      /// <param name="pts">the points that defines the poly line</param>
      /// <param name="isClosed">if true, the last line segment is defined by the last point of the array and the first point of the array</param>
      /// <param name="color">the color used for drawing</param>
      /// <param name="thickness">the thinkness of the line</param>
      public virtual void DrawPolyline<T>(Point[] pts, bool isClosed, TColor color, int thickness) where T : struct, IComparable
      {
         DrawPolyline(
             pts,
             isClosed,
             color,
             thickness);
      }

      /// <summary>
      /// Draw the polyline defined by the array of 2D points
      /// </summary>
      /// <param name="pts">A polyline defined by its point</param>
      /// <param name="isClosed">if true, the last line segment is defined by the last point of the array and the first point of the array</param>
      /// <param name="color">the color used for drawing</param>
      /// <param name="thickness">the thinkness of the line</param>
      public void DrawPolyline(System.Drawing.Point[] pts, bool isClosed, TColor color, int thickness)
      {
         DrawPolyline(new System.Drawing.Point[][] { pts }, isClosed, color, thickness);
      }

      /// <summary>
      /// Draw the polylines defined by the array of array of 2D points
      /// </summary>
      /// <param name="pts">An array of polylines each represented by an array of points</param>
      /// <param name="isClosed">if true, the last line segment is defined by the last point of the array and the first point of the array</param>
      /// <param name="color">the color used for drawing</param>
      /// <param name="thickness">the thinkness of the line</param>
      public void DrawPolyline(System.Drawing.Point[][] pts, bool isClosed, TColor color, int thickness)
      {
         if (thickness > 0)
         {
            GCHandle[] handles = Array.ConvertAll<System.Drawing.Point[], GCHandle>(pts, delegate(System.Drawing.Point[] polyline) { return GCHandle.Alloc(polyline, GCHandleType.Pinned); });

            CvInvoke.cvPolyLine(
                Ptr,
                Array.ConvertAll<GCHandle, IntPtr>(handles, delegate(GCHandle h) { return h.AddrOfPinnedObject(); }),
                Array.ConvertAll<System.Drawing.Point[], int>(pts, delegate(System.Drawing.Point[] polyline) { return polyline.Length; }),
                pts.Length,
                isClosed,
                color.MCvScalar,
                thickness,
                Emgu.CV.CvEnum.LINE_TYPE.EIGHT_CONNECTED,
                0);

            foreach (GCHandle h in handles)
               h.Free();
         }
      }

      ///<summary> Draw a Circle of the specific color and thickness </summary>
      ///<param name="circle"> The circle to be drawn</param>
      ///<param name="color"> The color of the circle </param>
      ///<param name="thickness"> If thickness is less than 1, the circle is filled up </param>
      public virtual void Draw(CircleF circle, TColor color, int thickness)
      {
         CvInvoke.cvCircle(
             Ptr,
             Point.Round(circle.Center),
             (int) circle.Radius,
             color.MCvScalar,
             (thickness <= 0) ? -1 : thickness,
             CvEnum.LINE_TYPE.EIGHT_CONNECTED,
             0);
      }

      ///<summary> Draw a Ellipse of the specific color and thickness </summary>
      ///<param name="ellipse"> The ellipse to be draw</param>
      ///<param name="color"> The color of the ellipse </param>
      ///<param name="thickness"> If thickness is less than 1, the ellipse is filled up </param>
      public void Draw(Ellipse ellipse, TColor color, int thickness) 
      {
         CvInvoke.cvEllipse(
             Ptr,
             Point.Round(ellipse.MCvBox2D.center),
             new System.Drawing.Size(( (int)ellipse.MCvBox2D.size.Width) >> 1, ((int)ellipse.MCvBox2D.size.Height) >> 1),
             ellipse.MCvBox2D.angle,
             0.0,
             360.0,
             color.MCvScalar,
             (thickness <= 0) ? -1 : thickness,
             CvEnum.LINE_TYPE.EIGHT_CONNECTED,
             0);
      }

      /// <summary>
      /// Draw the text using the specific font on the image
      /// </summary>
      /// <param name="message">The text message to be draw</param>
      /// <param name="font">The font used for drawing</param>
      /// <param name="bottomLeft">The location of the bottom left corner of the font</param>
      /// <param name="color">The color of the text</param>
      public virtual void Draw(String message, ref MCvFont font, System.Drawing.Point bottomLeft, TColor color)
      {
         CvInvoke.cvPutText(
             Ptr,
             message,
             bottomLeft,
             ref font,
             color.MCvScalar);
      }

      /// <summary>
      /// Draws contour outlines in the image if thickness&gt;=0 or fills area bounded by the contours if thickness&lt;0
      /// </summary>
      /// <param name="c">Pointer to the contour</param>
      /// <param name="color">Color of the contour</param>
      /// <param name="thickness">Thickness of lines the contours are drawn with. If it is negative, the contour interiors are drawn</param>
      public void Draw(Seq<System.Drawing.Point> c, TColor color, int thickness)
      {
         Draw(c, color, color, 0, thickness, Point.Empty);
      }

      /// <summary>
      /// Draws contour outlines in the image if thickness&gt;=0 or fills area bounded by the contours if thickness&lt;0
      /// </summary>
      /// <param name="c">Pointer to the first contour</param>
      /// <param name="externalColor">Color of the external contours</param>
      /// <param name="holeColor">Color of internal contours (holes). </param>
      /// <param name="maxLevel">
      /// Maximal level for drawn contours. 
      /// If 0, only contour is drawn. 
      /// If 1, the contour and all contours after it on the same level are drawn. 
      /// If 2, all contours after and all contours one level below the contours are drawn, etc. If the value is negative, the function does not draw the contours following after contour but draws child contours of contour up to abs(maxLevel)-1 level
      /// </param>
      /// <param name="thickness">Thickness of lines the contours are drawn with. If it is negative, the contour interiors are drawn</param>
      public void Draw(Seq<System.Drawing.Point> c, TColor externalColor, TColor holeColor, int maxLevel, int thickness)
      {
         Draw(c, externalColor, holeColor, maxLevel, thickness, Point.Empty);
      }

      /// <summary>
      /// Draws contour outlines in the image if thickness&gt;=0 or fills area bounded by the contours if thickness&lt;0
      /// </summary>
      /// <param name="c">Pointer to the first contour</param>
      /// <param name="externalColor">Color of the external contours</param>
      /// <param name="holeColor">Color of internal contours (holes). </param>
      /// <param name="maxLevel">
      /// Maximal level for drawn contours. 
      /// If 0, only contour is drawn. 
      /// If 1, the contour and all contours after it on the same level are drawn. 
      /// If 2, all contours after and all contours one level below the contours are drawn, etc. If the value is negative, the function does not draw the contours following after contour but draws child contours of contour up to abs(maxLevel)-1 level
      /// </param>
      /// <param name="thickness">Thickness of lines the contours are drawn with. If it is negative, the contour interiors are drawn</param>
      /// <param name="offset">Shift all the point coordinates by the specified value. It is useful in case if the contours retrived in some image ROI and then the ROI offset needs to be taken into account during the rendering</param>
      public void Draw(Seq<System.Drawing.Point> c, TColor externalColor, TColor holeColor, int maxLevel, int thickness, System.Drawing.Point offset)
      {
         CvInvoke.cvDrawContours(
             Ptr,
             c.Ptr,
             externalColor.MCvScalar,
             holeColor.MCvScalar,
             maxLevel,
             thickness,
             CvEnum.LINE_TYPE.EIGHT_CONNECTED,
             offset);
      }
      #endregion

      #region Object Detection
      #region Haar detection
      /// <summary>
      /// Detect HaarCascade object in the current image, using predifined parameters
      /// </summary>
      /// <param name="haarObj">The object to be detected</param>
      /// <returns>The objects detected, one array per channel</returns>
      public MCvAvgComp[][] DetectHaarCascade(HaarCascade haarObj)
      {
         return DetectHaarCascade(haarObj, 1.1, 3, CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING, new System.Drawing.Size(0, 0));
      }

      /// <summary>
      /// Finds rectangular regions in the given image that are likely to contain objects the cascade has been trained for and returns those regions as a sequence of rectangles. The function scans the image several times at different scales (see cvSetImagesForHaarClassifierCascade). Each time it considers overlapping regions in the image and applies the classifiers to the regions using cvRunHaarClassifierCascade. It may also apply some heuristics to reduce number of analyzed regions, such as Canny prunning. After it has proceeded and collected the candidate rectangles (regions that passed the classifier cascade), it groups them and returns a sequence of average rectangles for each large enough group. The default parameters (scale_factor=1.1, min_neighbors=3, flags=0) are tuned for accurate yet slow object detection. For a faster operation on real video images the settings are: scale_factor=1.2, min_neighbors=2, flags=CV_HAAR_DO_CANNY_PRUNING, min_size=&lt;minimum possible face size&gt; (for example, ~1/4 to 1/16 of the image area in case of video conferencing). 
      /// </summary>
      /// <param name="haarObj">Haar classifier cascade in internal representation</param>
      /// <param name="scaleFactor">The factor by which the search window is scaled between the subsequent scans, for example, 1.1 means increasing window by 10%</param>
      /// <param name="minNeighbors">Minimum number (minus 1) of neighbor rectangles that makes up an object. All the groups of a smaller number of rectangles than min_neighbors-1 are rejected. If min_neighbors is 0, the function does not any grouping at all and returns all the detected candidate rectangles, which may be useful if the user wants to apply a customized grouping procedure</param>
      /// <param name="flag">Mode of operation. Currently the only flag that may be specified is CV_HAAR_DO_CANNY_PRUNING. If it is set, the function uses Canny edge detector to reject some image regions that contain too few or too much edges and thus can not contain the searched object. The particular threshold values are tuned for face detection and in this case the pruning speeds up the processing.</param>
      /// <param name="minSize">Minimum window size. By default, it is set to the size of samples the classifier has been trained on (~20x20 for face detection)</param>
      /// <returns>The objects detected, one array per channel</returns>
      public MCvAvgComp[][] DetectHaarCascade(HaarCascade haarObj, double scaleFactor, int minNeighbors, CvEnum.HAAR_DETECTION_TYPE flag, System.Drawing.Size minSize)
      {
         using (MemStorage stor = new MemStorage())
         {
            Emgu.Util.Toolbox.Func<IImage, int, MCvAvgComp[]> detector =
                delegate(IImage img, int channel)
                {
                   IntPtr objects = CvInvoke.cvHaarDetectObjects(
                       img.Ptr,
                       haarObj.Ptr,
                       stor.Ptr,
                       scaleFactor,
                       minNeighbors,
                       flag,
                       minSize);

                   if (objects == IntPtr.Zero)
                      return new MCvAvgComp[0];

                   Seq<MCvAvgComp> rects = new Seq<MCvAvgComp>(objects, stor);
                   return rects.ToArray();
                };

            MCvAvgComp[][] res = ForEachDuplicateChannel(detector);
            return res;
         }
      }
      #endregion

      #region Hough line and circles
      ///<summary> 
      ///Apply Hough transform to find line segments. 
      ///The current image must be a binary image (eg. the edges as a result of the Canny edge detector) 
      ///</summary> 
      ///<param name="rhoResolution">Distance resolution in pixel-related units.</param>
      ///<param name="thetaResolution">Angle resolution measured in radians</param>
      ///<param name="threshold">A line is returned by the function if the corresponding accumulator value is greater than threshold</param>
      ///<param name="minLineWidth">Minimum width of a line</param>
      ///<param name="gapBetweenLines">Minimum gap between lines</param>
      public LineSegment2D[][] HoughLinesBinary(double rhoResolution, double thetaResolution, int threshold, double minLineWidth, double gapBetweenLines)
      {
         using (MemStorage stor = new MemStorage())
         {
            Emgu.Util.Toolbox.Func<IImage, int, LineSegment2D[]> detector =
                delegate(IImage img, int channel)
                {
                   IntPtr lines = CvInvoke.cvHoughLines2(img.Ptr, stor.Ptr, CvEnum.HOUGH_TYPE.CV_HOUGH_PROBABILISTIC, rhoResolution, thetaResolution, threshold, minLineWidth, gapBetweenLines);
                   Seq<LineSegment2D> segments = new Seq<LineSegment2D>(lines, stor);
                   return segments.ToArray() ;
                };
            return ForEachDuplicateChannel(detector);
         }
      }

      ///<summary> 
      ///First apply Canny Edge Detector on the current image, 
      ///then apply Hough transform to find line segments 
      ///</summary>
      public LineSegment2D[][] HoughLines(TColor cannyThreshold, TColor cannyThresholdLinking, double rhoResolution, double thetaResolution, int threshold, double minLineWidth, double gapBetweenLines)
      {
         using (Image<TColor, TDepth> canny = Canny(cannyThreshold, cannyThresholdLinking))
         {
            return canny.HoughLinesBinary(rhoResolution, thetaResolution, threshold, minLineWidth, gapBetweenLines);
         }
      }

      ///<summary> 
      ///First apply Canny Edge Detector on the current image, 
      ///then apply Hough transform to find circles 
      ///</summary>
      ///<param name="cannyThreshold">The higher threshold of the two passed to Canny edge detector (the lower one will be twice smaller).</param>
      ///<param name="accumulatorThreshold">Accumulator threshold at the center detection stage. The smaller it is, the more false circles may be detected. Circles, corresponding to the larger accumulator values, will be returned first</param>
      ///<param name="dp">Resolution of the accumulator used to detect centers of the circles. For example, if it is 1, the accumulator will have the same resolution as the input image, if it is 2 - accumulator will have twice smaller width and height, etc</param>
      ///<param name="minRadius">Minimal radius of the circles to search for</param>
      ///<param name="maxRadius">Maximal radius of the circles to search for</param>
      ///<param name="minDist">Minimum distance between centers of the detected circles. If the parameter is too small, multiple neighbor circles may be falsely detected in addition to a true one. If it is too large, some circles may be missed</param>
      public CircleF[][] HoughCircles(TColor cannyThreshold, TColor accumulatorThreshold, double dp, double minDist, int minRadius, int maxRadius)
      {
         using (MemStorage stor = new MemStorage())
         {
            double[] cannyThresh = cannyThreshold.MCvScalar.ToArray();
            double[] accumulatorThresh = accumulatorThreshold.MCvScalar.ToArray();
            Emgu.Util.Toolbox.Func<IImage, int, CircleF[]> detector =
                delegate(IImage img, int channel)
                {
                   IntPtr circlesSeqPtr = CvInvoke.cvHoughCircles(
                       img.Ptr,
                       stor.Ptr,
                       CvEnum.HOUGH_TYPE.CV_HOUGH_GRADIENT,
                       dp,
                       minDist,
                       cannyThresh[channel],
                       accumulatorThresh[channel],
                       minRadius,
                       maxRadius);

                   Seq<CircleF> cirSeq = new Seq<CircleF>(circlesSeqPtr, stor);
                   return cirSeq.ToArray();
                };
            CircleF[][] res = ForEachDuplicateChannel(detector);

            return res;
         }
      }
      #endregion

      #region Contour detection
      /// <summary>
      /// Find a list of contours using simple approximation method.
      /// </summary>
      /// <returns>
      /// Contour if there is any;
      /// null if no contour is found
      /// </returns>
      public Contour<System.Drawing.Point> FindContours()
      {
         return FindContours(CvEnum.CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_SIMPLE, CvEnum.RETR_TYPE.CV_RETR_LIST);
      }

      /// <summary>
      /// Find contours 
      /// </summary>
      /// <param name="method">The type of approximation method</param>
      /// <param name="type">The retrival type</param>
      /// <returns>
      /// Contour if there is any;
      /// null if no contour is found
      /// </returns>
      public Contour<System.Drawing.Point> FindContours(CvEnum.CHAIN_APPROX_METHOD method, CvEnum.RETR_TYPE type)
      {
         return FindContours(method, type, new MemStorage());
      }

      /// <summary>
      /// Find contours using the specific memory storage
      /// </summary>
      /// <param name="method">The type of approximation method</param>
      /// <param name="type">The retrival type</param>
      /// <param name="stor">The storage used by the sequences</param>
      /// <returns>
      /// Contour if there is any;
      /// null if no contour is found
      /// </returns>
      public Contour<System.Drawing.Point> FindContours(CvEnum.CHAIN_APPROX_METHOD method, CvEnum.RETR_TYPE type, MemStorage stor)
      {
         if (method == Emgu.CV.CvEnum.CHAIN_APPROX_METHOD.CV_CHAIN_CODE)
         {
            //TODO: wrap CvChain and add code here
            throw new NotImplementedException("Not implmented");
         }

         IntPtr seq = IntPtr.Zero;

         using (Image<TColor, TDepth> imagecopy = Copy()) //since cvFindContours modifies the content of the source, we need to make a clone
         {
            CvInvoke.cvFindContours(
                imagecopy.Ptr,
                stor.Ptr,
                ref seq,
                StructSize.MCvContour,
                type,
                method,
                new System.Drawing.Point(0, 0));

            return (seq == IntPtr.Zero) ? null : new Contour<System.Drawing.Point>(seq, stor);
         }
      }
      #endregion
      #endregion

      #region Indexer
      /// <summary>
      /// Get or Set the specific channel of the current image. 
      /// For Get operation, a copy of the specific channel is returned.
      /// For Set operation, the specific channel is copied to this image.
      /// </summary>
      /// <param name="channel">The channel to get from the current image, zero based index</param>
      /// <returns>The specific channel of the current image</returns>
      public Image<Gray, TDepth> this[int channel]
      {
         get
         {
            Image<Gray, TDepth> imageChannel = new Image<Gray, TDepth>(Width, Height);
            int coi = CvInvoke.cvGetImageCOI(Ptr);
            CvInvoke.cvSetImageCOI(Ptr, channel + 1);
            CvInvoke.cvCopy(Ptr, imageChannel, IntPtr.Zero);
            CvInvoke.cvSetImageCOI(Ptr, coi);
            return imageChannel;
         }
         set
         {
            IntPtr[] channels = new IntPtr[4];
            channels[channel] = value.Ptr;
            CvInvoke.cvCvtPlaneToPix(channels[0], channels[1], channels[2], channels[3], Ptr);
         }
      }

      /// <summary>
      /// Get or Set the color in the <paramref name="row"/>th row (y direction) and <paramref name="column"/>th column (x direction)
      /// </summary>
      /// <param name="row">The zero-based row (y direction) of the pixel </param>
      /// <param name="col">The zero-based column (x direction) of the pixel</param>
      /// <returns>The color in the specific <paramref name="row"/> and <paramref name="column"/></returns>
      public TColor this[int row, int col]
      {
         get
         {
            TColor res = new TColor();
            res.MCvScalar = CvInvoke.cvGet2D(Ptr, row, col);
            return res;
         }
         set
         {
            CvInvoke.cvSet2D(Ptr, row, col, value.MCvScalar);
         }
      }

      /// <summary>
      /// Get or Set the color in the <paramref name="location"/>
      /// </summary>
      /// <param name="location">the location of the pixel </param>
      /// <returns>the color in the <paramref name="location"/></returns>
      public TColor this[Point location]
      {
         get
         {
            return this[location.Y, location.X];
         }
         set
         {
            this[location.Y, location.X] = value;
         }
      }
      #endregion

      #region utilities
      /// <summary>
      /// Return parameters based on ROI
      /// </summary>
      /// <param name="ptr">The Pointer to the IplImage</param>
      /// <param name="start">The address of the pointer that point to the start of the Bytes taken into consideration ROI</param>
      /// <param name="elementCount">ROI.Width * ColorType.Dimension</param>
      /// <param name="byteWidth">The number of bytes in a row taken into consideration ROI</param>
      /// <param name="rows">The number of rows taken into consideration ROI</param>
      /// <param name="widthStep">The width step required to jump to the next row</param>
      protected static void RoiParam(IntPtr ptr, out Int64 start, out int rows, out int elementCount, out int byteWidth, out int widthStep)
      {
         MIplImage ipl = (MIplImage)Marshal.PtrToStructure(ptr, typeof(MIplImage));
         start = ipl.imageData.ToInt64();
         widthStep = ipl.widthStep;

         if (ipl.roi != IntPtr.Zero)
         {
            System.Drawing.Rectangle rec = CvInvoke.cvGetImageROI(ptr);
            elementCount = rec.Width * ipl.nChannels;
            byteWidth = ((int)ipl.depth >> 3) * elementCount;

            start += rec.Y * widthStep
                    + ((int)ipl.depth >> 3) * rec.X;
            rows = rec.Height;
         }
         else
         {
            byteWidth = widthStep;
            elementCount = ipl.width * ipl.nChannels;
            rows = ipl.height;
         }
      }

      /// <summary>
      /// Apply convertor and compute result for each channel of the image.
      /// </summary>
      /// <remarks>
      /// For single channel image, apply converter directly.
      /// For multiple channel image, set the COI for the specific channel before appling the convertor
      /// </remarks>
      /// <typeparam name="TResult">The return type</typeparam>
      /// <param name="conv">The converter such that accept the IntPtr of a single channel IplImage, and image channel index which returning result of type R</param>
      /// <returns>An array which contains result for each channel</returns>
      private TResult[] ForEachChannel<TResult>(Emgu.Util.Toolbox.Func<IntPtr, int, TResult> conv)
      {
         TResult[] res = new TResult[NumberOfChannels];
         if (NumberOfChannels == 1)
            res[0] = conv(Ptr, 0);
         else
         {
            for (int i = 0; i < NumberOfChannels; i++)
            {
               CvInvoke.cvSetImageCOI(Ptr, i + 1);
               res[i] = conv(Ptr, i);
            }
            CvInvoke.cvSetImageCOI(Ptr, 0);
         }
         return res;
      }

      /// <summary>
      /// Apply convertor and compute result for each channel of the image, for single channel image, apply converter directly, for multiple channel image, make a copy of each channel to a temperary image and apply the convertor
      /// </summary>
      /// <param name="action">The converter such that accept the IntPtr of a single channel IplImage, and image channel index which returning result of type R</param>
      /// <returns>An array which contains result for each channel</returns>
      private void ForEachDuplicateChannel(Emgu.Util.Toolbox.Action<IImage, int> action)
      {
         if (NumberOfChannels == 1)
            action(this, 0);
         else
         {
            using (Image<Gray, TDepth> tmp = new Image<Gray, TDepth>(Size))
               for (int i = 0; i < NumberOfChannels; i++)
               {
                  CvInvoke.cvSetImageCOI(Ptr, i + 1);
                  CvInvoke.cvCopy(Ptr, tmp.Ptr, IntPtr.Zero);
                  action(tmp, i);
               }
            CvInvoke.cvSetImageCOI(Ptr, 0);
         }
      }

      /// <summary>
      /// Apply convertor and compute result for each channel of the image, for single channel image, apply converter directly, for multiple channel image, make a copy of each channel to a temperary image and apply the convertor
      /// </summary>
      /// <typeparam name="TReturn">The return type</typeparam>
      /// <param name="conv">The converter such that accept the IntPtr of a single channel IplImage, and image channel index which returning result of type R</param>
      /// <returns>An array which contains result for each channel</returns>
      private TReturn[] ForEachDuplicateChannel<TReturn>(Emgu.Util.Toolbox.Func<IImage, int, TReturn> conv)
      {
         TReturn[] res = new TReturn[NumberOfChannels];
         if (NumberOfChannels == 1)
            res[0] = conv(this, 0);
         else
         {
            using (Image<Gray, TDepth> tmp = new Image<Gray, TDepth>(Size))
               for (int i = 0; i < NumberOfChannels; i++)
               {
                  CvInvoke.cvSetImageCOI(Ptr, i + 1);
                  CvInvoke.cvCopy(Ptr, tmp.Ptr, IntPtr.Zero);
                  res[i] = conv(tmp, i);
               }

            CvInvoke.cvSetImageCOI(Ptr, 0);
         }
         return res;
      }

      /// <summary>
      /// If the image has only one channel, apply the action directly on the IntPtr of this image and <paramref name="image2"/>,
      /// otherwise, make copy each channel of this image to a temperary one, apply action on it and another temperory image and copy the resulting image back to image2
      /// </summary>
      /// <typeparam name="TOtherDepth">The type of the depth of the <paramref name="dest"/> image</typeparam>
      /// <param name="act">The function which acepts the src IntPtr, dest IntPtr and index of the channel as input</param>
      /// <param name="dest">The destination image</param>
      private void ForEachDuplicateChannel<TOtherDepth>(Emgu.Util.Toolbox.Action<IntPtr, IntPtr, int> act, Image<TColor, TOtherDepth> dest)
      {
         if (NumberOfChannels == 1)
            act(Ptr, dest.Ptr, 0);
         else
         {
            using (Image<Gray, TDepth> tmp1 = new Image<Gray, TDepth>(Size))
            using (Image<Gray, TOtherDepth> tmp2 = new Image<Gray, TOtherDepth>(dest.Size))
            {
               for (int i = 0; i < NumberOfChannels; i++)
               {
                  CvInvoke.cvSetImageCOI(Ptr, i + 1);
                  CvInvoke.cvSetImageCOI(dest.Ptr, i + 1);
                  CvInvoke.cvCopy(Ptr, tmp1.Ptr, IntPtr.Zero);
                  act(tmp1.Ptr, tmp2.Ptr, i);
                  CvInvoke.cvCopy(tmp2.Ptr, dest.Ptr, IntPtr.Zero);
               }
            }
            CvInvoke.cvSetImageCOI(Ptr, 0);
            CvInvoke.cvSetImageCOI(dest.Ptr, 0);
         }
      }
      #endregion

      #region Gradient, Edges and Features
      /// <summary>
      /// Calculates the image derivative by convolving the image with the appropriate kernel
      /// The Sobel operators combine Gaussian smoothing and differentiation so the result is more or less robust to the noise. Most often, the function is called with (xorder=1, yorder=0, aperture_size=3) or (xorder=0, yorder=1, aperture_size=3) to calculate first x- or y- image derivative.
      /// </summary>
      /// <param name="xorder">Order of the derivative x</param>
      /// <param name="yorder">Order of the derivative y</param>
      /// <param name="apertureSize">Size of the extended Sobel kernel, must be 1, 3, 5 or 7. In all cases except 1, aperture_size xaperture_size separable kernel will be used to calculate the derivative.</param>
      /// <returns>The result of the sobel edge detector</returns>
      [ExposableMethod(Exposable = true, Category = "Gradients, Edges")]
      public Image<TColor, Single> Sobel(int xorder, int yorder, int apertureSize)
      {
         Image<TColor, Single> res = new Image<TColor, float>(Width, Height);
         CvInvoke.cvSobel(Ptr, res.Ptr, xorder, yorder, apertureSize);
         return res;
      }

      /// <summary>
      /// Calculates Laplacian of the source image by summing second x- and y- derivatives calculated using Sobel operator.
      /// Specifying aperture_size=1 gives the fastest variant that is equal to convolving the image with the following kernel:
      ///
      /// |0  1  0|
      /// |1 -4  1|
      /// |0  1  0|
      /// </summary>
      /// <param name="apertureSize">Aperture size </param>
      /// <returns>The Laplacian of the image</returns>
      [ExposableMethod(Exposable = true, Category = "Gradients, Edges")]
      public Image<TColor, Single> Laplace(int apertureSize)
      {
         Image<TColor, Single> res = new Image<TColor, float>(Width, Height);
         CvInvoke.cvLaplace(Ptr, res.Ptr, apertureSize);
         return res;
      }

      ///<summary> Find the edges on this image and marked them in the returned image.</summary>
      ///<param name="thresh"> The threshhold to find initial segments of strong edges</param>
      ///<param name="threshLinking"> The threshold used for edge Linking</param>
      ///<returns> The edges found by the Canny edge detector</returns>
      [ExposableMethod(Exposable = true, Category = "Gradients, Edges")]
      public Image<TColor, TDepth> Canny(TColor thresh, TColor threshLinking)
      {
         Image<TColor, TDepth> res = new Image<TColor, TDepth>(Size);
         double[] t1 = thresh.MCvScalar.ToArray();
         double[] t2 = threshLinking.MCvScalar.ToArray();
         Emgu.Util.Toolbox.Action<IntPtr, IntPtr, int> act =
             delegate(IntPtr src, IntPtr dest, int channel)
             {
                CvInvoke.cvCanny(src, dest, t1[channel], t2[channel], 3);
             };
         ForEachDuplicateChannel<TDepth>(act, res);

         return res;
      }

      #region SURF
      /// <summary>
      /// Finds robust features in the image (basic descriptor is returned in this case). For each feature it returns its location, size, orientation and optionally the descriptor, basic or extended. The function can be used for object tracking and localization, image stitching etc
      /// </summary>
      /// <param name="param">The SURF parameters</param>
      public SURFFeature[] ExtractSURF(ref MCvSURFParams param)
      {
         return ExtractSURF(null, ref param);
      }

      /// <summary>
      /// Finds robust features in the image (basic descriptor is returned in this case). For each feature it returns its location, size, orientation and optionally the descriptor, basic or extended. The function can be used for object tracking and localization, image stitching etc
      /// </summary>
      /// <param name="mask">The optional input 8-bit mask, can be null if not needed. The features are only found in the areas that contain more than 50% of non-zero mask pixels</param>
      /// <param name="param">The SURF parameters</param>
      public SURFFeature[] ExtractSURF(Image<Gray, Byte> mask, ref MCvSURFParams param)
      {
         using (MemStorage stor = new MemStorage())
         {
            IntPtr descriptorPtr;
            Seq<MCvSURFPoint> keypoints;
            ExtractSURF(mask, ref param, stor, out keypoints, out descriptorPtr);
            MCvSURFPoint[] surfPoints = keypoints.ToArray();

            SURFFeature[] res = new SURFFeature[surfPoints.Length];

            int elementsInDescriptor = param.extended == 0 ? 64 : 128;
            int bytesToCopy = elementsInDescriptor * sizeof(float);

            for (int i = 0; i < res.Length; i++)
            {
               float[,] descriptor = new float[elementsInDescriptor, 1];
               GCHandle handle = GCHandle.Alloc(descriptor, GCHandleType.Pinned);
               Emgu.Util.Toolbox.memcpy(handle.AddrOfPinnedObject(), CvInvoke.cvGetSeqElem(descriptorPtr, i), bytesToCopy);
               handle.Free();
               res[i] = new SURFFeature(ref surfPoints[i], new Matrix<float>(descriptor));
            }

            return res;
         }
      }

      private void ExtractSURF(Image<Gray, Byte> mask, ref MCvSURFParams param, MemStorage stor, out Seq<MCvSURFPoint> keypoints, out IntPtr descriptorPtr)
      {
         IntPtr keypointsPtr;
         CvInvoke.cvExtractSURF(Ptr, mask == null ? IntPtr.Zero : mask.Ptr, out keypointsPtr, out descriptorPtr, stor.Ptr, param);
         keypoints = new Seq<MCvSURFPoint>(keypointsPtr, stor);
      }
      #endregion

      /// <summary>
      /// Finds corners with big eigenvalues in the image. 
      /// </summary>
      /// <remarks>The function first calculates the minimal eigenvalue for every source image pixel using cvCornerMinEigenVal function and stores them in eig_image. Then it performs non-maxima suppression (only local maxima in 3x3 neighborhood remain). The next step is rejecting the corners with the minimal eigenvalue less than quality_level?max(eig_image(x,y)). Finally, the function ensures that all the corners found are distanced enough one from another by considering the corners (the most strongest corners are considered first) and checking that the distance between the newly considered feature and the features considered earlier is larger than min_distance. So, the function removes the features than are too close to the stronger features</remarks>
      /// <param name="maxFeaturesPerChannel">The maximum features to be detected per channel</param>
      /// <param name="qualityLevel">Multiplier for the maxmin eigenvalue; specifies minimal accepted quality of image corners</param>
      /// <param name="minDistance">Limit, specifying minimum possible distance between returned corners; Euclidian distance is used. </param>
      /// <param name="blockSize">Size of the averaging block, passed to underlying cvCornerMinEigenVal or cvCornerHarris used by the function</param>
      /// <returns>The good features for each channel</returns>
      public PointF[][] GoodFeaturesToTrack(int maxFeaturesPerChannel, double qualityLevel, double minDistance, int blockSize)
      {
         return GoodFeaturesToTrack(maxFeaturesPerChannel, qualityLevel, minDistance, blockSize, false, 0);
      }

      /// <summary>
      /// Finds corners with big eigenvalues in the image. 
      /// </summary>
      /// <remarks>The function first calculates the minimal eigenvalue for every source image pixel using cvCornerMinEigenVal function and stores them in eig_image. Then it performs non-maxima suppression (only local maxima in 3x3 neighborhood remain). The next step is rejecting the corners with the minimal eigenvalue less than quality_level?max(eig_image(x,y)). Finally, the function ensures that all the corners found are distanced enough one from another by considering the corners (the most strongest corners are considered first) and checking that the distance between the newly considered feature and the features considered earlier is larger than min_distance. So, the function removes the features than are too close to the stronger features</remarks>
      /// <param name="maxFeaturesPerChannel">The maximum features to be detected per channel</param>
      /// <param name="qualityLevel">Multiplier for the maxmin eigenvalue; specifies minimal accepted quality of image corners</param>
      /// <param name="minDistance">Limit, specifying minimum possible distance between returned corners; Euclidian distance is used. </param>
      /// <param name="blockSize">Size of the averaging block, passed to underlying cvCornerMinEigenVal or cvCornerHarris used by the function</param>
      /// <param name="k">Free parameter of Harris detector. If provided, Harris operator (cvCornerHarris) is used instead of default cvCornerMinEigenVal. </param>
      /// <returns>The good features for each channel</returns>
      public PointF[][] GoodFeaturesToTrack(int maxFeaturesPerChannel, double qualityLevel, double minDistance, int blockSize, double k)
      {
         return GoodFeaturesToTrack(maxFeaturesPerChannel, qualityLevel, minDistance, blockSize, true, k);
      }

      /// <summary>
      /// Finds corners with big eigenvalues in the image. 
      /// </summary>
      /// <remarks>The function first calculates the minimal eigenvalue for every source image pixel using cvCornerMinEigenVal function and stores them in eig_image. Then it performs non-maxima suppression (only local maxima in 3x3 neighborhood remain). The next step is rejecting the corners with the minimal eigenvalue less than quality_level?max(eig_image(x,y)). Finally, the function ensures that all the corners found are distanced enough one from another by considering the corners (the most strongest corners are considered first) and checking that the distance between the newly considered feature and the features considered earlier is larger than min_distance. So, the function removes the features than are too close to the stronger features</remarks>
      /// <param name="maxFeaturesPerChannel">The maximum features to be detected per channel</param>
      /// <param name="qualityLevel">Multiplier for the maxmin eigenvalue; specifies minimal accepted quality of image corners</param>
      /// <param name="minDistance">Limit, specifying minimum possible distance between returned corners; Euclidian distance is used. </param>
      /// <param name="blockSize">Size of the averaging block, passed to underlying cvCornerMinEigenVal or cvCornerHarris used by the function</param>
      /// <param name="useHarris">If nonzero, Harris operator (cvCornerHarris) is used instead of default cvCornerMinEigenVal</param>
      /// <param name="k">Free parameter of Harris detector; used only if use_harris = true </param>
      /// <returns>The good features for each channel</returns>
      public PointF[][] GoodFeaturesToTrack(int maxFeaturesPerChannel, double qualityLevel, double minDistance, int blockSize, bool useHarris, double k)
      {
         PointF[][] res = new PointF[_numberOfChannels][];

         using (Image<Gray, Single> eigImage = new Image<Gray, float>(Width, Height))
         using (Image<Gray, Single> tmpImage = new Image<Gray, float>(Width, Height))
         {
            Emgu.Util.Toolbox.Func<IImage, int, PointF[]> detector =
                delegate(IImage img, int channel)
                {
                   int cornercount = maxFeaturesPerChannel;
                   PointF[] pts = new PointF[maxFeaturesPerChannel];

                   CvInvoke.cvGoodFeaturesToTrack(
                       img.Ptr,
                       eigImage.Ptr,
                       tmpImage.Ptr,
                       pts,
                       ref cornercount,
                       qualityLevel,
                       minDistance,
                       IntPtr.Zero,
                       blockSize,
                       useHarris ? 1 : 0,
                       k);
                   Array.Resize(ref pts, cornercount);
                   return pts;
                };

            res = ForEachDuplicateChannel(detector);
         }
         return res;
      }

      /// <summary>
      /// Iterates to find the sub-pixel accurate location of corners, or radial saddle points
      /// </summary>
      /// <param name="corners">Coordinates of the input corners, the values will be modified by this function call</param>
      /// <param name="win">Half sizes of the search window. For example, if win=(5,5) then 5*2+1 x 5*2+1 = 11 x 11 search window is used</param>
      /// <param name="zeroZone">Half size of the dead region in the middle of the search zone over which the summation in formulae below is not done. It is used sometimes to avoid possible singularities of the autocorrelation matrix. The value of (-1,-1) indicates that there is no such size</param>
      /// <param name="criteria">Criteria for termination of the iterative process of corner refinement. That is, the process of corner position refinement stops either after certain number of iteration or when a required accuracy is achieved. The criteria may specify either of or both the maximum number of iteration and the required accuracy</param>
      /// <returns>Refined corner coordinates</returns>
      public void FindCornerSubPix(
         PointF[][] corners,
         System.Drawing.Size win,
         System.Drawing.Size zeroZone,
         MCvTermCriteria criteria)
      {
         Emgu.Util.Toolbox.Action<IImage, int> detector =
             delegate(IImage img, int channel)
             {
                PointF[] ptsForCurrentChannel = corners[channel];
                CvInvoke.cvFindCornerSubPix(
                   img.Ptr, 
                   ptsForCurrentChannel, 
                   ptsForCurrentChannel.Length, 
                   win, 
                   zeroZone, 
                   criteria);
             };
         ForEachDuplicateChannel(detector);
      }

      #endregion

      #region Matching
      /// <summary>
      /// The function slids through image, compares overlapped patches of size wxh with templ using the specified method and return the comparison results 
      /// </summary>
      /// <param name="template">Searched template; must be not greater than the source image and the same data type as the image</param>
      /// <param name="method">Specifies the way the template must be compared with image regions </param>
      /// <returns>The comparison result: width = this.Width - template.Width + 1; height = this.Height - template.Height + 1 </returns>
      public Image<Gray, Single> MatchTemplate(Image<TColor, TDepth> template, CvEnum.TM_TYPE method)
      {
         Image<Gray, Single> res = new Image<Gray, Single>(Width - template.Width + 1, Height - template.Height + 1);
         CvInvoke.cvMatchTemplate(Ptr, template.Ptr, res.Ptr, method);
         return res;
      }
      #endregion

      #region Object Tracking
      /// <summary>
      /// Updates snake in order to minimize its total energy that is a sum of internal energy that depends on contour shape (the smoother contour is, the smaller internal energy is) and external energy that depends on the energy field and reaches minimum at the local energy extremums that correspond to the image edges in case of image gradient.
      /// </summary>
      /// <param name="contour">Some existing contour</param>
      /// <param name="alpha">Weight[s] of continuity energy, single float or array of length floats, one per each contour point</param>
      /// <param name="beta">Weight[s] of curvature energy, similar to alpha.</param>
      /// <param name="gamma">Weight[s] of image energy, similar to alpha.</param>
      /// <param name="windowSize">Size of neighborhood of every point used to search the minimum, both win.width and win.height must be odd</param>
      /// <param name="tc">Termination criteria. The parameter criteria.epsilon is used to define the minimal number of points that must be moved during any iteration to keep the iteration process running. If at some iteration the number of moved points is less than criteria.epsilon or the function performed criteria.max_iter iterations, the function terminates. </param>
      /// <param name="storage">The memory storage used by the resulting sequence</param>
      /// <returns>The snake[d] contour</returns>
      public Contour<System.Drawing.Point> Snake(Seq<System.Drawing.Point> contour, float alpha, float beta, float gamma, System.Drawing.Size windowSize, MCvTermCriteria tc, MemStorage storage)
      {
         int count = contour.Total;

         Point[] points = new Point[count];
         GCHandle handle = GCHandle.Alloc(points, GCHandleType.Pinned);
         CvInvoke.cvCvtSeqToArray(contour.Ptr, handle.AddrOfPinnedObject(), MCvSlice.WholeSeq);
         CvInvoke.cvSnakeImage(
             Ptr,
             handle.AddrOfPinnedObject(),
             count,
             new float[1] { alpha },
             new float[1] { beta },
             new float[1] { gamma },
             1,
             windowSize,
             tc,
             true);

         Contour<System.Drawing.Point> rSeq = new Contour<System.Drawing.Point>(storage);

         CvInvoke.cvSeqPushMulti(rSeq.Ptr, handle.AddrOfPinnedObject(), count, Emgu.CV.CvEnum.BACK_OR_FRONT.FRONT);
         handle.Free();

         return rSeq;
      }

      /// <summary>
      /// Updates snake in order to minimize its total energy that is a sum of internal energy that depends on contour shape (the smoother contour is, the smaller internal energy is) and external energy that depends on the energy field and reaches minimum at the local energy extremums that correspond to the image edges in case of image gradient.
      /// </summary>
      /// <param name="contour">Some existing contour. It's value will be update by this function</param>
      /// <param name="alpha">Weight[s] of continuity energy, single float or array of length floats, one per each contour point</param>
      /// <param name="beta">Weight[s] of curvature energy, similar to alpha.</param>
      /// <param name="gamma">Weight[s] of image energy, similar to alpha.</param>
      /// <param name="windowSize">Size of neighborhood of every point used to search the minimum, both win.width and win.height must be odd</param>
      /// <param name="tc">Termination criteria. The parameter criteria.epsilon is used to define the minimal number of points that must be moved during any iteration to keep the iteration process running. If at some iteration the number of moved points is less than criteria.epsilon or the function performed criteria.max_iter iterations, the function terminates. </param>
      /// <param name="calculateGradiant">If true, the function calculates gradient magnitude for every image pixel and considers it as the energy field, otherwise the input image itself is considered</param>
      public void Snake(System.Drawing.Point[] contour, float alpha, float beta, float gamma, System.Drawing.Size windowSize, MCvTermCriteria tc, bool calculateGradiant)
      {
         CvInvoke.cvSnakeImage(
             Ptr,
             contour,
             contour.Length,
             new float[1] { alpha },
             new float[1] { beta },
             new float[1] { gamma },
             1,
             windowSize,
             tc,
             calculateGradiant ? 1: 0);
      }
      #endregion

      #region Logic
      #region And Methods
      ///<summary> Perform an elementwise AND operation with another image and return the result</summary>
      ///<param name="img2">The second image for the AND operation</param>
      ///<returns> The result of the AND operation</returns>
      public Image<TColor, TDepth> And(Image<TColor, TDepth> img2)
      {
         Image<TColor, TDepth> res = new Image<TColor, TDepth>(Size);
         CvInvoke.cvAnd(Ptr, img2.Ptr, res.Ptr, IntPtr.Zero);
         return res;
      }

      ///<summary> 
      ///Perform an elementwise AND operation with another image, using a mask, and return the result
      ///</summary>
      ///<param name="img2">The second image for the AND operation</param>
      ///<param name="mask">The mask for the AND operation</param>
      ///<returns> The result of the AND operation</returns>
      public Image<TColor, TDepth> And(Image<TColor, TDepth> img2, Image<Gray, Byte> mask)
      {
         Image<TColor, TDepth> res = new Image<TColor, TDepth>(Size);
         CvInvoke.cvAnd(Ptr, img2.Ptr, res.Ptr, mask.Ptr);
         return res;
      }

      ///<summary> Perform an binary AND operation with some color</summary>
      ///<param name="val">The color for the AND operation</param>
      ///<returns> The result of the AND operation</returns>
      public Image<TColor, TDepth> And(TColor val)
      {
         Image<TColor, TDepth> res = new Image<TColor, TDepth>(Size);
         CvInvoke.cvAndS(Ptr, val.MCvScalar, res.Ptr, IntPtr.Zero);
         return res;
      }

      ///<summary> Perform an binary AND operation with some color using a mask</summary>
      ///<param name="val">The color for the AND operation</param>
      ///<param name="mask">The mask for the AND operation</param>
      ///<returns> The result of the AND operation</returns>
      public Image<TColor, TDepth> And(TColor val, Image<Gray, Byte> mask)
      {
         Image<TColor, TDepth> res = new Image<TColor, TDepth>(Size);
         CvInvoke.cvAndS(Ptr, val.MCvScalar, res.Ptr, mask.Ptr);
         return res;
      }
      #endregion

      #region Or Methods
      ///<summary> Perform an elementwise OR operation with another image and return the result</summary>
      ///<param name="img2">The second image for the OR operation</param>
      ///<returns> The result of the OR operation</returns>
      public Image<TColor, TDepth> Or(Image<TColor, TDepth> img2)
      {
         Image<TColor, TDepth> res = CopyBlank();
         CvInvoke.cvOr(Ptr, img2.Ptr, res.Ptr, IntPtr.Zero);
         return res;
      }
      ///<summary> Perform an elementwise OR operation with another image, using a mask, and return the result</summary>
      ///<param name="img2">The second image for the OR operation</param>
      ///<param name="mask">The mask for the OR operation</param>
      ///<returns> The result of the OR operation</returns>
      public Image<TColor, TDepth> Or(Image<TColor, TDepth> img2, Image<Gray, Byte> mask)
      {
         Image<TColor, TDepth> res = CopyBlank();
         CvInvoke.cvOr(Ptr, img2.Ptr, res.Ptr, mask.Ptr);
         return res;
      }

      ///<summary> Perform an elementwise OR operation with some color</summary>
      ///<param name="val">The value for the OR operation</param>
      ///<returns> The result of the OR operation</returns>
      [ExposableMethod(Exposable = true, Category = "Logic Operation")]
      public Image<TColor, TDepth> Or(TColor val)
      {
         Image<TColor, TDepth> res = CopyBlank();
         CvInvoke.cvOrS(Ptr, val.MCvScalar, res.Ptr, IntPtr.Zero);
         return res;
      }
      ///<summary> Perform an elementwise OR operation with some color using a mask</summary>
      ///<param name="val">The color for the OR operation</param>
      ///<param name="mask">The mask for the OR operation</param>
      ///<returns> The result of the OR operation</returns>
      public Image<TColor, TDepth> Or(TColor val, Image<Gray, Byte> mask)
      {
         Image<TColor, TDepth> res = CopyBlank();
         CvInvoke.cvOrS(Ptr, val.MCvScalar, res.Ptr, mask.Ptr);
         return res;
      }
      #endregion

      #region Xor Methods
      ///<summary> Perform an elementwise XOR operation with another image and return the result</summary>
      ///<param name="img2">The second image for the XOR operation</param>
      ///<returns> The result of the XOR operation</returns>
      public Image<TColor, TDepth> Xor(Image<TColor, TDepth> img2)
      {
         Image<TColor, TDepth> res = CopyBlank();
         CvInvoke.cvXor(Ptr, img2.Ptr, res.Ptr, IntPtr.Zero);
         return res;
      }

      /// <summary>
      /// Perform an elementwise XOR operation with another image, using a mask, and return the result
      /// </summary>
      /// <param name="img2">The second image for the XOR operation</param>
      /// <param name="mask">The mask for the XOR operation</param>
      /// <returns>The result of the XOR operation</returns>
      public Image<TColor, TDepth> Xor(Image<TColor, TDepth> img2, Image<Gray, Byte> mask)
      {
         Image<TColor, TDepth> res = CopyBlank();
         CvInvoke.cvXor(Ptr, img2.Ptr, res.Ptr, mask.Ptr);
         return res;
      }

      /// <summary> 
      /// Perform an binary XOR operation with some color
      /// </summary>
      /// <param name="val">The value for the XOR operation</param>
      /// <returns> The result of the XOR operation</returns>
      [ExposableMethod(Exposable = true, Category = "Logic Operation")]
      public Image<TColor, TDepth> Xor(TColor val)
      {
         Image<TColor, TDepth> res = CopyBlank();
         CvInvoke.cvXorS(Ptr, val.MCvScalar, res.Ptr, IntPtr.Zero);
         return res;
      }

      /// <summary>
      /// Perform an binary XOR operation with some color using a mask
      /// </summary>
      /// <param name="val">The color for the XOR operation</param>
      /// <param name="mask">The mask for the XOR operation</param>
      /// <returns> The result of the XOR operation</returns>
      public Image<TColor, TDepth> Xor(TColor val, Image<Gray, Byte> mask)
      {
         Image<TColor, TDepth> res = CopyBlank();
         CvInvoke.cvXorS(Ptr, val.MCvScalar, res.Ptr, mask.Ptr);
         return res;
      }
      #endregion

      ///<summary> 
      ///Compute the complement image
      ///</summary>
      ///<returns> The complement image</returns>
      [ExposableMethod(Exposable = true, Category = "Logic Operation")]
      public Image<TColor, TDepth> Not()
      {
         Image<TColor, TDepth> res = CopyBlank();
         CvInvoke.cvNot(Ptr, res.Ptr);
         return res;
      }
      #endregion

      #region Comparison
      ///<summary> Find the elementwise maximum value </summary>
      ///<param name="img2">The second image for the Max operation</param>
      ///<returns> An image where each pixel is the maximum of <i>this</i> image and the parameter image</returns>
      public Image<TColor, TDepth> Max(Image<TColor, TDepth> img2)
      {
         Image<TColor, TDepth> res = CopyBlank();
         CvInvoke.cvMax(Ptr, img2.Ptr, res.Ptr);
         return res;
      }

      ///<summary> Find the elementwise maximum value </summary>
      ///<param name="value">The value to compare with</param>
      ///<returns> An image where each pixel is the maximum of <i>this</i> image and <paramref name="value"/></returns>
      [ExposableMethod(Exposable = true, Category = "Logic Operation")]
      public Image<TColor, TDepth> Max(double value)
      {
         Image<TColor, TDepth> res = CopyBlank();
         CvInvoke.cvMaxS(Ptr, value, res.Ptr);
         return res;
      }

      ///<summary> Find the elementwise minimum value </summary>
      ///<param name="img2">The second image for the Min operation</param>
      ///<returns> An image where each pixel is the minimum of <i>this</i> image and the parameter image</returns>
      public Image<TColor, TDepth> Min(Image<TColor, TDepth> img2)
      {
         Image<TColor, TDepth> res = CopyBlank();
         CvInvoke.cvMin(Ptr, img2.Ptr, res.Ptr);
         return res;
      }

      ///<summary> Find the elementwise minimum value </summary>
      ///<param name="value">The value to compare with</param>
      ///<returns> An image where each pixel is the minimum of <i>this</i> image and <paramref name="value"/></returns>
      [ExposableMethod(Exposable = true, Category = "Logic Operation")]
      public Image<TColor, TDepth> Min(double value)
      {
         Image<TColor, TDepth> res = CopyBlank();
         CvInvoke.cvMinS(Ptr, value, res.Ptr);
         return res;
      }

      ///<summary>Checks that image elements lie between two scalars</summary>
      ///<param name="lower"> The lower limit of color value</param>
      ///<param name="higher"> The upper limit of color value</param>
      ///<returns> res[i,j] = 255 if inrange, 0 otherwise</returns>
      public Image<TColor, Byte> InRange(TColor lower, TColor higher)
      {
         Image<TColor, Byte> res = new Image<TColor, Byte>(Width, Height);
         CvInvoke.cvInRangeS(Ptr, lower.MCvScalar, higher.MCvScalar, res.Ptr);
         return res;
      }

      /// <summary>
      /// This function compare the current image with <paramref name="img2"/> and returns the comparison mask
      /// </summary>
      /// <param name="img2">The other image to compare with</param>
      /// <param name="cmp_type">The comparison type</param>
      /// <returns>The result of the comparison as a mask</returns>
      public Image<TColor, Byte> Cmp(Image<TColor, TDepth> img2, CvEnum.CMP_TYPE cmp_type)
      {
         Size size = Size;
         Image<TColor, Byte> res = new Image<TColor, byte>(size);

         if (NumberOfChannels == 1)
         {
            CvInvoke.cvCmp(Ptr, img2.Ptr, res.Ptr, cmp_type);
         }
         else
         {
            using (Image<Gray, TDepth> src1 = new Image<Gray, TDepth>(size))
            using (Image<Gray, TDepth> src2 = new Image<Gray, TDepth>(size))
            using (Image<Gray, Byte> dest = new Image<Gray, Byte>(size))
               for (int i = 0; i < NumberOfChannels; i++)
               {
                  CvInvoke.cvSetImageCOI(Ptr, i + 1);
                  CvInvoke.cvSetImageCOI(img2.Ptr, i + 1);
                  CvInvoke.cvCopy(Ptr, src1.Ptr, IntPtr.Zero);
                  CvInvoke.cvCopy(img2.Ptr, src2.Ptr, IntPtr.Zero);

                  CvInvoke.cvCmp(src1.Ptr, src2.Ptr, dest.Ptr, cmp_type);

                  CvInvoke.cvSetImageCOI(res.Ptr, i + 1);
                  CvInvoke.cvCopy(dest.Ptr, res.Ptr, IntPtr.Zero);
               }
            CvInvoke.cvSetImageCOI(Ptr, 0);
            CvInvoke.cvSetImageCOI(img2.Ptr, 0);
            CvInvoke.cvSetImageCOI(res.Ptr, 0);
         }

         return res;
      }

      /// <summary>
      /// This function compare the current image with <paramref name="value"/> and returns the comparison mask
      /// </summary>
      /// <param name="value">The value to compare with</param>
      /// <param name="cmp_type">The comparison type</param>
      /// <returns>The result of the comparison as a mask</returns>
      [ExposableMethod(Exposable = true, Category = "Logic Operation")]
      public Image<TColor, Byte> Cmp(double value, CvEnum.CMP_TYPE cmp_type)
      {
         Size size = Size;
         Image<TColor, Byte> res = new Image<TColor, byte>(size);

         if (NumberOfChannels == 1)
         {
            CvInvoke.cvCmpS(Ptr, value, res.Ptr, cmp_type);
         }
         else
         {
            using (Image<Gray, TDepth> src1 = new Image<Gray, TDepth>(size))
            using (Image<Gray, TDepth> dest = new Image<Gray, TDepth>(size))
               for (int i = 0; i < NumberOfChannels; i++)
               {
                  CvInvoke.cvSetImageCOI(Ptr, i + 1);
                  CvInvoke.cvCopy(Ptr, src1.Ptr, IntPtr.Zero);

                  CvInvoke.cvCmpS(src1.Ptr, value, dest.Ptr, cmp_type);

                  CvInvoke.cvSetImageCOI(res.Ptr, i + 1);
                  CvInvoke.cvCopy(dest.Ptr, res.Ptr, IntPtr.Zero);
               }
            CvInvoke.cvSetImageCOI(Ptr, 0);
            CvInvoke.cvSetImageCOI(res.Ptr, 0);
         }

         return res;
      }

      /// <summary>
      /// Compare two images, returns true if the each of the pixels are equal, false otherwise
      /// </summary>
      /// <param name="img2">The other image to compare with</param>
      /// <returns>true if the each of the pixels for the two images are equal, false otherwise</returns>
      public bool Equals(Image<TColor, TDepth> img2)
      {
         //true if the references are equal
         if (System.Object.ReferenceEquals(this, img2)) return true;

         //false if size are not equal
         if (!EqualSize(img2)) return false;

         using (Image<TColor, Byte> neqMask = Cmp(img2, Emgu.CV.CvEnum.CMP_TYPE.CV_CMP_NE))
         {
            foreach (int c in neqMask.CountNonzero())
               if (c != 0) return false;
            return true;
         }
      }
      #endregion

      /*
      #region Discrete Transforms
      /// <summary>
      /// performs forward or inverse transform of 1D or 2D floating-point array
      /// </summary>
      /// <param name="type">Transformation flags</param>
      /// <param name="nonzeroRows">Number of nonzero rows to in the source array (in case of forward 2d transform), or a number of rows of interest in the destination array (in case of inverse 2d transform). If the value is negative, zero, or greater than the total number of rows, it is ignored. The parameter can be used to speed up 2d convolution/correlation when computing them via DFT</param>
      /// <returns>The result of DFT</returns>
      [ExposableMethod(Exposable = true, Category = "Discrete Transforms")]
      public Image<TColor, Single> DFT(CvEnum.CV_DXT type, int nonzeroRows)
      {
         Image<TColor, Single> res = new Image<TColor, float>(Width, Height);
         CvInvoke.cvDFT(Ptr, res.Ptr, type, nonzeroRows);
         return res;
      }

      /// <summary>
      /// performs forward or inverse transform of 2D floating-point image
      /// </summary>
      /// <param name="type">Transformation flags</param>
      /// <returns>The result of DFT</returns>
      public Image<TColor, Single> DFT(CvEnum.CV_DXT type)
      {
         return DFT(type, 0);
      }

      /// <summary>
      /// performs forward or inverse transform of 2D floating-point image
      /// </summary>
      /// <param name="type">Transformation flags</param>
      /// <returns>The result of DCT</returns>
      [ExposableMethod(Exposable = true, Category = "Discrete Transforms")]
      public Image<TColor, Single> DCT(CvEnum.CV_DCT_TYPE type)
      {
         Image<TColor, Single> res = new Image<TColor, float>(Width, Height);
         CvInvoke.cvDCT(Ptr, res.Ptr, type);
         return res;
      }
      #endregion
      */

      #region Arithmatic
      #region Substraction methods
      ///<summary> Elementwise subtract another image from the current image </summary>
      ///<param name="img2">The second image to be subtraced from the current image</param>
      ///<returns> The result of elementwise subtracting img2 from the current image</returns>
      public Image<TColor, TDepth> Sub(Image<TColor, TDepth> img2)
      {
         Image<TColor, TDepth> res = CopyBlank();
         CvInvoke.cvSub(Ptr, img2.Ptr, res.Ptr, IntPtr.Zero);
         return res;
      }

      ///<summary> Elementwise subtrace another image from the current image, using a mask</summary>
      ///<param name="img2">The image to be subtraced from the current image</param>
      ///<param name="mask">The mask for the subtract operation</param>
      ///<returns> The result of elementwise subtrating img2 from the current image, using the specific mask</returns>
      public Image<TColor, TDepth> Sub(Image<TColor, TDepth> img2, Image<Gray, Byte> mask)
      {
         Image<TColor, TDepth> res = CopyBlank();
         CvInvoke.cvSub(Ptr, img2.Ptr, res.Ptr, mask.Ptr);
         return res;
      }

      ///<summary> Elementwise subtrace a color from the current image</summary>
      ///<param name="val">The color value to be subtraced from the current image</param>
      ///<returns> The result of elementwise subtracting color 'val' from the current image</returns>
      [ExposableMethod(Exposable = true, Category = "Math Functions")]
      public Image<TColor, TDepth> Sub(TColor val)
      {
         Image<TColor, TDepth> res = CopyBlank();
         CvInvoke.cvSubS(Ptr, val.MCvScalar, res.Ptr, IntPtr.Zero);
         return res;
      }

      /// <summary>
      /// result = val - this
      /// </summary>
      /// <param name="val">the value which subtract this image</param>
      /// <returns>val - this</returns>
      [ExposableMethod(Exposable = true, Category = "Math Functions")]
      public Image<TColor, TDepth> SubR(TColor val)
      {
         Image<TColor, TDepth> res = CopyBlank();
         CvInvoke.cvSubRS(Ptr, val.MCvScalar, res.Ptr, IntPtr.Zero);
         return res;
      }

      /// <summary>
      /// result = val - this, using a mask
      /// </summary>
      /// <param name="val">the value which subtract this image</param>
      /// <param name="mask"> The mask for substraction</param>
      /// <returns>val - this, with mask</returns>
      public Image<TColor, TDepth> SubR(TColor val, Image<Gray, Byte> mask)
      {
         Image<TColor, TDepth> res = CopyBlank();
         CvInvoke.cvSubRS(Ptr, val.MCvScalar, res.Ptr, mask.Ptr);
         return res;
      }
      #endregion

      #region Addition methods
      ///<summary> Elementwise add another image with the current image </summary>
      ///<param name="img2">The image to be added to the current image</param>
      ///<returns> The result of elementwise adding img2 to the current image</returns>
      public Image<TColor, TDepth> Add(Image<TColor, TDepth> img2)
      {
         Image<TColor, TDepth> res = CopyBlank();
         CvInvoke.cvAdd(Ptr, img2.Ptr, res.Ptr, IntPtr.Zero);
         return res;
      }
      ///<summary> Elementwise add <paramref name="img2"/> with the current image, using a mask</summary>
      ///<param name="img2">The image to be added to the current image</param>
      ///<param name="mask">The mask for the add operation</param>
      ///<returns> The result of elementwise adding img2 to the current image, using the specific mask</returns>
      public Image<TColor, TDepth> Add(Image<TColor, TDepth> img2, Image<Gray, Byte> mask)
      {
         Image<TColor, TDepth> res = CopyBlank();
         CvInvoke.cvAdd(Ptr, img2.Ptr, res.Ptr, mask.Ptr);
         return res;
      }
      ///<summary> Elementwise add a color <paramref name="val"/> to the current image</summary>
      ///<param name="val">The color value to be added to the current image</param>
      ///<returns> The result of elementwise adding color <paramref name="val"/> from the current image</returns>
      [ExposableMethod(Exposable = true, Category = "Math Functions")]
      public Image<TColor, TDepth> Add(TColor val)
      {
         Image<TColor, TDepth> res = CopyBlank();
         CvInvoke.cvAddS(Ptr, val.MCvScalar, res.Ptr, IntPtr.Zero);
         return res;
      }
      #endregion

      #region Multiplication methods
      ///<summary> Elementwise multiply another image with the current image and the <paramref name="scale"/></summary>
      ///<param name="img2">The image to be elementwise multiplied to the current image</param>
      ///<param name="scale">The scale to be multiplied</param>
      ///<returns> this .* img2 * scale </returns>
      public Image<TColor, TDepth> Mul(Image<TColor, TDepth> img2, double scale)
      {
         Image<TColor, TDepth> res = CopyBlank();
         CvInvoke.cvMul(Ptr, img2.Ptr, res.Ptr, scale);
         return res;
      }

      ///<summary> Elementwise multiply <paramref name="img2"/> with the current image</summary>
      ///<param name="img2">The image to be elementwise multiplied to the current image</param>
      ///<returns> this .* img2 </returns>
      public Image<TColor, TDepth> Mul(Image<TColor, TDepth> img2)
      {
         return Mul(img2, 1.0);
      }

      ///<summary> Elementwise multiply the current image with <paramref name="scale"/></summary>
      ///<param name="scale">The scale to be multiplied</param>
      ///<returns> The scaled image </returns>
      [ExposableMethod(Exposable = true, Category = "Math Functions")]
      public Image<TColor, TDepth> Mul(double scale)
      {
         Image<TColor, TDepth> res = CopyBlank();
         CvInvoke.cvConvertScale(Ptr, res.Ptr, scale, 0.0);
         return res;
      }
      #endregion

      /// <summary>
      /// Accumulate <paramref name="img2"/> to the current image using the specific mask
      /// </summary>
      /// <param name="img2">The image to be added to the current image</param>
      /// <param name="mask">the mask</param>
      public void Acc(Image<TColor, TDepth> img2, Image<Gray, Byte> mask)
      {
         CvInvoke.cvAcc(img2.Ptr, Ptr, mask.Ptr);
      }

      /// <summary>
      /// Accumulate <paramref name="img2"/> to the current image using the specific mask
      /// </summary>
      /// <param name="img2">The image to be added to the current image</param>
      public void Acc(Image<TColor, TDepth> img2)
      {
         CvInvoke.cvAcc(img2.Ptr, Ptr, IntPtr.Zero);
      }

      ///<summary> 
      ///Return the weighted sum such that: res = this * alpha + img2 * beta + gamma
      ///</summary>
      public Image<TColor, TDepth> AddWeighted(Image<TColor, TDepth> img2, double alpha, double beta, double gamma)
      {
         Image<TColor, TDepth> res = CopyBlank();
         CvInvoke.cvAddWeighted(Ptr, alpha, img2.Ptr, beta, gamma, res.Ptr);
         return res;
      }

      ///<summary> 
      /// Update Running Average. <i>this</i> = (1-alpha)*<i>this</i> + alpha*img
      ///</summary>
      ///<param name="img">Input image, 1- or 3-channel, Byte or Single (each channel of multi-channel image is processed independently). </param>
      ///<param name="alpha">the weight of <paramref name="img"/></param>
      public void RunningAvg(Image<TColor, TDepth> img, double alpha)
      {
         RunningAvg(img, alpha, null);
      }

      ///<summary> 
      /// Update Running Average. <i>this</i> = (1-alpha)*<i>this</i> + alpha*img, using the mask
      ///</summary>
      ///<param name="img">Input image, 1- or 3-channel, Byte or Single (each channel of multi-channel image is processed independently). </param>
      ///<param name="alpha">The weight of <paramref name="img"/></param>
      ///<param name="mask">The mask for the running average</param>
      public void RunningAvg(Image<TColor, TDepth> img, double alpha, Image<Gray, Byte> mask)
      {
         CvInvoke.cvRunningAvg(img.Ptr, Ptr, alpha, mask == null ? IntPtr.Zero : mask.Ptr);
      }

      ///<summary> 
      ///Computes absolute different between <i>this</i> image and the other image
      ///</summary>
      ///<param name="img2">The other image to compute absolute different with</param>
      ///<returns> The image that contains the absolute different value</returns>
      public Image<TColor, TDepth> AbsDiff(Image<TColor, TDepth> img2)
      {
         Image<TColor, TDepth> res = CopyBlank();
         CvInvoke.cvAbsDiff(Ptr, img2.Ptr, res.Ptr);
         return res;
      }
      #endregion

      #region Math Functions
      /// <summary>
      /// Raises every element of input array to p
      /// dst(I)=src(I)^p, if p is integer
      /// dst(I)=abs(src(I))^p, otherwise
      /// </summary>
      /// <param name="power">The exponent of power</param>
      /// <returns>The power image</returns>
      [ExposableMethod(Exposable = true, Category = "Math Functions")]
      public Image<TColor, TDepth> Pow(double power)
      {
         Image<TColor, TDepth> res = CopyBlank();
         CvInvoke.cvPow(Ptr, res.Ptr, power);
         return res;
      }

      /// <summary>
      /// calculates exponent of every element of input array:
      /// dst(I)=exp(src(I))
      /// Maximum relative error is ~7e-6. Currently, the function converts denormalized values to zeros on output.
      /// </summary>
      /// <returns>The exponent image</returns>
      [ExposableMethod(Exposable = true, Category = "Math Functions")]
      public Image<TColor, TDepth> Exp()
      {
         Image<TColor, TDepth> res = CopyBlank();
         CvInvoke.cvExp(Ptr, res.Ptr);
         return res;
      }

      /// <summary>
      /// Calculates natural logarithm of absolute value of every element of input array
      /// </summary>
      /// <returns>Natural logarithm of absolute value of every element of input array</returns>
      [ExposableMethod(Exposable = true, Category = "Math Functions")]
      public Image<TColor, TDepth> Log()
      {
         Image<TColor, TDepth> res = CopyBlank();
         CvInvoke.cvLog(Ptr, res.Ptr);
         return res;
      }
      #endregion

      #region Sampling, Interpolation and Geometrical Transforms
      ///<summary> Sample the pixel values on the specific line segment </summary>
      ///<param name="line"> The line to obtain samples</param>
      ///<returns>The values on the (Eight-connected) line </returns>
      public TDepth[,] Sample(LineSegment2D line)
      {
         return Sample(line, Emgu.CV.CvEnum.CONNECTIVITY.EIGHT_CONNECTED);
      }

      /// <summary>
      /// Sample the pixel values on the specific line segment
      /// </summary>
      /// <param name="line">The line to obtain samples</param>
      /// <param name="type">The sampling type</param>
      /// <returns>The values on the line</returns>
      public TDepth[,] Sample(LineSegment2D line, CvEnum.CONNECTIVITY type)
      {
         int size = type == Emgu.CV.CvEnum.CONNECTIVITY.EIGHT_CONNECTED ?
            Math.Max(Math.Abs(line.P2.X - line.P1.X), Math.Abs(line.P2.Y - line.P1.Y))
            : Math.Abs(line.P2.X - line.P1.X) + Math.Abs(line.P2.Y - line.P1.Y);

         TDepth[,] data = new TDepth[size, NumberOfChannels];
         GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
         CvInvoke.cvSampleLine(
             Ptr,
             line.P1,
             line.P2,
             handle.AddrOfPinnedObject(),
             type);
         handle.Free();
         return data;
      }

      /// <summary>
      /// Scale the image to the specific size 
      /// </summary>
      /// <param name="width">The width of the returned image.</param>
      /// <param name="height">The height of the returned image.</param>
      /// <returns>The resized image</returns>
      [ExposableMethod(Exposable = true)]
      public Image<TColor, TDepth> Resize(int width, int height)
      {
         Image<TColor, TDepth> imgScale = new Image<TColor, TDepth>(width, height);
         CvInvoke.cvResize(Ptr, imgScale.Ptr, CvEnum.INTER.CV_INTER_LINEAR);
         return imgScale;
      }

      /// <summary>
      /// Scale the image to the specific size
      /// </summary>
      /// <param name="width">The width of the returned image.</param>
      /// <param name="height">The height of the returned image.</param>
      /// <param name="preserverScale">if true, the scale is preservered and the resulting image has maximum width(height) possible that is &lt;= <paramref name="width"/> (<paramref name="height"/>), if false, this function is equaivalent to Resize(int width, int height)</param>
      /// <returns></returns>
      public Image<TColor, TDepth> Resize(int width, int height, bool preserverScale)
      {
         return preserverScale ?
            Resize(Math.Min((double)width / Width, (double)height / Height))
            : Resize(width, height);
      }

      /// <summary>
      /// Scale the image to the specific size: width *= scale; height *= scale  
      /// </summary>
      /// <returns>The scaled image</returns>
      [ExposableMethod(Exposable = true)]
      public Image<TColor, TDepth> Resize(double scale)
      {
         return Resize(
             (int)(Width * scale),
             (int)(Height * scale));
      }

      /// <summary>
      /// Rotate the image the specified angle cropping the result to the original size
      /// </summary>
      /// <param name="angle">The angle of rotation in degrees.</param>
      /// <param name="background">The color with wich to fill the background</param>        
      public Image<TColor, TDepth> Rotate(double angle, TColor background)
      {
         return Rotate(angle, background, true);
      }

      /// <summary>
      /// Transforms source image using the specified matrix
      /// </summary>
      /// <param name="mapMatrix">2x3 transformation matrix</param>
      /// <param name="interpolationType">Interpolation type</param>
      /// <param name="warpType">Warp type</param>
      /// <param name="backgroundColor">A value used to fill outliers</param>
      /// <returns>The result of the transformation</returns>
      public Image<TColor, TDepth> WarpAffine(Matrix<float> mapMatrix, CvEnum.INTER interpolationType, CvEnum.WARP warpType, TColor backgroundColor)
      {
         return WarpAffine(mapMatrix, Width, Height, interpolationType, warpType, backgroundColor);
      }

      /// <summary>
      /// Transforms source image using the specified matrix
      /// </summary>
      /// <param name="mapMatrix">2x3 transformation matrix</param>
      /// <param name="interpolationType">Interpolation type</param>
      /// <param name="warpType">Warp type</param>
      /// <param name="backgroundColor">A value used to fill outliers</param>
      /// <returns>The result of the transformation</returns>
      public Image<TColor, TDepth> WarpAffine(Matrix<double> mapMatrix, CvEnum.INTER interpolationType, CvEnum.WARP warpType, TColor backgroundColor)
      {
         return WarpAffine(mapMatrix, Width, Height, interpolationType, warpType, backgroundColor);
      }

      /// <summary>
      /// Transforms source image using the specified matrix
      /// </summary>
      /// <param name="mapMatrix">2x3 transformation matrix</param>
      /// <param name="width">The width of the resulting image</param>
      /// <param name="height">the height of the resulting image</param>
      /// <param name="interpolationType">Interpolation type</param>
      /// <param name="warpType">Warp type</param>
      /// <param name="backgroundColor">A value used to fill outliers</param>
      /// <returns>The result of the transformation</returns>
      public Image<TColor, TDepth> WarpAffine(Matrix<float> mapMatrix, int width, int height, CvEnum.INTER interpolationType, CvEnum.WARP warpType, TColor backgroundColor)
      {
         Image<TColor, TDepth> res = new Image<TColor, TDepth>(width, height);
         CvInvoke.cvWarpAffine(Ptr, res.Ptr, mapMatrix.Ptr, (int)interpolationType | (int)warpType, backgroundColor.MCvScalar);
         return res;
      }

      /// <summary>
      /// Transforms source image using the specified matrix
      /// </summary>
      /// <param name="mapMatrix">2x3 transformation matrix</param>
      /// <param name="width">The width of the resulting image</param>
      /// <param name="height">the height of the resulting image</param>
      /// <param name="interpolationType">Interpolation type</param>
      /// <param name="warpType">Warp type</param>
      /// <param name="backgroundColor">A value used to fill outliers</param>
      /// <returns>The result of the transformation</returns>
      public Image<TColor, TDepth> WarpAffine(Matrix<double> mapMatrix, int width, int height, CvEnum.INTER interpolationType, CvEnum.WARP warpType, TColor backgroundColor)
      {
         Image<TColor, TDepth> res = new Image<TColor, TDepth>(width, height);
         CvInvoke.cvWarpAffine(Ptr, res.Ptr, mapMatrix.Ptr, (int)interpolationType | (int)warpType, backgroundColor.MCvScalar);
         return res;
      }

      /// <summary>
      /// Transforms source image using the specified matrix
      /// </summary>
      /// <param name="mapMatrix">3x3 transformation matrix</param>
      /// <param name="interpolationType">Interpolation type</param>
      /// <param name="warpType">Warp type</param>
      /// <param name="backgroundColor">A value used to fill outliers</param>
      /// <returns>The result of the transformation</returns>
      public Image<TColor, TDepth> WarpPerspective(Matrix<float> mapMatrix, CvEnum.INTER interpolationType, CvEnum.WARP warpType, TColor backgroundColor)
      {
         return WarpPerspective(mapMatrix, Width, Height, interpolationType, warpType, backgroundColor);
      }

      /// <summary>
      /// Transforms source image using the specified matrix
      /// </summary>
      /// <param name="mapMatrix">3x3 transformation matrix</param>
      /// <param name="width">The width of the resulting image</param>
      /// <param name="height">the height of the resulting image</param>
      /// <param name="interpolationType">Interpolation type</param>
      /// <param name="warpType">Warp type</param>
      /// <param name="backgroundColor">A value used to fill outliers</param>
      /// <returns>The result of the transformation</returns>
      public Image<TColor, TDepth> WarpPerspective(Matrix<float> mapMatrix, int width, int height, CvEnum.INTER interpolationType, CvEnum.WARP warpType, TColor backgroundColor)
      {
         Image<TColor, TDepth> res = new Image<TColor, TDepth>(width, height);
         CvInvoke.cvWarpPerspective(Ptr, res.Ptr, mapMatrix.Ptr, (int)interpolationType | (int)warpType, backgroundColor.MCvScalar);
         return res;
      }

      /// <summary>
      /// Rotate this image the specified <paramref name="angle"/>
      /// </summary>
      /// <param name="angle">The angle of rotation in degrees.</param>
      /// <param name="background">The color with wich to fill the background</param>
      /// <param name="crop">If set to true the image is cropped to its original size, possibly losing corners information. If set to false the result image has different size than original and all rotation information is preserved</param>
      /// <returns>The rotated image</returns>
      [ExposableMethod(Exposable = true)]
      public Image<TColor, TDepth> Rotate(double angle, TColor background, bool crop)
      {
         Size size = Size;
         if (crop)
         {
            PointF center = new PointF(size.Width * 0.5f, size.Height * 0.5f);
            using (RotationMatrix2D<double> rotationMatrix = new RotationMatrix2D<double>(center, -angle, 1))
            {
               return WarpAffine(rotationMatrix, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC, Emgu.CV.CvEnum.WARP.CV_WARP_FILL_OUTLIERS, background);
            }
         }
         else
         {
            //Maximum possible size is equal to the diagonal length of the image
            int maxSize = (int)Math.Round(Math.Sqrt(size.Width * size.Width + size.Height * size.Height));
            double offsetX = (maxSize - size.Width) * 0.5;
            double offsetY = (maxSize - size.Height) * 0.5;

            PointF center = new PointF(maxSize * .5f, maxSize * .5f);

            using (RotationMatrix2D<double> rotationMatrix = new RotationMatrix2D<double>(center, -angle, 1))
            using (Matrix<double> corners = new Matrix<double>(new double[4, 3]{
                 {offsetX,offsetY,1},
                 {offsetX,offsetY+size.Height,1},
                 {offsetX+size.Width,offsetY,1},
                 {offsetX+size.Width,offsetY+size.Height,1}}))
            using (Matrix<double> rotatedCorners = new Matrix<double>(4, 2))
            using (Image<TColor, TDepth> tempImage1 = new Image<TColor, TDepth>(maxSize, maxSize, background))
            using (Image<TColor, TDepth> tempImage2 = new Image<TColor, TDepth>(maxSize, maxSize))
            {
               // Frame the original image into a bigger one of side maxSize
               // Rotating the framed image will always keep the original image without losing corners information
               System.Drawing.Rectangle CvR = new System.Drawing.Rectangle((maxSize - size.Width) >> 1, (maxSize - size.Height) >> 1, size.Width, size.Height);
               CvInvoke.cvSetImageROI(tempImage1.Ptr, CvR);
               CvInvoke.cvCopy(Ptr, tempImage1.Ptr, IntPtr.Zero);
               CvInvoke.cvResetImageROI(tempImage1.Ptr);

               // Rotate
               CvInvoke.cvWarpAffine(tempImage1.Ptr, tempImage2.Ptr, rotationMatrix.Ptr, (int)CvEnum.INTER.CV_INTER_CUBIC | (int)CvEnum.WARP.CV_WARP_FILL_OUTLIERS, background.MCvScalar);

               #region  Calculate the position of the original corners in the rotated image
               CvInvoke.cvGEMM(corners.Ptr, rotationMatrix.Ptr, 1, IntPtr.Zero, 1, rotatedCorners.Ptr, Emgu.CV.CvEnum.GEMM_TYPE.CV_GEMM_B_T);
               double[,] data = rotatedCorners.Data;
               int minX = (int)Math.Round(Math.Min(Math.Min(data[0, 0], data[1, 0]), Math.Min(data[2, 0], data[3, 0])));
               int maxX = (int)Math.Round(Math.Max(Math.Max(data[0, 0], data[1, 0]), Math.Max(data[2, 0], data[3, 0])));
               int minY = (int)Math.Round(Math.Min(Math.Min(data[0, 1], data[1, 1]), Math.Min(data[2, 1], data[3, 1])));
               int maxY = (int)Math.Round(Math.Max(Math.Max(data[0, 1], data[1, 1]), Math.Max(data[2, 1], data[3, 1])));
               System.Drawing.Rectangle toCrop = new System.Drawing.Rectangle(minX, maxSize - maxY, maxX - minX, maxY - minY);
               #endregion
             
               //Crop the region of interest
               return tempImage2.Copy(toCrop);
            }
         }
      }

      ///<summary>
      /// Convert the image to log polar, simulating the human foveal vision
      /// </summary>
      /// <param name="center">The transformation center, where the output precision is maximal</param>
      /// <param name="M">Magnitude scale parameter</param>
      /// <param name="flags">A combination of interpolation method and the optional flag CV_WARP_FILL_OUTLIERS and/or CV_WARP_INVERSE_MAP</param>
      /// <returns>The converted image</returns>
      public Image<TColor, TDepth> LogPolar(System.Drawing.PointF center, double M, int flags)
      {
         Image<TColor, TDepth> imgPolar = CopyBlank();
         CvInvoke.cvLogPolar(Ptr, imgPolar.Ptr, center, M, flags);
         return imgPolar;
      }
      #endregion

      #region Image color and depth conversion
      private static CvEnum.COLOR_CONVERSION GetColorCvtCode(Type srcType, Type destType)
      {
         ColorInfoAttribute srcInfo = (ColorInfoAttribute)srcType.GetCustomAttributes(typeof(ColorInfoAttribute), true)[0];
         ColorInfoAttribute destInfo = (ColorInfoAttribute)destType.GetCustomAttributes(typeof(ColorInfoAttribute), true)[0];

         String key = String.Format("CV_{0}2{1}", srcInfo.ConversionCodename, destInfo.ConversionCodename);
         return (CvEnum.COLOR_CONVERSION)Enum.Parse(typeof(CvEnum.COLOR_CONVERSION), key, true);
      }

      ///<summary> Convert the current image to the specific color and depth </summary>
      ///<typeparam name="TOtherColor"> The type of color to be converted to </typeparam>
      ///<typeparam name="TOtherDepth"> The type of pixel depth to be converted to </typeparam>
      ///<returns> Image of the specific color and depth </returns>
      [ExposableMethod(
         Exposable = true,
         Category = "Convertion",
         GenericParametersOptions = ":Emgu.CV.Bgr,Emgu.CV.Gray;:System.Single,System.Byte,System.Double")]
      public Image<TOtherColor, TOtherDepth> Convert<TOtherColor, TOtherDepth>() 
         where TOtherColor : struct, IColor
      {
         Image<TOtherColor, TOtherDepth> res = new Image<TOtherColor, TOtherDepth>(Size);
         res.ConvertFrom(this);
         return res;
      }

      /// <summary>
      /// Convert the source image to the current image, if the size are different, the current image will be a resized version of the srcImage. 
      /// </summary>
      /// <typeparam name="TSrcColor">The color type of the source image</typeparam>
      /// <typeparam name="TSrcDepth">The color depth of the source image</typeparam>
      /// <param name="srcImage">The sourceImage</param>
      public void ConvertFrom<TSrcColor, TSrcDepth>(Image<TSrcColor, TSrcDepth> srcImage) 
         where TSrcColor : struct, IColor
      {
         if (!Size.Equals(srcImage.Size))
         {  //if the size of the source image do not match the size of the current image
            using (Image<TSrcColor, TSrcDepth> tmp = srcImage.Resize(Width, Height))
            {
               ConvertFrom(tmp);
               return;
            }
         }

         if (typeof(TColor) == typeof(TSrcColor))
         {
            #region same color
            if (typeof(TDepth) == typeof(TSrcDepth))
            {   //same depth
               CvInvoke.cvCopy(srcImage.Ptr, Ptr, IntPtr.Zero);
            }
            else
            {
               //different depth
               int channelCount = NumberOfChannels;
               Type dstDepth = typeof(TDepth);
               Type srcDepth = typeof(TSrcDepth);
               {
                  if (dstDepth == typeof(Byte) && srcDepth != typeof(Byte))
                  {
                     double min = 0.0, max = 0.0, scale, shift;
                     System.Drawing.Point p1 = new System.Drawing.Point();
                     System.Drawing.Point p2 = new System.Drawing.Point();
                     if (channelCount == 1)
                     {
                        CvInvoke.cvMinMaxLoc(srcImage.Ptr, ref min, ref max, ref p1, ref p2, IntPtr.Zero);
                     }
                     else
                     {
                        for (int i = 0; i < channelCount; i++)
                        {
                           double minForChannel = 0.0, maxForChannel = 0.0;
                           CvInvoke.cvSetImageCOI(srcImage.Ptr, i + 1);
                           CvInvoke.cvMinMaxLoc(srcImage.Ptr, ref minForChannel, ref maxForChannel, ref p1, ref p2, IntPtr.Zero);
                           min = Math.Min(min, minForChannel);
                           max = Math.Max(max, maxForChannel);
                        }
                        CvInvoke.cvSetImageCOI(srcImage.Ptr, 0);
                     }
                     if (max <= 255.0 && min >= 0)
                     {
                        scale = 1.0;
                        shift = 0.0;
                     }
                     else
                     {
                        scale = (max == min) ? 0.0 : 255.0 / (max - min);
                        shift = (scale == 0) ? min : -min * scale;
                     }
                     CvInvoke.cvConvertScaleAbs(srcImage.Ptr, Ptr, scale, shift);
                  }
                  else
                  {
                     CvInvoke.cvConvertScale(srcImage.Ptr, Ptr, 1.0, 0.0);
                  }
               }
            }
            #endregion
         }
         else
         {
            #region different color
            if (typeof(TDepth) == typeof(TSrcDepth))
            {   //same depth

               ConvertColor(srcImage.Ptr, Ptr, typeof(TSrcColor), typeof(TColor), Width, Height);
            }
            else
            {   //different depth
               using (Image<TSrcColor, TDepth> tmp = srcImage.Convert<TSrcColor, TDepth>()) //convert depth
                  ConvertColor(tmp.Ptr, Ptr, typeof(TSrcColor), typeof(TColor), Width, Height);
            }
            #endregion
         }
      }

      private static void ConvertColor(IntPtr src, IntPtr dest, Type srcColor, Type destColor, int width, int height)
      {
         try
         {
            // if the direct conversion exist, apply the conversion
            CvInvoke.cvCvtColor(src, dest, GetColorCvtCode(srcColor, destColor));
         }
         catch (Exception)
         {
            //if a direct conversion doesn't exist, apply a two step conversion
            using (Image<Bgr, TDepth> tmp = new Image<Bgr, TDepth>(width, height))
            {
               CvInvoke.cvCvtColor(src, tmp.Ptr, GetColorCvtCode(srcColor, typeof(Bgr)));
               CvInvoke.cvCvtColor(tmp.Ptr, dest, GetColorCvtCode(typeof(Bgr), destColor));
            }
         }
      }

      ///<summary> Convert the current image to the specific depth, at the same time scale and shift the values of the pixel</summary>
      ///<param name="scale"> The value to be multipled with the pixel </param>
      ///<param name="shift"> The value to be added to the pixel</param>
      /// <typeparam name="TOtherDepth"> The type of depth to convert to</typeparam>
      ///<returns> Image of the specific depth, val = val * scale + shift </returns>
      public Image<TColor, TOtherDepth> ConvertScale<TOtherDepth>(double scale, double shift)
      {
         Image<TColor, TOtherDepth> res = new Image<TColor, TOtherDepth>(Width, Height);

         if (typeof(TOtherDepth) == typeof(System.Byte))
            CvInvoke.cvConvertScaleAbs(Ptr, res.Ptr, scale, shift);
         else
            CvInvoke.cvConvertScale(Ptr, res.Ptr, scale, shift);

         return res;
      }
      #endregion

      #region Conversion with Bitmap
      /// <summary>
      /// The Get property provide a more efficient way to convert Image&lt;Gray, Byte&gt;, Image&lt;Bgr, Byte&gt; and Image&lt;Bgra, Byte&gt; into Bitmap
      /// such that the image data is <b>shared</b> with Bitmap. 
      /// If you change the pixel value on the Bitmap, you change the pixel values on the Image object as well!
      /// For other types of image this property has the same effect as ToBitmap()
      /// <b>Take extra caution not to use the Bitmap after the Image object is disposed</b>
      /// The Set property convert the bitmap to this Image type.
      /// </summary>
      public Bitmap Bitmap
      {
         get
         {
            IntPtr scan0;
            int step;
            System.Drawing.Size size;
            CvInvoke.cvGetRawData(Ptr, out scan0, out step, out size);

            if (
               typeof(TColor) == typeof(Gray) && 
               typeof(TDepth) == typeof(Byte))
            {   //Grayscale of Bytes
               Bitmap bmp = new Bitmap(
                   size.Width,
                   size.Height,
                   step,
                   System.Drawing.Imaging.PixelFormat.Format8bppIndexed,
                   scan0
                   );
               bmp.Palette = Util.GrayscalePalette;
               return bmp;
            }
            // Mono in Linux doesn't support scan0 constructure with Format24bppRgb, use ToBitmap instead
            // See https://bugzilla.novell.com/show_bug.cgi?id=363431
            // TODO: check mono buzilla Bug 363431 to see when it will be fixed 
            else if (
               Platform.OperationSystem == Emgu.Util.TypeEnum.OS.Windows &&
               Platform.Runtime == Emgu.Util.TypeEnum.Runtime.DotNet &&
               typeof(TColor) == typeof(Bgr) && 
               typeof(TDepth) == typeof(Byte))
            {   //Bgr byte    
               return new Bitmap(
                   size.Width,
                   size.Height,
                   step,
                   System.Drawing.Imaging.PixelFormat.Format24bppRgb,
                   scan0);
            }
            else if (
               typeof(TColor) == typeof(Bgra) && 
               typeof(TDepth) == typeof(Byte))
            {   //Bgra byte
               return new Bitmap(
                   size.Width,
                   size.Height,
                   step,
                   System.Drawing.Imaging.PixelFormat.Format32bppArgb,
                   scan0);
            }
            /*
            //PixelFormat.Format16bppGrayScale is not supported in .NET
            else if (typeof(TColor) == typeof(Gray) && typeof(TDepth) == typeof(UInt16))
            {
               return new Bitmap(
                  size.width,
                  size.height,
                  step,
                  System.Drawing.Imaging.PixelFormat.Format16bppGrayScale;
                  scan0);
            }*/
            else
            {  //default handler
               return ToBitmap();
            }
         }
         set
         {
            #region reallocate memory if necessary
            if (Ptr == IntPtr.Zero || !Size.Equals(value.Size))
            {
               AllocateData(value.Height, value.Width, NumberOfChannels);
            }
            #endregion

            switch (value.PixelFormat)
            {
               case System.Drawing.Imaging.PixelFormat.Format32bppArgb:
                  if (typeof(TColor) == typeof(Bgra) && typeof(TDepth) == typeof(Byte))
                     CopyFromBitmap(value);
                  else
                  {
                     using (Image<Bgra, Byte> tmp = new Image<Bgra, byte>(value))
                        ConvertFrom(tmp);
                  }
                  break;
               case System.Drawing.Imaging.PixelFormat.Format8bppIndexed:
                  if (typeof(TColor) == typeof(Bgra) && typeof(TDepth) == typeof(Byte))
                  {
                     using (Image<Gray, Byte> indexValue = new Image<Gray, byte>(value.Width, value.Height))
                     {
                        indexValue.CopyFromBitmap(value);
                        Matrix<Byte> bTable, gTable, rTable, aTable;
                        Util.ColorPaletteToLookupTable(value.Palette, out bTable, out gTable, out rTable, out aTable);

                        using (Image<Gray, Byte> b = indexValue.CopyBlank())
                        using (Image<Gray, Byte> g = indexValue.CopyBlank())
                        using (Image<Gray, Byte> r = indexValue.CopyBlank())
                        using (Image<Gray, Byte> a = indexValue.CopyBlank())
                        {
                           CvInvoke.cvLUT(indexValue.Ptr, b.Ptr, bTable.Ptr);
                           CvInvoke.cvLUT(indexValue.Ptr, g.Ptr, gTable.Ptr);
                           CvInvoke.cvLUT(indexValue.Ptr, r.Ptr, rTable.Ptr);
                           CvInvoke.cvLUT(indexValue.Ptr, a.Ptr, aTable.Ptr);
                           CvInvoke.cvMerge(b.Ptr, g.Ptr, r.Ptr, a.Ptr, Ptr);
                        }
                        bTable.Dispose(); gTable.Dispose(); rTable.Dispose(); aTable.Dispose();
                     }
                  }
                  else
                  {
                     using (Image<Bgra, Byte> tmp = new Image<Bgra, byte>(value))
                        ConvertFrom(tmp);
                  }
                  break;
               case System.Drawing.Imaging.PixelFormat.Format24bppRgb:
                  if (typeof(TColor) == typeof(Bgr) && typeof(TDepth) == typeof(Byte))
                     CopyFromBitmap(value);
                  else
                  {
                     using (Image<Bgr, Byte> tmp = new Image<Bgr, byte>(value))
                        ConvertFrom(tmp);
                  }
                  break;
               case System.Drawing.Imaging.PixelFormat.Format1bppIndexed:
                  if (typeof(TColor) == typeof(Gray) && typeof(TDepth) == typeof(Byte))
                  {
                     int rows = value.Height;
                     int cols = value.Width;
                     System.Drawing.Imaging.BitmapData data = value.LockBits(
                         new System.Drawing.Rectangle(0, 0, cols, rows),
                         System.Drawing.Imaging.ImageLockMode.ReadOnly,
                         value.PixelFormat);

                     int fullByteCount = cols >> 3;
                     int partialBitCount = cols & 7;

                     int mask = 1 << 7;

                     Int64 srcAddress = data.Scan0.ToInt64();
                     Byte[, ,] imagedata = Data as Byte[, ,];

                     Byte[] row = new byte[fullByteCount + (partialBitCount == 0 ? 0 : 1)];

                     int v = 0;
                     for (int i = 0; i < rows; i++, srcAddress += data.Stride)
                     {
                        Marshal.Copy((IntPtr)srcAddress, row, 0, row.Length);

                        for (int j = 0; j < cols; j++, v <<= 1)
                        {
                           if ((j & 7) == 0)
                           {  //fetch the next byte 
                              v = row[j >> 3];
                           }
                           imagedata[i, j, 0] = (v & mask) == 0 ? (Byte)0 : (Byte)255;
                        }
                     }
                  }
                  else
                  {
                     using (Image<Gray, Byte> tmp = new Image<Gray, Byte>(value))
                        ConvertFrom(tmp);
                  }
                  break;
               default:
                  #region Handle other image type
                  /*
				               Bitmap bgraImage = new Bitmap(value.Width, value.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
				               using (Graphics g = Graphics.FromImage(bgraImage))
				               {
				                  g.DrawImageUnscaled(value, 0, 0, value.Width, value.Height);
				               }
				               Bitmap = bgraImage;*/
                  using (Image<Bgra, Byte> tmp1 = new Image<Bgra, Byte>(value.Width, value.Height))
                  {
                     Byte[, ,] data = tmp1.Data;
                     for (int i = 0; i < value.Width; i++)
                        for (int j = 0; j < value.Height; j++)
                        {
                           System.Drawing.Color color = value.GetPixel(i, j);
                           data[j, i, 0] = color.B;
                           data[j, i, 1] = color.G;
                           data[j, i, 2] = color.R;
                           data[j, i, 3] = color.A;
                        }

                     ConvertFrom<Bgra, Byte>(tmp1);
                  }
                  #endregion
                  break;
            }
         }
      }

      /// <summary>
      /// Utility function for Bitmap Set property
      /// </summary>
      /// <param name="bmp"></param>
      private void CopyFromBitmap(Bitmap bmp)
      {
         int rows = bmp.Height;
         System.Drawing.Imaging.BitmapData data = bmp.LockBits(
             new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height),
             System.Drawing.Imaging.ImageLockMode.ReadOnly,
             bmp.PixelFormat);

         int arrayWidthStep = _sizeOfElement * NumberOfChannels * _array.GetLength(1);

         Int64 destAddress = _dataHandle.AddrOfPinnedObject().ToInt64();
         Int64 srcAddress = data.Scan0.ToInt64();
         for (int i = 0; i < rows; i++, destAddress += arrayWidthStep, srcAddress += data.Stride)
         {
            Emgu.Util.Toolbox.memcpy((IntPtr)destAddress, (IntPtr)srcAddress, data.Stride);
         }
         bmp.UnlockBits(data);
      }

      /// <summary> 
      /// Convert this image into Bitmap, the pixel values are copied over to the Bitmap
      /// </summary>
      /// <remarks> For better performance on Image&lt;Gray, Byte&gt; and Image&lt;Bgr, Byte&gt;, consider using the Bitmap property </remarks>
      /// <returns> This image in Bitmap format, the pixel data are copied over to the Bitmap</returns>
      public Bitmap ToBitmap()
      {
         Type typeOfColor = typeof(TColor);
         Type typeofDepth = typeof(TDepth);

         if (typeOfColor == typeof(Gray)) // if this is a gray scale image
         {
            if (typeofDepth == typeof(Byte))
            {
               System.Drawing.Size size;
               IntPtr startPtr;
               int widthStep;
               CvInvoke.cvGetRawData(Ptr, out startPtr, out widthStep, out size);
               Int64 start = startPtr.ToInt64();

               Bitmap bmp = new Bitmap(size.Width, size.Height, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
               System.Drawing.Imaging.BitmapData data = bmp.LockBits(
                   new System.Drawing.Rectangle(0, 0, size.Width, size.Height),
                   System.Drawing.Imaging.ImageLockMode.WriteOnly,
                   System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
               Int64 dataPtr = data.Scan0.ToInt64();
               
               for (int row = 0; row < data.Height; row++, start += widthStep, dataPtr += data.Stride)
                  Emgu.Util.Toolbox.memcpy((IntPtr)dataPtr, (IntPtr)start, data.Stride);

               bmp.UnlockBits(data);
               bmp.Palette = Util.GrayscalePalette;

               return bmp;
            }
            else
            {
               using (Image<Gray, Byte> temp = Convert<Gray, Byte>())
                  return temp.ToBitmap();
            }
         }
         else if (typeOfColor == typeof(Bgra)) //if this is Bgra image
         {
            if (typeofDepth == typeof(byte))
            {
               IntPtr startPtr;
               int widthStep;
               System.Drawing.Size size;
               CvInvoke.cvGetRawData(Ptr, out startPtr, out widthStep, out size);
               Int64 start = startPtr.ToInt64();

               Bitmap bmp = new Bitmap(size.Width, size.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
               System.Drawing.Imaging.BitmapData data = bmp.LockBits(
                    new System.Drawing.Rectangle(0, 0, size.Width, size.Height),
                    System.Drawing.Imaging.ImageLockMode.WriteOnly,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb);
               Int64 dataPtr = data.Scan0.ToInt64();

               for (int row = 0; row < data.Height; row++, start += widthStep, dataPtr += data.Stride)
                  Emgu.Util.Toolbox.memcpy((IntPtr)dataPtr, (IntPtr)start, data.Stride);

               bmp.UnlockBits(data);
               return bmp;
            }
            else
            {
               using (Image<Bgra, Byte> tmp = Convert<Bgra, Byte>())
                  return tmp.ToBitmap();
            }
         }
         else if (typeOfColor == typeof(Bgr) && typeofDepth == typeof(Byte))
         {   //if this is a Bgr Byte image
            IntPtr startPtr;
            int widthStep;
            System.Drawing.Size size;
            CvInvoke.cvGetRawData(Ptr, out startPtr, out widthStep, out size);
            Int64 start = startPtr.ToInt64();

            //create the bitmap and get the pointer to the data
            Bitmap bmp = new Bitmap(size.Width, size.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            System.Drawing.Imaging.BitmapData data = bmp.LockBits(
                new System.Drawing.Rectangle(0, 0, size.Width, size.Height),
                System.Drawing.Imaging.ImageLockMode.WriteOnly,
                System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            Int64 dataPtr = data.Scan0.ToInt64();

            for (int row = 0; row < data.Height; row++, start += widthStep, dataPtr += data.Stride)
               Emgu.Util.Toolbox.memcpy((IntPtr)dataPtr, (IntPtr)start, data.Stride);
           
            bmp.UnlockBits(data);

            return bmp;
         }
         else
         {
            using (Image<Bgr, Byte> temp = Convert<Bgr, Byte>())
               return temp.ToBitmap();
         }
      }

      ///<summary> Create a Bitmap image of certain size</summary>
      ///<param name="width">The width of the bitmap</param>
      ///<param name="height"> The height of the bitmap</param>
      ///<returns> This image in Bitmap format of the specific size</returns>
      public Bitmap ToBitmap(int width, int height)
      {
         using (Image<TColor, TDepth> scaledImage = Resize(width, height))
            return scaledImage.ToBitmap();
      }
      #endregion

      #region Pyramids
      ///<summary>
      /// Performs downsampling step of Gaussian pyramid decomposition. 
      /// First it convolves <i>this</i> image with the specified filter and then downsamples the image 
      /// by rejecting even rows and columns.
      ///</summary>
      ///<returns> The downsampled image</returns>
      [ExposableMethod(Exposable = true, Category = "Pyramids")]
      public Image<TColor, TDepth> PyrDown()
      {
         Image<TColor, TDepth> res = new Image<TColor, TDepth>(Width >> 1, Height >> 1);
         CvInvoke.cvPyrDown(Ptr, res.Ptr, CvEnum.FILTER_TYPE.CV_GAUSSIAN_5x5);
         return res;
      }

      ///<summary>
      /// Performs up-sampling step of Gaussian pyramid decomposition. 
      /// First it upsamples <i>this</i> image by injecting even zero rows and columns and then convolves 
      /// result with the specified filter multiplied by 4 for interpolation. 
      /// So the resulting image is four times larger than the source image.
      ///</summary>
      ///<returns> The upsampled image</returns>
      [ExposableMethod(Exposable = true, Category = "Pyramids")]
      public Image<TColor, TDepth> PyrUp()
      {
         Image<TColor, TDepth> res = new Image<TColor, TDepth>(Width << 1, Height << 1);
         CvInvoke.cvPyrUp(Ptr, res.Ptr, CvEnum.FILTER_TYPE.CV_GAUSSIAN_5x5);
         return res;
      }
      #endregion

      #region Special Image Transforms
      ///<summary> Use impaint to recover the intensity of the pixels which location defined by <paramref>mask</paramref> on <i>this</i> image </summary>
      ///<returns> The inpainted image </returns>
      public Image<TColor, TDepth> InPaint(Image<Gray, Byte> mask, double radius)
      {
         Image<TColor, TDepth> res = CopyBlank();
         CvInvoke.cvInpaint(Ptr, mask.Ptr, res.Ptr, CvEnum.INPAINT_TYPE.CV_INPAINT_TELEA, radius);
         return res;
      }
      #endregion

      #region Morphological Operations
      /// <summary>
      /// Perform advanced morphological transformations using erosion and dilation as basic operations.
      /// </summary>
      /// <param name="element">Structuring element</param>
      /// <param name="operation">Type of morphological operation</param>
      /// <param name="iterations">Number of times erosion and dilation are applied</param>
      /// <returns>The result of the morphological operation</returns>
      public Image<TColor, TDepth> MorphologyEx(StructuringElementEx element, CvEnum.CV_MORPH_OP operation, int iterations)
      {
         Image<TColor, TDepth> res = CopyBlank();
         
         //For MOP_GRADIENT, a temperary buffer is required
         Image<TColor, TDepth> temp = null;
         if (operation == Emgu.CV.CvEnum.CV_MORPH_OP.CV_MOP_GRADIENT)
         {
            temp = CopyBlank();
         }

         CvInvoke.cvMorphologyEx(
            Ptr, res.Ptr, 
            temp == null? IntPtr.Zero : temp.Ptr, 
            element.Ptr, 
            operation, 
            iterations);

         if (temp != null) temp.Dispose();
         return res;
      }

      ///<summary>
      ///Erodes <i>this</i> image using a 3x3 rectangular structuring element.
      ///Erosion are applied serveral (iterations) times
      ///</summary>
      ///<returns> The eroded image</returns>
      [ExposableMethod(Exposable = true, Category = "Morphological Operations")]
      public Image<TColor, TDepth> Erode(int iterations)
      {
         Image<TColor, TDepth> res = CopyBlank();
         CvInvoke.cvErode(Ptr, res.Ptr, IntPtr.Zero, iterations);
         return res;
      }

      ///<summary>
      ///Dilates <i>this</i> image using a 3x3 rectangular structuring element.
      ///Dilation are applied serveral (iterations) times
      ///</summary>
      ///<returns> The dialated image</returns>
      [ExposableMethod(Exposable = true, Category = "Morphological Operations")]
      public Image<TColor, TDepth> Dilate(int iterations)
      {
         Image<TColor, TDepth> res = CopyBlank();
         CvInvoke.cvDilate(Ptr, res.Ptr, IntPtr.Zero, iterations);
         return res;
      }

      /// <summary>
      /// Perform inplace advanced morphological transformations using erosion and dilation as basic operations.
      /// </summary>
      /// <param name="element">Structuring element</param>
      /// <param name="operation">Type of morphological operation</param>
      /// <param name="iterations">Number of times erosion and dilation are applied</param>
      public void _MorphologyEx(StructuringElementEx element, CvEnum.CV_MORPH_OP operation, int iterations)
      {
         Image<TColor, TDepth> temp = null;
         if (operation == Emgu.CV.CvEnum.CV_MORPH_OP.CV_MOP_GRADIENT
            || operation == Emgu.CV.CvEnum.CV_MORPH_OP.CV_MOP_TOPHAT
            || operation == Emgu.CV.CvEnum.CV_MORPH_OP.CV_MOP_BLACKHAT)
            temp = CopyBlank();

         CvInvoke.cvMorphologyEx(
            Ptr,
            Ptr,
            temp == null ? IntPtr.Zero : temp.Ptr,
            element.Ptr,
            operation,
            iterations);

         if (temp != null) temp.Dispose();
      }

      ///<summary>
      ///Erodes <i>this</i> image inplace using a 3x3 rectangular structuring element.
      ///Erosion are applied serveral (iterations) times
      ///</summary>
      public void _Erode(int iterations)
      {
         CvInvoke.cvErode(Ptr, Ptr, IntPtr.Zero, iterations);
      }

      ///<summary>
      ///Dilates <i>this</i> image inplace using a 3x3 rectangular structuring element.
      ///Dilation are applied serveral (iterations) times
      ///</summary>
      public void _Dilate(int iterations)
      {
         CvInvoke.cvDilate(Ptr, Ptr, IntPtr.Zero, iterations);
      }
      #endregion

      #region generic operations
      ///<summary> perform an generic action based on each element of the Image</summary>
      public void Action(System.Action<TDepth> action)
      {
         int cols1 = Width * new TColor().Dimension;

         int step1;
         IntPtr start;
         System.Drawing.Size roiSize;
         CvInvoke.cvGetRawData(Ptr, out start, out step1, out roiSize);
         Int64 data1 = start.ToInt64();
         int width1 = _sizeOfElement * cols1;

         using (PinnedArray<TDepth> row1 = new PinnedArray<TDepth>(cols1))
            for (int row = 0; row < Height; row++, data1 += step1)
            {
               Emgu.Util.Toolbox.memcpy(row1.AddrOfPinnedObject(), new IntPtr(data1), width1);
               System.Array.ForEach(row1.Array, action);
            }
      }

      /// <summary>
      /// perform an generic operation based on the elements of the two images
      /// </summary>
      /// <typeparam name="TOtherDepth">The depth of the second image</typeparam>
      /// <param name="img2">The second image to perform action on</param>
      /// <param name="action">An action such that the first parameter is the a single channel of a pixel from the first image, the second parameter is the corresponding channel of the correspondind pixel from the second image </param>
      public void Action<TOtherDepth>(Image<TColor, TOtherDepth> img2, Emgu.Util.Toolbox.Action<TDepth, TOtherDepth> action)
      {
         Debug.Assert(EqualSize(img2));

         Int64 data1;
         int height1, cols1, width1, step1;
         RoiParam(Ptr, out data1, out height1, out cols1, out width1, out step1);

         Int64 data2;
         int height2, cols2, width2, step2;
         RoiParam(img2.Ptr, out data2, out height2, out cols2, out width2, out step2);

         TDepth[] row1 = new TDepth[cols1];
         TOtherDepth[] row2 = new TOtherDepth[cols1];
         GCHandle handle1 = GCHandle.Alloc(row1, GCHandleType.Pinned);
         GCHandle handle2 = GCHandle.Alloc(row2, GCHandleType.Pinned);

         for (int row = 0; row < height1; row++, data1 += step1, data2 += step2)
         {
            Emgu.Util.Toolbox.memcpy(handle1.AddrOfPinnedObject(), (IntPtr)data1, width1);
            Emgu.Util.Toolbox.memcpy(handle2.AddrOfPinnedObject(), (IntPtr)data2, width2);
            for (int col = 0; col < cols1; action(row1[col], row2[col]), col++) ;
         }
         handle1.Free();
         handle2.Free();
      }

      ///<summary> Compute the element of a new image based on the value as well as the x and y positions of each pixel on the image</summary> 
      public Image<TColor, TOtherDepth> Convert<TOtherDepth>(Emgu.Util.Toolbox.Func<TDepth, int, int, TOtherDepth> converter)
      {
         Image<TColor, TOtherDepth> res = new Image<TColor, TOtherDepth>(Width, Height);

         int nchannel = MIplImage.nChannels;

         Int64 data1;
         int height1, cols1, width1, step1;
         RoiParam(Ptr, out data1, out height1, out cols1, out width1, out step1);

         Int64 data2;
         int height2, cols2, width2, step2;
         RoiParam(res.Ptr, out data2, out height2, out cols2, out width2, out step2);

         TDepth[] row1 = new TDepth[cols1];
         TOtherDepth[] row2 = new TOtherDepth[cols1];
         GCHandle handle1 = GCHandle.Alloc(row1, GCHandleType.Pinned);
         GCHandle handle2 = GCHandle.Alloc(row2, GCHandleType.Pinned);

         for (int row = 0; row < height1; row++, data1 += step1, data2 += step2)
         {
            Emgu.Util.Toolbox.memcpy(handle1.AddrOfPinnedObject(), (IntPtr)data1, width1);
            for (int col = 0; col < cols1; row2[col] = converter(row1[col], row, col / nchannel), col++) ;
            Emgu.Util.Toolbox.memcpy((IntPtr)data2, handle2.AddrOfPinnedObject(), width2);
         }
         handle1.Free();
         handle2.Free();
         return res;
      }

      ///<summary> Compute the element of the new image based on element of this image</summary> 
      public Image<TColor, TOtherDepth> Convert<TOtherDepth>(System.Converter<TDepth, TOtherDepth> converter)
      {
         Image<TColor, TOtherDepth> res = new Image<TColor, TOtherDepth>(Width, Height);

         Int64 data1;
         int height1, cols1, width1, step1;
         RoiParam(Ptr, out data1, out height1, out cols1, out width1, out step1);

         Int64 data2;
         int height2, cols2, width2, step2;
         RoiParam(res.Ptr, out data2, out height2, out cols2, out width2, out step2);

         TDepth[] row1 = new TDepth[cols1];
         TOtherDepth[] row2 = new TOtherDepth[cols1];

         GCHandle handle1 = GCHandle.Alloc(row1, GCHandleType.Pinned);
         GCHandle handle2 = GCHandle.Alloc(row2, GCHandleType.Pinned);
         for (int row = 0; row < height1; row++, data1 += step1, data2 += step2)
         {
            Emgu.Util.Toolbox.memcpy(handle1.AddrOfPinnedObject(), (IntPtr)data1, width1);
            for (int col = 0; col < cols1; row2[col] = converter(row1[col]), col++) ;
            Emgu.Util.Toolbox.memcpy((IntPtr)data2, handle2.AddrOfPinnedObject(), width2);
         }
         handle1.Free();
         handle2.Free();
         return res;
      }

      ///<summary> Compute the element of the new image based on the elements of the two image</summary>
      public Image<TColor, TDepth3> Convert<TDepth2, TDepth3>(Image<TColor, TDepth2> img2, Emgu.Util.Toolbox.Func<TDepth, TDepth2, TDepth3> converter)
      {
         Debug.Assert(EqualSize(img2), "Image size do not match");

         Image<TColor, TDepth3> res = new Image<TColor, TDepth3>(Width, Height);

         Int64 data1;
         int height1, cols1, width1, step1;
         RoiParam(Ptr, out data1, out height1, out cols1, out width1, out step1);

         Int64 data2;
         int height2, cols2, width2, step2;
         RoiParam(img2.Ptr, out data2, out height2, out cols2, out width2, out step2);

         Int64 data3;
         int height3, cols3, width3, step3;
         RoiParam(res.Ptr, out data3, out height3, out cols3, out width3, out step3);

         TDepth[] row1 = new TDepth[cols1];
         TDepth2[] row2 = new TDepth2[cols1];
         TDepth3[] row3 = new TDepth3[cols1];
         GCHandle handle1 = GCHandle.Alloc(row1, GCHandleType.Pinned);
         GCHandle handle2 = GCHandle.Alloc(row2, GCHandleType.Pinned);
         GCHandle handle3 = GCHandle.Alloc(row3, GCHandleType.Pinned);

         for (int row = 0; row < height1; row++, data1 += step1, data2 += step2, data3 += step3)
         {
            Emgu.Util.Toolbox.memcpy(handle1.AddrOfPinnedObject(), (IntPtr)data1, width1);
            Emgu.Util.Toolbox.memcpy(handle2.AddrOfPinnedObject(), (IntPtr)data2, width2);
            for (int col = 0; col < cols1; row3[col] = converter(row1[col], row2[col]), col++) ;
            Emgu.Util.Toolbox.memcpy((IntPtr)data3, handle3.AddrOfPinnedObject(), width3);
         }

         handle1.Free();
         handle2.Free();
         handle3.Free();

         return res;
      }

      ///<summary> Compute the element of the new image based on the elements of the three image</summary>
      public Image<TColor, TDepth4> Convert<TDepth2, TDepth3, TDepth4>(Image<TColor, TDepth2> img2, Image<TColor, TDepth3> img3, Emgu.Util.Toolbox.Func<TDepth, TDepth2, TDepth3, TDepth4> converter)
      {
         Debug.Assert(EqualSize(img2) && EqualSize(img3), "Image size do not match");

         Image<TColor, TDepth4> res = new Image<TColor, TDepth4>(Width, Height);

         Int64 data1;
         int height1, cols1, width1, step1;
         RoiParam(Ptr, out data1, out height1, out cols1, out width1, out step1);

         Int64 data2;
         int height2, cols2, width2, step2;
         RoiParam(img2.Ptr, out data2, out height2, out cols2, out width2, out step2);

         Int64 data3;
         int height3, cols3, width3, step3;
         RoiParam(img3.Ptr, out data3, out height3, out cols3, out width3, out step3);

         Int64 data4;
         int height4, cols4, width4, step4;
         RoiParam(res.Ptr, out data4, out height4, out cols4, out width4, out step4);

         TDepth[] row1 = new TDepth[cols1];
         TDepth2[] row2 = new TDepth2[cols1];
         TDepth3[] row3 = new TDepth3[cols1];
         TDepth4[] row4 = new TDepth4[cols1];
         GCHandle handle1 = GCHandle.Alloc(row1, GCHandleType.Pinned);
         GCHandle handle2 = GCHandle.Alloc(row2, GCHandleType.Pinned);
         GCHandle handle3 = GCHandle.Alloc(row3, GCHandleType.Pinned);
         GCHandle handle4 = GCHandle.Alloc(row4, GCHandleType.Pinned);

         for (int row = 0; row < height1; row++, data1 += step1, data2 += step2, data3 += step3, data4 += step4)
         {
            Emgu.Util.Toolbox.memcpy(handle1.AddrOfPinnedObject(), (IntPtr)data1, width1);
            Emgu.Util.Toolbox.memcpy(handle2.AddrOfPinnedObject(), (IntPtr)data2, width2);
            Emgu.Util.Toolbox.memcpy(handle3.AddrOfPinnedObject(), (IntPtr)data3, width3);

            for (int col = 0; col < cols1; row4[col] = converter(row1[col], row2[col], row3[col]), col++) ;

            Emgu.Util.Toolbox.memcpy((IntPtr)data4, handle4.AddrOfPinnedObject(), width4);
         }
         handle1.Free();
         handle2.Free();
         handle3.Free();
         handle4.Free();

         return res;
      }

      ///<summary> Compute the element of the new image based on the elements of the four image</summary>
      public Image<TColor, TDepth5> Convert<TDepth2, TDepth3, TDepth4, TDepth5>(Image<TColor, TDepth2> img2, Image<TColor, TDepth3> img3, Image<TColor, TDepth4> img4, Emgu.Util.Toolbox.Func<TDepth, TDepth2, TDepth3, TDepth4, TDepth5> converter)
      {
         Debug.Assert(EqualSize(img2) && EqualSize(img3) && EqualSize(img4), "Image size do not match");

         Image<TColor, TDepth5> res = new Image<TColor, TDepth5>(Width, Height);

         Int64 data1;
         int height1, cols1, width1, step1;
         RoiParam(Ptr, out data1, out height1, out cols1, out width1, out step1);

         Int64 data2;
         int height2, cols2, width2, step2;
         RoiParam(img2.Ptr, out data2, out height2, out cols2, out width2, out step2);

         Int64 data3;
         int height3, cols3, width3, step3;
         RoiParam(img3.Ptr, out data3, out height3, out cols3, out width3, out step3);

         Int64 data4;
         int height4, cols4, width4, step4;
         RoiParam(img4.Ptr, out data4, out height4, out cols4, out width4, out step4);

         Int64 data5;
         int height5, cols5, width5, step5;
         RoiParam(res.Ptr, out data5, out height5, out cols5, out width5, out step5);

         TDepth[] row1 = new TDepth[cols1];
         TDepth2[] row2 = new TDepth2[cols1];
         TDepth3[] row3 = new TDepth3[cols1];
         TDepth4[] row4 = new TDepth4[cols1];
         TDepth5[] row5 = new TDepth5[cols1];
         GCHandle handle1 = GCHandle.Alloc(row1, GCHandleType.Pinned);
         GCHandle handle2 = GCHandle.Alloc(row2, GCHandleType.Pinned);
         GCHandle handle3 = GCHandle.Alloc(row3, GCHandleType.Pinned);
         GCHandle handle4 = GCHandle.Alloc(row4, GCHandleType.Pinned);
         GCHandle handle5 = GCHandle.Alloc(row5, GCHandleType.Pinned);

         for (int row = 0; row < height1; row++, data1 += step1, data2 += step2, data3 += step3, data4 += step4, data5 += step5)
         {
            Emgu.Util.Toolbox.memcpy(handle1.AddrOfPinnedObject(), (IntPtr)data1, width1);
            Emgu.Util.Toolbox.memcpy(handle2.AddrOfPinnedObject(), (IntPtr)data2, width2);
            Emgu.Util.Toolbox.memcpy(handle3.AddrOfPinnedObject(), (IntPtr)data3, width3);
            Emgu.Util.Toolbox.memcpy(handle4.AddrOfPinnedObject(), (IntPtr)data4, width4);

            for (int col = 0; col < cols1; row5[col] = converter(row1[col], row2[col], row3[col], row4[col]), col++) ;
            Emgu.Util.Toolbox.memcpy((IntPtr)data5, handle5.AddrOfPinnedObject(), width5);
         }
         handle1.Free();
         handle2.Free();
         handle3.Free();
         handle4.Free();
         handle5.Free();

         return res;
      }
      #endregion

      #region Implment UnmanagedObject
      /// <summary>
      /// Release all unmanaged memory associate with the image
      /// </summary>
      protected override void DisposeObject()
      {
         if (_ptr != IntPtr.Zero)
         {
            CvInvoke.cvReleaseImageHeader(ref _ptr);
            _ptr = IntPtr.Zero;
            GC.RemoveMemoryPressure(StructSize.MIplImage);
         }

         base.DisposeObject();
      }
      #endregion

      #region Operator overload

      /// <summary>
      /// Perform an elementwise AND operation on the two images
      /// </summary>
      /// <param name="img1">The first image to AND</param>
      /// <param name="img2">The second image to AND</param>
      /// <returns>The result of the AND operation</returns>
      public static Image<TColor, TDepth> operator &(Image<TColor, TDepth> img1, Image<TColor, TDepth> img2)
      {
         return img1.And(img2);
      }

      /// <summary>
      /// Perform an elementwise AND operation using an images and a color
      /// </summary>
      /// <param name="img1">The first image to AND</param>
      /// <param name="val">The color to AND</param>
      /// <returns>The result of the AND operation</returns>
      public static Image<TColor, TDepth> operator &(Image<TColor, TDepth> img1, double val)
      {
         TColor color = new TColor();
         color.MCvScalar = new MCvScalar(val, val, val, val);
         return img1.And(color);
      }

      /// <summary>
      /// Perform an elementwise AND operation using an images and a color
      /// </summary>
      /// <param name="img1">The first image to AND</param>
      /// <param name="val">The color to AND</param>
      /// <returns>The result of the AND operation</returns>
      public static Image<TColor, TDepth> operator &(double val, Image<TColor, TDepth> img1)
      {
         TColor color = new TColor();
         color.MCvScalar = new MCvScalar(val, val, val, val);
         return img1.And(color);
      }

      /// <summary>
      /// Perform an elementwise AND operation using an images and a color
      /// </summary>
      /// <param name="img1">The first image to AND</param>
      /// <param name="val">The color to AND</param>
      /// <returns>The result of the AND operation</returns>
      public static Image<TColor, TDepth> operator &(Image<TColor, TDepth> img1, TColor val)
      {
         return img1.And(val);
      }

      /// <summary>
      /// Perform an elementwise AND operation using an images and a color
      /// </summary>
      /// <param name="img1">The first image to AND</param>
      /// <param name="val">The color to AND</param>
      /// <returns>The result of the AND operation</returns>
      public static Image<TColor, TDepth> operator &(TColor val, Image<TColor, TDepth> img1)
      {
         return img1.And(val);
      }

      ///<summary> Perform an elementwise OR operation with another image and return the result</summary>
      ///<returns> The result of the OR operation</returns>
      public static Image<TColor, TDepth> operator |(Image<TColor, TDepth> img1, Image<TColor, TDepth> img2)
      {
         return img1.Or(img2);
      }

      ///<summary> 
      /// Perform an binary OR operation with some color
      /// </summary>
      ///<param name="img1">The image to OR</param>
      ///<param name="val"> The color to OR</param>
      ///<returns> The result of the OR operation</returns>
      public static Image<TColor, TDepth> operator |(Image<TColor, TDepth> img1, double val)
      {
         TColor color = new TColor();
         color.MCvScalar = new MCvScalar(val, val, val, val);
         return img1.Or(color);
      }

      ///<summary> 
      /// Perform an binary OR operation with some color
      /// </summary>
      ///<param name="img1">The image to OR</param>
      ///<param name="val"> The color to OR</param>
      ///<returns> The result of the OR operation</returns>
      public static Image<TColor, TDepth> operator |(double val, Image<TColor, TDepth> img1)
      {
         return img1 | val;
      }

      ///<summary> 
      /// Perform an binary OR operation with some color
      /// </summary>
      ///<param name="img1">The image to OR</param>
      ///<param name="val"> The color to OR</param>
      ///<returns> The result of the OR operation</returns>
      public static Image<TColor, TDepth> operator |(Image<TColor, TDepth> img1, TColor val)
      {
         return img1.Or(val);
      }

      ///<summary> 
      /// Perform an binary OR operation with some color
      /// </summary>
      ///<param name="img1">The image to OR</param>
      ///<param name="val"> The color to OR</param>
      ///<returns> The result of the OR operation</returns>
      public static Image<TColor, TDepth> operator |(TColor val, Image<TColor, TDepth> img1)
      {
         return img1.Or(val);
      }

      ///<summary> Compute the complement image</summary>
      public static Image<TColor, TDepth> operator ~(Image<TColor, TDepth> img1)
      {
         return img1.Not();
      }

      /// <summary>
      /// Elementwise add <paramref name="img1"/> with <paramref name="img2"/>
      /// </summary>
      /// <param name="img1">The first image to be added</param>
      /// <param name="img2">The second image to be added</param>
      /// <returns>The sum of the two images</returns>
      public static Image<TColor, TDepth> operator +(Image<TColor, TDepth> img1, Image<TColor, TDepth> img2)
      {
         return img1.Add(img2);
      }

      /// <summary>
      /// Elementwise add <paramref name="img1"/> with <paramref name="val"/>
      /// </summary>
      /// <param name="img1">The image to be added</param>
      /// <param name="val">The value to be added</param>
      /// <returns>The images plus the color</returns>
      public static Image<TColor, TDepth> operator +(double val, Image<TColor, TDepth> img1)
      {
         return img1 + val;
      }

      /// <summary>
      /// Elementwise add <paramref name="img1"/> with <paramref name="val"/>
      /// </summary>
      /// <param name="img1">The image to be added</param>
      /// <param name="val">The value to be added</param>
      /// <returns>The images plus the color</returns>
      public static Image<TColor, TDepth> operator +(Image<TColor, TDepth> img1, double val)
      {
         TColor color = new TColor();
         color.MCvScalar = new MCvScalar(val, val, val, val);
         return img1.Add(color);
      }

      /// <summary>
      /// Elementwise add <paramref name="img1"/> with <paramref name="val"/>
      /// </summary>
      /// <param name="img1">The image to be added</param>
      /// <param name="val">The color to be added</param>
      /// <returns>The images plus the color</returns>
      public static Image<TColor, TDepth> operator +(Image<TColor, TDepth> img1, TColor val)
      {
         return img1.Add(val);
      }

      /// <summary>
      /// Elementwise add <paramref name="img1"/> with <paramref name="val"/>
      /// </summary>
      /// <param name="img1">The image to be added</param>
      /// <param name="val">The color to be added</param>
      /// <returns>The images plus the color</returns>
      public static Image<TColor, TDepth> operator +(TColor val, Image<TColor, TDepth> img1)
      {
         return img1.Add(val);
      }

      /// <summary>
      /// Elementwise subtract another image from the current image
      /// </summary>
      /// <param name="img1">The image to be substracted</param>
      /// <param name="img2">The second image to be subtraced from <paramref name="img1"/></param>
      /// <returns> The result of elementwise subtracting img2 from <paramref name="img1"/> </returns>
      public static Image<TColor, TDepth> operator -(Image<TColor, TDepth> img1, Image<TColor, TDepth> img2)
      {
         return img1.Sub(img2);
      }

      /// <summary>
      /// Elementwise subtract another image from the current image
      /// </summary>
      /// <param name="img1">The image to be substracted</param>
      /// <param name="val">The color to be subtracted</param>
      /// <returns> The result of elementwise subtracting <paramred name="val"/> from <paramref name="img1"/> </returns>
      public static Image<TColor, TDepth> operator -(Image<TColor, TDepth> img1, TColor val)
      {
         return img1.Sub(val);
      }

      /// <summary>
      /// Elementwise subtract another image from the current image
      /// </summary>
      /// <param name="img1">The image to be substracted</param>
      /// <param name="val">The color to be subtracted</param>
      /// <returns> <paramred name="val"/> - <paramref name="img1"/> </returns>
      public static Image<TColor, TDepth> operator -(TColor val, Image<TColor, TDepth> img1)
      {
         return img1.SubR(val);
      }

      /// <summary>
      /// <paramred name="val"/> - <paramref name="img1"/>
      /// </summary>
      /// <param name="img1">The image to be substracted</param>
      /// <param name="val">The value to be subtracted</param>
      /// <returns> <paramred name="val"/> - <paramref name="img1"/> </returns>
      public static Image<TColor, TDepth> operator -(double val, Image<TColor, TDepth> img1)
      {
         TColor color = new TColor();
         color.MCvScalar = new MCvScalar(val, val, val, val);
         return img1.SubR(color);
      }

      /// <summary>
      /// Elementwise subtract another image from the current image
      /// </summary>
      /// <param name="img1">The image to be substracted</param>
      /// <param name="val">The value to be subtracted</param>
      /// <returns> <paramref name="img1"/> - <paramred name="val"/>   </returns>
      public static Image<TColor, TDepth> operator -(Image<TColor, TDepth> img1, double val)
      {
         TColor color = new TColor();
         color.MCvScalar = new MCvScalar(val, val, val, val);
         return img1.Sub(color);
      }

      /// <summary>
      ///  <paramref name="img1"/> * <paramref name="scale"/>
      /// </summary>
      /// <param name="img1">The image</param>
      /// <param name="scale">The multiplication scale</param>
      /// <returns><paramref name="img1"/> * <paramref name="scale"/></returns>
      public static Image<TColor, TDepth> operator *(Image<TColor, TDepth> img1, double scale)
      {
         return img1.Mul(scale);
      }

      /// <summary>
      ///   <paramref name="scale"/>*<paramref name="img1"/>
      /// </summary>
      /// <param name="img1">The image</param>
      /// <param name="scale">The multiplication scale</param>
      /// <returns><paramref name="scale"/>*<paramref name="img1"/></returns>
      public static Image<TColor, TDepth> operator *(double scale, Image<TColor, TDepth> img1)
      {
         return img1.Mul(scale);
      }

      /// <summary>
      /// Perform the convolution with <paramref name="kernel"/> on <paramref name="img1"/>
      /// </summary>
      /// <param name="img1">The image</param>
      /// <param name="kernel">The kernel</param>
      /// <returns>Result of the convolution</returns>
      public static Image<TColor, Single> operator *(Image<TColor, TDepth> img1, ConvolutionKernelF kernel)
      {
         return img1.Convolution(kernel);
      }

      /// <summary>
      ///  <paramref name="img1"/> / <paramref name="scale"/>
      /// </summary>
      /// <param name="img1">The image</param>
      /// <param name="scale">The division scale</param>
      /// <returns><paramref name="img1"/> / <paramref name="scale"/></returns>
      public static Image<TColor, TDepth> operator /(Image<TColor, TDepth> img1, double scale)
      {
         return img1.Mul(1.0 / scale);
      }

      /// <summary>
      ///   <paramref name="scale"/> / <paramref name="img1"/>
      /// </summary>
      /// <param name="img1">The image</param>
      /// <param name="scale">The scale</param>
      /// <returns><paramref name="scale"/> / <paramref name="img1"/></returns>
      public static Image<TColor, TDepth> operator /(double scale, Image<TColor, TDepth> img1)
      {
         Image<TColor, TDepth> res = img1.CopyBlank();
         CvInvoke.cvDiv(IntPtr.Zero, img1.Ptr, res.Ptr, scale);
         return res;
      }

      #endregion

      #region Filters
      /// <summary>
      /// Summation over a pixel param1 x param2 neighborhood with subsequent scaling by 1/(param1 x param2)
      /// </summary>
      /// <param name="width">The width of the window</param>
      /// <param name="height">The height of the window</param>
      /// <returns>The result of blur</returns>
      public Image<TColor, TDepth> SmoothBlur(int width, int height)
      {
         return SmoothBlur(width, height, true);
      }

      /// <summary>
      /// Summation over a pixel param1 x param2 neighborhood. If scale is true, the result is subsequent scaled by 1/(param1 x param2)
      /// </summary>
      /// <param name="width">The width of the window</param>
      /// <param name="height">The height of the window</param>
      /// <param name="scale">If true, the result is subsequent scaled by 1/(param1 x param2)</param>
      /// <returns>The result of blur</returns>
      public Image<TColor, TDepth> SmoothBlur(int width, int height, bool scale)
      {
         Emgu.CV.CvEnum.SMOOTH_TYPE type = scale ? Emgu.CV.CvEnum.SMOOTH_TYPE.CV_BLUR : Emgu.CV.CvEnum.SMOOTH_TYPE.CV_BLUR_NO_SCALE;
         Image<TColor, TDepth> res = CopyBlank();
         CvInvoke.cvSmooth(Ptr, res.Ptr, type, width, height, 0.0, 0.0);
         return res;
      }

      /// <summary>
      /// Finding median of <paramref name="size"/>x<paramref name="size"/> neighborhood 
      /// </summary>
      /// <param name="size">The size (width &amp; height) of the window</param>
      /// <returns>The result of mediam smooth</returns>
      public Image<TColor, TDepth> SmoothMedian(int size)
      {
         Image<TColor, TDepth> res = CopyBlank();
         CvInvoke.cvSmooth(Ptr, res.Ptr, Emgu.CV.CvEnum.SMOOTH_TYPE.CV_MEDIAN, size, size, 0, 0);
         return res;
      }

      /// <summary>
      /// Applying bilateral 3x3 filtering
      /// </summary>
      /// <param name="colorSigma">Color sigma</param>
      /// <param name="spaceSigma">Space sigma</param>
      /// <returns>The result of bilateral smooth</returns>
      public Image<TColor, TDepth> SmoothBilatral(int colorSigma, int spaceSigma)
      {
         Image<TColor, TDepth> res = CopyBlank();
         CvInvoke.cvSmooth(Ptr, res.Ptr, Emgu.CV.CvEnum.SMOOTH_TYPE.CV_BILATERAL, colorSigma, spaceSigma, 0, 0);
         return res;
      }

      #region Gaussian Smooth
      ///<summary> Perform Gaussian Smoothing in the current image and return the result </summary>
      ///<param name="kernelSize"> The size of the Gaussian kernel (<paramref>kernelSize</paramref> x <paramref>kernelSize</paramref>)</param>
      ///<returns> The smoothed image</returns>
      public Image<TColor, TDepth> SmoothGaussian(int kernelSize)
      {
         return SmoothGaussian(kernelSize, 0, 0, 0);
      }

      ///<summary> Perform Gaussian Smoothing in the current image and return the result </summary>
      ///<param name="kernelWidth"> The width of the Gaussian kernel</param>
      ///<param name="kernelHeight"> The height of the Gaussian kernel</param>
      ///<param name="sigma1"> The standard deviation of the Gaussian kernel in the horizontal dimwnsion</param>
      ///<param name="sigma2"> The standard deviation of the Gaussian kernel in the vertical dimwnsion</param>
      ///<returns> The smoothed image</returns>
      public Image<TColor, TDepth> SmoothGaussian(int kernelWidth, int kernelHeight, double sigma1, double sigma2)
      {
         Image<TColor, TDepth> res = CopyBlank();
         CvInvoke.cvSmooth(Ptr, res.Ptr, CvEnum.SMOOTH_TYPE.CV_GAUSSIAN, kernelWidth, kernelHeight, sigma1, sigma2);
         return res;
      }

      ///<summary> Perform Gaussian Smoothing inplace for the current image </summary>
      ///<param name="kernelSize"> The size of the Gaussian kernel (<paramref>kernelSize</paramref> x <paramref>kernelSize</paramref>)</param>
      public void _SmoothGaussian(int kernelSize)
      {
         _SmoothGaussian(kernelSize, 0, 0, 0);
      }

      ///<summary> Perform Gaussian Smoothing inplace for the current image </summary>
      ///<param name="kernelWidth"> The width of the Gaussian kernel</param>
      ///<param name="kernelHeight"> The height of the Gaussian kernel</param>
      ///<param name="sigma1"> The standard deviation of the Gaussian kernel in the horizontal dimwnsion</param>
      ///<param name="sigma2"> The standard deviation of the Gaussian kernel in the vertical dimwnsion</param>
      public void _SmoothGaussian(int kernelWidth, int kernelHeight, double sigma1, double sigma2)
      {
         CvInvoke.cvSmooth(Ptr, Ptr, CvEnum.SMOOTH_TYPE.CV_GAUSSIAN, kernelWidth, kernelHeight, sigma1, sigma2);
      }

      ///<summary> 
      ///performs a convolution using the provided kernel 
      ///</summary>
      ///<param name="kernel"> The convolution kernel</param>
      public Image<TColor, Single> Convolution(ConvolutionKernelF kernel)
      {
         bool isFloat = (typeof(TDepth) == typeof(Single));

         Emgu.Util.Toolbox.Action<IntPtr, IntPtr, int> act =
             delegate(IntPtr src, IntPtr dest, int channel)
             {
                IntPtr srcFloat = src;
                if (!isFloat)
                {
                   srcFloat = CvInvoke.cvCreateImage(new System.Drawing.Size(Width, Height), CvEnum.IPL_DEPTH.IPL_DEPTH_32F, 1);
                   CvInvoke.cvConvertScale(src, srcFloat, 1.0, 0.0);
                }

                //perform the convolution operation
                CvInvoke.cvFilter2D(
                    srcFloat,
                    dest,
                    kernel.Ptr,
                    kernel.Center);

                if (!isFloat)
                {
                   CvInvoke.cvReleaseImage(ref srcFloat);
                }
             };

         Image<TColor, Single> res = new Image<TColor, Single>(Width, Height);
         ForEachDuplicateChannel(act, res);

         return res;
      }

      /// <summary>
      /// Calculates integral images for the source image
      /// </summary>
      /// <param name="sum">The integral image</param>
      public void Integral(out Image<TColor, double> sum)
      {
         sum = new Image<TColor, double>(Width + 1, Height + 1);
         CvInvoke.cvIntegral(Ptr, sum.Ptr, IntPtr.Zero, IntPtr.Zero);
      }

      /// <summary>
      /// Calculates integral images for the source image
      /// </summary>
      /// <param name="sum">The integral image</param>
      /// <param name="squareSum">The integral image for squared pixel values</param>
      public void Integral(out Image<TColor, double> sum, out Image<TColor, double> squareSum)
      {
         sum = new Image<TColor, double>(Width + 1, Height + 1);
         squareSum = new Image<TColor, double>(Width + 1, Height + 1);
         CvInvoke.cvIntegral(Ptr, sum.Ptr, squareSum.Ptr, IntPtr.Zero);
      }

      /// <summary>
      /// calculates one or more integral images for the source image
      /// </summary>
      /// <param name="sum">The integral image</param>
      /// <param name="squareSum">The integral image for squared pixel values</param>
      /// <param name="titledSum">The integral for the image rotated by 45 degrees</param>
      public void Integral(out Image<TColor, double> sum, out Image<TColor, double> squareSum, out Image<TColor, double> titledSum)
      {
         sum = new Image<TColor, double>(Width + 1, Height + 1);
         squareSum = new Image<TColor, double>(Width + 1, Height + 1);
         titledSum = new Image<TColor, double>(Width + 1, Height + 1);
         CvInvoke.cvIntegral(Ptr, sum.Ptr, squareSum.Ptr, titledSum.Ptr);
      }
      #endregion

      #region Threshold methods
      ///<summary> 
      ///the base threshold method shared by public threshold functions 
      ///</summary>
      private void ThresholdBase(Image<TColor, TDepth> dest, TColor threshold, TColor max_value, CvEnum.THRESH thresh_type)
      {
         double[] t = threshold.MCvScalar.ToArray();
         double[] m = max_value.MCvScalar.ToArray();
         Emgu.Util.Toolbox.Action<IntPtr, IntPtr, int> act =
             delegate(IntPtr src, IntPtr dst, int channel)
             {
                CvInvoke.cvThreshold(src, dst, t[channel], m[channel], thresh_type);
             };
         ForEachDuplicateChannel<TDepth>(act, dest);
      }

      ///<summary> Threshold the image such that: dst(x,y) = src(x,y), if src(x,y)>threshold;  0, otherwise </summary>
      ///<returns> dst(x,y) = src(x,y), if src(x,y)>threshold;  0, otherwise </returns>
      [ExposableMethod(Exposable = true, Category = "Threshold")]
      public Image<TColor, TDepth> ThresholdToZero(TColor threshold)
      {
         Image<TColor, TDepth> res = CopyBlank();
         ThresholdBase(res, threshold, new TColor(), CvEnum.THRESH.CV_THRESH_TOZERO);
         return res;
      }

      /// <summary> 
      /// Threshold the image such that: dst(x,y) = 0, if src(x,y)>threshold;  src(x,y), otherwise 
      /// </summary>
      /// <param name="threshold">The threshold to apply</param>
      /// <returns>The image such that: dst(x,y) = 0, if src(x,y)>threshold;  src(x,y), otherwise</returns>
      [ExposableMethod(Exposable = true, Category = "Threshold")]
      public Image<TColor, TDepth> ThresholdToZeroInv(TColor threshold)
      {
         Image<TColor, TDepth> res = CopyBlank();
         ThresholdBase(res, threshold, new TColor(), CvEnum.THRESH.CV_THRESH_TOZERO_INV);
         return res;
      }

      /// <summary>
      /// Threshold the image such that: dst(x,y) = threshold, if src(x,y)>threshold; src(x,y), otherwise 
      /// </summary>
      /// <param name="threshold">The threshold to apply</param>
      /// <returns>The image such that: dst(x,y) = threshold, if src(x,y)>threshold; src(x,y), otherwise</returns>
      [ExposableMethod(Exposable = true, Category = "Threshold")]
      public Image<TColor, TDepth> ThresholdTrunc(TColor threshold)
      {
         Image<TColor, TDepth> res = CopyBlank();
         ThresholdBase(res, threshold, new TColor(), CvEnum.THRESH.CV_THRESH_TRUNC);
         return res;
      }

      /// <summary> 
      /// Threshold the image such that: dst(x,y) = max_value, if src(x,y)>threshold; 0, otherwise 
      /// </summary>
      [ExposableMethod(Exposable = true, Category = "Threshold")]
      public Image<TColor, TDepth> ThresholdBinary(TColor threshold, TColor maxValue)
      {
         Image<TColor, TDepth> res = CopyBlank();
         ThresholdBase(res, threshold, maxValue, CvEnum.THRESH.CV_THRESH_BINARY);
         return res;
      }

      ///<summary> Threshold the image such that: dst(x,y) = 0, if src(x,y)>threshold;  max_value, otherwise </summary>
      [ExposableMethod(Exposable = true, Category = "Threshold")]
      public Image<TColor, TDepth> ThresholdBinaryInv(TColor threshold, TColor maxValue)
      {
         Image<TColor, TDepth> res = CopyBlank();
         ThresholdBase(res, threshold, maxValue, CvEnum.THRESH.CV_THRESH_BINARY_INV);
         return res;
      }

      ///<summary> Threshold the image inplace such that: dst(x,y) = src(x,y), if src(x,y)>threshold;  0, otherwise </summary>
      public void _ThresholdToZero(TColor threshold)
      {
         ThresholdBase(this, threshold, new TColor(), CvEnum.THRESH.CV_THRESH_TOZERO);
      }

      ///<summary> Threshold the image inplace such that: dst(x,y) = 0, if src(x,y)>threshold;  src(x,y), otherwise </summary>
      public void _ThresholdToZeroInv(TColor threshold)
      {
         ThresholdBase(this, threshold, new TColor(), CvEnum.THRESH.CV_THRESH_TOZERO_INV);
      }
      ///<summary> Threshold the image inplace such that: dst(x,y) = threshold, if src(x,y)>threshold; src(x,y), otherwise </summary>
      public void _ThresholdTrunc(TColor threshold)
      {
         ThresholdBase(this, threshold, new TColor(), CvEnum.THRESH.CV_THRESH_TRUNC);
      }
      ///<summary> Threshold the image inplace such that: dst(x,y) = max_value, if src(x,y)>threshold; 0, otherwise </summary>
      public void _ThresholdBinary(TColor threshold, TColor max_value)
      {
         ThresholdBase(this, threshold, max_value, CvEnum.THRESH.CV_THRESH_BINARY);
      }
      ///<summary> Threshold the image inplace such that: dst(x,y) = 0, if src(x,y)>threshold;  max_value, otherwise </summary>
      public void _ThresholdBinaryInv(TColor threshold, TColor max_value)
      {
         ThresholdBase(this, threshold, max_value, CvEnum.THRESH.CV_THRESH_BINARY_INV);
      }
      #endregion
      #endregion

      #region Statistic
      /// <summary>
      /// Calculates the average value and standard deviation of array elements, independently for each channel
      /// </summary>
      /// <param name="avg">The avg color</param>
      /// <param name="sdv">The standard deviation for each channel</param>
      /// <param name="mask">The operation mask</param>
      public void AvgSdv(out TColor avg, out MCvScalar sdv, Image<Gray, Byte> mask)
      {
         avg = new TColor();
         MCvScalar avgScalar = new MCvScalar();
         sdv = new MCvScalar();

         CvInvoke.cvAvgSdv(Ptr, ref avgScalar, ref sdv, mask.Ptr);
         avg.MCvScalar = avgScalar;
      }

      /// <summary>
      /// Calculates the average value and standard deviation of array elements, independently for each channel
      /// </summary>
      /// <param name="avg">The avg color</param>
      /// <param name="sdv">The standard deviation for each channel</param>
      public void AvgSdv(out TColor avg, out MCvScalar sdv)
      {
         avg = new TColor();
         MCvScalar avgScalar = new MCvScalar();
         sdv = new MCvScalar();

         CvInvoke.cvAvgSdv(Ptr, ref avgScalar, ref sdv, IntPtr.Zero);
         avg.MCvScalar = avgScalar;
      }

      /// <summary>
      /// Count the non Zero elements for each channel
      /// </summary>
      /// <returns>Count the non Zero elements for each channel</returns>
      public int[] CountNonzero()
      {
         return
             ForEachChannel<int>(delegate(IntPtr channel, int channelNumber)
             {
                return CvInvoke.cvCountNonZero(channel);
             });
      }

      /// <summary>
      /// Returns the min / max location and values for the image
      /// </summary>
      /// <returns>
      /// Returns the min / max location and values for the image
      /// </returns>
      public void MinMax(out double[] minValues, out double[] maxValues, out System.Drawing.Point[] minLocations, out System.Drawing.Point[] maxLocations)
      {
         minValues = new double[NumberOfChannels];
         maxValues = new double[NumberOfChannels];
         minLocations = new System.Drawing.Point[NumberOfChannels];
         maxLocations = new System.Drawing.Point[NumberOfChannels];

         if (NumberOfChannels == 1)
         {
            CvInvoke.cvMinMaxLoc(Ptr, ref minValues[0], ref maxValues[0], ref minLocations[0], ref maxLocations[0], IntPtr.Zero);
         }
         else
         {
            for (int i = 0; i < NumberOfChannels; i++)
            {
               CvInvoke.cvSetImageCOI(Ptr, i + 1);
               CvInvoke.cvMinMaxLoc(Ptr, ref minValues[i], ref maxValues[i], ref minLocations[i], ref maxLocations[i], IntPtr.Zero);
            }
            CvInvoke.cvSetImageCOI(Ptr, 0);
         }
      }
      #endregion

      #region various
      ///<summary> Return a filpped copy of the current image</summary>
      ///<param name="flipType">The type of the flipping</param>
      ///<returns> The flipped copy of <i>this</i> image </returns>
      public Image<TColor, TDepth> Flip(CvEnum.FLIP flipType)
      {
         if (flipType == Emgu.CV.CvEnum.FLIP.NONE) return Copy();

         int code =
            //-1 indicates vertical and horizontal flip
             flipType == (Emgu.CV.CvEnum.FLIP.HORIZONTAL | Emgu.CV.CvEnum.FLIP.VERTICAL) ? -1 :
            //1 indicates horizontal flip only
             flipType == Emgu.CV.CvEnum.FLIP.HORIZONTAL ? 1 :
            //0 indicates vertical flip only
             0;

         Image<TColor, TDepth> res = CopyBlank();
         CvInvoke.cvFlip(Ptr, res.Ptr, code);
         return res;
      }

      ///<summary> Inplace flip the image</summary>
      ///<param name="flipType">The type of the flipping</param>
      ///<returns> The flipped copy of <i>this</i> image </returns>
      [ExposableMethod(Exposable = true)]
      public void _Flip(CvEnum.FLIP flipType)
      {
         if (flipType != Emgu.CV.CvEnum.FLIP.NONE)
         {
            int code =
               //-1 indicates vertical and horizontal flip
                flipType == (Emgu.CV.CvEnum.FLIP.HORIZONTAL | Emgu.CV.CvEnum.FLIP.VERTICAL) ? -1 :
               //1 indicates horizontal flip only
                flipType == Emgu.CV.CvEnum.FLIP.HORIZONTAL ? 1 :
               //0 indicates vertical flip only
                0;
            CvInvoke.cvFlip(
               Ptr,
               Ptr,
               code);
         }
      }

      /// <summary>
      /// Calculates spatial and central moments up to the third order and writes them to moments. The moments may be used then to calculate gravity center of the shape, its area, main axises and various shape characeteristics including 7 Hu invariants.
      /// </summary>
      /// <param name="binary">If the flag is true, all the zero pixel values are treated as zeroes, all the others are treated as 1's</param>
      /// <returns>spatial and central moments up to the third order</returns>
      public MCvMoments GetMoments(bool binary)
      {
         MCvMoments m = new MCvMoments();
         CvInvoke.cvMoments(Ptr, ref m, binary ? 1 : 0);
         return m;
      }

      ///<summary> 
      ///Split current Image into an array of gray scale images where each element 
      ///in the array represent a single color channel of the original image
      ///</summary>
      ///<returns> 
      ///An array of gray scale images where each element 
      ///in the array represent a single color channel of the original image 
      ///</returns>
      public Image<Gray, TDepth>[] Split()
      {
         //If single channel, return a copy
         if (NumberOfChannels == 1) return new Image<Gray, TDepth>[] { Copy() as Image<Gray, TDepth> };

         //handle multiple channels
         Image<Gray, TDepth>[] res = new Image<Gray, TDepth>[NumberOfChannels];
         IntPtr[] a = new IntPtr[4];
         System.Drawing.Size size = Size;
         for (int i = 0; i < NumberOfChannels; i++)
         {
            res[i] = new Image<Gray, TDepth>(size);
            a[i] = res[i].Ptr;
         }

         CvInvoke.cvSplit(Ptr, a[0], a[1], a[2], a[3]);

         return res;
      }

      /// <summary>
      /// Save this image to the specific file
      /// </summary>
      /// <param name="fileName">The name of the file to be saved to</param>
      public override void Save(String fileName)
      {
         try
         {
            base.Save(fileName); //save the image using OpenCV
         }
         catch
         {
            using (Bitmap bmp = Bitmap)
               bmp.Save(fileName); //save the image using Bitmap
         }
      }

      /// <summary>
      /// The algorithm normalizes brightness and increases contrast of the image
      /// </summary>
      [ExposableMethod(Exposable = true)]
      public void _EqualizeHist()
      {
         //TODO: handle multiple channel as well
         CvInvoke.cvEqualizeHist(Ptr, Ptr);
      }
      #endregion

      #region IImage
      IImage[] IImage.Split()
      {
         return Array.ConvertAll<Image<Gray, TDepth>, IImage>(Split(), delegate(Image<Gray, TDepth> img) { return (IImage)img; });
      }
      #endregion

      #region ICloneable Members

      object ICloneable.Clone()
      {
         return Clone();
      }

      #endregion
   }
}
