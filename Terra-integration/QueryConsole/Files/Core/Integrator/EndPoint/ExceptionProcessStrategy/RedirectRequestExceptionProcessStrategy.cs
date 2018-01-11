﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Terrasoft.TsIntegration.Configuration
{
	[ExceptionProcessStrategy("RedirectRequest")]
	public class RedirectRequestExceptionProcessStrategy: IExceptionProcessStrategy
	{
		public RedirectRequestExceptionProcessStrategy(IServiceHandlerWorkers serviceWorker)
		{
			ServiceHandlerWorker = serviceWorker;
		}

		public virtual IServiceHandlerWorkers ServiceHandlerWorker { get; set; }

		//Log key=Integration Redirect
		public Stream Process(ExceptionProcessData data)
		{
			Stream result = null;
			var serviceName = data.GetConfigValue("ServiceName");
			LoggerHelper.DoInLogBlock("Redirect to: " + serviceName, () =>
			{
				var config = ServiceHandlerWorker.GetServiceConfig(serviceName);
				if (config != null)
				{
					var service = ServiceHandlerWorker.GetService(config.ServiceName);
					var request = service.Create(config, data.RequestContent);
					service.SendRequest(request, response =>
					{
						result = response.GetResponseStream();
					}, exception =>
					{
						IntegrationLogger.Error(exception);
					});
				}
			});
			return result;
		}
	}
}
