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
	public static class SettingsManager
	{
		private static ISettingProvider _settingProvider;

		private static ISettingProvider settingProvider
		{
			get
			{
				if (_settingProvider == null)
				{
					_settingProvider = ObjectFactory.Get<ISettingProvider>();
				}
				return _settingProvider;
			}
		}

		private static ConcurrentDictionary<string, ValueType> _integration;
		public static ConcurrentDictionary<string, ValueType> Integration {
			get {
				if (_integration == null)
				{
					_integration = InitIntegrationSettings();
				}
				return _integration;
			}
		}

		public static ConcurrentDictionary<string, List<ConfigSetting>> ConfigSettings;
		public static ConcurrentDictionary<string, MappingConfig> MappingConfig;

		public static Guid IntegrationId;

		public static List<Type> Handlers;
		public static T GetIntegratorSetting<T>(string settingName)
		{
			return GetSetting<T>(Integration, settingName);
		}
		public static List<ConfigSetting> GetHandlerConfigs(string key, CsConstant.TIntegrationType type)
		{
			if (string.IsNullOrEmpty(key))
			{
				return new List<ConfigSetting>();
			}
			if (ConfigSettings == null)
			{
				ConfigSettings = new ConcurrentDictionary<string, List<ConfigSetting>>();
			}
			if (ConfigSettings.ContainsKey(key))
			{
				return ConfigSettings[key];
			}
			List<RouteConfig> routeConfig;
			switch (type)
			{
				case CsConstant.TIntegrationType.Export:
					routeConfig = GetExportRoutes(key).ToList();
					break;
				default:
					routeConfig = GetImportRoutes(key).ToList();
					break;
			}
			var config = GetSettings<ConfigSetting>("ConfigSetting").Where(x => routeConfig.Any(y => y.ConfigId == x.Id)).ToList();
			ConfigSettings.TryAdd(key, config);
			return config;
		}
		public static IEnumerable<RouteConfig> GetExportRoutes(string key)
		{
			return GetSettings<RouteConfig>("ExportRouteConfig").Where(x => x.Key == key);
		}

		private static IEnumerable<T> GetSettings<T>(string name)
		{
			return settingProvider.Get(name).SelectFromList<T>();
		}
		public static IEnumerable<RouteConfig> GetImportRoutes(string key)
		{
			return GetSettings<RouteConfig>("ImportRouteConfig").Where(x => x.Key == key);
		}
		public static ConfigSetting GetHandlerConfig(string key, string handlerName, CsConstant.TIntegrationType type)
		{
			return GetHandlerConfigs(key, type).FirstOrDefault(x => x.Handler == handlerName);
		}
		public static MappingConfig GetMappingConfig(string key)
		{
			return GetSettings<MappingConfig>("MappingConfig").FirstOrDefault(x => x.Id == key);
		}
		public static MappingConfig GetMappingConfigById(string mappingId)
		{
			return GetSettings<MappingConfig>("MappingConfig").FirstOrDefault(x => x.Id == mappingId);
		}
		public static ConfigSetting GetHandlerConfigById(string configId)
		{
			return GetSettings<ConfigSetting>("ConfigSetting").FirstOrDefault(x => x.Id == configId);
		}
		public static MappingConfig GetMappingConfigByRoute(string key, string handlerName, CsConstant.TIntegrationType type)
		{
			var configSetting = GetHandlerConfigs(key, type);
			if (configSetting != null)
			{
				var config = configSetting.FirstOrDefault(x => x.Handler == handlerName);
				if (config != null)
				{
					return GetMappingConfig(config.DefaultMappingConfig);
				}
			}
			return null;
		}
		public static ServiceConfig GetServiceConfig(string key)
		{
			return GetSettings<ServiceConfig>("ServiceConfig").FirstOrDefault(x => x.Id == key);
		}
		public static ServiceMockConfig GetServiceMockConfig(string key)
		{
			return GetSettings<ServiceMockConfig>("ServiceMockConfig").FirstOrDefault(x => x.Id == key);
		}
		public static TemplateSetting GetTemplatesConfig(string key)
		{
			return GetSettings<TemplateSetting>("TemplateConfig").FirstOrDefault(x => x.Name == key);
		}
		public static List<TriggerSetting> GetTriggersConfig()
		{
			return GetSettings<TriggerSetting>("TriggerConfig").ToList();
		}
		public static List<EndPointConfig> GetEndPointConfigs()
		{
			return GetSettings<EndPointConfig>("EndPointConfig").ToList();
		}
		public static EndPointConfig GetEndPointConfig(string name)
		{
			return GetSettings<EndPointConfig>("EndPointConfig").FirstOrDefault(x => x.Name == name);
		}
		public static EndPointConfig GetEndPointConfigByHandler(string name)
		{
			return GetSettings<EndPointConfig>("EndPointConfig").FirstOrDefault(x => x.Name == name);
		}
		public static LogItemConfig GetLogConfig(string name)
		{
			return GetSettings<LogItemConfig>("LogConfig").FirstOrDefault(x => x.Name == name);
		}
		public static T GetSetting<T>(ConcurrentDictionary<string, ValueType> Settings, string settingName)
		{
			if (Settings == null)
			{
				return default(T);
			}
			ValueType setting;
			if (Integration.TryGetValue(settingName, out setting))
			{
				return ObjToGenericClass<T>(setting);
			}
			return default(T);
		}
		public static T ObjToGenericClass<T>(object obj)
		{
			if (obj is T)
			{
				return (T)obj;
			}
			else
			{
				try
				{
					return (T)Convert.ChangeType(obj, typeof(T));
				}
				catch (Exception e)
				{
					return default(T);
				}
			}
		}
		#region Init Settings
		public static ConcurrentDictionary<string, ValueType> InitIntegrationSettings()
		{
			if (_integration != null)
			{
				return _integration;
			}
			//var infoDictionary = InitIntegrationId();
			//InitXmlConfig();
			//RegisterHandlers();
			return new ConcurrentDictionary<string, ValueType>();
		}
		public static void ReinitXmlConfigSettings()
		{
			if (ConfigSettings != null)
			{
				ConfigSettings.Clear();
			}
			if (MappingConfig != null)
			{
				MappingConfig.Clear();
			}
			_settingProvider.Reinit();
		}
		public static void RegisterHandlers()
		{
			var attributeType = typeof(IntegrationHandlerAttribute);
			var assembly = typeof(BaseEntityHandler).Assembly;
			Handlers = assembly
				.GetTypes()
				.Where(x => x.GetCustomAttributes(attributeType, true).Any())
				.ToList();
		}
		#endregion

		#region Private: Methods
		private static TIntegrationObjectType GetIntegrationObjectTypeById(Guid integrObjectTypeId)
		{
			if (integrObjectTypeId == CsConstant.IntegartionObjectTypeIds.Json)
			{
				return TIntegrationObjectType.Json;
			}
			else if (integrObjectTypeId == CsConstant.IntegartionObjectTypeIds.Xml)
			{
				return TIntegrationObjectType.Xml;
			}
			else
			{
				throw new Exception("Невозможно распознать тип интеграции!");
			}
		}
		#endregion



		#region UserConnection

		public static UserConnection UserConnection;

		#endregion
	}
}