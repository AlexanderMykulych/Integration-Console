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
					routeConfig = XmlConfigManager.IntegrationConfig.ExportRouteConfig.Where(x => x.Key == key).ToList();
					break;
				default:
					routeConfig = XmlConfigManager.IntegrationConfig.ImportRouteConfig.Where(x => x.Key == key).ToList();
					break;
			}
			if (routeConfig != null)
			{
				var config = XmlConfigManager.IntegrationConfig.ConfigSetting.Where(x => routeConfig.Any(y => y.ConfigId == x.Id)).ToList();
				ConfigSettings.TryAdd(key, config);
				return config;
			}
			return new List<ConfigSetting>();
		}
		public static IEnumerable<RouteConfig> GetExportRoutes(string key)
		{
			return XmlConfigManager.IntegrationConfig.ExportRouteConfig.Where(x => x.Key == key);
		}
		public static IEnumerable<RouteConfig> GetImportRoutes(string key)
		{
			return XmlConfigManager.IntegrationConfig.ImportRouteConfig.Where(x => x.Key == key);
		}
		public static ConfigSetting GetHandlerConfig(string key, string handlerName, CsConstant.TIntegrationType type)
		{
			return GetHandlerConfigs(key, type).FirstOrDefault(x => x.Handler == handlerName);
		}
		public static MappingConfig GetMappingConfig(string key)
		{
			if (MappingConfig == null)
			{
				MappingConfig = new ConcurrentDictionary<string, MappingConfig>();
			}
			if (MappingConfig.ContainsKey(key))
			{
				return MappingConfig[key];
			}
			var mappingConfig = XmlConfigManager.IntegrationConfig.MappingConfig.FirstOrDefault(x => x.Id == key);
			if (mappingConfig != null)
			{
				MappingConfig.TryAdd(key, mappingConfig);
				return mappingConfig;
			}
			return null;
		}
		public static MappingConfig GetMappingConfigById(string mappingId)
		{
			return XmlConfigManager.IntegrationConfig.MappingConfig.FirstOrDefault(x => x.Id == mappingId);
		}
		public static ConfigSetting GetHandlerConfigById(string configId)
		{
			return XmlConfigManager.IntegrationConfig.ConfigSetting.FirstOrDefault(x => x.Id == configId);
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
			return XmlConfigManager.IntegrationConfig.ServiceConfig.FirstOrDefault(x => x.Id == key);
		}
		public static ServiceMockConfig GetServiceMockConfig(string key)
		{
			return XmlConfigManager.IntegrationConfig.ServiceMockConfig.FirstOrDefault(x => x.Id == key);
		}
		public static TemplateSetting GetTemplatesConfig(string key)
		{
			return XmlConfigManager.GetTemplateConfig(key);
		}
		public static List<TriggerSetting> GetTriggersConfig()
		{
			return XmlConfigManager.IntegrationConfig.TriggerConfig;
		}
		public static List<EndPointConfig> GetEndPointConfigs()
		{
			return XmlConfigManager.IntegrationConfig.EndPointConfig;
		}
		public static EndPointConfig GetEndPointConfig(string name)
		{
			return XmlConfigManager.IntegrationConfig.EndPointConfig.FirstOrDefault(x => x.Name == name);
		}
		public static EndPointConfig GetEndPointConfigByHandler(string name)
		{
			return XmlConfigManager.IntegrationConfig.EndPointConfig.FirstOrDefault(x => x.EndPointHandler == name);
		}
		public static LogItemConfig GetLogConfig(string name)
		{
			if (XmlConfigManager.IntegrationConfig != null && XmlConfigManager.IntegrationConfig.LogConfig != null)
			{
				return XmlConfigManager.IntegrationConfig.LogConfig.FirstOrDefault(x => x.Name == name);
			}
			return null;
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
			var infoDictionary = InitIntegrationId();
			InitXmlConfig();
			RegisterHandlers();
			return infoDictionary;
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
			XmlConfigManager.IsConfigInited = false;
			InitXmlConfig();
		}
		public static ConcurrentDictionary<string, ValueType> InitIntegrationId()
		{
			var addData = new ConcurrentDictionary<string, ValueType>();
			var select = new Select(UserConnection)
				.Top(1)
				.Column("Id")
				.Column("TsIsActive")
				.Column("TsIsDebugMode")
				.Column("TsIntegrObjectTypeId")
				.From("TsIntegration")
				.Where("TsIsActive").IsEqual(Column.Const(1)) as Select;
			using (var dBExecutor = UserConnection.EnsureDBConnection())
			{
				using (var reader = select.ExecuteReader(dBExecutor))
				{
					if (reader.Read())
					{
						IntegrationId = reader.GetColumnValue<Guid>("Id");
						addData.TryAdd("TsIsDebugMode", reader.GetColumnValue<bool>("TsIsDebugMode"));
						addData.TryAdd("TsIsActive", reader.GetColumnValue<bool>("TsIsActive"));
						addData.TryAdd("TsIntegrationObjectType", GetIntegrationObjectTypeById(reader.GetColumnValue<Guid>("TsIntegrObjectTypeId")));
					}
				}
			}
			return addData;
		}
		public static void InitXmlConfig()
		{
			var xmlData = GetXmlConfigData();
			XmlConfigManager.InitLoadConfig(xmlData);
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
		public static string GetXmlConfigData()
		{
			var select = new Select(UserConnection)
				.Column("tmc", "TsXmlConfig")
				.From("TsMappingConfig").As("tmc")
				.InnerJoin("TsIntegrMapping").As("tim")
				.On("tim", "TsMappingConfigId").IsEqual("tmc", "Id")
				.Where("tim", "TsIntegrationId").IsEqual(Column.Parameter(IntegrationId)) as Select;
			using (var dBExecutor = UserConnection.EnsureDBConnection())
			{
				using (var reader = select.ExecuteReader(dBExecutor))
				{
					if (reader.Read())
					{
						return reader.GetColumnValue<string>("TsXmlConfig");
					}
				}
			}
			return string.Empty;
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