using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Terrasoft.TsIntegration.Configuration
{
	[EndPoint("BaseEndPoint")]
	public class BaseEndPointHandler: IEndPointHandler
	{
		public List<EndPointHandlerConfig> Config { get; set; }
		//Log key=EndPoint
		public string GetImportRoute(IIntegrationObject iObject)
		{
			var path = GetValue("RoutePath");
			var routeName = iObject.GetProperty<string>(path);;
			if (routeName != null)
			{
				var routeFormat = GetValue("Format");
				if (string.IsNullOrEmpty(routeFormat))
				{
					return routeName;
				}
				return string.Format(routeFormat, routeName);
			}
			return null;
		}
		//Log key=EndPoint
		public string GetValue(string key)
		{
			if (Config != null )
			{
				var keyValue = Config.FirstOrDefault(x => x.Key == key);
				if (keyValue != null)
				{
					return keyValue.Value;
				}
			}
			return null;
		}
	}
}
