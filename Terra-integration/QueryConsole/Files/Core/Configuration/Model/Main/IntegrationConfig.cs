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
	public class IntegrationConfig
	{
		public List<PrerenderConfig> PrerenderConfig;
		public List<RouteConfig> ExportRouteConfig;
		public List<RouteConfig> ImportRouteConfig;
		public List<ConfigSetting> ConfigSetting;
		public List<MappingConfig> DefaultMappingConfig;
		public List<MappingConfig> MappingConfig;
		public List<ServiceConfig> ServiceConfig;
		public List<ServiceMockConfig> ServiceMockConfig;
		public List<TemplateSetting> TemplateConfig;
		public List<TriggerSetting> TriggerConfig;
		public List<EndPointConfig> EndPointConfig;
		public List<LogItemConfig> LogConfig;

		private Dictionary<string, Func<object>> _mapping;

		public IntegrationConfig()
		{
			_mapping = new Dictionary<string, Func<object>>()
			{
				{ "PrerenderConfig", () => PrerenderConfig },
				{ "ExportRouteConfig", () => ExportRouteConfig },
				{ "ImportRouteConfig", () => ImportRouteConfig },
				{ "ConfigSetting", () => ConfigSetting },
				{ "DefaultMappingConfig", () => DefaultMappingConfig },
				{ "MappingConfig", () => MappingConfig },
				{ "ServiceConfig", () => ServiceConfig },
				{ "ServiceMockConfig", () => ServiceMockConfig },
				{ "TemplateConfig", () => TemplateConfig },
				{ "TriggerConfig", () => TriggerConfig },
				{ "EndPointConfig", () => EndPointConfig },
				{ "LogConfig", () => LogConfig }
			};
		}
		public object Get(string settingName)
		{
			if (_mapping != null && _mapping.ContainsKey(settingName))
			{
				return _mapping[settingName]();
			}
			return null;
		}
	}
}