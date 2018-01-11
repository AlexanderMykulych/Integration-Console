using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using System;
namespace Terrasoft.TsIntegration.Configuration{
	public class IntegrationConfig: IIntegrationConfig
	{

		public virtual List<PrerenderConfig> PrerenderConfig { get; set; }
		public virtual List<RouteConfig> ExportRouteConfig { get; set; }
		public virtual List<RouteConfig> ImportRouteConfig { get; set; }
		public virtual List<ConfigSetting> ConfigSetting { get; set; }
		public virtual List<MappingConfig> DefaultMappingConfig { get; set; }
		public virtual List<MappingConfig> MappingConfig { get; set; }
		public virtual List<ServiceConfig> ServiceConfig { get; set; }
		public virtual List<ServiceMockConfig> ServiceMockConfig { get; set; }
		public virtual List<TemplateSetting> TemplateConfig { get; set; }
		public virtual List<TriggerSetting> TriggerConfig { get; set; }
		public virtual List<EndPointConfig> EndPointConfig { get; set; }
		public virtual List<LogItemConfig> LogConfig { get; set; }

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

		public void Init(XDocument document)
		{
			Document = document;
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

		public  List<MappingConfig> GetMappingConfig(string path, Func<XElement, MappingItem> customeXmlMapper = null)
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
		private  List<EndPointConfig> GetEndPointConfig(string path, Func<XElement, EndPointHandlerConfig> customeXmlMapper = null)
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

		public  List<ServiceConfig> GetServiceConfig(string path, Func<XElement, ServiceHeaderConfig> customeXmlMapper = null)
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
		public  TemplateSetting GetTemplateConfig(string name)
		{
			if (TemplateConfig.Any())
			{
				return TemplateConfig.FirstOrDefault(x => x.Name == name);
			}
			return null;
		}
		public  List<TriggerSetting> GetTriggerConfigs(string path)
		{

			return RootNode
				.XPathSelectElements(path)
				.Select(x => DynamicXmlParser.StartMapXmlToObj<TriggerSetting>(x, typeof(TriggerSetting), null, PreparePredicate))
				.ToList();
		}
		public  List<TemplateSetting> GetTemplateConfigs(string path, Func<XElement, ServiceHeaderConfig> customeXmlMapper = null)
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
		private  List<RouteConfig> GetRoutesByPath(string path)
		{
			return RootNode
				.XPathSelectElements(path)
				.Select(x => DynamicXmlParser.StartMapXmlToObj<RouteConfig>(x, typeof(RouteConfig), null, PreparePredicate))
				.ToList();
		}
		private  MappingItem GetMappingDefaultObjByNode(XElement node)
		{
			if (node != null)
			{
				var mapTypeAttr = node.Attribute("MapType");
				if (mapTypeAttr != null)
				{
					var mappingType = mapTypeAttr.Value;
					var defaultByTypeMapConfig =
						DefaultMappingConfig.FirstOrDefault(x => x.Id == DefaultByTypeMapAttrName);
					if (defaultByTypeMapConfig != null)
					{
						var itemByType = defaultByTypeMapConfig.Items.FirstOrDefault(x => x.MapType == mappingType);
						if (itemByType != null)
						{
							return itemByType;
						}
					}
				}
				var defaultMapConfig = DefaultMappingConfig.FirstOrDefault(x => x.Id == DefaultMapAttrName);
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
		public  string PreparePredicate(string value)
		{
			if (string.IsNullOrEmpty(value))
			{
				return value;
			}
			PrerenderConfig.ForEach(x => value = value.Replace(x.From, x.To));
			return value;
		}

		public  XDocument Document;

		private  XElement RootNode {
			get { return Document.Root; }
		}

		private  void InitLogConfig()
		{
			LogConfig = RootNode
				.XPathSelectElements("logConfig")
				.Select(x => DynamicXmlParser.StartMapXmlToObj<LogItemConfig>(x, null, null, PreparePredicate))
				.ToList();
		}
		public  void InitPrepareConfig()
		{
			PrerenderConfig = RootNode
				.XPathSelectElements("prerenderConfig/replace")
				.Select(x => DynamicXmlParser.StartMapXmlToObj<PrerenderConfig>(x, typeof(PrerenderConfig)))
				.ToList();
		}
		public  void InitExportRouteConfig()
		{
			ExportRouteConfig = GetRoutesByPath("ExportRoutes/route");
			ImportRouteConfig = GetRoutesByPath("ImportRoutes/route");
		}
		public  void InitConfigSetting()
		{
			ConfigSetting = RootNode
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
		public  void InitMockConfig()
		{
			ServiceMockConfig = RootNode
				.XPathSelectElements("serviceMockConfig")
				.Select(x => DynamicXmlParser.StartMapXmlToObj<ServiceMockConfig>(x, typeof(ServiceMockConfig), null, PreparePredicate))
				.ToList();
		}
		public  void InitTemplateConfig()
		{
			TemplateConfig = GetTemplateConfigs("templateConfig");
		}
		public  void InitTriggerConfig()
		{
			TriggerConfig = GetTriggerConfigs("TriggerSettings");
		}
		public  void InitDefaultMappingConfig()
		{
			DefaultMappingConfig = GetMappingConfig("mappingConfig[@Id='" + DefaultMapAttrName + "']");
			DefaultMappingConfig.AddRange(GetMappingConfig("mappingConfig[@Id='" + DefaultByTypeMapAttrName + "']"));
		}
		public  void InitMappingConfig()
		{
			MappingConfig = GetMappingConfig(String.Format("mappingConfig[@Id!='{0}' and @Id!='{1}']", DefaultMapAttrName, DefaultByTypeMapAttrName),
				x => DynamicXmlParser.StartMapXmlToObj<MappingItem>(x, typeof(MappingItem), GetMappingDefaultObjByNode(x), PreparePredicate));
		}
		public  void InitServiceConfig()
		{
			ServiceConfig = GetServiceConfig("serviceConfig");
		}
		private void InitEndPointConfig()
		{
			EndPointConfig = GetEndPointConfig("endPointConfig");
		}
		public const string DefaultMapAttrName = "Default";
		public const string DefaultByTypeMapAttrName = "DefaultByMappingType";
	}
}