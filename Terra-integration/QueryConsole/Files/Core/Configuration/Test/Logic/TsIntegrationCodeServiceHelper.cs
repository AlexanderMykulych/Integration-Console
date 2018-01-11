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
					integrator = ObjectFactory.Get<BaseIntegratorMock>();
				}
				else
				{
					integrator = ObjectFactory.Get<IIntegrator>();
				}
				integrator.ExportWithRequest(info.Id, info.SchemaName, info.RouteKey);
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
								var integrationInfo = CsConstant.IntegrationInfo.CreateForExport(entity);
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
								var integrationInfoCreate = CsConstant.IntegrationInfo.CreateForImport(CsConstant.IntegrationActionName.Create, jObj);
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
							var integrationInfoExists = CsConstant.IntegrationInfo.CreateForImport(CsConstant.IntegrationActionName.Create, jObj);
							isEntityExist = handler.IsEntityAlreadyExist(integrationInfoExists);
							builder.AppendLineFormat("Результат поиска: {0}", isEntityExist);
						}
						if (info.IsUpdate)
						{
							var integrationInfoUpdate = CsConstant.IntegrationInfo.CreateForImport(CsConstant.IntegrationActionName.Update, jObj);
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
}