using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Terrasoft.TsIntegration.Configuration
{
	public class ExceptionProcessData
	{
		public Dictionary<string, string> Config;
		public string Route;
		public IIntegrationObject IObject;
		public CsConstant.IntegrationInfo Info;
		public Exception Exception;
		public string RequestContent;

		public string GetConfigValue(string name, string defaultValue = null)
		{
			if (Config != null && Config.ContainsKey(name))
			{
				return Config[name];
			}
			return defaultValue;
		}
	}
}
