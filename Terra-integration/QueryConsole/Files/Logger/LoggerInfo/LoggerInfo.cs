using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terrasoft.Core;

namespace Terrasoft.TsConfiguration
{
	public class LoggerInfo: BaseLoggerInfo
	{
		public UserConnection UserConnection;
		public string RequesterName;
		public string ReciverName;
		public string BpmObjName;
		public string ServiceObjName;
		public string AdditionalInfo;
		public bool UseExistTransaction;
		public static LoggerInfo GetBpmRequestLogInfo(UserConnection userConnection, string serviceName, string bpmObjName, string serviceObjName, string addInfo = "")
		{
			return new LoggerInfo()
			{
				UserConnection = userConnection,
				RequesterName = CsConstant.PersonName.Bpm,
				ReciverName = serviceName,
				ServiceObjName = serviceObjName,
				BpmObjName = bpmObjName,
				AdditionalInfo = addInfo
			};
		}
		public static LoggerInfo GetNotifyRequestLogInfo(UserConnection userConnection, string addInfo = "")
		{
			return new LoggerInfo()
			{
				UserConnection = userConnection,
				RequesterName = CsConstant.PersonName.Unknown,
				ReciverName = CsConstant.PersonName.Bpm,
				ServiceObjName = CsConstant.PersonName.Unknown,
				BpmObjName = CsConstant.PersonName.Unknown,
				AdditionalInfo = addInfo
			};
		}

		public LoggerInfo()
		{
			UseExistTransaction = true;
		}
	}
}
