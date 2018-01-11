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
	public class ServiceHandlerWorker : IServiceHandlerWorkers
	{
		private readonly ISettingProvider _settingProvider;

		public ServiceHandlerWorker(ISettingProvider settingProvider)
		{
			_settingProvider = settingProvider;
		}
		public List<ConfigSetting> GetConfigs(string routeKey, CsConstant.TIntegrationType type)
		{
			IEnumerable<RouteConfig> routeConfig;
			switch (type)
			{
				case CsConstant.TIntegrationType.Export:
					routeConfig = _settingProvider.Get("ExportRouteConfig").SelectFromList<RouteConfig>().Where(x => x.Key == routeKey).ToList();
					break;
				default:
					routeConfig = _settingProvider.Get("ImportRouteConfig").SelectFromList<RouteConfig>().Where(x => x.Key == routeKey).ToList();
					break;
			}
			var config = _settingProvider.SelectEnumerableByType<ConfigSetting>().Where(x => routeConfig.Any(y => y.ConfigId == x.Id)).ToList();
			return config;
		}
		public BaseEntityHandler GetWithConfig(string name, ConfigSetting config)
		{
			return HandlerFactory.Get(name, config);
		}
		public ServiceConfig GetServiceConfig(string serviceName)
		{
			return _settingProvider.SelectFirstByType<ServiceConfig>(x => x.Id == serviceName);
		}
		public IIntegrationService GetService(string serviceName)
		{
			return ServiceFactory.Get(serviceName);
		}
		public MappingConfig GetMappingConfig(string configId)
		{
			return _settingProvider.SelectFirstByType<MappingConfig>(x => x.Id == configId);
		}
	}
}