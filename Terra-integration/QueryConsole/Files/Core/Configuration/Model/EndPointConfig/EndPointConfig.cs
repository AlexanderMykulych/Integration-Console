using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Terrasoft.TsIntegration.Configuration
{
	public class EndPointConfig
	{
		public string Name { get; set; }
		public string EndPointHandler { get; set; }
		public List<EndPointHandlerConfig> HandlerConfigs { get; set; }

		public string GetHandlerConfig(string name, string defaultValue = null)
		{
			if (HandlerConfigs != null)
			{
				var result = HandlerConfigs.FirstOrDefault(x => x.Key == name);
				if (result != null)
				{
					return result.Value;
				}
			}
			return defaultValue;
		}
	}
}
