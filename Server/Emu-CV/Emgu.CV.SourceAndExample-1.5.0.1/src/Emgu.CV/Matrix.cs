using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;
using Emgu.CV.Structure;

namespace Emgu.CV
{
   /// <summary> 
   /// The Matrix class that wrap around CvMat in OpenCV 
   /// </summary>
   /// <typeparam name="TDepth">Depth of this matrix (either Byte, SByte, Single, double, UInt16, Int16 or Int32)</typeparam>
   [Serializable]
   public class Matrix<TDepth> : CvArray<TDepth>, ICloneable, IEquatable<Matrix<TDepth>> where TDepth : new()
   {
      private TDepth[,] _array;

      private void AllocateHeader()
      {
         if (_ptr == IntPtr.Zero)
         {
            _ptr = Marshal.AllocHGlobal(StructSize.MCvMat);
            GC.AddMemoryPressure(StructSize.MCvMat);
         }
      }

      #region Constructors
      /// <summary>
      /// The default constructor which allows Data to be set later on
      /// </summary>
      protected Matrix()
      {
      }

      /// <summary>
      /// Create a Matrix (only header is allocated) using the Pinned/Unmanaged <paramref name="data"/>. The <paramref name="data"/> is not freed by the disposed function of this class 
      /// </summary>
      /// <param name="rows">The number of rows</param>
      /// <param name="cols">The number of cols</param>
      /// <param name="data">The Pinned/Unmanaged data</param>
      /// <param name="step">The step (row stride in bytes)</param>
      public Matrix(int rows, int cols, IntPtr data, int step)
      {
         AllocateHeader();
         CvInvoke.cvInitMatHeader(_ptr, rows, cols, CvDepth, data, step);
      }

      /// <summary>
      /// Create a Matrix (only header is allocated) using the Pinned/Unmanaged <paramref name="data"/>. The <paramref name="data"/> is not freed by the disposed function of this class 
      /// </summary>
      /// <param name="rows">The number of rows</param>
      /// <param name="cols">The number of cols</param>
      /// <param name="data">The Pinned/Unmanaged data</param>
      public Matrix(int rows, int cols, IntPtr data)
         : this(rows, cols, data, 0)
      {
      }

      /*
      /// <summary>
      /// Create a Matrix from the existing CvMat. The user is responsible for releasing the CvMat. 
      /// </summary>
      /// <param name="ptr">Pointer to the CvMat structure </param>
      public Matrix(IntPtr ptr)
      {
          _ptr = ptr;
      }*/

      /// <summary>
      /// Create a matrix of the specific size
      /// </summary>
      /// <param name="rows">The number of rows (<b>height</b>)</param>
      /// <param name="cols">The number of cols (<b>width</b>)</param>
      public Matrix(int rows, int cols)
         : this(rows, cols, 1)
      {
      }

      /// <summary>
      /// Create a matrix of the specific size
      /// </summary>
      /// <param name="size">The size of the matrix</param>
      public Matrix(System.Drawing.Size size)
         : this(size.Height, size.Width)
      {
      }

      /// <summary>
      /// Create a matrix of the specific size and channels
      /// </summary>
      /// <param name="rows">The number of rows</param>
      /// <param name="cols">The number of cols</param>
      /// <param name="channels">The number of channels</param>
      public Matrix(int rows, int cols, int channels)
      {
         AllocateData(rows, cols, channels);
      }

      /// <summary> 
      /// Create a matrix using the specific <paramref>data</paramref>
      /// </summary>
      public Matrix(TDepth[,] data)
      {
         Data = data;
      }

      /// <summary>
      /// Create a matrix using the specific <paramref name="data"/>
      /// </summary>
      /// <param name="data">the data for this matrix</param>
      public Matrix(TDepth[] data)
      {
         TDepth[,] mat = new TDepth[data.Length, 1];
         GCHandle hdl1 = GCHandle.Alloc(data, GCHandleType.Pinned);
         GCHandle hdl2 = GCHandle.Alloc(mat, GCHandleType.Pinned);
         Emgu.Util.Toolbox.memcpy(hdl2.AddrOfPinnedObject(), hdl1.AddrOfPinnedObject(), data.Length * _sizeOfElement);
         hdl1.Free();
         hdl2.Free();
         Data = mat;
      }
      #endregion

      #region Properties
      ///<summary> Get the depth representation for openCV</summary>
      protected static CvEnum.MAT_DEPTH CvDepth
      {
         get
         {
            Type typeOfDepth = typeof(TDepth);

            if (typeOfDepth == typeof(Single))
               return CvEnum.MAT_DEPTH.CV_32F;
            if (typeOfDepth == typeof(Int32))
               return Emgu.CV.CvEnum.MAT_DEPTH.CV_32S;
            if (typeOfDepth == typeof(SByte))
               return Emgu.CV.CvEnum.MAT_DEPTH.CV_8S;
            if (typeOfDepth == typeof(Byte))
               return CvEnum.MAT_DEPTH.CV_8U;
            if (typeOfDepth == typeof(Double))
               return CvEnum.MAT_DEPTH.CV_64F;
            if (typeOfDepth == typeof(UInt16))
               return CvEnum.MAT_DEPTH.CV_16U;
            if (typeOfDepth == typeof(Int16))
               return CvEnum.MAT_DEPTH.CV_16S;
            throw new NotImplementedException("Unsupported matrix depth");
         }
      }

      /// <summary>
      /// Get the underneath managed array
      /// </summary>
      public override System.Array ManagedArray
      {
         get { return Data; }
      }

      /// <summary>
      /// Get or Set the data for this matrix
      /// </summary>
      public TDepth[,] Data
      {
         get
         {
            return _array;
         }
         set
         {
            Debug.Assert(value != null, "The Array cannot be null");

            AllocateHeader();

            if (_dataHandle.IsAllocated)
               _dataHandle.Free(); //free the data handle
            Debug.Assert(!_dataHandle.IsAllocated, "Handle should be freed");

            _array = value;
            _dataHandle = GCHandle.Alloc(_array, GCHandleType.Pinned);

            CvInvoke.cvInitMatHeader(_ptr, _array.GetLength(0), _array.GetLength(1), CvDepth, _dataHandle.AddrOfPinnedObject(), 0x7fffffff);
         }
      }

      /// <summary>
      /// Get the number of channels for this matrix
      /// </summary>
      public override int NumberOfChannels
      {
         get
         {
            return MCvMat.NumberOfChannels;
         }
      }

      /// <summary>
      /// The MCvMat structure format  
      /// </summary>
      public MCvMat MCvMat
      {
         get
         {
            return (MCvMat)Marshal.PtrToStructure(Ptr, typeof(MCvMat));
         }
      }

      /// <summary>
      /// The function cvDet returns determinant of the square matrix
      /// </summary>
      [System.Diagnostics.DebuggerBrowsable(DebuggerBrowsableState.Never)]
      public double Det
      {
         get
         {
            return CvInvoke.cvDet(Ptr);
         }
      }

      /// <summary>
      /// Return the sum of the elements in this matrix
      /// </summary>
      [System.Diagnostics.DebuggerBrowsable(DebuggerBrowsableState.Never)]
      public double Sum
      {
         get
         {
            return CvInvoke.cvSum(Ptr).v0;
         }
      }
      #endregion

      #region copy and clone
      /// <summary>
      /// Return a matrix of the same size with all elements equals 0
      /// </summary>
      /// <returns>A matrix of the same size with all elements equals 0</returns>
      public Matrix<TDepth> CopyBlank()
      {
         return new Matrix<TDepth>(Rows, Cols);
      }

      /// <summary>
      /// Make a copy of this matrix
      /// </summary>
      /// <returns>A copy if this matrix</returns>
      public Matrix<TDepth> Clone()
      {
         Matrix<TDepth> mat = new Matrix<TDepth>(Rows, Cols, NumberOfChannels);
         CvInvoke.cvCopy(Ptr, mat.Ptr, IntPtr.Zero);
         return mat;
      }
      #endregion

      /// <summary>
      /// Convert this matrix to different depth
      /// </summary>
      /// <typeparam name="TOtherDepth">The depth type to convert to</typeparam>
      /// <returns>Matrix of different depth</returns>
      public Matrix<TOtherDepth> Convert<TOtherDepth>()  where TOtherDepth : new ()
      {
         Matrix<TOtherDepth> res = new Matrix<TOtherDepth>(Rows, Cols, NumberOfChannels);
         CvInvoke.cvConvertScale(Ptr, res.Ptr, 1.0, 0.0);
         return res;
      }

      ///<summary> Returns the transpose of this matrix</summary>
      public Matrix<TDepth> Transpose()
      {
         Matrix<TDepth> res = new Matrix<TDepth>(Cols, Rows);
         CvInvoke.cvTranspose(_ptr, res._ptr);
         return res;
      }

      /// <summary>
      /// Get or Set the value in the specific <paramref name="row"/> and <paramref name="col"/>
      /// </summary>
      /// <param name="row">the row of the element</param>
      /// <param name="col">the col of the element</param>
      /// <returns></returns>
      public TDepth this[int row, int col]
      {
         get
         {
            return (TDepth)System.Convert.ChangeType(CvInvoke.cvGetReal2D(Ptr, row, col), typeof(TDepth));
         }
         set
         {
            CvInvoke.cvSet2D(Ptr, row, col, new MCvScalar(System.Convert.ToDouble(value)));
         }
      }

      /// <summary>
      /// Allocate data for the array
      /// </summary>
      /// <param name="rows">The number of rows</param>
      /// <param name="cols">The number of columns</param>
      /// <param name="numberOfChannels">The number of channels for this matrix</param>
      protected override void AllocateData(int rows, int cols, int numberOfChannels)
      {
         Data = new TDepth[rows, cols*numberOfChannels];
         if (numberOfChannels > 1)
            CvInvoke.cvReshape(_ptr, _ptr, numberOfChannels, 0);
      }

      #region Accessing Elements and sub-Arrays
      /// <summary>
      /// Get a submatrix corresponding to a specified rectangle
      /// </summary>
      /// <param name="rect">the rectangle area of the sub-matrix</param>
      /// <returns>A submatrix corresponding to a specified rectangle</returns>
      public Matrix<TDepth> GetSubRect(System.Drawing.Rectangle rect)
      {
         Matrix<TDepth> subMat = new Matrix<TDepth>();
         subMat._array = _array;
         subMat.AllocateHeader();
         CvInvoke.cvGetSubRect(_ptr, subMat.Ptr, rect);
         return subMat;
      }

      /// <summary>
      /// Get a submatrix corresponding to a specified rectangle
      /// </summary>
      /// <param name="rect">the rectangle area of the sub-matrix</param>
      /// <returns>A submatrix corresponding to a specified rectangle</returns>
      [Obsolete("Use GetSubRect instead, will be removed in the next version")]
      public Matrix<TDepth> GetSubMatrix(System.Drawing.Rectangle rect)
      {
         Matrix<TDepth> subMat = new Matrix<TDepth>();
         subMat._array = _array;
         subMat.AllocateHeader();
         CvInvoke.cvGetSubRect(_ptr, subMat.Ptr, rect);
         return subMat;
      }

      /// <summary>
      /// Get the specific row of the matrix
      /// </summary>
      /// <param name="row">the index of the row to be reterived</param>
      /// <returns>the specific row of the matrix</returns>
      public Matrix<TDepth> GetRow(int row)
      {
         return GetRows(row, row + 1, 1);
      }

      /// <summary>
      /// Return the matrix corresponding to a specified row span of the input array
      /// </summary>
      /// <param name="startRow">Zero-based index of the starting row (inclusive) of the span</param>
      /// <param name="endRow">Zero-based index of the ending row (exclusive) of the span</param>
      /// <param name="deltaRow">Index step in the row span. That is, the function extracts every delta_row-th row from start_row and up to (but not including) end_row</param>
      /// <returns>A matrix corresponding to a specified row span of the input array</returns>
      public Matrix<TDepth> GetRows(int startRow, int endRow, int deltaRow)
      {
         Matrix<TDepth> subMat = new Matrix<TDepth>();
         subMat._array = _array;
         subMat.AllocateHeader();
         subMat._ptr = CvInvoke.cvGetRows(_ptr, subMat.Ptr, startRow, endRow, deltaRow);
         return subMat;
      }

      /// <summary>
      /// Get the specific column of the matrix
      /// </summary>
      /// <param name="col">the index of the column to be reterived</param>
      /// <returns>the specific column of the matrix</returns>
      public Matrix<TDepth> GetCol(int col)
      {
         return GetCols(col, col + 1);
      }

      /// <summary>
      /// Get the Matrix, corresponding to a specified column span of the input array
      /// </summary>
      /// <param name="endCol">Zero-based index of the ending column (exclusive) of the span</param>
      /// <param name="startCol">Zero-based index of the selected column</param>
      /// <returns>the specific column span of the matrix</returns>
      public Matrix<TDepth> GetCols(int startCol, int endCol)
      {
         Matrix<TDepth> subMat = new Matrix<TDepth>();
         subMat._array = _array;
         subMat.AllocateHeader();
         subMat._ptr = CvInvoke.cvGetCols(_ptr, subMat.Ptr, startCol, endCol);
         return subMat;
      }

      /// <summary>
      /// Return the specific diagonal elements of this matrix
      /// </summary>
      /// <param name="diag">Array diagonal. Zero corresponds to the main diagonal, -1 corresponds to the diagonal above the main etc., 1 corresponds to the diagonal below the main etc</param>
      /// <returns>The specific diagonal elements of this matrix</returns>
      public Matrix<TDepth> GetDiag(int diag)
      {
         Matrix<TDepth> subMat = new Matrix<TDepth>();
         subMat._array = _array;
         subMat.AllocateHeader();
         subMat._ptr = CvInvoke.cvGetDiag(_ptr, subMat.Ptr, diag);
         return subMat;
      }

      /// <summary>
      /// Return the main diagonal element of this matrix
      /// </summary>
      /// <returns>The main diagonal element of this matrix</returns>
      public Matrix<TDepth> GetDiag()
      {
         return GetDiag(0);
      }
      #endregion

      #region Removing rows or columns
      /// <summary>
      /// Return the matrix without a specified row span of the input array
      /// </summary>
      /// <param name="startRow">Zero-based index of the starting row (inclusive) of the span</param>
      /// <param name="endRow">Zero-based index of the ending row (exclusive) of the span</param>
      /// <returns>The matrix without a specified row span of the input array</returns>
      public Matrix<TDepth> RemoveRows(int startRow, int endRow)
      {
         if (startRow == 0)
            return GetRows(endRow, Rows, 1);
         if (endRow == Rows)
            return GetRows(0, startRow, 1);

         using (Matrix<TDepth> upper = GetRows(0, startRow, 1))
         using (Matrix<TDepth> lower = GetRows(endRow, Rows, 1))
            return upper.ConcateVertical(lower);
      }

      /// <summary>
      /// Return the matrix without a specified column span of the input array
      /// </summary>
      /// <param name="startCol">Zero-based index of the starting column (inclusive) of the span</param>
      /// <param name="endCol">Zero-based index of the ending column (exclusive) of the span</param>
      /// <returns>The matrix without a specified column span of the input array</returns>
      public Matrix<TDepth> RemoveCols(int startCol, int endCol)
      {
         if (startCol == 0)
            return GetCols(endCol, Cols);
         if (endCol == Cols)
            return GetCols(0, startCol);

         using (Matrix<TDepth> upper = GetCols(0, startCol))
         using (Matrix<TDepth> lower = GetCols(endCol, Cols))
            return upper.ConcateHorizontal(lower);
      }
      #endregion

      #region Matrix convatenation
      /// <summary>
      /// Concate the current matrix with another matrix vertically. If this matrix is n1 x m and <paramref name="otherMatrix"/> is n2 x m, the resulting matrix is (n1+n2) x m.
      /// </summary>
      /// <param name="otherMatrix">The other matrix to concate</param>
      /// <returns>A new matrix that is the vertical concatening of this matrix and <paramref name="otheMatrix"/></returns>
      public Matrix<TDepth> ConcateVertical(Matrix<TDepth> otherMatrix)
      {
         Debug.Assert(Cols == otherMatrix.Cols, "The number of columns must be the same when concatening matrices verticly.");
         Matrix<TDepth> res = new Matrix<TDepth>(Rows + otherMatrix.Rows, Cols);
         using (Matrix<TDepth> subUppper = res.GetRows(0, Rows, 1))
            CopyTo(subUppper);
         using (Matrix<TDepth> subLower = res.GetRows(Rows, res.Rows, 1))
            otherMatrix.CopyTo(subLower);
         return res;
      }

      /// <summary>
      /// Concate the current matrix with another matrix horizontally. If this matrix is n x m1 and <paramref name="otherMatrix"/> is n x m2, the resulting matrix is n x (m1 + m2).
      /// </summary>
      /// <param name="otherMatrix">The other matrix to concate</param>
      /// <returns>A matrix that is the horizontal concatening of this matrix and <paramref name="otheMatrix"/></returns>
      public Matrix<TDepth> ConcateHorizontal(Matrix<TDepth> otherMatrix)
      {
         Debug.Assert(Rows == otherMatrix.Rows, "The number of rows must be the same when concatening matrices horizontally.");
         Matrix<TDepth> res = new Matrix<TDepth>(Rows, Cols + otherMatrix.Cols);
         using (Matrix<TDepth> subLeft = res.GetCols(0, Cols))
            CopyTo(subLeft);
         using (Matrix<TDepth> subRight = res.GetCols(Cols, res.Cols))
            otherMatrix.CopyTo(subRight);
         return res;
      }
      #endregion

      /// <summary>
      /// Returns the min / max locations and values for the matrix
      /// </summary>
      public void MinMax(out double minValue, out double maxValue, out System.Drawing.Point minLocation, out System.Drawing.Point maxLocation)
      {
         minValue = 0; maxValue = 0;
         minLocation = new System.Drawing.Point(); maxLocation = new System.Drawing.Point();
         CvInvoke.cvMinMaxLoc(Ptr, ref minValue, ref maxValue, ref minLocation, ref maxLocation, IntPtr.Zero);
      }

      #region Addition
      ///<summary> Elementwise add another matrix with the current matrix </summary>
      ///<param name="mat2">The matrix to be added to the current matrix</param>
      ///<returns> The result of elementwise adding mat2 to the current matrix</returns>
      public Matrix<TDepth> Add(Matrix<TDepth> mat2)
      {
         Matrix<TDepth> res = CopyBlank();
         CvInvoke.cvAdd(Ptr, mat2.Ptr, res.Ptr, IntPtr.Zero);
         return res;
      }

      ///<summary> Elementwise add a color <paramref name="val"/> to the current matrix</summary>
      ///<param name="val">The value to be added to the current matrix</param>
      ///<returns> The result of elementwise adding <paramref name="val"/> from the current matrix</returns>
      public Matrix<TDepth> Add(TDepth val)
      {
         Matrix<TDepth> res = CopyBlank();
         CvInvoke.cvAddS(Ptr, new MCvScalar(System.Convert.ToDouble(val)), res.Ptr, IntPtr.Zero);
         return res;
      }
      #endregion

      #region Substraction
      ///<summary> Elementwise substract another matrix from the current matrix </summary>
      ///<param name="mat2"> The matrix to be substracted to the current matrix</param>
      ///<returns> The result of elementwise substracting mat2 from the current matrix</returns>
      public Matrix<TDepth> Sub(Matrix<TDepth> mat2)
      {
         Matrix<TDepth> res = CopyBlank();
         CvInvoke.cvSub(Ptr, mat2.Ptr, res.Ptr, IntPtr.Zero);
         return res;
      }

      ///<summary> Elementwise substract a color <paramref name="val"/> to the current matrix</summary>
      ///<param name="val"> The value to be substracted from the current matrix</param>
      ///<returns> The result of elementwise substracting <paramref name="val"/> from the current matrix</returns>
      public Matrix<TDepth> Sub(TDepth val)
      {
         Matrix<TDepth> res = CopyBlank();
         CvInvoke.cvSubS(Ptr, new MCvScalar(System.Convert.ToDouble(val)), res.Ptr, IntPtr.Zero);
         return res;
      }

      /// <summary>
      /// result = val - this
      /// </summary>
      /// <param name="val">The value which subtract this matrix</param>
      /// <returns>val - this</returns>
      public Matrix<TDepth> SubR(TDepth val)
      {
         Matrix<TDepth> res = CopyBlank();
         CvInvoke.cvSubRS(Ptr, new MCvScalar(System.Convert.ToDouble(val)), res.Ptr, IntPtr.Zero);
         return res;
      }
      #endregion

      #region Multiplication
      ///<summary> Multiply the current matrix with <paramref name="scale"/></summary>
      ///<param name="scale">The scale to be multiplied</param>
      ///<returns> The scaled matrix </returns>
      public Matrix<TDepth> Mul(double scale)
      {
         Matrix<TDepth> res = CopyBlank();
         CvInvoke.cvConvertScale(Ptr, res.Ptr, scale, 0.0);
         return res;
      }

      ///<summary> Multiply the current matrix with <paramref name="mat2"/></summary>
      ///<param name="mat2">The matrix to be multiplied</param>
      ///<returns> Result matrix of the multiplication </returns>
      public Matrix<TDepth> Mul(Matrix<TDepth> mat2)
      {
         Matrix<TDepth> res = new Matrix<TDepth>(Rows, mat2.Cols);
         CvInvoke.cvGEMM(Ptr, mat2.Ptr, 1.0, IntPtr.Zero, 0.0, res.Ptr, Emgu.CV.CvEnum.GEMM_TYPE.CV_GEMM_DEFAULT);
         return res;
      }
      #endregion

      #region Operator overload
      /// <summary>
      /// Elementwise add <paramref name="mat1"/> with <paramref name="mat2"/>
      /// </summary>
      /// <param name="mat1">The Matrix to be added</param>
      /// <param name="mat2">The Matrix to be added</param>
      /// <returns>The elementwise sum of the two matrices</returns>
      public static Matrix<TDepth> operator +(Matrix<TDepth> mat1, Matrix<TDepth> mat2)
      {
         return mat1.Add(mat2);
      }

      /// <summary>
      /// Elementwise add <paramref name="mat1"/> with <paramref name="val"/>
      /// </summary>
      /// <param name="mat1">The Matrix to be added</param>
      /// <param name="val">The value to be added</param>
      /// <returns>The matrix plus the value</returns>
      public static Matrix<TDepth> operator +(Matrix<TDepth> mat1, double val)
      {
         return mat1.Add((TDepth)System.Convert.ChangeType(val, typeof(TDepth)));
      }

      /// <summary>
      /// <paramref name="val"/> + <paramref name="mat1"/>
      /// </summary>
      /// <param name="mat1">The Matrix to be added</param>
      /// <param name="val">The value to be added</param>
      /// <returns>The matrix plus the value</returns>
      public static Matrix<TDepth> operator +(double val, Matrix<TDepth> mat1)
      {
         return mat1.Add((TDepth)System.Convert.ChangeType(val, typeof(TDepth)));
      }

      /// <summary>
      /// <paramref name="val"/> - <paramref name="mat1"/> 
      /// </summary>
      /// <param name="mat1">The Matrix to be subtracted</param>
      /// <param name="val">The value to be subtracted</param>
      /// <returns><paramref name="val"/> - <paramref name="mat1"/></returns>
      public static Matrix<TDepth> operator -(double val, Matrix<TDepth> mat1)
      {
         return mat1.SubR((TDepth)System.Convert.ChangeType(val, typeof(TDepth)));
      }

      /// <summary>
      /// <paramref name="mat1"/> - <paramref name="mat2"/> 
      /// </summary>
      /// <param name="mat1">The Matrix to be subtracted</param>
      /// <param name="mat2">The matrix to subtract</param>
      /// <returns><paramref name="mat1"/> - <paramref name="mat2"/></returns>
      public static Matrix<TDepth> operator -(Matrix<TDepth> mat1, Matrix<TDepth> mat2)
      {
         return mat1.Sub(mat2);
      }

      /// <summary>
      /// <paramref name="mat1"/> - <paramref name="val"/> 
      /// </summary>
      /// <param name="mat1">The Matrix to be subtracted</param>
      /// <param name="val">The value to be subtracted</param>
      /// <returns><paramref name="mat1"/> - <paramref name="val"/></returns>
      public static Matrix<TDepth> operator -(Matrix<TDepth> mat1, double val)
      {
         return mat1.Sub((TDepth)System.Convert.ChangeType(val, typeof(TDepth)));
      }

      /// <summary>
      /// <paramref name="mat1"/> * <paramref name="val"/> 
      /// </summary>
      /// <param name="mat1">The Matrix to be multiplied</param>
      /// <param name="val">The value to be multiplied</param>
      /// <returns><paramref name="mat1"/> * <paramref name="val"/></returns>
      public static Matrix<TDepth> operator *(Matrix<TDepth> mat1, double val)
      {
         return mat1.Mul(val);
      }

      /// <summary>
      ///  <paramref name="val"/> * <paramref name="mat1"/> 
      /// </summary>
      /// <param name="mat1">The matrix to be multiplied</param>
      /// <param name="val">The value to be multiplied</param>
      /// <returns> <paramref name="val"/> * <paramref name="mat1"/> </returns>
      public static Matrix<TDepth> operator *(double val, Matrix<TDepth> mat1)
      {
         return mat1.Mul(val);
      }

      /// <summary>
      /// <paramref name="mat1"/> / <paramref name="val"/> 
      /// </summary>
      /// <param name="mat1">The Matrix to be divided</param>
      /// <param name="val">The value to be divided</param>
      /// <returns><paramref name="mat1"/> / <paramref name="val"/></returns>
      public static Matrix<TDepth> operator /(Matrix<TDepth> mat1, double val)
      {
         return mat1.Mul(1.0 / val);
      }

      /// <summary>
      /// <paramref name="mat1"/> * <paramref name="mat2"/> 
      /// </summary>
      /// <param name="mat1">The Matrix to be multiplied</param>
      /// <param name="mat2">The Matrix to be multiplied</param>
      /// <returns><paramref name="mat1"/> * <paramref name="mat2"/></returns>
      public static Matrix<TDepth> operator *(Matrix<TDepth> mat1, Matrix<TDepth> mat2)
      {
         return mat1.Mul(mat2);
      }
      #endregion

      #region Implement ISerializable interface
      /// <summary>
      /// Constructor used to deserialize runtime serialized object
      /// </summary>
      /// <param name="info">The serialization info</param>
      /// <param name="context">The streaming context</param>
      public Matrix(SerializationInfo info, StreamingContext context)
      {
         DeserializeObjectData(info, context);
      }
      #endregion

      #region UnmanagedObject
      /// <summary>
      /// Release the matrix and all the memory associate with it
      /// </summary>
      protected override void DisposeObject()
      {
         if (_ptr != IntPtr.Zero)
         {
            Marshal.FreeHGlobal(_ptr);
            GC.RemoveMemoryPressure(StructSize.MCvMat);
            _ptr = IntPtr.Zero;
         }

         base.DisposeObject();
      }
      #endregion

      #region Comparison
      /// <summary>
      /// This function compare the current matrix with <paramref name="mat2"/> and returns the comparison mask
      /// </summary>
      /// <param name="mat2">The other matrix to compare with</param>
      /// <param name="type">Comparison type</param>
      /// <returns>The comparison mask</returns>
      public Matrix<Byte> Cmp(Matrix<TDepth> mat2, Emgu.CV.CvEnum.CMP_TYPE type)
      {
         Matrix<Byte> res = new Matrix<Byte>(Rows, Cols);
         CvInvoke.cvCmp(Ptr, mat2.Ptr, res.Ptr, type);
         return res;
      }

      /// <summary>
      /// Get all channels for the multi channel matrix
      /// </summary>
      /// <returns></returns>
      public Matrix<TDepth>[] Split()
      {
         int channelCount = NumberOfChannels;
         Matrix<TDepth>[] channels = new Matrix<TDepth>[channelCount];
         for (int i = 0; i < channelCount; i++)
         {
            channels[i] = new Matrix<TDepth>(Rows, Cols);
         }
         CvInvoke.cvSplit(
            Ptr,
            channels[0].Ptr,
            channelCount >= 2 ? channels[1].Ptr : IntPtr.Zero,
            channelCount >= 3 ? channels[2].Ptr : IntPtr.Zero,
            channelCount >= 4 ? channels[3].Ptr : IntPtr.Zero);
         return channels;
      }

      /// <summary>
      /// Return true if every element of this matrix equals elements in <paramref name="mat2"/>
      /// </summary>
      /// <param name="mat2">The other matrix to compare with</param>
      /// <returns>true if every element of this matrix equals elements in <paramref name="mat2"/></returns>
      public bool Equals(Matrix<TDepth> mat2)
      {
         if (!EqualSize(mat2)) return false;
         int numberOfChannels = NumberOfChannels;
         if (numberOfChannels != mat2.NumberOfChannels) return false;

         if (numberOfChannels == 1)
            using (Matrix<Byte> neqMask = Cmp(mat2, Emgu.CV.CvEnum.CMP_TYPE.CV_CMP_NE))
            {
               return CvInvoke.cvCountNonZero(neqMask.Ptr) == 0;
            }
         else
         {  //comapre channel by channel
            Matrix<TDepth>[] channels = Split();
            Matrix<TDepth>[] channels2 = mat2.Split();
            try
            {
               for (int i = 0; i < numberOfChannels; i++)
               {
                  if (!channels[i].Equals(channels2[i]))
                     return false;
               }
               return true;
            }
            finally
            {
               foreach (Matrix<TDepth> channel in channels)
                  channel.Dispose();
               foreach (Matrix<TDepth> channel in channels2)
                  channel.Dispose();
            }
         }
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
