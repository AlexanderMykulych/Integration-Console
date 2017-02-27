using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Terrasoft.TsConfiguration
{
	public class RequestErrorLoggerInfo: BaseLoggerInfo
	{
		public Exception Exception;
		public string ResponseText;
		public string ResponseJson;
	}
}
