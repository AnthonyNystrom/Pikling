using System;
using System.Collections.Generic;
using System.Text;
using System.ServiceModel;

namespace Emgu.CV
{
   ///<summary>
   ///The interface to request a duplex image capture
   ///</summary>
   [ServiceContract(CallbackContract = typeof(IDuplexCaptureCallback))]
   public interface IDuplexCapture
   {
      /// <summary>
      /// Request a frame from server
      /// </summary>
      [OperationContract(IsOneWay = true)]
      void DuplexQueryFrame();

      /// <summary>
      /// Request a frame from server which is half width and half height
      /// </summary>
      [OperationContract(IsOneWay = true)]
      void DuplexQuerySmallFrame();
   }
}
