using System;
using System.Web;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using Terrasoft.Core;
using System.Runtime.Serialization;
using ServiceStack.ServiceInterface.ServiceModel;
using Terrasoft.Nui.ServiceModel.DataContract;

namespace Terrasoft.TsIntegration.Configuration
{
	[DataContract]
	public class MappingServiceResponse : BaseResponse
	{
		[DataMember(Name = "data")]
		public string Data;
		#region Constructors: Public

		public MappingServiceResponse()
		{
			Success = true;
		}

		public MappingServiceResponse(Exception e)
		{
			Exception = e;
		}

		#endregion

		#region Properties: Public

		public Exception Exception {
			set {
				Success = false;
				ResponseStatus = SetResponseStatus(value);
				ErrorInfo = SetErrorInfo(value);
			}
		}

		#endregion

		#region Methods: public

		public virtual ResponseStatus SetResponseStatus(Exception e)
		{
			return new ResponseStatus
			{
				ErrorCode = e.GetType().Name,
				Message = e.Message,
				StackTrace = e.StackTrace
			};
		}

		public virtual ErrorInfo SetErrorInfo(Exception e)
		{
			return new ErrorInfo
			{
				ErrorCode = e.GetType().Name,
				Message = e.Message,
				StackTrace = e.StackTrace
			};
		}

		#endregion
	}
}
