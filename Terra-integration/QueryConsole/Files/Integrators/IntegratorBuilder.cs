using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terrasoft.Core;

namespace Terrasoft.TsConfiguration
{
	public static class IntegratorBuilder
	{
		public static BaseServiceIntegrator Build(string serviceObjName, UserConnection userConnection)
		{
			IntegrationPath serviceIntegrationPath = IntegrationConfigurationManager.IntegrationPathConfig.Paths.FirstOrDefault(x => x.Name == serviceObjName);
			if(serviceIntegrationPath != null && !string.IsNullOrEmpty(serviceIntegrationPath.ServiceName))
			{
				string serviceName = serviceIntegrationPath.ServiceName;
				switch(serviceName)
				{
					case "OrderService":
						return new OrderServiceIntegrator(userConnection);
					case "ClientService":
						return new ClientServiceIntegrator(userConnection);
				}
			}
			return null;
		}
	}
}
