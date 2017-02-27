using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Terrasoft.TsConfiguration
{
	public class RequestLoggerInfo : BaseLoggerInfo
	{
		public TRequstMethod RequestMethod;
		public string Url;
		public string JsonText;
		public string AdditionalInfo;
	}
}
