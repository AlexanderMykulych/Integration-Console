using EntitySchema = Terrasoft.Core.Entities.EntitySchema;
using IntegrationInfo = Terrasoft.TsIntegration.Configuration.CsConstant.IntegrationInfo;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Ninject.Infrastructure.Language;
using ServiceStack.ServiceInterface.ServiceModel;
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
using Terrasoft.Core.Process;
using Terrasoft.Core;
using Terrasoft.Nui.ServiceModel.DataContract;
using Terrasoft.UI.WebControls;
using TIntegrationType = Terrasoft.TsIntegration.Configuration.CsConstant.TIntegrationType;

namespace Terrasoft.TsIntegration.Configuration {

	#region Class: ConfigSetting
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Configuration\Model\ConfigSetting\ConfigSetting.cs
		
	*/
	public partial class ConfigSetting
	{
		public string Handler { get; set; }
		public string Url { get; set; }
		public string DefaultMappingConfig { get; set; }
		public string Id { get; set; }
		public string JName { get; set; }
		public string EntityName { get; set; }
		public string ExternalIdPath { get; set; }
		public string JsonIdPath { get; set; }
		public string Auth { get; set; }
		public TIntegrationObjectType IntegrationObjectType { get; set; }
		public string Service { get; set; }
		public string ResponseRoute { get; set; }
		public string ResponseMappingConfig { get; set; }
		public string ResponseJName { get; set; }
		public string TemplateName { get; set; }
		public object AdditionalParameter = null;
		public List<HandlerConfig> HandlerConfigs;
	}

	#endregion


	#region Class: HandlerConfig
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Configuration\Model\ConfigSetting\HandlerConfig.cs
		
	*/
	public class HandlerConfig
	{
		public string Key { get; set; }
		public string Value { get; set; }
	}

	#endregion


	#region Class: EndPointConfig
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Configuration\Model\EndPointConfig\EndPointConfig.cs
		
	*/
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

	#endregion


	#region Class: EndPointHandlerConfig
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Configuration\Model\EndPointConfig\EndPointHandlerConfig.cs
		
	*/
	public class EndPointHandlerConfig
	{
		public string Key { get; set; }
		public string Value { get; set; }
	}

	#endregion


	#region Class: LogItemConfig
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Configuration\Model\LogConfig\LogItemConfig.cs
		
	*/
	public class LogItemConfig
	{
		public string Name { get; set; }
		public bool IsActive { get; set; }
		public bool IsLogParameter { get; set; }
	}

	#endregion


	#region Class: IntegrationConfig
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Configuration\Model\Main\IntegrationConfig.cs
		
	*/
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
	}

	#endregion


	#region Class: MappingConfig
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Configuration\Model\MappingConfig\MappingConfig.cs
		
	*/
	public class MappingConfig
	{
		public string Id { get; set; }
		public List<MappingItem> Items;
	}

	#endregion


	#region Class: MappingItem
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Configuration\Model\MappingItem\MappingItem.cs
		
	*/
	public partial class MappingItem
	{

		public string TsSourcePath { get; set; }
		public string JSourceName { get; set; }
		public string JSourcePath { get; set; }

		public string TsDestinationPath { get; set; }
		public string TsDestinationName { get; set; }
		public string TsDestinationResPath { get; set; }
		public string MapType { get; set; }
		public TMapExecuteType MapExecuteType { get; set; }
		public CsConstant.TIntegrationType MapIntegrationType { get; set; }
		public bool EFieldRequier { get; set; }

		public string TsExternalIdPath { get; set; }

		public object ConstValue { get; set; }
		public TConstType ConstType { get; set; }

		public bool IgnoreError { get; set; }
		public bool SaveOnResponse { get; set; }

		public string OrderColumn { get; set; }
		public Common.OrderDirection OrderType { get; set; }

		public string HandlerName { get; set; }

		public bool DeleteBeforeExport { get; set; }
		public string BeforeDeleteMacros { get; set; }

		public string MacrosName { get; set; }

		public string TsExternalSource { get; set; }
		public string TsExternalPath { get; set; }
		public string TsDestinationPathToSource { get; set; }
		public string TsDestinationPathToExternal { get; set; }
		public bool SerializeIfNull { get; set; }
		public bool SerializeIfZero { get; set; }
		public string TsDetailName { get; set; }
		public string TsDetailPath { get; set; }
		public string TsDetailResPath { get; set; }
		public string TsTag { get; set; }
		public string TsDetailTag { get; set; }
		public string OverRuleMacros { get; set; }
		public string Selector { get; set; }

		public bool CreateIfNotExist { get; set; }
		public bool AllowNullToOverMacros { get; set; }
		public bool IsAllowEmptyResult { get; set; }

		public bool LoadDependentEntity { get; set; }
		public string HandlerConfigId { get; set; }
		public string ImportRouteId { get; set; }
		public bool IsArrayItem { get; set; }
		public string TsMappingConfigId { get; set; }
		public string Value { get; set; }
		public bool IsParentMapp { get; set; }
		public MappingItem()
		{

		}
		public override string ToString()
		{
			return JsonConvert.SerializeObject(this).ToString();
		}

	}

	#endregion


	#region Class: PrerenderConfig
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Configuration\Model\PrerenderConfig\PrerenderConfig.cs
		
	*/
	public class PrerenderConfig
	{
		public string From { get; set; }
		public string To { get; set; }
	}

	#endregion


	#region Class: RouteConfig
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Configuration\Model\RouteConfig\RouteConfig.cs
		
	*/
	public class RouteConfig
	{
		public string Key { get; set; }
		public string ConfigId { get; set; }
		public bool IsSyncEnable { get; set; }
		public ulong SyncMilliseconds { get; set; }
	}

	#endregion


	#region Class: ServiceConfig
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Configuration\Model\ServiceConfig\ServiceConfig.cs
		
	*/
	public partial class ServiceConfig
	{
		public string Method { get; set; }
		public string Url { get; set; }
		public List<ServiceHeaderConfig> Headers { get; set; }
		public string Id { get; set; }
		public string Mock { get; set; }
		public string ServiceName { get; set; }
	}

	#endregion


	#region Class: ServiceHeaderConfig
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Configuration\Model\ServiceConfig\ServiceHeaderConfig.cs
		
	*/
	public partial class ServiceHeaderConfig
	{
		public string Key { get; set; }
		public string Value { get; set; }
	}

	#endregion


	#region Class: TemplateSetting
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Configuration\Model\TemplateSetting\TemplateSetting.cs
		
	*/
	public partial class TemplateSetting
	{
		public string Name { get; set; }
		public string Handler { get; set; }
		public Dictionary<string, string> Settings;
	}

	#endregion


	#region Class: TriggerSetting
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Configuration\Model\TriggerSetting\TriggerSetting.cs
		
	*/
	public partial class TriggerSetting
	{
		public string Name { get; set; }
		public string Caption { get; set; }
		public string EntitySchema { get; set; }
		public string Route { get; set; }
		public string Filter { get; set; }
		public bool OnInsert { get; set; }
		public bool OnUpdate { get; set; }
	}

	#endregion


	#region Class: EntitySchemaInfo
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Configuration\Test\DataModel\EntitySchemaInfo.cs
		
	*/
	[DataContract]
	public class EntitySchemaInfo
	{
		[DataMember(Name = "name")]
		public string Name;
		[DataMember(Name = "caption")]
		public string Caption;
	}

	#endregion


	#region Class: TestExportInfo
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Configuration\Test\DataModel\TestExportInfo.cs
		
	*/
	[DataContract]
	public class TestExportInfo
	{
		[DataMember(Name = "configId")]
		public string ConfigId;
		[DataMember(Name = "entityId")]
		public string EntityId;
	}

	#endregion


	#region Class: TestImportInfo
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Configuration\Test\DataModel\TestImportInfo.cs
		
	*/
	[DataContract]
	public class TestImportInfo
	{
		[DataMember(Name = "configId")]
		public string ConfigId;
		[DataMember(Name = "json")]
		public string Json;
		[DataMember(Name = "isUpdate")]
		public bool IsUpdate;
		[DataMember(Name = "isCreate")]
		public bool IsCreate;
		[DataMember(Name = "isExists")]
		public bool IsExists;
	}

	#endregion


	#region Class: TestServiceInfo
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Configuration\Test\DataModel\TestServiceInfo.cs
		
	*/
	[DataContract]
	public class TestServiceInfo
	{
		[DataMember(Name = "id")]
		public Guid Id;
		[DataMember(Name = "schemaName")]
		public string SchemaName;
		[DataMember(Name = "routeKey")]
		public string RouteKey;
		[DataMember(Name = "isUseMock")]
		public bool IsUseMock;
	}

	#endregion


	#region Class: TriggerServiceInfo
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Configuration\Test\DataModel\TriggerServiceInfo.cs
		
	*/
	[DataContract]
	public class TriggerServiceInfo
	{
		[DataMember]
		public string TriggerName;
		[DataMember]
		public string SchemaName;
		[DataMember]
		public Guid PrimaryColumnValue;
	}

	#endregion


	#region Class: TestScenarioProvider
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Configuration\Test\Logic\TestScenarioProvider.cs
		
	*/
	public class TestScenarioProvider
	{
		public StringBuilder LogBuilder;
		public int Count;
		public bool Enable;
		public TestScenarioProvider()
		{
			LogBuilder = new StringBuilder();
			Count = 0;
			Enable = true;
			LogBuilder.AppendLine("--Начало--");
		}
		public TestScenarioProvider Do(string name, Func<string> actionPredicate, bool stopIfError = true)
		{
			if (!Enable)
			{
				return this;
			}
			try
			{
				LogBuilder.AppendLineFormat("{0}. {1}", ++Count, name);
				var str = actionPredicate();
				if (!string.IsNullOrEmpty(str))
				{
					LogBuilder.AppendLine(str);
				}
			}
			catch (Exception e)
			{
				LogBuilder.AppendLineFormat("Ошибка: {0}", e);
				if (stopIfError)
				{
					Enable = false;
				}
			}
			return this;
		}

		public string End()
		{
			LogBuilder.AppendLine("--Конец--");
			return LogBuilder.ToString();
		}
	}

	#endregion


	#region Class: TsIntegrationCodeServiceHelper
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Configuration\Test\Logic\TsIntegrationCodeServiceHelper.cs
		
	*/
	public class TsIntegrationCodeServiceHelper
	{
		private global::Common.Logging.ILog _log;

		public global::Common.Logging.ILog Log {
			get {
				if (_log == null)
				{
					_log = global::Common.Logging.LogManager.GetLogger("TscIntegration") ??
						global::Common.Logging.LogManager.GetLogger("Common");
				}
				return _log;
			}
		}
		private IIntegrationObjectProvider _integrationObjectProvider;
		public IIntegrationObjectProvider IntegrationObjectProvider {
			set {
				_integrationObjectProvider = value;
			}
			get {
				if (_integrationObjectProvider == null)
				{
					_integrationObjectProvider = new IntegrationObjectProvider();
				}
				return _integrationObjectProvider;
			}
		}
		public UserConnection userConnection;
		public const string XmlHeaderStr = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n";
		public TsIntegrationCodeServiceHelper(UserConnection userConnection)
		{
			this.userConnection = userConnection;
		}

		public void ReinitSettings()
		{
			SettingsManager.UserConnection = userConnection;
			SettingsManager.InitIntegrationSettings();
			SettingsManager.ReinitXmlConfigSettings();
			TriggerEngine.ClearTriggerCollection();
		}
		#region Service Mock
		public void TestServiceByMock(TestServiceInfo info)
		{
			SettingsManager.UserConnection = userConnection;
			SettingsManager.InitIntegrationSettings();
			SettingsManager.ReinitXmlConfigSettings();
			LoggerHelper.DoInLogBlock("Экспорт (Mock)", () =>
			{
				IIntegrator integrator = null;
				if (info.IsUseMock)
				{
					integrator = ClassFactory.Get<BaseIntegratorMock>();
				}
				else
				{
					integrator = ClassFactory.Get<BaseIntegrator>();
				}
				integrator.ExportWithRequest(userConnection, info.Id, info.SchemaName, info.RouteKey);
			});
		}
		#endregion

		#region Log Analyze
		public string GetBlockLogDataForAnalyze(Guid blockId)
		{
			var sqlText = @"select
				tsl.""Id"",
				tsl.""TsParentId"",
				tsl.""TsText"",
				tsl.""TsType"",
				tsl.""CreatedOn""
			  from
				""TsIntegrationCoreLog"" tsl
			  start with ""Id"" = :blockId
			  connect by  ""TsParentId"" = prior ""Id""
			  order by tsl.""CreatedOn""";
			var customQuery = new CustomQuery(userConnection, sqlText);
			customQuery.WithParameter("blockId", blockId);
			var result = new List<Dictionary<string, string>>();
			using (var dbExecutor = userConnection.EnsureDBConnection())
			{
				using (var reader = customQuery.ExecuteReader(dbExecutor))
				{
					while (reader.Read())
					{
						var dict = new Dictionary<string, string>()
						{
							{ "id", reader.GetColumnValue<string>("Id") },
							{ "parentId", reader.GetColumnValue<string>("TsParentId") },
							{ "text", reader.GetColumnValue<string>("TsText") },
							{ "type", reader.GetColumnValue<string>("TsType") },
							{ "createdOn", reader.GetColumnValue<DateTime>("CreatedOn").ToString("o") },
						};
						result.Add(dict);
					}
				}
			}
			return JsonConvert.SerializeObject(new
			{
				data = result
			}, Newtonsoft.Json.Formatting.Indented);
		}
		#endregion

		#region EntityShema Helper
		public List<EntitySchemaInfo> GetAllEntityNames()
		{
			var result = new List<EntitySchemaInfo>();
			try
			{
				var selectEntitySchema = new Select(userConnection)
									.Distinct()
									.Column("Caption")
									.Column("Name")
									.From("SysSchema")
									.Where("ExtendParent").IsEqual(Column.Parameter(false))
									.And("ManagerName").IsEqual(Column.Parameter("EntitySchemaManager"))
									.OrderByAsc("Caption") as Select;
				selectEntitySchema.ExecuteReader(dataReader =>
				{
					result.Add(new EntitySchemaInfo()
					{
						Name = dataReader.GetColumnValue<string>("Name"),
						Caption = dataReader.GetColumnValue<string>("Caption")
					});
				});
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
			return result;
		}
		#endregion

		#region Testing To Json
		public string TestToJson(TestExportInfo info)
		{
			var scenarioProvider = new TestScenarioProvider();
			SettingsManager.UserConnection = userConnection;
			SettingsManager.InitIntegrationSettings();
			SettingsManager.ReinitXmlConfigSettings();
			string result = string.Empty;
			LoggerHelper.DoInLogBlock("Test To Json", () =>
			{
				IntegrationLogger.Info(string.Format("Parameters\nConfig Id: {0}\nEntity Id: {1}", info.ConfigId, info.EntityId));
				result = scenarioProvider
					.Do(string.Format("Поиск конфига({0})", info.ConfigId), FindConfigAction(info.ConfigId))
					.Do("Поиск обработчика", FindHandlerAction(info.ConfigId))
					.Do("Поиск маппинга", FindMappingAction(info.ConfigId))
					.Do("Приведение к Json/Xml", TestHandlerToJsonAction(info))
				.End();
			});
			return result;
		}
		public Func<string> FindConfigAction(string configId)
		{
			return () =>
			{
				var config = SettingsManager.GetHandlerConfigById(configId);
				if (config != null)
				{
					return string.Format("Конфиг найден!\nConfigId={0}\nHandler={1}\nEntityName={2}\nJName={3}\nUrl={4}\nAuth={5}\n",
						config.Id, config.Handler, config.EntityName, config.JName, config.Url, config.Auth);
				}
				throw new Exception("Конфиг не найден!");
			};
		}
		public Func<string> FindHandlerAction(string configId)
		{
			return () =>
			{
				var config = SettingsManager.GetHandlerConfigById(configId);
				if (config != null)
				{
					string result = string.Empty;
					var entityHelper = new IntegrationEntityHelper();
					var handlers = entityHelper.GetAllIntegrationHandler(new List<ConfigSetting>() { config });
					if (handlers != null && handlers.Any())
					{
						foreach (var handler in handlers)
						{
							result += "Обработчик: " + handler.GetType().ToString() + "\n";
						}
						return result;
					}
				}
				throw new Exception("Обработчиков не найден");
			};
		}
		public Func<string> FindMappingAction(string сonfigId)
		{
			return () =>
			{
				var config = SettingsManager.GetHandlerConfigById(сonfigId);
				if (config != null)
				{
					var mapping = SettingsManager.GetMappingConfigById(config.DefaultMappingConfig);
					if (mapping != null)
					{
						return string.Format("Маппинг найден:\nId={0}\nКоличество элементов={1}", mapping.Id, mapping.Items.Count());
					}
				}
				throw new Exception("Маппинг не найден!");
			};
		}
		public Func<string> TestHandlerToJsonAction(TestExportInfo info)
		{
			return () =>
			{
				var config = SettingsManager.GetHandlerConfigById(info.ConfigId);
				if (config != null)
				{
					var builder = new StringBuilder();
					var entityHelper = new IntegrationEntityHelper();
					var handlers = entityHelper.GetAllIntegrationHandler(new List<ConfigSetting>() { config });
					if (handlers != null && handlers.Any())
					{
						var esq = new EntitySchemaQuery(userConnection.EntitySchemaManager, config.EntityName);
						esq.AddAllSchemaColumns();
						var entity = esq.GetEntity(userConnection, info.EntityId);
						if (entity != null)
						{
							builder.AppendLineFormat("Объект найден: Display Value{0}\n", entity.PrimaryDisplayColumnValue);
							foreach (var handler in handlers)
							{
								builder.AppendLineFormat("Обработчик: {0}", handler.GetType());
								var integrationInfo = CsConstant.IntegrationInfo.CreateForExport(userConnection, entity);
								builder.AppendLineFormat("Результат: {0}", handler.ToJson(integrationInfo));
							}
							return builder.ToString();
						}
						else
						{
							throw new Exception(string.Format("Объект {0} c id {1} не найден", config.EntityName, info.EntityId));
						}
					}
					else
					{
						throw new Exception("Обработчик не найден");
					}
				}
				return "";
			};
		}
		#endregion

		#region Testing To Entity
		public string TestToEntity(TestImportInfo info)
		{
			var scenarioProvider = new TestScenarioProvider();
			SettingsManager.UserConnection = userConnection;
			SettingsManager.InitIntegrationSettings();
			SettingsManager.ReinitXmlConfigSettings();
			string result = string.Empty;
			LoggerHelper.DoInLogBlock("Test To Entity", () =>
			{
				IntegrationLogger.Info(string.Format("Parameters\nConfig Id: {0}\nJson\\Xml:\n{4}\nIs Create: {1}\nSearch: {2}\nIs Update: {3}",
					info.ConfigId, info.IsCreate, info.IsExists, info.IsUpdate, info.Json));
				result = scenarioProvider
					.Do(string.Format("Поиск конфига({0})", info.ConfigId), FindConfigAction(info.ConfigId))
					.Do("Поиск обработчика", FindHandlerAction(info.ConfigId))
					.Do("Поиск маппинга", FindHandlerAction(info.ConfigId))
					.Do("Обработка Json/Xml", TestHandlerProcessJson(info))
				.End();
			});
			return result;
		}
		public Func<string> TestHandlerProcessJson(TestImportInfo info)
		{
			return () =>
			{
				var config = SettingsManager.GetHandlerConfigById(info.ConfigId);
				if (config != null)
				{
					var builder = new StringBuilder();
					var entityHelper = new IntegrationEntityHelper();
					var handlers = entityHelper.GetAllIntegrationHandler(new List<ConfigSetting>() { config });
					IIntegrationObject jObj = null;
					try
					{
						jObj = IntegrationObjectProvider.Parse(info.Json);
					}
					catch (Exception e)
					{
						throw new Exception("Возникла ошибка при парсинге Json/Xml");
					}
					foreach (var handler in handlers)
					{
						builder.AppendLineFormat("Обработчик: {0}", handler.GetType());
						if (info.IsCreate)
						{
							try
							{
								var integrationInfoCreate = CsConstant.IntegrationInfo.CreateForImport(userConnection, CsConstant.IntegrationActionName.Create, jObj);
								handler.Create(integrationInfoCreate);
								if (integrationInfoCreate != null && integrationInfoCreate.IntegratedEntity != null)
								{
									builder.AppendLineFormat("Идентификатор объекта: {0}", integrationInfoCreate.IntegratedEntity.PrimaryColumnValue);
								}
							}
							catch (Exception e)
							{
								builder.AppendLineFormat("Ошибка: {0}", e);
							}
						}
						bool isEntityExist = false;
						if (info.IsExists)
						{
							var integrationInfoExists = CsConstant.IntegrationInfo.CreateForImport(userConnection, CsConstant.IntegrationActionName.Create, jObj);
							isEntityExist = handler.IsEntityAlreadyExist(integrationInfoExists);
							builder.AppendLineFormat("Результат поиска: {0}", isEntityExist);
						}
						if (info.IsUpdate)
						{
							var integrationInfoUpdate = CsConstant.IntegrationInfo.CreateForImport(userConnection, CsConstant.IntegrationActionName.Update, jObj);
							if (!isEntityExist && !info.IsExists)
							{
								isEntityExist = handler.IsEntityAlreadyExist(integrationInfoUpdate);
							}
							if (isEntityExist)
							{
								handler.Update(integrationInfoUpdate);
								builder.AppendLineFormat("Идентификатор объекта: {0}", integrationInfoUpdate.IntegratedEntity.PrimaryColumnValue);
							}
							else
							{
								builder.AppendLine("Ошибка: Объект еще не существует. Невозможно обновить!");
							}
						}
					}
					return builder.ToString();
				}
				throw new Exception("Конфиг не найден!");
			};
		}
		#endregion

		#region Work with config
		public string GetXmlConfigFromJson(string configJson, out string name)
		{
			try
			{
				var jObj = JObject.Parse(configJson);
				if (jObj == null)
				{
					name = string.Empty;
					return string.Empty;
				}
				var config = new XmlDocument();
				var rootEl = config.AppendChild(config.CreateElement("MapingConfiguration"));
				InitPrerenderConfigXml(jObj.SelectToken("PrepareModuleValue"), rootEl);
				InitMappingConfigXml(jObj.SelectToken("MappingConfig"), rootEl);
				InitMappingConfigXml(jObj.SelectToken("DefaultMappingConfig"), rootEl);
				InitMappingConfigXml(jObj.SelectToken("DefaultByTypeMappingConfig"), rootEl);
				InitImportRoutesXml(jObj.SelectToken("ImportRoutes"), rootEl);
				InitExportRoutesXml(jObj.SelectToken("ExportRoutes"), rootEl);
				InitMockConfigXml(jObj.SelectToken("MockConfig"), rootEl);
				InitServiceConfigXml(jObj.SelectToken("ServiceConfig"), rootEl);
				InitConfigurationItemModuleValueXml(jObj.SelectToken("ConfigurationItemModuleValue"), rootEl);
				InitTriggerSettingsXml(jObj.SelectToken("TriggerSettings"), rootEl);
				InitTemplateConfigXml(jObj.SelectToken("TemplateSettings"), rootEl);
				InitEndPointConfigXml(jObj.SelectToken("EndPointConfig"), rootEl);
				InitLogConfig(jObj.SelectToken("LogConfig"), rootEl);
				name = jObj.SelectToken("Caption").Value<string>();
				return XmlHeaderStr + config.InnerXml;
			}
			catch (Exception e)
			{
				Log.Error(e);
				throw;
			}
		}


		public string GetJsonConfigFromXml(string configXml, string name)
		{
			try
			{
				if (!string.IsNullOrEmpty(configXml))
				{
					var xmlDoc = XDocument.Parse(configXml);
					var jObj = new JObject();
					jObj.Add("Caption", new JValue(name));
					InitPrerenderConfigJson(xmlDoc.XPathSelectElement("/MapingConfiguration/prerenderConfig"), jObj);
					InitExportRoutesJson(xmlDoc.XPathSelectElement("/MapingConfiguration/ExportRoutes"), jObj);
					InitImportRoutesJson(xmlDoc.XPathSelectElement("/MapingConfiguration/ImportRoutes"), jObj);
					InitConfigurationItemModuleValueJson(xmlDoc.XPathSelectElements("/MapingConfiguration/config"), jObj);
					InitMappingConfigJson(xmlDoc.XPathSelectElements("/MapingConfiguration/mappingConfig[@Id!=\"Default\" and @Id!=\"DefaultByMappingType\"]"), jObj);
					InitMappingConfigJson(xmlDoc.XPathSelectElements("/MapingConfiguration/mappingConfig[@Id=\"Default\"]"), jObj, "DefaultMappingConfig");
					InitMappingConfigJson(xmlDoc.XPathSelectElements("/MapingConfiguration/mappingConfig[@Id=\"DefaultByMappingType\"]"), jObj, "DefaultByTypeMappingConfig");
					InitMockConfigJson(xmlDoc.XPathSelectElements("/MapingConfiguration/serviceMockConfig"), jObj);
					InitTemplateConfigJson(xmlDoc.XPathSelectElements("/MapingConfiguration/templateConfig"), jObj);
					InitTriggerConfigJson(xmlDoc.XPathSelectElements("/MapingConfiguration/TriggerSettings"), jObj);
					InitServiceConfigJson(xmlDoc.XPathSelectElements("/MapingConfiguration/serviceConfig"), jObj);
					InitEndPointConfigJson(xmlDoc.XPathSelectElements("/MapingConfiguration/endPointConfig"), jObj);
					InitLogConfigJson(xmlDoc.XPathSelectElements("/MapingConfiguration/logConfig"), jObj);
					return jObj.ToString();
				}
				return string.Empty;
			}
			catch (Exception e)
			{
				Log.Error(e);
				throw;
			}
		}



		public Guid CreateNewConfig(string configJson, Guid integrationId)
		{
			try
			{
				string name;
				var xml = GetXmlConfigFromJson(configJson, out name);
				if (!string.IsNullOrEmpty(xml))
				{
					var mappingConfigId = Guid.NewGuid();
					var insertMappingConfig = new Insert(userConnection)
						.Into("TsMappingConfig")
						.Set("Id", Column.Parameter(mappingConfigId))
						.Set("TsXmlConfig", Column.Parameter(xml))
						.Set("TsName", Column.Parameter(name));
					insertMappingConfig.Execute();
					var insertMappingInIntegration = new Insert(userConnection)
						.Into("TsIntegrMapping")
						.Set("TsIntegrationId", Column.Parameter(integrationId))
						.Set("TsMappingConfigId", Column.Parameter(mappingConfigId));
					insertMappingInIntegration.Execute();
					return mappingConfigId;
				}
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
			return Guid.Empty;
		}

		public string GetConfigByJson(Guid configId)
		{
			try
			{
				var select = new Select(userConnection)
						.Top(1)
						.Column("TsName")
						.Column("TsXmlConfig")
						.From("TsMappingConfig")
						.Where("Id").IsEqual(Column.Parameter(configId)) as Select;
				string xmlConfig = string.Empty, name = string.Empty;
				using (var dbExecutor = userConnection.EnsureDBConnection())
				{
					using (var reader = select.ExecuteReader(dbExecutor))
					{
						if (reader.Read())
						{
							xmlConfig = reader.GetColumnValue<string>("TsXmlConfig");
							name = reader.GetColumnValue<string>("TsName");
						}
					}
				}
				if (!string.IsNullOrEmpty(xmlConfig))
				{
					return GetJsonConfigFromXml(xmlConfig, name);
				}
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
			return String.Empty;
		}
		public void UpdateConfig(string configJson, Guid configId)
		{
			try
			{
				string name;
				var xml = GetXmlConfigFromJson(configJson, out name);
				if (!string.IsNullOrEmpty(xml))
				{
					var updateMappingConfig = new Update(userConnection, "TsMappingConfig")
						.Set("TsXmlConfig", Column.Parameter(xml))
						.Set("TsName", Column.Parameter(name))
						.Where("Id").IsEqual(Column.Parameter(configId));
					updateMappingConfig.Execute();
				}
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
		}

		

		private void InitServiceConfigXml(JToken jToken, XmlNode config)
		{
			InitNestingConfigXml(jToken, config, "serviceConfig", "serviceHeaderConfig");
		}
		private void InitEndPointConfigXml(JToken jToken, XmlNode config)
		{
			InitNestingConfigXml(jToken, config, "endPointConfig", "handlerConfig");
		}
		private void InitMockConfigXml(JToken jToken, XmlNode config)
		{
			InitSimpleConfigXml(jToken, config, "serviceMockConfig");
		}
		private void InitTemplateConfigXml(JToken jToken, XmlNode config)
		{
			InitNestingConfigXml(jToken, config, "templateConfig", "setting");
		}
		private void InitNestingConfigXml(JToken jToken, XmlNode config, string rootName, string nestName)
		{
			try
			{
				if (jToken == null)
				{
					return;
				}
				foreach (var configs in jToken)
				{
					var mappingConfig = (XmlElement)config.AppendChild(config.OwnerDocument.CreateElement(rootName));
					foreach (var configToken in configs.First)
					{
						if (configToken is JProperty)
						{
							var configProp = (JProperty)configToken;
							if (configProp.Value is JValue)
							{
								mappingConfig.SetAttribute(configProp.Name, ((JValue)configProp.Value).Value<string>());
							}
							else
							{
								foreach (var configItemToken in configToken.First)
								{
									var mappingItem = (XmlElement)mappingConfig.AppendChild(config.OwnerDocument.CreateElement(nestName));
									foreach (var configAttr in configItemToken.First)
									{
										if (configAttr is JProperty)
										{
											var configAttrProp = (JProperty)configAttr;
											mappingItem.SetAttribute(configAttrProp.Name, ((JValue)configAttrProp.Value).Value<string>());
										}
									}
								}
							}
						}
					}
				}
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
		}
		private void InitSimpleConfigXml(JToken jToken, XmlNode config, string elName)
		{
			try
			{
				if (jToken == null)
				{
					return;
				}
				foreach (var jConfig in jToken)
				{
					if (jConfig is JProperty)
					{
						var configObj = ((JProperty)jConfig).Value as JObject;
						if (configObj != null)
						{
							var configElement = (XmlElement)config.AppendChild(config.OwnerDocument.CreateElement(elName));
							foreach (var jConfAttr in configObj)
							{
								if (jConfAttr.Value is JValue)
								{
									configElement.SetAttribute(jConfAttr.Key, ((JValue)jConfAttr.Value).Value<string>());
								}
							}
						}
					}
				}
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
		}
		
		#endregion

		#region To Xml
		public void InitPrerenderConfigXml(JToken jToken, XmlNode config)
		{
			try
			{
				if (jToken == null)
				{
					return;
				}
				InitXmlAttrByJToken(jToken, config, "prerenderConfig", "render");
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
		}
		public void InitImportRoutesXml(JToken jToken, XmlNode config)
		{
			if (jToken == null)
			{
				return;
			}
			InitXmlAttrByJToken(jToken, config, "ImportRoutes", "route");
		}
		public void InitExportRoutesXml(JToken jToken, XmlNode config)
		{
			if (jToken == null)
			{
				return;
			}
			InitXmlAttrByJToken(jToken, config, "ExportRoutes", "route");
		}
		public void InitConfigurationItemModuleValueXml(JToken jToken, XmlNode config)
		{
			//InitSimpleConfigXml(jToken, config, "config");
			InitNestingConfigXml(jToken, config, "config", "handlerConfig");
		}
		private void InitTriggerSettingsXml(JToken jToken, XmlNode config)
		{
			InitSimpleConfigXml(jToken, config, "TriggerSettings");
		}


		private void InitLogConfig(JToken jToken, XmlNode config)
		{
			if (jToken == null)
			{
				return;
			}
			InitSimpleConfigXml(jToken, config, "LogConfig");
		}
		public void InitMappingConfigXml(JToken jToken, XmlNode config)
		{
			try
			{
				if (jToken == null)
				{
					return;
				}
				foreach (var configs in jToken)
				{
					var mappingConfig = (XmlElement)config.AppendChild(config.OwnerDocument.CreateElement("mappingConfig"));
					foreach (var configToken in configs.First)
					{
						if (configToken is JProperty)
						{
							var configProp = (JProperty)configToken;
							if (configProp.Value is JValue)
							{
								mappingConfig.SetAttribute(configProp.Name, ((JValue)configProp.Value).Value<string>());
							}
							else
							{
								foreach (var configItemToken in configToken.First)
								{
									var mappingItem = (XmlElement)mappingConfig.AppendChild(config.OwnerDocument.CreateElement("mappingItem"));
									foreach (var configAttr in configItemToken.First)
									{
										if (configAttr is JProperty)
										{
											var configAttrProp = (JProperty)configAttr;
											mappingItem.SetAttribute(configAttrProp.Name, ((JValue)configAttrProp.Value).Value<string>());
										}
									}
								}
							}
						}
					}
				}
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
		}
		public void InitXmlAttrByJToken(JToken jToken, XmlNode config, string rootName, string propName)
		{
			try
			{
				var prerenderConfig = config.AppendChild(config.OwnerDocument.CreateElement(rootName));
				foreach (var property in jToken)
				{
					var renderEl = (XmlElement)prerenderConfig.AppendChild(config.OwnerDocument.CreateElement(propName));
					foreach (var jAttr in property.First)
					{
						if (jAttr is JProperty && jAttr.HasValues)
						{
							var jProp = (JProperty)jAttr;
							renderEl.SetAttribute(jProp.Name, ((JValue)jProp.Value).ToString());
						}
					}
				}
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
		}
		#endregion

		#region To Json

		public void InitPrerenderConfigJson(XElement xElement, JObject jObj)
		{
			InitJsonTokenByXmlAttr(xElement, jObj, "PrepareModuleValue", "render");
		}

		public void InitExportRoutesJson(XElement xElement, JObject jObj)
		{
			InitJsonTokenByXmlAttr(xElement, jObj, "ExportRoutes", "route");
		}
		public void InitImportRoutesJson(XElement xElement, JObject jObj)
		{
			InitJsonTokenByXmlAttr(xElement, jObj, "ImportRoutes", "route");
		}
		public void InitTriggerConfigJson(IEnumerable<XElement> xElements, JObject jObj)
		{
			InitSimpleJson(xElements, jObj, "TriggerSettings");
		}

		private void InitLogConfigJson(IEnumerable<XElement> xElements, JObject jObj)
		{
			InitSimpleJson(xElements, jObj, "LogConfig");
		}
		public void InitConfigurationItemModuleValueJson(IEnumerable<XElement> xElements, JObject jObj)
		{
			//InitSimpleJson(xElements, jObj, "ConfigurationItemModuleValue");
			InitNestingConfigJson(xElements, jObj, "ConfigurationItemModuleValue", "HandlerConfigs", "handlerConfig");
		}
		public void InitSimpleJson(IEnumerable<XElement> xElements, JObject jObj, string name)
		{
			if (xElements != null && xElements.Any() && jObj != null)
			{
				var jItem = new JObject();
				jObj.Add(name, jItem);
				int index = 0;
				xElements
					.ForEach(xElement =>
					{
						var jItemObj = new JObject();
						jItem.Add((index++).ToString(), jItemObj);
						xElement
							.Attributes()
							.ForEach(xAttr => jItemObj.Add(xAttr.Name.LocalName, xAttr.Value));
					});
			}
		}
		public void InitJsonTokenByXmlAttr(XElement xElement, JObject jObj, string rootName, string propName)
		{
			if (xElement != null && jObj != null)
			{
				var prepareJObj = new JObject();
				jObj.Add(rootName, prepareJObj);
				int index = 0;
				xElement
					.XPathSelectElements(propName)
					.ForEach(renderElement =>
					{
						var renderJObj = new JObject();
						prepareJObj.Add((index++).ToString(), renderJObj);
						renderElement
							.Attributes()
							.ForEach(renderAttribute => renderJObj.Add(renderAttribute.Name.LocalName, new JValue(renderAttribute.Value)));
					});
			}
		}

		public void InitMappingConfigJson(IEnumerable<XElement> xElements, JObject jObj, string mappingConfName = "MappingConfig")
		{
			if (xElements != null && xElements.Any() && jObj != null)
			{
				var jItem = new JObject();
				jObj.Add(mappingConfName, jItem);
				int index = 0;
				xElements
					.ForEach(xElement =>
					{
						var jItemObj = new JObject();
						jItem.Add((index++).ToString(), jItemObj);
						var jMapItem = new JObject();
						jItemObj.Add("MappingItem", jMapItem);
						var itemIndex = 0;
						xElement
							.Attributes()
							.ForEach(xMapConfAttr => jItemObj.Add(xMapConfAttr.Name.LocalName, xMapConfAttr.Value));
						xElement
							.XPathSelectElements("mappingItem")
							.ForEach(xMapItem =>
							{
								var jMapItemProp = new JObject();
								jMapItem.Add((itemIndex++).ToString(), jMapItemProp);
								xMapItem
									.Attributes()
									.ForEach(xAttr => jMapItemProp.Add(xAttr.Name.LocalName, xAttr.Value));
							});
					});
			}
		}
		private void InitServiceConfigJson(IEnumerable<XElement> xElements, JObject jObj)
		{
			InitNestingConfigJson(xElements, jObj, "ServiceConfig", "ServiceHeaderConfig", "serviceHeaderConfig");
		}

		private void InitEndPointConfigJson(IEnumerable<XElement> xElements, JObject jObj)
		{
			InitNestingConfigJson(xElements, jObj, "EndPointConfig", "HandlerConfigs", "handlerConfig");
		}

		private void InitMockConfigJson(IEnumerable<XElement> xElements, JObject jObj)
		{
			if (xElements != null && xElements.Any() && jObj != null)
			{
				var jItem = new JObject();
				jObj.Add("MockConfig", jItem);
				int index = 0;
				xElements
					.ForEach(xElement =>
					{
						var jItemObj = new JObject();
						jItem.Add((index++).ToString(), jItemObj);
						xElement
							.Attributes()
							.ForEach(xAttr => jItemObj.Add(xAttr.Name.LocalName, xAttr.Value));
					});
			}
		}
		private void InitTemplateConfigJson(IEnumerable<XElement> xElements, JObject jObj)
		{
			InitNestingConfigJson(xElements, jObj, "TemplateSettings", "Setting", "setting");
		}
		private void InitNestingConfigJson(IEnumerable<XElement> xElements, JObject jObj, string rootName, string nestName, string xmlNestName)
		{
			if (xElements != null && xElements.Any() && jObj != null)
			{
				var jItem = new JObject();
				jObj.Add(rootName, jItem);
				int index = 0;
				xElements
					.ForEach(xElement =>
					{
						var jItemObj = new JObject();
						jItem.Add((index++).ToString(), jItemObj);
						var jMapItem = new JObject();
						jItemObj.Add(nestName, jMapItem);
						var itemIndex = 0;
						xElement
							.Attributes()
							.ForEach(xMapConfAttr => jItemObj.Add(xMapConfAttr.Name.LocalName, xMapConfAttr.Value));
						xElement
							.XPathSelectElements(xmlNestName)
							.ForEach(xMapItem =>
							{
								var jMapItemProp = new JObject();
								jMapItem.Add((itemIndex++).ToString(), jMapItemProp);
								xMapItem
									.Attributes()
									.ForEach(xAttr => jMapItemProp.Add(xAttr.Name.LocalName, xAttr.Value));
							});
					});
			}
		}
		#endregion
	}

	#endregion


	#region Class: TsIntegrationCodeServiceHelperTester
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Configuration\Test\Logic\TsIntegrationCodeServiceHelperTester.cs
		
	*/
	public class TsIntegrationCodeServiceHelperTester
	{
		public static void TestJsonToXml()
		{
			string name, xml;
			var helper = new TsIntegrationCodeServiceHelper(new UserConnection(new AppConnection()));
			using (var reader = new StreamReader(new FileStream("../../IntegrationJson/JsonToXmlTest.json", FileMode.Open)))
			{
				xml = helper.GetXmlConfigFromJson(reader.ReadToEnd(), out name);
			}
			Console.WriteLine(name);
			Console.WriteLine(xml);
		}

		public static void TestXmlToJson()
		{
			var helper = new TsIntegrationCodeServiceHelper(new UserConnection(new AppConnection()));
			string json;
			using (var reader = new StreamReader(new FileStream("../../IntegrationJson/XmlToJsonTest.txt", FileMode.Open)))
			{
				json = helper.GetJsonConfigFromXml(reader.ReadToEnd(), "some name");
			}
			Console.WriteLine(json);
		}

		public static void TestConfig(UserConnection userConnection)
		{
			var helper = new TsIntegrationCodeServiceHelper(userConnection);
			string result = helper.TestToJson(new TestExportInfo()
			{
				ConfigId = "ContactExport",
				EntityId = "b6aa40ea-7062-4d3a-afa3-000016c0b6df"
			});
			Console.WriteLine(result);
		}
		public static void TestConfigToEntity(UserConnection userConnection)
		{
			var helper = new TsIntegrationCodeServiceHelper(userConnection);
			string json;
			using (var reader = new StreamReader(new FileStream("../../IntegrationJson/TestJsonToEntityConfig.json", FileMode.Open)))
			{
				json = reader.ReadToEnd();
			}
			string result = helper.TestToEntity(new TestImportInfo()
			{
				ConfigId = "ContactExport",
				IsExists = true,
				IsUpdate = true,
				Json = json
			});
			Console.WriteLine(result);
		}

		public static void TestGetEntityInfo(UserConnection userConnection)
		{
			var helper = new TsIntegrationCodeServiceHelper(userConnection);
			var schemaInfos = helper.GetAllEntityNames();
			schemaInfos.ForEach(x => Console.WriteLine(x.Name + " - " + x.Caption));
		}

		public static void GetBlockLogDataForAnalyze(UserConnection userConnection)
		{
			var helper = new TsIntegrationCodeServiceHelper(userConnection);
			var result = helper.GetBlockLogDataForAnalyze(new Guid("7AE02CAA-49CC-4F11-AFEC-D1E604F9D887"));
			Console.WriteLine(result);
		}
		public static void TestServiceByMock(UserConnection userConnection)
		{
			var helper = new TsIntegrationCodeServiceHelper(userConnection);
			var info = new TestServiceInfo()
			{
				Id = new Guid("df34de78-81a7-4531-a215-000007e3d012"),
				RouteKey = "Contact",
				SchemaName = "Contact",
				IsUseMock = true
			};
			helper.TestServiceByMock(info);
		}
	}

	#endregion


	#region Class: XmlConfigManager
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Configuration\XmlManager\XmlConfigManager.cs
		
	*/
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

	#endregion


	#region Class: CsConstant
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Constants\CsConstant.cs
		
	*/
	public static class CsConstant
	{
		//IsDebugMode = true только для QueryConsole.
		public static bool IsDebugMode = false;
		public class IntegrationResult
		{
			public bool Success {
				get;
				set;
			}
			public IIntegrationObject Data {
				get;
				set;
			}
			public TResultType Type {
				get;
				set;
			}
			public TResultException Exception {
				get;
				set;
			}
			public string ExceptionMessage {
				get;
				set;
			}
			public IntegrationResult()
			{

			}
			public IntegrationResult(IIntegrationObject data)
			{
				Data = data;
			}
			public IntegrationResult(TResultType type, IIntegrationObject data = null)
			{
				Type = type;
				Data = data;
			}

			public IntegrationResult(TResultException exception, string message = null, IIntegrationObject data = null)
			{
				Type = TResultType.Exception;
				Exception = exception;
				ExceptionMessage = message;
				Data = data;
			}


			public enum TResultException
			{
				OnCreateEntityExist
			}
			public enum TResultType
			{
				Exception,
				Success
			}

		}


		public class IntegrationInfo
		{

			public IIntegrationObject Data {
				get;
				set;
			}
			public string StrData {
				get;
				set;
			}
			public UserConnection UserConnection {
				get;
				set;
			}
			public TIntegrationType IntegrationType {
				get;
				set;
			}
			public string Action {
				get;
				set;
			}
			public IntegrationResult Result {
				get;
				set;
			}
			public Entity IntegratedEntity {
				get;
				set;
			}
			public string TsExternalIdPath {
				get;
				set;
			}
			public string TsExternalVersionPath {
				get;
				set;
			}
			public BaseEntityHandler Handler {
				get;
				set;
			}
			public Entity ParentEntity {
				get;
				set;
			}


			public IntegrationInfo(IIntegrationObject data, UserConnection userConnection, TIntegrationType integrationType = TIntegrationType.Export,
				string action = "Create", Entity integratedEntity = null)
			{
				Data = data;
				UserConnection = userConnection;
				IntegrationType = integrationType;
				Action = action;
				IntegratedEntity = integratedEntity;
			}


			public override string ToString()
			{
				return string.Format("Data = {0}\nIntegrationType={1}", Data, IntegrationType);
			}


			public static IntegrationInfo CreateForImport(UserConnection userConnection, string action, IIntegrationObject data)
			{
				return new IntegrationInfo(data, userConnection, TIntegrationType.Import, action, null);
			}
			public static IntegrationInfo CreateForExport(UserConnection userConnection, Entity entity)
			{
				return new IntegrationInfo(null, userConnection, TIntegrationType.Export, CsConstant.IntegrationActionName.Empty, entity);
			}
			public static IntegrationInfo CreateForResponse(UserConnection userConnection, Entity entity)
			{
				return new IntegrationInfo(null, userConnection, TIntegrationType.ExportResponseProcess, CsConstant.IntegrationActionName.UpdateFromResponse, entity);
			}
		}

		public enum TIntegrationType
		{
			Export = 0,
			Import = 1,
			All = 3,
			ExportResponseProcess = 4
		}
		public static class IntegrationActionName
		{
			public const string Create = @"create";
			public const string Update = @"update";
			public const string Delete = @"delete";
			public const string UpdateFromResponse = @"updateFromResponse";
			public const string Empty = @"";
		}
		public static class IntegrationFlagSetting
		{
			public const bool AllowErrorOnColumnAssign = false;
		}
		public static class TsRequestType
		{
			public static readonly Guid Push = new Guid("bda8d5fb-3c8f-41c6-9823-44615ab20596");
			public static readonly Guid GetResponse = new Guid("173dc5c7-0d32-4512-86b8-e91691b22c19");
		}

		public static class PersonName
		{
			public const string Bpm = @"Bpm`online";
			public const string ClientService = @"Client Service";
			public const string IntegrationService = @"Integration Service";
			public const string OrderService = @"Order Service";
			public const string Unknown = @"Unknown";
		}
		public static class TsRequestStatus
		{
			public static readonly Guid Success = new Guid("5a0d25f5-d718-45ab-b4e3-d615ef7e09c6");
			public static readonly Guid Error = new Guid("88c5e88e-410d-4d67-99c3-722d92f93631");
		}

		public static class LoggerSettings
		{
			public static bool IsLoggedStackTrace = false;
			public static bool IsLoggedDbActive = false;
			public static bool IsLoggedFileActive = true;
		}

		public static class IntegratorSettings
		{
			public static bool IsIntegrationAsync = false;
			public static bool isLockerActive = true;
			public static Dictionary<Type, IntegratorSetting> Settings = new Dictionary<Type, IntegratorSetting>()
			{

			};

			#region Class: Setting
			public class IntegratorSetting
			{
				public string Name;
				public string Auth;
				public bool IsIntegratorActive;
				public bool IsDebugMode;
				public TIntegrationObjectType ObjectType;
			}
			#endregion
		}

		public static class IntegartionObjectTypeIds
		{
			public static Guid Json = new Guid("5D304534-9D7D-467C-918F-95264BF29295");
			public static Guid Xml = new Guid("C5AC9B2B-A50E-41C8-B5E1-9035CDBB24B8");
		}
	}

	#endregion


	#region Class: SettingsManager
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Constants\SettingsManager.cs
		
	*/
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

	#endregion


	#region Class: ObjectExtension
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Extension\ObjectExtension.cs
		
	*/
	public static class ObjectExtension
	{
		public static object CloneObject(this object objSource)
		{
			Type typeSource = objSource.GetType();
			object objTarget = Activator.CreateInstance(typeSource);

			PropertyInfo[] propertyInfo = typeSource.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

			foreach (PropertyInfo property in propertyInfo)
			{
				if (property.CanWrite)
				{
					if (property.PropertyType.IsValueType || property.PropertyType.IsEnum || property.PropertyType.Equals(typeof(String)))
					{
						property.SetValue(objTarget, property.GetValue(objSource, null), null);
					}
					else
					{
						object objPropertyValue = property.GetValue(objSource, null);
						if (objPropertyValue == null)
						{
							property.SetValue(objTarget, null, null);
						}
						else
						{
							property.SetValue(objTarget, CloneObject(objPropertyValue), null);
						}
					}
				}
			}
			return objTarget;
		}
	}

	#endregion


	#region Class: StringBuilderExtension
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Extension\StringBuilderExtension.cs
		
	*/
	public static class StringBuilderExtension
	{
		public static StringBuilder AppendLineFormat(
			this StringBuilder builder,
			string formatString,
			params object[] args)
		{
			return builder.AppendFormat(formatString, args)
				.AppendLine();
		}
	}

	#endregion


	#region Class: BaseAttributeFactory
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Factory\BaseAttributeFactory.cs
		
	*/
	public abstract class BaseAttributeFactory<TResult, TAttribute, TName> : IFactory<TResult, TName>
	{
		protected abstract Dictionary<TAttribute, TResult> Instances { get; }
		protected abstract bool IsRuleRegister { get; set; }
		public virtual TResult Get(TName name)
		{
			RegisterRules();
			return Instances.First(x => IsInstanceNameEqual(x, name)).Value;
		}

		protected abstract bool IsInstanceNameEqual(KeyValuePair<TAttribute, TResult> instanceKeyValue, TName name);

		protected virtual void RegisterRules()
		{
			if (IsRuleRegister)
			{
				return;
			}
			var assembly = this.GetType().Assembly;
			var ruleAttrType = typeof(TAttribute);
			assembly
				.GetTypes()
				.Where(x => x.HasAttribute(ruleAttrType))
				.ForEach(x =>
				{
					var attributess = x.GetCustomAttributes(ruleAttrType, true);
					if (attributess.Length == 0)
					{
						return;
					}
					attributess
						.Where(attr => attr is TAttribute)
						.ForEach(attr => RegisterRule((TAttribute)attr, CreateRuleInstanse(x)));
				});
			IsRuleRegister = true;
		}

		protected virtual void RegisterRule(TAttribute attr, TResult instance)
		{
			if (!Instances.ContainsKey(attr))
			{
				Instances.Add(attr, instance);
			}
		}

		protected virtual TResult CreateRuleInstanse(Type ruleType)
		{
			return (TResult)Activator.CreateInstance(ruleType);
		}
	}

	#endregion


	#region Class: EntityNotFoundException
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Handler\Exception\EntityNotFoundException.cs
		
	*/
	public class EntityNotFoundException : Exception
	{
		public EntityNotFoundException()
		{
		}

		public EntityNotFoundException(string message) : base(message)
		{
		}

		public EntityNotFoundException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected EntityNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}

	#endregion


	#region Class: HandlerFactory
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Handler\Factory\HandlerFactory.cs
		
	*/
	public class HandlerFactory
	{
		public static bool IsRegistred = false;
		public static Type HandlerAttrType = typeof(IntegrationHandlerAttribute);
		public static ConcurrentDictionary<string, Type> Handlers;
		public static void Register()
		{
			if (!IsRegistred)
			{
				var handlerDictionary = typeof(HandlerFactory)
					.Assembly
					.GetTypes()
					.Where(x => x.GetCustomAttributes(HandlerAttrType, true).Any())
					.Select(x => new
					{
						key = (x.GetCustomAttributes(HandlerAttrType, true).First() as IntegrationHandlerAttribute).Name,
						value = x
					})
					.Where(x => x.value != null)
					.ToDictionary(x => x.key, x => x.value);
				Handlers = new ConcurrentDictionary<string, Type>(handlerDictionary);
				IsRegistred = true;
			}
		}
		public static BaseEntityHandler Get(string name, ConfigSetting config)
		{
			Register();
			if (Handlers != null && Handlers.ContainsKey(name))
			{
				return Activator.CreateInstance(Handlers[name], config) as BaseEntityHandler;
			}
			return null;
		}
	}

	#endregion


	#region Class: AdvancedSearchHandler
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Handler\Instance\AdvancedSearchHandler.cs
		
	*/
	[IntegrationHandlerAttribute("AdvancedSearchHandler")]
	public class AdvancedSearchHandler: DefaultEntityHandler
	{
		public AdvancedSearchHandler(ConfigSetting handlerConfig) : base(handlerConfig)
		{
		}
		public override bool IsEntityAlreadyExist(CsConstant.IntegrationInfo integrationInfo)
		{
			bool result = false;
			LoggerHelper.DoInLogBlock("Handler: Advanced Search", () =>
			{
				var searchEntityName = GetHandlerConfigValue("SearchEntity");
				var searchEntityColumn = GetHandlerConfigValue("SearchEntityColumn");
				var valuePath = GetHandlerConfigValue("SearchPathValue");
				var value = integrationInfo.Data.GetProperty<string>(valuePath);
				if (!string.IsNullOrEmpty(value))
				{
					result = isEntityExist(searchEntityName, searchEntityColumn, value, integrationInfo.UserConnection);
				}
			});
			if (!result)
			{
				throw new EntityNotFoundException();
			}
			return bool.Parse(GetHandlerConfigValue("DefaultResult"));
		}
		//Log Key = Handler
		protected virtual bool isEntityExist(string entityName, string columnName, string value, UserConnection userConnection)
		{
			var select = new Select(userConnection)
				.Column(Func.Count(Column.Asterisk()))
				.From(entityName)
				.Where(columnName).IsEqual(Column.Parameter(value)) as Select;
			return select.ExecuteScalar<int>() > 0;
		}
	}

	#endregion


	#region Class: BaseEntityHandler
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Handler\Instance\BaseEntityHandler.cs
		
	*/
	public abstract class BaseEntityHandler
	{
		public ConfigSetting HandlerConfig;
		public BaseEntityHandler(ConfigSetting handlerConfig)
		{
			HandlerConfig = handlerConfig;
			Mapper = new IntegrationMapper();
		}

		#region Fields
		private IMapper _mapper;
		public virtual IMapper Mapper {
			set {
				_mapper = value;
			}
			get {
				if (_mapper == null)
				{
					_mapper = new IntegrationMapper();
				}
				return _mapper;
			}
		}
		private IHandlerEntityWorker _handlerEntityWorker;
		public virtual IHandlerEntityWorker HandlerEntityWorker {
			set {
				_handlerEntityWorker = value;
			}
			get {
				if (_handlerEntityWorker == null)
				{
					_handlerEntityWorker = new HandlerEntityWorker();
				}
				return _handlerEntityWorker;
			}
		}
		private IServiceHandlerWorkers _serviceHandlerWorker;
		public virtual IServiceHandlerWorkers ServiceHandlerWorker {
			set {
				_serviceHandlerWorker = value;
			}
			get {
				if (_serviceHandlerWorker == null)
				{
					_serviceHandlerWorker = new ServiceHandlerWorker();
				}
				return _serviceHandlerWorker;
			}
		}
		private IHandlerKeyGenerator _handlerKeyGenerator;
		public virtual IHandlerKeyGenerator HandlerKeyGenerator {
			set {
				_handlerKeyGenerator = value;
			}
			get {
				if (_handlerKeyGenerator == null)
				{
					_handlerKeyGenerator = new HandlerKeyGenerator();
				}
				return _handlerKeyGenerator;
			}
		}
		private IIntegrationObjectProvider _integrationObjectProvider;
		public virtual IIntegrationObjectProvider IntegrationObjectProvider {
			set {
				_integrationObjectProvider = value;
			}
			get {
				if (_integrationObjectProvider == null)
				{
					_integrationObjectProvider = new IntegrationObjectProvider();
				}
				return _integrationObjectProvider;
			}
		}
		private ITemplateFactory _templateFactory;
		public ITemplateFactory TemplateFactory {
			get {
				if (_templateFactory == null)
				{
					_templateFactory = new TemplateHandlerFactory();
				}
				return _templateFactory;
			}
		}
		public string ResponseMappingConfig {
			get {
				if (!string.IsNullOrEmpty(HandlerConfig.ResponseMappingConfig))
				{
					return HandlerConfig.ResponseMappingConfig;
				}
				return HandlerConfig.DefaultMappingConfig;
			}
		}
		public string ResponseJName {
			get {
				if (!string.IsNullOrEmpty(HandlerConfig.ResponseJName))
				{
					return HandlerConfig.ResponseJName;
				}
				return HandlerConfig.JName;
			}
		}
		public string EntityName;
		public string JName;
		public Guid EntityId;
		#endregion

		#region Properties
		public virtual string ExternalIdPath {
			get { return "TsExternalId"; }
		}
		public virtual string JsonIdPath {
			get {
				return "id";
			}
		}
		public virtual bool IsJsonWithHeader {
			get {
				return true;
			}
		}
		/// <summary>
		/// Признак разширеного поиска
		/// </summary>
		public virtual bool IsAdvancedSearch {
			get {
				return false;
			}
		}
		/// <summary>
		/// Информация о розширеном поиске
		/// </summary>
		public virtual AdvancedSearchInfo AdvancedSearchInfo {
			get {
				return null;
			}
		}
		#endregion

		#region Methods
		public virtual void Create(IntegrationInfo integrationInfo)
		{
			if (!CheckIntegrationInfoForCreate(integrationInfo))
			{
				return;
			}
			LoggerHelper.DoInLogBlock("Handler: Create", () =>
			{
				integrationInfo.TsExternalIdPath = ExternalIdPath;
				integrationInfo.IntegratedEntity = HandlerEntityWorker.CreateEntity(integrationInfo.UserConnection, EntityName);
				Templated(integrationInfo);
				AddParentInfoToIntegrationObject(integrationInfo);
				BeforeMapping(integrationInfo);
				Mapper.StartMappByConfig(integrationInfo, JName, ServiceHandlerWorker.GetMappingConfig(HandlerConfig.DefaultMappingConfig));
				AfterMapping(integrationInfo);
				try
				{
					HandlerEntityWorker.SaveEntity(integrationInfo.IntegratedEntity, JName, OnSuccessSave, OnErrorSave);
					integrationInfo.Result = new CsConstant.IntegrationResult()
					{
						Type = CsConstant.IntegrationResult.TResultType.Success
					};
					AfterEntitySave(integrationInfo);
				}
				catch (Exception e)
				{
					integrationInfo.Result = new CsConstant.IntegrationResult()
					{
						Type = CsConstant.IntegrationResult.TResultType.Exception,
						ExceptionMessage = e.Message
					};
				}
			});
		}
		protected virtual bool CheckIntegrationInfoForCreate(IntegrationInfo integrationInfo)
		{
			if (integrationInfo == null)
			{
				IntegrationLogger.Warning("Method Create Validate: integrationInfo is null!");
				return false;
			}
			if (integrationInfo.IntegrationType == CsConstant.TIntegrationType.Export)
			{
				IntegrationLogger.Warning("Method Create Validate: method doesn`t support Export");
				return false;
			}
			if (GetHandlerConfigValue("EnableCreate", "true") == "false")
			{
				return false;
			}
			return true;
		}
		protected virtual void OnErrorSave()
		{
			Mapper.ExecuteMapMethodQueue();
		}

		private void OnSuccessSave()
		{
			Mapper.ExecuteMapMethodQueue();
		}

		public virtual void Update(IntegrationInfo integrationInfo)
		{
			if (!CheckIntegrationInfoForUpdate(integrationInfo))
			{
				return;
			}
			LoggerHelper.DoInLogBlock("Handler: Update", () =>
			{
				integrationInfo.TsExternalIdPath = ExternalIdPath;
				Templated(integrationInfo);
				Entity entity = GetEntityByExternalId(integrationInfo);
				integrationInfo.IntegratedEntity = entity;
				AddParentInfoToIntegrationObject(integrationInfo);
				BeforeMapping(integrationInfo);
				Mapper.StartMappByConfig(integrationInfo, JName, ServiceHandlerWorker.GetMappingConfig(HandlerConfig.DefaultMappingConfig));
				AfterMapping(integrationInfo);
				HandlerEntityWorker.SaveEntity(entity, JName, OnSuccessSave, OnErrorSave);
				AfterEntitySave(integrationInfo);
			});
		}
		protected virtual bool CheckIntegrationInfoForUpdate(IntegrationInfo integrationInfo)
		{
			if (integrationInfo == null)
			{
				IntegrationLogger.Warning("Method Update Validate: integrationInfo is null!");
				return false;
			}
			if (integrationInfo.IntegrationType == CsConstant.TIntegrationType.Export)
			{
				IntegrationLogger.Warning("Method Update Validate: method doesn`t support Export");
				return false;
			}
			if (GetHandlerConfigValue("EnableUpdate", "true") == "false")
			{
				return false;
			}
			return true;
		}

		public virtual EntitySchemaQuery GetEntitySchemaQuery(ref MappingConfig mappingConfig, UserConnection userConnection)
		{
			var esq = new EntitySchemaQuery(userConnection.EntitySchemaManager, HandlerConfig.EntityName);
			esq.UseAdminRights = false;
			esq.IgnoreDisplayValues = true;
			esq.PrimaryQueryColumn.IsAlwaysSelect = true;
			var dict = new Dictionary<string, EntitySchemaQueryColumn>();

			var mapItems = mappingConfig.Items.Where(i => i.TsSourcePath != null && i.TsSourcePath != esq.RootSchema.PrimaryColumn.Name);
			var columns = mapItems.Select(i => i.TsSourcePath).Distinct();

			foreach (var col in columns)
			{
				var queryColumn = esq.AddColumn(col);
				dict.Add(col, queryColumn);
			}

			foreach (var mapItem in mapItems)
			{
				var queColumn = dict[mapItem.TsSourcePath];
				var columnValuePath = queColumn.Name;
				if (queColumn.IsLookup)
				{
					columnValuePath = queColumn.ValueExpression.Path;
				}
				if (mapItem.TsSourcePath != columnValuePath)
				{
					mapItem.TsSourcePath = columnValuePath;
				}
			}
			return esq;
		}

		public virtual Entity CreateEntityForExportMyMapping(ref MappingConfig mappingConfig, UserConnection userConnection)
		{
			var esqEntity = GetEntitySchemaQuery(ref mappingConfig, userConnection);
			return esqEntity.GetEntity(userConnection, EntityId);
		}
		public virtual void Delete(IntegrationInfo integrationInfo)
		{
			throw new NotImplementedException();
		}
		public virtual void Unknown(IntegrationInfo integrationInfo)
		{
			Update(integrationInfo);
		}


		public virtual IIntegrationObject ToJson(IntegrationInfo integrationInfo)
		{
			IIntegrationObject result = null;
			LoggerHelper.DoInLogBlock("Handler: To Json", () => {
				if (integrationInfo.IntegrationType == CsConstant.TIntegrationType.Export)
				{
					integrationInfo.TsExternalIdPath = ExternalIdPath;
					BeforeMapping(integrationInfo);
					var mapConfig = ServiceHandlerWorker.GetMappingConfig(HandlerConfig.DefaultMappingConfig);
					if (integrationInfo.IntegratedEntity == null)
					{
						integrationInfo.IntegratedEntity = CreateEntityForExportMyMapping(ref mapConfig, integrationInfo.UserConnection);
					}
					Mapper.StartMappByConfig(integrationInfo, JName, mapConfig);
					AfterMapping(integrationInfo);
					Templated(integrationInfo);
					result = integrationInfo.Data;
				}
			});
			return result;
		}

		public virtual void ProcessResponse(IntegrationInfo integrationInfo)
		{
			if (!CheckIntegrationInfoForProcessResponse(integrationInfo))
			{
				return;
			}
			LoggerHelper.DoInLogBlock("Handler: Process Response", () =>
			{
				integrationInfo.TsExternalIdPath = ExternalIdPath;
				if (!string.IsNullOrEmpty(integrationInfo.StrData))
				{
					IntegrationLogger.Info(integrationInfo.StrData);
					integrationInfo.Data = IntegrationObjectProvider.Parse(integrationInfo.StrData);
				}
				if (integrationInfo.IntegratedEntity == null)
				{
					integrationInfo.IntegratedEntity = GetEntityByExternalId(integrationInfo);
				}
				Templated(integrationInfo);
				AddParentInfoToIntegrationObject(integrationInfo);
				if (!string.IsNullOrEmpty(HandlerConfig.ResponseRoute))
				{
					ProcessResponse(integrationInfo, HandlerConfig.ResponseRoute);
					return;
				}
				BeforeMapping(integrationInfo);
				Mapper.StartMappByConfig(integrationInfo, ResponseJName, ServiceHandlerWorker.GetMappingConfig(ResponseMappingConfig));
				AfterMapping(integrationInfo);
				try
				{
					HandlerEntityWorker.SaveEntity(integrationInfo.IntegratedEntity, ResponseJName, OnSuccessSave, OnErrorSave);
					integrationInfo.Result = new CsConstant.IntegrationResult()
					{
						Type = CsConstant.IntegrationResult.TResultType.Success
					};
					AfterEntitySave(integrationInfo);
				}
				catch (Exception e)
				{
					integrationInfo.Result = new CsConstant.IntegrationResult()
					{
						Type = CsConstant.IntegrationResult.TResultType.Exception,
						ExceptionMessage = e.Message
					};
					IntegrationLogger.Error(e);
				}
			});
		}
		protected virtual void AddParentInfoToIntegrationObject(IntegrationInfo integrationInfo)
		{
			if (integrationInfo.ParentEntity != null)
			{
				integrationInfo.Data.SetProperty("TsiIntegrateParentId", integrationInfo.ParentEntity.PrimaryColumnValue);
			}
		}
		//Log key=Handler
		protected virtual void ProcessResponse(IntegrationInfo integrationInfo, string route)
		{
			integrationInfo.Data.SetProperty("TsiIntegrateParentId", integrationInfo.IntegratedEntity.PrimaryColumnValue);
			var integrator = ClassFactory.Get<BaseIntegrator>();
			integrator.Import(integrationInfo.UserConnection, integrationInfo.Data, route);
		}
		protected virtual bool CheckIntegrationInfoForProcessResponse(IntegrationInfo integrationInfo)
		{
			if (integrationInfo == null)
			{
				IntegrationLogger.Warning("Method ProcessResponse Validate: integrationInfo is null!");
				return false;
			}
			if (integrationInfo.Action != CsConstant.IntegrationActionName.UpdateFromResponse)
			{
				IntegrationLogger.WarningFormat("Method ProcessResponse Validate: method doesn`t support action {0}", integrationInfo.Action);
				return false;
			}
			if (integrationInfo.IntegrationType != CsConstant.TIntegrationType.ExportResponseProcess)
			{
				IntegrationLogger.WarningFormat("Method ProcessResponse Validate: method doesn`t support type {0}", integrationInfo.IntegrationType);
				return false;
			}
			if (integrationInfo.Data == null && string.IsNullOrEmpty(integrationInfo.StrData))
			{
				IntegrationLogger.Warning("Method ProcessResponse Validate: data response doesn`t set");
				return false;
			}
			if (GetHandlerConfigValue("EnableProcessResponse", "true") == "false")
			{
				return false;
			}
			return true;
		}
		//Log key=Handler
		public virtual ServiceRequestInfo GetRequestInfo(IntegrationInfo integrationInfo)
		{
			var id = integrationInfo.IntegratedEntity.GetColumnValue(ExternalIdPath);
			var requestInfo = new ServiceRequestInfo()
			{
				RequestJson = integrationInfo.Result.Data.ToString(),
				Entity = integrationInfo.IntegratedEntity
			};
			if (id != null && !string.IsNullOrEmpty(id.ToString()) && id.ToString() != "0")
			{
				requestInfo.Method = TRequstMethod.PUT;
			}
			else
			{
				requestInfo.Method = TRequstMethod.POST;
			}
			requestInfo.FullUrl = GetUrl(integrationInfo, HandlerConfig);
			requestInfo.Auth = HandlerConfig.Auth;
			requestInfo.Handler = this;
			return requestInfo;
		}
		//Log key=Handler
		public virtual void OnRequestException(Exception e)
		{
			//TODO
			if (e is WebException)
			{
				WebResponse response = ((WebException)e).Response;
				using (StreamReader sr = new StreamReader(response.GetResponseStream()))
				{
					string responceText = sr.ReadToEnd();
					IntegrationLogger.Error(RequestErrorLoggerInfo.GetMessage(e, responceText));
				}
			}
		}
		protected virtual void BeforeMapping(IntegrationInfo integrationInfo)
		{
		}
		protected virtual void AfterMapping(IntegrationInfo integrationInfo)
		{
		}
		protected virtual void AfterEntitySave(IntegrationInfo integrationInfo)
		{
		}
		public virtual bool IsEntityAlreadyExist(IntegrationInfo integrationInfo)
		{
			bool result = false;
			LoggerHelper.DoInLogBlock("Handler: Search Entity", () =>
			{
				integrationInfo.TsExternalIdPath = ExternalIdPath;
				object externalId = null;
				if (integrationInfo.IntegratedEntity != null)
				{
					externalId = integrationInfo.IntegratedEntity.GetColumnValue(ExternalIdPath);
				}
				var path = IntegrationPath.GenerateValuePath(JName, JsonIdPath);
				result = Mapper.CheckIsExist(integrationInfo.UserConnection, EntityName, integrationInfo.Data.GetProperty<string>(path), integrationInfo.TsExternalIdPath, externalId);
				if (!result && IsAdvancedSearch)
				{
					result = IsEntityAlreadyExistAdvanced(integrationInfo);
				}
			});
			return result;
		}
		protected virtual bool IsEntityAlreadyExistAdvanced(IntegrationInfo integrationInfo)
		{
			integrationInfo.TsExternalIdPath = ExternalIdPath;
			if (integrationInfo.IntegratedEntity != null)
			{
				return true;
			}
			if (AdvancedSearchInfo == null)
			{
				return false;
			}
			Guid resultId = AdvancedSearchInfo.Search(integrationInfo.UserConnection,
					procedure => AddParameterToSearchProcedure(integrationInfo, procedure), IntegrationLogger.SimpleLoggerErrorAction);
			if (resultId == Guid.Empty)
			{
				return false;
			}
			integrationInfo.IntegratedEntity = HandlerEntityWorker.GetEntityById(integrationInfo.UserConnection, EntityName, resultId);
			return true;
		}
		protected virtual void AddParameterToSearchProcedure(IntegrationInfo integrationInfo, StoredProcedure searchProcedure)
		{
			return;
		}
		protected virtual Entity GetEntityByExternalId(CsConstant.IntegrationInfo integrationInfo)
		{
			Entity entity = null;
			if (IsAdvancedSearch && integrationInfo.IntegratedEntity != null)
			{
				entity = integrationInfo.IntegratedEntity;
			}
			else
			{
				var path = IntegrationPath.GenerateValuePath(JName, JsonIdPath);
				entity = HandlerEntityWorker.GetEntityByExternalId(integrationInfo.UserConnection, EntityName, integrationInfo.TsExternalIdPath, integrationInfo.Data.GetProperty<string>(path));
			}
			if (entity != null)
			{
				return entity;
			}
			var msg = string.Format("Не удалось найти объект: {0}, по информации: {1}", EntityName, integrationInfo);
			IntegrationLogger.ErrorFormat(msg);
			throw new Exception(msg);
		}
		public virtual bool IsExport(CsConstant.IntegrationInfo integrationInfo)
		{
			return true;
		}
		public virtual string GetKeyForLock(IntegrationInfo integrationInfo)
		{
			return HandlerKeyGenerator.GenerateBlockKey(this, integrationInfo);
		}
		//Log key=Handler
		protected virtual string GetUrl(IntegrationInfo integrationInfo, ConfigSetting handlerConfig)
		{
			if (integrationInfo.IntegrationType == CsConstant.TIntegrationType.Export)
			{
				var url = handlerConfig.Url;
				return string.Format("{0}/{1}", handlerConfig.Url, integrationInfo.IntegratedEntity.GetTypedColumnValue<string>(ExternalIdPath));
			}
			return handlerConfig.Url;
		}
		protected virtual IntegrationInfo Templated(IntegrationInfo integrationInfo)
		{
			if (string.IsNullOrEmpty(HandlerConfig.TemplateName))
			{
				return integrationInfo;
			}
			LoggerHelper.DoInLogBlock("Templated", () =>
			{
				var templateConfig = SettingsManager.GetTemplatesConfig(HandlerConfig.TemplateName);
				if (templateConfig == null)
				{
					IntegrationLogger.WarningFormat("Template Config: {0} doesn`t found", HandlerConfig.TemplateName);
				}
				var templateHandler = TemplateFactory.Get(templateConfig.Handler);
				if (templateHandler == null)
				{
					IntegrationLogger.WarningFormat("Template Handler: {0} doesn`t found", templateConfig.Handler);
					return;
				}

				if (integrationInfo.IntegrationType == TIntegrationType.Export)
				{
					templateHandler.Export(templateConfig, integrationInfo);
				}
				else
				{
					templateHandler.Import(templateConfig, integrationInfo);
				}
			});
			return integrationInfo;
		}
		//Log key=Handler
		protected virtual string GetHandlerConfigValue(string name, string defaultValue = null)
		{
			if (HandlerConfig.HandlerConfigs != null)
			{
				var result = HandlerConfig.HandlerConfigs.FirstOrDefault(x => x.Key == name);
				if (result != null)
				{
					return result.Value;
				}
			}
			return defaultValue;
		}
		#endregion
	}

	#endregion


	#region Class: DefaultEntityHandler
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Handler\Instance\DefaultEntityHandler.cs
		
	*/
	[IntegrationHandlerAttribute("DefaultHandler")]
	public class DefaultEntityHandler : BaseEntityHandler
	{
		private string _externalIdPath;

		public override string ExternalIdPath {
			get { return _externalIdPath; }
		}
		private string _jsonIdPath;
		public override string JsonIdPath {
			get {
				return _jsonIdPath;
			}
		}
		public DefaultEntityHandler(ConfigSetting handlerConfig) : base(handlerConfig)
		{
			JName = handlerConfig.JName;
			EntityName = handlerConfig.EntityName;
			_externalIdPath = handlerConfig.ExternalIdPath;
			_jsonIdPath = handlerConfig.JsonIdPath;
		}
	}

	#endregion


	#region Class: NoEntityHandler
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Handler\Instance\NoEntityHandler.cs
		
	*/
	[IntegrationHandlerAttribute("NoEntityHandler")]
	public class NoEntityHandler: DefaultEntityHandler
	{
		public NoEntityHandler(ConfigSetting handlerConfig) : base(handlerConfig)
		{
		}
		public override Entity CreateEntityForExportMyMapping(ref MappingConfig mappingConfig, UserConnection userConnection)
		{
			return null;
		}
	}

	#endregion


	#region Class: TsiServiceCallCaseIntegrationHandler
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Handler\Instance\TsiServiceCallCaseIntegrationHandler.cs
		
	*/
	[IntegrationHandlerAttribute("ReintegrateOrErrorHandler")]
	public class TsiServiceCallCaseIntegrationHandler : DefaultEntityHandler
	{
		public TsiServiceCallCaseIntegrationHandler(ConfigSetting handlerConfig) : base(handlerConfig)
		{
		}

		public override void ProcessResponse(CsConstant.IntegrationInfo integrationInfo)
		{
			if (!TryProcessIfError(integrationInfo))
			{
				base.ProcessResponse(integrationInfo);
			}
		}

		private bool TryProcessIfError(CsConstant.IntegrationInfo integrationInfo)
		{
			if (!string.IsNullOrEmpty(integrationInfo.StrData))
			{
				IntegrationLogger.Info(integrationInfo.StrData);
				integrationInfo.Data = IntegrationObjectProvider.Parse(integrationInfo.StrData);
				Templated(integrationInfo);

				var errorCodePath = GetHandlerConfigValue("GetErrorCodePath", string.Empty);
				var triggerMappingStr = GetHandlerConfigValue("GetErrorCodeReupdateTriggerMapping", string.Empty);

				var errorCode = integrationInfo.Data.GetProperty(errorCodePath, string.Empty);
				if (!string.IsNullOrEmpty(errorCode))
				{
					var mapping = ParseTriggerMapping(triggerMappingStr)
						.FirstOrDefault(x => x.ErrorCode == errorCode);
					if (mapping != null)
					{
						InsertInTriggerQueue(integrationInfo, mapping.Trigger);
						return true;
					}
				}
			}

			return false;
		}

		private void InsertInTriggerQueue(CsConstant.IntegrationInfo integrationInfo, string triggerName)
		{
			try
			{
				var id = integrationInfo.IntegratedEntity.PrimaryColumnValue;
				var insert = new Insert(integrationInfo.UserConnection)
					.Into("TsiTriggerQueue")
					.Set("TsiObjectName", Column.Parameter(integrationInfo.IntegratedEntity.SchemaName))
					.Set("TsiObjectId", Column.Parameter(id))
					.Set("TsiTriggerName", Column.Parameter(triggerName)) as Insert;
				insert.Execute();
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e);
			}
		}

		private IEnumerable<ErrorTriggerMapping> ParseTriggerMapping(string triggerMappingStr)
		{
			return triggerMappingStr.Split(';')
				.Where(x => !string.IsNullOrEmpty(x))
				.Select(x =>
				{
					var parameters = x.Split(',').Where(y => !string.IsNullOrEmpty(y)).Take(2).ToList();
					if (parameters.Count == 2)
					{
						return new ErrorTriggerMapping()
						{
							ErrorCode = parameters[0],
							Trigger = parameters[1]
						};
					}
					return null;
				})
				.Where(x => x != null);
		}

		public class ErrorTriggerMapping
		{
			public string ErrorCode { get; set; }
			public string Trigger { get; set; }
		}
	}

	#endregion


	#region Class: DefaultHandlerAttribute
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Handler\Manager\Attribute\DefaultHandlerAttribute.cs
		
	*/
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public class DefaultHandlerAttribute : System.Attribute
	{
		public int Index;
		public DefaultHandlerAttribute(int index)
		{
			Index = index;
		}
	}

	#endregion


	#region Class: ExportHandlerAttribute
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Handler\Manager\Attribute\ExportHandlerAttribute.cs
		
	*/
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
	public class ExportHandlerAttribute : IntegrationHandlerAttribute
	{
		public ExportHandlerAttribute(string _name)
			: base(_name)
		{
		}
	}

	#endregion


	#region Class: ImportHandlerAttribute
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Handler\Manager\Attribute\ImportHandlerAttribute.cs
		
	*/
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
	public class ImportHandlerAttribute : IntegrationHandlerAttribute
	{
		public ImportHandlerAttribute(string _name)
			: base(_name)
		{
		}
	}

	#endregion


	#region Class: IntegrationHandlerAttribute
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Handler\Manager\Attribute\IntegrationHandlerAttribute.cs
		
	*/
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Method)]
	public class IntegrationHandlerAttribute : System.Attribute
	{
		private string _name;
		public string Name {
			get { return _name; }
		}
		public IntegrationHandlerAttribute(string _name)
		{
			this._name = _name;
		}
	}

	#endregion


	#region Class: EntityPreparer
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Handler\Plugin\Entity\EntityPreparer.cs
		
	*/
	public class EntityPreparer : IEntityPreparer
	{
		//Log key=Handler Util
		public Entity Get(UserConnection userConnection, string schemaName, Guid id)
		{
			EntitySchema entitySchema = userConnection.EntitySchemaManager.GetInstanceByName(schemaName);
			if (entitySchema != null)
			{
				Entity entity = entitySchema.CreateEntity(userConnection);
				if (entity.FetchFromDB(id, false))
				{
					return entity;
				}
			}
			return null;
		}
	}

	#endregion


	#region Class: HandlerEntityWorker
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Handler\Plugin\Entity\HandlerEntityWorker.cs
		
	*/
	public class HandlerEntityWorker : IHandlerEntityWorker
	{
		//Log key=Handler Util
		public Entity CreateEntity(UserConnection userConnection, string entityName)
		{
			var entitySchema = userConnection.EntitySchemaManager.GetInstanceByName(entityName);
			var entity = entitySchema.CreateEntity(userConnection);
			entity.SetDefColumnValues();
			return entity;
		}
		//Log key=Handler Util
		public void SaveEntity(Entity entity, string jName, Action OnSuccess, Action OnError)
		{
			try
			{
				LoggerHelper.DoInLogBlock("Save Entity", () =>
				{
					IntegrationLogger.Info(string.Format("Entity\nType: {0}\nPrimary Value: \"{1}\"\nDisplay Value: \"{2}\"", entity.GetType(), entity.PrimaryColumnValue, entity.PrimaryDisplayColumnValue));
					try
					{
						entity.Save(false);
						OnSuccess();
					}
					catch (Exception e)
					{
						IntegrationLogger.Error(e);
						OnError();
					}
				});
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e);
			}
		}
		//Log key=Handler Util
		public Entity GetEntityByExternalId(UserConnection userConnection, string entityName, string externalIdPath, string externalId)
		{
			var esq = new EntitySchemaQuery(userConnection.EntitySchemaManager, entityName);
			esq.AddAllSchemaColumns();
			esq.RowCount = 1;
			esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, externalIdPath, externalId));
			return esq.GetEntityCollection(userConnection).FirstOrDefault();
		}
		//Log key=Handler Util
		public Entity GetEntityById(UserConnection userConnection, string entityName, Guid id)
		{
			var entityEsq = new EntitySchemaQuery(userConnection.EntitySchemaManager, entityName);
			entityEsq.AddAllSchemaColumns();
			return entityEsq.GetEntity(userConnection, id);
		}
	}

	#endregion


	#region Class: HandlerKeyGenerator
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Handler\Plugin\KeyGenerator\HandlerKeyGenerator.cs
		
	*/
	public class HandlerKeyGenerator : IHandlerKeyGenerator
	{
		//Log key=Handler Util
		public string GenerateBlockKey(BaseEntityHandler handler, CsConstant.IntegrationInfo integrationInfo)
		{
			switch (integrationInfo.IntegrationType)
			{
				case CsConstant.TIntegrationType.Export:
					return "e_" + handler.EntityName + "_" + integrationInfo.IntegratedEntity.PrimaryColumnValue;
				case CsConstant.TIntegrationType.ExportResponseProcess:
				case CsConstant.TIntegrationType.Import:
				case CsConstant.TIntegrationType.All:
					var path = IntegrationPath.GenerateValuePath(handler.JName, handler.JsonIdPath);
					return "i_" + handler.JName + "_" + integrationInfo.Data.GetProperty<string>(path);
			}
			return handler.EntityName;
		}
	}

	#endregion


	#region Class: AdvancedSearchInfo
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Handler\Plugin\Search\AdvancedSearchInfo.cs
		
	*/
	public class AdvancedSearchInfo
	{
		public string StoredProcedureName;
		//Log key=Handler Util
		public Guid Search(UserConnection userConnection, Action<StoredProcedure> procedureAction, Action<Exception> onErrorAction = null)
		{
			try
			{
				if (string.IsNullOrEmpty(StoredProcedureName) || procedureAction == null)
				{
					return Guid.Empty;
				}
				var searchProcedure = new StoredProcedure(userConnection, StoredProcedureName)
					.WithOutputParameter("ResultId", userConnection.DataValueTypeManager.GetInstanceByName("Guid")) as StoredProcedure;
				procedureAction(searchProcedure);
				searchProcedure.PackageName = userConnection.DBEngine.SystemPackageName;
				searchProcedure.Execute();
				var result = searchProcedure.Parameters.GetByName("ResultId").Value;
				if (result != null && result is Guid)
				{
					return (Guid)result;
				}
				return Guid.Empty;
			}
			catch (Exception e)
			{
				if (onErrorAction != null)
				{
					onErrorAction(e);
				}
			}
			return Guid.Empty;
		}
	}

	#endregion


	#region Class: IntegrJObject
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\IntegrationObject\Instance\Json\IntegrJObject.cs
		
	*/
	public class IntegrJObject : IIntegrationObject
	{
		private JToken _data;
		public IntegrJObject()
		{
			_data = new JObject();
		}
		public IntegrJObject(JToken jObj)
		{
			if (jObj != null)
			{
				SetObject(jObj);
			}
			else
			{
				_data = new JObject();
			}
		}

		public void FromObject(object obj)
		{
			if (obj == null)
			{
				_data = null;
			}
			else
			{
				_data = JToken.FromObject(obj);
			}
		}
		public object GetObject()
		{
			return _data;
		}
		public T GetProperty<T>(string name, T defaultValue = default(T))
		{
			if (_data != null)
			{
				if (string.IsNullOrEmpty(name))
				{
					return _data.Value<T>();
				}
				return _data.SelectToken(name).Value<T>();
			}
			return defaultValue;
		}

		public string GetRootName(string defaultValue = null)
		{
			if (_data != null && _data is JObject)
			{
				return ((JObject)_data).Properties().First().Name;
			}
			return defaultValue;
		}

		public IIntegrationObject GetSubObject(string path)
		{
			if (_data != null)
			{
				var jObj = _data.SelectToken(path);
				return new IntegrJObject(jObj);
			}
			return null;
		}
		public IEnumerable<IIntegrationObject> GetSubObjects(string path)
		{
			return new List<IIntegrationObject>();
		}
		public void InitObject(string rootName = null)
		{
			if (!string.IsNullOrEmpty(rootName))
			{
				_data[rootName] = new JObject();
			}
		}

		public void SetObject(object jObj)
		{
			if (jObj is JToken)
			{
				_data = (JToken)jObj;
			}
		}
		public void SetProperty(string name, object obj)
		{
			if (_data != null && !string.IsNullOrEmpty(name))
			{
				JToken token = GetJTokenByPath(_data, name);
				if (obj == null)
				{
					token.Replace(null);
				}
				JToken resToken = null;
				if (obj is JToken || obj is JObject || obj is JValue)
				{
					resToken = (JToken)obj;
				}
				else if (obj is IntegrJObject)
				{
					var objToken = ((IntegrJObject)obj).GetObject();
					if (objToken == null)
					{
						resToken = null;
					}
					else
					{
						resToken = objToken as JToken;
					}
				}
				token.Replace(resToken);
			}
		}
		public static JToken GetJTokenByPath(JToken jToken, string path)
		{
			var pItems = path.Split('.');
			foreach (var pItem in pItems)
			{
				if (!jToken.HasValues || jToken[pItem] == null)
				{
					jToken[pItem] = new JObject();
				}
				jToken = jToken[pItem];
			}
			return jToken;
		}
		public override string ToString()
		{
			if (_data != null)
			{
				return _data.ToString();
			}
			return string.Empty;
		}
	}

	#endregion


	#region Class: IntegrXObject
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\IntegrationObject\Instance\Xml\IntegrXObject.cs
		
	*/
	public class IntegrXObject : IIntegrationObject
	{
		private XDocument _document;
		private XElement _data;
		public IntegrXObject(string name)
		{
			_data = new XElement(name);
			_document = new XDocument(_data);
		}
		public IntegrXObject(XElement data)
		{
			_data = data;
			_document = new XDocument(_data);
		}
		//Log key=iObject
		public void FromObject(object obj)
		{
			if (obj == null)
			{
				_data.Value = string.Empty;
				return;
			}
			if (obj is byte[])
			{
				obj = FromBinaryObject((byte[])obj);
			}
			if (obj is ValueType || obj is string)
			{
				_data.Value = obj.ToString();
				return;
			}
			var serializer = new XmlSerializer(obj.GetType());
			var document = new XDocument();
			using (var writer = document.CreateWriter())
			{
				serializer.Serialize(writer, obj);
			}
			_data.SetValue(document.Root);
		}
		//Log key=iObject
		public object FromBinaryObject(byte[] binaryData)
		{
			return Convert.ToBase64String(binaryData);
		}
		public object GetObject()
		{
			return _data;
		}
		//Log key=iObject
		public T GetProperty<T>(string name, T defaultValue = default(T))
		{
			if (_data != null)
			{
				if (string.IsNullOrEmpty(name))
				{
					return CastToTempl<T>(_data.Value);
				}
				return CastToTempl<T>(_document.XPathEvaluate(name));
			}
			return defaultValue;
		}
		//Log key=iObject
		public string GetRootName(string defaultValue = null)
		{
			if (_data != null)
			{
				return _data.Name.LocalName;
			}
			return defaultValue;
		}
		//Log key=iObject
		public IIntegrationObject GetSubObject(string path)
		{
			if (_data != null)
			{
				var xElement = _document.XPathSelectElement(path);
				if (xElement == null)
				{
					_document = new XDocument(_data);
					xElement = _document.XPathSelectElement(path);
				}
				return new IntegrXObject(xElement);
			}
			return null;
		}
		//Log key=iObject
		public IEnumerable<IIntegrationObject> GetSubObjects(string path)
		{
			if (_data != null)
			{
				return _document.XPathSelectElements(path).Select(xElement => new IntegrXObject(xElement)).ToList();
			}
			return null;
		}
		//Log key=iObject
		public void InitObject(string rootName = null)
		{
			if (!string.IsNullOrEmpty(rootName))
			{
				_data.Add(new XElement(rootName));
			}
		}
		//Log key=iObject
		public void SetObject(object obj)
		{
			if (obj is XElement)
			{
				_data = (XElement)obj;
			}
		}
		//Log key=iObject
		public void SetProperty(string name, object obj)
		{
			if (_data != null && !string.IsNullOrEmpty(name))
			{
				XElement token = GetXElementByPath(_data, name);
				XElement resToken = null;
				if (obj is XElement)
				{
					resToken = (XElement)obj;
				}
				else if (obj is IntegrXObject)
				{
					resToken = ((IntegrXObject)obj).GetObject() as XElement;
				}
				else
				{
					token.SetValue(obj);
				}
				if (resToken != null)
				{
					token.ReplaceWith(resToken);
				}
			}
		}
		public static T CastToTempl<T>(object obj)
		{
			return (T)Convert.ChangeType(obj, typeof(T));
		}
		//Log key=iObject
		public static XElement GetXElementByPath(XElement xElement, string path)
		{
			var pItems = path.Split('/');
			if (pItems.Any())
			{
				if (xElement.Name.LocalName == pItems[0])
				{
					pItems = pItems.Skip(1).ToArray();
				}
				foreach (var pItem in pItems)
				{
					if (xElement.Element(pItem) == null)
					{
						xElement.Add(new XElement(pItem));
					}
					xElement = xElement.Element(pItem);
				}
				return xElement;
			}
			return xElement;
		}
		public override string ToString()
		{
			if (_data != null)
			{
				return _data.ToString();
			}
			return string.Empty;
		}
	}

	#endregion


	#region Class: IntegrationObjectProvider
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\IntegrationObject\Plugin\IntegrationObjectProvider.cs
		
	*/
	public class IntegrationObjectProvider : IIntegrationObjectProvider
	{
		public static TIntegrationObjectType GetObjectType()
		{
			//TODO: Перенести в ServiceHandlerWorker
			return SettingsManager.GetIntegratorSetting<TIntegrationObjectType>("TsIntegrationObjectType");
		}
		public virtual IIntegrationObject Parse(string text)
		{
			var objectType = GetObjectType();
			switch (objectType)
			{
				case TIntegrationObjectType.Json:
					return new IntegrJObject(JObject.Parse(text));
				case TIntegrationObjectType.Xml:
					return new IntegrXObject(XElement.Parse(text));
				default:
					throw new Exception("Не удалось распознать тип объекта интеграции!");
			}
		}

		public virtual IIntegrationObject NewInstance(string name = null)
		{
			var objectType = GetObjectType();
			switch (objectType)
			{
				case TIntegrationObjectType.Json:
					return new IntegrJObject();
				case TIntegrationObjectType.Xml:
					return new IntegrXObject(name);
				default:
					throw new Exception("Не удалось распознать тип объекта интеграции!");
			}
		}
		public virtual Stream GetMemoryStream(IIntegrationObject iObject)
		{
			var stream = new MemoryStream();
			var encoding = new UTF8Encoding();
			var encodeData = encoding.GetBytes(iObject.ToString());
			stream.Write(encodeData, 0, encodeData.Length);
			stream.Seek(0, SeekOrigin.Begin);
			return stream;
		}
		public virtual string GetContentType(IIntegrationObject iObject)
		{
			if (iObject is IntegrXObject)
			{
				return "text/xml";
			}
			else if (iObject is IntegrJObject)
			{
				return "application/json";
			}
			return "text/plain";
		}
	}

	#endregion


	#region Class: IntegrationPath
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\IntegrationObject\Plugin\IntegrationPath.cs
		
	*/
	public static class IntegrationPath
	{
		public static string GeneratePath(params string[] steps)
		{
			var objectType = IntegrationObjectProvider.GetObjectType();
			switch (objectType)
			{
				case TIntegrationObjectType.Json:
					return GenerateJsonPath(steps);
				case TIntegrationObjectType.Xml:
					return GenerateXPath(steps);
				default:
					throw new Exception("Не удалось распознать тип объекта интеграции!");
			}
		}

		public static string GenerateValuePath(params string[] steps)
		{
			var objectType = IntegrationObjectProvider.GetObjectType();
			switch (objectType)
			{
				case TIntegrationObjectType.Json:
					return GenerateJsonPath(steps);
				case TIntegrationObjectType.Xml:
					return GenerateValueXPath(steps);
				default:
					throw new Exception("Не удалось распознать тип объекта интеграции!");
			}
		}

		private static string GenerateValueXPath(string[] steps)
		{
			return string.Format("string(//{0}/text())", GenerateXPath(steps));
		}

		public static string GenerateXPath(params string[] steps)
		{
			return steps.Aggregate((x, y) => x + "/" + y);
		}
		
		public static string GenerateJsonPath(params string[] steps)
		{
			return steps.Aggregate((x, y) => x + "." + y);
		}
	}

	#endregion


	#region Class: BaseIntegrator
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Integrator\BaseIntegrator.cs
		
	*/
	public class BaseIntegrator : IIntegrator
	{
		private IEntityPreparer _entityPreparer;
		public virtual IEntityPreparer EntityPreparer {
			set {
				_entityPreparer = value;
			}
			get {
				if (_entityPreparer == null)
				{
					_entityPreparer = new EntityPreparer();
				}
				return _entityPreparer;
			}
		}

		private IIntegrationObjectWorker _iObjectWorker;
		public virtual IIntegrationObjectWorker IObjectWorker {
			set {
				_iObjectWorker = value;
			}
			get {
				if (_iObjectWorker == null)
				{
					IObjectWorker = new IntegrationObjectWorker();
				}
				return _iObjectWorker;
			}
		}

		private IServiceHandlerWorkers _serviceHandlerWorker;
		public virtual IServiceHandlerWorkers ServiceHandlerWorker {
			set {
				_serviceHandlerWorker = value;
			}
			get {
				if (_serviceHandlerWorker == null)
				{
					_serviceHandlerWorker = new ServiceHandlerWorker();
				}
				return _serviceHandlerWorker;
			}
		}

		private IServiceRequestWorker _serviceRequestWorker;
		public virtual IServiceRequestWorker ServiceRequestWorker {
			set {
				_serviceRequestWorker = value;
			}
			get {
				if (_serviceRequestWorker == null)
				{
					_serviceRequestWorker = new ServiceRequestWorker();
				}
				return _serviceRequestWorker;
			}
		}
		//Log key=Integrator
		public virtual void ExportWithRequest(UserConnection userConnection, Guid id, string schemaName, string routeKey = null)
		{
			Export(userConnection, id, schemaName, routeKey, (iObject, handlerConfig, handler, entity) =>
			{
				ServiceRequestWorker.MakeRequest(userConnection, ServiceHandlerWorker, entity, handler, handlerConfig.Service, iObject.ToString());
			});
		}
		//Log key=Integrator
		public virtual void Export(UserConnection userConnection, Guid id, string schemaName, string routeKey = null, Action<IIntegrationObject, ConfigSetting, BaseEntityHandler, Entity> OnGet = null)
		{
			if (OnGet == null)
			{
				return;
			}
			try
			{
				var key = string.Format("{0}_{1}_{2}", id, schemaName, routeKey);
				LockerHelper.DoWithLock(key, () =>
				{
					routeKey = routeKey ?? schemaName;
					var handlerConfigs = ServiceHandlerWorker.GetConfigs(routeKey, CsConstant.TIntegrationType.Export);
					if (!handlerConfigs.Any())
					{
						IntegrationLogger.Warning("Не найдено конфигураций для " + routeKey);
					}
					schemaName = schemaName ?? handlerConfigs.First().EntityName;
					Entity entity = EntityPreparer.Get(userConnection, schemaName, id);
					foreach (var handlerConfig in handlerConfigs)
					{

						var handler = ServiceHandlerWorker.GetWithConfig(handlerConfig.Handler, handlerConfig);
						if (handler == null)
						{
							IntegrationLogger.Warning("Не найден обработчик " + handlerConfig.Handler);
							continue;
						}
						LoggerHelper.DoInLogBlock("Экспорт", () =>
						{
							IntegrationLogger.Info(LoggerInfo.GetMessage(handler.JName, entity, handler));
							var iObject = IObjectWorker.Get(userConnection, handler, entity);
							OnGet(iObject, handlerConfig, handler, entity);
						});
					}
				}, IntegrationLogger.SimpleLoggerErrorAction);
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e);
			}
		}
		//Log key=Integrator
		public virtual void Import(UserConnection userConnection, IIntegrationObject iObject, string routeKey = null, Action<CsConstant.IntegrationInfo> onSuccess = null, Action<CsConstant.IntegrationInfo, Exception> onError = null)
		{
			if (string.IsNullOrEmpty(routeKey))
			{
				routeKey = GetJNameFromJObject(iObject);
			}
			var handlerConfigs = ServiceHandlerWorker.GetConfigs(routeKey, CsConstant.TIntegrationType.Import);
			foreach (var handlerConfig in handlerConfigs)
			{
				LoggerHelper.DoInLogBlock("Импорт", () =>
				{
					IObjectWorker.Import(userConnection, ServiceHandlerWorker, handlerConfig, iObject, onSuccess, onError);
				});
			}
		}
		protected virtual string GetJNameFromJObject(IIntegrationObject jObject)
		{
			if (jObject != null)
			{
				return jObject.GetRootName(string.Empty);
			}
			return String.Empty;
		}
		//Log key=Integrator
		protected virtual ServiceConfig GetServiceConfig(string serviceName)
		{
			return ServiceHandlerWorker.GetServiceConfig(serviceName);
		}
		//Log key=Integrator
		protected virtual IIntegrationService GetService(string serviceName)
		{
			return ServiceHandlerWorker.GetService(serviceName);
		}
	}

	#endregion


	#region Class: RedirectRequestExceptionProcessStrategy
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Integrator\EndPoint\ExceptionProcessStrategy\RedirectRequestExceptionProcessStrategy.cs
		
	*/
	[ExceptionProcessStrategy("RedirectRequest")]
	public class RedirectRequestExceptionProcessStrategy: IExceptionProcessStrategy
	{
		private IServiceHandlerWorkers _serviceHandlerWorker;
		public virtual IServiceHandlerWorkers ServiceHandlerWorker {
			set {
				_serviceHandlerWorker = value;
			}
			get {
				if (_serviceHandlerWorker == null)
				{
					_serviceHandlerWorker = new ServiceHandlerWorker();
				}
				return _serviceHandlerWorker;
			}
		}
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

	#endregion


	#region Class: ExceptionProcessStrategyAttribute
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Integrator\EndPoint\ExceptionProcessStrategy\Factory\ExceptionProcessStrategyAttribute.cs
		
	*/
	[AttributeUsage(AttributeTargets.Class)]
	public class ExceptionProcessStrategyAttribute: Attribute
	{
		public string Name;
		public ExceptionProcessStrategyAttribute(string name)
		{
			Name = name;
		}
	}

	#endregion


	#region Class: ExceptionProcessStrategyFactory
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Integrator\EndPoint\ExceptionProcessStrategy\Factory\ExceptionProcessStrategyFactory.cs
		
	*/
	public class ExceptionProcessStrategyFactory: BaseAttributeFactory<IExceptionProcessStrategy, ExceptionProcessStrategyAttribute, string>
	{
		private static Dictionary<ExceptionProcessStrategyAttribute, IExceptionProcessStrategy> _instances;
		protected override Dictionary<ExceptionProcessStrategyAttribute, IExceptionProcessStrategy> Instances {
			get {
				if (_instances == null)
				{
					_instances = new Dictionary<ExceptionProcessStrategyAttribute, IExceptionProcessStrategy>();
				}
				return _instances;
			}
		}
		private static bool _isRuleRegister;
		protected override bool IsRuleRegister { get { return _isRuleRegister; } set { _isRuleRegister = value; } }
		protected override bool IsInstanceNameEqual(KeyValuePair<ExceptionProcessStrategyAttribute, IExceptionProcessStrategy> instanceKeyValue, string name)
		{
			return instanceKeyValue.Key.Name == name;
		}
	}

	#endregion


	#region Class: ExceptionProcessData
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Integrator\EndPoint\ExceptionProcessStrategy\Model\ExceptionProcessData.cs
		
	*/
	public class ExceptionProcessData
	{
		public Dictionary<string, string> Config;
		public string Route;
		public IIntegrationObject IObject;
		public CsConstant.IntegrationInfo Info;
		public Exception Exception;
		public string RequestContent;

		public string GetConfigValue(string name, string defaultValue = null)
		{
			if (Config != null && Config.ContainsKey(name))
			{
				return Config[name];
			}
			return defaultValue;
		}
	}

	#endregion


	#region Class: EndPointAttribute
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Integrator\EndPoint\Factory\EndPointAttribute.cs
		
	*/
	[AttributeUsage(AttributeTargets.Class)]
	public class EndPointAttribute : Attribute
	{
		public string Name;
		public EndPointAttribute(string name)
		{
			Name = name;
		}
	}

	#endregion


	#region Class: EndPointFactory
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Integrator\EndPoint\Factory\EndPointFactory.cs
		
	*/
	public class EndPointFactory: BaseAttributeFactory<IEndPointHandler, EndPointAttribute, string>
	{
		private static Dictionary<EndPointAttribute, IEndPointHandler> _instances;

		protected override Dictionary<EndPointAttribute, IEndPointHandler> Instances
		{
			get
			{
				if (_instances == null)
				{
					_instances = new Dictionary<EndPointAttribute, IEndPointHandler>();
				}
				return _instances;
			}
		}
		private static bool _isRuleRegister;
		protected override bool IsRuleRegister { get { return _isRuleRegister; } set { _isRuleRegister = value; } }
		protected override bool IsInstanceNameEqual(KeyValuePair<EndPointAttribute, IEndPointHandler> instanceKeyValue, string name)
		{
			return instanceKeyValue.Key.Name == name;
		}

		protected override void RegisterRule(EndPointAttribute attr, IEndPointHandler instance)
		{
			var endPointConfig = SettingsManager.GetEndPointConfigByHandler(attr.Name);
			if (endPointConfig != null)
			{
				instance.Config = endPointConfig.HandlerConfigs;
			}
			base.RegisterRule(attr, instance);
		}
	}

	#endregion


	#region Class: BaseEndPointHandler
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Integrator\EndPoint\Instance\BaseEndPointHandler.cs
		
	*/
	[EndPoint("BaseEndPoint")]
	public class BaseEndPointHandler: IEndPointHandler
	{
		public List<EndPointHandlerConfig> Config { get; set; }
		//Log key=EndPoint
		public string GetImportRoute(IIntegrationObject iObject)
		{
			var path = GetValue("RoutePath");
			var routeName = iObject.GetProperty<string>(path);;
			if (routeName != null)
			{
				var routeFormat = GetValue("Format");
				if (string.IsNullOrEmpty(routeFormat))
				{
					return routeName;
				}
				return string.Format(routeFormat, routeName);
			}
			return null;
		}
		//Log key=EndPoint
		public string GetValue(string key)
		{
			if (Config != null )
			{
				var keyValue = Config.FirstOrDefault(x => x.Key == key);
				if (keyValue != null)
				{
					return keyValue.Value;
				}
			}
			return null;
		}
	}

	#endregion


	#region Class: BaseIntegrationService
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Integrator\Integrator\BaseIntegrationService.cs
		
	*/
	[IntegrationService("BaseService")]
	public class BaseIntegrationService : IIntegrationService
	{
		//Log key=Integration Service
		public virtual WebRequest Create(ServiceConfig config, string content)
		{
			try
			{
				var request = WebRequest.Create(new Uri(config.Url)) as HttpWebRequest;
				request.Method = config.Method;
				if (config.Headers != null)
				{
					config.Headers.ForEach(header =>
					{
						request.SetHeaderValue(header.Key, header.Value);
					});
				}
				switch (config.Method)
				{
					case "POST":
					case "PUT":
						if (!string.IsNullOrEmpty(content))
						{
							AddDataToRequest(request, content);
						}
						break;
				}
				return request;
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(string.Format("Create Request Error\nConfig: {1}\nContent:{2}\nError: {0}", e, config, content));
			}
			return null;
		}
		//Log key=Integration Service
		protected virtual void AddDataToRequest(HttpWebRequest request, string data)
		{
			if (string.IsNullOrEmpty(data))
				return;
			IntegrationLogger.Info("Data:" + data);
			var encoding = new UTF8Encoding();
			var bytes = Encoding.UTF8.GetBytes(data);
			request.ContentLength = bytes.Length;
			using (var writeStream = request.GetRequestStream())
			{
				writeStream.Write(bytes, 0, bytes.Length);
			}
		}
		//Log key=Integration Service
		public virtual void SendRequest(WebRequest request, Action<WebResponse> OnResponse, Action<WebException> OnException)
		{
			try
			{
				WebResponse response = null;
				LoggerHelper.DoInLogBlock("Send Request", () =>
				{
					try
					{
						response = request.GetResponse();
					}
					catch (WebException e)
					{
						if (OnException != null)
						{
							OnException(e);
						}
					}
				});
				if (OnResponse != null)
				{
					OnResponse(response);
				}
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e);
			}
		}
		//Log key=Integration Service
		public virtual string GetContentFromResponse(WebResponse response)
		{
			using (StreamReader sr = new StreamReader(response.GetResponseStream()))
			{
				return sr.ReadToEnd();
			}
		}
	}

	#endregion


	#region Class: SyncIntegrator
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Integrator\Integrator\Sync\SyncIntegrator.cs
		
	*/
	[Override]
	public class SyncIntegrator : BaseIntegrator
	{
		private ISyncExportChecker<Guid> _syncExportChecker;
		public virtual ISyncExportChecker<Guid> SyncExportChecker {
			set {
				_syncExportChecker = value;
			}
			get {
				if (_syncExportChecker == null)
				{
					_syncExportChecker = new SyncValidator();
				}
				return _syncExportChecker;
			}
		}
		//Log key=Integration Sync
		public override void Export(UserConnection userConnection, Guid id, string schemaName, string routeKey = null, Action<IIntegrationObject, ConfigSetting, BaseEntityHandler, Entity> OnGet = null)
		{
			string exportRouteKey = routeKey ?? schemaName;
			if (SyncExportChecker.IsSyncEnable(exportRouteKey))
			{
				SyncExportChecker.DoInSync(userConnection, exportRouteKey, id, () => {
					base.Export(userConnection, id, schemaName, routeKey, OnGet);
				});
			}
			else
			{
				base.Export(userConnection, id, schemaName, routeKey, OnGet);
			}
		}
	}

	#endregion


	#region Class: SyncValidator
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Integrator\Integrator\Sync\SyncValidator.cs
		
	*/
	public class SyncValidator : ISyncExportChecker<Guid>
	{
		public class SyncValidatorInfo
		{
			public DateTime LastSyncDate;
			public Guid SyncId;
		}
		protected virtual string SyncTableName {
			get {
				return "TsiIntegrationSync";
			}
		}
		protected virtual string SyncRouteColumnName {
			get {
				return "TsiRoute";
			}
		}
		protected virtual string SyncKeyColumnName {
			get {
				return "TsiKey";
			}
		}
		protected virtual string SyncLastSyncDateColumnName {
			get {
				return "TsiLastSyncDate";
			}
		}
		//Log key=Integration Sync
		public virtual void DoInSync(UserConnection userConnection, string routeKey, Guid info, Action syncAction)
		{
			if (info == Guid.Empty)
			{
				return;
			}
			var routeConfig = SettingsManager.GetExportRoutes(routeKey).FirstOrDefault();
			SyncValidatorInfo lastSyncInfo = GetLastSyncDate(userConnection, routeKey, info);
			if (lastSyncInfo != null)
			{
				if ((DateTime.UtcNow - lastSyncInfo.LastSyncDate).TotalMilliseconds >= routeConfig.SyncMilliseconds)
				{
					syncAction();
					UpdateSyncDate(userConnection, lastSyncInfo);
				}
			}
			else
			{
				syncAction();
				InsertSyncDate(userConnection, routeKey, info);
			}
		}
		//Log key=Integration Sync
		public virtual bool IsSyncEnable(string routeKey)
		{
			var routeConfig = SettingsManager.GetExportRoutes(routeKey).FirstOrDefault();
			return routeConfig.IsSyncEnable;
		}
		//Log key=Integration Sync
		protected virtual SyncValidatorInfo GetLastSyncDate(UserConnection userConnection, string routeKey, Guid info)
		{
			var select = new Select(userConnection)
					.Top(1)
					.Column("Id")
					.Column(SyncLastSyncDateColumnName)
					.From(SyncTableName)
					.Where(SyncRouteColumnName).IsEqual(Column.Parameter(routeKey))
					.And(SyncKeyColumnName).IsEqual(Column.Parameter(info)) as Select;
			SyncValidatorInfo lastSyncInfo = null;
			using (var dbExecutor = userConnection.EnsureDBConnection())
			{
				using (var reader = select.ExecuteReader(dbExecutor))
				{
					if (reader.Read())
					{
						lastSyncInfo = new SyncValidatorInfo()
						{
							LastSyncDate = reader.GetColumnValue<DateTime>(SyncLastSyncDateColumnName),
							SyncId = reader.GetColumnValue<Guid>("Id")
						};
					}
				}
			}
			return lastSyncInfo;
		}
		//Log key=Integration Sync
		protected virtual void UpdateSyncDate(UserConnection userConnection, SyncValidatorInfo lastSyncInfo)
		{
			var update = new Update(userConnection, SyncTableName)
				.Set(SyncLastSyncDateColumnName, Column.Parameter(DateTime.UtcNow))
				.Where("Id").IsEqual(Column.Parameter(lastSyncInfo.SyncId)) as Update;
			update.Execute();
		}
		//Log key=Integration Sync
		protected virtual void InsertSyncDate(UserConnection userConnection, string routeKey, Guid info)
		{
			var insert = new Insert(userConnection)
				.Into(SyncTableName)
				.Set(SyncRouteColumnName, Column.Parameter(routeKey))
				.Set(SyncKeyColumnName, Column.Parameter(info))
				.Set(SyncLastSyncDateColumnName, Column.Parameter(DateTime.UtcNow)) as Insert;
			insert.Execute();
		}
	}

	#endregion


	#region Class: BaseIntegratorMock
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Integrator\Mock\BaseIntegratorMock.cs
		
	*/
	public class BaseIntegratorMock : BaseIntegrator
	{
		public string Mock { get; set; }
		protected override ServiceConfig GetServiceConfig(string serviceName)
		{
			var result = base.GetServiceConfig(serviceName);
			Mock = result.Mock;
			return result;
		}
		protected override IIntegrationService GetService(string serviceName)
		{
			var serviceMockConfig = SettingsManager.GetServiceMockConfig(Mock);
			return new ServiceMock(serviceMockConfig);
		}
	}

	#endregion


	#region Class: IntegrationObjectWorker
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Integrator\ObjectWorker\IntegrationObjectWorker.cs
		
	*/
	public class IntegrationObjectWorker : IIntegrationObjectWorker
	{
		//Log key=Integration Service
		public virtual IIntegrationObject Get(UserConnection userConnection, BaseEntityHandler handler, Entity entity)
		{
			var integrationInfo = CsConstant.IntegrationInfo.CreateForExport(userConnection, entity);
			integrationInfo.Handler = handler;
			return handler.ToJson(integrationInfo);
		}
		//Log key=Integration Service
		public virtual void Import(UserConnection userConnection, IServiceHandlerWorkers handlerWorker, ConfigSetting handlerConfig, IIntegrationObject iObject,
			Action<CsConstant.IntegrationInfo> onSuccess = null, Action<CsConstant.IntegrationInfo, Exception> onError = null)
		{
			var integrationInfo = CsConstant.IntegrationInfo.CreateForImport(userConnection,
					CsConstant.IntegrationActionName.Create, iObject);
			try
			{
				integrationInfo.Handler = handlerWorker.GetWithConfig(handlerConfig.Handler, handlerConfig);
				Import(integrationInfo);
				if (onSuccess != null)
				{
					onSuccess(integrationInfo);
				}
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e);
				if (onError != null)
				{
					onError(integrationInfo, e);
				}
			}
		}
		//Log key=Integration Service
		protected virtual void Import(CsConstant.IntegrationInfo integrationInfo)
		{
			var handler = integrationInfo.Handler;
			if (handler == null)
			{
				IntegrationLogger.Warning(string.Format("Обработчик не найден!\n{0}", integrationInfo));
				return;
			}
			string key = handler.GetKeyForLock(integrationInfo);
			LockerHelper.DoWithLock(key, () =>
			{
				if (!handler.IsEntityAlreadyExist(integrationInfo))
				{
					handler.Create(integrationInfo);
				}
				else
				{
					integrationInfo.Action = CsConstant.IntegrationActionName.Update;
					handler.Update(integrationInfo);
					return;
				}
			}, IntegrationLogger.SimpleLoggerErrorAction, throwException: true);
		}
	}

	#endregion


	#region Class: IntegrationServiceAttribute
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Integrator\Service\Attribute\IntegrationServiceAttribute.cs
		
	*/
	public class IntegrationServiceAttribute : System.Attribute
	{
		public string Name;
		public IntegrationServiceAttribute(string name)
		{
			Name = name;
		}
	}

	#endregion


	#region Class: HttpRequestHelper
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Integrator\Service\Extension\HttpRequestHelper.cs
		
	*/
	public static class HttpRequestHelper
	{
		public static void SetHeaderValue(this HttpWebRequest request, string key, string value)
		{
			if (HeaderSetterDict.ContainsKey(key))
			{
				HeaderSetterDict[key](request, value);
			}
			else
			{
				request.Headers.Add(key, value);
			}
		}
		#region Header Setter
		public static Dictionary<string, Action<HttpWebRequest, string>> HeaderSetterDict = new Dictionary<string, Action<HttpWebRequest, string>>()
		{
			{"Accept", (request, value) => request.Accept=value},
			{"Connection", (request, value) => request.Connection=value},
			{"Content-Length", (request, value) => request.ContentLength=long.Parse(value)},
			{"Content-Type", (request, value) => request.ContentType=value},
			{"Date", (request, value) => request.Date=DateTime.Parse(value)},
			{"Expect", (request, value) => request.Expect=value},
			{"Host", (request, value) => request.Host=value},
			{"If-Modified-Since", (request, value) => request.IfModifiedSince=DateTime.Parse(value)},
			{"Referer", (request, value) => request.Referer=value},
			{"Transfer-Encoding", (request, value) => request.TransferEncoding=value },
			{"User-Agent", (request, value) => request.UserAgent=value}
		};
		#endregion
	}

	#endregion


	#region Class: ServiceFactory
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Integrator\Service\Factory\ServiceFactory.cs
		
	*/
	public static class ServiceFactory
	{
		public static bool IsRegistred = false;
		public static Type ServiceAttrType = typeof(IntegrationServiceAttribute);
		public static ConcurrentDictionary<string, IIntegrationService> Services;
		public static void Register()
		{
			if (!IsRegistred)
			{
				var serviceDictionary = typeof(ServiceFactory)
					.Assembly
					.GetTypes()
					.Where(x => x.GetCustomAttributes(ServiceAttrType, false).Any())
					.Select(x => new
					{
						key = (x.GetCustomAttributes(ServiceAttrType, true).First() as IntegrationServiceAttribute).Name,
						value = Activator.CreateInstance(x) as IIntegrationService
					})
					.Where(x => x.value != null)
					.ToDictionary(x => x.key, x => x.value);
				Services = new ConcurrentDictionary<string, IIntegrationService>(serviceDictionary);
				IsRegistred = true;
			}
		}
		public static IIntegrationService Get(string name)
		{
			Register();
			if (Services != null && Services.ContainsKey(name))
			{
				return Services[name];
			}
			return null;
		}
	}

	#endregion


	#region Class: ServiceHandlerWorker
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Integrator\Service\Manager\ServiceHandlerWorker.cs
		
	*/
	public class ServiceHandlerWorker : IServiceHandlerWorkers
	{
		public List<ConfigSetting> GetConfigs(string routeKey, CsConstant.TIntegrationType type)
		{
			return SettingsManager.GetHandlerConfigs(routeKey, type);
		}
		public BaseEntityHandler GetWithConfig(string name, ConfigSetting config)
		{
			return HandlerFactory.Get(name, config);
		}
		public ServiceConfig GetServiceConfig(string serviceName)
		{
			return SettingsManager.GetServiceConfig(serviceName);
		}
		public IIntegrationService GetService(string serviceName)
		{
			return ServiceFactory.Get(serviceName);
		}
		public MappingConfig GetMappingConfig(string configId)
		{
			return SettingsManager.GetMappingConfig(configId);
		}
	}

	#endregion


	#region Class: ServiceMock
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Integrator\Service\Mock\ServiceMock.cs
		
	*/
	public class ServiceMock : BaseIntegrationService
	{
		ServiceMockConfig Config;
		public ServiceMock(ServiceMockConfig config)
		{
			IntegrationLogger.Info("Создание мока!");
			Config = config;
		}
		public override WebRequest Create(ServiceConfig config, string content)
		{
			IntegrationLogger.InfoFormat("Create Request\n{0}", LogConfig(config, content));
			return new WebRequestMock(Config);
		}
		protected virtual string LogConfig(ServiceConfig config, string content)
		{
			return string.Format("{0}\n{1}\n{2}\n{3}", config.Method, config.Url, config.Headers.Select(x => x.Key + " = " + x.Value).Aggregate((x, y) => x + ",\n" + y), content);
		}
	}

	#endregion


	#region Class: ServiceMockConfig
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Integrator\Service\Mock\ServiceMockConfig.cs
		
	*/
	public class ServiceMockConfig
	{
		public string Id { get; set; }
		public string Response { get; set; }
	}

	#endregion


	#region Class: WebRequestMock
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Integrator\Service\Mock\WebRequestMock.cs
		
	*/
	public class WebRequestMock : WebRequest
	{
		public ServiceMockConfig Config;
		public WebRequestMock(ServiceMockConfig config)
		{
			IntegrationLogger.Info("Создание мока запроса!");
			Config = config;
		}
		public override WebResponse GetResponse()
		{
			IntegrationLogger.InfoFormat("Get Response\ndata:\n{0}", Config.Response);
			var webResponse = new WebResponseMock(Config.Response);
			return webResponse;
		}
	}

	#endregion


	#region Class: WebResponseMock
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Integrator\Service\Mock\WebResponseMock.cs
		
	*/
	public class WebResponseMock : WebResponse
	{
		public MemoryStream Stream;
		public WebResponseMock(string response)
		{
			IntegrationLogger.Info("Создание мока ответа!");
			Stream = new MemoryStream();
			var expectedBytes = Encoding.UTF8.GetBytes(response);
			Stream.Write(expectedBytes, 0, expectedBytes.Length);
			Stream.Seek(0, SeekOrigin.Begin);
		}
		public override Stream GetResponseStream()
		{
			Stream.Seek(0, SeekOrigin.Begin);
			return Stream;
		}
	}

	#endregion


	#region Class: ServiceRequestWorker
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Integrator\Service\RequestWorker\ServiceRequestWorker.cs
		
	*/
	public class ServiceRequestWorker : IServiceRequestWorker
	{
		//Log key=Integration Service
		public void MakeRequest(UserConnection userConnection, IServiceHandlerWorkers serviceHandlerWorker, Entity entity, BaseEntityHandler handler, string serviceName, string content)
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

	#endregion


	#region Class: IntegrationEntityHelper
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Integrator\Utils\IntegrationEntityHelper.cs
		
	*/
	public class IntegrationEntityHelper
	{
		private static List<Type> IntegrationEntityTypes { get; set; }
		private static ConcurrentDictionary<Type, BaseEntityHandler> EntityHandlers { get; set; }
		private static Type DefaultHandlerType;
		public IntegrationEntityHelper()
		{
			EntityHandlers = new ConcurrentDictionary<Type, BaseEntityHandler>();
			RegisterDefaultHandler();
		}
		/// <summary>
		/// Експортирует или импортирует объекты в зависимости от настроек
		/// </summary>
		/// <param name="integrationInfo">Настройки интеграции</param>
		public void IntegrateEntity(IntegrationInfo integrationInfo)
		{
			ExecuteHandlerMethod(integrationInfo);
		}
		/// <summary>
		/// В зависимости от типа интеграции возвращает соответсвенный атрибут
		/// </summary>
		/// <param name="integrationInfo">Настройки интеграции</param>
		/// <returns></returns>

		/// <summary>
		/// Возвращает все классы помеченые атрибутами интеграции которые розмещены в пространстве имен Terrasoft.Configuration
		/// </summary>
		/// <param name="integrationInfo">Настройки интеграции</param>
		/// <returns></returns>
		public List<Type> GetIntegrationTypes(IntegrationInfo integrationInfo)
		{
			return GetIntegrationTypes(integrationInfo.IntegrationType);
		}
		public List<Type> GetIntegrationTypes(CsConstant.TIntegrationType integrationType)
		{
			if (IntegrationEntityTypes != null && IntegrationEntityTypes.Any())
			{
				return IntegrationEntityTypes;
			}
			var attributeType = typeof(IntegrationHandlerAttribute);
			var assembly = typeof(BaseEntityHandler).Assembly;
			return IntegrationEntityTypes = assembly.GetTypes().Where(x =>
			{
				var attributes = x.GetCustomAttributes(attributeType, true);
				return attributes != null && attributes.Length > 0;
			}).ToList();
		}
		public List<BaseEntityHandler> GetAllIntegrationHandler(List<ConfigSetting> handlerConfigs)
		{
			var handlers = new List<BaseEntityHandler>();
			foreach (var handlerConfig in handlerConfigs)
			{
				var attrType = typeof(IntegrationHandlerAttribute);
				var handlerType = SettingsManager
					.Handlers
					.FirstOrDefault(x => x.GetCustomAttributes(attrType, true).Any(y => ((IntegrationHandlerAttribute)y).Name == handlerConfig.Handler));
				if (handlerType != null)
				{
					var handler = Activator.CreateInstance(handlerType, handlerConfig) as BaseEntityHandler;
					handlers.Add(handler);
				}
			}
			return handlers;
		}
		/// <summary>
		/// В зависимости от настройки интеграции, выполняет соответсвенный метод объкта, который отвечает за интеграцию конкретной сущности
		/// </summary>
		/// <param name="integrationInfo">Настройки интеграции</param>
		/// <param name="handler">объект, который отвечает за интеграцию конкретной сущности</param>
		public void ExecuteHandlerMethod(IntegrationInfo integrationInfo)
		{
			BaseEntityHandler handler = integrationInfo.Handler;
			if (handler != null)
			{
				//Id - для уникальной блокировки интеграции. Блокируем по Id, EntityName, ServiceName и JName
				string key = handler.GetKeyForLock(integrationInfo);
				try
				{
					LockerHelper.DoWithLock(key, () => {
						//Export
						if (integrationInfo.IntegrationType == CsConstant.TIntegrationType.Export)
						{
							if (handler.IsExport(integrationInfo))
							{
								var result = new CsConstant.IntegrationResult(CsConstant.IntegrationResult.TResultType.Success, handler.ToJson(integrationInfo));
								integrationInfo.Result = result;
							}
							return;
						}
						//Export on Response
						if (integrationInfo.IntegrationType == CsConstant.TIntegrationType.ExportResponseProcess)
						{
							handler.ProcessResponse(integrationInfo);
							return;
						}
						//Import
						if (integrationInfo.Action == CsConstant.IntegrationActionName.Create)
						{
							if (!handler.IsEntityAlreadyExist(integrationInfo))
							{
								handler.Create(integrationInfo);
							}
							else
							{
								integrationInfo.Action = CsConstant.IntegrationActionName.Update;
								handler.Update(integrationInfo);
								return;
							}
						}
						else if (integrationInfo.Action == CsConstant.IntegrationActionName.Update)
						{
							if (handler.IsEntityAlreadyExist(integrationInfo))
							{
								handler.Update(integrationInfo);
							}
							else
							{
								integrationInfo.Action = CsConstant.IntegrationActionName.Create;
								handler.Create(integrationInfo);
							}
						}
						else if (integrationInfo.Action == CsConstant.IntegrationActionName.Delete)
						{
							handler.Delete(integrationInfo);
						}
						else
						{
							handler.Unknown(integrationInfo);
						}
					}, IntegrationLogger.SimpleLoggerErrorAction, string.Format("{0}", handler.EntityName));
				}
				catch (Exception e)
				{
					IntegrationLogger.Error(e);
				}
			}
		}
		/// <summary>
		/// Регистрирует обработчик интеграции по умолчанию
		/// Если таких несколько, то возвращает обработчинк с большим полем Index в атрибуте
		/// </summary>
		public static void RegisterDefaultHandler()
		{
			if (DefaultHandlerType != null)
			{
				return;
			}
			var assembly = typeof(IntegrationEntityHelper).Assembly;
			var attrType = typeof(DefaultHandlerAttribute);
			var defaultHandlerItem = assembly
				.GetTypes()
				.Where(x => x.HasAttribute(attrType))
				.Select(x => new
				{
					type = x,
					attr = x.GetCustomAttributes(attrType, false).FirstOrDefault() as DefaultHandlerAttribute
				})
				.Where(x => x.attr != null)
				.OrderByDescending(x => x.attr.Index)
				.FirstOrDefault();
			if (defaultHandlerItem != null)
			{
				DefaultHandlerType = defaultHandlerItem.type;
			}
		}
	}

	#endregion


	#region Class: IntegrationService
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Integrator\WCFService\IntegrationService.cs
		
	*/
	[ServiceContract]
	[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
	public class IntegrationService
	{
		private HttpContextBase _httpContext;
		protected virtual HttpContextBase CurrentHttpContext {
			get { return _httpContext ?? (_httpContext = new HttpContextWrapper(HttpContext.Current)); }
			set { _httpContext = value; }
		}
		private UserConnection _userConnection;
		protected virtual UserConnection UserConnection {
			get {
				if (_userConnection != null)
				{
					return _userConnection;
				}
				_userConnection = CurrentHttpContext.Session["UserConnection"] as UserConnection;
				if (_userConnection != null)
				{
					return _userConnection;
				}
				var appConnection = (AppConnection)CurrentHttpContext.Application["AppConnection"];
				_userConnection = appConnection.SystemUserConnection;
				return _userConnection;
			}
		}
		private IIntegrationObjectProvider _iObjectProvider;
		public virtual IIntegrationObjectProvider IObjectProvider {
			set {
				_iObjectProvider = value;
			}
			get {
				if (_iObjectProvider == null)
				{
					_iObjectProvider = new IntegrationObjectProvider();
				}
				return _iObjectProvider;
			}
		}
		[OperationContract]
		[WebGet(UriTemplate = "entity/{routeKey}/{id}")]
		public virtual Stream Get(string routeKey, string id)
		{
			return new IntegrationServiceWrapper(UserConnection, IObjectProvider).Get(routeKey, id);
		}

		[OperationContract]
		[WebInvoke(Method = "POST", UriTemplate = "entity/{routeKey}", BodyStyle = WebMessageBodyStyle.Bare)]
		public virtual Stream Post(string routeKey, Stream requestStream)
		{
			return new IntegrationServiceWrapper(UserConnection, IObjectProvider).Post(routeKey, requestStream);
		}
		[OperationContract]
		[WebInvoke(Method = "PUT", UriTemplate = "entity/{routeKey}", BodyStyle = WebMessageBodyStyle.Bare)]
		public virtual Stream Put(string routeKey, Stream requestStream)
		{
			return new IntegrationServiceWrapper(UserConnection, IObjectProvider).Put(routeKey, requestStream);
		}
		[OperationContract]
		[WebInvoke(Method = "POST", UriTemplate = "integrate/{endPointName}", BodyStyle = WebMessageBodyStyle.Bare)]
		public virtual Stream PostEndPoint(string endPointName, Stream requestStream)
		{
			return new IntegrationServiceWrapper(UserConnection, IObjectProvider).PostEndPoint(endPointName, requestStream);
		}
	}

	#endregion


	#region Class: IntegrationServiceWrapper
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Integrator\WCFService\IntegrationServiceWrapper.cs
		
	*/
	public class IntegrationServiceWrapper
	{
		public IntegrationServiceWrapper(UserConnection userConnection)
		{
			_userConnection = userConnection;
		}
		public IntegrationServiceWrapper(UserConnection userConnection, IIntegrationObjectProvider iObjectProvider)
		{
			_userConnection = userConnection;
			_iObjectProvider = iObjectProvider;
		}
		private HttpContextBase _httpContext;
		protected virtual HttpContextBase CurrentHttpContext {
			get { return _httpContext ?? (_httpContext = new HttpContextWrapper(HttpContext.Current)); }
			set { _httpContext = value; }
		}
		private UserConnection _userConnection;
		protected virtual UserConnection UserConnection {
			get {
				if (_userConnection != null)
				{
					return _userConnection;
				}
				_userConnection = CurrentHttpContext.Session["UserConnection"] as UserConnection;
				if (_userConnection != null)
				{
					return _userConnection;
				}
				var appConnection = (AppConnection)CurrentHttpContext.Application["AppConnection"];
				_userConnection = appConnection.SystemUserConnection;
				return _userConnection;
			}
		}
		private IIntegrationObjectProvider _iObjectProvider;
		public virtual IIntegrationObjectProvider IObjectProvider {
			set {
				_iObjectProvider = value;
			}
			get {
				if (_iObjectProvider == null)
				{
					_iObjectProvider = new IntegrationObjectProvider();
				}
				return _iObjectProvider;
			}
		}
		//Log: key=Integration Service
		public virtual Stream Get(string routeKey, string id)
		{
			if (UserConnection == null)
			{
				return null;
			}
			SettingsManager.UserConnection = UserConnection;
			SettingsManager.InitIntegrationSettings();
			SettingsManager.ReinitXmlConfigSettings();
			var integrator = ClassFactory.Get<BaseIntegrator>();
			IIntegrationObject integrObject = null;
			integrator.Export(UserConnection, new Guid(id), null, routeKey,
				(iObject, handlerConfig, handler, entity) =>
				{
					integrObject = iObject;
				});
			if (integrObject != null)
			{
				var stream = IObjectProvider.GetMemoryStream(integrObject);
				WebOperationContext.Current.OutgoingResponse.ContentType = IObjectProvider.GetContentType(integrObject);
				return stream;
			}
			return null;
		}
		//Log: key=Integration Service
		public virtual Stream Post(string routeKey, Stream requestStream)
		{
			return Import(routeKey, requestStream);
		}
		//Log: key=Integration Service
		public virtual Stream Put(string routeKey, Stream requestStream)
		{
			return Import(routeKey, requestStream);
		}
		//Log: key=Integration Service
		public virtual Stream PostEndPoint(string endPointName, Stream requestStream)
		{
			SettingsManager.UserConnection = UserConnection;
			SettingsManager.InitIntegrationSettings();
			SettingsManager.ReinitXmlConfigSettings();
			var endPointConfig = SettingsManager.GetEndPointConfig(endPointName);
			if (endPointConfig != null)
			{
				var endPointHandler = new EndPointFactory().Get(endPointConfig.EndPointHandler);
				if (UserConnection == null)
				{
					return null;
				}
				string content = null;
				using (StreamReader reader = new StreamReader(requestStream))
				{
					content = reader.ReadToEnd();
				}
				var integrObject = IObjectProvider.Parse(content);
				var route = endPointHandler.GetImportRoute(integrObject);
				var integrator = ClassFactory.Get<BaseIntegrator>();
				Stream responseStream = null;
				integrator.Import(UserConnection, integrObject, route, integrationInfo =>
				{
					if (integrationInfo.IntegratedEntity != null)
					{
						var exportRoute = string.Format(endPointConfig.GetHandlerConfig("SuccesExportRouteFormat"), route);
						responseStream = Get(exportRoute, integrationInfo.IntegratedEntity.PrimaryColumnValue.ToString());
					}
				}, (info, exception) =>
				{
					responseStream = ProcessImportException(new ExceptionProcessData()
					{
						Exception = exception,
						IObject = integrObject,
						Info = info,
						Route = route,
						RequestContent = content
					}, endPointConfig);
				});
				WebOperationContext.Current.OutgoingResponse.ContentType = IObjectProvider.GetContentType(integrObject);
				return responseStream;
			}
			WebOperationContext.Current.OutgoingResponse.ContentType = "text/plain";
			return Error(string.Format("End Point {0} not found!", endPointName));
		}
		//Log: key=Integration Service
		protected Stream ProcessImportException(ExceptionProcessData processData, EndPointConfig endPointConfig)
		{
			var exceptionName = processData.Exception.GetType().Name;
			bool isCanProcessException = endPointConfig.GetHandlerConfig("ProcessException", "")
				.Split(';')
				.Any(x => x == exceptionName);
			if (isCanProcessException)
			{
				var strategyName = endPointConfig.GetHandlerConfig("ProcessStrategyHandler_" + exceptionName, "DefaultProcessStrategy");
				var strategy = new ExceptionProcessStrategyFactory().Get(strategyName);
				processData.Config = endPointConfig.GetHandlerConfig("ProcessStrategyHandlerConfig_" + exceptionName, "")
					.Split(';')
					.Where(x => !string.IsNullOrEmpty(x))
					.Select(x => x.Split('=').Where(y => !string.IsNullOrEmpty(y)).ToList())
					.Where(x => x.Count > 1)
					.ToDictionary(x => x[0], x => x[1]);
				return strategy.Process(processData);
			}
			return null;
		}
		//Log key=Integration Service
		protected virtual Stream Import(string routeKey, Stream requestStream)
		{
			if (UserConnection == null)
			{
				return null;
			}
			SettingsManager.UserConnection = UserConnection;
			SettingsManager.InitIntegrationSettings();
			SettingsManager.ReinitXmlConfigSettings();
			string content = null;
			using (StreamReader reader = new StreamReader(requestStream))
			{
				content = reader.ReadToEnd();
			}
			var integrObject = IObjectProvider.Parse(content);
			return Import(routeKey, integrObject);
		}
		//Log key=Integration Service
		protected virtual Stream Import(string routeKey, IIntegrationObject iObject)
		{
			var integrator = ClassFactory.Get<BaseIntegrator>();
			Guid resultId = Guid.Empty;
			integrator.Import(UserConnection, iObject, routeKey, integrationInfo =>
			{
				if (integrationInfo.IntegratedEntity != null)
				{
					resultId = integrationInfo.IntegratedEntity.PrimaryColumnValue;
				}
			});
			if (resultId != Guid.Empty)
			{
				return Get(routeKey, resultId.ToString());
			}
			WebOperationContext.Current.OutgoingResponse.ContentType = "text/plain";
			return Error("Результат не найдено");
		}
		//Log key=Integration Service
		protected virtual Stream Error(string message)
		{
			var stream = new MemoryStream();
			var encoding = new UTF8Encoding();
			var encodeData = encoding.GetBytes(message);
			stream.Write(encodeData, 0, encodeData.Length);
			stream.Seek(0, SeekOrigin.Begin);
			return stream;
		}
	}

	#endregion


	#region Class: TsIntegrationCoreService
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Integrator\WCFService\TsIntegrationCoreService.cs
		
	*/
	[ServiceContract]
	[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
	public class TsIntegrationCoreService
	{
		private global::Common.Logging.ILog _log;

		public global::Common.Logging.ILog Log {
			get {
				if (_log == null)
				{
					_log = global::Common.Logging.LogManager.GetLogger("TscIntegration") ??
						global::Common.Logging.LogManager.GetLogger("Common");
				}
				return _log;
			}
		}
		[OperationContract]
		[WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, RequestFormat = WebMessageFormat.Json,
			ResponseFormat = WebMessageFormat.Json)]
		public MappingServiceResponse SaveMappingConfig(string config, string action, Guid Id)
		{
			var response = new MappingServiceResponse();
			try
			{
				var userConnection = (UserConnection)HttpContext.Current.Session["UserConnection"];
				var helper = new TsIntegrationCodeServiceHelper(userConnection);
				switch (action)
				{
					case "create":
						var id = helper.CreateNewConfig(config, Id);
						if (id != Guid.Empty)
						{
							response.Data = id.ToString();
						}
						break;
					case "update":
						helper.UpdateConfig(config, Id);
						break;
				}
				helper.ReinitSettings();
			}
			catch (Exception e)
			{
				Log.Error(e);
				response.Exception = e;
			}
			return response;
		}
		[OperationContract]
		[WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, RequestFormat = WebMessageFormat.Json,
			ResponseFormat = WebMessageFormat.Json)]
		public string GetMappingConfig(Guid Id)
		{
			try
			{
				var userConnection = (UserConnection)HttpContext.Current.Session["UserConnection"];
				var helper = new TsIntegrationCodeServiceHelper(userConnection);
				return helper.GetConfigByJson(Id);
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
			return string.Empty;
		}
		[OperationContract]
		[WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, RequestFormat = WebMessageFormat.Json,
			ResponseFormat = WebMessageFormat.Json)]
		public string TestToJson(TestExportInfo testConfig)
		{
			try
			{
				var userConnection = (UserConnection)HttpContext.Current.Session["UserConnection"];
				var helper = new TsIntegrationCodeServiceHelper(userConnection);
				return helper.TestToJson(testConfig);
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
			return string.Empty;
		}
		[OperationContract]
		[WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, RequestFormat = WebMessageFormat.Json,
			ResponseFormat = WebMessageFormat.Json)]
		public string TestToEntity(TestImportInfo testConfig)
		{
			try
			{
				var userConnection = (UserConnection)HttpContext.Current.Session["UserConnection"];
				var helper = new TsIntegrationCodeServiceHelper(userConnection);
				return helper.TestToEntity(testConfig);
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
			return string.Empty;
		}
		[OperationContract]
		[WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, RequestFormat = WebMessageFormat.Json,
			ResponseFormat = WebMessageFormat.Json)]
		public List<EntitySchemaInfo> GetAllEntityInfo()
		{
			var userConnection = (UserConnection)HttpContext.Current.Session["UserConnection"];
			var helper = new TsIntegrationCodeServiceHelper(userConnection);
			return helper.GetAllEntityNames();
		}
		[OperationContract]
		[WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, RequestFormat = WebMessageFormat.Json,
			ResponseFormat = WebMessageFormat.Json)]
		public string GetLogBlockForAnalyse(Guid blockId)
		{
			var userConnection = (UserConnection)HttpContext.Current.Session["UserConnection"];
			var helper = new TsIntegrationCodeServiceHelper(userConnection);
			return helper.GetBlockLogDataForAnalyze(blockId);
		}
		[OperationContract]
		[WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, RequestFormat = WebMessageFormat.Json,
			ResponseFormat = WebMessageFormat.Json)]
		public void TestServiceByMock(TestServiceInfo info)
		{
			var userConnection = (UserConnection)HttpContext.Current.Session["UserConnection"];
			var helper = new TsIntegrationCodeServiceHelper(userConnection);
			helper.TestServiceByMock(info);
		}
		[OperationContract]
		[WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, RequestFormat = WebMessageFormat.Json,
			ResponseFormat = WebMessageFormat.Json)]
		public void RunTrigger(TriggerServiceInfo info)
		{
			LoggerHelper.DoInLogBlock("Run Trigger " + info.TriggerName, () =>
			{
				try
				{
					var userConnection = (UserConnection)HttpContext.Current.Session["UserConnection"];
					var triggerEngine = new TriggerEngine();
					var triggerInfo = triggerEngine.GetTriggerByName(info.TriggerName, userConnection);
					if (triggerInfo != null)
					{
						var integrator = ClassFactory.Get<BaseIntegrator>();
						integrator.ExportWithRequest(userConnection, info.PrimaryColumnValue, info.SchemaName, triggerInfo.Route);
					}
					else
					{
						IntegrationLogger.Error(string.Format("Trigger {0} not found!", info.TriggerName));
					}
				}
				catch (Exception e)
				{
					IntegrationLogger.Error(e);
				}
			});
		}
	}

	#endregion


	#region Class: MappingServiceResponse
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Integrator\WCFService\DataModel\MappingServiceResponse.cs
		
	*/
	[DataContract]
	public class MappingServiceResponse : BaseResponse
	{
		[DataMember(Name = "data")]
		public string Data;
		#region Constructors: Public

		public MappingServiceResponse()
		{
			Success = true;
		}

		public MappingServiceResponse(Exception e)
		{
			Exception = e;
		}

		#endregion

		#region Properties: Public

		public Exception Exception {
			set {
				Success = false;
				ResponseStatus = SetResponseStatus(value);
				ErrorInfo = SetErrorInfo(value);
			}
		}

		#endregion

		#region Methods: public

		public virtual ResponseStatus SetResponseStatus(Exception e)
		{
			return new ResponseStatus
			{
				ErrorCode = e.GetType().Name,
				Message = e.Message,
				StackTrace = e.StackTrace
			};
		}

		public virtual ErrorInfo SetErrorInfo(Exception e)
		{
			return new ErrorInfo
			{
				ErrorCode = e.GetType().Name,
				Message = e.Message,
				StackTrace = e.StackTrace
			};
		}

		#endregion
	}

	#endregion


	#region Class: ServiceRequestInfo
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Integrator\_Object\ServiceRequestInfo.cs
		
	*/
	public class ServiceRequestInfo
	{
		public TRequstMethod Method;
		public string FullUrl;
		public string RequestJson;
		public string ResponseData;
		public Entity Entity;
		public Action AfterIntegrate;
		public BaseEntityHandler Handler;
		public bool UpdateIfExist = true;
		public Action<int, int> ProgressAction;
		public int IntegrateCount = 0;
		public int TotalCount = 0;
		public string Auth;
		public static ServiceRequestInfo CreateForExportInBpm()
		{
			return new ServiceRequestInfo();
		}
		public void SetProgress(int progressed, int allCount)
		{
			if (ProgressAction != null)
			{
				ProgressAction(progressed, allCount);
			}
		}
	}

	#endregion


	#region Class: IntegrationLocker
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Locker\IntegrationLocker.cs
		
	*/
	public static class IntegrationLocker
	{
		public static ConcurrentDictionary<string, bool> LockerInfo = new ConcurrentDictionary<string, bool>();
		private static bool isLocckerActive {
			get {
				return CsConstant.IntegratorSettings.isLockerActive;
			}
		}
		public static void Lock(string id, string keyMixin = null)
		{
			if (!isLocckerActive)
			{
				return;
			}
			var key = GetKey(id, keyMixin);
			IntegrationLogger.LockInfo(string.Format("Lock => Schema Name: {0} Id: {1}", id, key));
			if (!LockerInfo.ContainsKey(key))
			{
				LockerInfo.TryAdd(key, true);
			}
		}
		public static void Unlock(string id, string keyValue = null)
		{
			if (!isLocckerActive)
			{
				return;
			}
			var key = GetKey(id, keyValue);
			IntegrationLogger.UnlockInfo(string.Format("Unlock => Schema Name: {0} Id: {1}", id, key));
			if (LockerInfo.ContainsKey(key))
			{
				bool removeItem;
				LockerInfo.TryRemove(key, out removeItem);
			}
		}

		public static bool CheckWithUnlock(string key, string keyValue = null)
		{
			if (!isLocckerActive)
			{
				return true;
			}
			if (!CheckUnLock(key, keyValue))
			{
				Unlock(key);
			}
			return CheckUnLock(key);
		}
		public static bool CheckUnLock(string key, string keyValue = null)
		{
			return !isLocckerActive || !LockerInfo.ContainsKey(GetKey(key, keyValue));
		}

		private static string GetKey(string key, string keyValue)
		{
			return string.Format("{0}_{1}_{2}", key, Thread.CurrentThread.ManagedThreadId, keyValue ?? "!");
		}
	}

	#endregion


	#region Class: LockerHelper
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Locker\LockerHelper.cs
		
	*/
	public static class LockerHelper
	{
		public static void DoWithLock(string key, Action action, Action<Exception> OnExceptionAction = null, string keyMixin = null, bool withLock = true, bool withCheckLock = true, bool throwException = false)
		{
			if (!withLock || !withCheckLock || IntegrationLocker.CheckUnLock(key, keyMixin))
			{
				if (withLock)
				{
					IntegrationLocker.Lock(key, keyMixin);
				}
				try
				{
					action();
				}
				catch (Exception e)
				{
					if (OnExceptionAction != null)
					{
						OnExceptionAction(e);
					}
					if (throwException)
					{
						throw;
					}
				}
				finally
				{
					if (withLock)
					{
						IntegrationLocker.Unlock(key, keyMixin);
					}
				}
			}
		}
	}

	#endregion


	#region Class: IntegrationLogger
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Logger\IntegrationLogger.cs
		
	*/
	public static class IntegrationLogger
	{
		public static Action<Exception> SimpleLoggerErrorAction = e => IntegrationLogger.Error(e);
		private static TsLogger _log = new TsLogger();
		public static ConcurrentDictionary<int, Guid> ThreadLogIds = new ConcurrentDictionary<int, Guid>();
		public static int CurrentThreadId {
			get { return Thread.CurrentThread.ManagedThreadId; }
		}
		public static Guid CurrentLogBlockId {
			get {
				if (ThreadLogIds.ContainsKey(CurrentThreadId))
				{
					return ThreadLogIds[CurrentThreadId];
				}
				return Guid.Empty;
			}
			set {
				if (value == Guid.Empty)
				{
					//Если Empty, то чистим запись в этом потоке
					Guid oldBlockId;
					ThreadLogIds.TryRemove(CurrentThreadId, out oldBlockId);
					return;
				}
				if (ThreadLogIds.ContainsKey(CurrentThreadId))
				{
					ThreadLogIds[CurrentThreadId] = value;
				}
				else
				{
					ThreadLogIds.TryAdd(CurrentThreadId, value);
				}
			}
		}
		public static TsLogger CurrentLogger {
			get {
				return _log;
			}
		}
		public static Guid StartLogBlock(string blockName, TLogObjectType type = TLogObjectType.Block)
		{
			try
			{
				var oldBlockId = CurrentLogBlockId;
				CurrentLogBlockId = Guid.NewGuid();
				CurrentLogger.CreateBlock(blockName, CurrentLogBlockId, oldBlockId, type);
				return oldBlockId;
			}
			catch (Exception e2)
			{
				CurrentLogger.Instance.Error(e2.ToString());
			}
			return Guid.Empty;
		}

		public static void FinishLogBlock()
		{
			CurrentLogger.UpdateBlockTime(CurrentLogBlockId, DateTime.UtcNow);
		}
		public static void Error(string message)
		{
			try
			{
				CurrentLogger.Error(CurrentLogBlockId, message);
			}
			catch (Exception e2)
			{
				CurrentLogger.Instance.Info(e2.ToString());
			}
		}
		public static void ErrorFormat(string format, params object[] args)
		{
			Error(string.Format(format, args));
		}
		public static void Error(Exception e)
		{
			try
			{
				CurrentLogger.Error(CurrentLogBlockId, e.ToString());
			}
			catch (Exception e2)
			{
				CurrentLogger.Instance.Info(e2.ToString());
			}
		}
		public static void ErrorMapping(string message)
		{
			try
			{
				CurrentLogger.ErrorMapping(CurrentLogBlockId, message);
			}
			catch (Exception e2)
			{
				CurrentLogger.Instance.Info(e2.ToString());
			}
		}
		/// <summary>
		/// Логгирование в файл
		/// </summary>
		/// <param name="message">Сообщение</param>
		public static void Info(string message, TLogObjectType type = TLogObjectType.Info)
		{
			try
			{
				CurrentLogger.Info(CurrentLogBlockId, message, type);
			}
			catch (Exception e)
			{
				CurrentLogger.Instance.Info(e.ToString());
			}
		}
		public static void InfoFormat(string format, params object[] args)
		{
			Info(string.Format(format, args));
		}
		public static void InfoTypeFormat(string format, TLogObjectType type = TLogObjectType.Info, params object[] args)
		{
			Info(string.Format(format, args), type);
		}

		public static void InfoArguments(string key, string format, params object[] args)
		{
			InfoTypeFormat(format, TLogObjectType.InfoMethodArgs, args);
		}

		public static void InfoReturn(string key, object arg)
		{
			InfoTypeFormat("Return: {0}", TLogObjectType.InfoMethodResult, arg);
		}
		public static void Warning(string message)
		{
			try
			{
				CurrentLogger.Info(CurrentLogBlockId, message, TLogObjectType.Warning);
			}
			catch (Exception e)
			{
				CurrentLogger.Instance.Info(e.ToString());
			}
		}
		public static void LockInfo(string message)
		{
			try
			{
				CurrentLogger.Info(CurrentLogBlockId, message, TLogObjectType.Lock);
			}
			catch (Exception e)
			{
				CurrentLogger.Instance.Info(e.ToString());
			}
		}
		public static void UnlockInfo(string message)
		{
			try
			{
				CurrentLogger.Info(CurrentLogBlockId, message, TLogObjectType.Unlock);
			}
			catch (Exception e)
			{
				CurrentLogger.Instance.Info(e.ToString());
			}
		}
		public static void InfoMapping(string message)
		{
			try
			{
				CurrentLogger.InfoMapping(CurrentLogBlockId, message);
			}
			catch (Exception e)
			{
				CurrentLogger.Instance.Info(e.ToString());
			}
		}

		internal static void WarningFormat(string format, params object[] args)
		{
			Warning(string.Format(format, args));
		}
	}

	#endregion


	#region Class: LoggerHelper
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Logger\LoggerHelper.cs
		
	*/
	public static class LoggerHelper
	{
		/// <summary>
		/// Гарантирует выполнение Action в транзакции логгирования
		/// </summary>
		/// <param name="info">Информация о транзакции</param>
		/// <param name="action">Предикат</param>
		public static void DoInLogBlock(string blockName, Action action)
		{
			try
			{
				var oldBlockId = CreateBlock(blockName);
				try
				{
					action();
				}
				catch (Exception e)
				{
					IntegrationLogger.Error(e);
				}
				finally
				{
					FinishTransaction(oldBlockId);
				}
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e);
			}
		}
		/// <summary>
		/// Создает транзакцию логгирования
		/// </summary>
		/// <param name="info">Информация о транзакции</param>
		/// <returns>Возвращает используется уже рание создана транзакция или новая</returns>
		public static Guid CreateBlock(string blockName, TLogObjectType type = TLogObjectType.Block)
		{
			return IntegrationLogger.StartLogBlock(blockName, type);
		}
		/// <summary>
		/// Завершает текущую транзакцию
		/// </summary>
		/// <param name="info">Информация о транзакции</param>
		public static void FinishTransaction(Guid oldBlockId)
		{
			IntegrationLogger.FinishLogBlock();
			IntegrationLogger.CurrentLogBlockId = oldBlockId;
		}

		public static bool IsActive(string logName)
		{
			var logConfig = SettingsManager.GetLogConfig(logName);
			if (logConfig != null)
			{
				return logConfig.IsActive;
			}
			return false;
		}
	}

	#endregion


	#region Class: LoggerInfo
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Logger\LoggerInfo.cs
		
	*/
	public static class LoggerInfo
	{
		public static string GetMessage(string jName, Entity entity, BaseEntityHandler handler)
		{
			return string.Format("Integrate Object Name={0}\n{1}\nHandler Info={2}", jName, GetEntityInfo(entity), handler);
		}
		public static string GetEntityInfo(Entity entity)
		{
			try
			{
				if (entity != null)
				{
					return string.Format("Entity Name={0}, Id={1}, Name={2}", entity.GetType(), entity.PrimaryColumnValue, entity.PrimaryDisplayColumnValue);
				}
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e);
			}
			return string.Empty;
		}
	}

	#endregion


	#region Class: LoggerState
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Logger\LoggerState.cs
		
	*/
	public class LoggerState
	{
		public Guid TransactionId;
		public Guid LastRequestId;
	}

	#endregion


	#region Class: TsLogger
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Logger\TsLogger.cs
		
	*/
	public class TsLogger
	{
		private global::Common.Logging.ILog _log;
		private global::Common.Logging.ILog _updateLog;
		private global::Common.Logging.ILog _emptyLog;

		public global::Common.Logging.ILog Instance {
			get {
				return _log ?? _emptyLog;
			}
		}
		public global::Common.Logging.ILog UpdateInstance {
			get {
				return _updateLog ?? _emptyLog;
			}
		}

		public UserConnection userConnection;

		public TsLogger()
		{
			_log = global::Common.Logging.LogManager.GetLogger("TscIntegration") ??
				   global::Common.Logging.LogManager.GetLogger("Common");
			_updateLog = global::Common.Logging.LogManager.GetLogger("TscUpdateIntegration");
			_emptyLog = global::Common.Logging.LogManager.GetLogger("NotExistingLogger");
		}
		public void CreateBlock(string blockName, Guid blockId, Guid parentBlockId, TLogObjectType type = TLogObjectType.Block)
		{
			SetBlockType(Instance, type);
			SetBlockId(Instance, blockId);
			SetParentBlockId(Instance, parentBlockId);
			SetLoggerVariable(UpdateInstance, "CreatedDate", DateTime.UtcNow);
			Instance.Info(blockName);
		}

		public void UpdateBlockTime(Guid blockId, DateTime endDateTime)
		{
			SetLoggerVariable(UpdateInstance, "FinishDateTime", endDateTime.ToUniversalTime());
			SetBlockId(UpdateInstance, blockId);
			UpdateInstance.Info(string.Empty);
		}
		public void Error(Guid blockId, string errorMessage, TLogObjectType type = TLogObjectType.Error)
		{
			try
			{
				if (blockId == Guid.Empty)
				{
					return;
				}
				SetBlockType(Instance, type);
				SetParentBlockId(Instance, blockId);
				SetBlockId(Instance, Guid.NewGuid());
				Instance.Info(errorMessage);
			}
			catch (Exception e)
			{
				Instance.Info(e.ToString());
			}
		}
		public void ErrorMapping(Guid blockId, string errorMessage)
		{
			Error(blockId, errorMessage, TLogObjectType.ErrorMapping);
		}
		public void Info(Guid blockId, string message, TLogObjectType type = TLogObjectType.Info)
		{
			try
			{
				if (blockId == Guid.Empty)
				{
					return;
				}
				SetBlockType(Instance, type);
				SetParentBlockId(Instance, blockId);
				SetBlockId(Instance, Guid.NewGuid());
				Instance.Info(message);
			}
			catch (Exception e)
			{
				Instance.Info(e.ToString());
			}
		}
		public void InfoMapping(Guid blockId, string message)
		{
			Info(blockId, message, TLogObjectType.InfoMapping);
		}
		public static void SetBlockId(global::Common.Logging.ILog Instance, Guid id)
		{
			Instance.ThreadVariablesContext.Set("BlockId", id.ToString("B").ToUpper());
		}
		public static void SetParentBlockId(global::Common.Logging.ILog Instance, Guid id)
		{
			Instance.ThreadVariablesContext.Set("ParentBlockId", id.ToString("B").ToUpper());
		}
		public static void SetBlockType(global::Common.Logging.ILog Instance, TLogObjectType type)
		{
			Instance.ThreadVariablesContext.Set("Type", (int)type);
		}

		public static void SetLoggerVariable(global::Common.Logging.ILog Instance, string name, object value)
		{
			Instance.ThreadVariablesContext.Set(name, value);
		}
	}

	#endregion


	#region Class: MappingErrorLoggerInfo
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Logger\Templater\MappingErrorLoggerInfo.cs
		
	*/
	public static class MappingErrorLoggerInfo
	{
		public static string GetMessage(Exception e, MappingItem item, CsConstant.IntegrationInfo integrationInfo)
		{
			return string.Format("Mapping Error\nError={0}\nMapping Item={1}\nIntegration Info={2}", e, item, integrationInfo);
		}
	}

	#endregion


	#region Class: RequestErrorLoggerInfo
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Logger\Templater\RequestErrorLoggerInfo.cs
		
	*/
	public static class RequestErrorLoggerInfo
	{
		internal static string GetMessage(Exception e, string responceText)
		{
			return string.Format("Request Error\nError={0}\nResponse={1}", e, responceText);
		}
	}

	#endregion


	#region Class: RequestLoggerInfo
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Logger\Templater\RequestLoggerInfo.cs
		
	*/
	public static class RequestLoggerInfo
	{
		public static string GetMessage(TRequstMethod requestMethod, string url, string auth, string jsonText)
		{
			return string.Format("Requst Info\nMethod={0}\nUrl={1}\nAuth={2}\nText={3}", requestMethod, url, auth, jsonText);
		}
	}

	#endregion


	#region Class: ResponseLoggerInfo
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Logger\Templater\ResponseLoggerInfo.cs
		
	*/
	public static class ResponseLoggerInfo
	{
		internal static string GetMessage(string responseText)
		{
			return string.Format("Response=\n{0}", responseText);
		}
	}

	#endregion


	#region Class: MacrosExportAttribute
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Mapping\Macros\Attribute\MacrosExportAttribute.cs
		
	*/
	public class MacrosExportAttribute : System.Attribute
	{
		public MacrosType Type;
		public string Name;
		public MacrosExportAttribute(string name, MacrosType type = MacrosType.Rule)
		{
			Name = name;
			Type = type;
		}
	}

	#endregion


	#region Class: MacrosImportAttribute
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Mapping\Macros\Attribute\MacrosImportAttribute.cs
		
	*/
	public class MacrosImportAttribute : System.Attribute
	{
		public MacrosType Type;
		public string Name;
		public MacrosImportAttribute(string name, MacrosType type = MacrosType.Rule)
		{
			Name = name;
			Type = type;
		}
	}

	#endregion


	#region Class: MacrosFactory
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Mapping\Macros\Factory\MacrosFactory.cs
		
	*/
	public static class MacrosFactory
	{
		public static bool IsMacrosRegistred = false;
		public static Dictionary<string, Func<object, object>> MacrosDictImport = new Dictionary<string, Func<object, object>>() { };
		public static Dictionary<string, Func<object, object>> MacrosDictExport = new Dictionary<string, Func<object, object>>() { };
		public static Dictionary<string, Func<object, CsConstant.IntegrationInfo, object>> OverMacrosDictImport = new Dictionary<string, Func<object, CsConstant.IntegrationInfo, object>>() { };
		public static Dictionary<string, Func<object, CsConstant.IntegrationInfo, object>> OverMacrosDictExport = new Dictionary<string, Func<object, CsConstant.IntegrationInfo, object>>() { };
		//TODO доделать реестрацию макросов "перед удалением"
		public static Dictionary<string, Action<object, UserConnection>> BeforeDeleteMacros = new Dictionary<string, Action<object, UserConnection>>() { };
		public static void RegisterMacros()
		{
			if (IsMacrosRegistred)
			{
				return;
			}
			var assembly = typeof(MacrosFactory).Assembly;
			var ruleAttrType = typeof(MacrosImportAttribute);
			var ruleExportType = typeof(MacrosExportAttribute);
			try
			{
				assembly
					.GetTypes()
					.Where(x => x.HasAttribute(ruleAttrType) || x.HasAttribute(ruleExportType))
					.ForEach(x =>
					{
						var attributess = x.GetCustomAttributes(ruleAttrType, true).ToList();
						attributess.AddRange(x.GetCustomAttributes(ruleExportType, true).ToList());
						if (attributess == null || !attributess.Any())
						{
							return;
						}
						attributess
							.ForEach(attr =>
							{
								if (attr is MacrosImportAttribute)
								{
									var macrosAttr = (MacrosImportAttribute)attr;
									if (macrosAttr.Type == MacrosType.Rule)
									{
										var macrosCreator = Activator.CreateInstance(x) as IMacrosCreator;
										if (macrosCreator != null)
										{
											MacrosDictImport.Add(macrosAttr.Name, macrosCreator.Create());
										}
									}
									else if (macrosAttr.Type == MacrosType.OverRule)
									{
										var macrosCreator = Activator.CreateInstance(x) as IMacrosOverRuleCreator;
										if (macrosCreator != null)
										{
											OverMacrosDictImport.Add(macrosAttr.Name, macrosCreator.Create());
										}
									}
								}
								else
								{
									var macrosAttr = (MacrosExportAttribute)attr;
									if (macrosAttr.Type == MacrosType.Rule)
									{
										var macrosCreator = Activator.CreateInstance(x) as IMacrosCreator;
										if (macrosCreator != null)
										{
											MacrosDictExport.Add(macrosAttr.Name, macrosCreator.Create());
										}
									}
									else if (macrosAttr.Type == MacrosType.OverRule)
									{
										var macrosCreator = Activator.CreateInstance(x) as IMacrosOverRuleCreator;
										if (macrosCreator != null)
										{
											OverMacrosDictExport.Add(macrosAttr.Name, macrosCreator.Create());
										}
									}
								}
							});
					});
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e);
			}
			IsMacrosRegistred = true;
		}

		public static object GetMacrosResultImport(string macrosName, object value, MacrosType type = MacrosType.Rule, CsConstant.IntegrationInfo integrationInfo = null)
		{
			RegisterMacros();
			switch (type)
			{
				case MacrosType.Rule:
					if (MacrosDictImport.ContainsKey(macrosName) && MacrosDictImport[macrosName] != null)
					{
						return MacrosDictImport[macrosName](value);
					}
					return value;
				case MacrosType.OverRule:
					if (OverMacrosDictImport.ContainsKey(macrosName) && OverMacrosDictImport[macrosName] != null)
					{
						return OverMacrosDictImport[macrosName](value, integrationInfo);
					}
					return value;
				default:
					return value;
			}

		}
		public static object GetMacrosResultExport(string macrosName, object value, MacrosType type = MacrosType.Rule, CsConstant.IntegrationInfo integrationInfo = null)
		{
			RegisterMacros();
			switch (type)
			{
				case MacrosType.Rule:
					if (MacrosDictExport.ContainsKey(macrosName) && MacrosDictExport[macrosName] != null)
					{
						return MacrosDictExport[macrosName](value);
					}
					return value;
				case MacrosType.OverRule:
					if (OverMacrosDictExport.ContainsKey(macrosName) && OverMacrosDictExport[macrosName] != null)
					{
						return OverMacrosDictExport[macrosName](value, integrationInfo);
					}
					return value;
				default:
					return value;
			}
		}
		public static void ExecuteBeforeDeleteMacros(string macrosName, object value, UserConnection userConnection)
		{
			RegisterMacros();
			if (BeforeDeleteMacros.ContainsKey(macrosName))
			{
				BeforeDeleteMacros[macrosName](value, userConnection);
			}
		}
	}

	#endregion


	#region Class: DateFormatMacros
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Mapping\Macros\Instace\DateFormatMacros.cs
		
	*/
	[MacrosExport("DateFormat")]
	public class DateFormatMacros : IMacrosCreator
	{
		public Func<object, object> Create()
		{
			return (x) =>
			{
				DateTime result;
				if (x != null && x is string && DateTime.TryParse((string)x, out result))
				{
					return result.ToString("MM-dd-yyyy");
				}
				return x;
			};
		}
	}

	#endregion


	#region Class: DateFormatMacros2
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Mapping\Macros\Instace\DateFormatMacros2.cs
		
	*/
	[MacrosExport("DateFormat2")]
	public class DateFormatMacros2 : IMacrosCreator
	{
		public Func<object, object> Create()
		{
			return (x) =>
			{
				DateTime result;
				if (x != null && x is string && DateTime.TryParse((string)x, out result))
				{
					return result.ToString("dd.MM.yyyy");
				}
				return x;
			};
		}
	}

	#endregion


	#region Class: DateTimeMacros
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Mapping\Macros\Instace\DateTimeMacros.cs
		
	*/
	[MacrosImport("DateTimeParse")]
	public class DateTimeMacros : IMacrosCreator
	{
		public Func<object, object> Create()
		{
			return (x) =>
			{
				if (x != null && x is string)
				{
					DateTime result;
					if (DateTime.TryParse(x.ToString(), out result))
					{
						return result;
					}
				}
				return x;
			};
		}
	}

	#endregion


	#region Class: EpochDateTime
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Mapping\Macros\Instace\EpochDateTime.cs
		
	*/
	[MacrosImport("EpochDateTime")]
	public class EpochDateTime : IMacrosCreator
	{
		private static readonly DateTime _epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		public Func<object, object> Create()
		{
			return (x) =>
			{
				if (x != null && x is string)
				{
					long time;
					if (long.TryParse((string) x, out time))
					{
						return _epoch.AddSeconds(time);
					}
				}
				return x;
			};
		}
	}

	#endregion


	#region Class: GetOnlyNumberMacros
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Mapping\Macros\Instace\GetOnlyNumberMacros.cs
		
	*/
	[MacrosImport("GetOnlyNumber")]
	public class GetOnlyNumberMacros : IMacrosCreator
	{
		public Func<object, object> Create()
		{
			return (x) =>
			{
				if (x != null && x is string)
				{
					var regex = new Regex(@"^-?\d+(?:\,\d+)?(?:\.\d+)?");
					Match match = regex.Match((string)x);
					if (match.Success)
					{
						return decimal.Parse(match.Value);
					}
				}
				return x;
			};
		}
	}

	#endregion


	#region Class: IntToLowMacros
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Mapping\Macros\Instace\IntToLowMacros.cs
		
	*/
	[MacrosExport("IntToLow")]
	public class IntToLowMacros : IMacrosCreator
	{
		public Func<object, object> Create()
		{
			return (x) =>
			{
				decimal decValue = 0;
				if (x != null)
				{
					if (x is string && decimal.TryParse((string)x, out decValue))
					{
						return decimal.ToInt64(decValue);
					}
					if (x is decimal)
					{
						return decimal.ToInt64((decimal)x);
					}
				}
				return x;
			};
		}
	}

	#endregion


	#region Class: IntegrationMapper
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Mapping\Mapper\IntegrationMapper.cs
		
	*/
	public class IntegrationMapper : IMapper
	{
		public List<MappingItem> MapConfig;
		public Queue<Action> MethodQueue;
		public RulesFactory RulesFactory;
		private IMapperDbWorker _mapperDbWorker;
		public virtual IMapperDbWorker MapperDbWorker {
			set {
				_mapperDbWorker = value;
			}
			get {
				if (_mapperDbWorker == null)
				{
					_mapperDbWorker = new MapperDbWorker();
				}
				return _mapperDbWorker;
			}
		}
		private IIntegrationObjectProvider _integrationObjectProvider;
		public virtual IIntegrationObjectProvider IntegrationObjectProvider {
			set {
				_integrationObjectProvider = value;
			}
			get {
				if (_integrationObjectProvider == null)
				{
					_integrationObjectProvider = new IntegrationObjectProvider();
				}
				return _integrationObjectProvider;
			}
		}
		public IntegrationMapper()
		{
			MethodQueue = new Queue<Action>();
			RulesFactory = new RulesFactory();
		}
		public virtual void StartMappByConfig(CsConstant.IntegrationInfo integrationInfo, string jName, MappingConfig mapConfig)
		{
			try
			{
				if (mapConfig == null)
				{
					return;
				}
				LoggerHelper.DoInLogBlock("Process Mapping", () =>
				{
					IntegrationLogger.InfoMapping(string.Format("Mapping Start\nIntegration Object: \"{0}\"\nIntegration Type: {1}", jName, integrationInfo.IntegrationType));
					switch (integrationInfo.IntegrationType)
					{
						case CsConstant.TIntegrationType.Import:
							{
								StartMappImportByConfig(integrationInfo, jName, mapConfig.Items);
								break;
							}
						case CsConstant.TIntegrationType.Export:
							{
								StartMappExportByConfig(integrationInfo, jName, mapConfig.Items);
								break;
							}
						case CsConstant.TIntegrationType.ExportResponseProcess:
							{
								StartMappExportResponseProcessByConfig(integrationInfo, jName, mapConfig.Items);
								break;
							}
					}
				});
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e);
				throw;
			}
		}
		public virtual bool CheckIsExist(UserConnection userConnection, string entityName, object externalId, string externalIdPath = "TsExternalId", object entityExternalId = null)
		{
			if (entityExternalId != null && entityExternalId.ToString() != string.Empty && entityExternalId.ToString() != "0")
			{
				return true;
			}
			if (externalId == null || string.IsNullOrEmpty(externalId.ToString()) || externalId.ToString() == "0")
			{
				return false;
			}
			return MapperDbWorker.IsExists(userConnection, entityName, externalIdPath, externalId);
		}
		//Log key=Mapper
		protected virtual void StartMappImportByConfig(IntegrationInfo integrationInfo, string jName, List<MappingItem> mapConfig)
		{
			if (integrationInfo.IntegratedEntity == null)
				throw new Exception(string.Format("Integration Entity not exist {0} ({1})", jName));
			var entityJObj = integrationInfo.Data;
			foreach (var item in mapConfig)
			{
				if (item.MapIntegrationType == CsConstant.TIntegrationType.All || item.MapIntegrationType == CsConstant.TIntegrationType.Import)
				{
					try
					{
						IIntegrationObject subJObj = null;
						if (!string.IsNullOrEmpty(item.Selector))
						{
							subJObj = entityJObj.GetSubObject(item.Selector);
						}
						else
						{
							var path = IntegrationPath.GeneratePath(jName, item.JSourcePath);
							subJObj = entityJObj.GetSubObject(path);
						}
						if (subJObj != null)
						{
							MapColumn(integrationInfo.UserConnection, item, ref subJObj, integrationInfo);
						}
					}
					catch (Exception e)
					{
						IntegrationLogger.Error(e);
						if (CsConstant.IntegrationFlagSetting.AllowErrorOnColumnAssign)
						{
							throw;
						}
					}
				}
			}
		}
		//Log key=Mapper
		protected virtual void StartMappExportByConfig(IntegrationInfo integrationInfo, string jName, List<MappingItem> mapConfig)
		{
			integrationInfo.Data = IntegrationObjectProvider.NewInstance(jName);
			foreach (var item in mapConfig)
			{
				if (item.MapIntegrationType == CsConstant.TIntegrationType.All || item.MapIntegrationType == CsConstant.TIntegrationType.Export)
				{
					IIntegrationObject jObjItem = IntegrationObjectProvider.NewInstance(item.JSourcePath);
					try
					{
						MapColumn(integrationInfo.UserConnection, item, ref jObjItem, integrationInfo);
					}
					catch (Exception e)
					{
						IntegrationLogger.ErrorMapping(MappingErrorLoggerInfo.GetMessage(e, item, integrationInfo));
						if (!item.IgnoreError)
						{
							throw;
						}
						jObjItem = null;
					}
					var path = IntegrationPath.GeneratePath(jName, item.JSourcePath);
					integrationInfo.Data.SetProperty(path, jObjItem);
				}
			}
		}
		//Log key=Mapper
		protected virtual void StartMappExportResponseProcessByConfig(IntegrationInfo integrationInfo, string jName, List<MappingItem> mapConfig)
		{
			foreach (var item in mapConfig)
			{
				try
				{
					if (item.SaveOnResponse)
					{
						var path = IntegrationPath.GeneratePath(jName, item.JSourcePath);
						if (item.IsArrayItem)
						{
							var subJObjs = integrationInfo.Data.GetSubObjects(path);
							subJObjs.ForEach(subJObj =>
							{
								MapColumn(integrationInfo.UserConnection, item, ref subJObj, integrationInfo);
							});
						}
						else
						{
							var subJObj = integrationInfo.Data.GetSubObject(path);
							MapColumn(integrationInfo.UserConnection, item, ref subJObj, integrationInfo);
						}
					}
				}
				catch (Exception e)
				{
					IntegrationLogger.Error(e);
					if (CsConstant.IntegrationFlagSetting.AllowErrorOnColumnAssign)
					{
						throw;
					}
				}
			}
		}
		//Log key=Mapper
		protected virtual void MapColumn(UserConnection userConnection, MappingItem mapItem, ref IIntegrationObject jToken, IntegrationInfo integrationInfo)
		{
			try
			{
				var entity = integrationInfo.IntegratedEntity;
				var integrationType = integrationInfo.IntegrationType;
				Action executedMethod = new Action(() => { });
				var rule = RulesFactory.GetRule(mapItem.MapType.ToString());
				if (rule != null)
				{
					RuleInfo ruleInfo = null;
					switch (integrationInfo.IntegrationType)
					{
						case TIntegrationType.ExportResponseProcess:
						case TIntegrationType.Import:
							ExecuteOverRuleMacros(mapItem, ref jToken, integrationInfo);
							ruleInfo = new RuleImportInfo()
							{
								config = mapItem,
								entity = integrationInfo.IntegratedEntity,
								json = jToken,
								userConnection = userConnection,
								integrationType = integrationInfo.IntegrationType,
								action = integrationInfo.Action
							};
							executedMethod = () => rule.Import((RuleImportInfo)ruleInfo);
							if (mapItem.MapExecuteType == TMapExecuteType.BeforeEntitySave)
							{
								executedMethod();
								var importRuleInfo = ruleInfo as RuleImportInfo;
								if (importRuleInfo.AfterEntitySave != null)
								{
									MethodQueue.Enqueue(importRuleInfo.AfterEntitySave);
								}
							}
							else
							{
								MethodQueue.Enqueue(executedMethod);
							}
							break;
						case TIntegrationType.Export:
							ruleInfo = new RuleExportInfo()
							{
								config = mapItem,
								entity = integrationInfo.IntegratedEntity,
								json = jToken,
								userConnection = userConnection,
								integrationType = integrationInfo.IntegrationType,
								action = integrationInfo.Action
							};
							rule.Export((RuleExportInfo)ruleInfo);
							jToken = ruleInfo.json;
							ExecuteOverRuleMacros(mapItem, ref jToken, integrationInfo);
							break;
					}
				}
			}
			catch (Exception e)
			{
				IntegrationLogger.ErrorMapping(MappingErrorLoggerInfo.GetMessage(e, mapItem, integrationInfo));
			}
		}
		//Log key=Mapper
		protected virtual void ExecuteOverRuleMacros(MappingItem mapItem, ref IIntegrationObject jToken, IntegrationInfo integrationInfo)
		{
			if (mapItem.OverRuleMacros.IsNullOrEmpty() || (!mapItem.AllowNullToOverMacros && jToken == null))
			{
				return;
			}
			switch (integrationInfo.IntegrationType)
			{
				case TIntegrationType.ExportResponseProcess:
				case TIntegrationType.Import:
					jToken = MacrosFactory.GetMacrosResultImport(mapItem.OverRuleMacros, jToken, MacrosType.OverRule, integrationInfo) as IIntegrationObject;
					break;
				case TIntegrationType.Export:
					jToken.SetObject(MacrosFactory.GetMacrosResultExport(mapItem.OverRuleMacros, jToken, MacrosType.OverRule, integrationInfo));
					break;
			}
		}
		//Log key=Mapper
		public virtual void ExecuteMapMethodQueue()
		{
			while (MethodQueue.Any())
			{
				var method = MethodQueue.Dequeue();
				try
				{
					method();
				}
				catch (Exception e)
				{
					IntegrationLogger.Error(e);
				}
			}
		}
	}

	#endregion


	#region Class: DependentEntityLoader
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Mapping\Plugin\DependentEntityLoader.cs
		
	*/
	public static class DependentEntityLoader
	{
		public static ConcurrentDictionary<int, int> ThreadLoadEntityLevel = new ConcurrentDictionary<int, int>();
		public static int CurrentThreadId {
			get {
				return Thread.CurrentThread.ManagedThreadId;
			}
		}
		/// <summary>
		/// Инициирует загрузку связаного объекта, если уровень загрузки не привышает значение системной настройки LoadDependentEntityLevel
		/// </summary>
		/// <param name="name">Имя в сервисе</param>
		/// <param name="externalId">Идентификатор в сервисе</param>
		/// <param name="userConnection">Подключение пользователя</param>
		/// <param name="afterIntegrate">Срабатывает после завершения интеграции. Если импорт не инициировался, то предикат не вызывается</param>
		/// <param name="onException">Срабатывает на исключение</param>
		public static void LoadDependenEntity(string name, int externalId, UserConnection userConnection, Action afterIntegrate = null, Action<Exception> onException = null)
		{
			try
			{
				AddCurrentLevel();
				if (!string.IsNullOrEmpty(name) && externalId > 0 && CheckCurrentLoadLevel())
				{
					try
					{
						var serviceRequestInfo = ServiceRequestInfo.CreateForExportInBpm();
						serviceRequestInfo.UpdateIfExist = true;
						//var integrator = new BaseServiceIntegrator(userConnection);
						//if (integrator != null)
						//{
						//	integrator.GetRequest(serviceRequestInfo);
						//}
					}
					catch (Exception e)
					{
						if (onException != null)
						{
							onException(e);
						}
					}
				}
			}
			catch (Exception e)
			{
				if (onException != null)
				{
					onException(e);
				}
			}
			finally
			{
				DecCurrentLevel();
			}
			if (afterIntegrate != null)
			{
				afterIntegrate();
			}
		}
		public static bool CheckCurrentLoadLevel()
		{
			var level = GetCurrentLevel();
			if (level < 0)
			{
				IntegrationLogger.Error(new Exception("level is less than zero!"));
				return false;
			}
			return level <= 2;
		}
		public static int GetCurrentLevel()
		{
			int level = -1;
			ThreadLoadEntityLevel.TryGetValue(CurrentThreadId, out level);
			return level;
		}

		public static void AddCurrentLevel()
		{
			if (!ThreadLoadEntityLevel.ContainsKey(CurrentThreadId))
			{
				ThreadLoadEntityLevel.TryAdd(CurrentThreadId, 0);
			}
			ThreadLoadEntityLevel[CurrentThreadId]++;
		}
		public static void DecCurrentLevel()
		{
			if (!ThreadLoadEntityLevel.ContainsKey(CurrentThreadId))
			{
				ThreadLoadEntityLevel.TryAdd(CurrentThreadId, 0);
			}
			ThreadLoadEntityLevel[CurrentThreadId]--;
		}
	}

	#endregion


	#region Class: DynamicXmlParser
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Mapping\Plugin\DynamicXmlParser.cs
		
	*/
	public static class DynamicXmlParser
	{
		public static T StartMapXmlToObj<T>(XElement node, Type objType = null, object defObj = null, Func<string, string> prepareValuePredicate = null)
		{
			object resultObj = null;
			objType = objType ?? typeof(T);
			if (defObj == null)
			{
				resultObj = Activator.CreateInstance(objType);
			}
			else
			{
				resultObj = defObj.CloneObject();
			}
			var columnsName = objType.GetProperties().Where(x => x.MemberType == MemberTypes.Property).Select(x => x.Name).ToList();
			foreach (var columnName in columnsName)
			{
				PropertyInfo propertyInfo = objType.GetProperty(columnName);
				var xmlAttr = node.Attribute(columnName);
				if (xmlAttr != null)
				{
					string value = xmlAttr.Value;
					if (!string.IsNullOrEmpty(value))
					{
						if (prepareValuePredicate != null)
						{
							value = prepareValuePredicate(value);
						}
						var propertyType = propertyInfo.PropertyType;
						if (propertyType.IsEnum || propertyType == typeof(int))
						{
							propertyInfo.SetValue(resultObj, int.Parse(value));
						}
						else if (propertyType.IsEnum || propertyType == typeof(ulong))
						{
							propertyInfo.SetValue(resultObj, ulong.Parse(value));
						}
						else if (propertyType == typeof(bool))
						{
							propertyInfo.SetValue(resultObj, value != "0");
						}
						else
						{
							propertyInfo.SetValue(resultObj, value);
						}
					}
				}
			}
			if (resultObj is T)
			{
				return (T)resultObj;
			}
			return default(T);
		}
	}

	#endregion


	#region Class: MapperDbWorker
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Mapping\Plugin\MapperDbWorker.cs
		
	*/
	public class MapperDbWorker : IMapperDbWorker
	{
		public bool IsExists(UserConnection userConnection, string entityName, string externalIdPath, object externalId)
		{
			var select = new Select(userConnection)
							.Column(Func.Count(Column.Const(1))).As("Count")
							.From(entityName)
							.Where(externalIdPath).IsEqual(Column.Parameter(externalId)) as Select;
			using (DBExecutor dbExecutor = userConnection.EnsureDBConnection())
			{
				using (IDataReader reader = select.ExecuteReader(dbExecutor))
				{
					while (reader.Read())
					{
						return DBUtilities.GetColumnValue<int>(reader, "Count") > 0;
					}
				}
			}
			return false;
		}
	}

	#endregion


	#region Class: RuleAttribute
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Mapping\Rules\Attribute\RuleAttribute.cs
		
	*/
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class RuleAttribute : System.Attribute
	{
		public string Name;
		public TIntegrationObjectType DataType;
		public RuleAttribute(string name, TIntegrationObjectType dataType = TIntegrationObjectType.Json)
		{
			Name = name.ToLower();
			DataType = dataType;
		}
	}

	#endregion


	#region Class: RuleFactoryItem
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Mapping\Rules\Factory\RuleFactoryItem.cs
		
	*/
	public class RuleFactoryItem
	{
		public RuleAttribute Attribute;
		public IMappRule Rule;

		public RuleFactoryItem(RuleAttribute attribute, IMappRule rule)
		{
			Attribute = attribute;
			Rule = rule;
		}
	}

	#endregion


	#region Class: RulesFactory
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Mapping\Rules\Factory\RulesFactory.cs
		
	*/
	public class RulesFactory
	{
		private TIntegrationObjectType? _objectType;
		public TIntegrationObjectType ObjectType {
			get {
				if (_objectType == null)
				{
					try
					{
						_objectType = SettingsManager.GetIntegratorSetting<TIntegrationObjectType>("TsIntegrationObjectType");
					}
					catch (Exception e)
					{
						IntegrationLogger.Error(e);
					}
					if (_objectType == null)
					{
						_objectType = TIntegrationObjectType.Json;
					}
				}
				return _objectType.Value;
			}
		}
		public RulesFactory()
		{
			RegisterRules();
		}

		/// <summary>
		/// Возвращает правила по его имени
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public IMappRule GetRule(string name)
		{
			if (string.IsNullOrEmpty(name))
			{
				return null;
			}
			var factoryItem = Rules.FirstOrDefault(x => x.Attribute.Name == name.ToLower() && x.Attribute.DataType == ObjectType);
			return factoryItem == null ? null : factoryItem.Rule;
		}

		#region Static Field
		/// <summary>
		/// Правыла маппинга
		/// </summary>
		public static List<RuleFactoryItem> Rules;
		/// <summary>
		/// Признак что правила прошли реестрацию
		/// </summary>
		public static bool IsRuleRegister = false;
		/// <summary>
		/// Ищет по сборке все классы с атрибутом RuleAttribute и для каждого атрибута
		/// создает инстанс правила
		/// </summary>
		public static void RegisterRules()
		{
			if (Rules == null)
			{
				Rules = new List<RuleFactoryItem>();
			}
			if (IsRuleRegister)
			{
				return;
			}
			var assembly = typeof(RulesFactory).Assembly;
			var ruleAttrType = typeof(RuleAttribute);
			assembly
				.GetTypes()
				.Where(x => x.HasAttribute(ruleAttrType))
				.ForEach(x =>
				{
					var attributess = x.GetCustomAttributes(ruleAttrType, true);
					if (attributess == null || attributess.Length == 0)
					{
						return;
					}
					attributess
						.Where(attr => AttributeDataValidate(attr as RuleAttribute))
						.ForEach(attr => Rules.Add(new RuleFactoryItem((RuleAttribute)attr, CreateRuleInstanse(x))));
				});
			IsRuleRegister = true;
		}
		/// <summary>
		/// Проверяет правило на соответсвия настройкам интеграции
		/// </summary>
		/// <param name="attribute"></param>
		/// <returns></returns>
		public static bool AttributeDataValidate(RuleAttribute attribute)
		{
			return true;
		}
		/// <summary>
		/// Создает инстанс правила по его типу
		/// </summary>
		/// <param name="ruleType"></param>
		/// <returns></returns>
		public static IMappRule CreateRuleInstanse(Type ruleType)
		{
			return Activator.CreateInstance(ruleType) as IMappRule;
		}
		#endregion
	}

	#endregion


	#region Class: ArrayOfCompositeObjectMappRule
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Mapping\Rules\Instance\Json\ArrayOfCompositeObjectMappRule.cs
		
	*/
	[RuleAttribute("ArrayOfCompositeObject")]
	public class ArrayOfCompositeObjectMappRule : IMappRule
	{
		public ArrayOfCompositeObjectMappRule()
		{
		}
		public void Import(RuleImportInfo info)
		{
			if (info.integrationType == CsConstant.TIntegrationType.ExportResponseProcess && !info.config.SaveOnResponse)
			{
				return;
			}
			if (info.json is JArray)
			{
				var jArray = (JArray)info.json;
				var handlerName = info.config.HandlerName;
				var integrator = new IntegrationEntityHelper();
				var integrateIds = new List<QueryColumnExpression>();
				try
				{
					foreach (JToken jArrayItem in jArray)
					{
						JObject jObj = jArrayItem as JObject;
						handlerName = handlerName ?? jObj.Properties().First().Name;
						CsConstant.IntegrationInfo objIntegrInfo = null;
						objIntegrInfo.ParentEntity = info.entity;
						integrator.IntegrateEntity(objIntegrInfo);
						if (info.config.DeleteBeforeExport)
						{
							integrateIds.Add(Column.Parameter(objIntegrInfo.IntegratedEntity.GetTypedColumnValue<Guid>("Id")));
						}
					}
					if (info.config.DeleteBeforeExport)
					{
						EntitySchema entitySchema = info.userConnection.EntitySchemaManager.GetInstanceByName(info.config.TsDestinationName);
						string destColumnName = JsonEntityHelper.GetSqlNameByEntity(entitySchema, info.config.TsDestinationPath);
						if (integrateIds != null && integrateIds.Any())
						{
							var idSelect = new Select(info.userConnection)
											.Column("Id")
											.From(info.config.TsDestinationName)
											.Where("Id").Not().In(integrateIds)
											.And(destColumnName).In(
												new Select(info.userConnection)
													.Column(destColumnName)
													.From(info.config.TsDestinationName)
													.Where("Id").In(integrateIds)
											)
											as Select;
							if (!string.IsNullOrEmpty(info.config.BeforeDeleteMacros))
							{
								MacrosFactory.ExecuteBeforeDeleteMacros(info.config.BeforeDeleteMacros, idSelect, info.userConnection);
							}
							var delete = new Delete(info.userConnection)
									.From(info.config.TsDestinationName)
									.Where("Id").In(idSelect) as Delete;
							delete.Execute();
						}
						else
						{
							var delete = new Delete(info.userConnection)
									.From(info.config.TsDestinationName)
									.Where(destColumnName).IsEqual(Column.Parameter(info.entity.GetColumnValue(info.config.TsSourcePath))) as Delete;
							delete.Execute();
						}
					}
				}
				catch (Exception e)
				{
					throw new Exception("Mapp Rule arrayofcompositobject, import", e);
				}
			}
		}
		public void Export(RuleExportInfo info)
		{
			try
			{
				if (JsonEntityHelper.IsAllNotNullAndEmpty(info.entity, info.config.TsSourcePath, info.config.TsDestinationPath, info.config.TsDestinationName))
				{
					//var srcEntity = info.entity;
					//var dscValue = srcEntity.GetColumnValue(info.config.TsSourcePath);
					//string handlerName = JsonEntityHelper.GetFirstNotNull(info.config.HandlerName, info.config.TsDestinationName, info.config.JSourceName);
					//var resultJObjs = JsonEntityHelper.GetCompositeJObjects(dscValue, info.config.TsDestinationPath, info.config.TsDestinationName, handlerName, info.userConnection);
					//if (resultJObjs.Any()) {
					//	var jArray = (info.json = new JArray()) as JArray;
					//	resultJObjs.ForEach(x => jArray.Add(x));
					//} else {
					//	info.json = null;
					//}
				}
			}
			catch (Exception e)
			{
				throw new Exception("Mapp Rule arrayofcompositobject, export", e);
			}
		}
	}

	#endregion


	#region Class: ArrayOfReferenceMappRule
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Mapping\Rules\Instance\Json\ArrayOfReferenceMappRule.cs
		
	*/
	[RuleAttribute("ArrayOfReference")]
	public class ArrayOfReferenceMappRule : IMappRule
	{
		public ArrayOfReferenceMappRule()
		{
		}
		public void Import(RuleImportInfo info)
		{
			try
			{
				if (info.json != null && info.json is JArray)
				{
					var jArray = (JArray)info.json;
					Action integrateRelatedEntity = () =>
					{
						foreach (JToken jArrayItem in jArray)
						{
							JObject jObj = jArrayItem as JObject;
							var externalId = jObj.SelectToken("#ref.id").Value<int>();
							var type = jObj.SelectToken("#ref.type").Value<string>();
							DependentEntityLoader.LoadDependenEntity(type, externalId, info.userConnection, null,
								IntegrationLogger.SimpleLoggerErrorAction);
						}
					};
					info.AfterEntitySave = integrateRelatedEntity;
				}
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e);
			}
		}
		public void Export(RuleExportInfo info)
		{
			if (JsonEntityHelper.IsAllNotNullAndEmpty(info.entity, info.config.TsDestinationName, info.config.TsSourcePath, info.config.JSourceName))
			{
				//var srcValue = info.entity.GetTypedColumnValue<Guid>(info.config.TsSourcePath);
				//var jArray = new JArray();
				//var resultList = JsonEntityHelper.GetColumnValues(info.userConnection, info.config.TsDestinationName, info.config.TsDestinationPath, srcValue, info.config.TsDestinationResPath);
				//foreach (var resultItem in resultList)
				//{
				//	var extId = int.Parse(resultItem.ToString());
				//	if (extId != 0)
				//	{
				//		jArray.Add(JToken.FromObject(CsReference.Create(extId, info.config.JSourceName)));
				//	}
				//}
				//info.json = jArray;
			}
			else
			{
				info.json = null;
			}
		}
	}

	#endregion


	#region Class: ComplexFieldMappRule
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Mapping\Rules\Instance\Json\ComplexFieldMappRule.cs
		
	*/
	[RuleAttribute("JoinedColumn")]
	public class ComplexFieldMappRule : IMappRule
	{
		public ComplexFieldMappRule()
		{
		}
		public void Import(RuleImportInfo info)
		{
			object resultId = null;
			if (info.json != null)
			{
				var newJValue = info.json.GetObject() as JValue;
				if (newJValue != null)
				{
					var newValue = newJValue.Value;
					if (newValue != null)
					{
						resultId = JsonEntityHelper.GetColumnValues(info.userConnection, info.config.TsDestinationName, info.config.TsDestinationResPath, newValue, info.config.TsDestinationPath, 1).FirstOrDefault();
						if (info.config.CreateIfNotExist && (resultId == null || (resultId is string && (string)resultId == string.Empty) || (resultId is Guid && (Guid)resultId == Guid.Empty)))
						{
							Dictionary<string, string> defaultColumn = null;
							if (!string.IsNullOrEmpty(info.config.TsTag))
							{
								defaultColumn = JsonEntityHelper.ParsToDictionary(info.config.TsTag, '|', ',');
								foreach (var columnKey in defaultColumn.Keys.ToList())
								{
									string value = defaultColumn[columnKey];
									if (value.StartsWith("$"))
									{
										defaultColumn[columnKey] = GetAdvancedSelectTokenValue(newJValue, value.Substring(1));
									}
								}
							}
							resultId = JsonEntityHelper.CreateColumnValues(info.userConnection, info.config.TsDestinationName, info.config.TsDestinationResPath, newValue, info.config.TsDestinationPath, 1, "CreateOn", Common.OrderDirection.Descending, defaultColumn).FirstOrDefault();
						}
					}
				}
			}
			info.entity.SetColumnValue(info.config.TsSourcePath, resultId);
		}
		public void Export(RuleExportInfo info)
		{
			object resultObject = null;
			if (JsonEntityHelper.IsAllNotNullAndEmpty(info.entity, info.config.TsDestinationName, info.config.TsSourcePath, info.config.TsDestinationPath, info.config.TsDestinationResPath))
			{
				var sourceValue = info.entity.GetColumnValue(info.config.TsSourcePath);
				resultObject = JsonEntityHelper.GetColumnValues(info.userConnection, info.config.TsDestinationName, info.config.TsDestinationPath, sourceValue, info.config.TsDestinationResPath).FirstOrDefault();
			}
			info.json.FromObject(resultObject);
		}
		public string GetAdvancedSelectTokenValue(JToken jToken, string path)
		{
			if (path.StartsWith(".-") && jToken.Parent != null)
			{
				return GetAdvancedSelectTokenValue(jToken.Parent, path.Substring(2));
			}
			if (jToken != null)
			{
				var resultToken = jToken.SelectToken(path);
				if (resultToken != null)
				{
					return resultToken.Value<string>();
				}
			}
			return string.Empty;
		}
	}

	#endregion


	#region Class: CompositMappRule
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Mapping\Rules\Instance\Json\CompositMappRule.cs
		
	*/
	[RuleAttribute("CompositObject")]
	public class CompositMappRule : IMappRule
	{
		public CompositMappRule()
		{
		}
		public void Import(RuleImportInfo info)
		{
			//var integrator = new BaseServiceIntegrator(info.userConnection);
			//integrator.IntegrateServiceEntity(info.json);
		}
		public void Export(RuleExportInfo info)
		{
			var entityId = info.entity.GetTypedColumnValue<Guid>(info.config.TsSourcePath);
			if (entityId != Guid.Empty)
			{
				ConfigSetting config = SettingsManager.GetHandlerConfigById(info.config.HandlerConfigId);
				var entityHelper = new IntegrationEntityHelper();
				var handler = entityHelper.GetAllIntegrationHandler(new List<ConfigSetting>() { config }).FirstOrDefault();
				if (handler != null)
				{
					var esq = new EntitySchemaQuery(info.userConnection.EntitySchemaManager, config.EntityName);
					esq.AddAllSchemaColumns();
					var entity = esq.GetEntity(info.userConnection, entityId);
					if (entity != null)
					{
						var integrationInfo = CsConstant.IntegrationInfo.CreateForExport(info.userConnection, entity);
						var resultJson = handler.ToJson(integrationInfo);
						info.json.SetObject(resultJson.GetObject());
						return;
					}
				}
			}
			info.json.SetObject(null);
		}
	}

	#endregion


	#region Class: ConstMappRule
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Mapping\Rules\Instance\Json\ConstMappRule.cs
		
	*/
	[RuleAttribute("Const")]
	public class ConstMappRule : IMappRule
	{
		public ConstMappRule()
		{ }
		public void Import(RuleImportInfo info)
		{
			//throw new NotImplementedException();
		}
		public void Export(RuleExportInfo info)
		{
			object resultValue = null;
			switch (info.config.ConstType)
			{
				case TConstType.String:
					resultValue = info.config.ConstValue;
					break;
				case TConstType.Bool:
					resultValue = Convert.ToBoolean(info.config.ConstValue.ToString());
					break;
				case TConstType.Int:
					resultValue = int.Parse(info.config.ConstValue.ToString());
					break;
				case TConstType.Null:
					resultValue = null;
					break;
				case TConstType.EmptyArray:
					resultValue = new ArrayList();
					break;
			}
			//info.json = resultValue != null ? JToken.FromObject(resultValue) : null;
		}
	}

	#endregion


	#region Class: ManyToManyMappRule
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Mapping\Rules\Instance\Json\ManyToManyMappRule.cs
		
	*/
	[RuleAttribute("ManyToMany")]
	public class ManyToManyMappRule : IMappRule
	{
		public ManyToManyMappRule()
		{
		}
		public void Import(RuleImportInfo info)
		{
			//if (info.json != null && info.json.HasValues)
			//{
			//	var jArray = info.json as JArray;
			//	foreach (var refItem in jArray)
			//	{
			//		var item = refItem[JsonEntityHelper.RefName];
			//		var externalId = int.Parse(item["id"].ToString());
			//		var type = item["type"];
			//		Tuple<Dictionary<string, string>, Entity> tuple = JsonEntityHelper.GetEntityByExternalId(info.config.TsExternalSource, externalId, info.userConnection, false, info.config.TsExternalPath);
			//		Dictionary<string, string> columnDict = tuple.Item1;
			//		Entity entity = tuple.Item2;
			//		if(entity != null) {
			//			if(!JsonEntityHelper.isEntityExist(info.config.TsDestinationName, info.userConnection, new Dictionary<string,object>() {
			//				{ info.config.TsDestinationPathToSource, info.entity.GetTypedColumnValue<Guid>(info.config.TsSourcePath) },
			//				{ info.config.TsDestinationPathToExternal, entity.GetTypedColumnValue<Guid>(columnDict[info.config.TsExternalPath]) }
			//			})) {
			//				var schema = info.userConnection.EntitySchemaManager.GetInstanceByName(info.config.TsDestinationName);
			//				var destEntity = schema.CreateEntity(info.userConnection);
			//				var firstColumn = schema.Columns.GetByName(info.config.TsDestinationPathToExternal).ColumnValueName;
			//				var secondColumn = schema.Columns.GetByName(info.config.TsDestinationPathToSource).ColumnValueName;
			//				destEntity.SetColumnValue(firstColumn, entity.GetTypedColumnValue<Guid>(columnDict[info.config.TsExternalPath]));
			//				destEntity.SetColumnValue(secondColumn, info.entity.GetTypedColumnValue<Guid>(info.config.TsSourcePath));
			//				destEntity.Save(false);
			//			}
			//		}
			//	}
			//}
		}
		public void Export(RuleExportInfo info)
		{

		}
	}

	#endregion


	#region Class: ReferensToEntityMappRule
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Mapping\Rules\Instance\Json\ReferensToEntityMappRule.cs
		
	*/
	[RuleAttribute("RefToGuid")]
	public class ReferensToEntityMappRule : IMappRule
	{
		public ReferensToEntityMappRule()
		{
		}

		public void Import(RuleImportInfo info)
		{
			//Guid? resultGuid = null;
			//if (info.json != null && info.json.HasValues)
			//{
			//	var refColumns = info.json[JsonEntityHelper.RefName];
			//	var externalId = int.Parse(refColumns["id"].ToString());
			//	var type = refColumns["type"].Value<string>();
			//	Func<Guid?> resultGuidAction = () => JsonEntityHelper.GetColumnValues(info.userConnection, info.config.TsDestinationName, info.config.TsExternalIdPath,
			//			externalId, info.config.TsDestinationPath, -1, "CreatedOn", Terrasoft.Common.OrderDirection.Descending,
			//			JsonEntityHelper.ParsToDictionary(info.config.TsTag, '|', ',')).FirstOrDefault() as Guid?;
			//	if (info.config.LoadDependentEntity)
			//	{
			//		DependentEntityLoader.LoadDependenEntity(type, externalId, info.userConnection, () =>
			//		{
			//			resultGuid = resultGuidAction();
			//		}, IntegrationLogger.SimpleLoggerErrorAction);
			//	}
			//	else
			//	{
			//		resultGuid = resultGuidAction();
			//	}
			//}
			//if (!info.config.IsAllowEmptyResult && (resultGuid == null || resultGuid.Value == Guid.Empty))
			//{
			//	return;
			//}
			//info.entity.SetColumnValue(info.config.TsSourcePath, resultGuid);
		}

		public void Export(RuleExportInfo info)
		{
			object resultObj = null;
			if (JsonEntityHelper.IsAllNotNullAndEmpty(info.entity, info.config.TsDestinationName, info.config.TsSourcePath,
				info.config.JSourceName, info.config.TsDestinationPath))
			{
				var resultValue =
					JsonEntityHelper.GetColumnValues(info.userConnection, info.config.TsDestinationName, info.config.TsDestinationPath,
							info.entity.GetTypedColumnValue<Guid>(info.config.TsSourcePath), info.config.TsExternalIdPath)
						.FirstOrDefault(x => (int)x > 0);
				if (resultValue != null)
				{
					var resultRef = CsReference.Create(int.Parse(resultValue.ToString()), info.config.JSourceName);
					resultObj = resultRef != null ? JToken.FromObject(resultRef) : null;
				}
			}
			//info.json = resultObj as JToken;
		}
	}

	#endregion


	#region Class: SimpleMappRule
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Mapping\Rules\Instance\Json\SimpleMappRule.cs
		
	*/
	[RuleAttribute("Simple")]
	public class SimpleMappRule : IMappRule
	{
		public void Import(RuleImportInfo info)
		{
			object value = info.json.GetProperty<object>(null);
			if (value is JValue)
			{
				value = ((JValue)value).Value;
			}
			if (!string.IsNullOrEmpty(info.config.MacrosName))
			{
				value = MacrosFactory.GetMacrosResultImport(info.config.MacrosName, value);
			}
			if (value is ValueType || value is string)
			{
				info.entity.SetColumnValue(info.config.TsSourcePath, value);
			}
		}
		public void Export(RuleExportInfo info)
		{
			var value = info.entity.GetColumnValue(info.config.TsSourcePath);
			var simpleResult = value != null ? JsonEntityHelper.GetSimpleTypeValue(value) : null;
			if (!string.IsNullOrEmpty(info.config.MacrosName))
			{
				simpleResult = MacrosFactory.GetMacrosResultExport(info.config.MacrosName, simpleResult);
				if (simpleResult is DateTime)
				{
					simpleResult = ((DateTime)simpleResult).ToString("yyyy-MM-dd");
				}
			}

			info.json.FromObject(simpleResult);
		}
	}

	#endregion


	#region Class: ToDetailMappRule
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Mapping\Rules\Instance\Json\ToDetailMappRule.cs
		
	*/
	[RuleAttribute("ToDetail")]
	class ToDetailMappRule : IMappRule
	{
		public ToDetailMappRule()
		{
		}
		public void Import(RuleImportInfo info)
		{
			object resultId = null;
			if (info.json != null)
			{
				object resultValue = null;
				var newValue = JsonEntityHelper.GetSimpleTypeValue(info.json);
				if (newValue != null && !string.IsNullOrEmpty(newValue.ToString()))
				{
					resultId = info.entity.GetColumnValue(info.config.TsSourcePath);
					var optionalColumns = new Dictionary<string, string>();
					if (!string.IsNullOrEmpty(info.config.TsDetailTag))
					{
						optionalColumns = JsonEntityHelper.ParsToDictionary(info.config.TsDetailTag, '|', ',');
					}
					optionalColumns.Add(info.config.TsDetailPath, resultId.ToString());
					if (info.config.TsTag == "simple")
					{
						resultValue = newValue.ToString();
					}
					else if (info.config.TsTag == "stringtoguid")
					{
						resultValue = JsonEntityHelper.GetColumnValues(info.userConnection, info.config.TsDestinationName, info.config.TsDestinationResPath, newValue, info.config.TsDestinationPath, 1).FirstOrDefault();
					}
					var filters = new List<Tuple<string, object>>() {
						new Tuple<string, object>(info.config.TsDetailPath, resultId)
					};
					JsonEntityHelper.UpdateOrInsertEntityColumn(info.config.TsDetailName, info.config.TsDetailResPath, resultValue, info.userConnection, optionalColumns, filters);
				}
			}
		}
		public void Export(RuleExportInfo info)
		{
			//object resultObject = null;
			//var sourceValue = info.entity.GetColumnValue(info.config.TsSourcePath);
			//var optionalColumns = JsonEntityHelper.ParsToDictionary(info.config.TsDetailTag, '|', ',');
			//var detailValue = JsonEntityHelper.GetColumnValuesWithFilters(info.userConnection, info.config.TsDetailName, info.config.TsDetailPath, sourceValue, info.config.TsDetailResPath, optionalColumns).FirstOrDefault();
			//if (info.config.TsTag == "simple")
			//{
			//	resultObject = detailValue;
			//}
			//else if (info.config.TsTag == "stringtoguid")
			//{
			//	resultObject = JsonEntityHelper.GetColumnValues(info.userConnection, info.config.TsDestinationName, info.config.TsDestinationPath, detailValue, info.config.TsDestinationResPath).FirstOrDefault();
			//}
			//info.json = resultObject != null ? JToken.FromObject(resultObject) : null;
		}

		public IEnumerable<Tuple<string, string>> ParseDetailTag(string tag)
		{
			if (string.IsNullOrEmpty(tag))
			{
				return new List<Tuple<string, string>>();
			}
			return tag.Split(',').Select(x =>
			{
				var block = x.Split('|');
				return new Tuple<string, string>(block[0], block[1]);
			});
		}
	}

	#endregion


	#region Class: ComplexFieldXmlMappRule
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Mapping\Rules\Instance\Xml\ComplexFieldMappRule.cs
		
	*/
	[RuleAttribute("SimpeJoinedColumn")]
	[RuleAttribute("SimpeJoinedColumn", TIntegrationObjectType.Xml)]
	public class ComplexFieldXmlMappRule : IMappRule
	{
		public ComplexFieldXmlMappRule()
		{
		}
		public void Import(RuleImportInfo info)
		{
			object resultId = null;
			if (info.json != null)
			{
				var newValue = info.json.GetProperty<string>(null);
				if (newValue != null)
				{
					resultId = JsonEntityHelper.GetColumnValues(info.userConnection, info.config.TsDestinationName, info.config.TsDestinationResPath, newValue, info.config.TsDestinationPath, 1).FirstOrDefault();
				}
			}
			info.entity.SetColumnValue(info.config.TsSourcePath, resultId);
		}
		public void Export(RuleExportInfo info)
		{
			object resultObject = null;
			if (JsonEntityHelper.IsAllNotNullAndEmpty(info.entity, info.config.TsDestinationName, info.config.TsSourcePath, info.config.TsDestinationPath, info.config.TsDestinationResPath))
			{
				var sourceValue = info.entity.GetColumnValue(info.config.TsSourcePath);
				resultObject = JsonEntityHelper.GetColumnValues(info.userConnection, info.config.TsDestinationName, info.config.TsDestinationPath, sourceValue, info.config.TsDestinationResPath).FirstOrDefault();
			}
			info.json.FromObject(resultObject);
		}
	}

	#endregion


	#region Class: CompositXmlMappRule
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Mapping\Rules\Instance\Xml\CompositXmlMappRule.cs
		
	*/
	[RuleAttribute("CompositObject", TIntegrationObjectType.Xml)]
	public class CompositXmlMappRule : IMappRule
	{
		public CompositXmlMappRule()
		{
		}
		public void Import(RuleImportInfo info)
		{
			var integrator = ClassFactory.Get<BaseIntegrator>();
			info.json.SetProperty("TsiIntegrateParentId", info.entity.PrimaryColumnValue);
			integrator.Import(info.userConnection, info.json, info.config.ImportRouteId);
		}
		public void Export(RuleExportInfo info)
		{
			throw new NotImplementedException();
		}
	}

	#endregion


	#region Class: ConstRule
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Mapping\Rules\Instance\Xml\ConstRule.cs
		
	*/
	[RuleAttribute("Const", TIntegrationObjectType.Xml)]
	public class ConstRule : IMappRule
	{
		public void Export(RuleExportInfo info)
		{
			object value = info.config.Value;
			if (!string.IsNullOrEmpty(info.config.MacrosName))
			{
				value = MacrosFactory.GetMacrosResultExport(info.config.MacrosName, (string)value);
			}
			info.json.FromObject(value);
		}

		public void Import(RuleImportInfo info)
		{
			throw new NotImplementedException();
		}
	}

	#endregion


	#region Class: JoinedColumnXmlMappRule
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Mapping\Rules\Instance\Xml\JoinedColumnXmlMappRule.cs
		
	*/
	[RuleAttribute("JoinedColumn", TIntegrationObjectType.Xml)]
	public class JoinedColumnXmlMappRule : IMappRule
	{
		public void Import(RuleImportInfo info)
		{
			object resultId = null;
			if (info.json != null)
			{
				var newJValue = info.json.GetObject() as XElement;
				if (newJValue != null)
				{
					var newValue = newJValue.Value;
					if (newValue != null)
					{
						resultId = JsonEntityHelper.GetColumnValues(info.userConnection, info.config.TsDestinationName, info.config.TsDestinationResPath, newValue, info.config.TsDestinationPath, 1).FirstOrDefault();
					}
				}
			}
			info.entity.SetColumnValue(info.config.TsSourcePath, resultId);
		}
		public void Export(RuleExportInfo info)
		{
			object resultObject = null;
			if (JsonEntityHelper.IsAllNotNullAndEmpty(info.entity, info.config.TsDestinationName, info.config.TsSourcePath, info.config.TsDestinationPath, info.config.TsDestinationResPath))
			{
				var sourceValue = info.entity.GetColumnValue(info.config.TsSourcePath);
				resultObject = JsonEntityHelper.GetColumnValues(info.userConnection, info.config.TsDestinationName, info.config.TsDestinationPath, sourceValue, info.config.TsDestinationResPath).FirstOrDefault();
			}
			info.json.FromObject(resultObject);
		}
	}

	#endregion


	#region Class: SimpleByPathRule
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Mapping\Rules\Instance\Xml\SimpleByPathRule.cs
		
	*/
	[RuleAttribute("SimpleByPath", TIntegrationObjectType.Xml)]
	public class SimpleByPathRule : IMappRule
	{
		//Log Name=Simple By Path Mapp, Key=MappingRule
		public void Export(RuleExportInfo info)
		{
			var esq = new EntitySchemaQuery(info.userConnection.EntitySchemaManager, info.entity.SchemaName);
			esq.RowCount = 1;
			var column = esq.AddColumn(info.config.TsSourcePath);
			Entity entity = null;
			bool loadById = true;
			if (!string.IsNullOrEmpty(info.config.OrderColumn))
			{
				var orderColumn = esq.AddColumn(info.config.OrderColumn);
				orderColumn.OrderPosition = 0;
				orderColumn.OrderDirection = info.config.OrderType;
				loadById = false;
			}
			if (!string.IsNullOrEmpty(info.config.TsTag))
			{
				Dictionary<string, string> filterColumns = JsonEntityHelper.ParsToDictionary(info.config.TsTag, '|', ',');
				foreach (var filterColumn in filterColumns)
				{
					object filterValue = filterColumn.Value;
					string key = filterColumn.Key;
					if (filterColumn.Value.StartsWith("$"))
					{
						filterValue = info.entity.GetColumnValue(filterColumn.Value.Substring(1));
					}
					esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, filterColumn.Key, filterValue));
				}
				loadById = false;
			}
			if (loadById)
			{
				entity = esq.GetEntity(info.userConnection, info.entity.PrimaryColumnValue);
			}
			else
			{
				entity = esq.GetEntityCollection(info.userConnection).FirstOrDefault();
			}
			if (entity != null)
			{
				var nameColumn = entity.Schema.Columns.GetByName(column.Name);
				var value = entity.GetColumnValue(nameColumn.Name);
				var simpleResult = value != null ? JsonEntityHelper.GetSimpleTypeValue(value) : null;
				if (!string.IsNullOrEmpty(info.config.MacrosName))
				{
					simpleResult = MacrosFactory.GetMacrosResultExport(info.config.MacrosName, simpleResult);
					if (simpleResult is DateTime)
					{
						simpleResult = ((DateTime)simpleResult).ToString("dd-MM-yyyy");
					}
				}

				info.json.FromObject(simpleResult);
			}
		}

		public void Import(RuleImportInfo info)
		{
			throw new NotImplementedException();
		}
	}

	#endregion


	#region Class: XmlArrayMappRule
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Mapping\Rules\Instance\Xml\XmlArrayMappRule.cs
		
	*/
	[RuleAttribute("Array", TIntegrationObjectType.Xml)]
	public class XmlArrayMappRule : IMappRule
	{
		public void Import(RuleImportInfo info)
		{
			object value = info.json.GetProperty<object>(null);
			if (value is XElement)
			{
				value = ((XElement)value).Value;
			}
			if (!string.IsNullOrEmpty(info.config.MacrosName))
			{
				value = MacrosFactory.GetMacrosResultImport(info.config.MacrosName, value);
			}
			if (value is ValueType || value is string)
			{
				info.entity.SetColumnValue(info.config.TsSourcePath, value);
			}
		}
		public void Export(RuleExportInfo info)
		{
		}


	}

	#endregion


	#region Class: XmlSimpleMappRule
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Mapping\Rules\Instance\Xml\XmlSimpleMappRule.cs
		
	*/
	[RuleAttribute("simple", TIntegrationObjectType.Xml)]
	public class XmlSimpleMappRule : IMappRule
	{
		public void Import(RuleImportInfo info)
		{
			object value = info.json.GetProperty<object>(null);
			if (value is XElement)
			{
				value = ((XElement)value).Value;
			}
			if (!string.IsNullOrEmpty(info.config.MacrosName))
			{
				value = MacrosFactory.GetMacrosResultImport(info.config.MacrosName, value);
			}
			if (value is ValueType || value is string)
			{
				info.entity.SetColumnValue(info.config.TsSourcePath, value);
			}
		}
		public void Export(RuleExportInfo info)
		{
			var value = info.entity.GetColumnValue(info.config.TsSourcePath);
			var simpleResult = value != null ? JsonEntityHelper.GetSimpleTypeValue(value) : null;
			if (!string.IsNullOrEmpty(info.config.MacrosName))
			{
				simpleResult = MacrosFactory.GetMacrosResultExport(info.config.MacrosName, simpleResult);
				if (simpleResult is DateTime)
				{
					simpleResult = ((DateTime)simpleResult).ToString("yyyy-MM-dd");
				}
			}
			info.json.FromObject(simpleResult);
		}


	}

	#endregion


	#region Class: MappRuleOptimizatorAttribute
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Mapping\Rules\Optimizator\Attribute\MappRuleOptimizatorAttribute.cs
		
	*/
	[AttributeUsage(AttributeTargets.Class)]
	public class MappRuleOptimizatorAttribute: System.Attribute
	{
		public Type RuleType;
		public MappRuleOptimizatorAttribute(Type ruleType)
		{
			RuleType = ruleType;
		}
	}

	#endregion


	#region Class: MappRuleOptimizatorFactory
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Mapping\Rules\Optimizator\Factory\MappRuleOptimizatorFactory.cs
		
	*/
	public class MappRuleOptimizatorFactory: BaseAttributeFactory<IMappRuleOptimizator, MappRuleOptimizatorAttribute, Type>
	{
		private static Dictionary<MappRuleOptimizatorAttribute, IMappRuleOptimizator> _instances;

		protected override Dictionary<MappRuleOptimizatorAttribute, IMappRuleOptimizator> Instances
		{
			get
			{
				if (_instances == null)
				{
					_instances = new Dictionary<MappRuleOptimizatorAttribute, IMappRuleOptimizator>();
				}
				return _instances;
			}
		}

		private static bool _isRuleRegister;

		protected override bool IsRuleRegister
		{
			get { return _isRuleRegister; }
			set { _isRuleRegister = value; }
		}

		protected override bool IsInstanceNameEqual(KeyValuePair<MappRuleOptimizatorAttribute, IMappRuleOptimizator> instanceKeyValue, Type name)
		{
			return instanceKeyValue.Key.RuleType == name;
		}
	}

	#endregion


	#region Class: MappRuleOptimizator
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Mapping\Rules\Optimizator\Model\MappRuleOptimizator.cs
		
	*/
	[MappRuleOptimizator(typeof(SimpleMappRule))]
	public class MappRuleOptimizator : IMappRuleOptimizator
	{
		public void Optimize(CsConstant.IntegrationInfo integrationInfo)
		{
			//throw new NotImplementedException();
		}
	}

	#endregion


	#region Class: RuleExportInfo
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Mapping\Rules\Parameter\RuleExportInfo.cs
		
	*/
	public class RuleExportInfo : RuleInfo
	{
	}

	#endregion


	#region Class: RuleImportInfo
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Mapping\Rules\Parameter\RuleImportInfo.cs
		
	*/
	public class RuleImportInfo : RuleInfo
	{
		/// <summary>
		/// Выполняется после сохранения объекта
		/// </summary>
		public Action AfterEntitySave;
	}

	#endregion


	#region Class: RuleInfo
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Mapping\Rules\Parameter\RuleInfo.cs
		
	*/
	public class RuleInfo
	{
		public MappingItem config;
		public Entity entity;
		public IIntegrationObject json;
		public UserConnection userConnection;
		public CsConstant.TIntegrationType integrationType;
		public string action;
	}

	#endregion


	#region Class: CsReference
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Mapping\Rules\_Objects\Ref\CsReference.cs
		
	*/
	public class CsReference
	{
		public CsReferenceProperty ReferenceClientService;
		/// <summary>
		/// Cоздает объект, который обозначает ссылку (#ref) в Clientservice
		/// </summary>
		/// <param name="pid">id</param>
		/// <param name="ptype">type</param>
		/// <param name="pname">name</param>
		/// <returns></returns>
		public static CsReference Create(int pid, string ptype, string pname = "")
		{
			return pid != 0 ? new CsReference
			{
				ReferenceClientService = new CsReferenceProperty
				{
					id = pid,
					type = ptype,
					name = pname
				}
			} : (CsReference)null;
		}
	}

	#endregion


	#region Class: CsReferenceProperty
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Mapping\Rules\_Objects\Ref\CsReferenceProperty.cs
		
	*/
	public class CsReferenceProperty
	{
		public int id;
		public string type;
		public string name;
	}

	#endregion


	#region Class: ArhiveSessionIntegrationHandler
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Project\ArhiveSessionIntegrationHandler.cs
		
	*/
	[IntegrationHandlerAttribute("ArhiveSessionIntegrationHandler")]
	public class ArhiveSessionIntegrationHandler: DefaultEntityHandler
	{
		protected virtual string IdsPath {
			get {
				return "GetCArchiveSessionOUT/item/id";
			}
		}

		protected virtual string DeleteEntityName
		{
			get { return "TsiArchiveSession"; }
		}

		protected virtual string EntityDeleteIdPath
		{
			get { return "TsiIdentifier"; }
		}

		protected virtual string DeleteLogCaption
		{
			get { return "Delete Archive Session"; }
		}

		protected virtual string DeleteEntityPrimaryColumnName {
			get { return "TsiPersAccProductId"; }
		}
		protected virtual string MainEntityPrimaryColumnName {
			get { return "Id"; }
		}
		public ArhiveSessionIntegrationHandler(ConfigSetting handlerConfig) : base(handlerConfig)
		{
		}

		protected override void BeforeMapping(CsConstant.IntegrationInfo integrationInfo)
		{
			base.BeforeMapping(integrationInfo);
			if (CheckIntegrationInfoForProcessResponse(integrationInfo))
			{
				DeleteOldSession(integrationInfo);
			}
		}

		private void DeleteOldSession(CsConstant.IntegrationInfo integrationInfo)
		{
			List<string> sessionIds = GetSessionIds(integrationInfo.Data);
			LoggerHelper.DoInLogBlock(DeleteLogCaption, () =>
			{
				DeleteArchiveSessionByNotThisIds(sessionIds, integrationInfo.UserConnection);
			});
		}

		private void DeleteArchiveSessionByNotThisIds(List<string> sessionIds, UserConnection userConnection)
		{
			var delete = new Delete(userConnection)
				.From(DeleteEntityName)
				.Where(DeleteEntityPrimaryColumnName).IsEqual(Column.Parameter(MainEntityPrimaryColumnName)) as Delete;
			if (sessionIds.Any())
			{
				delete.And(EntityDeleteIdPath).Not().In(sessionIds.Select(x => Column.Parameter(x)).ToArray());
			}
			delete.Execute();
		}

		private List<string> GetSessionIds(IIntegrationObject data)
		{
			var xElement = data.GetObject() as XElement;
			if (xElement != null)
			{
				return xElement.XPathSelectElements(IdsPath).Select(x => x.Value).ToList();
			}
			return new List<string>();
		}
	}

	#endregion


	#region Class: DeleteBeforeImportHandler
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Project\DeleteBeforeImportHandler.cs
		
	*/
	[IntegrationHandlerAttribute("DeleteBeforeImportHandler")]
	public class DeleteBeforeImportHandler: ArhiveSessionIntegrationHandler
	{
		public DeleteBeforeImportHandler(ConfigSetting handlerConfig) : base(handlerConfig)
		{
		}
		protected override string IdsPath
		{
			get { return GetHandlerConfigValue("Delete_IObjectIdPath", base.IdsPath); }
		}
		protected override string DeleteEntityName
		{
			get { return GetHandlerConfigValue("Delete_EntityName", base.DeleteEntityName); }
		}
		protected override string EntityDeleteIdPath {
			get { return GetHandlerConfigValue("Delete_EntityIdName", base.EntityDeleteIdPath); }
		}
		protected override string DeleteLogCaption
		{
			get { return GetHandlerConfigValue("Delete_LogCaption", base.DeleteLogCaption); }
		}
		protected override string DeleteEntityPrimaryColumnName {
			get { return GetHandlerConfigValue("Delete_EntityPrimaryColumnName", base.DeleteEntityPrimaryColumnName); }
		}
		protected override string MainEntityPrimaryColumnName {
			get { return GetHandlerConfigValue("Delete_MainEntityPrimaryColumnName", base.MainEntityPrimaryColumnName); }
		}
	}

	#endregion


	#region Class: TemplateHandlerFactory
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\ServiceConstructor\Model\TemplateHandlerFactory.cs
		
	*/
	public class TemplateHandlerFactory : ITemplateFactory
	{
		public ConcurrentDictionary<string, Type> Handlers;
		public TemplateHandlerFactory()
		{
			Register();
		}
		public void Register()
		{
			var handlerAttrType = typeof(TemplateAttribute);
			var handlerDictionary = typeof(TemplateHandlerFactory)
				.Assembly
				.GetTypes()
				.Where(x => x.GetCustomAttributes(handlerAttrType, true).Any())
				.Select(x => new
				{
					key = (x.GetCustomAttributes(handlerAttrType, true).First() as TemplateAttribute).Name,
					value = x
				})
				.Where(x => x.value != null)
				.ToDictionary(x => x.key, x => x.value);
			Handlers = new ConcurrentDictionary<string, Type>(handlerDictionary);
		}
		public ITemplateHandler Get(string name)
		{
			if (Handlers != null && Handlers.ContainsKey(name))
			{
				return Activator.CreateInstance(Handlers[name]) as ITemplateHandler;
			}
			return null;
		}
	}

	#endregion


	#region Class: XmlBaseTemplateHandler
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\ServiceConstructor\Model\XmlBaseTemplateHandler.cs
		
	*/
	[TemplateAttribute("XmlTemplateHandler")]
	public class XmlBaseTemplateHandler : ITemplateHandler
	{
		//Log key=XmlHandler, throw
		public virtual void Import(TemplateSetting templateSettings, IntegrationInfo integrationInfo)
		{
			string sysPrefixName = templateSettings.Settings["sysPrefixName"];
			var element = integrationInfo.Data.GetObject() as XElement;
			if (element != null)
			{
				element = RemoveNamespaces(element);
				var bodyElement = element.Descendants().Where(x => x.Name.LocalName == "Body").FirstOrDefault();
				integrationInfo.Data = new IntegrXObject((XElement)bodyElement.FirstNode);
			}
		}
		//Log key=XmlHandler, throw
		public virtual void Export(TemplateSetting templateSettings, IntegrationInfo integrationInfo)
		{
			XNamespace soapPrefix = templateSettings.Settings["soapPrefix"];
			string sysPrefixName = templateSettings.Settings["sysPrefixName"];
			XNamespace sysPrefixValue = templateSettings.Settings["sysPrefixValue"];
			var headerNodes = GetHeaderNodes(templateSettings);
			var element = integrationInfo.Data.GetObject() as XElement;
			integrationInfo.Data = WrapElement(element, soapPrefix, sysPrefixName, sysPrefixValue, headerNodes);
		}
		//Log key=XmlHandler, throw
		protected virtual IIntegrationObject WrapElement(XElement element, XNamespace soapPrefix, string sysPrefixName, XNamespace sysPrefixValue, List<XElement> headers)
		{
			var wrappedElement = new XElement(soapPrefix + "Envelope",
				new XAttribute(XNamespace.Xmlns + "soapenv", soapPrefix),
				new XAttribute(XNamespace.Xmlns + sysPrefixName, sysPrefixValue),
				new XElement(soapPrefix + "Header",
					AddNamespaceToXElement(new XElement("AuthenticationInfo", headers), sysPrefixValue)),
				new XElement(soapPrefix + "Body", AddNamespaceToXElement(element, sysPrefixValue)));
			return new IntegrXObject(wrappedElement);
		}
		//Log key=XmlHandler, throw
		public List<XElement> GetHeaderNodes(TemplateSetting templateSettings)
		{
			if (templateSettings.Settings.ContainsKey("Headers"))
			{
				return templateSettings.Settings["Headers"]
					.Split(',')
					.Where(x => !string.IsNullOrEmpty(x) && templateSettings.Settings.ContainsKey(x))
					.Select(x => new XElement(x, templateSettings.Settings[x]))
					.ToList();
			}
			return new List<XElement>();
		}
		public XElement AddNamespaceToXElement(XElement xElement, XNamespace xNamespace)
		{
			foreach (XElement el in xElement.DescendantsAndSelf())
			{
				el.Name = xNamespace.GetName(el.Name.LocalName);
			}
			return xElement;
		}
		public XElement RemoveNamespaces(XElement root)
		{
			XElement res = new XElement(
				root.Name.LocalName,
				root.HasElements ?
					root.Elements().Select(el => RemoveNamespaces(el)) :
					(object)root.Value
			);
			res.ReplaceAttributes(
				root.Attributes().Where(attr => (!attr.IsNamespaceDeclaration)));
			return res;
		}
	}

	#endregion


	#region Class: TemplateAttribute
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\ServiceConstructor\Model\Attribute\TemplateAttribute.cs
		
	*/
	public class TemplateAttribute : System.Attribute
	{
		private string _name;
		public string Name {
			get { return _name; }
		}
		public TemplateAttribute(string _name)
		{
			this._name = _name;
		}
	}

	#endregion


	#region Class: MappingConfigTriggerCollection
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Trigger\MappingConfigTriggerCollection.cs
		
	*/
	public class MappingConfigTriggerCollection : ITriggerCollection<Entity>
	{
		private TriggerCheckerCollection _instance;
		public MappingConfigTriggerCollection(UserConnection userConnection, TriggerCheckerCollection obj)
		{
			_instance = obj;
			LoadAllChecker(userConnection);
		}
		public bool Check(string eventName, Entity eventInfo, Action<TriggerSetting> onMath)
		{
			return _instance.Check(eventName, eventInfo, onMath);
		}

		public TriggerSetting GetSetting()
		{
			return _instance.GetSetting();
		}

		public void LoadAllChecker(UserConnection userConnection)
		{
			try
			{
				SettingsManager.UserConnection = userConnection;
				SettingsManager.InitIntegrationSettings();
				var triggersConfig = SettingsManager.GetTriggersConfig();
				_instance.AddRange(triggersConfig.Select(x => new TriggerCheckerItem(x)));
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e);
			}
		}

		public TriggerSetting GetSettingByName(string name)
		{
			return _instance.GetSettingByName(name);
		}
	}

	#endregion


	#region Class: TriggerCheckerCollection
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Trigger\TriggerCheckerCollection.cs
		
	*/
	public class TriggerCheckerCollection : ITriggerCollection<Entity>
	{
		List<ITriggerCheckerItem<Entity>> _items;
		public TriggerCheckerCollection()
		{
			_items = new List<ITriggerCheckerItem<Entity>>();
		}
		public bool Check(string eventName, Entity eventInfo, Action<TriggerSetting> onMath)
		{
			bool isCheked = false;
			_items.ForEach(x => {
				if (x.Check(eventName, eventInfo, onMath))
				{
					isCheked = true;
				}
			});
			return isCheked;
		}

		public TriggerSetting GetSetting()
		{
			return null;
		}

		public void Add(ITriggerCheckerItem<Entity> entityChecker)
		{
			_items.Add(entityChecker);
		}
		public void AddRange(IEnumerable<ITriggerCheckerItem<Entity>> entityCheckers)
		{
			_items.AddRange(entityCheckers);
		}

		public virtual TriggerSetting GetSettingByName(string name)
		{
			var trigger = _items.FirstOrDefault(x => x.GetSetting().Name == name);
			if (trigger != null)
			{
				return trigger.GetSetting();
			}
			return null;
		}
	}

	#endregion


	#region Class: TriggerCheckerItem
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Trigger\TriggerCheckerItem.cs
		
	*/
	public class TriggerCheckerItem : ITriggerCheckerItem<Entity>
	{
		private TriggerSetting _setting;
		private EntitySchemaQueryFilterCollection _filter;

		public TriggerCheckerItem(TriggerSetting setting)
		{
			_setting = setting;
		}

		public bool Check(string eventName, Entity eventInfo, Action<TriggerSetting> onMath)
		{
			if (CheckEventName(eventName) && eventInfo.SchemaName == _setting.EntitySchema)
			{
				var userConnection = eventInfo.UserConnection;
				var esq = new EntitySchemaQuery(eventInfo.Schema);
				esq.UseAdminRights = false;
				var filters = GetEsqFilterCollection(esq, userConnection);
				if (filters != null && filters.Count > 0)
				{
					esq.Filters.Add(filters);
					esq.AddColumn(eventInfo.Schema.PrimaryColumn.Name);
					var resultEntity = esq.GetEntity(userConnection, eventInfo.PrimaryColumnValue);
					if (resultEntity != null)
					{
						onMath(_setting);
						return true;
					}
				}
				else
				{
					onMath(_setting);
				}
			}
			return false;
		}

		public TriggerSetting GetSetting()
		{
			return _setting;
		}

		public EntitySchemaQueryFilterCollection GetEsqFilterCollection(EntitySchemaQuery esq, UserConnection userConnection)
		{
			if (_filter == null)
			{
				var processDataContractFilterConverter = ClassFactory.Get<ProcessDataContractFilterConverter>(new ConstructorArgument("userConnection", userConnection));
				_filter = processDataContractFilterConverter.ConvertToEntitySchemaQueryFilterItem(esq, _setting.Filter) as EntitySchemaQueryFilterCollection;
			}
			return _filter;
		}
		protected virtual bool CheckEventName(string eventName)
		{
			return (_setting.OnUpdate && eventName == "update") || (_setting.OnInsert && eventName == "insert");
		}
	}

	#endregion


	#region Class: TriggerEngine
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Trigger\TriggerEngine.cs
		
	*/
	public class TriggerEngine : ITriggerEngine<Entity>
	{
		public static ITriggerCollection<Entity> _triggerCollection;
		public static object _lock = new object();
		public static object _refreshLock = new object();
		//Log name=TriggerEngine, key=Trigger, throw
		public void Push(string eventName, Entity eventInfo)
		{
			var triggerCollection = GetTriggerCollection(eventInfo.UserConnection);
			triggerCollection.Check(eventName, eventInfo, triggerInfo =>
			{
				LoggerHelper.DoInLogBlock("Find Trigger " + triggerInfo.Caption, () =>
				{
					var integrator = ClassFactory.Get<BaseIntegrator>();
					integrator.ExportWithRequest(eventInfo.UserConnection, eventInfo.PrimaryColumnValue, eventInfo.SchemaName, triggerInfo.Route);
				});
			});
		}

		public TriggerSetting GetTriggerByName(string name, UserConnection userConnection)
		{
			return GetTriggerCollection(userConnection).GetSettingByName(name);
		}

		public ITriggerCollection<Entity> GetTriggerCollection(UserConnection userConnection)
		{
			if (_triggerCollection != null)
			{
				return _triggerCollection;
			}
			lock (_lock)
			{
				if (_triggerCollection != null)
				{
					return _triggerCollection;
				}
				_triggerCollection = new MappingConfigTriggerCollection(userConnection, new TriggerCheckerCollection());
				return _triggerCollection;
			}
		}

		public static void ClearTriggerCollection()
		{
			_triggerCollection = null;
		}
	}

	#endregion


	#region Class: IntegratorHelper
	/*
		Project Path: ..\..\..\QueryConsole\Files\Remove\IntegratorHelper.cs
		
	*/
	public class IntegratorHelper
	{

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="requestMethod">Get, Put, Post</param>
		/// <param name="url"></param>
		/// <param name="jsonText">Данные для отправки в формате json</param>
		/// <param name="callback">callback - для обработки ответа</param>
		/// <param name="userConnection"></param>
		public void PushRequest(TRequstMethod requestMethod, string url, string jsonText, Action<string, UserConnection> callback, UserConnection userConnection,
			 Action<string, UserConnection> errorCallback = null, string auth = null)
		{
			if (string.IsNullOrEmpty(url))
			{
				return;
			}

			LoggerHelper.DoInLogBlock("PushRequest", () =>
			{
				IntegrationLogger.Info(RequestLoggerInfo.GetMessage(requestMethod, url, auth, jsonText));
				MakeAsyncRequest(requestMethod, url, jsonText, callback, userConnection, errorCallback, auth);
			});
		}


		/// <summary>
		/// Делает асинхронный запрос
		/// </summary>
		/// <param name="requestMethod"></param>
		/// <param name="url"></param>
		/// <param name="jsonText"></param>
		/// <param name="callback"></param>
		/// <param name="userConnection"></param>
		private static void MakeAsyncRequest(TRequstMethod requestMethod, string url, string jsonText, Action<string, UserConnection> callback,
			 UserConnection userConnection = null,
			 Action<string, UserConnection> errorCallback = null, string auth = null)
		{
			try
			{
				var _request = WebRequest.Create(new Uri(url)) as HttpWebRequest;
				_request.Method = requestMethod.ToString();
				_request.ContentType = "application/json";
				_request.Headers.Add("authorization", string.IsNullOrEmpty(auth) ? "Basic YnBtb25saW5lOmJwbW9ubGluZQ==" : auth);
				_request.Headers.Add("cache-control", "no-cache");
				switch (requestMethod)
				{
					case TRequstMethod.POST:
					case TRequstMethod.PUT:
						if (string.IsNullOrEmpty(jsonText))
							return;
						jsonText = jsonText.Replace("ReferenceClientService", "#ref");
						AddDataToRequest(_request, jsonText);
						break;
				}
				try
				{
					var response = _request.GetResponse();
					using (Stream responseStream = response.GetResponseStream())
					using (StreamReader sr = new StreamReader(responseStream))
					{
						if (callback != null)
						{
							string responceText = sr.ReadToEnd();
							IntegrationLogger.Info(ResponseLoggerInfo.GetMessage(responceText));
							callback(responceText, userConnection);
						}
					}
				}
				catch (WebException e)
				{
					WebResponse response = e.Response;
					using (StreamReader sr = new StreamReader(response.GetResponseStream()))
					{
						string responceText = sr.ReadToEnd();
						IntegrationLogger.Error(RequestErrorLoggerInfo.GetMessage(e, responceText));
						if (errorCallback != null)
						{
							errorCallback(responceText, userConnection);
						}
					}
				}
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(RequestErrorLoggerInfo.GetMessage(e, "Ошибка при формировании запроса"));
				if (errorCallback != null)
				{
					errorCallback(e.Message, userConnection);
				}
			}
		}
		/// <summary>
		/// Добавляет данные к запросу
		/// </summary>
		/// <param name="request"></param>
		/// <param name="data"></param>
		private static void AddDataToRequest(HttpWebRequest request, string data)
		{
			if (string.IsNullOrEmpty(data))
				return;
			var encoding = new UTF8Encoding();
			data = data.Replace("ReferenceClientService", "#ref");
			var bytes = Encoding.UTF8.GetBytes(data);
			request.ContentLength = bytes.Length;

			using (var writeStream = request.GetRequestStream())
			{
				writeStream.Write(bytes, 0, bytes.Length);
			}
		}

		private class ResponceParams
		{
			public ResponceParams(HttpWebRequest request, Action<string, UserConnection> callback, UserConnection userConnection, string jsonData)
			{
				Request = request;
				Callback = callback;
				UserConnection = userConnection;
				JsonData = jsonData;
			}
			public HttpWebRequest Request;
			public Action<string, UserConnection> Callback;
			public UserConnection UserConnection;
			public string JsonData;
		}

	}

	#endregion


	#region Class: JsonEntityHelper
	/*
		Project Path: ..\..\..\QueryConsole\Files\Remove\JsonEntityHelper.cs
		
	*/
	public static class JsonEntityHelper
	{
		public static object GetSimpleTypeValue(object value)
		{
			try
			{
				if (value is DateTime)
				{
					return ((DateTime)value).ToString("yyyy-MM-dd'T'HH:mm:ss");
				}
				if (value is bool)
				{
					return (bool)value == true ? true : false;
				}
				return value;
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e);
				throw;
			}
		}

		public static List<object> GetColumnValues(UserConnection userConnection, string entityName, string entityPath, object entityPathValue, string resultColumnName, int limit = -1,
			string orderColumnName = "CreatedOn", OrderDirection orderType = OrderDirection.Descending, Dictionary<string, string> filters = null)
		{
			var esq = new EntitySchemaQuery(userConnection.EntitySchemaManager, entityName);
			if (limit > 0)
			{
				esq.RowCount = limit;
			}
			var resColumn = esq.AddColumn(resultColumnName);
			if (!string.IsNullOrEmpty(orderColumnName))
			{
				var orderColumn = esq.AddColumn(orderColumnName);
				orderColumn.SetForcedQueryColumnValueAlias("orderColumn");
				orderColumn.OrderDirection = orderType;
				orderColumn.OrderPosition = 0;
			}
			esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, entityPath, entityPathValue));
			if (filters != null)
			{
				foreach (var filter in filters)
				{
					esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, filter.Key, filter.Value));
				}
			}
			return esq.GetEntityCollection(userConnection).Select(x =>
				x.GetColumnValue(resColumn.IsLookup ? PrepareColumn(resColumn.Name, true) : resColumn.Name)
			).ToList();
		}

		public static List<object> CreateColumnValues(UserConnection userConnection, string entityName, string entityPath, object entityPathValue, string resultColumnName, int limit = -1,
			string orderColumnName = "CreatedOn", OrderDirection orderType = OrderDirection.Descending, Dictionary<string, string> filters = null)
		{
			try
			{
				var schema = userConnection.EntitySchemaManager.GetInstanceByName(entityName);
				var insert = new Insert(userConnection)
								.Into(entityName) as Insert;
				object resultValue;
				var resultColumn = schema.Columns.GetByName(resultColumnName);
				if (resultColumn.DataValueType.ValueType == typeof(Guid))
				{
					resultValue = Guid.NewGuid();
				}
				else
				{
					resultValue = resultColumn.DataValueType.DefValue;
				}
				var resColumn = insert.Set(GetSqlNameByEntity(schema, resultColumnName), Column.Parameter(resultValue));
				insert.Set(GetSqlNameByEntity(schema, entityPath), Column.Parameter(entityPathValue));
				if (filters != null)
				{
					foreach (var filter in filters)
					{
						insert.Set(GetSqlNameByEntity(schema, filter.Key), Column.Parameter(filter.Value));
					}
				}
				insert.Execute();
				return new List<object>() { resultValue };
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e);
				return new List<object>();
			}
		}

		public static List<object> GetColumnValuesWithFilters(UserConnection userConnection, string entityName, string entityPath, object entityPathValue, string resultColumnName, Dictionary<string, string> filters, int limit = -1,
			string orderColumnName = "CreatedOn", OrderDirection orderType = OrderDirection.Descending)
		{
			var esq = new EntitySchemaQuery(userConnection.EntitySchemaManager, entityName);
			if (limit > 0)
			{
				esq.RowCount = limit;
			}
			var resColumn = esq.AddColumn(resultColumnName);
			if (!string.IsNullOrEmpty(orderColumnName))
			{
				var orderColumn = esq.AddColumn(orderColumnName);
				orderColumn.SetForcedQueryColumnValueAlias("orderColumn");
				orderColumn.OrderDirection = orderType;
				orderColumn.OrderPosition = 0;
			}
			esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, entityPath, entityPathValue));
			foreach (var filter in filters)
			{
				esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, filter.Key, filter.Value));
			}
			return esq.GetEntityCollection(userConnection).Select(x =>
				x.GetColumnValue(resColumn.IsLookup ? PrepareColumn(resColumn.Name, true) : resColumn.Name)
			).ToList();
		}

		public static string PrepareColumn(string columnName, bool withId = false)
		{
			var endWithId = columnName.EndsWith("Id");
			return withId ? (endWithId ? columnName : columnName + "Id") : (endWithId ? columnName.Substring(0, columnName.Length - 2) : columnName);
		}
		public static bool IsAllNotNullAndEmpty(params object[] values)
		{
			foreach (var value in values)
			{
				if (value == null || (value is string && string.IsNullOrEmpty(value as string)))
					return false;
			}
			return true;
		}
		public static string GetFirstNotNull(params string[] strings)
		{
			return strings.FirstOrDefault(x => !string.IsNullOrEmpty(x));
		}
		public static List<IIntegrationObject> GetCompositeJObjects(object colValue, string colName, string entityName, string handlerName, UserConnection userConnection, int maxCount = -1)
		{
			try
			{
				var jObjectsList = new List<IIntegrationObject>();
				var esq = new EntitySchemaQuery(userConnection.EntitySchemaManager, entityName);
				esq.AddAllSchemaColumns();
				var dateColumn = esq.AddColumn("CreatedOn");
				dateColumn.OrderByDesc();
				if (maxCount > 0)
					esq.RowCount = maxCount;
				esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, colName, colValue));
				var collection = esq.GetEntityCollection(userConnection);
				foreach (var item in collection)
				{
					try
					{
						CsConstant.IntegrationInfo integrationInfo = null;//new CsConstant.IntegrationInfo(new JObject(), userConnection, CsConstant.TIntegrationType.Export, null, handlerName, "", item);
						BaseEntityHandler handler = null;//(new IntegrationEntityHelper()).GetIntegrationHandler(integrationInfo);
						if (handler != null)
						{
							jObjectsList.Add(handler.ToJson(integrationInfo));
						}
					}
					catch (Exception e)
					{
						IntegrationLogger.Error(e);
					}
				}
				return jObjectsList;
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e);
				return new List<IIntegrationObject>();
			}
		}
		public static Tuple<Dictionary<string, string>, Entity> GetEntityByExternalId(string schemaName, int externalId, UserConnection userConnection, bool addAllColumn, params string[] columns)
		{
			var esq = new EntitySchemaQuery(userConnection.EntitySchemaManager, schemaName);
			var columnDict = new Dictionary<string, string>();
			if (addAllColumn)
			{
				esq.AddAllSchemaColumns();
			}
			else
			{
				foreach (var column in columns)
				{
					var columnSchema = esq.AddColumn(column);
					columnDict.Add(column, columnSchema.Name);
				}
			}
			esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, "TsExtenalId", externalId));
			var entity = esq.GetEntityCollection(userConnection).FirstOrDefault();
			return new Tuple<Dictionary<string, string>, Entity>(columnDict, entity);
		}
		public static bool isEntityExist(string schemaName, UserConnection userConnection, Dictionary<string, object> filters)
		{
			var esq = new EntitySchemaQuery(userConnection.EntitySchemaManager, schemaName);
			var schema = userConnection.EntitySchemaManager.GetInstanceByName(schemaName);
			esq.AddColumn(esq.CreateAggregationFunction(AggregationTypeStrict.Count, schema.PrimaryColumn.Name));
			foreach (var filter in filters)
			{
				esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, filter.Key, filter.Value));
			}
			var select = esq.GetSelectQuery(userConnection);
			using (DBExecutor dbExecutor = userConnection.EnsureDBConnection())
			{
				using (IDataReader reader = select.ExecuteReader(dbExecutor))
				{
					while (reader.Read())
					{
						return reader.GetColumnValue<int>("Count") > 0;
					}
				}
			}
			return false;
		}

		public static void UpdateOrInsertEntityColumn(string entityName, string setColumn, object setValue, UserConnection userConnection, Dictionary<string, string> optionalColumns, List<Tuple<string, object>> filters)
		{
			var schema = userConnection.EntitySchemaManager.GetInstanceByName(entityName);

			filters.AddRange(optionalColumns.Select(x => new Tuple<string, object>(x.Key, x.Value)));
			if (GetEntityCount(entityName, userConnection, filters) > 0)
			{
				var update = new Update(userConnection, entityName);
				var selectUpdate = new Select(userConnection)
									.Top(1)
									.Column("Id")
									.From(entityName)
									.OrderByDesc("CreatedOn");
				if (filters.Any())
				{
					selectUpdate.Where(GetSqlNameByEntity(schema, filters[0].Item1)).IsEqual(Column.Parameter(filters[0].Item2));
					foreach (var filter in filters.Skip(1))
					{
						selectUpdate.And(GetSqlNameByEntity(schema, filter.Item1)).IsEqual(Column.Parameter(filter.Item2));
					}
				}
				update.Where("Id").In(selectUpdate);
				update.Set(GetSqlNameByEntity(schema, setColumn), Column.Parameter(setValue));
				foreach (var optionalColumn in optionalColumns)
				{
					update.Set(GetSqlNameByEntity(schema, optionalColumn.Key), Column.Parameter(optionalColumn.Value));
				}
				update.Execute();
			}
			else
			{
				var insert = new Insert(userConnection).Into(entityName);
				insert.Set(GetSqlNameByEntity(schema, setColumn), Column.Parameter(setValue));
				foreach (var optionalColumn in optionalColumns)
				{
					insert.Set(GetSqlNameByEntity(schema, optionalColumn.Key), Column.Parameter(optionalColumn.Value));
				}
				insert.Execute();
			}
		}
		public static string GetSqlNameByEntity(EntitySchema schema, string columnName)
		{
			return schema.Columns.GetByName(columnName).ColumnValueName;
		}
		public static int GetEntityCount(string entityName, UserConnection userConnection, List<Tuple<string, object>> filters)
		{
			var schema = userConnection.EntitySchemaManager.GetInstanceByName(entityName);
			var select = new Select(userConnection)
						.Column(Func.Count("Id")).As("count")
						.From(entityName);
			if (filters.Any())
			{
				select.Where(GetSqlNameByEntity(schema, filters[0].Item1)).IsEqual(Column.Parameter(filters[0].Item2));
				foreach (var filter in filters.Skip(1))
				{
					select.And(GetSqlNameByEntity(schema, filter.Item1)).IsEqual(Column.Parameter(filter.Item2));
				}
			}
			using (DBExecutor dbExecutor = select.UserConnection.EnsureDBConnection())
			{
				using (IDataReader reader = select.ExecuteReader(dbExecutor))
				{
					while (reader.Read())
					{
						return DBUtilities.GetColumnValue<int>(reader, "count");
					}
				}
			}
			return 0;
		}

		public static Dictionary<string, string> ParsToDictionary(string text, char first, char second)
		{
			var result = new Dictionary<string, string>();
			if (string.IsNullOrEmpty(text))
			{
				return result;
			}
			var pairs = text.Split(first);
			foreach (var pair in pairs)
			{
				var values = pair.Split(second);
				result.AddIfNotExists(new KeyValuePair<string, string>(values.First(), values.Last()));
			}
			return result;
		}


	}

	#endregion


	#region Class: UsingHelper
	/*
		Project Path: ..\..\..\QueryConsole\Files\UsingHelper\UsingHelper.cs
		
	*/
	public class UsingHelper
	{
		//Класс заглушка для коректного переноса usings и решение конфликтов с ними
	}

	#endregion


	#region Enum: TIntegrationObjectType
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\IntegrationObject\Type\TIntegrationObjectType.cs
		
	*/
	public enum TIntegrationObjectType
	{
		Json,
		Xml,
		Excell
	}

	#endregion


	#region Enum: TRequstMethod
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Integrator\Service\MethodType\TRequstMethod.cs
		
	*/
	public enum TRequstMethod
	{
		GET,
		POST,
		PUT,
		DELETE
	}

	#endregion


	#region Enum: TLogObjectType
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Logger\Type\TLogObjectType.cs
		
	*/
	public enum TLogObjectType
	{
		None = 0,
		Block = 1,
		Error = 2,
		Info = 3,
		InfoMapping = 4,
		ErrorMapping = 5,
		Warning = 6,
		Lock = 7,
		Unlock = 8,
		InfoMethodArgs = 9,
		InfoMethodResult = 10,
		BlockMethod = 11
	}

	#endregion


	#region Enum: MacrosType
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Mapping\Type\MacrosType.cs
		
	*/
	public enum MacrosType
	{
		Rule,
		OverRule
	}

	#endregion


	#region Enum: TConstType
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Mapping\Type\TConstType.cs
		
	*/
	public enum TConstType
	{
		String = 0,
		Bool = 1,
		Int = 2,
		Null = 3,
		EmptyArray = 4
	}

	#endregion


	#region Enum: TMapExecuteType
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Mapping\Type\TMapExecuteType.cs
		
	*/
	public enum TMapExecuteType
	{
		AfterEntitySave = 0,
		BeforeEntitySave = 1
	}

	#endregion


	#region Interface: IFactory
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Factory\IFactory.cs
		
	*/
	public interface IFactory<out TResult, in TName>
	{
		TResult Get(TName name);
	}

	#endregion


	#region Interface: IEntityPreparer
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Handler\Plugin\Entity\Interface\IEntityPreparer.cs
		
	*/
	public interface IEntityPreparer
	{
		Entity Get(UserConnection userConnection, string schemaName, Guid id);
	}

	#endregion


	#region Interface: IHandlerEntityWorker
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Handler\Plugin\Entity\Interface\IHandlerEntityWorker.cs
		
	*/
	public interface IHandlerEntityWorker
	{
		void SaveEntity(Entity entity, string jName, Action OnSuccess, Action OnError);
		Entity CreateEntity(UserConnection userConnection, string entityName);
		Entity GetEntityByExternalId(UserConnection userConnection, string entityName, string externalIdPath, string externalId);
		Entity GetEntityById(UserConnection userConnection, string entityName, Guid id);
	}

	#endregion


	#region Interface: IHandlerKeyGenerator
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Handler\Plugin\KeyGenerator\Interface\IHandlerKeyGenerator.cs
		
	*/
	public interface IHandlerKeyGenerator
	{
		string GenerateBlockKey(BaseEntityHandler handler, CsConstant.IntegrationInfo integrationInfo);
	}

	#endregion


	#region Interface: IIntegrationObject
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\IntegrationObject\Instance\Interface\IIntegrationObject.cs
		
	*/
	public interface IIntegrationObject
	{
		object GetObject();
		void SetObject(object obj);
		void InitObject(string rootName = null);
		T GetProperty<T>(string name, T defaultValue = default(T));
		void SetProperty(string name, object obj);
		string GetRootName(string defaultValue = null);
		IIntegrationObject GetSubObject(string path);
		IEnumerable<IIntegrationObject> GetSubObjects(string path);
		void FromObject(object obj);
	}

	#endregion


	#region Interface: IIntegrationObjectProvider
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\IntegrationObject\Plugin\Interface\IIntegrationObjectProvider.cs
		
	*/
	public interface IIntegrationObjectProvider
	{
		IIntegrationObject Parse(string text);
		IIntegrationObject NewInstance(string name = null);
		Stream GetMemoryStream(IIntegrationObject iObject);
		string GetContentType(IIntegrationObject iObject);
	}

	#endregion


	#region Interface: IExceptionProcessStrategy
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Integrator\EndPoint\ExceptionProcessStrategy\Interface\IExceptionProcessStrategy.cs
		
	*/
	public interface IExceptionProcessStrategy
	{
		Stream Process(ExceptionProcessData exceptionProcessData);
	}

	#endregion


	#region Interface: IEndPointHandler
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Integrator\EndPoint\Interface\IEndPointHandler.cs
		
	*/
	public interface IEndPointHandler
	{
		List<EndPointHandlerConfig> Config { get; set; }
		string GetImportRoute(IIntegrationObject iObject);
	}

	#endregion


	#region Interface: IIntegrationService
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Integrator\Integrator\Interface\IIntegrationService.cs
		
	*/
	public interface IIntegrationService
	{
		WebRequest Create(ServiceConfig config, string content);
		void SendRequest(WebRequest request, Action<WebResponse> OnResponse, Action<WebException> OnException);
		string GetContentFromResponse(WebResponse response);
	}

	#endregion


	#region Interface: ISyncExportChecker
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Integrator\Integrator\Sync\Interface\ISyncExportChecker.cs
		
	*/
	public interface ISyncExportChecker<T>
	{
		void DoInSync(UserConnection userConnection, string routeKey, T info, Action syncAction);
		bool IsSyncEnable(string routeKey);
	}

	#endregion


	#region Interface: IIntegrator
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Integrator\Interface\IIntegrator.cs
		
	*/
	public interface IIntegrator
	{
		void ExportWithRequest(UserConnection userConnection, Guid id, string schemaName, string routeKey = null);
		void Export(UserConnection userConnection, Guid id, string schemaName, string routeKey = null, Action<IIntegrationObject, ConfigSetting, BaseEntityHandler, Entity> OnGet = null);
		void Import(UserConnection userConnection, IIntegrationObject iObject, string routeKey = null, Action<CsConstant.IntegrationInfo> onSuccess = null, Action<CsConstant.IntegrationInfo, Exception> onError = null);
	}

	#endregion


	#region Interface: IIntegrationObjectWorker
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Integrator\ObjectWorker\Interface\IIntegrationObjectWorker.cs
		
	*/
	public interface IIntegrationObjectWorker
	{
		IIntegrationObject Get(UserConnection userConnection, BaseEntityHandler handler, Entity entity);
		void Import(UserConnection userConnection, IServiceHandlerWorkers handlerWorker, ConfigSetting handlerConfig, IIntegrationObject iObject,
			Action<CsConstant.IntegrationInfo> onSuccess = null, Action<CsConstant.IntegrationInfo, Exception> onError = null);
	}

	#endregion


	#region Interface: IServiceIntegrator
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Integrator\Service\Interface\IServiceIntegrator.cs
		
	*/
	public interface IServiceIntegrator
	{
		void GetRequest(ServiceRequestInfo info);
		void IntegrateBpmEntity(Entity entity, BaseEntityHandler handler = null, bool withLock = true);
	}

	#endregion


	#region Interface: IServiceHandlerWorkers
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Integrator\Service\Manager\Interface\IServiceHandlerWorkers.cs
		
	*/
	public interface IServiceHandlerWorkers
	{
		List<ConfigSetting> GetConfigs(string routeKey, CsConstant.TIntegrationType type);
		BaseEntityHandler GetWithConfig(string name, ConfigSetting config);
		ServiceConfig GetServiceConfig(string serviceName);
		IIntegrationService GetService(string serviceName);
		MappingConfig GetMappingConfig(string configId);
	}

	#endregion


	#region Interface: IServiceRequestWorker
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Integrator\Service\RequestWorker\Interface\IServiceRequestWorker.cs
		
	*/
	public interface IServiceRequestWorker
	{
		void MakeRequest(UserConnection userConnection, IServiceHandlerWorkers serviceHandlerWorker, Entity entity, BaseEntityHandler handler, string serviceName, string content);
	}

	#endregion


	#region Interface: IMacrosCreator
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Mapping\Macros\Instace\Interface\IMacrosCreator.cs
		
	*/
	public interface IMacrosCreator
	{
		Func<object, object> Create();
	}

	#endregion


	#region Interface: IMacrosOverRuleCreator
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Mapping\Macros\Instace\Interface\IMacrosOverRuleCreator.cs
		
	*/
	public interface IMacrosOverRuleCreator
	{
		Func<object, CsConstant.IntegrationInfo, object> Create();
	}

	#endregion


	#region Interface: IMapper
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Mapping\Mapper\Interface\IMapper.cs
		
	*/
	public interface IMapper
	{
		void StartMappByConfig(CsConstant.IntegrationInfo integrationInfo, string jName, MappingConfig mapConfig);
		bool CheckIsExist(UserConnection userConnection, string entityName, object externalId, string externalIdPath = "TsExternalId", object entityExternalId = null);
		void ExecuteMapMethodQueue();
	}

	#endregion


	#region Interface: IMapperDbWorker
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Mapping\Plugin\Interface\IMapperDbWorker.cs
		
	*/
	public interface IMapperDbWorker
	{
		bool IsExists(UserConnection userConnection, string entityName, string externalIdPath, object externalId);
	}

	#endregion


	#region Interface: IMappRule
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Mapping\Rules\Instance\Interface\IMappRule.cs
		
	*/
	public interface IMappRule
	{
		void Import(RuleImportInfo info);
		void Export(RuleExportInfo info);
	}

	#endregion


	#region Interface: IMappRuleOptimizator
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Mapping\Rules\Optimizator\Model\Interface\IMappRuleOptimizator.cs
		
	*/
	public interface IMappRuleOptimizator
	{
		void Optimize(IntegrationInfo integrationInfo);
	}

	#endregion


	#region Interface: ITemplateFactory
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\ServiceConstructor\Model\Interface\ITemplateFactory.cs
		
	*/
	public interface ITemplateFactory
	{
		ITemplateHandler Get(string name);
	}

	#endregion


	#region Interface: ITemplateHandler
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\ServiceConstructor\Model\Interface\ITemplateHandler.cs
		
	*/
	public interface ITemplateHandler
	{
		void Import(TemplateSetting templateSettings, IntegrationInfo integrationInfo);
		void Export(TemplateSetting templateSettings, IntegrationInfo integrationInfo);
	}

	#endregion


	#region Interface: ITriggerCheckerItem
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Trigger\Interface\ITriggerCheckerItem.cs
		
	*/
	public interface ITriggerCheckerItem<T>
	{
		bool Check(string eventName, T eventInfo, Action<TriggerSetting> onMath);
		TriggerSetting GetSetting();
	}

	#endregion


	#region Interface: ITriggerCollection
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Trigger\Interface\ITriggerCollection.cs
		
	*/
	public interface ITriggerCollection<T>: ITriggerCheckerItem<T>
	{
		TriggerSetting GetSettingByName(string name);
	}

	#endregion


	#region Interface: ITriggerEngine
	/*
		Project Path: ..\..\..\QueryConsole\Files\Core\Trigger\Interface\ITriggerEngine.cs
		
	*/
	public interface ITriggerEngine<T>
	{
		void Push(string eventName, T eventInfo);
		TriggerSetting GetTriggerByName(string name, UserConnection userConnection);
	}

	#endregion

}