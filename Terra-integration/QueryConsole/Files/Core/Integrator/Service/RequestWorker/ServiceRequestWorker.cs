using IntegrationInfo = Terrasoft.TsIntegration.Configuration.CsConstant.IntegrationInfo;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Ninject.Infrastructure.Language;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Serialization;
using System.ServiceModel.Activation;
using System.ServiceModel.Channels;
using System.ServiceModel.Web;
using System.ServiceModel;
using System.Text.RegularExpressions;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Web.Configuration;
using System.Web;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml.XPath;
using System.Xml;
using System;
using Terrasoft.Common;
using Terrasoft.Core.Configuration;
using Terrasoft.Core.DB;
using Terrasoft.Core.Entities;
using Terrasoft.Core.Factories;
using Terrasoft.Core;
using Terrasoft.UI.WebControls;
using TIntegrationType = Terrasoft.TsIntegration.Configuration.CsConstant.TIntegrationType;
namespace Terrasoft.TsIntegration.Configuration{
	public class ServiceRequestWorker : IServiceRequestWorker
	{
		//Log key=Integration Service
		public void MakeRequest(IServiceHandlerWorkers serviceHandlerWorker, Entity entity, BaseEntityHandler handler, string serviceName, string content)
		{
			var config = serviceHandlerWorker.GetServiceConfig(serviceName);
			if (config != null)
			{
				var service = serviceHandlerWorker.GetService(config.ServiceName);
				if (service != null)
				{
					var request = service.Create(config, content);
					service.SendRequest(request, response =>
					{
						string responseContent = service.GetContentFromResponse(response);
						var integrationInfo = CsConstant.IntegrationInfo.CreateForResponse(userConnection, entity);
						integrationInfo.StrData = responseContent;
						integrationInfo.Handler = handler;
						integrationInfo.Action = CsConstant.IntegrationActionName.UpdateFromResponse;
						handler.ProcessResponse(integrationInfo);
					}, exception =>
					{
						IntegrationLogger.Error(exception);
						handler.OnRequestException(exception);
					});
				}
				else
				{
					IntegrationLogger.Warning(string.Format("Сервис {0} не найден!", serviceName));
				}
			}
			else
			{
				IntegrationLogger.Warning(string.Format("Конфиг {0} не нейден!", serviceName));
			}
		}
	}
}