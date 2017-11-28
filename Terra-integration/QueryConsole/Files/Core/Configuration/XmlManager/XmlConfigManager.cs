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
	public static class XmlConfigManager
	{
		public const string DefaultMapAttrName = "Default";
		public const string DefaultByTypeMapAttrName = "DefaultByMappingType";
		public static bool IsConfigInited = false;
		public static IntegrationConfig IntegrationConfig;
		public static void InitLoadConfig(string xmlData)
		{
			if (IsConfigInited)
			{
				return;
			}
			InitDocument(xmlData);
			InitConfigData();
			IsConfigInited = true;
		}
		private static void InitDocument(string xmlData)
		{
			Document = XDocument.Parse(xmlData);
		}
		private static void InitConfigData()
		{
			//
			IntegrationConfig = new IntegrationConfig();
			InitPrepareConfig();
			InitExportRouteConfig();
			InitConfigSetting();
			InitDefaultMappingConfig();
			InitMappingConfig();
			InitServiceConfig();
			InitMockConfig();
			InitTemplateConfig();
			InitTriggerConfig();
			InitEndPointConfig();
			InitLogConfig();
		}

		private static void InitEndPointConfig()
		{
			IntegrationConfig.EndPointConfig = GetEndPointConfig("endPointConfig");
		}

		private static void InitLogConfig()
		{
			IntegrationConfig.LogConfig = RootNode
				.XPathSelectElements("logConfig")
				.Select(x => DynamicXmlParser.StartMapXmlToObj<LogItemConfig>(x, null, null, PreparePredicate))
				.ToList();
		}
		public static void InitPrepareConfig()
		{
			IntegrationConfig.PrerenderConfig = RootNode
				.XPathSelectElements("prerenderConfig/replace")
				.Select(x => DynamicXmlParser.StartMapXmlToObj<PrerenderConfig>(x, typeof(PrerenderConfig)))
				.ToList();
		}
		public static void InitExportRouteConfig()
		{
			IntegrationConfig.ExportRouteConfig = GetRoutesByPath("ExportRoutes/route");
			IntegrationConfig.ImportRouteConfig = GetRoutesByPath("ImportRoutes/route");
		}
		public static void InitConfigSetting()
		{
			IntegrationConfig.ConfigSetting = RootNode
				.XPathSelectElements("config")
				.Select(x =>
				{
					var config = DynamicXmlParser.StartMapXmlToObj<ConfigSetting>(x, typeof(ConfigSetting), null, PreparePredicate);
					config.HandlerConfigs = x.XPathSelectElements("handlerConfig")
						.Select(y => DynamicXmlParser.StartMapXmlToObj<HandlerConfig>(y, typeof(HandlerConfig), null, PreparePredicate))
						.ToList();
					return config;
				})
				.ToList();
		}
		public static void InitMockConfig()
		{
			IntegrationConfig.ServiceMockConfig = RootNode
				.XPathSelectElements("serviceMockConfig")
				.Select(x => DynamicXmlParser.StartMapXmlToObj<ServiceMockConfig>(x, typeof(ServiceMockConfig), null, PreparePredicate))
				.ToList();
		}
		public static void InitTemplateConfig()
		{
			IntegrationConfig.TemplateConfig = GetTemplateConfigs("templateConfig");
		}
		public static void InitTriggerConfig()
		{
			IntegrationConfig.TriggerConfig = GetTriggerConfigs("TriggerSettings");
		}
		public static void InitDefaultMappingConfig()
		{
			IntegrationConfig.DefaultMappingConfig = GetMappingConfig("mappingConfig[@Id='" + DefaultMapAttrName + "']");
			IntegrationConfig.DefaultMappingConfig.AddRange(GetMappingConfig("mappingConfig[@Id='" + DefaultByTypeMapAttrName + "']"));
		}
		public static void InitMappingConfig()
		{
			IntegrationConfig.MappingConfig = GetMappingConfig(String.Format("mappingConfig[@Id!='{0}' and @Id!='{1}']", DefaultMapAttrName, DefaultByTypeMapAttrName),
				x => DynamicXmlParser.StartMapXmlToObj<MappingItem>(x, typeof(MappingItem), GetMappingDefaultObjByNode(x), PreparePredicate));
		}
		public static void InitServiceConfig()
		{
			IntegrationConfig.ServiceConfig = GetServiceConfig("serviceConfig");
		}
		public static List<MappingConfig> GetMappingConfig(string path, Func<XElement, MappingItem> customeXmlMapper = null)
		{
			if (customeXmlMapper == null)
			{
				customeXmlMapper =
					x => DynamicXmlParser.StartMapXmlToObj<MappingItem>(x, typeof(MappingItem), null, PreparePredicate);
			}
			return RootNode
				.XPathSelectElements(path)
				.Select(x =>
				{
					var mappingItem = x
						.XPathSelectElements("mappingItem")
						.Select(y => customeXmlMapper(y))
						.ToList();
					return new MappingConfig()
					{
						Id = x.Attribute("Id").Value,
						Items = mappingItem
					};
				})
				.ToList();
		}
		private static List<EndPointConfig> GetEndPointConfig(string path, Func<XElement, EndPointHandlerConfig> customeXmlMapper = null)
		{
			if (customeXmlMapper == null)
			{
				customeXmlMapper =
					x => DynamicXmlParser.StartMapXmlToObj<EndPointHandlerConfig>(x, typeof(EndPointHandlerConfig), null, PreparePredicate);
			}
			return RootNode
				.XPathSelectElements(path)
				.Select(x =>
				{
					var handlerConfig = x
						.XPathSelectElements("handlerConfig")
						.Select(y => customeXmlMapper(y))
						.ToList();
					return new EndPointConfig()
					{
						Name = x.Attribute("Name").Value,
						EndPointHandler = x.Attribute("EndPointHandler").Value,
						HandlerConfigs = handlerConfig
					};
				})
				.ToList();
		}
		
		public static List<ServiceConfig> GetServiceConfig(string path, Func<XElement, ServiceHeaderConfig> customeXmlMapper = null)
		{
			if (customeXmlMapper == null)
			{
				customeXmlMapper =
					x => DynamicXmlParser.StartMapXmlToObj<ServiceHeaderConfig>(x, typeof(ServiceHeaderConfig), null, PreparePredicate);
			}
			return RootNode
				.XPathSelectElements(path)
				.Select(x =>
				{
					var headers = x
						.XPathSelectElements("serviceHeaderConfig")
						.Select(y => customeXmlMapper(y))
						.ToList();
					return new ServiceConfig()
					{
						Url = x.Attribute("Url").Value,
						Method = x.Attribute("Method").Value,
						Id = x.Attribute("Id").Value,
						Mock = x.Attribute("Mock").Value,
						ServiceName = x.Attribute("ServiceName").Value,
						Headers = headers
					};
				})
				.ToList();
		}
		public static TemplateSetting GetTemplateConfig(string name)
		{
			if (IntegrationConfig != null && IntegrationConfig.TemplateConfig.Any())
			{
				return IntegrationConfig.TemplateConfig.FirstOrDefault(x => x.Name == name);
			}
			return null;
		}
		public static List<TriggerSetting> GetTriggerConfigs(string path)
		{

			return RootNode
				.XPathSelectElements(path)
				.Select(x => DynamicXmlParser.StartMapXmlToObj<TriggerSetting>(x, typeof(TriggerSetting), null, PreparePredicate))
				.ToList();
		}
		public static List<TemplateSetting> GetTemplateConfigs(string path, Func<XElement, ServiceHeaderConfig> customeXmlMapper = null)
		{
			if (customeXmlMapper == null)
			{
				customeXmlMapper =
					x => DynamicXmlParser.StartMapXmlToObj<ServiceHeaderConfig>(x, typeof(ServiceHeaderConfig), null, PreparePredicate);
			}
			return RootNode
				.XPathSelectElements(path)
				.Select(x =>
				{
					var settings = x
						.XPathSelectElements("setting")
						.Select(y => customeXmlMapper(y))
						.ToDictionary(y => y.Key, y => y.Value);
					return new TemplateSetting()
					{
						Name = x.Attribute("Name").Value,
						Handler = x.Attribute("Handler").Value,
						Settings = settings
					};
				})
				.ToList();
		}
		private static List<RouteConfig> GetRoutesByPath(string path)
		{
			return RootNode
				.XPathSelectElements(path)
				.Select(x => DynamicXmlParser.StartMapXmlToObj<RouteConfig>(x, typeof(RouteConfig), null, PreparePredicate))
				.ToList();
		}
		private static MappingItem GetMappingDefaultObjByNode(XElement node)
		{
			if (node != null)
			{
				var mapTypeAttr = node.Attribute("MapType");
				if (mapTypeAttr != null)
				{
					var mappingType = mapTypeAttr.Value;
					var defaultByTypeMapConfig =
						IntegrationConfig.DefaultMappingConfig.FirstOrDefault(x => x.Id == DefaultByTypeMapAttrName);
					if (defaultByTypeMapConfig != null)
					{
						var itemByType = defaultByTypeMapConfig.Items.FirstOrDefault(x => x.MapType == mappingType);
						if (itemByType != null)
						{
							return itemByType;
						}
					}
				}
				var defaultMapConfig = IntegrationConfig.DefaultMappingConfig.FirstOrDefault(x => x.Id == DefaultMapAttrName);
				if (defaultMapConfig != null)
				{
					var item = defaultMapConfig.Items.FirstOrDefault();
					if (item != null)
					{
						return item;
					}
				}
			}
			return null;
		}
		public static string PreparePredicate(string value)
		{
			if (string.IsNullOrEmpty(value))
			{
				return value;
			}
			IntegrationConfig.PrerenderConfig.ForEach(x => value = value.Replace(x.From, x.To));
			return value;
		}

		#region Fields
		public static XDocument Document;

		private static XElement RootNode {
			get { return Document.Root; }
		}
		#endregion
	}
}