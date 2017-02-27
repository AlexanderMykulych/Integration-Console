using IntegrationInfo = Terrasoft.Configuration.CsConstant.IntegrationInfo;
using Newtonsoft.Json.Linq;
using NodaTime.TimeZones.Cldr;
using NodaTime.TimeZones;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.ServiceModel;
using System.Text.RegularExpressions;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Web.Configuration;
using System.Web;
using System.Xml;
using System;
using Terrasoft.Common;
using Terrasoft.Configuration;
using Terrasoft.Core.Configuration;
using Terrasoft.Core.DB;
using Terrasoft.Core.Entities;
using Terrasoft.Core.Process;
using Terrasoft.Core;
using TIntegrationType = Terrasoft.Configuration.CsConstant.TIntegrationType;

namespace Terrasoft.Configuration {

	#region Class: ExtensionHelper
	/*
		Project Path: ..\..\..\QueryConsole\Files\TsClientServiceInegration.TsBase.cs
		
	*/

	public static class ExtensionHelper {
				/// <summary>
		/// Сериализирует объект
		/// </summary>
		/// <param name="obj">Объект</param>
		/// <returns></returns>
		public static string SerializeToJson(this object obj) {
			return Newtonsoft.Json.JsonConvert.SerializeObject(obj).Replace("ReferenceClientService", "#ref");
		}

		/// <summary>
		/// Десериализирует объект
		/// </summary>
		/// <param name="json">json текс</param>
		/// <returns></returns>
		public static Dictionary<string, object> DeserializeJson(this string json) {
			return Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
		}
		 
	}

	#endregion


	#region Class: MappingMethodAttribute
	/*
		Project Path: ..\..\..\QueryConsole\Files\TsClientServiceInegration.TsBase.cs
		
	*/

	public class MappingMethodAttribute : System.Attribute {
		private string methodName;
		public string MethodName {
			get {
				return methodName;
			}
		}

		public MappingMethodAttribute(string methodName) {
			this.methodName = methodName;
		}
	}

	#endregion


	#region Class: IntegrationConfigurationManager
	/*
		Project Path: ..\..\..\QueryConsole\Files\TsClientServiceInegration.TsBase.cs
		
	*/

	public static class IntegrationConfigurationManager {
		private static List<string> _columnNames;
		private static string _xmlData;
		private static XmlDocument _xDocument;
		private static MappingItem _defaultItem;
		private static Dictionary<string, string> _prerenderConfigDict;
		private static IntegrationPathConfig _pathConfig;
		private static string xmlConfigurationLocation;
		public static string XmlConfigurationLocation {
			get {
				if(string.IsNullOrEmpty(xmlConfigurationLocation)) {
					xmlConfigurationLocation = CsConstant.IsDebugMode ? "file" : "db";
				}
				return xmlConfigurationLocation;
			}
		}
		private static string xmlConfigurationFilePath;
		public static string XmlConfigurationFilePath {
			get {
				if(string.IsNullOrEmpty(xmlConfigurationFilePath)) {
					xmlConfigurationFilePath = "../../ConfigurationFile.xml";
				}
				return xmlConfigurationFilePath;
			}
		}
		public static IntegrationPathConfig IntegrationPathConfig {
			get {
				if (_pathConfig == null) {
					if(_xDocument == null) {
						_xDocument = GetConfigXmlDocument(CsConstant.UserConnection);
					}
					var node = _xDocument[CsConstant.XmlManagerConstant.XmlConfigRootNodeName][CsConstant.XmlManagerConstant.XmlConfigEntityConfigNodeName];
					_pathConfig = new IntegrationPathConfig();
					var resultList = new List<IntegrationPath>();
					var pathType = typeof(IntegrationPath);
					foreach (XmlNode pathNode in node.ChildNodes) {
						var path = DynamicXmlParser.StartMapXmlToObj<IntegrationPath>(pathNode, pathType);
						if (path != null) {
							resultList.Add(path);
						}
					}
					_pathConfig.Paths = resultList;
				}
				return _pathConfig;
			}
		}

				/// <summary>
		/// Возвращает xml документ c настройками маппинга
		/// </summary>
		/// <param name="userConnection"></param>
		/// <returns></returns>
		private static XmlDocument GetConfigXmlDocument(UserConnection userConnection) {
			try {
				if(_xDocument != null) {
					return _xDocument;
				}

				if (XmlConfigurationLocation == "db") {
					if(string.IsNullOrEmpty(_xmlData)) {
						_xmlData = CsConstant.IntegratorSettings.MappingConfiguration;
					}
				} else if (XmlConfigurationLocation == "file") {
					if(string.IsNullOrEmpty(_xmlData)) {
						using (var stream = new StreamReader(XmlConfigurationFilePath)) {
							_xmlData = stream.ReadToEnd();
						}
					}
				}
				if(_xDocument == null) {
					_xDocument = new XmlDocument();
					_xDocument.LoadXml(_xmlData);
				}
				return _xDocument;
			} catch(Exception e) {
				IntegrationLogger.Error(e);
				throw;
			}
		}

		/// <summary>
		/// Возвращает ноду с именем name, документа doc
		/// </summary>
		/// <param name="doc">Документ</param>
		/// <param name="name">Имя ноды</param>
		/// <returns></returns>
		private static XmlNode GetXmlNodeByNameAttr(XmlDocument doc, string name, string tag = null) {
			try {
				foreach(XmlNode node in doc.DocumentElement) {
					if(node is XmlComment)
						continue;
					if(
						(
							(node.Attributes["TsName"] != null && node.Attributes["TsName"].Value == name) ||
							(node.Attributes["JName"] != null && node.Attributes["JName"].Value == name)
						) &&
						(
							string.IsNullOrEmpty(tag) || (node.Attributes["Tag"] != null && node.Attributes["Tag"].Value == tag)
						)
					) {
						return node;
					}
				}
				return null;
			} catch(Exception e) {
				IntegrationLogger.Error(e, "GetXmlNodeByNameAttr");
				throw;
			}
		}

		/// <summary>
		/// Возвращает елемент маппинга по ноде из документа конфигурации
		/// </summary>
		/// <param name="userConnection"></param>
		/// <param name="node">Нода</param>
		/// <param name="defItem">Елемент маппинга по умолчанию. Если в ноде не будет какого-то поля, то подставится поле с этого объекта</param>
		/// <returns></returns>
		private static MappingItem GetItemByXmlNode(UserConnection userConnection, XmlNode node, MappingItem defItem = null) {
			try {
				var resultObj = Activator.CreateInstance(_mapItemType) as MappingItem;
				bool isAttrSetting = false;
				foreach(string attributeName in ColumnNames) {
					isAttrSetting = false;
					PropertyInfo propertyInfo = _mapItemType.GetProperty(attributeName);
					var xmlAttribute = node.Attributes[attributeName];
					if(xmlAttribute != null) {
						string xmlValue = PrepareValue(userConnection, xmlAttribute.Value);
						if(propertyInfo != null) {
							Type propertyType = propertyInfo.PropertyType;

							if(propertyType.IsEnum || propertyType == typeof(int)) {
								isAttrSetting = true;
								propertyInfo.SetValue(resultObj, int.Parse(xmlValue));
							} else if(propertyType == typeof(bool)) {
								isAttrSetting = true;
								propertyInfo.SetValue(resultObj, xmlValue != "0");
							} else {
								isAttrSetting = true;
								propertyInfo.SetValue(resultObj, xmlValue);
							}
						}
					}
					if(!isAttrSetting && defItem != null) {
						propertyInfo.SetValue(resultObj, propertyInfo.GetValue(defItem));
					}
				}
				return resultObj;
			} catch(Exception e) {
				IntegrationLogger.Error(e, "GetItemByXmlNode");
				throw;
			}
		}

		/// <summary>
		/// Возращает конфиг пререндеринга prerenderConfig
		/// </summary>
		/// <param name="userConnection"></param>
		/// <returns></returns>
		private static Dictionary<string, string> GetPrerenderConfig(UserConnection userConnection) {
			try {
				if(_prerenderConfigDict != null && _prerenderConfigDict.Any())
					return _prerenderConfigDict;
				var doc = GetConfigXmlDocument(userConnection);
				var element = doc.DocumentElement["prerenderConfig"];
				_prerenderConfigDict = new Dictionary<string, string>();
				if(element != null) {
					foreach(XmlNode confItem in element.ChildNodes) {
						string from = confItem.Attributes["From"].Value;
						string to = confItem.Attributes["To"].Value;
						if(!string.IsNullOrEmpty(from) && !string.IsNullOrEmpty(to)) {
							_prerenderConfigDict.Add(from, to);
						}
					}
				}
				return _prerenderConfigDict;
			} catch(Exception e) {
				IntegrationLogger.Error(e, "GetPrerenderConfig");
				throw;
			}
		}

		/// <summary>
		/// Подготавливает конфигурацию маппинга с помощю конфигурации prerenderConfig.
		/// </summary>
		/// <param name="userConnection"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		private static string PrepareValue(UserConnection userConnection, string value) {
			try {
				var configDict = GetPrerenderConfig(userConnection);
				if(configDict.ContainsKey(value))
					return configDict[value];
				return value;
			} catch(Exception e) {
				IntegrationLogger.Error(e, "PrepareValue");
				throw;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="userConnection"></param>
		/// <returns></returns>
		private static MappingItem GetDefaultItem(UserConnection userConnection) {
			try {
				return _defaultItem == null ? _defaultItem = GetItemByXmlNode(userConnection, GetXmlNodeByNameAttr(GetConfigXmlDocument(userConnection), "Default").ChildNodes[0]) : _defaultItem;
			} catch(Exception e) {
				IntegrationLogger.Error(e, "GetDefaultItem");
				throw;
			}
		}

		private static MappingItem GetDafaultItemByType(UserConnection userConnection, string type, MappingItem defaultItem = null) {
			try
			{
				var document = GetConfigXmlDocument(userConnection);
				foreach(XmlNode node in document.DocumentElement) {
					if(node.Name == "configItemType") {
						foreach(XmlNode childNode in node.ChildNodes) {
							if (childNode.Attributes["MapType"] != null && childNode.Attributes["MapType"].Value == type)
							{
								return GetItemByXmlNode(userConnection, childNode, defaultItem);
							}
						}
					} 
				}
				return defaultItem;
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e, "GetDafaultItemByType");
				throw;
			}
		}

		 

		public static List<MappingItem> GetConfigItem(UserConnection userConnection, string Name, string tag = null) {
			try {
				var result = new List<MappingItem>();
				XmlDocument xDoc = GetConfigXmlDocument(userConnection);
				var node = GetXmlNodeByNameAttr(xDoc, Name, tag);
				
				var defItem = GetDefaultItem(userConnection);
				foreach(XmlNode mapItem in node.ChildNodes) {
					if(mapItem is XmlElement) {
						string type = null;
						if (mapItem.Attributes["MapType"] != null)
						{
							type = mapItem.Attributes["MapType"].Value;
						} else {
							type = defItem.MapType.ToString();
						}
						var typeDefItem = GetDafaultItemByType(userConnection, type, defItem);
						result.Add(GetItemByXmlNode(userConnection, mapItem, typeDefItem));
					}
				}
				return result;
			} catch(Exception e) {
				IntegrationLogger.Error(e, "GetConfigItem");
				throw;
			}
		}
		 

				public static Type _mapItemType = typeof(MappingItem);
		 

				public static List<string> ColumnNames {
			get {
				return _columnNames == null || !_columnNames.Any() ? _columnNames = _mapItemType.GetProperties().Where(x => x.MemberType == MemberTypes.Property).Select(x => x.Name).ToList() : _columnNames;
			}
		}
		 
		
	}

	#endregion


	#region Class: EntityHandler
	/*
		Project Path: ..\..\..\QueryConsole\Files\TsIntegrationServiceEntity.TsBase.cs
		
	*/
	public abstract class EntityHandler
	{
		public MappingHelper Mapper;
		public string EntityName;
		public string JName;
		public virtual string HandlerName {
			get {
				return EntityName;
			}
		}
		public virtual string SettingName {
			get {
				return JName;
			}
		}
		private string _ServiceName;
		public virtual string ServiceName {
			get {
				if (string.IsNullOrEmpty(_ServiceName))
				{
					var handlerSetting = IntegrationConfigurationManager.IntegrationPathConfig.Paths.FirstOrDefault(x => x.Name == SettingName);
					if (handlerSetting != null)
					{
						_ServiceName = handlerSetting.ServiceName;
					}
					else
					{
						IntegrationLogger.CurrentLogger.Instance.Error(string.Format("Problem with ({0}-{1}-{2}-{3}) handler setting", JName, EntityName, HandlerName, this.GetType().Name));
					}
				}
				return _ServiceName;
			}
		}
		public virtual TServiceObject ServiceObjectType {
			get {
				return TServiceObject.Entity;
			}
		}
		public virtual string ExternalIdPath {
			get {
				return CsConstant.ServiceColumnInBpm.Identifier;
			}
		}
		public virtual string ExternalVersionPath {
			get {
				return CsConstant.ServiceColumnInBpm.Version;
			}
		}
		public virtual bool IsJsonWithHeader {
			get {
				return true;
			}
		}
		public virtual bool IsEmbeddedObject {
			get {
				return false;
			}
		}

		public virtual string ParentObjectJName {
			get {
				return string.Empty;
			}
		}
		public virtual string ParentObjectTsName {
			get {
				return string.Empty;
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
		public virtual void Create(IntegrationInfo integrationInfo)
		{
			integrationInfo.TsExternalIdPath = ExternalIdPath;
			integrationInfo.TsExternalVersionPath = ExternalVersionPath;
			var entitySchema = integrationInfo.UserConnection.EntitySchemaManager.GetInstanceByName(EntityName);
			integrationInfo.IntegratedEntity = entitySchema.CreateEntity(integrationInfo.UserConnection);
			integrationInfo.IntegratedEntity.SetDefColumnValues();
			BeforeMapping(integrationInfo);
			Mapper.StartMappByConfig(integrationInfo, JName, GetMapConfig(integrationInfo.UserConnection), IsJsonWithHeader);
			AfterMapping(integrationInfo);
			try
			{
				Mapper.SaveEntity(integrationInfo.IntegratedEntity, JName, ServiceName);
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
		}
		public virtual void BeforeMapping(IntegrationInfo integrationInfo)
		{
		}
		public virtual void AfterMapping(IntegrationInfo integrationInfo)
		{
		}
		public virtual void AfterEntitySave(IntegrationInfo integrationInfo)
		{
		}
		public virtual void Update(IntegrationInfo integrationInfo)
		{
			integrationInfo.TsExternalIdPath = ExternalIdPath;
			integrationInfo.TsExternalVersionPath = ExternalVersionPath;
			var entity = GetEntityByExternalId(integrationInfo);
			if (entity != null)
			{
				integrationInfo.IntegratedEntity = entity;
				if (IsVersionHigger(integrationInfo))
				{
					BeforeMapping(integrationInfo);
					Mapper.StartMappByConfig(integrationInfo, JName, GetMapConfig(integrationInfo.UserConnection));
					AfterMapping(integrationInfo);
					Mapper.SaveEntity(entity, JName, ServiceName);
					AfterEntitySave(integrationInfo);
				}
			}
			else
			{
				throw new Exception(string.Format("Can not create entity {0}", EntityName));
			}
		}
		public virtual void Delete(IntegrationInfo integrationInfo)
		{
			throw new NotImplementedException();
		}
		public virtual void Unknown(IntegrationInfo integrationInfo)
		{
			Update(integrationInfo);
		}
		public virtual bool IsEntityAlreadyExist(IntegrationInfo integrationInfo)
		{
			integrationInfo.TsExternalIdPath = ExternalIdPath;
			integrationInfo.TsExternalVersionPath = ExternalVersionPath;
			Mapper.UserConnection = integrationInfo.UserConnection;
			int externalId = 0;
			if (integrationInfo.IntegratedEntity != null)
			{
				externalId = integrationInfo.IntegratedEntity.GetTypedColumnValue<int>(ExternalIdPath);
			}
			bool result = Mapper.CheckIsExist(EntityName, integrationInfo.Data[JName].Value<int>("id"), integrationInfo.TsExternalIdPath, externalId);
			if (!result && IsAdvancedSearch)
			{
				result = IsEntityAlreadyExistAdvanced(integrationInfo);
			}
			return result;
		}
		public virtual bool IsEntityAlreadyExistAdvanced(IntegrationInfo integrationInfo)
		{
			integrationInfo.TsExternalIdPath = ExternalIdPath;
			integrationInfo.TsExternalVersionPath = ExternalVersionPath;
			if(integrationInfo.IntegratedEntity != null)
			{
				return true;
			}
			if(AdvancedSearchInfo == null)
			{
				return false;
			}
			Guid resultId = AdvancedSearchInfo.Search(integrationInfo.UserConnection,
					procedure => AddParameterToSearchProcedure(integrationInfo, procedure), IntegrationLogger.SimpleLoggerErrorAction);
			if(resultId == Guid.Empty)
			{
				return false;
			}
			var entityEsq = new EntitySchemaQuery(integrationInfo.UserConnection.EntitySchemaManager, EntityName);
			entityEsq.AddAllSchemaColumns();
			integrationInfo.IntegratedEntity = entityEsq.GetEntity(integrationInfo.UserConnection, resultId);
			return true;
		}
		public virtual void AddParameterToSearchProcedure(IntegrationInfo integrationInfo, StoredProcedure searchProcedure)
		{
			return;
		}
		public virtual void ProcessResponse(IntegrationInfo integrationInfo)
		{
			integrationInfo.TsExternalIdPath = ExternalIdPath;
			integrationInfo.TsExternalVersionPath = ExternalVersionPath;
			if (!string.IsNullOrEmpty(integrationInfo.StrData))
			{
				integrationInfo.Data = Mapper.GetJObject(integrationInfo.StrData);
			}
			if (integrationInfo.IntegratedEntity == null)
			{
				integrationInfo.IntegratedEntity = GetEntityByExternalId(integrationInfo);
			}
			BeforeMapping(integrationInfo);
			Mapper.StartMappByConfig(integrationInfo, JName, GetMapConfig(integrationInfo.UserConnection));
			AfterMapping(integrationInfo);
			try
			{
				Mapper.SaveEntity(integrationInfo.IntegratedEntity, JName, ServiceName, true);
				integrationInfo.Result = new CsConstant.IntegrationResult()
				{
					Type = CsConstant.IntegrationResult.TResultType.Success
				};
				CheckDuplicateByExternalId(integrationInfo);
			}
			catch (Exception e)
			{
				integrationInfo.Result = new CsConstant.IntegrationResult()
				{
					Type = CsConstant.IntegrationResult.TResultType.Exception,
					ExceptionMessage = e.Message
				};
				IntegrationLogger.Error(e, integrationInfo.ToString());
			}
		}
		public virtual JObject ToJson(IntegrationInfo integrationInfo)
		{
			integrationInfo.TsExternalIdPath = ExternalIdPath;
			integrationInfo.TsExternalVersionPath = ExternalVersionPath;
			BeforeMapping(integrationInfo);
			Mapper.StartMappByConfig(integrationInfo, JName, GetMapConfig(integrationInfo.UserConnection));
			AfterMapping(integrationInfo);
			return integrationInfo.Data;
		}
		public virtual List<MappingItem> GetMapConfig(UserConnection userConnection)
		{
			return IntegrationConfigurationManager.GetConfigItem(userConnection, HandlerName);
		}
		public virtual Entity GetEntityByExternalId(CsConstant.IntegrationInfo integrationInfo)
		{
			if (IsAdvancedSearch && integrationInfo.IntegratedEntity != null)
			{
				return integrationInfo.IntegratedEntity;
			}
			else
			{
				string externalIdPath = integrationInfo.TsExternalIdPath;
				var esq = new EntitySchemaQuery(integrationInfo.UserConnection.EntitySchemaManager, EntityName);
				esq.AddAllSchemaColumns();
				esq.RowCount = 1;
				esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, externalIdPath, integrationInfo.Data[JName].Value<int>("id")));
				return esq.GetEntityCollection(integrationInfo.UserConnection).FirstOrDefault();
			}
		}
		public virtual bool IsExport(CsConstant.IntegrationInfo integrationInfo)
		{
			return true;
		}
		public virtual ServiceRequestInfo GetRequestInfo(CsConstant.IntegrationInfo integrationInfo)
		{
			var id = integrationInfo.IntegratedEntity.GetTypedColumnValue<int>(ExternalIdPath);
			var requestInfo = new ServiceRequestInfo()
			{
				ServiceObjectId = id.ToString(),
				ServiceObjectName = JName,
				Type = ServiceObjectType,
				RequestJson = integrationInfo.Result.Data.ToString(),
				Entity = integrationInfo.IntegratedEntity
			};
			if (id > 0)
			{
				requestInfo.Method = TRequstMethod.PUT;
			}
			else
			{
				requestInfo.Method = TRequstMethod.POST;
			}
			requestInfo.FullUrl = ServiceUrlMaker.MakeUrl(CsConstant.IntegratorSettings.GetUrlsByServiceName(ServiceName)[ServiceObjectType], requestInfo);
			requestInfo.Handler = this;
			return requestInfo;
		}
		public virtual void CheckDuplicateByExternalId(IntegrationInfo integrationInfo)
		{
			try
			{
				if (integrationInfo.Action == CsConstant.IntegrationActionName.Create && integrationInfo.IntegrationType == CsConstant.TIntegrationType.ExportResponseProcess && integrationInfo.IntegratedEntity != null)
				{
					int externalId = GetExternalIdValue(integrationInfo);
					var primaryColumnName = integrationInfo.IntegratedEntity.Schema.GetPrimaryColumnName();
					if (externalId == 0)
					{
						return;
					}
					if (EntityHelper.IsExistDuplicateByExternalId(integrationInfo.UserConnection, EntityName, ExternalIdPath, externalId, IntegrationLogger.SimpleLoggerErrorAction))
					{
						EntityHelper.ClearDuplicateExternalIdByIds(
							integrationInfo.UserConnection,
							EntityName,
							primaryColumnName,
							ExternalIdPath,
							externalId,
							integrationInfo.IntegratedEntity.GetTypedColumnValue<Guid>(primaryColumnName),
							IntegrationLogger.SimpleLoggerErrorAction
						);
					}
				}
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e, integrationInfo.ToString());
			}
		}
		public virtual int GetExternalIdValue(IntegrationInfo integrationInfo)
		{
			if (integrationInfo.Data != null)
			{
				var idToken = integrationInfo.Data.SelectToken(JName + ".id");
				if (idToken != null)
				{
					return idToken.Value<int>();
				}
			}
			return 0;
		}
		public virtual bool IsVersionHigger(IntegrationInfo integrationInfo)
		{
			int serviceVersion = 0, bpmVersion = 0;
			if (integrationInfo.IntegratedEntity != null &&
				integrationInfo.IntegratedEntity.IsColumnValueLoaded(ExternalVersionPath))
			{
				bpmVersion = integrationInfo.IntegratedEntity.GetTypedColumnValue<int>(ExternalVersionPath);
			}
			if (integrationInfo.Data != null)
			{
				serviceVersion = integrationInfo.Data.SelectToken(JName + ".version").Value<int>();
			}
			return serviceVersion > bpmVersion;
		}
	}

	#endregion


	#region Class: AccountHandler
	/*
		Project Path: ..\..\..\QueryConsole\Files\TsIntegrationServiceEntity.TsBase.cs
		
	*/

	[ImportHandlerAttribute("CompanyProfile")]
	[ExportHandlerAttribute("Account")]
	public class AccountHandler : EntityHandler
	{
		private AdvancedSearchInfo _advancedSearchInfo;
		

		public AccountHandler()
		{
			Mapper = new MappingHelper();
			EntityName = "Account";
			JName = "CompanyProfile";
			_advancedSearchInfo = new AdvancedSearchInfo()
			{
				StoredProcedureName = CsConstant.EntityConst.AccountConst.AccountSearchStoredProcedureB2c
			};
		}
		public override bool IsAdvancedSearch {
			get {
				return true;
			}
		}
		public override AdvancedSearchInfo AdvancedSearchInfo {
			get {
				return _advancedSearchInfo;
			}
		}
		public override JObject ToJson(IntegrationInfo integrationInfo)
		{
			var result = base.ToJson(integrationInfo);

			try
			{
				if (!result.IsJTokenPathHasValue("CompanyProfile.taxRegistrationNumber"))
				{
					result.RemoveByPath("CompanyProfile.taxRegistrationNumberName");
					result.RemoveByPath("CompanyProfile.taxRegistrationNumber");
				}
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e);
			}

			try
			{
				if (!result.IsJTokenPathHasValue("CompanyProfile.companyRegistrationNumber"))
				{
					result.RemoveByPath("CompanyProfile.companyRegistrationNumberName");
					result.RemoveByPath("CompanyProfile.companyRegistrationNumber");
				}
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e);
			}
			return result;
		}
		public override void BeforeMapping(IntegrationInfo integrationInfo)
		{
			try
			{
				if (integrationInfo.IntegrationType == CsConstant.TIntegrationType.Import)
				{
					integrationInfo.IntegratedEntity.ClearAllAddressField();
				}
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e);
			}
		}

		public override void AfterEntitySave(IntegrationInfo integrationInfo)
		{
			base.AfterEntitySave(integrationInfo);
			//AccountEntityHelper.ClearAccountPrimaryCommunication(integrationInfo.UserConnection, integrationInfo.IntegratedEntity, IntegrationLogger.SimpleLoggerErrorAction);
			AccountEntityHelper.ResaveAccountPrimaryAddress(integrationInfo.UserConnection, integrationInfo.IntegratedEntity, integrationInfo.IntegratedEntity.PrimaryColumnValue, IntegrationLogger.SimpleLoggerErrorAction);
			//http://tscore-task/browse/SKT-4696
			AccountEntityHelper.SynchronizeCommunication(integrationInfo.UserConnection, integrationInfo.IntegratedEntity.PrimaryColumnValue, IntegrationLogger.SimpleLoggerErrorAction);
		}

		public override void AddParameterToSearchProcedure(IntegrationInfo integrationInfo, StoredProcedure searchProcedure)
		{
			if (integrationInfo.Data != null)
			{
				if (AdvancedSearchInfo.StoredProcedureName == CsConstant.EntityConst.AccountConst.AccountSearchStoredProcedureB2c)
				{
					var emailPhones = GetEmailPhones(integrationInfo.Data);
					searchProcedure
						.WithParameter("Emails", emailPhones.Item1 ?? string.Empty)
						.WithParameter("Phones", emailPhones.Item2 ?? string.Empty);
				} else if (AdvancedSearchInfo.StoredProcedureName == CsConstant.EntityConst.AccountConst.AccountSearchStoredProcedureB2b)
				{
					var innAndKpp = GetInnAndKpp(integrationInfo.Data);
					searchProcedure
						.WithParameter("Inn", innAndKpp.Item1 ?? string.Empty)
						.WithParameter("Kpp", innAndKpp.Item2 ?? string.Empty);
				}
			}
			
		}
		/// <summary>
		/// Возвращает ИНН и КПП из json-строки
		/// </summary>
		/// <param name="data">JObject</param>
		/// <returns>Кортеж с ИНН и КПП</returns>
		private Tuple<string, string> GetInnAndKpp(JObject data)
		{
			var inn = string.Empty;
			var kpp = string.Empty;
			try
			{
				var innToken = data.SelectToken(JName + ".taxRegistrationNumber");
				var kppToken = data.SelectToken(JName + ".companyRegistrationNumber");
				if(innToken != null)
				{
					inn = innToken.Value<string>();
				}
				if(kppToken != null)
				{
					kpp = kppToken.Value<string>();
				}
			} catch(Exception e)
			{
				IntegrationLogger.Error(e);
			}
			return new Tuple<string, string>(inn, kpp);
		}
		/// <summary>
		/// Возвращает телефоны и мейлы из json-строки
		/// </summary>
		/// <param name="data">JObject</param>
		/// <returns>Кортеж с мейлами и телефонами</returns>
		private Tuple<string, string> GetEmailPhones(JObject data)
		{
			var emails = string.Empty;
			var phone = string.Empty;
			try
			{
				var communicationToken = data.SelectToken(JName + ".mainContactInfo.ContactInfo.records");
				if (communicationToken != null && communicationToken is JArray)
				{
					var communicationJArray = (JArray)communicationToken;
					emails = string.Join(",", communicationJArray
						.Where(x =>
						{
							var mailIdToken = x.SelectToken("ContactRecord.type.#ref.id");
							return mailIdToken != null && mailIdToken.Value<int>() == 1;
						})
						.Select(x => x.SelectToken("ContactRecord.value").Value<string>()));
					phone = string.Join(",", communicationJArray
						.Where(x =>
						{
							var mailIdToken = x.SelectToken("ContactRecord.type.#ref.id");
							return mailIdToken != null && mailIdToken.Value<int>() == 2;
						})
						.SelectMany(x => PhoneFormatHelper.ToAllFormats(x.SelectToken("ContactRecord.value").Value<string>())));
				}
			} catch(Exception e)
			{
				IntegrationLogger.Error(e);
			}
			return new Tuple<string, string>(emails, phone);
		}
		/// <summary>
		/// Разширеный поиск существующего контрагента
		/// </summary>
		/// <param name="integrationInfo">Информация о интеграции</param>
		/// <returns></returns>
		public override bool IsEntityAlreadyExistAdvanced(IntegrationInfo integrationInfo)
		{
			SetStoredProcedureNameByAccountInfo(integrationInfo);
			return base.IsEntityAlreadyExistAdvanced(integrationInfo);
		}
		/// <summary>
		/// В зависимости от бизнес-протокола контрагента устанавливает процедуру поиска
		/// </summary>
		/// <param name="integrationInfo">Информация о интеграции</param>
		private void SetStoredProcedureNameByAccountInfo(IntegrationInfo integrationInfo)
		{
			var jObj = integrationInfo.Data;
			if(jObj != null)
			{
				var isB2b = jObj.SelectToken(JName + ".b2b").Value<bool>();
				var isB2c = jObj.SelectToken(JName + ".b2c").Value<bool>();
				if (isB2b)
				{
					_advancedSearchInfo.StoredProcedureName = CsConstant.EntityConst.AccountConst.AccountSearchStoredProcedureB2b;
					return;
				} else if(isB2c)
				{
					_advancedSearchInfo.StoredProcedureName = CsConstant.EntityConst.AccountConst.AccountSearchStoredProcedureB2c;
					return;
				} else
				{
					var isB2s = jObj.SelectToken(JName + ".b2s").Value<bool>();
					if(isB2s)
					{
						var innToken = jObj.SelectToken(JName + ".taxRegistrationNumber");
						var kppToken = jObj.SelectToken(JName + ".companyRegistrationNumber");
						if(innToken == null || kppToken == null || string.IsNullOrEmpty(innToken.Value<string>()) || string.IsNullOrEmpty(kppToken.Value<string>()))
						{
							var firstEmailToken = jObj.SelectToken(JName + ".mainContactInfo.ContactInfo.records[?(@.ContactRecord.type.#ref.id == 1)][last()]");
							_advancedSearchInfo.StoredProcedureName = firstEmailToken != null ?
								CsConstant.EntityConst.AccountConst.AccountSearchStoredProcedureB2c :
								CsConstant.EntityConst.AccountConst.AccountSearchStoredProcedureB2b;
						} else
						{
							_advancedSearchInfo.StoredProcedureName = CsConstant.EntityConst.AccountConst.AccountSearchStoredProcedureB2b;
						}
					}
				}
			}
			_advancedSearchInfo.StoredProcedureName = string.Empty;
		}
	}

	#endregion


	#region Class: ContactHandler
	/*
		Project Path: ..\..\..\QueryConsole\Files\TsIntegrationServiceEntity.TsBase.cs
		
	*/

	[ImportHandlerAttribute("PersonProfile")]
	[ExportHandlerAttribute("Contact")]
	public class ContactHandler : EntityHandler
	{
		private AdvancedSearchInfo _advancedSearchInfo;

		public ContactHandler()
		{
			Mapper = new MappingHelper();
			EntityName = "Contact";
			JName = "PersonProfile";
			_advancedSearchInfo = new AdvancedSearchInfo()
			{
				StoredProcedureName = CsConstant.EntityConst.ContactConst.ContactSearchStoredProcedure
			};
		}

		public override void BeforeMapping(IntegrationInfo integrationInfo)
		{
			if (integrationInfo.IntegrationType == CsConstant.TIntegrationType.Import && integrationInfo.Action == CsConstant.IntegrationActionName.Update)
			{
				//http://tscore-task/browse/SKT-3478. Зачищаем чтобы синхронизация с деталью "Средства связи" не отработала
				integrationInfo.IntegratedEntity.ClearAllAddressField();
			}
		}
		public override void AfterEntitySave(IntegrationInfo integrationInfo)
		{
			base.AfterEntitySave(integrationInfo);
			ContactEntityHelper.ResaveContactBirthday(integrationInfo.UserConnection, integrationInfo.IntegratedEntity, IntegrationLogger.SimpleLoggerErrorAction);
			//ContactEntityHelper.ClearContactPrimaryCommunication(integrationInfo.UserConnection, integrationInfo.IntegratedEntity, IntegrationLogger.SimpleLoggerErrorAction);
			ContactEntityHelper.ResaveContactPrimaryAddress(integrationInfo.UserConnection, integrationInfo.IntegratedEntity, integrationInfo.IntegratedEntity.PrimaryColumnValue, IntegrationLogger.SimpleLoggerErrorAction);
			//http://tscore-task/browse/SKT-4696
			ContactEntityHelper.SynchronizeCommunication(integrationInfo.UserConnection, integrationInfo.IntegratedEntity.PrimaryColumnValue, IntegrationLogger.SimpleLoggerErrorAction);
		}
		public override bool IsAdvancedSearch {
			get {
				return true;
			}
		}
		public override AdvancedSearchInfo AdvancedSearchInfo {
			get {
				return _advancedSearchInfo;
			}
		}
		public override void AddParameterToSearchProcedure(IntegrationInfo integrationInfo, StoredProcedure searchProcedure)
		{
			try {
				Tuple<string, string> emailPhoneTuple = new Tuple<string, string>(string.Empty, string.Empty);
				if (integrationInfo != null && integrationInfo.Data != null)
				{
					emailPhoneTuple = GetEmailAndPhones(JName, integrationInfo.Data);
				}
				searchProcedure
					.WithParameter("Emails", emailPhoneTuple.Item1 ?? string.Empty)
					.WithParameter("Phones", emailPhoneTuple.Item2 ?? string.Empty);
			} catch(Exception e)
			{
				IntegrationLogger.Error(e);
			}
		}
		public static Tuple<string, string> GetEmailAndPhones(string jName, JObject jObj)
		{
			string emails = String.Empty;
			string phones = String.Empty;
			try
			{
				var communication = jObj.SelectToken(jName + ".contact.ContactInfo.records");
				if (communication != null && communication is JArray)
				{
					var jArrayCommunication = (JArray)communication;
					if (jArrayCommunication.Count() > 0)
					{
						emails = string.Join(",", jArrayCommunication.Where(x =>
						{
							var emailToken = x.SelectToken("ContactRecord.type.#ref.id");
							return emailToken != null && emailToken.Value<int>() == 1;
						})
						.Select(x => x.SelectToken("ContactRecord.value").Value<string>()));
						phones = string.Join(",", jArrayCommunication.Where(x =>
						{
							var emailToken = x.SelectToken("ContactRecord.type.#ref.id");
							return emailToken != null && emailToken.Value<int>() == 2;
						})
						.Select(x => x.SelectToken("ContactRecord.value").Value<string>()));
					}
				}
			} catch(Exception e)
			{
				IntegrationLogger.Error(e);
			}
			return new Tuple<string, string>(emails, phones);
		}
	}

	#endregion


	#region Class: TsAutoOwnerInfoHandler
	/*
		Project Path: ..\..\..\QueryConsole\Files\TsIntegrationServiceEntity.TsBase.cs
		
	*/

	[ImportHandlerAttribute("VehicleRelationship")]
	[ExportHandlerAttribute("TsAutoOwnerInfo")]
	[ExportHandlerAttribute("TsAutoOwnerHistory")]
	[ExportHandlerAttribute("TsAutoTechService")]
	[ExportHandlerAttribute("TsAutoTechHistory")]
	public class TsAutoOwnerInfoHandler : EntityHandler
	{
		public TsAutoOwnerInfoHandler()
		{
			Mapper = new MappingHelper();
			EntityName = "";
			JName = "VehicleRelationship";
		}
		string handlerName = "TsAutoOwnerInfo";
		public override string HandlerName {
			get {
				return handlerName;
			}
		}
		public override bool IsEntityAlreadyExist(IntegrationInfo integrationInfo)
		{
			if (integrationInfo.IntegrationType == CsConstant.TIntegrationType.Import)
			{
				var typeToken = integrationInfo.Data.SelectToken(JName + ".type.#ref.id");
				var activeToken = integrationInfo.Data.SelectToken(JName + ".active");
				if (typeToken != null && activeToken != null)
				{
					EntityName = GetEntityNameByTypeId(typeToken.Value<int>(), activeToken.Value<bool>());
					Mapper.UserConnection = integrationInfo.UserConnection;
				}
			}
			return base.IsEntityAlreadyExist(integrationInfo);
		}
		public string GetEntityNameByTypeId(int typeId, bool active)
		{
			if (typeId != 4)
			{
				handlerName = active ? "TsAutoOwnerInfo" : "TsAutoOwnerHistory";
				return handlerName;
			}
			handlerName = active ? "TsAutoTechService" : "TsAutoTechHistory";
			return handlerName;
		}

		public override void BeforeMapping(IntegrationInfo integrationInfo)
		{
			if (integrationInfo.IntegrationType == CsConstant.TIntegrationType.Export)
			{
				EntityName = integrationInfo.IntegratedEntity.GetType().Name;
				handlerName = EntityName;
			}
		}
		public override void ProcessResponse(IntegrationInfo integrationInfo)
		{
			base.ProcessResponse(integrationInfo);
			if (EntityName == "TsAutoTechService" || EntityName == "TsAutoOwnerInfo")
			{
				var desEntityName = EntityName == "TsAutoTechService" ? "TsAutoTechHistory" : "TsAutoOwnerHistory";
				var autoId = integrationInfo.IntegratedEntity.GetTypedColumnValue<Guid>("TsAutomobileId");
				var esq = new EntitySchemaQuery(integrationInfo.UserConnection.EntitySchemaManager, desEntityName);
				esq.AddAllSchemaColumns();
				esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, "TsAutomobile", autoId));
				esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, "TsExternalId", 0));
				var entities = esq.GetEntityCollection(integrationInfo.UserConnection);
				var integrator = new ClientServiceIntegrator(integrationInfo.UserConnection);
				foreach (var entity in entities)
				{
					integrator.IntegrateBpmEntity(entity);
				}
			}
		}
		public override bool IsExport(IntegrationInfo integrationInfo)
		{
			return false;
		}
		public void SyncAutomobileOwners(IntegrationInfo integrationInfo)
		{
			try
			{
				if (EntityName != "TsAutoOwnerInfo")
				{
					return;
				}
				var automobileId = integrationInfo.IntegratedEntity.GetTypedColumnValue<Guid>("TsAutomobileId");
				if (automobileId != Guid.Empty)
				{
					var storedProcedure = new StoredProcedure(integrationInfo.UserConnection, "tsp_Integration_SynchronizeAutomobileOwners")
						.WithParameter(Column.Parameter(automobileId)) as StoredProcedure;
					storedProcedure.PackageName = integrationInfo.UserConnection.DBEngine.SystemPackageName;
					storedProcedure.Execute();
				}
			}
			catch (Exception e)
			{
				IntegrationLogger.SimpleLoggerErrorAction(e);
			}
		}
		public override void AfterEntitySave(IntegrationInfo integrationInfo)
		{
			base.AfterEntitySave(integrationInfo);
			SyncAutomobileOwners(integrationInfo);
		}
	}

	#endregion


	#region Class: RelationshipHandler
	/*
		Project Path: ..\..\..\QueryConsole\Files\TsIntegrationServiceEntity.TsBase.cs
		
	*/

	[ImportHandlerAttribute("Relationship")]
	[ExportHandlerAttribute("Relationship")]
	public class RelationshipHandler : EntityHandler
	{
		public RelationshipHandler()
		{
			Mapper = new MappingHelper();
			EntityName = "Relationship";
			JName = "Relationship";
		}
	}

	#endregion


	#region Class: TsContactNotificationsHandler
	/*
		Project Path: ..\..\..\QueryConsole\Files\TsIntegrationServiceEntity.TsBase.cs
		
	*/

	[ImportHandlerAttribute("NotificationProfileContact")]
	[ExportHandlerAttribute("NotificationProfileContact")]
	public class TsContactNotificationsHandler : EntityHandler
	{
		public override bool IsEmbeddedObject {
			get {
				return true;
			}
		}
		public override string ParentObjectJName {
			get {
				return "PersonProfile";
			}
		}
		public override string ParentObjectTsName {
			get {
				return "Contact";
			}
		}
		public TsContactNotificationsHandler()
		{
			Mapper = new MappingHelper();
			EntityName = "TsContactNotifications";
			JName = "NotificationProfile";
		}
		public override string SettingName {
			get {
				return "NotificationProfileContact";
			}
		}

		public override void BeforeMapping(IntegrationInfo integrationInfo)
		{
			if (integrationInfo.IntegrationType == CsConstant.TIntegrationType.Import)
			{
				if (integrationInfo.ParentEntity != null)
				{
					integrationInfo.Data[JName]["parentContactId"] = JToken.Parse(integrationInfo.ParentEntity.GetTypedColumnValue<int>("TsExternalId").ToString());
				}
			}
		}

		public override Entity GetEntityByExternalId(IntegrationInfo integrationInfo)
		{
			if (integrationInfo.ParentEntity != null)
			{
				integrationInfo.Data[JName]["parentContactId"] = JToken.Parse(integrationInfo.ParentEntity.GetTypedColumnValue<int>("TsExternalId").ToString());
				string externalIdPath = integrationInfo.TsExternalIdPath;
				var esq = new EntitySchemaQuery(integrationInfo.UserConnection.EntitySchemaManager, EntityName);
				esq.AddAllSchemaColumns();
				var columnExt = esq.AddColumn("TsExternalId");
				columnExt.OrderByDesc();
				esq.RowCount = 1;
				esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, "TsContact.TsExternalId", integrationInfo.Data[JName].Value<int>("parentContactId")));
				var group = new EntitySchemaQueryFilterCollection(esq, LogicalOperationStrict.Or) {
					esq.CreateFilterWithParameters(FilterComparisonType.Equal, externalIdPath, integrationInfo.Data[JName].Value<int>("id")),
					esq.CreateFilterWithParameters(FilterComparisonType.Equal, externalIdPath, 0)
				};
				esq.Filters.Add(group);
				return esq.GetEntityCollection(integrationInfo.UserConnection).FirstOrDefault();
			}
			return base.GetEntityByExternalId(integrationInfo);
		}
		public override bool IsEntityAlreadyExist(IntegrationInfo integrationInfo)
		{
			if (integrationInfo.ParentEntity != null)
			{
				integrationInfo.Data[JName]["parentContactId"] = JToken.Parse(integrationInfo.ParentEntity.GetTypedColumnValue<int>("TsExternalId").ToString());
				string externalIdPath = ExternalIdPath;
				var esq = new EntitySchemaQuery(integrationInfo.UserConnection.EntitySchemaManager, EntityName);
				var columnExt = esq.AddColumn("TsExternalId");
				columnExt.OrderByDesc();
				esq.RowCount = 1;
				esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, "TsContact.TsExternalId", integrationInfo.Data[JName].Value<int>("parentContactId")));
				var group = new EntitySchemaQueryFilterCollection(esq, LogicalOperationStrict.Or) {
					esq.CreateFilterWithParameters(FilterComparisonType.Equal, externalIdPath, integrationInfo.Data[JName].Value<int>("id")),
					esq.CreateFilterWithParameters(FilterComparisonType.Equal, externalIdPath, 0)
				};
				esq.Filters.Add(group);
				return esq.GetEntityCollection(integrationInfo.UserConnection).Count > 0;
			}
			return base.IsEntityAlreadyExist(integrationInfo);
		}

		public override bool IsVersionHigger(IntegrationInfo integrationInfo)
		{
			return true;
		}
	}

	#endregion


	#region Class: TsAccountNotificationHandler
	/*
		Project Path: ..\..\..\QueryConsole\Files\TsIntegrationServiceEntity.TsBase.cs
		
	*/

	[ImportHandlerAttribute("NotificationProfileAccount")]
	[ExportHandlerAttribute("NotificationProfileAccount")]
	public class TsAccountNotificationHandler : EntityHandler
	{
		public override bool IsEmbeddedObject {
			get {
				return true;
			}
		}
		public override string ParentObjectJName {
			get {
				return "CompanyProfile";
			}
		}
		public override string ParentObjectTsName {
			get {
				return "Account";
			}
		}
		public TsAccountNotificationHandler()
		{
			Mapper = new MappingHelper();
			EntityName = "TsAccountNotification";
			JName = "NotificationProfile";
		}
		public override string SettingName {
			get {
				return "NotificationProfileAccount";
			}
		}
		public override void BeforeMapping(IntegrationInfo integrationInfo)
		{
			if (integrationInfo.IntegrationType == CsConstant.TIntegrationType.Import)
			{
				if (integrationInfo.ParentEntity != null)
				{
					integrationInfo.Data[JName]["parentId"] = JToken.FromObject(integrationInfo.ParentEntity.GetTypedColumnValue<Guid>("Id").ToString());
				}
			}
		}

		public override Entity GetEntityByExternalId(IntegrationInfo integrationInfo)
		{
			if (integrationInfo.ParentEntity != null)
			{
				integrationInfo.Data[JName]["parentId"] = JToken.FromObject(integrationInfo.ParentEntity.GetTypedColumnValue<Guid>("Id").ToString());
				string externalIdPath = integrationInfo.TsExternalIdPath;
				var esq = new EntitySchemaQuery(integrationInfo.UserConnection.EntitySchemaManager, EntityName);
				esq.AddAllSchemaColumns();
				var columnExt = esq.AddColumn("TsExternalId");
				columnExt.OrderByDesc();
				esq.RowCount = 1;
				esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, "TsAccount", integrationInfo.Data[JName].Value<string>("parentId")));
				var group = new EntitySchemaQueryFilterCollection(esq, LogicalOperationStrict.Or) {
					esq.CreateFilterWithParameters(FilterComparisonType.Equal, externalIdPath, integrationInfo.Data[JName].Value<int>("id")),
					esq.CreateFilterWithParameters(FilterComparisonType.Equal, externalIdPath, 0)
				};
				esq.Filters.Add(group);
				return esq.GetEntityCollection(integrationInfo.UserConnection).FirstOrDefault();
			}
			return base.GetEntityByExternalId(integrationInfo);
		}
		public override bool IsEntityAlreadyExist(IntegrationInfo integrationInfo)
		{
			if (integrationInfo.ParentEntity != null)
			{
				integrationInfo.Data[JName]["parentId"] = JToken.FromObject(integrationInfo.ParentEntity.GetTypedColumnValue<Guid>("Id").ToString());
				string externalIdPath = ExternalIdPath;
				var esq = new EntitySchemaQuery(integrationInfo.UserConnection.EntitySchemaManager, EntityName);
				var columnExt = esq.AddColumn("TsExternalId");
				columnExt.OrderByDesc();
				esq.RowCount = 1;
				esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, "TsAccount", integrationInfo.Data[JName].Value<string>("parentId")));
				var group = new EntitySchemaQueryFilterCollection(esq, LogicalOperationStrict.Or) {
					esq.CreateFilterWithParameters(FilterComparisonType.Equal, externalIdPath, integrationInfo.Data[JName].Value<int>("id")),
					esq.CreateFilterWithParameters(FilterComparisonType.Equal, externalIdPath, 0)
				};
				esq.Filters.Add(group);
				return esq.GetEntityCollection(integrationInfo.UserConnection).Count > 0;
			}
			return base.IsEntityAlreadyExist(integrationInfo);
		}

		public override bool IsVersionHigger(IntegrationInfo integrationInfo)
		{
			return true;
		}
	}

	#endregion


	#region Class: SysAdminUnitHandler
	/*
		Project Path: ..\..\..\QueryConsole\Files\TsIntegrationServiceEntity.TsBase.cs
		
	*/

	[ImportHandlerAttribute("Manager")]
	[ImportHandlerAttribute("ManagerGroup")]
	[ExportHandlerAttribute("SysAdminUnit")]
	public class SysAdminUnitHandler : EntityHandler
	{
		public ServiceUrlMaker UrlMaker;
		public override string HandlerName {
			get {
				return JName;
			}
		}
		public SysAdminUnitHandler()
		{
			Mapper = new MappingHelper();
			EntityName = "SysAdminUnit";
			JName = "";
			UrlMaker = new ServiceUrlMaker(CsConstant.IntegratorSettings.Settings[typeof(ClientServiceIntegrator)].BaseUrl);
		}

		public override void BeforeMapping(IntegrationInfo integrationInfo)
		{
			base.BeforeMapping(integrationInfo);
			if (integrationInfo.IntegrationType == CsConstant.TIntegrationType.Import)
			{
				if (JName == "Manager")
				{
					integrationInfo.IntegratedEntity.SetColumnValue("SysAdminUnitTypeValue", CsConstant.TSysAdminUnitType.User);
				}
				else
				{
					integrationInfo.IntegratedEntity.SetColumnValue("SysAdminUnitTypeValue", CsConstant.TSysAdminUnitType.Unit);
				}
			} else
			{
				var typeIndex = integrationInfo.IntegratedEntity.GetTypedColumnValue<int>("SysAdminUnitTypeValue");
				if (typeIndex < 4)
				{
					JName = "ManagerGroup";
				}
				else
				{
					JName = "Manager";
				}
			}
		}

		public override bool IsEntityAlreadyExist(IntegrationInfo integrationInfo)
		{
			Mapper.UserConnection = integrationInfo.UserConnection;
			int externalId = 0;
			if (integrationInfo.IntegratedEntity != null)
			{
				externalId = integrationInfo.IntegratedEntity.GetTypedColumnValue<int>("TsExternalId");
			}
			return Mapper.CheckIsExist("SysAdminUnit", integrationInfo.Data[JName].Value<int>("id"), "TsExternalId", externalId);
		}

		public override void ProcessResponse(IntegrationInfo integrationInfo)
		{
			base.ProcessResponse(integrationInfo);
			if (JName == "Manager" && integrationInfo.IntegratedEntity.GetTypedColumnValue<int>("SysAdminUnitTypeValue") == (int)CsConstant.TSysAdminUnitType.User)
			{
				try
				{
					ResaveContact(integrationInfo.IntegratedEntity.GetTypedColumnValue<Guid>("ContactId"), integrationInfo.UserConnection);
				}
				catch (Exception e)
				{
					IntegrationLogger.Error(e, "SysAdminUnitHandler.ProcessResponse");
				}
			}
		}

		public void ResaveContact(Guid contactId, UserConnection userConnection)
		{
			if (contactId != Guid.Empty)
			{
				var integrator = new ClientServiceIntegrator(userConnection);
				integrator.IntegrateBpmEntity(contactId, "Contact");
			}
		}

		public override bool IsVersionHigger(IntegrationInfo integrationInfo)
		{
			return true;
		}
		public override void AfterEntitySave(IntegrationInfo integrationInfo)
		{
			base.AfterEntitySave(integrationInfo);
			if (integrationInfo.IntegratedEntity != null && JName == "Manager" && integrationInfo.IntegratedEntity.GetTypedColumnValue<int>("SysAdminUnitTypeValue") == (int)CsConstant.TSysAdminUnitType.User) {
				ResaveContact(integrationInfo.IntegratedEntity.PrimaryColumnValue, integrationInfo.UserConnection);
			}
		}
	}

	#endregion


	#region Class: TsAccountManagerGroupHandler
	/*
		Project Path: ..\..\..\QueryConsole\Files\TsIntegrationServiceEntity.TsBase.cs
		
	*/

	[ImportHandlerAttribute("CompanyProfileAssignment")]
	[ExportHandlerAttribute("TsAccountManagerGroup")]
	public class TsAccountManagerGroupHandler : EntityHandler
	{
		public override bool IsEmbeddedObject {
			get {
				return true;
			}
		}
		public override string ParentObjectJName {
			get {
				return "CompanyProfile";
			}
		}
		public override string ParentObjectTsName {
			get {
				return "Account";
			}
		}
		public TsAccountManagerGroupHandler()
		{
			Mapper = new MappingHelper();
			EntityName = "TsAccountManagerGroup";
			JName = "CompanyProfileAssignment";
		}
	}

	#endregion


	#region Class: CaseHandler
	/*
		Project Path: ..\..\..\QueryConsole\Files\TsIntegrationServiceEntity.TsBase.cs
		
	*/

	[ImportHandlerAttribute("ClientRequest")]
	[ExportHandlerAttribute("Case")]
	public class CaseHandler : EntityHandler
	{
		public CaseHandler()
		{
			Mapper = new MappingHelper();
			EntityName = "Case";
			JName = "ClientRequest";
		}
	}

	#endregion


	#region Class: TsAutomobileHandler
	/*
		Project Path: ..\..\..\QueryConsole\Files\TsIntegrationServiceEntity.TsBase.cs
		
	*/

	[ImportHandlerAttribute("VehicleProfile")]
	[ExportHandlerAttribute("TsAutomobile")]
	public class TsAutomobileHandler : EntityHandler
	{
		public TsAutomobileHandler()
		{
			Mapper = new MappingHelper();
			EntityName = "TsAutomobile";
			JName = "VehicleProfile";
		}

		public override void ProcessResponse(IntegrationInfo integrationInfo)
		{
			base.ProcessResponse(integrationInfo);
			integratePassport(integrationInfo);
		}

		public override void AfterEntitySave(IntegrationInfo integrationInfo)
		{
			base.AfterEntitySave(integrationInfo);
			try
			{
				if (integrationInfo != null && integrationInfo.IntegratedEntity != null && integrationInfo.Data != null)
				{
					var automobile = integrationInfo.IntegratedEntity;
					var name = automobile.GetTypedColumnValue<string>("TsName");
					if (string.IsNullOrEmpty(name))
					{
						var mark = integrationInfo.Data.SelectToken("VehicleProfile.manufacturerName").Value<string>();
						var model = integrationInfo.Data.SelectToken("VehicleProfile.modelName").Value<string>();
						automobile.SetColumnValue("TsName", string.Concat(mark, " ", model));
						automobile.UpdateInDB(false);
					}
				}
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e);
			}
		}

		public void integratePassport(IntegrationInfo integrationInfo)
		{
			var automobileId = integrationInfo.IntegratedEntity.GetTypedColumnValue<Guid>("Id");
			if (automobileId != Guid.Empty)
			{
				var helper = new ClientServiceIntegrator(integrationInfo.UserConnection);
				helper.IntegrateBpmEntity(integrationInfo.IntegratedEntity, new VehiclePassportHandler(), false);
			}
		}
		public override bool IsExport(IntegrationInfo integrationInfo)
		{
			return false;
		}
	}

	#endregion


	#region Class: VehiclePassportHandler
	/*
		Project Path: ..\..\..\QueryConsole\Files\TsIntegrationServiceEntity.TsBase.cs
		
	*/

	[ImportHandlerAttribute("VehiclePassport")]
	[ExportHandlerAttribute("VehiclePassport")]
	public class VehiclePassportHandler : EntityHandler
	{
		public VehiclePassportHandler()
		{
			Mapper = new MappingHelper();
			EntityName = "TsAutomobile";
			JName = "VehiclePassport";
		}

		public override string ExternalIdPath {
			get {
				return "TsPassportExternalId";
			}
		}
		public override string HandlerName {
			get {
				return JName;
			}
		}
		public override void ProcessResponse(IntegrationInfo integrationInfo)
		{
			base.ProcessResponse(integrationInfo);
			try
			{
				if (integrationInfo.Action == CsConstant.IntegrationActionName.Create)
				{
					var integrator = new ClientServiceIntegrator(integrationInfo.UserConnection);
					integrator.IntegrateBpmEntity(integrationInfo.IntegratedEntity, new TsAutomobileHandler(), false);
				}
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e, "VehiclePassportHandler - ProcessResponse");
			}
		}
		public override bool IsVersionHigger(IntegrationInfo integrationInfo)
		{
			return true;
		}
		public override bool IsExport(IntegrationInfo integrationInfo)
		{
			return false;
		}

		public override void Create(IntegrationInfo integrationInfo)
		{
			//Не создаем паспорт если еще нету автомоболя к которому можно его привязать
			return;
		}
	}

	#endregion


	#region Class: ContactInfoHandler
	/*
		Project Path: ..\..\..\QueryConsole\Files\TsIntegrationServiceEntity.TsBase.cs
		
	*/

	[ImportHandlerAttribute("ContactInfo")]
	[ExportHandlerAttribute("ContactInfo")]
	public class ContactInfoHandler : EntityHandler
	{
		public override bool IsEmbeddedObject {
			get {
				return true;
			}
		}
		public override string ParentObjectJName {
			get {
				return "PersonProfile";
			}
		}
		public override string ParentObjectTsName {
			get {
				return "Contact";
			}
		}
		public ContactInfoHandler()
		{
			Mapper = new MappingHelper();
			EntityName = "Contact";
			JName = "ContactInfo";
		}

		public override string HandlerName {
			get {
				return JName;
			}
		}
	}

	#endregion


	#region Class: AddressInfoHandler
	/*
		Project Path: ..\..\..\QueryConsole\Files\TsIntegrationServiceEntity.TsBase.cs
		
	*/

	[ImportHandlerAttribute("ContactAddress")]
	[ExportHandlerAttribute("ContactAddress")]
	public class AddressInfoHandler : EntityHandler
	{
		public override bool IsEmbeddedObject {
			get {
				return true;
			}
		}
		public override string ParentObjectJName {
			get {
				return "PersonProfile";
			}
		}
		public override string ParentObjectTsName {
			get {
				return "Contact";
			}
		}
		public AddressInfoHandler()
		{
			Mapper = new MappingHelper();
			EntityName = "ContactAddress";
			JName = "AddressInfo";
		}

		public override string HandlerName {
			get {
				return "ContactAddress";
			}
		}

		public override void BeforeMapping(IntegrationInfo integrationInfo)
		{
			if (integrationInfo.IntegrationType == CsConstant.TIntegrationType.Import)
			{
				if (integrationInfo.ParentEntity != null)
				{
					integrationInfo.Data[JName]["parentContactId"] = JToken.Parse(integrationInfo.ParentEntity.GetTypedColumnValue<int>("TsExternalId").ToString());
				}
			}
		}

		public override Entity GetEntityByExternalId(IntegrationInfo integrationInfo)
		{
			if (integrationInfo.ParentEntity != null)
			{
				integrationInfo.Data[JName]["parentContactId"] = JToken.Parse(integrationInfo.ParentEntity.GetTypedColumnValue<int>("TsExternalId").ToString());
				string externalIdPath = integrationInfo.TsExternalIdPath;
				var esq = new EntitySchemaQuery(integrationInfo.UserConnection.EntitySchemaManager, EntityName);
				esq.AddAllSchemaColumns();
				var columnExt = esq.AddColumn("TsExternalId");
				columnExt.OrderByDesc();
				esq.RowCount = 1;
				esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, "Contact.TsExternalId", integrationInfo.Data[JName].Value<int>("parentContactId")));
				EntitySchemaQueryFilterCollection group;
				if (integrationInfo.Action == CsConstant.IntegrationActionName.UpdateFromResponse)
				{
					group = new EntitySchemaQueryFilterCollection(esq, LogicalOperationStrict.And) {
						esq.CreateFilterWithParameters(FilterComparisonType.Equal, externalIdPath, integrationInfo.Data[JName].Value<int>("id")),
					};
				} else
				{
					group = new EntitySchemaQueryFilterCollection(esq, LogicalOperationStrict.Or) {
						esq.CreateFilterWithParameters(FilterComparisonType.Equal, externalIdPath, integrationInfo.Data[JName].Value<int>("id")),
						esq.CreateFilterWithParameters(FilterComparisonType.Equal, externalIdPath, 0)
					};
				}
				esq.Filters.Add(group);
				return esq.GetEntityCollection(integrationInfo.UserConnection).FirstOrDefault();
			}
			return base.GetEntityByExternalId(integrationInfo);
		}
		public override bool IsEntityAlreadyExist(IntegrationInfo integrationInfo)
		{
			if (integrationInfo.ParentEntity != null)
			{
				integrationInfo.Data[JName]["parentContactId"] = JToken.Parse(integrationInfo.ParentEntity.GetTypedColumnValue<int>("TsExternalId").ToString());
				string externalIdPath = ExternalIdPath;
				var esq = new EntitySchemaQuery(integrationInfo.UserConnection.EntitySchemaManager, EntityName);
				var columnExt = esq.AddColumn("TsExternalId");
				columnExt.OrderByDesc();
				esq.RowCount = 1;
				esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, "Contact.TsExternalId", integrationInfo.Data[JName].Value<int>("parentContactId")));
				EntitySchemaQueryFilterCollection group;
				if (integrationInfo.Action == CsConstant.IntegrationActionName.UpdateFromResponse)
				{
					group = new EntitySchemaQueryFilterCollection(esq, LogicalOperationStrict.And) {
						esq.CreateFilterWithParameters(FilterComparisonType.Equal, externalIdPath, integrationInfo.Data[JName].Value<int>("id")),
					};
				}
				else
				{
					group = new EntitySchemaQueryFilterCollection(esq, LogicalOperationStrict.Or) {
						esq.CreateFilterWithParameters(FilterComparisonType.Equal, externalIdPath, integrationInfo.Data[JName].Value<int>("id")),
						esq.CreateFilterWithParameters(FilterComparisonType.Equal, externalIdPath, 0)
					};
				}
				esq.Filters.Add(group);
				return esq.GetEntityCollection(integrationInfo.UserConnection).Count > 0;
			}
			return base.IsEntityAlreadyExist(integrationInfo);
		}
		public override void AfterEntitySave(IntegrationInfo integrationInfo)
		{
			base.AfterEntitySave(integrationInfo);
			if (integrationInfo.IntegrationType == CsConstant.TIntegrationType.Import)
			{
				UpdateAddressFromDeliveryService(integrationInfo);
			}
		}
		public void UpdateAddressFromDeliveryService(IntegrationInfo integrationInfo)
		{
			try
			{
				AddressHelper.UpdateAddressFromDeliveryService(integrationInfo.UserConnection, integrationInfo.IntegratedEntity, IntegrationLogger.SimpleLoggerErrorAction);
				ContactEntityHelper.ResaveContactPrimaryAddress(integrationInfo.UserConnection, integrationInfo.ParentEntity, integrationInfo.ParentEntity.PrimaryColumnValue, IntegrationLogger.SimpleLoggerErrorAction);
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e);
			}
		}

		public override bool IsVersionHigger(IntegrationInfo integrationInfo)
		{
			return true;
		}
	}

	#endregion


	#region Class: AddressInfoAccountHandler
	/*
		Project Path: ..\..\..\QueryConsole\Files\TsIntegrationServiceEntity.TsBase.cs
		
	*/


	[ImportHandlerAttribute("AddressInfoAccount")]
	[ExportHandlerAttribute("AddressInfoAccount")]
	public class AddressInfoAccountHandler : EntityHandler
	{
		public override bool IsEmbeddedObject {
			get {
				return true;
			}
		}
		public override string ParentObjectJName {
			get {
				return "CompanyProfile";
			}
		}
		public override string ParentObjectTsName {
			get {
				return "Account";
			}
		}
		public AddressInfoAccountHandler()
		{
			Mapper = new MappingHelper();
			EntityName = "AccountAddress";
			JName = "AddressInfo";
		}

		public override string HandlerName {
			get {
				return "AddressInfoAccount";
			}
		}

		public override void BeforeMapping(IntegrationInfo integrationInfo)
		{
			if (integrationInfo.IntegrationType == CsConstant.TIntegrationType.Import)
			{
				if (integrationInfo.ParentEntity != null)
				{
					integrationInfo.Data[JName]["parentAccountId"] = JToken.Parse(integrationInfo.ParentEntity.GetTypedColumnValue<int>("TsExternalId").ToString());
				}
			}
		}

		public override Entity GetEntityByExternalId(IntegrationInfo integrationInfo)
		{
			if (integrationInfo.ParentEntity != null)
			{
				integrationInfo.Data[JName]["parentAccountId"] = JToken.Parse(integrationInfo.ParentEntity.GetTypedColumnValue<int>("TsExternalId").ToString());
				string externalIdPath = integrationInfo.TsExternalIdPath;
				var esq = new EntitySchemaQuery(integrationInfo.UserConnection.EntitySchemaManager, EntityName);
				esq.AddAllSchemaColumns();
				var columnExt = esq.AddColumn("TsExternalId");
				columnExt.OrderByDesc();
				esq.RowCount = 1;
				esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, "Account.TsExternalId", integrationInfo.Data[JName].Value<int>("parentAccountId")));
				EntitySchemaQueryFilterCollection group;
				if (integrationInfo.Action == CsConstant.IntegrationActionName.UpdateFromResponse)
				{
					group = new EntitySchemaQueryFilterCollection(esq, LogicalOperationStrict.And) {
						esq.CreateFilterWithParameters(FilterComparisonType.Equal, externalIdPath, integrationInfo.Data[JName].Value<int>("id")),
					};
				} else
				{
					group = new EntitySchemaQueryFilterCollection(esq, LogicalOperationStrict.Or) {
						esq.CreateFilterWithParameters(FilterComparisonType.Equal, externalIdPath, integrationInfo.Data[JName].Value<int>("id")),
						esq.CreateFilterWithParameters(FilterComparisonType.Equal, externalIdPath, 0)
					};
				}
				esq.Filters.Add(group);
				return esq.GetEntityCollection(integrationInfo.UserConnection).FirstOrDefault();
			}
			return base.GetEntityByExternalId(integrationInfo);
		}
		public override bool IsEntityAlreadyExist(IntegrationInfo integrationInfo)
		{
			if (integrationInfo.ParentEntity != null)
			{
				integrationInfo.Data[JName]["parentAccountId"] = JToken.Parse(integrationInfo.ParentEntity.GetTypedColumnValue<int>("TsExternalId").ToString());
				var esq = new EntitySchemaQuery(integrationInfo.UserConnection.EntitySchemaManager, EntityName);
				var columnExt = esq.AddColumn("TsExternalId");
				columnExt.OrderByDesc();
				esq.RowCount = 1;
				esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, "Account.TsExternalId", integrationInfo.Data[JName].Value<int>("parentAccountId")));
				EntitySchemaQueryFilterCollection group;
				if (integrationInfo.Action == CsConstant.IntegrationActionName.UpdateFromResponse)
				{
					group = new EntitySchemaQueryFilterCollection(esq, LogicalOperationStrict.And) {
						esq.CreateFilterWithParameters(FilterComparisonType.Equal, ExternalIdPath, integrationInfo.Data[JName].Value<int>("id")),
					};
				}
				else
				{
					group = new EntitySchemaQueryFilterCollection(esq, LogicalOperationStrict.Or) {
						esq.CreateFilterWithParameters(FilterComparisonType.Equal, ExternalIdPath, integrationInfo.Data[JName].Value<int>("id")),
						esq.CreateFilterWithParameters(FilterComparisonType.Equal, ExternalIdPath, 0)
					};
				}
				esq.Filters.Add(group);
				return esq.GetEntityCollection(integrationInfo.UserConnection).Count > 0;
			}
			return base.IsEntityAlreadyExist(integrationInfo);
		}
		public override void AfterEntitySave(IntegrationInfo integrationInfo)
		{
			base.AfterEntitySave(integrationInfo);
			if (integrationInfo.IntegrationType == CsConstant.TIntegrationType.Import)
			{
				UpdateAddressFromDeliveryService(integrationInfo);
				AccountEntityHelper.ResaveAccountPrimaryAddress(integrationInfo.UserConnection, integrationInfo.ParentEntity, integrationInfo.ParentEntity.PrimaryColumnValue, IntegrationLogger.SimpleLoggerErrorAction);
			}
		}
		public void UpdateAddressFromDeliveryService(IntegrationInfo integrationInfo)
		{
			try
			{
				AddressHelper.UpdateAddressFromDeliveryService(integrationInfo.UserConnection, integrationInfo.IntegratedEntity, IntegrationLogger.SimpleLoggerErrorAction);
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e);
			}
		}
		public override bool IsVersionHigger(IntegrationInfo integrationInfo)
		{
			return true;
		}
	}

	#endregion


	#region Class: ContactRecordHandler
	/*
		Project Path: ..\..\..\QueryConsole\Files\TsIntegrationServiceEntity.TsBase.cs
		
	*/

	[ImportHandlerAttribute("ContactCommunication")]
	[ExportHandlerAttribute("ContactCommunication")]
	public class ContactRecordHandler : EntityHandler
	{
		public override bool IsEmbeddedObject {
			get {
				return true;
			}
		}
		public override string ParentObjectJName {
			get {
				return "PersonProfile";
			}
		}
		public override string ParentObjectTsName {
			get {
				return "Contact";
			}
		}
		public ContactRecordHandler()
		{
			Mapper = new MappingHelper();
			EntityName = "ContactCommunication";
			JName = "ContactRecord";
		}

		public override string HandlerName {
			get {
				return "ContactCommunication";
			}
		}

		public override void AfterEntitySave(IntegrationInfo integrationInfo)
		{
			base.AfterEntitySave(integrationInfo);
			ContactEntityHelper.ClearContactPrimaryCommunication(integrationInfo.UserConnection, integrationInfo.ParentEntity, IntegrationLogger.SimpleLoggerErrorAction);
		}

		public override void BeforeMapping(IntegrationInfo integrationInfo)
		{
			if (integrationInfo.IntegrationType == CsConstant.TIntegrationType.Import)
			{
				if (integrationInfo.ParentEntity != null)
				{
					integrationInfo.Data[JName]["parentContactId"] = JToken.Parse(integrationInfo.ParentEntity.GetTypedColumnValue<int>("TsExternalId").ToString());
				}
				try
				{
					integrationInfo.Data[JName]["useInContact"] = JToken.FromObject(true);
				}
				catch (Exception e)
				{
					IntegrationLogger.Error(e);
				}
			}
		}

		public override Entity GetEntityByExternalId(IntegrationInfo integrationInfo)
		{
			if (integrationInfo.ParentEntity != null)
			{
				integrationInfo.Data[JName]["parentContactId"] = JToken.Parse(integrationInfo.ParentEntity.GetTypedColumnValue<int>("TsExternalId").ToString());
				string externalIdPath = integrationInfo.TsExternalIdPath;
				var esq = new EntitySchemaQuery(integrationInfo.UserConnection.EntitySchemaManager, EntityName);
				esq.AddAllSchemaColumns();
				var columnExt = esq.AddColumn("TsExternalId");
				columnExt.OrderByDesc();
				esq.RowCount = 1;
				esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, "Contact.TsExternalId", integrationInfo.Data[JName].Value<int>("parentContactId")));
				EntitySchemaQueryFilterCollection group;
				if (integrationInfo.Action == CsConstant.IntegrationActionName.UpdateFromResponse)
				{
					group = new EntitySchemaQueryFilterCollection(esq, LogicalOperationStrict.Or) {
						esq.CreateFilterWithParameters(FilterComparisonType.Equal, externalIdPath, integrationInfo.Data[JName].Value<int>("id"))
					};
				} else
				{
					group = new EntitySchemaQueryFilterCollection(esq, LogicalOperationStrict.Or) {
						esq.CreateFilterWithParameters(FilterComparisonType.Equal, externalIdPath, integrationInfo.Data[JName].Value<int>("id")),
						esq.CreateFilterWithParameters(FilterComparisonType.Equal, externalIdPath, 0)
					};
				}
				esq.Filters.Add(group);
				return esq.GetEntityCollection(integrationInfo.UserConnection).FirstOrDefault();
			}
			return base.GetEntityByExternalId(integrationInfo);
		}
		public override bool IsEntityAlreadyExist(IntegrationInfo integrationInfo)
		{
			if (integrationInfo.ParentEntity != null)
			{
				integrationInfo.Data[JName]["parentContactId"] = JToken.Parse(integrationInfo.ParentEntity.GetTypedColumnValue<int>("TsExternalId").ToString());
				string externalIdPath = ExternalIdPath;
				var esq = new EntitySchemaQuery(integrationInfo.UserConnection.EntitySchemaManager, EntityName);
				var columnExt = esq.AddColumn("TsExternalId");
				columnExt.OrderByDesc();
				esq.RowCount = 1;
				esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, "Contact.TsExternalId", integrationInfo.Data[JName].Value<int>("parentContactId")));
				EntitySchemaQueryFilterCollection group;
				if (integrationInfo.Action == CsConstant.IntegrationActionName.UpdateFromResponse)
				{
					group = new EntitySchemaQueryFilterCollection(esq, LogicalOperationStrict.Or) {
						esq.CreateFilterWithParameters(FilterComparisonType.Equal, externalIdPath, integrationInfo.Data[JName].Value<int>("id"))
					};
				}
				else
				{
					group = new EntitySchemaQueryFilterCollection(esq, LogicalOperationStrict.Or) {
						esq.CreateFilterWithParameters(FilterComparisonType.Equal, externalIdPath, integrationInfo.Data[JName].Value<int>("id")),
						esq.CreateFilterWithParameters(FilterComparisonType.Equal, externalIdPath, 0)
					};
				}
				esq.Filters.Add(group);
				return esq.GetEntityCollection(integrationInfo.UserConnection).Count > 0;
			}
			return base.IsEntityAlreadyExist(integrationInfo);
		}

		public override bool IsVersionHigger(IntegrationInfo integrationInfo)
		{
			return true;
		}
	}

	#endregion


	#region Class: AccountCommunicationHandler
	/*
		Project Path: ..\..\..\QueryConsole\Files\TsIntegrationServiceEntity.TsBase.cs
		
	*/

	[ImportHandlerAttribute("ContactRecordAccount")]
	[ExportHandlerAttribute("ContactRecordAccount")]
	public class AccountCommunicationHandler : EntityHandler
	{
		public override bool IsEmbeddedObject {
			get {
				return true;
			}
		}
		public override string ParentObjectJName {
			get {
				return "CompanyProfile";
			}
		}
		public override string ParentObjectTsName {
			get {
				return "Account";
			}
		}
		public AccountCommunicationHandler()
		{
			Mapper = new MappingHelper();
			EntityName = "AccountCommunication";
			JName = "ContactRecord";
		}

		public override string HandlerName {
			get {
				return EntityName;
			}
		}
		public override void BeforeMapping(IntegrationInfo integrationInfo)
		{
			if (integrationInfo.IntegrationType == CsConstant.TIntegrationType.Import)
			{
				if (integrationInfo.ParentEntity != null)
				{
					integrationInfo.Data[JName]["parentAccountId"] = JToken.Parse(integrationInfo.ParentEntity.GetTypedColumnValue<int>("TsExternalId").ToString());
				}
				try
				{
					integrationInfo.Data[JName]["useInAccount"] = JToken.FromObject(true);
				}
				catch (Exception e)
				{
					IntegrationLogger.Error(e);
				}
			}
		}
		public override Entity GetEntityByExternalId(IntegrationInfo integrationInfo)
		{
			if (integrationInfo.ParentEntity != null)
			{
				integrationInfo.Data[JName]["parentAccountId"] = JToken.Parse(integrationInfo.ParentEntity.GetTypedColumnValue<int>("TsExternalId").ToString());
				string externalIdPath = integrationInfo.TsExternalIdPath;
				var esq = new EntitySchemaQuery(integrationInfo.UserConnection.EntitySchemaManager, EntityName);
				esq.AddAllSchemaColumns();
				var columnExt = esq.AddColumn("TsExternalId");
				columnExt.OrderByDesc();
				esq.RowCount = 1;
				esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, "Account.TsExternalId", integrationInfo.Data[JName].Value<int>("parentAccountId")));
				EntitySchemaQueryFilterCollection group;
				if (integrationInfo.Action == CsConstant.IntegrationActionName.UpdateFromResponse)
				{
					group = new EntitySchemaQueryFilterCollection(esq, LogicalOperationStrict.And) {
						esq.CreateFilterWithParameters(FilterComparisonType.Equal, externalIdPath, integrationInfo.Data[JName].Value<int>("id"))
					};
				}
				else
				{
					group = new EntitySchemaQueryFilterCollection(esq, LogicalOperationStrict.Or) {
						esq.CreateFilterWithParameters(FilterComparisonType.Equal, externalIdPath, integrationInfo.Data[JName].Value<int>("id")),
						esq.CreateFilterWithParameters(FilterComparisonType.Equal, externalIdPath, 0)
					};
				}
				esq.Filters.Add(group);
				return esq.GetEntityCollection(integrationInfo.UserConnection).FirstOrDefault();
			}
			return base.GetEntityByExternalId(integrationInfo);
		}
		public override bool IsEntityAlreadyExist(IntegrationInfo integrationInfo)
		{
			if (integrationInfo.ParentEntity != null)
			{
				integrationInfo.Data[JName]["parentAccountId"] = JToken.Parse(integrationInfo.ParentEntity.GetTypedColumnValue<int>("TsExternalId").ToString());
				string externalIdPath = ExternalIdPath;
				var esq = new EntitySchemaQuery(integrationInfo.UserConnection.EntitySchemaManager, EntityName);
				var columnExt = esq.AddColumn("TsExternalId");
				columnExt.OrderByDesc();
				esq.RowCount = 1;
				esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, "Account.TsExternalId", integrationInfo.Data[JName].Value<int>("parentAccountId")));
				EntitySchemaQueryFilterCollection group;
				if (integrationInfo.Action == CsConstant.IntegrationActionName.UpdateFromResponse)
				{
					group = new EntitySchemaQueryFilterCollection(esq, LogicalOperationStrict.And) {
						esq.CreateFilterWithParameters(FilterComparisonType.Equal, externalIdPath, integrationInfo.Data[JName].Value<int>("id"))
					};
				}
				else
				{
					group = new EntitySchemaQueryFilterCollection(esq, LogicalOperationStrict.Or) {
						esq.CreateFilterWithParameters(FilterComparisonType.Equal, externalIdPath, integrationInfo.Data[JName].Value<int>("id")),
						esq.CreateFilterWithParameters(FilterComparisonType.Equal, externalIdPath, 0)
					};
				}
				esq.Filters.Add(group);
				return esq.GetEntityCollection(integrationInfo.UserConnection).Count > 0;
			}
			return base.IsEntityAlreadyExist(integrationInfo);
		}

		public override void AfterEntitySave(IntegrationInfo integrationInfo)
		{
			base.AfterEntitySave(integrationInfo);
			AccountEntityHelper.ClearAccountPrimaryCommunication(integrationInfo.UserConnection, integrationInfo.ParentEntity, IntegrationLogger.SimpleLoggerErrorAction);
		}

		public override bool IsVersionHigger(IntegrationInfo integrationInfo)
		{
			return true;
		}
	}

	#endregion


	#region Class: ContactCareerHandler
	/*
		Project Path: ..\..\..\QueryConsole\Files\TsIntegrationServiceEntity.TsBase.cs
		
	*/

	[ImportHandlerAttribute("Employee")]
	[ExportHandlerAttribute("ContactCareer")]
	public class ContactCareerHandler : EntityHandler
	{
		public ContactCareerHandler()
		{
			Mapper = new MappingHelper();
			EntityName = "ContactCareer";
			JName = "Employee";
		}
	}

	#endregion


	#region Class: TsLocSalMarketHandler
	/*
		Project Path: ..\..\..\QueryConsole\Files\TsIntegrationServiceEntity.TsBase.cs
		
	*/

	[ImportHandlerAttribute("Market")]
	[ExportHandlerAttribute("TsLocSalMarket")]
	public class TsLocSalMarketHandler : EntityHandler
	{
		public bool TypeIsLp = false;
		public static readonly Guid TypeLp = new Guid("f11e685a-060d-43cc-a221-26246317257d");
		public TsLocSalMarketHandler()
		{
			Mapper = new MappingHelper();
			EntityName = "TsLocSalMarket";
			JName = "Market";
		}
		//public override List<MappingItem> GetMapConfig(UserConnection userConnection)
		//{
		//	if (TypeIsLp)
		//	{
		//		return IntegrationConfigurationManager.GetConfigItem(userConnection, HandlerName, "lp");
		//	}
		//	return IntegrationConfigurationManager.GetConfigItem(userConnection, HandlerName, "gp");
		//}

		//public override void BeforeMapping(IntegrationInfo integrationInfo)
		//{
		//	if(integrationInfo.IntegratedEntity.GetTypedColumnValue<Guid>("TsMarketTypeId") == TypeLp) {
		//		TypeIsLp = true;
		//	}
		//}

		public override JObject ToJson(IntegrationInfo integrationInfo)
		{
			var result = base.ToJson(integrationInfo);
			var salesPerCapita = integrationInfo.IntegratedEntity.GetTypedColumnValue<double>("TsSalesForPersonInReg") + integrationInfo.IntegratedEntity.GetTypedColumnValue<double>("TsSalesForPersonInRegG");
			var capacity = integrationInfo.IntegratedEntity.GetTypedColumnValue<double>("TsMarketVolume") + integrationInfo.IntegratedEntity.GetTypedColumnValue<double>("TsMarketVolumeG");
			var population = Math.Max(integrationInfo.IntegratedEntity.GetTypedColumnValue<int>("TsPopulationCount"), integrationInfo.IntegratedEntity.GetTypedColumnValue<int>("TsPopulationCountG"));
			result[JName]["salesPerCapita"] = JToken.FromObject(salesPerCapita);
			result[JName]["capacity"] = JToken.FromObject(capacity);
			result[JName]["population"] = JToken.FromObject(population);

			//ValidateType(integrationInfo, result);
			return result;
		}
		public void ValidateType(IntegrationInfo integrationInfo, JObject jObj)
		{
			var type = jObj[JName]["type"].ToString();
			switch (type)
			{
				case "cargo":
					jObj[JName]["additionalInfo"]["auto"].Remove();
					break;
				case "auto":
					jObj[JName]["additionalInfo"]["cargo"].Remove();
					break;
			}
		}

		public override void AfterEntitySave(IntegrationInfo integrationInfo)
		{
			try
			{
				if (integrationInfo.IntegrationType == CsConstant.TIntegrationType.Import)
				{
					var type = integrationInfo.Data[JName]["type"].ToString();
					bool isLp = false;
					bool isGp = false;
					switch (type)
					{
						case "cargo":
							isGp = true;
							break;
						case "auto":
							isLp = true;
							break;
						case "cargo+auto":
						case "auto+cargo":
							isGp = true;
							isLp = true;
							break;
					}
					integrationInfo.IntegratedEntity.SetColumnValue("TsCargoProgram", isGp);
					integrationInfo.IntegratedEntity.SetColumnValue("TsPassengerProgram", isLp);
					integrationInfo.IntegratedEntity.UpdateInDB(false);
				}
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e, "AfterEntitySave");
			}
		}
	}

	#endregion


	#region Class: PaymentHandler
	/*
		Project Path: ..\..\..\QueryConsole\Files\TsIntegrationServiceEntity.TsBase.cs
		
	*/

	[ImportHandlerAttribute("Payment")]
	[ExportHandlerAttribute("TsPayment")]
	public class PaymentHandler : EntityHandler
	{
		public PaymentHandler()
		{
			Mapper = new MappingHelper();
			EntityName = "TsPayment";
			JName = "Payment";
		}
		public override void AfterEntitySave(IntegrationInfo integrationInfo)
		{
			try
			{
				var orderId = integrationInfo.IntegratedEntity.GetTypedColumnValue<Guid>("TsOrder");
				if (orderId != Guid.Empty)
				{
					UpdatePaymentSum(orderId, integrationInfo.UserConnection);
				}
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e, "PaymentHandler.AfterEntitySave");
			}
		}
		public void UpdatePaymentSum(Guid orderId, UserConnection userConnection)
		{
			var select = new Select(userConnection)
									.Top(1)
									.Column(Func.Sum("p", "TsAmount")).As("amount")
									.From("TsPaymentInOrder").As("pio")
									.InnerJoin("TsPayment").As("p").On("pio", "TsPaymentId").IsEqual("p", "Id")
									.Where("TsOrderId").IsEqual(Column.Parameter(orderId)) as Select;
			var update = new Update(userConnection, "Order")
									.Set("PaymentAmount", select)
									.Where("Id").IsEqual(Column.Parameter(orderId)) as Update;
			update.Execute();
		}
	}

	#endregion


	#region Class: OrderHandler
	/*
		Project Path: ..\..\..\QueryConsole\Files\TsIntegrationServiceEntity.TsBase.cs
		
	*/

	[ImportHandlerAttribute("Order")]
	[ExportHandlerAttribute("Order")]
	public class OrderHandler : EntityHandler
	{
		public OrderHandler()
		{
			Mapper = new MappingHelper();
			EntityName = "Order";
			JName = "Order";
		}
		public override void BeforeMapping(IntegrationInfo integrationInfo)
		{
			if (integrationInfo.IntegrationType == CsConstant.TIntegrationType.Import)
			{
				var name = integrationInfo.Data[JName]["createdByUser"].Value<string>();
				if (name != null && name.ToLower() == "shop")
				{
					integrationInfo.Data[JName]["createdByUser"] = "Supervisor";
				}
			}
		}

		public void importTransportationPointCompany(IntegrationInfo integrationInfo)
		{
			try
			{
				var transportationId = integrationInfo.Data.GetJTokenValuePath<int>(JName + ".shipmentInfo.ShipmentInfo.transportationPointId");
				var transportationName = integrationInfo.Data.GetJTokenValuePath<string>(JName + ".shipmentInfo.ShipmentInfo.transportationPointName");
				var transportationCompanyId = integrationInfo.Data.GetJTokenValuePath<int>(JName + ".shipmentInfo.ShipmentInfo.transportationCompanyId");
				var transportationCompanyName = integrationInfo.Data.GetJTokenValuePath<string>(JName + ".shipmentInfo.ShipmentInfo.transportationCompanyName");
				var isCurier = isCourierDeliveryMethod(integrationInfo);
				var shipmentPointId = GetTransportationIdAndCreateIfNotExist(transportationId, transportationName, integrationInfo.UserConnection);
				var shipmentCompanyId = GetTransportationCompanyIdAndCreateIfNotExist(transportationCompanyId, transportationCompanyName, integrationInfo.UserConnection);
				if (shipmentPointId != Guid.Empty)
				{
					integrationInfo.IntegratedEntity.SetColumnValue("TsShipmentPointId", shipmentPointId);
				}
				if (shipmentCompanyId != Guid.Empty)
				{
					integrationInfo.IntegratedEntity.SetColumnValue("TsTransportCompanyId", shipmentCompanyId);
				}
				integrationInfo.IntegratedEntity.SetColumnValue("TsIsCourierDelivery", isCurier);
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e, "importTransportationPointCompany " + integrationInfo.ToString());
				throw;
			}
		}
		private Guid GetTransportationIdAndCreateIfNotExist(int transId, string transName, UserConnection userConnection)
		{
			if (transId == 0 || string.IsNullOrEmpty(transName))
			{
				return Guid.Empty;
			}
			try
			{
				var select = new Select(userConnection)
								.Column("Id").As("Id")
								.From("TsShipmentPoint")
								.Where("TsExternalId").IsEqual(Column.Parameter(transId)) as Select;
				using (DBExecutor executor = select.UserConnection.EnsureDBConnection())
				{
					using (var reader = select.ExecuteReader(executor))
					{
						if (reader.Read())
						{
							return reader.GetColumnValue<Guid>("Id");
						}
					}
				}

				var resultId = Guid.NewGuid();
				var insert = new Insert(userConnection)
								.Into("TsShipmentPoint")
								.Set("Id", Column.Parameter(resultId))
								.Set("TsExternalId", Column.Parameter(transId))
								.Set("Name", Column.Parameter(transName));
				insert.Execute();
				return resultId;
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e, string.Format("{0} {1} {2}", "GetTransportationIdAndCreateIfNotExist", transId, transName));
			}
			return Guid.Empty;
		}

		private Guid GetTransportationCompanyIdAndCreateIfNotExist(int transId, string transName, UserConnection userConnection)
		{
			if (transId == 0 || string.IsNullOrEmpty(transName))
			{
				return Guid.Empty;
			}
			try
			{
				var select = new Select(userConnection)
								.Column("Id").As("Id")
								.From("TsTransportCompany")
								.Where("TsExternalId").IsEqual(Column.Parameter(transId)) as Select;
				using (DBExecutor executor = select.UserConnection.EnsureDBConnection())
				{
					using (var reader = select.ExecuteReader(executor))
					{
						if (reader.Read())
						{
							return reader.GetColumnValue<Guid>("Id");
						}
					}
				}

				var resultId = Guid.NewGuid();
				var insert = new Insert(userConnection)
								.Into("TsTransportCompany")
								.Set("Id", Column.Parameter(resultId))
								.Set("TsExternalId", Column.Parameter(transId))
								.Set("Name", Column.Parameter(transName));
				insert.Execute();
				return resultId;
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e, string.Format("{0} {1} {2}", "TsShipmentHandler.GetTransportationCompanyIdAndCreateIfNotExist", transId, transName));
			}
			return Guid.Empty;
		}
		public bool isCourierDeliveryMethod(IntegrationInfo integrationInfo)
		{
			try
			{
				var additionalInfo = integrationInfo.Data.GetJTokenValuePath<string>(JName + ".shipmentInfo.ShipmentInfo.additionalInfo");
				if (!string.IsNullOrEmpty(additionalInfo))
				{
					var jObject = JObject.Parse(additionalInfo);
					return jObject.GetJTokenValuePath<bool>("deliveryMethod.DeliveryMethod.isCourierDelivery");
				}
				return false;
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e, "OrderHandler.isCourierDeliveryMethod " + integrationInfo.ToString());
			}
			return false;
		}
		public override bool IsExport(IntegrationInfo integrationInfo)
		{
			return false;
		}
		public override void AfterEntitySave(IntegrationInfo integrationInfo)
		{
			if (integrationInfo.IntegrationType == CsConstant.TIntegrationType.Import)
			{
				try
				{
					if (integrationInfo.Data["Order"]["shipmentInfo"].HasValues)
					{
						var userConnection = integrationInfo.UserConnection;
						var id = integrationInfo.IntegratedEntity.GetTypedColumnValue<Guid>("Id");
						var country = integrationInfo.Data["Order"]["shipmentInfo"]["ShipmentInfo"]["country"].Value<string>();
						var region = integrationInfo.Data["Order"]["shipmentInfo"]["ShipmentInfo"]["region"].Value<string>();
						var place = integrationInfo.Data["Order"]["shipmentInfo"]["ShipmentInfo"]["place"].Value<string>();
						var district = integrationInfo.Data["Order"]["shipmentInfo"]["ShipmentInfo"]["district"].Value<string>();
						var street = integrationInfo.Data["Order"]["shipmentInfo"]["ShipmentInfo"]["street"].Value<string>();
						var building = integrationInfo.Data["Order"]["shipmentInfo"]["ShipmentInfo"]["building"].Value<string>();
						var appartament = integrationInfo.Data["Order"]["shipmentInfo"]["ShipmentInfo"]["appartament"].Value<string>();
						var zipCode = integrationInfo.Data["Order"]["shipmentInfo"]["ShipmentInfo"]["zipCode"].Value<string>();
						var address = integrationInfo.Data["Order"]["shipmentInfo"]["ShipmentInfo"]["address"].Value<string>();
						//http://tscore-task/browse/SKT-3911
						//address = string.Format("{0}, {1}, {2}, {3}", street, building, appartament, address);
						ImportAddress(id, integrationInfo.UserConnection,
								GetGuidByValue("Country", country, userConnection, true),
								GetGuidByValue("Region", region, userConnection, true),
								GetGuidByValue("City", place, userConnection, true),
								GetGuidByValue("TsCounty", district, userConnection, true),
								IfNullThanEmpty(address),
								IfNullThanEmpty(zipCode),
								CsConstant.TsAddressType.Delivery,
								IfNullThanEmpty(street),
								IfNullThanEmpty(appartament),
								IfNullThanEmpty(building));
					}
				}
				catch (Exception e)
				{
					IntegrationLogger.Error(e, "OrderHandler.AfterEntitySave[1 block] " + integrationInfo.ToString());
				}
				try
				{
					var OrderItemSum = GetOrderItemSum(integrationInfo.IntegratedEntity.GetTypedColumnValue<Guid>("Id"), integrationInfo.UserConnection);
					integrationInfo.IntegratedEntity.SetColumnValue("Amount", OrderItemSum);
				}
				catch (Exception e)
				{
					IntegrationLogger.Error(e, "OrderHandler.AfterEntitySave[2 block] " + integrationInfo.ToString());
				}
				try
				{
					integrationInfo.IntegratedEntity.SetColumnValue("PaymentAmount", integrationInfo.IntegratedEntity.GetColumnValue("PrimaryPaymentAmount"));
				}
				catch (Exception e)
				{
					IntegrationLogger.Error(e, "OrderHandler.AfterEntitySave[3 block] " + integrationInfo.ToString());
				}

				try
				{
					importTransportationPointCompany(integrationInfo);
				}
				catch (Exception e)
				{
					IntegrationLogger.Error(e, "OrderHandler.AfterEntitySave[4 block] " + integrationInfo.ToString());
				}
				try
				{
					if (integrationInfo.IntegratedEntity.GetTypedColumnValue<Guid>("TsContractId") != Guid.Empty)
					{
						var account = GetAccountByContract(integrationInfo.IntegratedEntity.GetTypedColumnValue<Guid>("TsContractId"), integrationInfo.UserConnection);
						if (account != Guid.Empty)
						{
							integrationInfo.IntegratedEntity.SetColumnValue("AccountId", account);
						}
					}
				}
				catch (Exception e)
				{
					IntegrationLogger.Error(e, "OrderHandler.AfterEntitySave[5 block] " + integrationInfo.ToString());
				}
				try
				{
					integrationInfo.IntegratedEntity.UpdateInDB(false);
				}
				catch (Exception e)
				{
					IntegrationLogger.Error(e, "OrderHandler.AfterEntitySave[6 block] " + integrationInfo.ToString());
				}
			}
		}

		public string IfNullThanEmpty(string text)
		{
			return string.IsNullOrEmpty(text) ? string.Empty : text;
		}
		public static Guid GetGuidByValue(string schemaName, string value, UserConnection userConnetion, bool createIfNotExist = false, string columnValue = "Name", string primaryColumn = "Id")
		{
			if (string.IsNullOrEmpty(value))
			{
				return Guid.Empty;
			}
			var select = new Select(userConnetion)
						.Column(primaryColumn).As("Id")
						.From(schemaName)
						.Where(columnValue).IsEqual(Column.Parameter(value)) as Select;
			using (DBExecutor dbExecutor = select.UserConnection.EnsureDBConnection())
			{
				using (IDataReader reader = select.ExecuteReader(dbExecutor))
				{
					if (reader.Read())
					{
						return DBUtilities.GetColumnValue<Guid>(reader, "Id");
					}
				}
			}
			if (createIfNotExist)
			{
				var resultId = Guid.NewGuid();
				var insert = new Insert(userConnetion)
									.Into(schemaName)
									.Set(columnValue, Column.Parameter(value))
									.Set(primaryColumn, Column.Parameter(resultId)) as Insert;
				insert.Execute();
				return resultId;
			}
			return Guid.Empty;
		}

		public void ImportAddress(Guid orderId, UserConnection userConnection, Guid country, Guid region, Guid city, Guid tsCountry, string address, string zipCode, Guid addressType, string street, string apartment, string house)
		{
			try
			{
				var orderAddressId = GetOrderAddres(orderId, userConnection);
				if (orderAddressId == Guid.Empty)
				{
					InsertOrderAddress(orderId, userConnection, country, tsCountry, city, addressType, region, zipCode, address, street, apartment, house);
				}
				else
				{
					UpdateOrderAddress(orderId, userConnection, country, tsCountry, city, addressType, region, zipCode, address, street, apartment, house);
				}
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e, string.Format("orderId {0}, country {1}, region {2}, city {3}, tsCountry {4}, address {5}, zipCode {6}, addressType {7}", orderId, country, region, city, tsCountry, address, zipCode, addressType));
				throw;
			}
		}

		public Guid GetOrderAddres(Guid orderId, UserConnection userConnetion)
		{
			var select = new Select(userConnetion)
						.Column("Id")
						.From("TsOrderAddress").As("src")
						.Where("TsOrderId").IsEqual(Column.Parameter(orderId)) as Select;
			using (DBExecutor dbExecutor = select.UserConnection.EnsureDBConnection())
			{
				using (IDataReader reader = select.ExecuteReader(dbExecutor))
				{
					while (reader.Read())
					{
						return DBUtilities.GetColumnValue<Guid>(reader, "Id");
					}
				}
			}
			return Guid.Empty;
		}

		public Guid InsertOrderAddress(Guid orderId, UserConnection userConnection, Guid countryId, Guid tsCountryId, Guid cityId, Guid addressTypeId, Guid regionId, string zip, string address, string street, string apartment, string house)
		{
			var addressId = Guid.NewGuid();
			var columnsGuid = new Dictionary<string, Guid>() {
				{ "TsOrderId", orderId },
				{ "TsCountyId", tsCountryId },
				{ "CityId", cityId },
				{ "RegionId", regionId },
				{ "CountryId", countryId },
				{ "AddressTypeId", addressTypeId }
			};
			var columnsString = new Dictionary<string, string>() {
				{ "Zip", zip },
				{ "TsStreet", street },
				{ "TsAppartment", apartment },
				{ "TsHouse", house },
				{ "Address", address }
			};
			var insert = new Insert(userConnection)
						.Into("TsOrderAddress")
						.Set("Id", Column.Parameter(addressId))
						.Set("Primary", Column.Parameter(true)) as Insert;
			columnsGuid
					.Where(x => x.Value != Guid.Empty)
					.ForEach(x => insert.Set(x.Key, Column.Parameter(x.Value)));
			columnsString
					.Select(x => x.Value == null ? new KeyValuePair<string, string>(x.Key, "") : x)
					.ForEach(x => insert.Set(x.Key, Column.Parameter(x.Value)));
			insert.Execute();
			return addressId;
		}
		public void UpdateOrderAddress(Guid orderId, UserConnection userConnection, Guid countryId, Guid tsCountryId, Guid cityId, Guid addressTypeId, Guid regionId, string zip, string address, string street, string apartment, string house)
		{
			var columnsGuid = new Dictionary<string, Guid>() {
				{ "TsOrderId", orderId },
				{ "TsCountyId", tsCountryId },
				{ "CityId", cityId },
				{ "RegionId", regionId },
				{ "CountryId", countryId },
				{ "AddressTypeId", addressTypeId }
			};
			var columnsString = new Dictionary<string, string>() {
				{ "Zip", zip },
				{ "TsStreet", street },
				{ "TsAppartment", apartment },
				{ "TsHouse", house },
				{ "Address", address }
			};
			var update = new Update(userConnection, "TsOrderAddress")
						.Set("Primary", Column.Parameter(true))
						.Where("Id").In(new Select(userConnection)
										.Top(1)
										.Column("a", "Id")
										.From("TsOrderAddress").As("a")
										.Where("a", "TsOrderId").IsEqual(Column.Parameter(orderId))
										.OrderByDesc("a", "CreatedOn") as Select) as Update;
			columnsGuid
					.Where(x => x.Value != Guid.Empty)
					.ForEach(x => update.Set(x.Key, Column.Parameter(x.Value)));
			columnsGuid
					.Where(x => x.Value == Guid.Empty)
					.ForEach(x => update.Set(x.Key, Column.Const(null)));
			columnsString
					.Select(x => x.Value == null ? new KeyValuePair<string, string>(x.Key, "") : x)
					.ForEach(x => update.Set(x.Key, Column.Parameter(x.Value)));
			update.Execute();
		}

		public Guid GetAccountFromContract(Guid id, UserConnection userConnection)
		{
			var select = new Select(userConnection)
								.Column("AccountId").As("Id")
								.From("Contract").As("c")
								.Where("Id").IsEqual(Column.Parameter(id)) as Select;
			using (DBExecutor dbExecutor = select.UserConnection.EnsureDBConnection())
			{
				using (IDataReader reader = select.ExecuteReader(dbExecutor))
				{
					while (reader.Read())
					{
						return DBUtilities.GetColumnValue<Guid>(reader, "Id");
					}
				}
			}
			return Guid.Empty;
		}

		public double GetOrderItemSum(Guid orderId, UserConnection userConnection)
		{
			var select = new Select(userConnection)
								.Column(Func.Sum("TotalAmount")).As("amount")
								.From("OrderProduct")
								.Where("OrderId").IsEqual(Column.Parameter(orderId)) as Select;
			using (DBExecutor dbExecutor = select.UserConnection.EnsureDBConnection())
			{
				using (IDataReader reader = select.ExecuteReader(dbExecutor))
				{
					while (reader.Read())
					{
						return DBUtilities.GetColumnValue<double>(reader, "amount");
					}
				}
			}
			return 0;
		}

		public double GetPaymentSum(Guid orderId, UserConnection userConnection)
		{
			var select = new Select(userConnection)
								   .Column(Func.Sum("TsAmount")).As("amount")
								   .From("TsPaymentInOrder").As("pio")
								   .InnerJoin("TsPayment").As("p").On("pio", "TsPaymentId").IsEqual("p", "Id")
								   .Where("TsOrderId").IsEqual(Column.Parameter(orderId)) as Select;
			using (DBExecutor dbExecutor = select.UserConnection.EnsureDBConnection())
			{
				using (IDataReader reader = select.ExecuteReader(dbExecutor))
				{
					while (reader.Read())
					{
						return DBUtilities.GetColumnValue<double>(reader, "amount");
					}
				}
			}
			return 0;
		}

		public Guid GetAccountByContract(Guid id, UserConnection userConnection)
		{
			var select = new Select(userConnection)
							.Column("AccountId")
							.From("Contract")
							.Where("Id").IsEqual(Column.Parameter(id)) as Select;

			using (DBExecutor dbExecutor = select.UserConnection.EnsureDBConnection())
			{
				using (IDataReader reader = select.ExecuteReader(dbExecutor))
				{
					while (reader.Read())
					{
						return DBUtilities.GetColumnValue<Guid>(reader, "AccountId");
					}
				}
			}
			return Guid.Empty;
		}
	}

	#endregion


	#region Class: OrderProductHandler
	/*
		Project Path: ..\..\..\QueryConsole\Files\TsIntegrationServiceEntity.TsBase.cs
		
	*/

	[ImportHandlerAttribute("OrderItem")]
	[ExportHandlerAttribute("OrderProduct")]
	public class OrderProductHandler : EntityHandler
	{
		public OrderProductHandler()
		{
			Mapper = new MappingHelper();
			EntityName = "OrderProduct";
			JName = "OrderItem";
		}
		public override void AfterEntitySave(IntegrationInfo integrationInfo)
		{
			if (integrationInfo.IntegrationType == CsConstant.TIntegrationType.Import)
			{
				try
				{
					if (integrationInfo.Data.SelectToken("OrderItem.contextInfo.OrderItemContextInfo") != null)
					{
						var orderId = integrationInfo.IntegratedEntity.GetTypedColumnValue<Guid>("OrderId");
						var productId = integrationInfo.IntegratedEntity.GetTypedColumnValue<Guid>("ProductId");
						var id = integrationInfo.Data.SelectToken("OrderItem.contextInfo.OrderItemContextInfo.id").Value<int>();
						var catalog = integrationInfo.Data.SelectToken("OrderItem.contextInfo.OrderItemContextInfo.catalog").Value<string>();
						var catalogV = integrationInfo.Data.SelectToken("OrderItem.contextInfo.OrderItemContextInfo.catalogVehicleId").Value<int>();
						var ssd = integrationInfo.Data.SelectToken("OrderItem.contextInfo.OrderItemContextInfo.ssd").Value<string>();
						var vih = integrationInfo.Data.SelectToken("OrderItem.contextInfo.OrderItemContextInfo.vin").Value<string>();
						var frame = integrationInfo.Data.SelectToken("OrderItem.contextInfo.OrderItemContextInfo.frame").Value<string>();
						var catCate = integrationInfo.Data.SelectToken("OrderItem.contextInfo.OrderItemContextInfo.catalogCategoryId").Value<int>();
						var unit = integrationInfo.Data.SelectToken("OrderItem.contextInfo.OrderItemContextInfo.unitId").Value<int>();
						var clientData = integrationInfo.Data.SelectToken("OrderItem.contextInfo.OrderItemContextInfo.clientData").Value<string>();
						var detailCode = integrationInfo.Data.SelectToken("OrderItem.contextInfo.OrderItemContextInfo.detailCode").Value<string>();
						if (id != 0)
						{
							var auto = JsonEntityHelper.GetEntityByExternalId("TsAutomobile", int.Parse(catalogV.ToString()), integrationInfo.UserConnection, false, "Id");
							if (JsonEntityHelper.isEntityExist("TsOrderAddInfo", integrationInfo.UserConnection, new Dictionary<string, object>() {
						{ "TsExternalId", id }
					}))
							{
								var update = new Update(integrationInfo.UserConnection, "TsOrderAddInfo")
											.Set("TsCatalog", Column.Parameter(catalog))
											.Set("TsSSD", Column.Parameter(ssd))
											.Set("TsVIN", Column.Parameter(vih))
											.Set("TsOrderId", Column.Parameter(orderId))
											.Set("TsProductId", Column.Parameter(productId))
											.Set("TsFrame", Column.Parameter(frame))
											.Set("TsCategory", Column.Parameter(catCate))
											.Set("TsQuantity", Column.Parameter(unit))
											.Set("TsClientInfo", Column.Parameter(clientData))
											.Set("TsCode", Column.Parameter(detailCode))
											.Where("TsExternalId").IsEqual(Column.Parameter(id)) as Update;
								var autoId = auto.Item2 != null ? auto.Item2.GetTypedColumnValue<Guid>(auto.Item1["Id"]) : Guid.Empty;
								if (autoId != Guid.Empty)
								{
									update.Set("TsAutomobileId", Column.Parameter(autoId));
								}
								update.Execute();
							}
							else
							{
								var insert = new Insert(integrationInfo.UserConnection)
												.Into("TsOrderAddInfo")
												.Set("TsExternalId", Column.Parameter(id))
												.Set("TsCatalog", Column.Parameter(catalog))
												.Set("TsOrderId", Column.Parameter(orderId))
												.Set("TsProductId", Column.Parameter(productId))
												.Set("TsSSD", Column.Parameter(ssd))
												.Set("TsVIN", Column.Parameter(vih))
												.Set("TsFrame", Column.Parameter(frame))
												.Set("TsCategory", Column.Parameter(catCate))
												.Set("TsQuantity", Column.Parameter(unit))
												.Set("TsClientInfo", Column.Parameter(clientData))
												.Set("TsCode", Column.Parameter(detailCode));
								var autoId = auto.Item2 != null ? auto.Item2.GetTypedColumnValue<Guid>(auto.Item1["Id"]) : Guid.Empty;
								if (autoId != Guid.Empty)
								{
									insert.Set("TsAutomobileId", Column.Parameter(autoId));
								}
								insert.Execute();
							}
						}
					}
				}
				catch (Exception e)
				{
					IntegrationLogger.Error(e, "OrderProductHandler.AfterEntitySave[1 block] " + integrationInfo.ToString());
				}

				try
				{
					var totalAmount = integrationInfo.IntegratedEntity.GetTypedColumnValue<double>("TotalAmount");
					integrationInfo.IntegratedEntity.SetColumnValue("PrimaryTotalAmount", totalAmount);
					integrationInfo.IntegratedEntity.UpdateInDB(false);
				}
				catch (Exception e)
				{
					IntegrationLogger.Error(e, "OrderProductHandler.AfterEntitySave[2 block] " + integrationInfo.ToString());
				}
			}
		}

		public override void Create(IntegrationInfo integrationInfo)
		{
			base.Create(integrationInfo);
			CreateProduct(integrationInfo);
		}


		public override void Update(IntegrationInfo integrationInfo)
		{
			base.Update(integrationInfo);
			CreateProduct(integrationInfo);
		}
		public void CreateProduct(IntegrationInfo integrationInfo)
		{
			try
			{
				var entity = integrationInfo.IntegratedEntity;
				var articul = integrationInfo.Data["OrderItem"]["oem"].Value<string>();
				var brand = integrationInfo.Data["OrderItem"]["brand"].Value<string>();
				integrationInfo.IntegratedEntity = GetProductByArticuleOrCreateNew(integrationInfo.UserConnection, articul, brand);
				integrationInfo.IntegratedEntity.SetDefColumnValues();
				Mapper.StartMappByConfig(integrationInfo, JName, IntegrationConfigurationManager.GetConfigItem(integrationInfo.UserConnection, "Product"));
				Mapper.SaveEntity(integrationInfo.IntegratedEntity, JName, ServiceName);
				var productId = integrationInfo.IntegratedEntity.GetTypedColumnValue<Guid>("Id");
				entity.SetColumnValue("ProductId", productId);
				entity.UpdateInDB(false);
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e, "OrderProductHandler.CreateProduct " + integrationInfo.ToString());
			}
		}

		public Entity GetProductByArticuleOrCreateNew(UserConnection userConnection, string articule, string brand)
		{
			var productId = ProductEntityHelper.GetOrCreateProductByBrandAndOem(userConnection, brand, articule);
			var esq = new EntitySchemaQuery(userConnection.EntitySchemaManager, "Product");
			esq.AddAllSchemaColumns();
			return esq.GetEntity(userConnection, productId);
		}

		public Guid GetProductIdByArticule(UserConnection userConnection, string articule)
		{
			var select = new Select(userConnection)
						.Column("Id")
						.From("Product")
						.Where("Code").IsEqual(Column.Parameter(articule)) as Select;
			using (DBExecutor dbExecutor = select.UserConnection.EnsureDBConnection())
			{
				using (IDataReader reader = select.ExecuteReader(dbExecutor))
				{
					while (reader.Read())
					{
						return DBUtilities.GetColumnValue<Guid>(reader, "Id");
					}
				}
			}
			return Guid.Empty;
		}
		public override bool IsExport(IntegrationInfo integrationInfo)
		{
			return false;
		}
	}

	#endregion


	#region Class: TsReturnHandler
	/*
		Project Path: ..\..\..\QueryConsole\Files\TsIntegrationServiceEntity.TsBase.cs
		
	*/

	[ImportHandlerAttribute("Return")]
	[ExportHandlerAttribute("TsReturn")]
	public class TsReturnHandler : EntityHandler
	{
		public TsReturnHandler()
		{
			Mapper = new MappingHelper();
			EntityName = "TsReturn";
			JName = "Return";
		}
		public override bool IsExport(IntegrationInfo integrationInfo)
		{
			return false;
		}
	}

	#endregion


	#region Class: TsReturnPositionHandler
	/*
		Project Path: ..\..\..\QueryConsole\Files\TsIntegrationServiceEntity.TsBase.cs
		
	*/

	[ImportHandlerAttribute("ReturnItem")]
	[ExportHandlerAttribute("TsReturnPosition")]
	public class TsReturnPositionHandler : EntityHandler
	{
		public TsReturnPositionHandler()
		{
			Mapper = new MappingHelper();
			EntityName = "TsReturnPosition";
			JName = "ReturnItem";
		}
		public override bool IsExport(IntegrationInfo integrationInfo)
		{
			return false;
		}
	}

	#endregion


	#region Class: TsShipmentHandler
	/*
		Project Path: ..\..\..\QueryConsole\Files\TsIntegrationServiceEntity.TsBase.cs
		
	*/

	[ImportHandlerAttribute("Shipment")]
	[ExportHandlerAttribute("TsShipment")]
	public class TsShipmentHandler : EntityHandler
	{
		public TsShipmentHandler()
		{
			Mapper = new MappingHelper();
			EntityName = "TsShipment";
			JName = "Shipment";
		}
		public override bool IsExport(IntegrationInfo integrationInfo)
		{
			return false;
		}
	}

	#endregion


	#region Class: TsShipmentPositionHandler
	/*
		Project Path: ..\..\..\QueryConsole\Files\TsIntegrationServiceEntity.TsBase.cs
		
	*/

	[ImportHandlerAttribute("ShipmentItem")]
	[ExportHandlerAttribute("TsShipmentPosition")]
	public class TsShipmentPositionHandler : EntityHandler
	{
		public TsShipmentPositionHandler()
		{
			Mapper = new MappingHelper();
			EntityName = "TsShipmentPosition";
			JName = "ShipmentItem";
		}

		public override void Create(IntegrationInfo integrationInfo)
		{
			base.Create(integrationInfo);
			UpdateProduct(integrationInfo);
		}


		public override void Update(IntegrationInfo integrationInfo)
		{
			base.Update(integrationInfo);
			UpdateProduct(integrationInfo);
		}

		public void UpdateProduct(IntegrationInfo integrationInfo)
		{
			try
			{
				var eom = integrationInfo.Data["ShipmentItem"]["oem"].Value<string>();
				var brand = integrationInfo.Data["ShipmentItem"]["brand"].Value<string>();
				var unitName = integrationInfo.Data["ShipmentItem"]["unitName"].Value<string>();
				var productId = ProductEntityHelper.GetOrCreateProductByBrandAndOem(integrationInfo.UserConnection, brand, eom,
								command =>
								{
									var unitId = JsonEntityHelper.GetColumnValues(integrationInfo.UserConnection, "Unit", "ShortName", unitName, "Id", 1).FirstOrDefault();
									if (unitId == null)
									{
										return;
									}
									if (command is Insert)
									{
										var insert = (Insert)command;
										insert.Set("UnitId", Column.Parameter(unitId));
									}
									else if (command is Update)
									{
										var update = (Update)command;
										update.Set("UnitId", Column.Parameter(unitId));
									}
								});
				integrationInfo.IntegratedEntity.SetColumnValue("TsProductId", productId);
				integrationInfo.IntegratedEntity.UpdateInDB(false);
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e, "UpdateProduct " + integrationInfo.ToString());
			}
		}

		public Guid GetProductIdByArticule(UserConnection userConnection, string articule)
		{
			var select = new Select(userConnection)
						.Column("Id")
						.From("Product")
						.Where("Code").IsEqual(Column.Parameter(articule)) as Select;
			using (DBExecutor dbExecutor = select.UserConnection.EnsureDBConnection())
			{
				using (IDataReader reader = select.ExecuteReader(dbExecutor))
				{
					while (reader.Read())
					{
						return DBUtilities.GetColumnValue<Guid>(reader, "Id");
					}
				}
			}
			return Guid.Empty;
		}

		public void updateProductUnitName(UserConnection userConnection, Guid productId, string unitName)
		{
			var unitId = OrderHandler.GetGuidByValue("Unit", unitName, userConnection, false, "ShortName");
			if (unitId == Guid.Empty)
				return;
			var update = new Update(userConnection, "Product")
						.Set("UnitId", Column.Parameter(unitId))
						.Where("Id").IsEqual(Column.Parameter(productId)) as Update;
			update.Execute();
			var updateOrderProduct = new Update(userConnection, "OrderProduct")
						.Set("UnitId", Column.Parameter(unitId))
						.Where("ProductId").IsEqual(Column.Parameter(productId)) as Update;
			updateOrderProduct.Execute();
		}

		public override bool IsExport(IntegrationInfo integrationInfo)
		{
			return false;
		}
	}

	#endregion


	#region Class: ContractBalanceHandler
	/*
		Project Path: ..\..\..\QueryConsole\Files\TsIntegrationServiceEntity.TsBase.cs
		
	*/

	[ImportHandlerAttribute("ContractBalance")]
	[ExportHandlerAttribute("Contract")]
	public class ContractBalanceHandler : EntityHandler
	{
		public override string ExternalIdPath {
			get {
				return "TsContractBalanceId";
			}
		}
		public override string HandlerName {
			get {
				return JName;
			}
		}
		public ContractBalanceHandler()
		{
			Mapper = new MappingHelper();
			EntityName = "Contract";
			JName = "ContractBalance";
		}
		public override bool IsEntityAlreadyExist(IntegrationInfo integrationInfo)
		{
			try
			{
				Mapper.UserConnection = integrationInfo.UserConnection;
				integrationInfo.TsExternalIdPath = ExternalIdPath;
				integrationInfo.TsExternalVersionPath = ExternalVersionPath;
				JToken externalIdToken = integrationInfo.Data.SelectToken("ContractBalance.contract.#ref.id");
				if (externalIdToken == null)
				{
					return false;
				}
				int externalId = 0;
				if (integrationInfo.IntegratedEntity != null)
				{
					externalId = integrationInfo.IntegratedEntity.GetTypedColumnValue<int>(ExternalIdPath);
				}
				return Mapper.CheckIsExist(EntityName, externalIdToken.Value<int>(), "TsExternalId", externalId);
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e, integrationInfo.ToString());
			}
			return false;
		}
		public override bool IsExport(IntegrationInfo integrationInfo)
		{
			return false;
		}
		public override Entity GetEntityByExternalId(IntegrationInfo integrationInfo)
		{
			string externalIdPath = "TsExternalId";
			var esq = new EntitySchemaQuery(integrationInfo.UserConnection.EntitySchemaManager, EntityName);
			esq.AddAllSchemaColumns();
			esq.RowCount = 1;
			esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, externalIdPath, integrationInfo.Data.SelectToken("ContractBalance.contract.#ref.id").Value<int>()));
			return esq.GetEntityCollection(integrationInfo.UserConnection).FirstOrDefault();
		}
		public override void Create(IntegrationInfo integrationInfo)
		{
			try
			{
				integrationInfo.TsExternalIdPath = ExternalIdPath;
				integrationInfo.TsExternalVersionPath = ExternalVersionPath;
				var entitySchema = integrationInfo.UserConnection.EntitySchemaManager.GetInstanceByName(EntityName);
				integrationInfo.IntegratedEntity = entitySchema.CreateEntity(integrationInfo.UserConnection);
				integrationInfo.IntegratedEntity.SetDefColumnValues();
				BeforeMapping(integrationInfo);
				Mapper.StartMappByConfig(integrationInfo, JName, GetMapConfig(integrationInfo.UserConnection), IsJsonWithHeader);
				AfterMapping(integrationInfo);
			} catch(Exception e)
			{
				IntegrationLogger.Error(e);
			}
		}
		public override bool IsVersionHigger(IntegrationInfo integrationInfo)
		{
			return true;
		}
	}

	#endregion


	#region Class: ContractHandler
	/*
		Project Path: ..\..\..\QueryConsole\Files\TsIntegrationServiceEntity.TsBase.cs
		
	*/

	[ImportHandlerAttribute("Contract")]
	[ExportHandlerAttribute("Contract")]
	public class ContractHandler : EntityHandler
	{
		public ContractHandler()
		{
			Mapper = new MappingHelper();
			EntityName = "Contract";
			JName = "Contract";
		}
		public override void BeforeMapping(IntegrationInfo integrationInfo)
		{
			if (integrationInfo.Data == null)
			{
				integrationInfo.Data = new JObject();
			}
		}

		public override void AfterEntitySave(IntegrationInfo integrationInfo)
		{
			if (integrationInfo.IntegrationType == CsConstant.TIntegrationType.Import)
			{
				SetState(integrationInfo);
				SetBussinesProtocol(integrationInfo);
			}
		}
		public void SetState(IntegrationInfo integrationInfo)
		{
			try
			{
				var isActive = integrationInfo.IntegratedEntity.GetTypedColumnValue<bool>("TsActive");
				if (isActive)
				{
					integrationInfo.IntegratedEntity.SetColumnValue("StateId", CsConstant.TsContractState.Signed);
					integrationInfo.IntegratedEntity.UpdateInDB(false);
				}
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e, "ContractHandler.SetState " + integrationInfo.ToString());
			}
		}
		public void SetBussinesProtocol(IntegrationInfo integrationInfo)
		{
			try
			{
				var accountId = integrationInfo.IntegratedEntity.GetTypedColumnValue<Guid>("AccountId");
				if (accountId != Guid.Empty && integrationInfo.Action == CsConstant.IntegrationActionName.Create)
				{
					var isLegal = IsAccountLegal(accountId, integrationInfo.UserConnection);
					integrationInfo.IntegratedEntity.SetColumnValue(isLegal ? "TsB2B" : "TsB2C", true);
					integrationInfo.IntegratedEntity.UpdateInDB(false);
				}
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e, "ContractHandler.SetBussinesProtocol " + integrationInfo.ToString());
			}
		}

		public bool IsAccountLegal(Guid accountId, UserConnection userConnection)
		{
			var acountLegalSelect = new Select(userConnection)
							.Column("TsIsLawPerson").As("IsLegal")
							.From("Account")
							.Where("Id").IsEqual(Column.Parameter(accountId)) as Select;
			using (DBExecutor dbExecutor = acountLegalSelect.UserConnection.EnsureDBConnection())
			{
				using (IDataReader reader = acountLegalSelect.ExecuteReader(dbExecutor))
				{
					while (reader.Read())
					{
						return DBUtilities.GetColumnValue<bool>(reader, "IsLegal");
					}
				}
			}
			throw new Exception("IsAccountLegal throw Exception: No account with id = " + accountId.ToString());
		}
		public override bool IsExport(IntegrationInfo integrationInfo)
		{
			return integrationInfo.IntegratedEntity != null && integrationInfo.IntegratedEntity.GetTypedColumnValue<Guid>("StateId") == CsConstant.TsContractState.Vising && !IsAlreadyExported(integrationInfo);
		}
		/// <summary>
		/// Возвращает проинтегрирован ли договор
		/// </summary>
		/// <param name="integrationInfo">Информация о интеграции</param>
		/// <returns>Признак проинтегрированости</returns>
		public bool IsAlreadyExported(IntegrationInfo integrationInfo)
		{
			if(integrationInfo.IntegratedEntity != null)
			{
				var externalId = integrationInfo.IntegratedEntity.GetTypedColumnValue<int>(ExternalIdPath);
				return externalId > 0;
			}
			return false;
		}
	}

	#endregion


	#region Class: TsContractDebtHandler
	/*
		Project Path: ..\..\..\QueryConsole\Files\TsIntegrationServiceEntity.TsBase.cs
		
	*/

	[ImportHandlerAttribute("Debt")]
	[ExportHandlerAttribute("TsContractDebt")]
	public class TsContractDebtHandler : EntityHandler
	{
		public TsContractDebtHandler()
		{
			Mapper = new MappingHelper();
			EntityName = "TsContractDebt";
			JName = "Debt";
		}


	}

	#endregion


	#region Class: ManagerInfoHandler
	/*
		Project Path: ..\..\..\QueryConsole\Files\TsIntegrationServiceEntity.TsBase.cs
		
	*/

	[ImportHandlerAttribute("ManagerInfo")]
	[ExportHandlerAttribute("Contact")]
	public class ManagerInfoHandler : EntityHandler
	{
		public override string HandlerName {
			get {
				return JName;
			}
		}
		public override string ExternalIdPath {
			get {
				return CsConstant.ServiceColumnInBpm.IdentifierManagerInfo;
			}
		}

		public override string ExternalVersionPath {
			get {
				return CsConstant.ServiceColumnInBpm.VersionManagerInfo;
			}
		}

		public ManagerInfoHandler()
		{
			Mapper = new MappingHelper();
			EntityName = "Contact";
			JName = "ManagerInfo";
		}

		public override bool IsExport(IntegrationInfo integrationInfo)
		{
			return IsContactHaveSysAdminUnit(integrationInfo.IntegratedEntity.GetTypedColumnValue<Guid>("Id"), integrationInfo.IntegratedEntity.UserConnection);
		}

		public bool IsContactHaveSysAdminUnit(Guid contactId, UserConnection userConnection)
		{
			var select = new Select(userConnection)
						.Column(Func.Count("Id")).As("count")
						.From("SysAdminUnit").As("sau")
						.Where("sau", "ContactId").IsEqual(Column.Parameter(contactId)) as Select;
			using (DBExecutor dbExecutor = select.UserConnection.EnsureDBConnection())
			{
				using (IDataReader reader = select.ExecuteReader(dbExecutor))
				{
					while (reader.Read())
					{
						return DBUtilities.GetColumnValue<int>(reader, "count") > 0;
					}
				}
			}
			return false;
		}
	}

	#endregion


	#region Class: CounteragentContactInfoHandler
	/*
		Project Path: ..\..\..\QueryConsole\Files\TsIntegrationServiceEntity.TsBase.cs
		
	*/
	[ImportHandlerAttribute("CounteragentContactInfo")]
	[ExportHandlerAttribute("Contact")]
	public class CounteragentContactInfoHandler : EntityHandler
	{
		private AdvancedSearchInfo _advancedSearchInfo;

		public override void BeforeMapping(IntegrationInfo integrationInfo)
		{
			if (integrationInfo.IntegrationType == CsConstant.TIntegrationType.Import)
			{
				integrationInfo.Data["CounteragentContactInfo"]["positionFull"] = integrationInfo.Data["CounteragentContactInfo"]["position"];
			}
		}
		public override string HandlerName {
			get {
				return JName;
			}
		}
		public override string ExternalIdPath {
			get {
				return CsConstant.ServiceColumnInBpm.IdentifierOrder;
			}
		}

		public override string ExternalVersionPath {
			get {
				return CsConstant.ServiceColumnInBpm.VersionOrder;
			}
		}
		public override bool IsAdvancedSearch {
			get {
				return true;
			}
		}
		public override AdvancedSearchInfo AdvancedSearchInfo {
			get {
				return _advancedSearchInfo;
			}
		}
		public bool isFindedAdvanced;
		public CounteragentContactInfoHandler()
		{
			Mapper = new MappingHelper();
			EntityName = "Contact";
			JName = "CounteragentContactInfo";
			_advancedSearchInfo = new AdvancedSearchInfo()
			{
				StoredProcedureName = "tsp_Integration_AdvancedSearch_Contact"
			};
		}

		public override bool IsExport(IntegrationInfo integrationInfo)
		{

			var isClient = integrationInfo.IntegratedEntity.GetTypedColumnValue<bool>("TsIsClient");
			if (!isClient)
			{
				var contactId = integrationInfo.IntegratedEntity.GetTypedColumnValue<Guid>("Id");
				var account = integrationInfo.IntegratedEntity.GetTypedColumnValue<Guid>("AccountId");
				return IsAccountHaveOrderServiceId(account, contactId, integrationInfo.UserConnection);
			}
			return true;
		}
		public bool IsAccountHaveOrderServiceId(Guid accountId, Guid contactId, UserConnection userConnection)
		{
			var select = new Select(userConnection)
				.Column(Func.Count("Id")).As("count")
				.From("Account").As("a")
				.Where().OpenBlock("a", "PrimaryContactId").IsEqual(Column.Parameter(contactId)) as Select;
			if (accountId != Guid.Empty)
			{
				select.Or("a", "Id").IsEqual(Column.Parameter(accountId));
			}
			select
				.CloseBlock()
				.And("a", "TsOrderServiceId").IsNotEqual(Column.Const(0));
			using (DBExecutor executor = select.UserConnection.EnsureDBConnection())
			{
				using (var reader = select.ExecuteReader(executor))
				{
					if (reader.Read())
					{
						return reader.GetColumnValue<int>("count") > 0;
					}
				}
			}
			return false;
		}
		public void UpdateAddressFromDeliveryService(IntegrationInfo integrationInfo)
		{
			try
			{
				ContactEntityHelper.UpdateAddressFromDeliveryService(integrationInfo.UserConnection, integrationInfo.IntegratedEntity, IntegrationLogger.SimpleLoggerErrorAction);
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e);
			}
		}
		public override void AfterEntitySave(IntegrationInfo integrationInfo)
		{
			base.AfterEntitySave(integrationInfo);
			if (integrationInfo.IntegrationType == CsConstant.TIntegrationType.Import && integrationInfo.Data != null)
			{
				if (!string.IsNullOrEmpty(integrationInfo.Data.SelectToken(JName + ".address").Value<string>()))
				{
					UpdateAddressFromDeliveryService(integrationInfo);
					UpdateLastAddressType(integrationInfo, IsAccountLegal(integrationInfo));
				}
			}
			ContactEntityHelper.ResaveContactPrimaryAddress(integrationInfo.UserConnection, integrationInfo.IntegratedEntity, integrationInfo.IntegratedEntity.PrimaryColumnValue, IntegrationLogger.SimpleLoggerErrorAction);
			//ContactEntityHelper.ClearContactPrimaryCommunication(integrationInfo.UserConnection, integrationInfo.IntegratedEntity, IntegrationLogger.SimpleLoggerErrorAction);
			//http://tscore-task/browse/SKT-4696
			ContactEntityHelper.SynchronizeCommunication(integrationInfo.UserConnection, integrationInfo.IntegratedEntity.PrimaryColumnValue, IntegrationLogger.SimpleLoggerErrorAction);
			ReintegrateContactPersonProfile(integrationInfo);
			CreateContactCareer(integrationInfo);
		}

		private void CreateContactCareer(IntegrationInfo integrationInfo)
		{
			if (integrationInfo == null || integrationInfo.IntegratedEntity == null)
			{
				return;
			}
			try
			{
				var contactEntity = integrationInfo.IntegratedEntity;
				var accountId = contactEntity.GetTypedColumnValue<Guid>("AccountId");
				if (accountId != Guid.Empty)
				{
					var createCareerIfNeedSp = new StoredProcedure(integrationInfo.UserConnection,
								"tsp_IntegrationHandler_CreateContactCareer")
							.WithParameter("contactId", contactEntity.PrimaryColumnValue)
							.WithParameter("accountId", accountId)
							.WithOutputParameter("resultId", integrationInfo.UserConnection.DataValueTypeManager.GetInstanceByName("Guid"))
							.WithOutputParameter("changedIds", integrationInfo.UserConnection.DataValueTypeManager.GetInstanceByName("Text"))
						as StoredProcedure;
					createCareerIfNeedSp.PackageName = integrationInfo.UserConnection.DBEngine.SystemPackageName;
					createCareerIfNeedSp.Execute();
					var resultId = createCareerIfNeedSp.Parameters.GetByName("resultId").Value as Guid?;
					var changedIds = createCareerIfNeedSp.Parameters.GetByName("changedIds").Value as String;
					var integrator = new ClientServiceIntegrator(integrationInfo.UserConnection);
					if (resultId.HasValue)
					{
						if(!CheckIsAccountIntegrateInClientService(integrationInfo.UserConnection, accountId))
						{
							integrator.IntegrateBpmEntity(accountId, "Account", new AccountHandler());
						}
						integrator.IntegrateBpmEntity(resultId.Value, "ContactCareer");
					}
					if (!string.IsNullOrEmpty(changedIds))
					{
						changedIds
							.Split(new char[] { ',' })
							.Select(x =>
							{
								Guid uId;
								Guid.TryParse(x, out uId);
								return uId;
							})
							.Where(x => x != Guid.Empty)
							.ForEach(careerId =>
							{
								integrator.IntegrateBpmEntity(careerId, "ContactCareer");
							});
					}
				}
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e);
			}
		}
		public bool CheckIsAccountIntegrateInClientService(UserConnection userConnection, Guid accountId)
		{
			try
			{
				if (accountId != Guid.Empty)
				{
					var select = new Select(userConnection)
							.Column(Func.Count("Id")).As("count")
							.From("Account")
							.Where("Id").IsEqual(Column.Parameter(accountId))
							.And("TsExternalId").IsGreater(Column.Const(0)) as Select;
					var count = select.ExecuteScalar<int>();
					return count > 0;
				}
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e);
			}
			return false;
		}
		private void ReintegrateContactPersonProfile(IntegrationInfo integrationInfo)
		{
			try
			{
				var integrator = new ClientServiceIntegrator(integrationInfo.UserConnection);
				integrator.IntegrateBpmEntity(integrationInfo.IntegratedEntity, new ContactHandler());
				if(isFindedAdvanced)
				{
					//!!!KOSTYL
					IntegrationLocker.Unlock("Contact", integrationInfo.IntegratedEntity.GetTypedColumnValue<int>("TsOrderServiceId"),
					"OrderService_CounteragentContactInfo");
					integrator.IntegrateBpmEntity(integrationInfo.IntegratedEntity, new CounteragentContactInfoHandler());
					//!!!KOSTYL
				}
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e, integrationInfo.ToString());
			}
		}
		public virtual bool? IsAccountLegal(IntegrationInfo integrationInfo)
		{
			try
			{
				if (integrationInfo.IntegratedEntity != null && integrationInfo.IntegratedEntity.IsColumnValueLoaded("AccountId"))
				{
					var accountId = integrationInfo.IntegratedEntity.GetTypedColumnValue<Guid>("AccountId");
					if (accountId != Guid.Empty)
					{
						var select = new Select(integrationInfo.UserConnection)
										.Top(1)
										.Column("TsIsLawPerson")
										.From("Account")
										.Where("Id").IsEqual(Column.Parameter(accountId)) as Select;
						using (var dbExecutor = select.UserConnection.EnsureDBConnection())
						{
							using (var reader = select.ExecuteReader(dbExecutor))
							{
								if (reader.Read())
								{
									return reader.GetColumnValue<bool>("TsIsLawPerson");
								}
							}
						}
					}
				}
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e, integrationInfo.ToString());
			}
			return null;
		}
		public virtual void UpdateLastAddressType(IntegrationInfo integrationInfo, bool? isAccountLegal)
		{
			if (!isAccountLegal.HasValue)
			{
				return;
			}
			try
			{
				var contactId = integrationInfo.IntegratedEntity.PrimaryColumnValue;
				if (contactId != Guid.Empty)
				{
					QueryColumnExpression addressTypeId = isAccountLegal.Value ? Column.Parameter(CsConstant.EntityConst.AddressType.Work) : Column.Parameter(CsConstant.EntityConst.AddressType.Delivery);
					var addressTypeUpdate = new Update(integrationInfo.UserConnection, "ContactAddress")
									.Set("AddressTypeId", addressTypeId)
									.Where("Id").IsEqual(new Select(integrationInfo.UserConnection)
															.Top(1)
															.Column("ca", "Id")
															.From("ContactAddress").As("ca")
															.Where("ca", "ContactId").IsEqual(Column.Parameter(contactId))
															.OrderByDesc("ca", "CreatedOn") as Select
									) as Update;
					addressTypeUpdate.Execute();
				}
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e, integrationInfo.ToString());
			}
		}
		public override void AddParameterToSearchProcedure(IntegrationInfo integrationInfo, StoredProcedure searchProcedure)
		{
			try
			{
				Tuple<string, string> emailPhoneTuple = new Tuple<string, string>(string.Empty, string.Empty);
				if (integrationInfo != null && integrationInfo.Data != null)
				{
					emailPhoneTuple = GetEmailAndPhones(integrationInfo.Data);
				}
				searchProcedure
					.WithParameter("Emails", emailPhoneTuple.Item1 ?? string.Empty)
					.WithParameter("Phones", emailPhoneTuple.Item2 ?? string.Empty)
					.WithParameter("ExternalIdPath", ExternalIdPath);
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e);
			}
		}
		public Tuple<string, string> GetEmailAndPhones(JObject jObj)
		{
			string emails = String.Empty;
			var phones = new List<string>();
			var emailToken = jObj.SelectToken(JName + ".email");
			if(emailToken != null)
			{
				emails = emailToken.Value<string>();
			}
			var phonesPath = new List<string>()
			{
				"mobilePhone",
				"primaryPhone",
				"workPhone"
			};
			phonesPath.ForEach(x =>
			{
				var phoneToken = jObj.SelectToken(JName + "." + x);
				if (phoneToken != null)
				{
					phones.AddRange(PhoneFormatHelper.ToAllFormats(phoneToken.Value<string>()));
				}
			});
			return new Tuple<string, string>(emails, string.Join(",", phones));
		}

		public override bool IsEntityAlreadyExistAdvanced(IntegrationInfo integrationInfo)
		{
			var result = base.IsEntityAlreadyExistAdvanced(integrationInfo);
			isFindedAdvanced = result;
			return result;
		}
		public override bool IsEntityAlreadyExist(IntegrationInfo integrationInfo)
		{
			isFindedAdvanced = false;
			return base.IsEntityAlreadyExist(integrationInfo);
		}
	}

	#endregion


	#region Class: CounteragentHandler
	/*
		Project Path: ..\..\..\QueryConsole\Files\TsIntegrationServiceEntity.TsBase.cs
		
	*/

	[ImportHandlerAttribute("Counteragent")]
	[ExportHandlerAttribute("Account")]
	public class CounteragentHandler : EntityHandler
	{
		private AdvancedSearchInfo _advancedSearchInfo;

		public override string HandlerName {
			get {
				return JName;
			}
		}
		public override string ExternalIdPath {
			get {
				return CsConstant.ServiceColumnInBpm.IdentifierOrder;
			}
		}

		public override string ExternalVersionPath {
			get {
				return CsConstant.ServiceColumnInBpm.VersionOrder;
			}
		}
		public override bool IsAdvancedSearch {
			get {
				return true;
			}
		}
		public override AdvancedSearchInfo AdvancedSearchInfo {
			get {
				return _advancedSearchInfo;
			}
		}
		public bool isFindedAdvanced;

		public bool? isAccountHasOsId = null;
		public CounteragentHandler()
		{
			Mapper = new MappingHelper();
			EntityName = "Account";
			JName = "Counteragent";
			_advancedSearchInfo = new AdvancedSearchInfo()
			{
				StoredProcedureName = CsConstant.EntityConst.AccountConst.AccountSearchStoredProcedureB2c
			};
		}
		public override bool IsExport(IntegrationInfo integrationInfo)
		{
			return
				integrationInfo.IntegratedEntity.GetTypedColumnValue<int>(ExternalIdPath) > 0
				||
				!isAccountExported(integrationInfo) && isAccountContracted(integrationInfo.IntegratedEntity.GetTypedColumnValue<Guid>("Id"), integrationInfo.UserConnection);
		}

		public bool isAccountContracted(Guid accountId, UserConnection userConnection)
		{
			var select = new Select(userConnection)
						.Column(Func.Count("c", "Id")).As("count")
						.From("Contract").As("c")
						.Where("c", "AccountId").IsEqual(Column.Parameter(accountId))
						.And("c", "TsActive").IsEqual(Column.Parameter(true)) as Select;
			using (DBExecutor dbExecutor = select.UserConnection.EnsureDBConnection())
			{
				using (IDataReader reader = select.ExecuteReader(dbExecutor))
				{
					while (reader.Read())
					{
						return DBUtilities.GetColumnValue<int>(reader, "count") > 0;
					}
				}
			}
			return false;
		}

		public bool isAccountExported(IntegrationInfo integrationInfo)
		{
			return integrationInfo.IntegratedEntity.GetTypedColumnValue<bool>("TsDontIntegrate");
		}

		public override void Update(IntegrationInfo integrationInfo)
		{
			if (integrationInfo.IntegratedEntity != null && integrationInfo.IntegratedEntity.IsColumnValueLoaded(ExternalVersionPath))
			{
				var id = integrationInfo.IntegratedEntity.GetTypedColumnValue<int>(ExternalVersionPath);
				isAccountHasOsId = id > 0;
			};
			base.Update(integrationInfo);
		}

		public override void AfterEntitySave(IntegrationInfo integrationInfo)
		{
			base.AfterEntitySave(integrationInfo);
			SetBussinesProtocol(integrationInfo);
			UpdateAddressFromDeliveryService(integrationInfo);
			CreateContact(integrationInfo);
			AccountEntityHelper.ResaveAccountPrimaryAddress(integrationInfo.UserConnection, integrationInfo.IntegratedEntity, integrationInfo.IntegratedEntity.PrimaryColumnValue, IntegrationLogger.SimpleLoggerErrorAction);
			//AccountEntityHelper.ClearAccountPrimaryCommunication(integrationInfo.UserConnection, integrationInfo.IntegratedEntity, IntegrationLogger.SimpleLoggerErrorAction);
			//http://tscore-task/browse/SKT-4696
			AccountEntityHelper.SynchronizeCommunication(integrationInfo.UserConnection, integrationInfo.IntegratedEntity.PrimaryColumnValue, IntegrationLogger.SimpleLoggerErrorAction);
			ReintegrateAccountCompanyProfile(integrationInfo);
			if(isFindedAdvanced)
			{
				ReintegrateAccountContacts(integrationInfo);
			}
		}

		private void ReintegrateAccountCompanyProfile(IntegrationInfo integrationInfo)
		{
			try
			{
				var integrator = new ClientServiceIntegrator(integrationInfo.UserConnection);
				integrator.IntegrateBpmEntity(integrationInfo.IntegratedEntity, new AccountHandler());
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e, integrationInfo.ToString());
			}
		}

		public void UpdateAddressFromDeliveryService(IntegrationInfo integrationInfo)
		{
			try
			{
				if (integrationInfo.IntegrationType == CsConstant.TIntegrationType.Import && integrationInfo.Data != null)
				{
					if (!string.IsNullOrEmpty(integrationInfo.Data.SelectToken(JName + ".address").Value<string>()))
					{
						AccountEntityHelper.UpdateAddressFromDeliveryService(integrationInfo.UserConnection, integrationInfo.IntegratedEntity, IntegrationLogger.SimpleLoggerErrorAction, esq =>
						{
							esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, "AddressType", CsConstant.EntityConst.AddressType.Legal));
						});
					}
					if (!string.IsNullOrEmpty(integrationInfo.Data.SelectToken(JName + ".locationAddress").Value<string>()))
					{
						AccountEntityHelper.UpdateAddressFromDeliveryService(integrationInfo.UserConnection, integrationInfo.IntegratedEntity, IntegrationLogger.SimpleLoggerErrorAction, esq =>
						{
							esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, "AddressType", CsConstant.EntityConst.AddressType.Fact));
						});
					}
				}
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e);
			}
		}
		public void SetBussinesProtocol(IntegrationInfo integrationInfo)
		{
			try
			{
				if (integrationInfo.Action == CsConstant.IntegrationActionName.Create)
				{
					var isLegal = integrationInfo.IntegratedEntity.GetTypedColumnValue<bool>("TsIsLawPerson");
					integrationInfo.IntegratedEntity.SetColumnValue(isLegal ? "TsB2B" : "TsB2C", true);
					integrationInfo.IntegratedEntity.UpdateInDB(false);
				}
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e, "CounteragentHandler");
			}
		}

		public void CreateContact(IntegrationInfo integrationInfo)
		{
			if ((integrationInfo.Action == CsConstant.IntegrationActionName.Create || (isAccountHasOsId.HasValue && !isAccountHasOsId.Value)) && integrationInfo.IntegrationType == CsConstant.TIntegrationType.Import)
			{
				try
				{
					var isB2c = integrationInfo.IntegratedEntity.GetTypedColumnValue<bool>("TsB2C");
					if (isB2c)
					{
						AdvancedSearchInfo.StoredProcedureName = CsConstant.EntityConst.AccountConst.AccountSearchStoredProcedureB2c;
						if(!ContactEntityHelper.FindContactByAccount(integrationInfo.IntegratedEntity, integrationInfo.UserConnection, (x) => AddParameterToSearchProcedure(integrationInfo, x), IntegrationLogger.SimpleLoggerErrorAction)) {
							ContactEntityHelper.CreateContactByAccount(integrationInfo.IntegratedEntity, integrationInfo.UserConnection, true, true, IntegrationLogger.SimpleLoggerErrorAction);
						}
					}
				}
				catch (Exception e)
				{
					IntegrationLogger.Error(e);
				}
			}
		}

		public override JObject ToJson(IntegrationInfo integrationInfo)
		{
			var result = base.ToJson(integrationInfo);

			try
			{
				if (!result.IsJTokenPathHasValue("Counteragent.taxRegistrationNumber"))
				{
					result.RemoveByPath("Counteragent.taxRegistrationNumberName");
					result.RemoveByPath("Counteragent.taxRegistrationNumber");
				}
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e);
			}

			try
			{
				if (!result.IsJTokenPathHasValue("Counteragent.companyRegistrationNumber"))
				{
					result.RemoveByPath("Counteragent.companyRegistrationNumberName");
					result.RemoveByPath("Counteragent.companyRegistrationNumber");
				}
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e);
			}
			return result;
		}

		public override void ProcessResponse(IntegrationInfo integrationInfo)
		{
			base.ProcessResponse(integrationInfo);
			//Если контрагент только законтрактован то отправляем его в client service. Это происходит единожды
			ReintegrateContactsAndSelf(integrationInfo);
		}
		/// <summary>
		/// http://tscore-task/browse/SKT-4216
		/// Если у Counteragent не заполнено поле основной контакт, то инициируем переотправку всех контактов контрагента.
		/// Если у даного Контрагента будет заполнено поле "Основной контакт", то инициируем переотправку Контрагента в OrderService
		/// Гарантируется, что в рамках этой переотправки интеграция рекурсивно не начнет переотпраку, еще раз, по даному контрагенту
		/// </summary>
		/// <param name="integrationInfo">Информация о текущей транзакции интеграции</param>
		public void ReintegrateContactsAndSelf(IntegrationInfo integrationInfo)
		{
			if (integrationInfo.Action == CsConstant.IntegrationActionName.Create || isMainContactEmpty(integrationInfo))
			{
				var externalId = integrationInfo.IntegratedEntity.GetExternalIdValue(ExternalIdPath);
				//Добавляем новую блокировку которая не позволит переотпраку несколько раз
				LockerHelper.DoWithEntityLock(externalId, "Account", () =>
				{
					if (ReintegrateAccountContacts(integrationInfo))
					{
						try
						{
							//Снимаем блокировку перед переотправкой контрагента
							if (!IntegrationLocker.CheckUnLock("Account", externalId, "OrderService_Counteragent"))
							{
								IntegrationLocker.Unlock("Account", externalId, "OrderService_Counteragent");
							}
							var integrator = new ClientServiceIntegrator(integrationInfo.UserConnection);
							integrator.IntegrateBpmEntity(integrationInfo.IntegratedEntity, new CounteragentHandler(), false);
						}
						catch (Exception e)
						{
							IntegrationLogger.Error(e);
						}
					}
				}, IntegrationLogger.SimpleLoggerErrorAction, "OrderService_Counteragent_ReintegrateSelf");
			}
		}
		/// <summary>
		/// Переотправляем контактов в ордерсервис
		/// </summary>
		/// <param name="integrationInfo"></param>
		/// <returns>Возвращает был ли отправлен </returns>
		public bool ReintegrateAccountContacts(IntegrationInfo integrationInfo)
		{
			try
			{
				var id = integrationInfo.IntegratedEntity.GetTypedColumnValue<Guid>("Id");
				//Контакты из детали "Контакты" контрагента
				var accountContacts = ContactEntityHelper.GetAccountContacts(id, integrationInfo.UserConnection, IntegrationLogger.SimpleLoggerErrorAction);
				//Основной контакт
				var primaryContactId = integrationInfo.IntegratedEntity.GetTypedColumnValue<Guid>("PrimaryContactId");
				if (primaryContactId != Guid.Empty)
				{
					accountContacts.Add(primaryContactId);
				}
				var integrator = new ClientServiceIntegrator(integrationInfo.UserConnection);
				foreach (var contactId in accountContacts)
				{
					integrator.IntegrateBpmEntity(contactId, "Contact", new CounteragentContactInfoHandler());
				}
				return primaryContactId != Guid.Empty;
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e);
			}
			return false;
		}
		private bool isMainContactEmpty(IntegrationInfo integrationInfo)
		{
			if (integrationInfo.Data != null)
			{
				JToken mainContactToken = integrationInfo.Data.SelectToken(JName + ".mainContact.#ref.id");
				if (mainContactToken != null)
				{
					var mainContact = mainContactToken.Value<int>();
					if (mainContact > 0)
					{
						return false;
					}
				}
			}
			return true;
		}
		public override void AddParameterToSearchProcedure(IntegrationInfo integrationInfo, StoredProcedure searchProcedure)
		{
			if (integrationInfo.Data != null)
			{
				if (AdvancedSearchInfo.StoredProcedureName == CsConstant.EntityConst.AccountConst.AccountSearchStoredProcedureB2c)
				{
					var emailPhones = GetEmailPhones(integrationInfo.Data);
					searchProcedure
						.WithParameter("Emails", emailPhones.Item1 ?? string.Empty)
						.WithParameter("Phones", emailPhones.Item2 ?? string.Empty)
						.WithParameter("ExternalIdPath", ExternalIdPath);
				} else if(AdvancedSearchInfo.StoredProcedureName == CsConstant.EntityConst.AccountConst.AccountSearchStoredProcedureB2b)
				{
					var innAndKpp = GetInnAndKpp(integrationInfo.Data);
					searchProcedure
						.WithParameter("Inn", innAndKpp.Item1 ?? string.Empty)
						.WithParameter("Kpp", innAndKpp.Item2 ?? string.Empty)
						.WithParameter("ExternalIdPath", ExternalIdPath);
				}
			}
			
		}

		private Tuple<string, string> GetEmailPhones(JObject data)
		{
			var emails = string.Empty;
			var phone = string.Empty;
			try
			{
				if (data != null)
				{
					var emailToken = data.SelectToken(JName + ".email");
					if (emailToken != null)
					{
						emails = emailToken.Value<string>();
					}
					var phoneToken = data.SelectToken(JName + ".phone");
					if (phoneToken != null)
					{
						phone = string.Join(",", PhoneFormatHelper.ToAllFormats(phoneToken.Value<string>()));
					}
				}
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e);
			}
			return new Tuple<string, string>(emails, phone);
		}
		public override bool IsEntityAlreadyExistAdvanced(IntegrationInfo integrationInfo)
		{
			SetStoredProcedureNameByAccountInfo(integrationInfo);
			var result = base.IsEntityAlreadyExistAdvanced(integrationInfo);
			isFindedAdvanced = result;
			return result;
		}
		public override bool IsEntityAlreadyExist(IntegrationInfo integrationInfo)
		{
			isFindedAdvanced = false;
			return base.IsEntityAlreadyExist(integrationInfo);
		}
		private Tuple<string, string> GetInnAndKpp(JObject data)
		{
			var inn = string.Empty;
			var kpp = string.Empty;
			try
			{
				var innToken = data.SelectToken(JName + ".taxRegistrationNumber");
				var kppToken = data.SelectToken(JName + ".companyRegistrationNumber");
				if (innToken != null)
				{
					inn = innToken.Value<string>();
				}
				if (kppToken != null)
				{
					kpp = kppToken.Value<string>();
				}
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e);
			}
			return new Tuple<string, string>(inn, kpp);
		}
		private void SetStoredProcedureNameByAccountInfo(IntegrationInfo integrationInfo)
		{
			var jObj = integrationInfo.Data;
			if (jObj != null)
			{
				var isB2b = jObj.SelectToken(JName + ".legalEntity").Value<bool>();
				if (isB2b)
				{
					_advancedSearchInfo.StoredProcedureName = CsConstant.EntityConst.AccountConst.AccountSearchStoredProcedureB2b;
					return;
				}
				else
				{
					_advancedSearchInfo.StoredProcedureName = CsConstant.EntityConst.AccountConst.AccountSearchStoredProcedureB2c;
					return;
				}
			}
			_advancedSearchInfo.StoredProcedureName = string.Empty;
		}
	}

	#endregion


	#region Class: AccountBillingInfoHandler
	/*
		Project Path: ..\..\..\QueryConsole\Files\TsIntegrationServiceEntity.TsBase.cs
		
	*/

	[ImportHandlerAttribute("AccountBillingInfo")]
	[ExportHandlerAttribute("AccountBillingInfo")]
	public class AccountBillingInfoHandler : EntityHandler
	{
		public override bool IsEmbeddedObject {
			get {
				return true;
			}
		}
		public override string ParentObjectJName {
			get {
				return "CompanyProfile";
			}
		}
		public override string ParentObjectTsName {
			get {
				return "Account";
			}
		}
		public override bool IsJsonWithHeader {
			get {
				return false;
			}
		}
		public AccountBillingInfoHandler()
		{
			Mapper = new MappingHelper();
			EntityName = "AccountBillingInfo";
			JName = "";
		}
		public override JObject ToJson(IntegrationInfo integrationInfo)
		{
			base.ToJson(integrationInfo);
			if (integrationInfo.Data.First != null && integrationInfo.Data.First.First != null)
			{
				integrationInfo.Data = (JObject)integrationInfo.Data.First.First;
				return integrationInfo.Data;
			}
			return null;
		}
		public override bool IsEntityAlreadyExist(IntegrationInfo integrationInfo)
		{
			return false;
		}
		public override void BeforeMapping(IntegrationInfo integrationInfo)
		{
			if (integrationInfo.IntegrationType == CsConstant.TIntegrationType.Import)
			{
				integrationInfo.Data["accountId"] = integrationInfo.ParentEntity.GetTypedColumnValue<string>("Id");
			}
		}
	}

	#endregion


	#region Class: AccountAnniversaryHandler
	/*
		Project Path: ..\..\..\QueryConsole\Files\TsIntegrationServiceEntity.TsBase.cs
		
	*/


	[ImportHandlerAttribute("AccountAnniversary")]
	[ExportHandlerAttribute("AccountAnniversary")]
	public class AccountAnniversaryHandler : EntityHandler
	{
		public override bool IsEmbeddedObject {
			get {
				return true;
			}
		}
		public override string ParentObjectJName {
			get {
				return "CompanyProfile";
			}
		}
		public override string ParentObjectTsName {
			get {
				return "Account";
			}
		}
		public override bool IsJsonWithHeader {
			get {
				return false;
			}
		}
		public AccountAnniversaryHandler()
		{
			Mapper = new MappingHelper();
			EntityName = "AccountAnniversary";
			JName = "";
		}
		public override JObject ToJson(IntegrationInfo integrationInfo)
		{
			base.ToJson(integrationInfo);
			if (integrationInfo.Data.First != null && integrationInfo.Data.First.First != null)
			{
				integrationInfo.Data = (JObject)integrationInfo.Data.First.First;
				return integrationInfo.Data;
			}
			return null;
		}
		public override bool IsEntityAlreadyExist(IntegrationInfo integrationInfo)
		{
			return false;
		}
		public override void BeforeMapping(IntegrationInfo integrationInfo)
		{
			if (integrationInfo.IntegrationType == CsConstant.TIntegrationType.Import)
			{
				integrationInfo.Data["accountId"] = integrationInfo.ParentEntity.GetTypedColumnValue<string>("Id");
			}
		}

	}

	#endregion


	#region Class: ContactAnniversaryHandler
	/*
		Project Path: ..\..\..\QueryConsole\Files\TsIntegrationServiceEntity.TsBase.cs
		
	*/

	[ImportHandlerAttribute("ContactAnniversary")]
	[ExportHandlerAttribute("ContactAnniversary")]
	public class ContactAnniversaryHandler : EntityHandler
	{
		public override bool IsEmbeddedObject {
			get {
				return true;
			}
		}
		public override string ParentObjectJName {
			get {
				return "PersonProfile";
			}
		}
		public override string ParentObjectTsName {
			get {
				return "Contact";
			}
		}
		public override bool IsJsonWithHeader {
			get {
				return false;
			}
		}
		public ContactAnniversaryHandler()
		{
			Mapper = new MappingHelper();
			EntityName = "ContactAnniversary";
			JName = "";
		}
		public override JObject ToJson(IntegrationInfo integrationInfo)
		{
			base.ToJson(integrationInfo);
			if (integrationInfo.Data.First != null && integrationInfo.Data.First.First != null)
			{
				integrationInfo.Data = (JObject)integrationInfo.Data.First.First;
				return integrationInfo.Data;
			}
			return null;
		}

		public override bool IsEntityAlreadyExist(IntegrationInfo integrationInfo)
		{
			return false;
		}
		public override void BeforeMapping(IntegrationInfo integrationInfo)
		{
			if (integrationInfo.IntegrationType == CsConstant.TIntegrationType.Import)
			{
				integrationInfo.Data["contactId"] = integrationInfo.ParentEntity.GetTypedColumnValue<string>("Id");
			}
		}

		public override void AfterEntitySave(IntegrationInfo integrationInfo)
		{
			base.AfterEntitySave(integrationInfo);

		}

		public override void AfterMapping(IntegrationInfo integrationInfo)
		{
			base.AfterMapping(integrationInfo);
			if (integrationInfo.ParentEntity != null)
			{
				var date = integrationInfo.IntegratedEntity.GetTypedColumnValue<DateTime>("Date");
				integrationInfo.ParentEntity.SetColumnValue("BirthDate", date);
				integrationInfo.ParentEntity.UpdateInDB(false);
			}
		}
	}

	#endregion


	#region Class: AdvancedSearchInfo
	/*
		Project Path: ..\..\..\QueryConsole\Files\AdvancedSearch\AdvancedSearchInfo.cs
		
	*/
	public class AdvancedSearchInfo
	{
		public string StoredProcedureName;
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
				if(result != null && result is Guid)
				{
					return (Guid)result;
				}
				return Guid.Empty;
			} catch(Exception e)
			{
				if(onErrorAction != null)
				{
					onErrorAction(e);
				}
			}
			return Guid.Empty;
		}
	}

	#endregion


	#region Class: AccountEntityHelper
	/*
		Project Path: ..\..\..\QueryConsole\Files\BpmEntityHelper\AccountEntityHelper.cs
		
	*/
	public static class AccountEntityHelper
	{
		public static void ImportInOrderServiceIfNeed(Guid id, UserConnection userConnection)
		{
			try
			{
				var logInfo = LoggerInfo.GetBpmRequestLogInfo(userConnection, CsConstant.PersonName.OrderService, "Account",
					"Counteragent", string.Format("Id = '{{0}}'", id));
				LoggerHelper.DoInTransaction(logInfo, () =>
				{
					var select = new Select(userConnection)
							.Top(1)
							.Column("a", "Id")
							.From("Account").As("a")
							.Where("a", "Id").IsEqual(Column.Parameter(id))
							.And("a", "TsOrderServiceId").IsEqual(Column.Const(0)) as Select;
					using (var dbExecutor = select.UserConnection.EnsureDBConnection())
					{
						using (var reader = select.ExecuteReader(dbExecutor))
						{
							if (reader.Read())
							{
								var integrator = new OrderServiceIntegrator(userConnection);
								integrator.IntegrateBpmEntity(id, "Account", new CounteragentHandler());
							}
						}
					}
				});
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e);
			}
		}
		public static Guid GetLrsByAccountId(Guid id, UserConnection userConnection)
		{
			var select = new Select(userConnection)
						.Top(1)
						.Column("a", "TsSalesMarketId")
						.From("Account").As("a")
						.InnerJoin("Account").As("aLrs")
						.On("aLrs", "Id").IsEqual("a", "TsLrsAccountId")
						.Where("a", "Id").IsEqual(Column.Parameter(id)) as Select;
			return select.ExecuteScalar<Guid>();
		}
		public static void UpdateAddressFromDeliveryService(UserConnection userConnection, Entity accountEntity, Action<Exception> onErrorAction = null, Action<EntitySchemaQuery> addFilterAction = null)
		{
			try
			{
				var addressEsq = new EntitySchemaQuery(userConnection.EntitySchemaManager, "AccountAddress");
				addressEsq.RowCount = 1;
				addressEsq.AddAllSchemaColumns();
				var orderColumn = addressEsq.AddColumn("CreatedOn");
				orderColumn.OrderByDesc();
				addressEsq.Filters.Add(addressEsq.CreateFilterWithParameters(FilterComparisonType.Equal, "Account", accountEntity.GetTypedColumnValue<Guid>("Id")));
				if (addFilterAction != null)
				{
					addFilterAction(addressEsq);
				}
				var address = addressEsq.GetEntityCollection(userConnection);
				if (address != null && address.Any())
				{
					AddressHelper.UpdateAddressFromDeliveryService(userConnection, address.FirstOrDefault(), onErrorAction, true);
				}
			}
			catch (Exception e)
			{
				onErrorAction(e);
			}
		}
		public static void ResaveAccountPrimaryAddress(UserConnection userConnection, Entity accountEntity, Guid accountId, Action<Exception> onException = null)
		{
			try
			{
				var select = new Select(userConnection)
								.Top(1)
								.Column("Address")
								.Column("CountryId")
								.Column("CityId")
								.Column("RegionId")
								.Column("Zip")
								.From("AccountAddress")
								.Where("AccountId").IsEqual(Column.Parameter(accountId))
								.And("Primary").IsEqual(Column.Const(1)) as Select;
				using (var dbExecutor = select.UserConnection.EnsureDBConnection())
				{
					using (var reader = select.ExecuteReader(dbExecutor))
					{
						if (reader.Read())
						{
							accountEntity.SetColumnValue("Address", reader.GetColumnValue<string>("Address"));
							accountEntity.SetColumnValue("CountryId", reader.GetColumnValue<string>("CountryId"));
							accountEntity.SetColumnValue("CityId", reader.GetColumnValue<string>("CityId"));
							accountEntity.SetColumnValue("RegionId", reader.GetColumnValue<string>("RegionId"));
							accountEntity.SetColumnValue("Zip", reader.GetColumnValue<string>("Zip"));
							accountEntity.UpdateInDB(false);
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
		}

		public static void ClearAccountPrimaryCommunication(UserConnection userConnection, Entity contactEntity,
			Action<Exception> onException = null)
		{
			try
			{
				contactEntity.SetColumnValue("Phone", string.Empty);
				contactEntity.SetColumnValue("AdditionalPhone", string.Empty);
				contactEntity.SetColumnValue("Web", string.Empty);
				contactEntity.SetColumnValue("Fax", string.Empty);
				contactEntity.UpdateInDB(false);
			}
			catch (Exception e)
			{
				if (onException != null)
				{
					onException(e);
				}
			}
		}
		public static void SynchronizeCommunication(UserConnection userConnection, Guid accountId, Action<Exception> OnErrorAction = null)
		{
			try
			{
				var storedProcedure = new StoredProcedure(userConnection, "tsp_Integration_SynchronizeAccountCommunication")
					.WithParameter(Column.Parameter(accountId)) as StoredProcedure;
				storedProcedure.PackageName = userConnection.DBEngine.SystemPackageName;
				storedProcedure.Execute();
			}
			catch (Exception e)
			{
				if (OnErrorAction != null)
				{
					OnErrorAction(e);
				}
			}
		}
	}

	#endregion


	#region Class: AddressHelper
	/*
		Project Path: ..\..\..\QueryConsole\Files\BpmEntityHelper\AddressHelper.cs
		
	*/
	public static class AddressHelper
	{
		public static void UpdateAddressFromDeliveryService(UserConnection userConnection, Entity addressEntity, Action<Exception> onErrorAction = null, bool withForceUpdate = false)
		{
			try
			{
				var address = addressEntity.GetTypedColumnValue<string>("Address");
				if (!string.IsNullOrEmpty(address))
				{
					var countryId = addressEntity.GetTypedColumnValue<Guid>("CountryId");
					var regionId = addressEntity.GetTypedColumnValue<Guid>("RegionId");
					var cityId = addressEntity.GetTypedColumnValue<Guid>("CityId");
					var street = addressEntity.GetTypedColumnValue<string>("TsStreet");
					var house = addressEntity.GetTypedColumnValue<string>("TsHouse");
					var areaId = addressEntity.GetTypedColumnValue<Guid>("TsDistrictId");
					var zip = addressEntity.GetTypedColumnValue<string>("Zip");
					if (withForceUpdate || IsOneOfNullOrEmptyString(street, house, zip) || IsOneOfEmptyGuid(countryId, regionId, cityId, areaId))
					{
						var addressSearchProvider = new DeliveryServiceAddressProvider(userConnection, null);
						var addressSearchResult = addressSearchProvider.GetLookupValues(address).FirstOrDefault();
						if (addressSearchResult != null && addressSearchResult.Any())
						{
							//For Strings
							SetNewValueIfNeed(addressEntity, "TsStreet", street, addressSearchResult["street"], withForceUpdate);
							SetNewValueIfNeed(addressEntity, "TsHouse", house, addressSearchResult["house"], withForceUpdate);
							SetNewValueIfNeed(addressEntity, "Zip", zip, addressSearchResult["zipCode"], withForceUpdate);
							//For Guides
							SetNewValueIfNeed(userConnection, addressEntity, "CountryId", countryId, addressSearchResult, "country", withForceUpdate);
							SetNewValueIfNeed(userConnection, addressEntity, "CityId", cityId, addressSearchResult, "settlement", withForceUpdate);
							SetRegionValue(userConnection, addressEntity, "RegionId", regionId, addressSearchResult, "region", withForceUpdate);
							SetNewValueIfNeed(userConnection, addressEntity, "TsDistrictId", areaId, addressSearchResult, "area", withForceUpdate);
							addressEntity.UpdateInDB(false);
						} else
						{
							if (withForceUpdate)
							{
								//For Strings
								addressEntity.SetColumnValue("TsStreet", string.Empty);
								addressEntity.SetColumnValue("TsHouse", string.Empty);
								addressEntity.SetColumnValue("Zip", string.Empty);
								//For Guides
								addressEntity.SetColumnValue("CountryId", null);
								addressEntity.SetColumnValue("CityId", null);
								addressEntity.SetColumnValue("RegionId", null);
								addressEntity.SetColumnValue("TsDistrictId", null);
								addressEntity.UpdateInDB(false);
							}
						}
					}
				}
			} catch(Exception e)
			{
				if(onErrorAction != null)
				{
					onErrorAction(e);
				}
				Terrasoft.Configuration.TsEntityLogger.MethodInfoError("UpdateAddressFromDeliveryService", e.ToString());
			}
		}
		public static bool IsOneOfNullOrEmptyString(params string[] strings)
		{
			return strings.Any(x => string.IsNullOrEmpty(x));
		}
		public static bool IsOneOfEmptyGuid(params Guid[] guides)
		{
			return guides.Any(x => x == Guid.Empty);
		}
		public static void SetNewValueIfNeed(Entity addressEntity, string columnName, string currentValue, string newValue, bool forceUpdate = false)
		{
			if(forceUpdate || string.IsNullOrEmpty(currentValue) && !string.IsNullOrEmpty(newValue))
			{
				addressEntity.SetColumnValue(columnName, newValue);
			}
		}
		public static void SetNewValueIfNeed(UserConnection userConnection, Entity addressEntity, string columnName, Guid currentValue, Dictionary<string, string> addressInfo, string newValueName, bool forceUpdate = false)
		{
			try
			{
				string newValue = string.Empty;
				if (addressInfo.TryGetValue(newValueName, out newValue) && (forceUpdate || currentValue == Guid.Empty) && !string.IsNullOrEmpty(newValue))
				{
					int newExternalId = 0;
					if (addressInfo.ContainsKey(newValueName + "Id"))
					{
						newExternalId = int.Parse(addressInfo[newValueName + "Id"]);
					}
					var columnSchema = addressEntity.Schema.Columns.GetByColumnValueName(columnName);
					if (columnSchema != null)
					{
						var schemaName = columnSchema.ReferenceSchema.Name;
						var displayColumnName = columnSchema.ReferenceSchema.PrimaryDisplayColumn.Name;
						var primaryColumnName = columnSchema.ReferenceSchema.PrimaryColumn.Name;
						var select = new Select(userConnection)
									.Top(1)
									.Column(primaryColumnName)
									.Column("TsExternalId")
									.From(schemaName)
									.Where(displayColumnName).IsEqual(Column.Parameter(newValue)) as Select;
						using (var dbExecutor = select.UserConnection.EnsureDBConnection())
						{
							using (var reader = select.ExecuteReader(dbExecutor))
							{
								if (reader.Read())
								{
									var id = reader.GetColumnValue<Guid>(primaryColumnName);
									var externalId = reader.GetColumnValue<int>("TsExternalId");
									addressEntity.SetColumnValue(columnName, id);
									if (externalId == 0)
									{
										var update = new Update(userConnection, schemaName)
													.Set("TsExternalId", Column.Parameter(newExternalId))
													.Where(primaryColumnName).IsEqual(Column.Parameter(id));
										update.Execute();
									}
									return;
								}
							}
						}
						var resultId = Guid.NewGuid();
						var insert = new Insert(userConnection)
										.Set(primaryColumnName, Column.Parameter(resultId))
										.Set(displayColumnName, Column.Parameter(newValue))
										.Set("TsExternalId", Column.Parameter(newExternalId))
										.Into(schemaName) as Insert;
						insert.Execute();
						addressEntity.SetColumnValue(columnName, resultId);
					}
				} else if(forceUpdate)
				{
					addressEntity.SetColumnValue(columnName, null);
				}
			} catch(Exception e)
			{
				Terrasoft.Configuration.TsEntityLogger.MethodInfoError("SetNewValueIfNeed", string.Format("{0} {1} {2} {3}", columnName, currentValue, newValueName, e.ToString()));
			}
		}

		public static void SetRegionValue(UserConnection userConnection, Entity addressEntity, string columnName,
			Guid currentValue, Dictionary<string, string> addressInfo, string newValueName, bool forceUpdate = false)
		{
			try
			{
				string newValue = string.Empty;
				if (addressInfo.TryGetValue(newValueName, out newValue) && (forceUpdate || currentValue == Guid.Empty) && !string.IsNullOrEmpty(newValue))
				{
					int newExternalId = 0;
					if (addressInfo.ContainsKey(newValueName + "Id"))
					{
						newExternalId = int.Parse(addressInfo[newValueName + "Id"]);
					}
					var columnSchema = addressEntity.Schema.Columns.GetByColumnValueName(columnName);
					if (columnSchema != null)
					{
						var schemaName = columnSchema.ReferenceSchema.Name;
						var displayColumnName = columnSchema.ReferenceSchema.PrimaryDisplayColumn.Name;
						var primaryColumnName = columnSchema.ReferenceSchema.PrimaryColumn.Name;
						Guid timeZoneId = Guid.Empty;
						Dictionary<string, string> regionValue = null;
						var regionProvider = new DeliveryServiceRegionProvider(userConnection, new Dictionary<string, string>()
										{
											{ "id", newExternalId.ToString() }
										});
						var select = new Select(userConnection)
									.Top(1)
									.Column(primaryColumnName)
									.Column("TsExternalId")
									.Column("TsTimeZoneId")
									.From(schemaName)
									.Where(displayColumnName).IsEqual(Column.Parameter(newValue)) as Select;
						using (var dbExecutor = select.UserConnection.EnsureDBConnection())
						{
							using (var reader = select.ExecuteReader(dbExecutor))
							{
								if (reader.Read())
								{
									var id = reader.GetColumnValue<Guid>(primaryColumnName);
									var externalId = reader.GetColumnValue<int>("TsExternalId");
									addressEntity.SetColumnValue(columnName, id);
									if (externalId == 0)
									{
										timeZoneId = reader.GetColumnValue<Guid>("TsTimeZoneId");
										if (timeZoneId == Guid.Empty)
										{
											regionValue = regionProvider.GetLookupValues().FirstOrDefault();
											if (regionValue != null)
											{
												timeZoneId = GetTimeZoneId(userConnection, regionValue["timeZone"]);
											}
										}
										var update = new Update(userConnection, schemaName)
													.Set("TsExternalId", Column.Parameter(newExternalId))
													.Where(primaryColumnName).IsEqual(Column.Parameter(id)) as Update;
										if (timeZoneId != Guid.Empty)
										{
											update.Set("TsTimeZoneId", Column.Parameter(timeZoneId));
										}
										update.Execute();
									}
									return;
								}
							}
						}
						regionValue = regionProvider.GetLookupValues().FirstOrDefault();
						if (regionValue != null)
						{
							timeZoneId = GetTimeZoneId(userConnection, regionValue["timeZone"]);
						}
						var resultId = Guid.NewGuid();
						var insert = new Insert(userConnection)
										.Set(primaryColumnName, Column.Parameter(resultId))
										.Set(displayColumnName, Column.Parameter(newValue))
										.Set("TsExternalId", Column.Parameter(newExternalId))
										.Into(schemaName) as Insert;
						if (timeZoneId != Guid.Empty)
						{
							insert.Set("TsTimeZoneId", Column.Parameter(timeZoneId));
						}
						insert.Execute();
						addressEntity.SetColumnValue(columnName, resultId);
					}
				}
				else if (forceUpdate)
				{
					addressEntity.SetColumnValue(columnName, null);
				}
			}
			catch (Exception e)
			{
				Terrasoft.Configuration.TsEntityLogger.MethodInfoError("SetNewValueIfNeed", string.Format("{0} {1} {2} {3}", columnName, currentValue, newValueName, e.ToString()));
			}
		}

		public static Guid GetTimeZoneId(UserConnection userConnection, string value)
		{
			MapZone windowsZone = TzdbDateTimeZoneSource.Default.WindowsMapping.MapZones.FirstOrDefault(x => x.TzdbIds.Contains(value));
			if (windowsZone != null)
			{
				var windowsZoneId = windowsZone.WindowsId;
				var timeZoneSelect = new Select(userConnection)
							.Column("Id")
							.From("TimeZone")
							.Where("Code").IsEqual(Column.Parameter(windowsZoneId)) as Select;
				return timeZoneSelect.ExecuteScalar<Guid>();
			}
			return Guid.Empty;
		}

	}

	#endregion


	#region Class: ContactEntityHelper
	/*
		Project Path: ..\..\..\QueryConsole\Files\BpmEntityHelper\ContactEntityHelper.cs
		
	*/
	public static class ContactEntityHelper
	{
		public static ContactSgm GetContactSgm(string name, UserConnection userConnection)
		{
			var converter = GetContactConverter(userConnection);
			return converter.GetContactSgm(name);
		}

		public static IContactFieldConverter GetContactConverter(UserConnection userConnection)
		{
			object converterIdValue = Terrasoft.Core.Configuration.SysSettings.GetValue(userConnection, "ContactFieldConverter");
			if (converterIdValue == null || string.IsNullOrEmpty(converterIdValue.ToString()))
			{
				return null;
			}
			Guid converterId = Guid.Parse(converterIdValue.ToString());
			if (converterId == Guid.Empty)
			{
				return null;
			}
			var showNamesByESQ = new EntitySchemaQuery(userConnection.EntitySchemaManager, "ShowNamesBy");
			showNamesByESQ.PrimaryQueryColumn.IsAlwaysSelect = true;
			string convertеrColumnName = showNamesByESQ.AddColumn("Converter").Name;
			string separatorColumnName = showNamesByESQ.AddColumn("Separator").Name;
			showNamesByESQ.Filters.Add(showNamesByESQ.CreateFilterWithParameters(FilterComparisonType.Equal, "Id", converterId));
			EntityCollection showNamesByEntityCollection = showNamesByESQ.GetEntityCollection(userConnection);
			if (showNamesByEntityCollection.Count < 1)
			{
				return null;
			}
			string converterName = showNamesByEntityCollection[0].GetTypedColumnValue<string>(convertеrColumnName);
			if (string.IsNullOrEmpty(converterName))
			{
				return null;
			}
			string separator = showNamesByEntityCollection[0].GetTypedColumnValue<string>(separatorColumnName);
			if (!userConnection.Workspace.IsWorkspaceAssemblyInitialized)
			{
				return null;
			}
			var converter = userConnection.Workspace.WorkspaceAssembly
				.CreateInstance(converterName) as IContactFieldConverter;
			if (converter == null)
			{
				return null;
			}
			if (!string.IsNullOrEmpty(separator))
			{
				converter.Separator = separator.ToCharArray();
			}
			return converter;
		}

		public static void CreateContactByAccount(Entity accountEntity, UserConnection userConnection,
			bool withCommunication = true, bool withAddress = true, Action<Exception> OnErrorAction = null)
		{
			try
			{
				var contactSchema = userConnection.EntitySchemaManager.GetInstanceByName("Contact");
				var contactEntity = contactSchema.CreateEntity(userConnection);
				contactEntity.SetDefColumnValues();
				var id = accountEntity.GetTypedColumnValue<Guid>("Id");
				var contactId = contactEntity.GetTypedColumnValue<Guid>("Id");
				SetContactFieldByAccount(contactEntity, accountEntity, userConnection, false, withCommunication, withAddress, OnErrorAction);
				ReintegrateContactAndAccount(userConnection, contactEntity, accountEntity, OnErrorAction);
			}
			catch (Exception e)
			{
				if (OnErrorAction != null)
				{
					OnErrorAction(e);
				}
			}
		}
		private static void ReintegrateContactAndAccount(UserConnection userConnection, Entity contactEntity, Entity accountEntity, Action<Exception> OnErrorAction)
		{
			try
			{
				var contactId = contactEntity.GetTypedColumnValue<Guid>("Id");
				var id = accountEntity.GetTypedColumnValue<Guid>("Id");
				var integrator = new ClientServiceIntegrator(userConnection);
				//http://tscore-task/browse/SKT-4678
				integrator.IntegrateBpmEntity(id, "Account", new AccountHandler());

				integrator.IntegrateBpmEntity(contactId, "Contact");
				accountEntity.SetColumnValue("PrimaryContactId", contactId);
				accountEntity.UpdateInDB(false);
				//KOSTYL!
				IntegrationLocker.Unlock("Account", accountEntity.GetTypedColumnValue<int>("TsOrderServiceId"),
					"OrderService_Counteragent");
				//KOSTYL!
				integrator.IntegrateBpmEntity(id, "Account", new CounteragentHandler());
				var careerId = CreateContactCareerByAccount(userConnection, contactId, id, OnErrorAction);
				if (careerId != Guid.Empty)
				{
					integrator.IntegrateBpmEntity(careerId, "ContactCareer");
				}
			} catch(Exception e)
			{
				if(OnErrorAction != null)
				{
					OnErrorAction(e);
				}
			}
		}
		public static void SetContactFieldByAccount(Entity contactEntity, Entity accountEntity, UserConnection userConnection, bool updateEntity, bool withCommunication, bool withAddress, Action<Exception> OnErrorAction, bool changeAccount = true)
		{
			try
			{
				var name = accountEntity.GetTypedColumnValue<string>("Name");
				var id = accountEntity.GetTypedColumnValue<Guid>("Id");
				var owner = accountEntity.GetTypedColumnValue<Guid>("OwnerId");
				var lrsId = accountEntity.GetTypedColumnValue<Guid>("TsLrsAccountId");
				var spId = accountEntity.GetTypedColumnValue<Guid>("TsSpAccountId");
				var sgm = ContactEntityHelper.GetContactSgm(name, userConnection);
				contactEntity.SetColumnValue("GivenName", sgm.GivenName);
				contactEntity.SetColumnValue("Surname", sgm.Surname);
				contactEntity.SetColumnValue("MiddleName", sgm.MiddleName);
				contactEntity.SetColumnValue("Name", name);
				contactEntity.SetColumnValue("TsBpB2C", true);
				//http://tscore-task/browse/SKT-3728
				contactEntity.SetColumnWithEmptyCheck("TsLrsAccountId", lrsId);
				contactEntity.SetColumnWithEmptyCheck("TsSpAccountId", spId);
				contactEntity.SetColumnWithEmptyCheck("TypeId", CsConstant.EntityConst.ContactType.Client);
				contactEntity.SetColumnWithEmptyCheck("OwnerId", owner);
				if (changeAccount)
				{
					contactEntity.SetColumnWithEmptyCheck("AccountId", id);
				}
				if (updateEntity)
				{
					contactEntity.UpdateInDB(false);
				}
				else
				{
					contactEntity.InsertToDB(false);
				}
				var contactId = contactEntity.GetTypedColumnValue<Guid>("Id");
				if (withCommunication)
				{
					CreateContactCommunicationByAccount(id, contactId, userConnection, OnErrorAction);
				}
				if (withAddress)
				{
					CreateContactAddressByAccount(id, contactId, userConnection, OnErrorAction);
				}
			} catch(Exception e)
			{
				if(OnErrorAction != null)
				{
					OnErrorAction(e);
				}
			}
		}
		public static void CreateContactCommunicationByAccount(Guid accountId, Guid contactId, UserConnection userConnection,
			Action<Exception> OnErrorAction = null)
		{
			try
			{
				var storedProcedure = new StoredProcedure(userConnection, "tsp_MigrateAccountCommunicationToContact")
					.WithParameter(Column.Parameter(accountId))
					.WithParameter(Column.Parameter(contactId)) as StoredProcedure;
				storedProcedure.PackageName = userConnection.DBEngine.SystemPackageName;
				storedProcedure.Execute();
			}
			catch (Exception e)
			{
				if (OnErrorAction != null)
				{
					OnErrorAction(e);
				}
			}
		}

		public static void CreateContactAddressByAccount(Guid accountId, Guid contactId, UserConnection userConnection,
			Action<Exception> OnErrorAction = null)
		{
			try
			{
				var storedProcedure = new StoredProcedure(userConnection, "tsp_MigrateAccountAddressToContact")
					.WithParameter(Column.Parameter(accountId))
					.WithParameter(Column.Parameter(contactId)) as StoredProcedure;
				storedProcedure.PackageName = userConnection.DBEngine.SystemPackageName;
				storedProcedure.Execute();
			}
			catch (Exception e)
			{
				if (OnErrorAction != null)
				{
					OnErrorAction(e);
				}
			}
		}

		public static List<Guid> GetAccountContacts(Guid accountId, UserConnection userConnection,
			Action<Exception> OnErrorAction = null)
		{
			try
			{
				var result = new List<Guid>();
				var select = new Select(userConnection)
					.Column("Id")
					.From("Contact")
					.Where("AccountId").IsEqual(Column.Parameter(accountId)) as Select;
				using (var dbExecutor = select.UserConnection.EnsureDBConnection())
				{
					using (var reader = select.ExecuteReader(dbExecutor))
					{
						while (reader.Read())
						{
							result.Add(reader.GetColumnValue<Guid>("Id"));
						}
					}
				}
				return result;
			}
			catch (Exception e)
			{
				if (OnErrorAction != null)
				{
					OnErrorAction(e);
				}
				return null;
			}
		}

		public static void ClearAllAddressField(this Entity contactEntity)
		{
			try
			{
				contactEntity.SetColumnValue("AddressTypeId", null);
				contactEntity.SetColumnValue("Address", "");
				contactEntity.SetColumnValue("CityId", null);
				contactEntity.SetColumnValue("RegionId", null);
				contactEntity.SetColumnValue("CountryId", null);
				contactEntity.SetColumnValue("Zip", "");
				contactEntity.UpdateInDB(false);
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e);
			}
		}

		public static void UpdateAddressFromDeliveryService(UserConnection userConnection, Entity contactEntity,
			Action<Exception> onErrorAction = null)
		{
			try
			{
				var addressEsq = new EntitySchemaQuery(userConnection.EntitySchemaManager, "ContactAddress");
				addressEsq.RowCount = 1;
				addressEsq.AddAllSchemaColumns();
				var orderColumn = addressEsq.AddColumn("CreatedOn");
				orderColumn.OrderByDesc();
				addressEsq.Filters.Add(addressEsq.CreateFilterWithParameters(FilterComparisonType.Equal, "Contact",
					contactEntity.GetTypedColumnValue<Guid>("Id")));
				var address = addressEsq.GetEntityCollection(userConnection);
				if (address != null && address.Any())
				{
					AddressHelper.UpdateAddressFromDeliveryService(userConnection, address.FirstOrDefault(), onErrorAction, true);
				}
			}
			catch (Exception e)
			{
				onErrorAction(e);
			}
		}

		public static void ResaveContactPrimaryAddress(UserConnection userConnection, Entity contactEntity, Guid contactId,
			Action<Exception> onException = null)
		{
			try
			{
				var select = new Select(userConnection)
					.Top(1)
					.Column("Address")
					.Column("CountryId")
					.Column("CityId")
					.Column("RegionId")
					.Column("Zip")
					.From("ContactAddress")
					.Where("ContactId").IsEqual(Column.Parameter(contactId))
					.And("Primary").IsEqual(Column.Const(1)) as Select;
				using (var dbExecutor = select.UserConnection.EnsureDBConnection())
				{
					using (var reader = select.ExecuteReader(dbExecutor))
					{
						if (reader.Read())
						{
							contactEntity.SetColumnValue("Address", reader.GetColumnValue<string>("Address"));
							contactEntity.SetColumnValue("CountryId", reader.GetColumnValue<string>("CountryId"));
							contactEntity.SetColumnValue("CityId", reader.GetColumnValue<string>("CityId"));
							contactEntity.SetColumnValue("RegionId", reader.GetColumnValue<string>("RegionId"));
							contactEntity.SetColumnValue("Zip", reader.GetColumnValue<string>("Zip"));
							contactEntity.UpdateInDB(false);
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
		}

		public static void ClearContactPrimaryCommunication(UserConnection userConnection, Entity contactEntity,
			Action<Exception> onException = null)
		{
			try
			{
				contactEntity.SetColumnValue("Phone", string.Empty);
				contactEntity.SetColumnValue("MobilePhone", string.Empty);
				contactEntity.SetColumnValue("HomePhone", string.Empty);
				contactEntity.SetColumnValue("Email", string.Empty);
				contactEntity.SetColumnValue("Skype", string.Empty);
				contactEntity.SetColumnValue("Facebook", string.Empty);
				contactEntity.UpdateInDB(false);
			}
			catch (Exception e)
			{
				if (onException != null)
				{
					onException(e);
				}
			}
		}

		public static void ResaveContactBirthday(UserConnection userConnection, Entity contactEntity,
			Action<Exception> onErrorAction = null)
		{
			try
			{
				var select = new Select(userConnection)
					.Top(1)
					.Column("Id")
					.Column("Date")
					.From("ContactAnniversary")
					.Where("ContactId").IsEqual(Column.Parameter(contactEntity.GetTypedColumnValue<Guid>("Id")))
					.And("AnniversaryTypeId").IsEqual(Column.Parameter(CsConstant.EntityConst.AnniversaryType.BirthDate))
					.OrderByDesc("CreatedOn") as Select;
				using (var dbExecutor = select.UserConnection.EnsureDBConnection())
				{
					using (var reader = select.ExecuteReader(dbExecutor))
					{
						while (reader.Read())
						{
							var birthDate = reader.GetColumnValue<DateTime>("Date");
							contactEntity.SetColumnValue("BirthDate", birthDate);
							contactEntity.UpdateInDB(false);
						}
					}
				}
			}
			catch (Exception e)
			{
				if (onErrorAction != null)
				{
					onErrorAction(e);
				}
			}
		}
		/// <summary>
		/// http://tscore-task/browse/SKT-4138
		/// http://tscore-task/browse/SKT-4707
		/// </summary>
		/// <param name="userConnection"></param>
		/// <param name="contactId"></param>
		/// <param name="accountId"></param>
		/// <param name="onErrorAction"></param>
		/// <returns></returns>
		public static Guid CreateContactCareerByAccount(UserConnection userConnection, Guid contactId, Guid accountId,
			Action<Exception> onErrorAction = null)
		{
			try
			{
				if (contactId == Guid.Empty || accountId == Guid.Empty)
				{
					return Guid.Empty;
				}
				var createCareerIfNeedSp = new StoredProcedure(userConnection,
								"tsp_IntegrationHandler_AppendContactCareer")
							.WithParameter("contactId", contactId)
							.WithParameter("accountId", accountId)
							.WithOutputParameter("resultId", userConnection.DataValueTypeManager.GetInstanceByName("Guid"))
						as StoredProcedure;
				createCareerIfNeedSp.PackageName = userConnection.DBEngine.SystemPackageName;
				createCareerIfNeedSp.Execute();
				return (Guid)createCareerIfNeedSp.Parameters.GetByName("resultId").Value;
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

		public static bool FindContactByAccount(Entity integratedEntity, UserConnection userConnection, Action<StoredProcedure> storedProcAction, Action<Exception> onErrorAction)
		{
			try
			{
				var advancedSearchInfo = new AdvancedSearchInfo()
				{
					StoredProcedureName = CsConstant.EntityConst.ContactConst.ContactSearchStoredProcedure
				};
				var contactId = advancedSearchInfo.Search(userConnection, storedProcAction, onErrorAction);
				if(contactId == Guid.Empty)
				{
					return false;
				}
				var id = integratedEntity.GetTypedColumnValue<Guid>("Id");
				var contactEsq = new EntitySchemaQuery(userConnection.EntitySchemaManager, "Contact");
				contactEsq.AddAllSchemaColumns(true);
				var contactEntity = contactEsq.GetEntity(userConnection, contactId);
				var isUser = IsContactHasUser(userConnection, contactId);
				SetContactFieldByAccount(contactEntity, integratedEntity, userConnection, true, true, true, onErrorAction, !isUser);
				ReintegrateContactAndAccount(userConnection, contactEntity, integratedEntity, onErrorAction);
				return true;
			} catch(Exception e)
			{
				if(onErrorAction != null)
				{
					onErrorAction(e);
				}
			}
			return false;
		}
		public static void SynchronizeCommunication(UserConnection userConnection, Guid contactId, Action<Exception> OnErrorAction = null)
		{
			try
			{
				var storedProcedure = new StoredProcedure(userConnection, "tsp_Integration_SynchronizeContactCommunication")
					.WithParameter(Column.Parameter(contactId)) as StoredProcedure;
				storedProcedure.PackageName = userConnection.DBEngine.SystemPackageName;
				storedProcedure.Execute();
			}
			catch (Exception e)
			{
				if (OnErrorAction != null)
				{
					OnErrorAction(e);
				}
			}
		}

		public static bool IsContactHasUser(UserConnection userConnection, Guid contactId, Action<Exception> OnErrorAction = null)
		{
			try
			{
				int count = 0;
				var select = new Select(userConnection)
							.Top(1)
							.Column("Id")
							.From("SysAdminUnit")
							.Where("SysAdminUnitTypeValue").IsEqual(Column.Const(CsConstant.TSysAdminUnitType.User))
							.And("ContactId").IsEqual(Column.Parameter(contactId)) as Select;
				select.ExecuteReader(dbReader =>
				{
					if(dbReader.GetColumnValue<Guid>("Id") != Guid.Empty)
					{
						count = 1;
					}
				});
				return count > 0;
			}
			catch (Exception e)
			{
				if (OnErrorAction != null)
				{
					OnErrorAction(e);
				}
			}
			return false;
		}
	}

	#endregion


	#region Class: DbExecutorHelper
	/*
		Project Path: ..\..\..\QueryConsole\Files\BpmEntityHelper\DbExecutorHelper.cs
		
	*/
	public static class DbExecutorHelper
	{
		public static void ExecuteSelectWithPaging(this DBExecutor dbExecutor, Select select, int startSkip, int rowCount, string orderColumn, Action<IDataReader> readerAction, Action<Exception> OnErrorAction = null)
		{
			select.Column(Column.Const("[ROWCOUNT]")).As("RowCount");
			var wrapSelect = new Select(select.UserConnection)
					.Column(Column.Asterisk())
					.From(select).As("src") as Select;
			var sqlText = ReplaceRowCount(wrapSelect.GetSqlText(), orderColumn);
			bool isReaderEmpty = false;
			int pageIndex = 0;
			while (!isReaderEmpty)
			{
				var pagingSqlText = WrapWithPaging(sqlText, startSkip + pageIndex * rowCount, rowCount);
				using (var reader = dbExecutor.ExecuteReader(pagingSqlText, select.Parameters) as SqlDataReader)
				{
					if (reader == null)
					{
						return;
					}
					if (!reader.HasRows)
					{
						isReaderEmpty = true;
					}
					try
					{
						readerAction(reader);
					}
					catch (Exception e)
					{
						if (OnErrorAction != null)
						{
							OnErrorAction(e);
						}
					}
				}
				pageIndex++;
			}
		}

		private static string ReplaceRowCount(string sqlText, string orderColumn)
		{
			return sqlText.Replace("N'[ROWCOUNT]'", "row_number() over(order by " + orderColumn + ")");
		}
		private static string WrapWithPaging(string sqlText, int skip, int top)
		{

			return sqlText + string.Format("\nWHERE [src].[RowCount] >= {0} and [src].[RowCount] < {1}\n", skip, skip + top);
		}
	}

	#endregion


	#region Class: EntityHelper
	/*
		Project Path: ..\..\..\QueryConsole\Files\BpmEntityHelper\EntityHelper.cs
		
	*/
	public static class EntityHelper {
		public static void SetColumnWithEmptyCheck(this Entity entity, string columnName, Guid value) {
			if(value != Guid.Empty) {
				entity.SetColumnValue(columnName, value);
			}
		}
		public static int GetExternalIdValue(this Entity entity, string externalIdPath) {
			int result = 0;
			try {
				if(entity.IsColumnValueLoaded(externalIdPath)) {
					result = entity.GetTypedColumnValue<int>(externalIdPath);
				} else {
					var select = new Select(entity.UserConnection)
								.Column(externalIdPath)
								.From(entity.SchemaName)
								.Where(entity.Schema.PrimaryColumn.Name).IsEqual(Column.Parameter(entity.PrimaryColumnValue)) as Select;
					using(var dbExecutor = entity.UserConnection.EnsureDBConnection()) {
						using(var reader = select.ExecuteReader(dbExecutor)) {
							if(reader.Read()) {
								result = DBUtilities.GetColumnValue<int>(reader, externalIdPath);
							}
						}
					}
				}
			} catch(Exception e) {
				IntegrationLogger.Error(e);
			}
			return result;
		}

		public static bool IsExistDuplicateByExternalId(UserConnection userConnection, string entityName, string externalIdPath, int externalId, Action<Exception> onExceptionAction = null)
		{
			try
			{
				var select = new Select(userConnection)
									.Top(1)
									.Column(Func.Count(Column.Asterisk())).As("count")
									.From(entityName)
									//Используем фильтр только по ExternalId, для того чтобы снизить количество чтений, так как существует индекс по полю ExternalId
									.Where(externalIdPath).IsEqual(Column.Parameter(externalId)) as Select;
				using (var dbExecutor = userConnection.EnsureDBConnection())
				{
					using (var reader = select.ExecuteReader(dbExecutor))
					{
						if (reader.Read())
						{
							return reader.GetColumnValue<int>("count") > 1;
						}
					}
				}
			}
			catch (Exception e)
			{
				if(onExceptionAction != null)
				{
					onExceptionAction(e);
				}
			}
			return false;
		}
		public static void ClearDuplicateExternalIdByIds(UserConnection userConnection, string entityName, string primaryColumnName, string externalIdPath, int externalId, Guid primaryColumnValue, Action<Exception> onExceptionAction = null)
		{
			try
			{
				var update = new Update(userConnection, entityName)
								.Set(externalIdPath, Column.Const(0))
								.Where(externalIdPath).IsEqual(Column.Parameter(externalId))
								.And(primaryColumnName).IsNotEqual(Column.Parameter(primaryColumnValue)) as Update;
				update.Execute();
			} catch(Exception e)
			{
				if (onExceptionAction != null)
				{
					onExceptionAction(e);
				}
			}
		}

		public static string GetColumnNameByCommunicationType(Guid communicationType)
		{
			string columnName;
			switch (communicationType.ToString())
			{
				case CommunicationTypeConsts.WebId:
					columnName = "Web";
					break;
				case CommunicationTypeConsts.MainPhoneId:
					columnName = "Phone";
					break;
				case CommunicationTypeConsts.AdditionalPhoneId:
					columnName = "AdditionalPhone";
					break;
				case CommunicationTypeConsts.FaxId:
					columnName = "Fax";
					break;
				default:
					columnName = string.Empty;
					break;
			}
			return columnName;
		}
	}

	#endregion


	#region Class: IntegrationEntityHelper
	/*
		Project Path: ..\..\..\QueryConsole\Files\BpmEntityHelper\IntegrationEntityHelper.cs
		
	*/
	public class IntegrationEntityHelper
	{
		private static List<Type> IntegrationEntityTypes { get; set; }
		private static Dictionary<Type, EntityHandler> EntityHandlers { get; set; }
		public IntegrationEntityHelper()
		{
			EntityHandlers = new Dictionary<Type, EntityHandler>();
		}
		/// <summary>
		/// Експортирует или импортирует объекты в зависимости от настроек
		/// </summary>
		/// <param name="integrationInfo">Настройки интеграции</param>
		public void IntegrateEntity(IntegrationInfo integrationInfo)
		{
			ExecuteHandlerMethod(integrationInfo, GetIntegrationHandler(integrationInfo));
		}
		/// <summary>
		/// В зависимости от типа интеграции возвращает соответсвенный атрибут
		/// </summary>
		/// <param name="integrationInfo">Настройки интеграции</param>
		/// <returns></returns>
		public Type GetAttributeType(IntegrationInfo integrationInfo)
		{
			return GetAttributeType(integrationInfo.IntegrationType);
		}
		public Type GetAttributeType(CsConstant.TIntegrationType integrationType)
		{
			switch (integrationType)
			{
				case CsConstant.TIntegrationType.Import:
					return typeof(ImportHandlerAttribute);
				case CsConstant.TIntegrationType.Export:
				case CsConstant.TIntegrationType.ExportResponseProcess:
					return typeof(ExportHandlerAttribute);
				default:
					return typeof(ExportHandlerAttribute);
			}
		}
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
			var attributeType = GetAttributeType(integrationType);
			var assembly = typeof(IntegrationServiceIntegrator).Assembly;
			return IntegrationEntityTypes = assembly.GetTypes().Where(x =>
			{
				var attributes = x.GetCustomAttributes(attributeType, true);
				return attributes != null && attributes.Length > 0;
			}).ToList();
		}
		/// <summary>
		/// Возвращает объект который отвечает за интеграцию конкретной сущности
		/// </summary>
		/// <param name="integrationInfo">Настройки интеграции</param>
		/// <returns></returns>
		public EntityHandler GetIntegrationHandler(IntegrationInfo integrationInfo) {
			var attributeType = GetAttributeType(integrationInfo);
			var types = GetIntegrationTypes(integrationInfo);
			if (integrationInfo.Handler != null) {
				return integrationInfo.Handler;
			}
			var handlerName = integrationInfo.EntityName;
			foreach (var type in types) {
				var attributes = type.GetCustomAttributes(attributeType, true);

				foreach (IntegrationHandlerAttribute attribute in attributes) {

					if (attribute != null && attribute.EntityName == handlerName) {
						if (EntityHandlers.ContainsKey(type)) {
							return EntityHandlers[type];
						}
						var entityHandler = Activator.CreateInstance(type) as EntityHandler;
						if (EntityHandlers.ContainsKey(type))
						{
							return EntityHandlers[type];
						}
						EntityHandlers.Add(type, entityHandler);
						return entityHandler;
					}
				}
			}
			return null;
		}
		public List<EntityHandler> GetAllIntegrationHandler(string entityName, CsConstant.TIntegrationType integrationType) {
			var result = new List<EntityHandler>();
			var attributeType = GetAttributeType(integrationType);
			var types = GetIntegrationTypes(integrationType);
			foreach (var type in types) {
				var attributes = type.GetCustomAttributes(attributeType, true);

				foreach (IntegrationHandlerAttribute attribute in attributes) {

					if (attribute != null && attribute.EntityName == entityName) {
						if (EntityHandlers.ContainsKey(type))
						{
							result.Add((EntityHandler)EntityHandlers[type]);
						}
						else
						{
							var entityHandler = Activator.CreateInstance(type) as EntityHandler;
							EntityHandlers.Add(type, entityHandler);
							result.Add(entityHandler);
						}
					}
				}
			}
			return result;
		}
		/// <summary>
		/// В зависимости от настройки интеграции, выполняет соответсвенный метод объкта, который отвечает за интеграцию конкретной сущности
		/// </summary>
		/// <param name="integrationInfo">Настройки интеграции</param>
		/// <param name="handler">объект, который отвечает за интеграцию конкретной сущности</param>
		public void ExecuteHandlerMethod(IntegrationInfo integrationInfo, EntityHandler handler)
		{
			if(integrationInfo.Handler == null) {
				integrationInfo.Handler = handler;
			}
			if (handler != null)
			{
				//Id - для уникальной блокировки интеграции. Блокируем по Id, EntityName, ServiceName и JName
				string serviceObjId = "0";
				string entityName = "";
				string jName = "";
				if (integrationInfo.IntegrationType == CsConstant.TIntegrationType.Export || (integrationInfo.IntegrationType == CsConstant.TIntegrationType.ExportResponseProcess && integrationInfo.IntegratedEntity != null)) {
					serviceObjId = integrationInfo.IntegratedEntity.GetExternalIdValue(handler.ExternalIdPath).ToString();
					if(serviceObjId == "0") {
						serviceObjId = integrationInfo.IntegratedEntity.PrimaryColumnValue.ToString();
					}
				} else {
					serviceObjId = integrationInfo.Data.GetJTokenValuePath<string>(handler.JName + ".id");
				}
				if(handler.IsEmbeddedObject) {
					entityName = handler.ParentObjectTsName;
					jName = handler.ParentObjectJName;
				} else {
					entityName = handler.EntityName;
					jName = handler.JName;
				}
				try
				{
					LockerHelper.DoWithEntityLock(serviceObjId, entityName, () => {
						//Export
						if (integrationInfo.IntegrationType == CsConstant.TIntegrationType.Export)
						{
							if(handler.IsExport(integrationInfo)) {
								var result = new CsConstant.IntegrationResult(CsConstant.IntegrationResult.TResultType.Success, handler.ToJson(integrationInfo));
								integrationInfo.Result = result;
							}
							return;
						}
						//Export on Response
						if (integrationInfo.IntegrationType == CsConstant.TIntegrationType.ExportResponseProcess)
						{
							integrationInfo.Action = CsConstant.IntegrationActionName.UpdateFromResponse;
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
					}, IntegrationLogger.SimpleLoggerErrorAction, string.Format("{0}_{1}", handler.ServiceName, jName));
				}
				catch (Exception e)
				{
					IntegrationLogger.Error(e);
				}
			}
		}
	}

	#endregion


	#region Class: PhoneFormatHelper
	/*
		Project Path: ..\..\..\QueryConsole\Files\BpmEntityHelper\PhoneFormatHelper.cs
		
	*/
	public class PhoneFormatHelper
	{
		public static List<Func<string, string>> Formaters = new List<Func<string, string>>() {
			FormaterRemoveFirtPlusSeven
		};
		public static List<string> ToAllFormats(string startPhone)
		{
			var phones = new List<string>()
			{
				startPhone
			};
			Formaters.ForEach(formater => phones.Add(formater(startPhone)));
			return phones;
		}
		public static string FormaterRemoveFirtPlusSeven(string phone)
		{
			string strToken = "+7";
			if(phone != null)
			{
				phone = phone.Trim();
				if(phone.StartsWith(strToken))
				{
					phone = phone.Replace(strToken, "8");
				}
				phone = Regex.Replace(phone, "[^0-9]+", string.Empty);
			}
			return phone;
		}
	}

	#endregion


	#region Class: ProductEntityHelper
	/*
		Project Path: ..\..\..\QueryConsole\Files\BpmEntityHelper\ProductEntityHelper.cs
		
	*/
	public static class ProductEntityHelper {
		public static Guid GetOrCreateProductByBrandAndOem(UserConnection userConnection, string brand, string oem, Action<object> insertOrUpdateAction = null) {
			try {
				var brandParam = Column.Parameter(brand);
				var oemParam = Column.Parameter(oem);
				var resultId = Guid.NewGuid();
				var select = new Select(userConnection)
								.Column("Id")
								.From("Product")
								.Where("Code").IsEqual(oemParam)
								.And("TsTradeMarkName").IsEqual(brandParam) as Select;
				using(var dbExecutor = userConnection.EnsureDBConnection()) {
					using(var reader = select.ExecuteReader(dbExecutor)) {
						if(reader.Read()) {
							resultId = DBUtilities.GetColumnValue<Guid>(reader, "Id");
							if(insertOrUpdateAction != null) {
								var update = new Update(userConnection, "Product")
											.Where("Id").IsEqual(Column.Parameter(resultId)) as Update;
								insertOrUpdateAction(update);
								update.Execute();
							}
							return resultId;
						}
					}
				}

				var insert = new Insert(userConnection)
								.Into("Product")
								.Set("Id", Column.Parameter(resultId))
								.Set("Code", oemParam)
								.Set("Name", Column.Parameter(string.Format("{0} {1}", brand, oem)))
								.Set("TsTradeMarkName", brandParam);
				if (insertOrUpdateAction != null) {
					insertOrUpdateAction(insert);
				}
				insert.Execute();
				return resultId;
			} catch(Exception e) {
				IntegrationLogger.Error(e, string.Format("[GetOrCreateProductByBrandAndOem] param = (brand={0}, oem={1}, userConnection is null = {2})", brand, oem, userConnection == null));
				return Guid.Empty;
			}
		}
	}

	#endregion


	#region Class: CsReference
	/*
		Project Path: ..\..\..\QueryConsole\Files\BpmIntegrator\CsReference.cs
		
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
		Project Path: ..\..\..\QueryConsole\Files\BpmIntegrator\CsReferenceProperty.cs
		
	*/
	public class CsReferenceProperty
	{
		public int id;
		public string type;
		public string name;
	}

	#endregion


	#region Class: IntegrationHandlerAttribute
	/*
		Project Path: ..\..\..\QueryConsole\Files\BpmIntegrator\IntegrationHandlerAttribute.cs
		
	*/
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Method)]
	public class IntegrationHandlerAttribute : System.Attribute
	{
		private string entityName;
		public string EntityName
		{
			get { return entityName; }
		}
		public IntegrationHandlerAttribute(string entityName)
		{
			this.entityName = entityName;
		}
	}

	#endregion


	#region Class: ImportHandlerAttribute
	/*
		Project Path: ..\..\..\QueryConsole\Files\BpmIntegrator\IntegrationHandlerAttribute.cs
		
	*/
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Method, AllowMultiple=true)]
	public class ImportHandlerAttribute : IntegrationHandlerAttribute
	{
		public ImportHandlerAttribute(string entityName)
			: base(entityName)
		{
		}
	}

	#endregion


	#region Class: ExportHandlerAttribute
	/*
		Project Path: ..\..\..\QueryConsole\Files\BpmIntegrator\IntegrationHandlerAttribute.cs
		
	*/
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
	public class ExportHandlerAttribute : IntegrationHandlerAttribute
	{
		public ExportHandlerAttribute(string entityName)
			: base(entityName)
		{
		}
	}

	#endregion


	#region Class: CsConstant
	/*
		Project Path: ..\..\..\QueryConsole\Files\Constants\CsConstant.cs
		
	*/
	public static class CsConstant {
		//IsDebugMode = true только для QueryConsole.
		public static bool IsDebugMode = false;
		public class IntegrationResult {
			public bool Success {
				get;
				set;
			}
			public JObject Data {
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


			public IntegrationResult() {

			}

			public IntegrationResult(JObject data) {
				Data = data;
			}

			public IntegrationResult(TResultType type, JObject data = null) {
				Type = type;
				Data = data;
			}

			public IntegrationResult(TResultException exception, string message = null, JObject data = null) {
				Type = TResultType.Exception;
				Exception = exception;
				ExceptionMessage = message;
				Data = data;
			}


			public enum TResultException {
				OnCreateEntityExist
			}
			public enum TResultType {
				Exception,
				Success
			}

		}


		public class IntegrationInfo {

			public JObject Data {
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
			public string EntityName {
				get;
				set;
			}
			public string Action {
				get;
				set;
			}
			public Guid? EntityIdentifier {
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
			public EntityHandler Handler {
				get;
				set;
			}
			public Entity ParentEntity {
				get;
				set;
			}


			public IntegrationInfo(JObject data, UserConnection userConnection, TIntegrationType integrationType = TIntegrationType.Export,
			Guid? entityIdentifier = null, string entityName = "", string action = "Create", Entity integratedEntity = null) {
				Data = data;
				UserConnection = userConnection;
				IntegrationType = integrationType;
				EntityIdentifier = entityIdentifier;
				EntityName = entityName;
				Action = action;
				IntegratedEntity = integratedEntity;
			}


			public override string ToString() {
				return string.Format("Data = {0}\nIntegrationType={1} EntityIdentifier={2}", Data, IntegrationType, EntityIdentifier);
			}


			public static IntegrationInfo CreateForImport(UserConnection userConnection, string action, string serviceEntityName, JObject data) {
				return new IntegrationInfo(data, userConnection, TIntegrationType.Import, null, serviceEntityName, action, null);
			}
			public static IntegrationInfo CreateForExport(UserConnection userConnection, Entity entity) {
				return new IntegrationInfo(null, userConnection, TIntegrationType.Export, entity.PrimaryColumnValue, entity.SchemaName, CsConstant.IntegrationActionName.Empty, entity);
			}
			public static IntegrationInfo CreateForResponse(UserConnection userConnection, Entity entity) {
				return new IntegrationInfo(null, userConnection, TIntegrationType.ExportResponseProcess, entity.PrimaryColumnValue, entity.SchemaName, CsConstant.IntegrationActionName.UpdateFromResponse, entity);
			}
		}

		public enum TIntegrationType {
			Export = 0,
			Import = 1,
			All = 3,
			ExportResponseProcess = 4
		}
		public enum TSysAdminUnitType {
			Organization = 0,
			Unit = 1,
			Head = 2,
			Team = 3,
			User = 4,
			SelfServicePortalUser = 5,
			FunctionalRole = 6
		}


		public const string clientserviceEntityUrl = "http://api.client-service.stage2.laximo.ru/v2/entity/AUTO3N";
		public const string clientserviceDictUrl = "http://api.client-service.stage2.laximo.ru/v2/dict/AUTO3N";
		public static Dictionary<string, string> clientserviceEntity = new Dictionary<string, string>() {
			{ "Account", "CompanyProfile" },
			{ "Contact", "PersonProfile" },
			{ "ContactCommunication", "ContactRecord" },
			{ "TsAutomobile", "VehicleProfile" },
			{ "SysAdminUnit", "Manager" },
			{ "SysAdminUnit2", "ManagerGroup" },
			{ "Case", "ClientRequest" },
			{ "Relationship", "Relationship" },
			{ "ContactCareer", "Employee" },
			{ "TsContactNotifications", "NotificationProfile" },
			{ "TsAccountNotification", "NotificationProfile" },
			{ "ContactAddress", "AddressInfo" },
			{ "TsAutoTechService", "VehicleRelationship" },
			{ "TsAutoOwnerHistory", "VehicleRelationship" },
			{ "TsAutoOwnerInfo", "VehicleRelationship" },
			{ "TsAutoTechHistory", "VehicleRelationship" },
			{ "TsLocSalMarket", "Market" }
		};
		public static Dictionary<string, string> clientserviceDict = new Dictionary<string, string>() {
			//Dictionary
			{ "RelationType", "RelationshipType" },
			{ "CommunicationType", "ContactRecordType" },
			{ "AddressType", "AddressType" },
			{ "TsSto", "VehicleRelationshipType" }
			//AssortmentRequestStatus - unrecognize
		};

		public static class VehicleRelationshipType {
			public const int Owner = 1;
			public const int Leasing = 2;
			public const int Driver = 3;
			public const int Service = 4;
			public const int Rent = 5;
			public const int Other = 6;
		}

		public const string ContactEntityName = "PersonProfile";
		public const string AccountEntityName = "CompanyProfile";
		public const string RelationshipTypeEntityName = "RelationshipType";
		public const string ManagerEntityName = "Manager";
		public const string RelationshipEntityName = "Relationship";
		public const string ContactRecordEntityName = "ContactRecordType";
		public const string ManagerGroupEntityName = "ManagerGroup";
		public const string AddressTypeEntityName = "AddressType";
		public const string AutomobilePassportEntityName = "VehiclePassport";
		public const string AutomobileRelationshipEntityName = "VehicleRelationship";
		public const string ContactNotificationProfileEntityName = "NotificationProfile";
		public const string AutomobileRelTypeEntityName = "VehicleRelationshipType";

		public static Dictionary<string, string> DefaultBusEventFilters = new Dictionary<string, string>() {
			{"isRead", "false"}
		};
		public static Dictionary<string, string> DefaultBusEventSorts = new Dictionary<string, string>() {
			{"createdAt", Terrasoft.Core.Configuration.SysSettings.GetValue<string>(UserConnection, "IntegrationServiceSortDirection", "asc") }
		};

		public static class IntegrationEventName {
			public const string BusEventNotify = @"BusEventNotification";
			public const string BusEventNotifyData = @"BusEventNotificationData";
		}


		public static class IntegrationActionName {
			public const string Create = @"create";
			public const string Update = @"update";
			public const string Delete = @"delete";
			public const string UpdateFromResponse = @"updateFromResponse";
			public const string Empty = @"";
		}


		public static class SysSettingsCode {
			public const string AllowImport = @"IntegrServAllowImport";
			public const string IntegrationServiceBaseUrl = @"IntegrServBaseUrl";
			public const string TerrasoftPostboxId = @"IntegrServTerrasoftPostboxId";
			public const string NotificationLimit = @"IntegrServBusEventNotificationLimin";
			public const string IsInsertToDB = @"IntegrServInsertToDbWithoutEntityLogic";
			public const string ClientServiceBaseUrl = @"ClientServiceBaseUrl";
			public const string ConfigurationData = @"IntegrationXmlConfigData";
			public const string IsIntegrationActive = @"IsIntegrationActive";
		}


		public static class IntegrationFlagSetting {
			public const bool AllowErrorOnColumnAssign = false;
		}


		public static class ServiceColumnInBpm {
			public const string Identifier = @"TsExternalId";
			public const string IdentifierOrder = @"TsOrderServiceId";
			public const string IdentifierManagerInfo = @"TsManagerInfoId";
			public const string Version = @"TsExternalVersion";
			public const string VersionOrder = @"TsOsVersion";
			public const string VersionManagerInfo = @"TsManagerVersion";
		}


		public static class TsRequestType {
			public static readonly Guid Push = new Guid("bda8d5fb-3c8f-41c6-9823-44615ab20596");
			public static readonly Guid GetResponse = new Guid("173dc5c7-0d32-4512-86b8-e91691b22c19");
		}

		public static class PersonName {
			public const string Bpm = @"Bpm`online";
			public const string ClientService = @"Client Service";
			public const string IntegrationService = @"Integration Service";
			public const string OrderService = @"Order Service";
			public const string Unknown = @"Unknown";
		}
		public static class TsRequestStatus {
			public static readonly Guid Success = new Guid("5a0d25f5-d718-45ab-b4e3-d615ef7e09c6");
			public static readonly Guid Error = new Guid("88c5e88e-410d-4d67-99c3-722d92f93631");
		}

		public static class TsAddressType {
			public static readonly Guid Delivery = new Guid("760bf68c-4b6e-df11-b988-001d60e938c6");
		}
		public static class TsContractState {
			public static readonly Guid Signed = new Guid("1f703f42-f7e8-4e3f-9b54-2b85f62ea507");
			public static readonly Guid Vising = new Guid("ed3a2347-4f14-4ef4-9157-841dd59c0f6a");
		}

		public static class LoggerSettings {
			public static bool IsLoggedStackTrace = Terrasoft.Core.Configuration.SysSettings.GetValue<bool>(UserConnection, "IsLoggedStackTrace", false);
			public static bool IsLoggedDbActive = Terrasoft.Core.Configuration.SysSettings.GetValue<bool>(UserConnection, "IsLoggedDbActive", true);
			public static bool IsLoggedFileActive = Terrasoft.Core.Configuration.SysSettings.GetValue<bool>(UserConnection, "IsLoggedFileActive", true);
		}

		public static class IntegratorSettings {
			public static bool IsIntegrationAsync = Terrasoft.Core.Configuration.SysSettings.GetValue<bool>(UserConnection, "IsIntegrationAsync", false);
			public static bool isLockerActive = Terrasoft.Core.Configuration.SysSettings.GetValue<bool>(UserConnection, "IsLockerActive", true);
			public static Dictionary<TServiceObject, string> GetUrlsByServiceName(string serviceName) {
				var serviceType = Settings.FirstOrDefault(x => x.Value.Name == serviceName);
				if ((object)serviceType != null) {
					return serviceType.Value.BaseUrl;
				}
				return new Dictionary<TServiceObject, string>();
			}
			public static Dictionary<Type, IntegratorSetting> Settings = new Dictionary<Type, IntegratorSetting>() {
				{
					typeof(ClientServiceIntegrator),
					new IntegratorSetting() {
						Auth = Terrasoft.Core.Configuration.SysSettings.GetValue<string>(UserConnection, "ClientServiceAuth", "Basic YnBtb25saW5lOmJwbW9ubGluZQ=="),
						Name = Terrasoft.Core.Configuration.SysSettings.GetValue<string>(UserConnection, "ClientServiceName", "ClientService"),
						BaseUrl = new Dictionary<TServiceObject, string>() {
							{ TServiceObject.Entity, Terrasoft.Core.Configuration.SysSettings.GetValue<string>(UserConnection, "ClientServiceEntityUrl", @"http://api.client-service.stage3.laximo.ru/v2/entity/AUTO3N") },
							{ TServiceObject.Dict, Terrasoft.Core.Configuration.SysSettings.GetValue<string>(UserConnection, "ClientServiceDictUrl", @"http://api.client-service.stage3.laximo.ru/v2/dict/AUTO3N") }
						},
						IsIntegratorActive = Terrasoft.Core.Configuration.SysSettings.GetValue<bool>(UserConnection, "ClientServiceIsActive", false),
						IsDebugMode = Terrasoft.Core.Configuration.SysSettings.GetValue<bool>(UserConnection, "ClientServiceisDebugMode", false)
					}
				},
				{
					typeof(OrderServiceIntegrator),
					new IntegratorSetting() {
						Auth = Terrasoft.Core.Configuration.SysSettings.GetValue<string>(UserConnection, "OrderServiceAuth", "Basic YnBtb25saW5lOmJwbW9ubGluZQ=="),
						Name = Terrasoft.Core.Configuration.SysSettings.GetValue<string>(UserConnection, "OrderServiceName", "OrderService"),
						BaseUrl = new Dictionary<TServiceObject, string>() {
							{
								TServiceObject.Entity,
								Terrasoft.Core.Configuration.SysSettings.GetValue<string>(UserConnection, "OrderServiceEntityUrl", @"http://api.order-service.bus.stage2.auto3n.ru/v2/entity/AUTO3N")
							},
							{
								TServiceObject.Dict,
								Terrasoft.Core.Configuration.SysSettings.GetValue<string>(UserConnection, "OrderServiceDictUrl", @"http://api.order-service.bus.stage2.auto3n.ru/v2/dict/AUTO3N")
							}
						},
						IsIntegratorActive = Terrasoft.Core.Configuration.SysSettings.GetValue<bool>(UserConnection, "OrderServiceIsActive", false),
						//IsIntegratorActive = true,
						IsDebugMode = CsConstant.IsDebugMode ? true : false,//Terrasoft.Core.Configuration.SysSettings.GetValue<bool>(UserConnection, "OrderServiceIsDebugMode", false),
						DebugModeInfo = new DebugModeInfo()
						{
							FilePath = @"../../Files/responseOrderService.json",
							DebugInfoSourceType = CsConstant.IsDebugMode ? "file" : "syssetting",
							SysSettingsCode = "OrderServiceDebugInfo"
						}
					}
				},
				{
					typeof(IntegrationServiceIntegrator),
					new IntegratorIntegrationServiceSetting() {
						Auth = Terrasoft.Core.Configuration.SysSettings.GetValue<string>(UserConnection, "IntegrationServiceAuth", "Basic YnBtb25saW5lOmJwbW9ubGluZQ=="),
						Name = Terrasoft.Core.Configuration.SysSettings.GetValue<string>(UserConnection, "IntegrationServiceName", "IntegrationService"),
						BaseUrl = new Dictionary<TServiceObject, string>() {
							{ TServiceObject.Entity, Terrasoft.Core.Configuration.SysSettings.GetValue<string>(UserConnection, "IntegrationServicePostUrl", @"http://api.integration.bus.stage2.auto3n.ru/v2/entity") }
						},
						PostboxId = Terrasoft.Core.Configuration.SysSettings.GetValue<int>(UserConnection, "IntegrationServicePostId", 10004),
						NotifyLimit = Terrasoft.Core.Configuration.SysSettings.GetValue<int>(UserConnection, "IntegrationServicePostLimit", 50),
						IsIntegratorActive = Terrasoft.Core.Configuration.SysSettings.GetValue<bool>(UserConnection, "IntegrationServiceIsActive", false),
						//IsIntegratorActive = true,
						IsDebugMode = CsConstant.IsDebugMode ? true : Terrasoft.Core.Configuration.SysSettings.GetValue<bool>(UserConnection, "IntegrationServiceIsDebugMode", false),
						//IsDebugMode = true,
						DebugModeInfo = new DebugModeInfo()
						{
							FilePath = @"../../Files/responseIntegrationService.json",
							DebugInfoSourceType = CsConstant.IsDebugMode ? "file" : "syssetting",
							SysSettingsCode = "IntegrationDebugInfo"
						},
						NotifyHierarhicalLevel = Terrasoft.Core.Configuration.SysSettings.GetValue<int>(UserConnection, "IntegrationServiceNotificationHierarhicalLevel", 15)
					}
				},
				{
					typeof(DeliveryServiceIntegrator),
					new IntegratorSetting() {
						Auth = Terrasoft.Core.Configuration.SysSettings.GetValue<string>(UserConnection, "DeliveryServiceAuth", "Basic YnBtb25saW5lOmJwbW9ubGluZQ=="),
						Name = Terrasoft.Core.Configuration.SysSettings.GetValue<string>(UserConnection, "DeliveryServiceName", "DeliveryService"),
						BaseUrl = new Dictionary<TServiceObject, string>() {
							{
								TServiceObject.Entity,
								Terrasoft.Core.Configuration.SysSettings.GetValue<string>(UserConnection, "DeliveryServiceEntityUrl", @"http://api.delivery-service.stage3.laximo.ru/v2/entity/AUTO3N")
							},
							{
								TServiceObject.Service,
								Terrasoft.Core.Configuration.SysSettings.GetValue<string>(UserConnection, "DeliveryServiceServiceUrl", @"http://api.delivery-service.stage3.laximo.ru/v2/service/AUTO3N")
							}
						},
						IsIntegratorActive = Terrasoft.Core.Configuration.SysSettings.GetValue<bool>(UserConnection, "DeliveryServiceIsActive", true),
						IsDebugMode = Terrasoft.Core.Configuration.SysSettings.GetValue<bool>(UserConnection, "DeliveryServiceIsDebugMode", false),
						DebugModeInfo = new DebugModeInfo()
						{
							FilePath = @"../../Files/Debug/response.json"
						}
					}
				}
			};

			public static string MappingConfiguration = Terrasoft.Core.Configuration.SysSettings.GetValue<string>(UserConnection, "IntegrationXmlConfigData", "<?xml version=\"1.0\" encoding=\"utf-8\"?>");
			public static string LdapDomainName = Terrasoft.Core.Configuration.SysSettings.GetValue<string>(UserConnection, "DomainArmtek", "");
			public static int LoadDependentEntityLevel = Terrasoft.Core.Configuration.SysSettings.GetValue<int>(UserConnection, "LoadDependentEntityLevel", 1);
			#region Class: Setting
			public class IntegratorSetting {
				public Dictionary<TServiceObject, string> BaseUrl;
				public string Name;
				public string Auth;
				public bool IsIntegratorActive;
				public bool IsDebugMode;
				public DebugModeInfo DebugModeInfo;
			}
			public class IntegratorIntegrationServiceSetting: IntegratorSetting {
				public int PostboxId;
				public int NotifyLimit;
				public int NotifyHierarhicalLevel;
			}

			public class DebugModeInfo {
				public string SysSettingsCode = @"IntegrationDebugInfo";
				public string FilePath;
				public string DebugInfoSourceType;
				public string GetDebugDataJson() {
					if(string.IsNullOrEmpty(DebugInfoSourceType) || DebugInfoSourceType == "file") {
						return GetFromFile();
					} else if(DebugInfoSourceType == "syssetting") {
						return GetFromSettings();
					}
					return "";
				}
				public string GetFromFile() {
					using (var stream = new StreamReader(new FileStream(FilePath, FileMode.Open))) {
						return stream.ReadToEnd();
					}
				}

				public string GetFromSettings() {
					return Terrasoft.Core.Configuration.SysSettings.GetValue<string>(UserConnection, SysSettingsCode, "");
				}
			}
			#endregion
		}
		private static AppConnection appConnection;
		private static AppConnection AppConnection {
			get {
				if (appConnection == null) {
					if(HttpContext.Current != null && HttpContext.Current.Application["AppConnection"] != null) {
						appConnection = HttpContext.Current.Application["AppConnection"] as AppConnection;
					} else {
						appConnection = new AppConnection();
						Initialize("Default");
					}
				}
				return appConnection;
			}
		}
		private static UserConnection _userConnection;
		public static UserConnection UserConnection {
			get {
				if (_userConnection == null) {
					if(HttpContext.Current != null) {
						_userConnection = (UserConnection)HttpContext.Current.Session["UserConnection"];
					}
					if (_userConnection == null) {
						var systemUserConnection = AppConnection.SystemUserConnection;
						//var autoAuthorization = Terrasoft.Core.Configuration.SysSettings.GetValue<bool>(
						//	systemUserConnection, "ClientSiteIntegrationAutoAuthorization", false);
						var autoAuthorization = false;
						if (autoAuthorization) {
							string userName = (string)Terrasoft.Core.Configuration.SysSettings.GetValue(
							systemUserConnection, "ClientSiteIntegrationUserName");
							if (!string.IsNullOrEmpty(userName)) {
								string userPassword = (string)Terrasoft.Core.Configuration.SysSettings.GetValue(
								systemUserConnection, "ClientSiteIntegrationUserPassword");
								string workspace = AppConnection.SystemUserConnection.Workspace.Name;
								_userConnection = new UserConnection(AppConnection);
								_userConnection.Initialize();
								try {
									_userConnection.Login(userName, userPassword, workspace, TimeZoneInfo.Utc);
								} catch (Exception) {
									_userConnection = null;
								}
							}
						} else {
							_userConnection = systemUserConnection;
						}
					}
				}
				if (_userConnection == null) {
					throw new ArgumentException("Invalid login or password");
				}
				return _userConnection;
			}
			set {
				_userConnection = value;
			}
		}
		private static void Initialize(string workspaceName) {
			AppConfigurationSectionGroup appConfigurationSectionGroup = GetAppSettings();
			var resources = (Terrasoft.Common.ResourceConfigurationSectionGroup)appConfigurationSectionGroup.SectionGroups["resources"];
			GeneralResourceStorage.Initialize(resources);
			var appSettings = (AppConfigurationSectionGroup)appConfigurationSectionGroup;
			Uri assemblyUri = new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
			string path = Path.GetDirectoryName(assemblyUri.LocalPath);
			string appDirectory = Path.GetDirectoryName(path);
			//Server.MapPath
			appSettings.Initialize(appDirectory, Path.Combine(appDirectory, "App_Data"), Path.Combine(appDirectory, "Resources"),
				appDirectory);
			AppConnection.Initialize(appSettings);
			AppConnection.InitializeWorkspace(workspaceName);
		}
		public static AppConfigurationSectionGroup GetAppSettings() {
			System.Configuration.Configuration configuration = null;
			try
			{
				configuration = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~");
			} catch(Exception e)
			{
				Terrasoft.Configuration.TsEntityLogger.Error(e.ToString());
				configuration = System.Configuration.ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
			}
			var appSettings = (AppConfigurationSectionGroup)configuration.SectionGroups["terrasoft"];
			appSettings.RootConfiguration = configuration;
			return appSettings;
		}

		public static class XmlManagerConstant {
			public static readonly string XmlConfigRootNodeName = @"MapingConfiguration";
			public static readonly string XmlConfigEntityConfigNodeName = @"integrationHandlerConfig";
		}
		public static class DeliveryServiceSettings {
			public static Dictionary<Type, DeliveryServiceSettingsValue> Settings = new Dictionary<Type, DeliveryServiceSettingsValue>() {
				{
					typeof(DeliveryServiceEntityProvider),
					new DeliveryServiceSettingsValue() {
						RequestLimit = Terrasoft.Core.Configuration.SysSettings.GetValue<int>(UserConnection, "DeliveryServiceEntityRequestLimit", 5),
						IdFieldName = "id"
					}
				},
				{
					typeof(DeliveryServiceServiceProvider),
					new DeliveryServiceSettingsValue() {
						RequestLimit = Terrasoft.Core.Configuration.SysSettings.GetValue<int>(UserConnection, "DeliveryServiceEntityRequestLimit", 5),
						IdFieldName = "id"
					}
				}
			};
			public class DeliveryServiceSettingsValue {
				public int RequestLimit;
				public string IdFieldName;
			}
		}
		public static class EntityConst {
			public static class ContactType {
				public static Guid Client = new Guid("00783ef6-f36b-1410-a883-16d83cab0980");
			}
			public static class AddressType
			{
				//Юридический
				public static Guid Legal = new Guid("770bf68c-4b6e-df11-b988-001d60e938c6");
				//Фактический
				public static Guid Fact = new Guid("780bf68c-4b6e-df11-b988-001d60e938c6");
				//Рабочий
				public static Guid Work = new Guid("fb7a3f6a-f36b-1410-6f81-1c6f65e50343");
				//Доставки
				public static Guid Delivery = new Guid("760bf68c-4b6e-df11-b988-001d60e938c6");
			}

			public static class AnniversaryType
			{
				public static Guid BirthDate = new Guid("173d56d2-fdca-df11-9b2a-001d60e938c6");
			}

			public static class AccountType
			{
				public static Guid OurCompany = new Guid("57412fad-53e6-df11-971b-001d60e938c6");
			}
			public static class AccountConst
			{
				public static string AccountSearchStoredProcedureB2b = "tsp_Integration_AdvancedSearch_Account_B2b";
				public static string AccountSearchStoredProcedureB2c = "tsp_Integration_AdvancedSearch_Account";
			}
			public static class ContactConst
			{
				public static string ContactSearchStoredProcedure = "tsp_Integration_AdvancedSearch_Contact";
			}
		}
		public static class PrimaryImportProviderConst
		{
			public static bool WithWatchProgress = Terrasoft.Core.Configuration.SysSettings.GetValue<bool>(UserConnection, "PrimaryImportWatchProgress", true);
		}
	}

	#endregion


	#region Class: JTokeExtension
	/*
		Project Path: ..\..\..\QueryConsole\Files\Extension\JTokeExtension.cs
		
	*/
	public static class JTokeExtension {
		public static JToken GetJTokenByPath(this JToken jToken, string path, CsConstant.TIntegrationType type = CsConstant.TIntegrationType.Import) {
			var pItems = path.Split('.');
			foreach (var pItem in pItems) {
				if (!jToken.HasValues || jToken[pItem] == null) {
					if (type != CsConstant.TIntegrationType.Import) {
						jToken[pItem] = new JObject();
					} else {
						return new JObject();
					}
				}
				jToken = jToken[pItem];
			}
			return jToken;
		}
		
		public static JToken GetJTokenValueByPath(this JToken jToken, string path, CsConstant.TIntegrationType type = CsConstant.TIntegrationType.Import)
		{
			var pItems = path.Split('.');
			foreach (var pItem in pItems)
			{
				if(jToken == null)
				{
					return null;
				}
				if(jToken is JObject)
				{
					jToken = jToken[pItem];
				} else if(jToken is JArray)
				{
					jToken = ((JArray)jToken).Last;
					if(jToken != null && jToken is JObject)
					{
						jToken = jToken[pItem];
					}
				}
			}
			return jToken;
		}

		public static T GetJTokenValuePath<T>(this JToken jToken, string path, T defValue = default(T)) {
			var token = jToken.SelectToken(path);
			if(token == null || string.IsNullOrEmpty(jToken.SelectToken(path).Value<string>()))
			{
				return defValue;
			}
			return token.Value<T>();
		}

		public static bool IsJTokenPathHasValue(this JToken jToken, string path) {
			var token = jToken.SelectToken(path);
			return token != null && !string.IsNullOrEmpty(token.Value<string>());
		}

		public static void RemoveByPath(this JToken jToken, string path) {
			var token = jToken.SelectToken(path);
			if(token != null) {
				token.Parent.Remove();
			}
		}
	}

	#endregion


	#region Class: ObjectExtension
	/*
		Project Path: ..\..\..\QueryConsole\Files\Extension\ObjectExstension.cs
		
	*/
	public static class ObjectExtension {
		public static object CloneObject(this object objSource) {
			Type typeSource = objSource.GetType();
			object objTarget = Activator.CreateInstance(typeSource);

			PropertyInfo[] propertyInfo = typeSource.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

			foreach (PropertyInfo property in propertyInfo) {
				if (property.CanWrite) {
					if (property.PropertyType.IsValueType || property.PropertyType.IsEnum || property.PropertyType.Equals(typeof(System.String))) {
						property.SetValue(objTarget, property.GetValue(objSource, null), null);
					} else {
						object objPropertyValue = property.GetValue(objSource, null);
						if (objPropertyValue == null) {
							property.SetValue(objTarget, null, null);
						} else {
							property.SetValue(objTarget, objPropertyValue.CloneObject(), null);
						}
					}
				}
			}
			return objTarget;
		}
	}

	#endregion


	#region Class: DependentEntityLoader
	/*
		Project Path: ..\..\..\QueryConsole\Files\IntegratorHelper\DependentEntityLoader.cs
		
	*/
	public static class DependentEntityLoader
	{
		public static ConcurrentDictionary<int, int> ThreadLoadEntityLevel = new ConcurrentDictionary<int, int>();
		public static int CurrentThreadId
		{
			get
			{
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
						var serviceRequestInfo = ServiceRequestInfo.CreateForExportInBpm(name);
						serviceRequestInfo.Limit = "1";
						serviceRequestInfo.ServiceObjectId = externalId.ToString();
						serviceRequestInfo.UpdateIfExist = true;
						serviceRequestInfo.ReupdateUrl = true;
						var integrator = IntegratorBuilder.Build(name, userConnection);
						if (integrator != null)
						{
							integrator.GetRequest(serviceRequestInfo);
						}
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
			return level <= CsConstant.IntegratorSettings.LoadDependentEntityLevel;
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


	#region Class: IntegratorHelper
	/*
		Project Path: ..\..\..\QueryConsole\Files\IntegratorHelper\IntegratorHelper.cs
		
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
			 Action<string, UserConnection> errorCallback = null, string auth = null, bool isAsync = true)
		{
			if (string.IsNullOrEmpty(url))
			{
				return;
			}
			Console.WriteLine(url);
			IntegrationLogger.Info(new RequestLoggerInfo()
			{
				RequestMethod = requestMethod,
				Url = url,
				JsonText = jsonText
			});
			MakeAsyncRequest(requestMethod, url, jsonText, callback, userConnection, errorCallback, auth);
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
							IntegrationLogger.Info(new ResponseLoggerInfo()
							{
								ResponseText = responceText
							});
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
						Console.WriteLine(responceText);
						IntegrationLogger.Info(new RequestErrorLoggerInfo()
						{
							Exception = e,
							ResponseText = responceText,
							ResponseJson = jsonText
						});
						if (errorCallback != null)
						{
							errorCallback(responceText, userConnection);
						}
					}
				}
			}
			catch (Exception e)
			{
				IntegrationLogger.Info(new RequestErrorLoggerInfo()
				{
					Exception = e,
					ResponseText = "Ошибка в формировании запроса"
				});
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


	#region Class: DeliveryServiceIntegrator
	/*
		Project Path: ..\..\..\QueryConsole\Files\Integrators\DeliveryServiceIntegrator.cs
		
	*/
	public class DeliveryServiceIntegrator {
		public UserConnection userConnection;
		public IntegratorHelper integratorHelper;
		public ServiceUrlMaker urlMaker;
		private CsConstant.IntegratorSettings.IntegratorSetting _Settings;
		public CsConstant.IntegratorSettings.IntegratorSetting Settings {
			get {
				if (_Settings == null) {
					_Settings = CsConstant.IntegratorSettings.Settings[GetType()];
				}
				return _Settings;
			}
		}
		public DeliveryServiceIntegrator(UserConnection userConnection) {
			this.userConnection = userConnection;
			integratorHelper = new IntegratorHelper();
			urlMaker = new ServiceUrlMaker(Settings.BaseUrl);
		}

		public void PushGetEntityRequest(string entityName, int limit, string query, string filterFieldName, Dictionary<string, string> filters, Action<string, UserConnection> callback, Action<string, UserConnection> errorCallback) {
			if(!Settings.IsIntegratorActive) {
				return;
			}
			string filterStr = string.Empty;
			string url = string.Empty;
			if (filters != null && filters.ContainsKey("id"))
			{
				url = urlMaker.Make(TServiceObject.Entity, entityName, filters["id"], null, TRequstMethod.GET, null, null);
			}
			else
			{
				filterStr = string.Format("q[{0}]={{\"$like\":\"{1}%\"}}&sort[{0}]=asc", filterFieldName, query);
				if (filters != null && filters.Any())
				{
					filterStr += "&" + filters.Select(x => x.Key + "=" + x.Value).Aggregate((x, y) => x + "&" + y);
				}
				url = urlMaker.Make(TServiceObject.Entity, entityName, null, filterStr, TRequstMethod.GET, limit.ToString(), null);
			}
			
			integratorHelper.PushRequest(TRequstMethod.GET, url, null, callback, userConnection, errorCallback, Settings.Auth, false);
		}

		public void PushGetServiceRequest(string methodName, string query, Dictionary<string, string> filters, Action<string, UserConnection> callback, Action<string, UserConnection> errorCallback) {
			if (!Settings.IsIntegratorActive) {
				return;
			}
			if (!string.IsNullOrEmpty(query)) {
				filters.Add("query", query);
			}
			string filterStr = string.Empty;
			if (filters != null && filters.Any()) {
				filterStr = filters.Select(x => x.Key + "=" + x.Value).Aggregate((x, y) => x + "&" + y);
			}
			string url = urlMaker.Make(TServiceObject.Service, methodName, null, filterStr, TRequstMethod.GET, null, null);
			integratorHelper.PushRequest(TRequstMethod.GET, url, null, callback, userConnection, errorCallback, Settings.Auth, false);
		}
	}

	#endregion


	#region Class: IntegrationServiceIntegrator
	/*
		Project Path: ..\..\..\QueryConsole\Files\Integrators\IntegrationServiceIntegrator.cs
		
	*/
	public class IntegrationServiceIntegrator {
		private UserConnection _userConnection;
		private List<string> ReadedNotificationIds = new List<string>();
		private IntegratorHelper _integratorHelper = new IntegratorHelper();
		private IntegrationEntityHelper _integrationEntityHelper;
		private string _basePostboxUrl {
			get {
				return Settings.BaseUrl[TServiceObject.Entity];
			}
		}
		private string _baseClientServiceUrl;
		private int _postboxId {
			get {
				return Settings.PostboxId;
			}
		}
		private int _notifyLimit {
			get {
				return Settings.NotifyLimit;
			}
		}
		private bool _isIntegratorActive {
			get {
				return Settings.IsIntegratorActive;
			}
		}
		private string _auth {
			get {
				return Settings.Auth;
			}
		}
		private CsConstant.IntegratorSettings.IntegratorIntegrationServiceSetting _Settings;
		public CsConstant.IntegratorSettings.IntegratorIntegrationServiceSetting Settings {
			get {
				if (_Settings == null) {
					_Settings = (CsConstant.IntegratorSettings.IntegratorIntegrationServiceSetting)CsConstant.IntegratorSettings.Settings[this.GetType()];
				}
				return _Settings;
			}
		}

		public UserConnection UserConnection {
			get {
				return _userConnection;
			}
		}
		public IntegrationEntityHelper IntegrationEntityHelper {
			get {
				return _integrationEntityHelper;
			}
		}


		public IntegrationServiceIntegrator(UserConnection userConnection) {
			_userConnection = userConnection;
			_integrationEntityHelper = new IntegrationEntityHelper();
		}


		/// <summary>
		/// Получает BusEventNotification, после чего вызывает OnBusEventNotificationsDataRecived
		/// </summary>
		/// <param name="withData"></param>
		public void GetBusEventNotification(bool withData = true, int level = 0) {
			if (!_isIntegratorActive)
			{
				return;
			}
			bool isNotifyEmpty = true;
			LockerHelper.DoWithEntityLock(0, "GetBusEventNotification", () =>
			{
				var url = GenerateUrl(
					withData == true ? TIntegratorRequest.BusEventNotificationData : TIntegratorRequest.BusEventNotification,
					TRequstMethod.GET,
					"0",
					_notifyLimit.ToString(),
					CsConstant.DefaultBusEventFilters,
					CsConstant.DefaultBusEventSorts
				);
				if (Settings.IsDebugMode)
				{
					var json = Settings.DebugModeInfo.GetDebugDataJson();
					var responceObj = json.DeserializeJson();
					var busEventNotifications = (JArray)responceObj["data"];
					if (busEventNotifications != null && busEventNotifications.Any())
					{
						OnBusEventNotificationsDataRecived(busEventNotifications, UserConnection);
					}
				}
				else
				{
					PushRequestWrapper(TRequstMethod.GET, url, "", (x, y) => {
						var responceObj = x.DeserializeJson();
						var busEventNotifications = (JArray)responceObj["data"];
						if (busEventNotifications != null && busEventNotifications.Any())
						{
							isNotifyEmpty = false;
							OnBusEventNotificationsDataRecived(busEventNotifications, y);
						}
					});
				}
			}, IntegrationLogger.SimpleLoggerErrorAction, "IntegrationService");
			if (!isNotifyEmpty && level < Settings.NotifyHierarhicalLevel)
			{
				GetBusEventNotification(true, ++level);
			}
		}
		public void IniciateLoadChanges()
		{
			var logInfo = LoggerInfo.GetNotifyRequestLogInfo(UserConnection);
			LoggerHelper.DoInTransaction(logInfo, () =>
			{
				GetBusEventNotification(true);
			});
		}
		/// <summary>
		/// Всем нотификейшенам в ReadedNotificationIds ставит статус "Прочитано"
		/// </summary>
		public void SetNotifyRead() {
			if(Settings.IsDebugMode || !ReadedNotificationIds.Any()) {
				return;
			}
			try
			{
				var url = GenerateUrl(
					TIntegratorRequest.BusEventNotification,
					TRequstMethod.PUT
				);

				var json = ReadedNotificationIds.Select(x => new {
					isRead = true,
					id = x
				}).SerializeToJson();
				PushRequestWrapper(TRequstMethod.PUT, url, json, null);
				ReadedNotificationIds.Clear();
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e);
			}
		}

		/// <summary>
		/// Сохраняет нотификейшен, чтобы потом скопом поставить признак прочитано
		/// </summary>
		/// <param name="notifyId"></param>
		public void AddReadId(string notifyId) {
			ReadedNotificationIds.Add(notifyId);
		}

		/// <summary>
		/// Делает запрос в clientservice и если версия объекта в нем больше за версию в integrationservice, то обновляет объектом из clientservice
		/// </summary>
		/// <param name="integrationInfo"></param>
		public void CreatedOnEntityExist(IntegrationInfo integrationInfo) {
			integrationInfo.Action = CsConstant.IntegrationActionName.Update;
			_integrationEntityHelper.IntegrateEntity(integrationInfo);
		}

		/// <summary>
		/// Срабатывает на получение нотификейшенов из integrationservice
		/// </summary>
		/// <param name="busEventNotifications"></param>
		/// <param name="userConnection"></param>
		public void OnBusEventNotificationsDataRecived(JArray busEventNotifications, UserConnection userConnection) {
			foreach (JObject busEventNotify in busEventNotifications) {
				var busEvent = busEventNotify[CsConstant.IntegrationEventName.BusEventNotify] as JObject;
				if (busEvent != null) {
					var data = busEvent["data"] as JObject;
					var objectType = busEvent["objectType"].ToString();
					var action = busEvent["action"].ToString();
					var system = busEvent["system"].ToString();
					var notifyId = busEvent["id"].ToString();
					var objectId = busEvent["objectId"].Value<int>();
					if (!string.IsNullOrEmpty(objectType) && data != null) {
						IntegrateServiceEntity(data, objectType);
					} else {
						if (system == CsConstant.IntegratorSettings.Settings[typeof(OrderServiceIntegrator)].Name) {
							var integrator = new OrderServiceIntegrator(userConnection);
							ExportServiceEntity(integrator, objectType, objectId);
						}
					}
					AddReadId(notifyId);
				}
			}
			SetNotifyRead();
		}
		public void IntegrateFromOrderService()
		{

		}

		public void ExportServiceEntity(BaseServiceIntegrator integrator, string name, int id, Action afterIntegrate = null)
		{
			var serviceRequestInfo = ServiceRequestInfo.CreateForExportInBpm(name);
			serviceRequestInfo.Limit = "1";
			serviceRequestInfo.Skip = "0";
			serviceRequestInfo.ServiceObjectId = id.ToString();
			serviceRequestInfo.AfterIntegrate = afterIntegrate;
			integrator.GetRequest(serviceRequestInfo);
		}
		public virtual void IntegrateServiceEntity(JObject serviceEntity, string serviceObjectName)
		{
			var integrationInfo = CsConstant.IntegrationInfo.CreateForImport(UserConnection, CsConstant.IntegrationActionName.Create, serviceObjectName, serviceEntity);
			IntegrationEntityHelper.IntegrateEntity(integrationInfo);
			if (integrationInfo.Result != null && integrationInfo.Result.Exception == CsConstant.IntegrationResult.TResultException.OnCreateEntityExist)
			{
				CreatedOnEntityExist(integrationInfo);
			}
		}
		/// <summary>
		/// Генерирует Url в integrationservice
		/// </summary>
		/// <param name="integratorRequestType">Тип возращаемой сущности</param>
		/// <param name="requstMethod">Тип запроса</param>
		/// <param name="skip">Сколько данныех пропустить от начала</param>
		/// <param name="limit">Сколько данных взять</param>
		/// <param name="filters">Фильтры</param>
		/// <param name="sorts">Сортировки</param>
		/// <returns></returns>
		public string GenerateUrl(TIntegratorRequest integratorRequestType, TRequstMethod requstMethod, string skip = null, string limit = null, Dictionary<string, string> filters = null, Dictionary<string, string> sorts = null) {
			string result = _basePostboxUrl;
			string filtersStr = "";
			string sortStr = "";
			string skipStr = "";
			string limitStr = "";
			//createdAt
			switch (requstMethod) {
				case TRequstMethod.GET:
				case TRequstMethod.PUT:

				break;
				default:
				throw new NotImplementedException();
			}


			switch (integratorRequestType) {
				case TIntegratorRequest.BusEventNotification:
				result += GenerateRouteToRequest("Postbox", _postboxId, "BusEventNotification");
				break;
				case TIntegratorRequest.BusEventNotificationData:
				result += GenerateRouteToRequest("Postbox", _postboxId, "BusEventNotificationData");
				break;
				case TIntegratorRequest.Postbox:
				result += GenerateRouteToRequest("Postbox");
				break;
			}


			if (!string.IsNullOrEmpty(skip))
				skipStr = string.Format("skip={0}", skip);


			if (!string.IsNullOrEmpty(limit))
				limitStr = string.Format("limit={0}", limit);


			if (filters != null && filters.Any()) {
				foreach (var filter in filters) {
					filtersStr += string.Format("q[{0}]={1}&", filter.Key, filter.Value);
				}
				filtersStr = filtersStr.Remove(filtersStr.Length - 1);
			}


			if (sorts != null && sorts.Any()) {
				foreach (var sort in sorts) {
					sortStr += string.Format("sort[{0}]={1}&", sort.Key, sort.Value);
				}
				sortStr = sortStr.Remove(sortStr.Length - 1);
			}


			string paramStr = GenerateParamRoRequest(skipStr, limitStr, filtersStr, sortStr);
			if (!string.IsNullOrEmpty(paramStr)) {
				result += string.Format("?{0}", paramStr);
			}

			return result;
		}


		private string GenerateRouteToRequest(params object[] routes) {
			return "/" + routes.Aggregate((cur, next) => cur.ToString() + "/" + next.ToString()).ToString();
		}
		private string GenerateParamRoRequest(params string[] param) {
			var collection = param.Where(x => !string.IsNullOrEmpty(x));
			return collection.Any() ? collection.Aggregate((cur, next) => cur + "&" + next) : "";
		}
		private void PushRequestWrapper(TRequstMethod requestMethod, string url, string jsonText, Action<string, UserConnection> callback) {
			if (!_isIntegratorActive) {
				return;
			}
			_integratorHelper.PushRequest(requestMethod, url, jsonText, callback, UserConnection, null, _auth);
		}


		public enum TIntegratorRequest {
			BusEventNotificationData,
			BusEventNotification,
			Postbox
		}

	}

	#endregion


	#region Class: IntegratorBuilder
	/*
		Project Path: ..\..\..\QueryConsole\Files\Integrators\IntegratorBuilder.cs
		
	*/
	public static class IntegratorBuilder
	{
		public static BaseServiceIntegrator Build(string serviceObjName, UserConnection userConnection)
		{
			IntegrationPath serviceIntegrationPath = IntegrationConfigurationManager.IntegrationPathConfig.Paths.FirstOrDefault(x => x.Name == serviceObjName);
			if(serviceIntegrationPath != null && !string.IsNullOrEmpty(serviceIntegrationPath.ServiceName))
			{
				string serviceName = serviceIntegrationPath.ServiceName;
				switch(serviceName)
				{
					case "OrderService":
						return new OrderServiceIntegrator(userConnection);
					case "ClientService":
						return new ClientServiceIntegrator(userConnection);
				}
			}
			return null;
		}
	}

	#endregion


	#region Class: ClientServiceIntegrator
	/*
		Project Path: ..\..\..\QueryConsole\Files\Integrators\NewClientServiceIntegrator.cs
		
	*/
	public class ClientServiceIntegrator : BaseServiceIntegrator
	{
		public ClientServiceIntegrator(UserConnection userConnection)
			: base(userConnection)
		{}
	}

	#endregion


	#region Class: OrderServiceIntegrator
	/*
		Project Path: ..\..\..\QueryConsole\Files\Integrators\OrderServiceIntegrator.cs
		
	*/
	public class OrderServiceIntegrator : BaseServiceIntegrator
	{
		public OrderServiceIntegrator(UserConnection userConnection)
			: base(userConnection)
		{
		}
	}

	#endregion


	#region Class: ServiceRequestInfo
	/*
		Project Path: ..\..\..\QueryConsole\Files\Integrators\TsServiceIntegrator.cs
		
	*/

	public class ServiceRequestInfo {
		public TServiceObject Type;
		public TRequstMethod Method;
		public string FullUrl;
		public string ServiceObjectName;
		public string ServiceObjectId;
		public string Filters;
		public string RequestJson;
		public string ResponseData;
		public Guid LogId;
		public Entity Entity;
		public string Limit;
		public string Skip;
		public Action AfterIntegrate;
		public EntityHandler Handler;
		public bool UpdateIfExist = true;
		public Action<int, int> ProgressAction;
		public int IntegrateCount = 0;
		public int TotalCount = 0;
		public bool ReupdateUrl = false;
		public string SortField = null;
		public string SortDirection = "asc";
		public static ServiceRequestInfo CreateForExportInBpm(string serviceObjectName, TServiceObject type = TServiceObject.Entity) {
			return new ServiceRequestInfo() {
				ServiceObjectName = serviceObjectName,
				Type = type
			};
		}

		public void SetProgress(int progressed, int allCount)
		{
			if(ProgressAction != null)
			{
				ProgressAction(progressed, allCount);
			}
		}
	}

	#endregion


	#region Class: BaseServiceIntegrator
	/*
		Project Path: ..\..\..\QueryConsole\Files\Integrators\TsServiceIntegrator.cs
		
	*/

	public abstract class BaseServiceIntegrator: IServiceIntegrator {
		public IntegratorHelper integratorHelper;
		public UserConnection userConnection;
		public ServiceUrlMaker UrlMaker;
		public IntegrationEntityHelper entityHelper;

		#region settings
		private CsConstant.IntegratorSettings.IntegratorSetting _Settings;
		public CsConstant.IntegratorSettings.IntegratorSetting Settings {
			get {
				if(_Settings == null) {
					_Settings = CsConstant.IntegratorSettings.Settings[this.GetType()];
				}
				return _Settings;
			}
		}
		public bool IsIntegratorActive {
			get {
				return Settings.IsIntegratorActive;
			}
		}
		public bool IsDebugMode
		{
			get
			{
				return Settings.IsDebugMode;
			}
		}
		public CsConstant.IntegratorSettings.DebugModeInfo DebugModeInfo
		{
			get
			{
				return Settings.DebugModeInfo;
			}
		}
		public string ServiceName {
			get {
				return Settings.Name;
			}
		}
		public string Auth {
			get {
				return Settings.Auth;
			}
		}
		public Dictionary<TServiceObject, string> baseUrls {
			get {
				return Settings.BaseUrl;
			}
		}
		#endregion

		public BaseServiceIntegrator(UserConnection userConnection) {
			this.userConnection = userConnection;
			entityHelper = new IntegrationEntityHelper();
			integratorHelper = new IntegratorHelper();
			UrlMaker = new ServiceUrlMaker(baseUrls);
		}

		public virtual void GetRequest(ServiceRequestInfo info)
		{
			if (!IsIntegratorActive) {
				return;
			}
			info.Method = TRequstMethod.GET;
			if (info.ReupdateUrl)
			{
				info.FullUrl = UrlMaker.Make(info);
			} else
			{
				info.FullUrl = info.FullUrl ?? UrlMaker.Make(info);
			}
			MakeRequest(info);
		}

		public virtual void MakeRequest(ServiceRequestInfo info)
		{
			if(!IsIntegratorActive) {
				return;
			}
			if (IsDebugMode)
			{
				info.ResponseData = DebugModeInfo.GetDebugDataJson();
				OnGetResponse(info);
			}
			else
			{
				integratorHelper.PushRequest(info.Method, info.FullUrl, info.RequestJson, (x, y) =>
				{
					info.ResponseData = x;
					OnGetResponse(info);
				}, userConnection,
				(x, y) =>
				{
					if (info.AfterIntegrate != null)
					{
						info.AfterIntegrate();
					}
				}, Auth);
			}
		}

		public virtual void OnGetResponse(ServiceRequestInfo info)
		{
			if (!IsIntegratorActive) {
				return;
			}
			var responseJObj = JObject.Parse(info.ResponseData);
			switch(info.Method) {
				case TRequstMethod.GET:
					try {
						IEnumerable<JObject> resultObjects;
						if (string.IsNullOrEmpty(info.ServiceObjectId) || info.ServiceObjectId == "0")
						{
							var objArray = responseJObj["data"] as JArray;
							info.TotalCount = responseJObj.Value<int>("total");
							resultObjects = objArray.Select(x => x as JObject);
						}
						else
						{
							resultObjects = new List<JObject>() {
								responseJObj
							};
							info.TotalCount = 1;
						}
						foreach (var jObj in resultObjects)
						{
							try
							{
								IntegrateServiceEntity(jObj, info.ServiceObjectName, info.UpdateIfExist);
								info.SetProgress(++info.IntegrateCount, info.TotalCount);
							} catch(Exception e)
							{
								Terrasoft.Configuration.TsEntityLogger.MethodInfoError("OnGetResponse - foreach", e.ToString(), jObj.ToString());
							}
						}
					} catch(Exception e) {
						IntegrationLogger.Error(e, "OnGetResponse");
					}
					if(info.AfterIntegrate != null) {
						info.AfterIntegrate();
					}
				break;
				case TRequstMethod.POST:
				case TRequstMethod.PUT:
					var integrationInfo = CsConstant.IntegrationInfo.CreateForResponse(userConnection, info.Entity);
					integrationInfo.StrData = responseJObj.ToString();
					integrationInfo.Handler = info.Handler;
					integrationInfo.Action = info.Method == TRequstMethod.POST ? CsConstant.IntegrationActionName.Create : CsConstant.IntegrationActionName.Update;
					entityHelper.IntegrateEntity(integrationInfo);
					Console.WriteLine("Ok");
				break;
			}
			
		}

		public virtual void IntegrateBpmEntity(Entity entity, EntityHandler defHandler = null, bool withLock = true) {
			IntegrateBpmEntity(entity.PrimaryColumnValue, entity.SchemaName, defHandler, withLock);
		}

		public virtual void IntegrateBpmEntity(Guid entityId, string schemaName, EntityHandler defHandler = null, bool withLock = true) {
			if (!IsIntegratorActive) {
				return;
			}
			try {
				LockerHelper.DoWithEntityLock(entityId, schemaName, () => {
					Entity entity = PrepareEntity(userConnection, schemaName, entityId);
					if(entity == null) {
						return;
					}
					var handlers = defHandler == null ? entityHelper.GetAllIntegrationHandler(entity.SchemaName, CsConstant.TIntegrationType.Export) : new List<EntityHandler>() { defHandler };
					foreach (var handler in handlers) {
						var logInfo = new LoggerInfo()
						{
							UserConnection = userConnection,
							RequesterName = CsConstant.PersonName.Bpm,
							ReciverName = handler.ServiceName,
							ServiceObjName = handler.JName,
							BpmObjName = entity.GetType().Name,
							AdditionalInfo = string.Format("{0} - {1}", entity.PrimaryColumnValue, entity.PrimaryDisplayColumnValue)
						};
						LoggerHelper.DoInTransaction(logInfo, () =>
						{
							try
							{
								var integrationInfo = CsConstant.IntegrationInfo.CreateForExport(userConnection, entity);
								integrationInfo.Handler = handler;
								entityHelper.IntegrateEntity(integrationInfo);
								if (integrationInfo.Result != null && integrationInfo.Result.Type == CsConstant.IntegrationResult.TResultType.Success)
								{
									var json = integrationInfo.Result.Data.ToString();
									var requestInfo = integrationInfo.Handler.GetRequestInfo(integrationInfo);
									requestInfo.LogId = IntegrationLogger.CurrentTransLogId;
									MakeRequest(requestInfo);
								}
							}
							catch (Exception e)
							{
								IntegrationLogger.Error(e);
							}
						});
					}
				}, IntegrationLogger.SimpleLoggerErrorAction, null, withLock);
			} catch (Exception e) {
				IntegrationLogger.Error(e);
			}
		}

		public virtual void IntegrateServiceEntity(JObject serviceEntity, string serviceObjectName, bool updateExists = true) {
			if (!IsIntegratorActive) {
				return;
			}
			var integrationInfo = CsConstant.IntegrationInfo.CreateForImport(userConnection, CsConstant.IntegrationActionName.Create, serviceObjectName, serviceEntity);
			entityHelper.IntegrateEntity(integrationInfo);
			if (integrationInfo.Result != null && integrationInfo.Result.Type == CsConstant.IntegrationResult.TResultType.Exception)
			{
				if (integrationInfo.Result.Exception == CsConstant.IntegrationResult.TResultException.OnCreateEntityExist && updateExists)
				{
					integrationInfo = CsConstant.IntegrationInfo.CreateForImport(userConnection, CsConstant.IntegrationActionName.Update, serviceObjectName, serviceEntity);
					entityHelper.IntegrateEntity(integrationInfo);
				}
			}
		}

		private Entity PrepareEntity(UserConnection userConnection, string schemaName, Guid entityId) {
			EntitySchema entitySchema = userConnection.EntitySchemaManager.GetInstanceByName(schemaName);
			if(entitySchema != null) {
				Entity entity = entitySchema.CreateEntity(userConnection);
				if (entity.FetchFromDB(entityId, false)) {
					return entity;
				}
			}
			return null;
		}
	}

	#endregion


	#region Class: DeliveryServiceEntityProvider
	/*
		Project Path: ..\..\..\QueryConsole\Files\Integrators\DeliveryServices\DeliveryServiceEntityProvider.cs
		
	*/
	public abstract class DeliveryServiceEntityProvider : DeliveryServiceLookupProvider {
		public DeliveryServiceIntegrator integrator;
		public Dictionary<string, string> Filters;
		public abstract string EntityName {
			get;
		}
		public virtual string IdFieldName {
			get {
				return Settings.IdFieldName;
			}
		}
		public abstract string DisplayFiledName {
			get;
		}
		public virtual int Limit {
			get {
				return Settings.RequestLimit;
			}
		}
		protected CsConstant.DeliveryServiceSettings.DeliveryServiceSettingsValue settings;
		public virtual CsConstant.DeliveryServiceSettings.DeliveryServiceSettingsValue Settings {
			get {
				if (settings == null) {
					if (!CsConstant.DeliveryServiceSettings.Settings.TryGetValue(GetType(), out settings)) {
						settings = CsConstant.DeliveryServiceSettings.Settings[typeof(DeliveryServiceEntityProvider)];
					}
				}
				return settings;
			}
		}
		protected Dictionary<string, string> srcFields;
		public virtual Dictionary<string, string> SrcFields {
			get {
				if (srcFields == null) {
					srcFields = new Dictionary<string, string>() {
						{ "id", IdFieldName },
						{ "displayValue", DisplayFiledName }
					};
				}
				return srcFields;
			}
		}
		public DeliveryServiceEntityProvider(UserConnection userConnection, Dictionary<string, string> filters) {
			integrator = new DeliveryServiceIntegrator(userConnection);
			Filters = filters ?? new Dictionary<string, string>();
		}
		public virtual List<Dictionary<string, string>> GetLookupValues(string query = null) {
			var result = new List<Dictionary<string, string>>();
			bool filtering = !string.IsNullOrEmpty(query);
			integrator.PushGetEntityRequest(EntityName, Limit, query, DisplayFiledName, Filters, (resultJson, resutlUserConnection) => {
				var jObject = JObject.Parse(resultJson);
				if (Filters.ContainsKey("id"))
				{
					if (jObject != null)
					{
						JToken entityValues = jObject[EntityName];
						result.Add(GetResult(entityValues));
					}
				}
				else
				{
					var entities = jObject.SelectToken("data");
					if (entities != null && entities.Any())
					{
						foreach (var entity in entities)
						{
							JToken entityValues = entity[EntityName];
							result.Add(GetResult(entityValues));
						}
					}
				}
			}, (errorText, errorUserConnection) => {
				IntegrationLogger.AfterRequestError(new Exception(errorText));
			});
			return result;
		}
		public virtual Dictionary<string, string> GetResult(JToken entityValues) {
			var result = new Dictionary<string, string>();
			foreach (var srcField in SrcFields) {
				var jToken = entityValues.SelectToken(srcField.Value);
				string value = string.Empty;
				if(jToken != null) {
					value = jToken.Value<string>();
				}
				result.Add(srcField.Key, value);
			}
			return result;
		}
	}

	#endregion


	#region Class: DeliveryServiceServiceProvider
	/*
		Project Path: ..\..\..\QueryConsole\Files\Integrators\DeliveryServices\DeliveryServiceServiceProvider.cs
		
	*/
	public abstract class DeliveryServiceServiceProvider : DeliveryServiceLookupProvider {
		public abstract string MethodName {
			get;
		}
		public abstract string EntityName {
			get;
		}
		public virtual string IdFiledName {
			get {
				return Settings.IdFieldName;
			}
		}
		public abstract string DisplayFiledName {
			get;
		}
		public virtual int FilterLimit {
			get {
				return Settings.RequestLimit;
			}
		}
		protected Dictionary<string, string> srcFields;
		public virtual Dictionary<string, string> SrcFields {
			get {
				if(srcFields == null) {
					srcFields = new Dictionary<string, string>() {
						{ "id", IdFiledName },
						{ "displayValue", DisplayFiledName }
					};
				}
				return srcFields;
			}
		}
		protected CsConstant.DeliveryServiceSettings.DeliveryServiceSettingsValue settings;
		public virtual CsConstant.DeliveryServiceSettings.DeliveryServiceSettingsValue Settings {
			get {
				if (settings == null) {
					if (!CsConstant.DeliveryServiceSettings.Settings.TryGetValue(GetType(), out settings)) {
						settings = CsConstant.DeliveryServiceSettings.Settings[typeof(DeliveryServiceServiceProvider)];
					}
				}
				return settings;
			}
		}
		public Dictionary<string, string> Filters;
		DeliveryServiceIntegrator integrator;
		public DeliveryServiceServiceProvider(UserConnection userConnection, Dictionary<string, string> filters) {
			integrator = new DeliveryServiceIntegrator(userConnection);
			Filters = filters ?? new Dictionary<string, string>();
		}
		public virtual List<Dictionary<string, string>> GetLookupValues(string query = null) {
			var result = new List<Dictionary<string, string>>();
			bool filtering = !string.IsNullOrEmpty(query);
			integrator.PushGetServiceRequest(MethodName, query, Filters, (resultJson, resutlUserConnection) => {
				var jObject = JObject.Parse(resultJson);
				JToken entities = jObject.SelectToken("data");
				if(entities == null)
				{
					var token = jObject.SelectToken(EntityName);
					if(token != null && token.HasValues)
					{
						entities = new JArray();
						((JArray)entities).Add(jObject);
					}
				}
				if (entities != null && entities.Any()) {
					foreach (var entity in entities) {
						JToken entityValues = entity[EntityName];
						result.Add(GetResult(entityValues));
						if (result.Count > FilterLimit) {
							break;
						}
					}
				}
			}, (errorText, errorUserConnection) => {
				IntegrationLogger.AfterRequestError(new Exception(errorText));
			});
			return result;
		}
		public virtual Dictionary<string, string> GetResult(JToken entityValues) {
			var result = new Dictionary<string, string>();
			foreach (var srcField in SrcFields)
			{
				var jToken = entityValues.SelectToken(srcField.Value);
				string value = string.Empty;
				if (jToken != null)
				{
					value = jToken.Value<string>();
				}
				result.Add(srcField.Key, value);
			}
			return result;
		}
	}

	#endregion


	#region Class: DeliveryServiceAreaProvider
	/*
		Project Path: ..\..\..\QueryConsole\Files\Integrators\DeliveryServices\Area\DeliveryServiceAreaProvider.cs
		
	*/
	public class DeliveryServiceAreaProvider : DeliveryServiceEntityProvider {
		public DeliveryServiceAreaProvider(UserConnection userConnection, Dictionary<string, string> filters) : base(userConnection, filters) {
		}
		public override string DisplayFiledName {
			get {
				return "displayName";
			}
		}
		public override string EntityName {
			get {
				return "Area";
			}
		}
		public override Dictionary<string, string> SrcFields {
			get {
				if (srcFields == null) {
					srcFields = base.SrcFields;
					srcFields.Add("region", "region.#ref.id");
				}
				return srcFields;
			}
		}
	}

	#endregion


	#region Class: DeliveryServiceCountryProvider
	/*
		Project Path: ..\..\..\QueryConsole\Files\Integrators\DeliveryServices\Country\DeliveryServiceCountryProvider.cs
		
	*/
	public class DeliveryServiceCountryProvider : DeliveryServiceEntityProvider {
		public DeliveryServiceCountryProvider(UserConnection userConnection, Dictionary<string, string> filters) : base(userConnection, filters) {
		}
		public override string EntityName {
			get {
				return "Country";
			}
		}
		public override string DisplayFiledName {
			get {
				return "name";
			}
			
		}
		public override Dictionary<string, string> SrcFields {
			get {
				if (srcFields == null) {
					srcFields = base.SrcFields;
					srcFields.Add("countryCode", "iso3166Number");
				}
				return srcFields;
			}
		}
	}

	#endregion


	#region Class: DeliveryServiceAddressProvider
	/*
		Project Path: ..\..\..\QueryConsole\Files\Integrators\DeliveryServices\ParseAddress\DeliveryServiceAddressProvider.cs
		
	*/
	public class DeliveryServiceAddressProvider : DeliveryServiceServiceProvider
	{
		public DeliveryServiceAddressProvider(UserConnection userConnection, Dictionary<string, string> filters) : base(userConnection, filters)
		{
		}

		public override string DisplayFiledName {
			get {
				return "displayName";
			}
		}

		public override string EntityName {
			get {
				return "AddressSearchResult";
			}
		}

		public override string MethodName {
			get {
				return "addressSearch";
			}
		}
		public override Dictionary<string, string> SrcFields {
			get {
				if (srcFields == null)
				{
					srcFields = new Dictionary<string, string>();
					srcFields.Add("house", "house");
					srcFields.Add("zipCode", "zipCode");
					srcFields.Add("settlement", "settlement.Settlement.displayName");
					srcFields.Add("settlementId", "settlement.Settlement.id");
					srcFields.Add("region", "settlement.Settlement.region.#ref.name");
					srcFields.Add("regionId", "settlement.Settlement.region.#ref.id");
					srcFields.Add("area", "settlement.Settlement.area.#ref.name");
					srcFields.Add("areaId", "settlement.Settlement.area.#ref.id");
					srcFields.Add("country", "settlement.Settlement.country.#ref.name");
					srcFields.Add("countryId", "settlement.Settlement.country.#ref.id");
					srcFields.Add("street", "street.Street.displayName");
				}
				return srcFields;
			}
		}
	}

	#endregion


	#region Class: DeliveryServiceRegionProvider
	/*
		Project Path: ..\..\..\QueryConsole\Files\Integrators\DeliveryServices\Region\DeliveryServiceRegionProvider.cs
		
	*/
	public class DeliveryServiceRegionProvider: DeliveryServiceEntityProvider {
		public DeliveryServiceRegionProvider(UserConnection userConnection, Dictionary<string, string> filters) : base(userConnection, filters) {
		}
		public override string DisplayFiledName {
			get {
				return "displayName";
			}
		}
		public override string EntityName {
			get {
				return "Region";
			}
		}
		public override Dictionary<string, string> SrcFields {
			get {
				if (srcFields == null) {
					srcFields = base.SrcFields;
					srcFields.Add("timeZone", "timeZone");
					srcFields.Add("country", "country.#ref.id");
				}
				return srcFields;
			}
		}
	}

	#endregion


	#region Class: DeliveryServiceSettlementProvider
	/*
		Project Path: ..\..\..\QueryConsole\Files\Integrators\DeliveryServices\Settlement\DeliveryServiceSettlementProvider.cs
		
	*/
	public class DeliveryServiceSettlementProvider : DeliveryServiceEntityProvider {
		public DeliveryServiceSettlementProvider(UserConnection userConnection, Dictionary<string, string> filters) : base(userConnection, filters) {
		}
		public override string DisplayFiledName {
			get {
				return "displayName";
			}
		}
		public override string EntityName {
			get {
				return "Settlement";
			}
		}
		public override Dictionary<string, string> SrcFields {
			get {
				if (srcFields == null) {
					srcFields = base.SrcFields;
					srcFields.Add("region", "region.#ref.id");
					srcFields.Add("area", "area.#ref.id");
					srcFields.Add("country", "country.#ref.id");
				}
				return srcFields;
			}
		}
	}

	#endregion


	#region Class: DeliveryServiceStreetProvider
	/*
		Project Path: ..\..\..\QueryConsole\Files\Integrators\DeliveryServices\Street\DeliveryServiceStreetProvider.cs
		
	*/
	class DeliveryServiceStreetProvider : DeliveryServiceServiceProvider {
		public DeliveryServiceStreetProvider(UserConnection userConnection, Dictionary<string, string> filters) : base(userConnection, filters) {
		}
		public override string DisplayFiledName {
			get {
				return "displayName";
			}
		}
		public override string EntityName {
			get {
				return "Street";
			}
		}
		public override string MethodName {
			get {
				return "streetSearch";
			}
		}
		public override Dictionary<string, string> SrcFields {
			get {
				if (srcFields == null) {
					srcFields = base.SrcFields;
					srcFields.Add("settlement", "settlement.#ref.id");
				}
				return srcFields;
			}
		}
	}

	#endregion


	#region Class: BaseIntegratorTester
	/*
		Project Path: ..\..\..\QueryConsole\Files\IntegratorTester\BaseIntegratorTester.cs
		
	*/
	public abstract class BaseIntegratorTester
	{
		public BaseServiceIntegrator Integrator;
		public List<string> BpmEntitiesName;
		public List<string> ServiceEntitiesName;
		public UserConnection UserConnection;
		public int Limit;
		public int Skip;
		public Action AfterIntegrate;

		public BaseIntegratorTester(UserConnection userConnection) {
			BpmEntitiesName = InitEntitiesName();
			ServiceEntitiesName = InitServiceEntitiesName();
			UserConnection = userConnection;
			Integrator = CreateIntegrator();
		}
		public void ImportAllBpmEntity() {
			foreach(var entityName in BpmEntitiesName) {
				ClonsoleGreen(entityName + " Start:");
				try {
					var collection = GetEntitiesBySchemaNames(entityName, false);
					Console.WriteLine("Count: " + collection.Count);
					foreach(var entity in collection) {
						ImportBpmEntity(entity);
					}
				} catch(Exception e) {
					ClonsoleGreen(" Error: " + e.Message);
				}
				ClonsoleGreen(entityName + " End:");
			}
		}
		public EntityCollection GetEntitiesBySchemaNames(string name, bool withoutIntegrated = true) {
			var esq = new EntitySchemaQuery(UserConnection.EntitySchemaManager, name);
			esq.RowCount = 1;
			esq.AddAllSchemaColumns();
			//var dateColumn = esq.AddColumn("CreatedOn");
			//dateColumn.OrderByDesc();
			//esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, "Name", "Наша компания"));
			esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, "Id", new Guid("b936770a-4f38-4ea3-9735-4df6c03808f0")));
			//if(name == "Contact") {
			//esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, "Id", new Guid("c4d4eb57-75a3-4698-9134-57a9fb4d10c1")));
			//}
			if (name == "ContactCareer") {
				//esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, "Id", new Guid("6D20D35D-82B9-4CFD-9D18-670003D79AB7")));
			}
			if (withoutIntegrated) {
				esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, CsConstant.ServiceColumnInBpm.Identifier, 0));
			}
			if(name == "SysAdminUnit") {
				//Manager Group
				//esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.NotEqual, "SysAdminUnitTypeValue", 4));
				////Lrs Moskov
				////esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, "Id", new Guid("735379a1-301a-411a-aa98-2e65651961ac")));
				////Sp
				//esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, "Id", new Guid("220f42be-85d4-48b6-8fa1-1fa3c992fc64")));

				//Manager
				//esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, "SysAdminUnitTypeValue", 4));
			}
			if(name=="Contact") {
				//
				//esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, "Id", new Guid("1009798a-7e7f-45d9-8afd-ba2aabb1b222")));
			}
			return esq.GetEntityCollection(UserConnection);
		}
		public void ExportServiceEntity(string name, Action afterIntegrate = null)
		{
			AfterIntegrate = afterIntegrate;
			var serviceRequestInfo = ServiceRequestInfo.CreateForExportInBpm(name);
			serviceRequestInfo.Limit = Limit.ToString();
			serviceRequestInfo.Skip = Skip.ToString();
			serviceRequestInfo.AfterIntegrate = AfterIntegrate;
			Integrator.GetRequest(serviceRequestInfo);
		}
		public void ExportServiceEntity(string name, int id, Action afterIntegrate = null)
		{
			AfterIntegrate = afterIntegrate;
			var serviceRequestInfo = ServiceRequestInfo.CreateForExportInBpm(name);
			serviceRequestInfo.Limit = Limit.ToString();
			serviceRequestInfo.Skip = Skip.ToString();
			serviceRequestInfo.ServiceObjectId = id.ToString();
			serviceRequestInfo.AfterIntegrate = AfterIntegrate;
			Integrator.GetRequest(serviceRequestInfo);
		}
		public void ExportAllServiceEntities(int limit = 10, int skip = 0) {
			Limit = limit;
			Skip = skip;
			foreach(var name in ServiceEntitiesName) {
				ClonsoleGreen("Start: " + name);
				try {
					ExportServiceEntity(name);
				} catch(Exception e) {
					ClonsoleGreen("Error: " + e.Message);
				}
				ClonsoleGreen("End: " + name);
			}
		}
		public void ExportById(string name, int id, Action afterIntegrate = null) {
			ExportServiceEntity(name, id, afterIntegrate);
		}
		public abstract BaseServiceIntegrator CreateIntegrator();
		public abstract List<string> InitEntitiesName();
		public abstract List<string> InitServiceEntitiesName();
		public abstract void ImportBpmEntity(Entity entity);
		public void ExportAllServiceEntitiesByStep(int stepCount = 10, int rightLimit = 1000, Action afterIntegrate = null) {
			Limit = stepCount;
			Skip = 0;
			AfterIntegrate = afterIntegrate;
			foreach (var name in ServiceEntitiesName)
			{
				ClonsoleGreen("Start: " + name);
				try
				{
					for(Skip = 0; Skip - Limit < rightLimit; Skip += stepCount) {
						ExportServiceEntity(name);
					}
				}
				catch (Exception e)
				{
					ClonsoleGreen("Error: " + e.Message);
				}
				ClonsoleGreen("End: " + name);
			}
		}
		private void ClonsoleGreen(string text)
		{
			//var buff = Console.ForegroundColor;
			//Console.ForegroundColor = ConsoleColor.Green;
			//Console.WriteLine(text);
			//Console.ForegroundColor = buff;
		}

		private void Clonsole(string text)
		{
			//Console.WriteLine(text);
		}
	}

	#endregion


	#region Class: ClientServiceIntegratorTester
	/*
		Project Path: ..\..\..\QueryConsole\Files\IntegratorTester\ClientServiceIntegratorTester.cs
		
	*/
	class ClientServiceIntegratorTester : BaseIntegratorTester
	{
		public ClientServiceIntegratorTester(UserConnection userConnection)
			: base(userConnection)
		{

		}
		public override BaseServiceIntegrator CreateIntegrator()
		{
			return new ClientServiceIntegrator(UserConnection);
		}
		public override List<string> InitEntitiesName() {
			return new List<string>() {
				"Account",
				//"Contact",
				//"TsAutoOwnerInfo",
				//"TsAutomobile",
				//"SysAdminUnit",
				//"Case",
				//"Relationship",
				//"ContactCareer",
				//"TsContactNotifications",
				//"TsAccountNotification",
				//"ContactAddress",
				//"TsAutoTechService",
				//"TsAutoOwnerHistory",
				//"TsAutoOwnerInfo",
				//"TsAutoTechHistory",
				//"TsLocSalMarket"
			};
		}
		public override void ImportBpmEntity(Entity entity)
		{
			Integrator.IntegrateBpmEntity(entity);
		}
		public override List<string> InitServiceEntitiesName()
		{
			return new List<string>() {
                "CompanyProfile",
                "PersonProfile",
                "ContactRecord",
                "VehicleProfile",
                "Manager",
                "ManagerGroup",
                "ClientRequest",
                "Relationship",
                "Employee",
                "NotificationProfile",
                "VehicleRelationship",
                "Market"
            };
		}
	}

	#endregion


	#region Class: OrderServiceIntegratorTester
	/*
		Project Path: ..\..\..\QueryConsole\Files\IntegratorTester\OrderServiceIntegratorTester.cs
		
	*/
	public class OrderServiceIntegratorTester: BaseIntegratorTester
	{
		public OrderServiceIntegratorTester(UserConnection userConnection): base(userConnection) {

		}
		public override BaseServiceIntegrator CreateIntegrator()
		{
			return new OrderServiceIntegrator(UserConnection);
		}
		public override List<string> InitEntitiesName() {
			return new List<string>() {
				//"TsPayment",
				//"Order",
				//"Account",
				//"OrderProduct",
				//"TsReturn",
				//"TsShipment",
				//"TsShipmentPosition",
				//"TsContractDebt",
				//"Contract"
				"Contact"
			};
		}
		public override void ImportBpmEntity(Entity entity)
		{
			Integrator.IntegrateBpmEntity(entity);
		}
		public override List<string> InitServiceEntitiesName()
		{
			return new List<string>() {
			  //  "Payment",
			  // "Order",
			  //  "OrderItem",
			  //  "Return",
			  "Shipment",
			  // "Debt",
			  //  "Contract",
			  //  "ContractBalance",
			  //  "ManagerInfo",
			  //  "CounteragentContactInfo",
			  //"Counteragent"
			};
		}
	}

	#endregion


	#region Class: TesterManager
	/*
		Project Path: ..\..\..\QueryConsole\Files\IntegratorTester\TesterManager.cs
		
	*/
	public class TesterManager: IEnumerable {
		public UserConnection UserConnection;
		public List<BaseIntegratorTester> Testers;
		public List<Action> Actions = new List<Action>();
		public List<Tuple<string, int, int, int>> Configs;

		public TesterManager(UserConnection userConnection, params BaseIntegratorTester[] testers) {
			UserConnection = userConnection;
			Testers = testers.ToList();
			Configs = new List<Tuple<string, int, int, int>>();
		}

		public void Add(string Name, int limit, int skip, int count = -1, int id = 0)
		{
			//IntegrationConsole.AddEntityProgress(Name, limit * count);
			if (count == -1)
			{
				Configs.Add(new Tuple<string, int, int, int>(Name, limit, skip, id));
			}
			else
			{
				for (var i = 0; i < count; i++)
				{
					Configs.Add(new Tuple<string, int, int, int>(Name, limit, skip + i * limit, id));
				}
			}
		}

		

		public void GenerateActions() {
			for(var i = Configs.Count - 1; i >= 0; i--) {
				var name = Configs[i].Item1;
				var limit = Configs[i].Item2;
				var skip = Configs[i].Item3;
				var id = Configs[i].Item4;
				var j = i;
				if(i == Configs.Count - 1) {
					Actions.Add(() => {
						//IntegrationConsole.SetCurrentEntity(name);
						var tester = GetTesterByEntityName(name);
						tester.Limit = limit;
						tester.Skip = skip;
						Action afterSaveAction = () =>
						{
							//IntegrationConsole.EndIntegrated();
						};
						if (id == 0)
						{
							tester.ExportServiceEntity(name, afterSaveAction);
						} else {
							tester.ExportById(name, id, afterSaveAction);
						}
					});
				} else {
					Actions.Add(() => {
						//IntegrationConsole.SetCurrentEntity(name);
						var tester = GetTesterByEntityName(name);
						tester.Limit = limit;
						tester.Skip = skip;
						if (id == 0)
						{
							tester.ExportServiceEntity(name, Actions[Configs.Count - 2 - j]);
						} else {
							tester.ExportById(name, id, Actions[Configs.Count - 2 - j]);
						}
					});
				}
			}
		}

		public void Run() {
			GenerateActions();
			//IntegrationConsole.StartIntegrate();
			Actions.Last()();
		}

		public BaseIntegratorTester GetTesterByEntityName(string name) {
			foreach(var tester in Testers) {
				if(tester.InitServiceEntitiesName().Contains(name)) {
					return tester;
				}
			}
			return null;
		}
			//new OrderServiceIntegratorTester(consoleApp.SystemUserConnection);
			//new ClientServiceIntegratorTester(consoleApp.SystemUserConnection);

		public IEnumerator GetEnumerator() {
			throw new NotImplementedException();
		}
	}

	#endregion


	#region Class: IntegrationLocker
	/*
		Project Path: ..\..\..\QueryConsole\Files\Locker\IntegrationLocker.cs
		
	*/
	public static class IntegrationLocker
	{
		public static ConcurrentDictionary<string, bool> LockerInfo = new ConcurrentDictionary<string, bool>();
		private static bool isLocckerActive {
			get {
				return CsConstant.IntegratorSettings.isLockerActive;
			}
		}
		public static void Lock(object schemaName, object id, string keyMixin = null)
		{
			if (!isLocckerActive) {
				return;
			}
			var key = GetKey(schemaName, id, keyMixin);
			IntegrationLogger.Info(string.Format("Lock => Schema Name: {0} Id: {1} key: {2}", schemaName, id, key));
			if (!LockerInfo.ContainsKey(key))
			{
				LockerInfo.TryAdd(key, true);
			}
		}
		public static void Unlock(object schemaName, object id, string keyValue = null)
		{
			if (!isLocckerActive) {
				return;
			}
			var key = GetKey(schemaName, id, keyValue);
			IntegrationLogger.Info(string.Format("Unlock => Schema Name: {0} Id: {1} key: {2}", schemaName, id, key));
			if (LockerInfo.ContainsKey(key))
			{
				bool removeItem;
				LockerInfo.TryRemove(key, out removeItem);
			}
		}

		public static bool CheckWithUnlock(object schemaName, object id, string keyValue = null)
		{
			if(!isLocckerActive) {
				return true;
			}
			if(!CheckUnLock(schemaName, id, keyValue))
			{
				Unlock(schemaName, id);
			}
			return CheckUnLock(schemaName, id);
		}
		public static bool CheckUnLock(object schemaName, object id, string keyValue = null)
		{
			return !isLocckerActive || !LockerInfo.ContainsKey(GetKey(schemaName, id, keyValue));
		}

		private static string GetKey(object schemaName, object id, string keyValue)
		{
			return string.Format("{0}_{1}_{2}_{3}", id, schemaName, Thread.CurrentThread.ManagedThreadId, keyValue ?? "!");
		}
	}

	#endregion


	#region Class: LockerHelper
	/*
		Project Path: ..\..\..\QueryConsole\Files\Locker\LockerHelper.cs
		
	*/
	public static class LockerHelper {
		public static void DoWithEntityLock(object entityId, object schemaName, Action action, Action<Exception> OnExceptionAction = null, string keyMixin = null, bool withLock = true, bool withCheckLock = true) {
			if (!withLock || !withCheckLock || IntegrationLocker.CheckUnLock(schemaName, entityId, keyMixin)) {
				if (withLock) {
					IntegrationLocker.Lock(schemaName, entityId, keyMixin);
				}
				try {
					action();
				} catch(Exception e) {
					if(OnExceptionAction != null) {
						OnExceptionAction(e);
					}
				} finally {
					if (withLock) {
						IntegrationLocker.Unlock(schemaName, entityId, keyMixin);
					}
				}
			}
		}
	}

	#endregion


	#region Class: IntegrationLogger
	/*
		Project Path: ..\..\..\QueryConsole\Files\Logger\IntegrationLogger.cs
		
	*/
	public static class IntegrationLogger
	{
		/// <summary>
		/// Простой предикат для логгирования ошибки
		/// </summary>
		public static Action<Exception> SimpleLoggerErrorAction = e => IntegrationLogger.Error(e);
		/// <summary>
		/// Активный файловой-логгер
		/// </summary>
		private static TsLogger _log = new TsLogger();
		/// <summary>
		/// Справочник всех состояний логгеров
		/// </summary>
		public static ConcurrentDictionary<int, LoggerState> ThreadLogIds = new ConcurrentDictionary<int, LoggerState>();
		/// <summary>
		/// Признак - логировать stacktrace
		/// </summary>
		public static bool IsLoggedStackTrace {
			get {
				return CsConstant.LoggerSettings.IsLoggedStackTrace;
			}
		}
		/// <summary>
		/// Идентификатор текущего потока
		/// </summary>
		public static int CurrentThreadId {
			get { return Thread.CurrentThread.ManagedThreadId; }
		}
		/// <summary>
		/// Идентификатор текущей транзакции
		/// </summary>
		public static Guid CurrentTransLogId {
			get {
				if (ThreadLogIds.ContainsKey(CurrentThreadId))
				{
					return ThreadLogIds[CurrentThreadId].TransactionId;
				}
				return Guid.Empty;
			}
		}
		/// <summary>
		/// Текущее состояние логгера
		/// </summary>
		public static LoggerState CurrentLogState {
			get {
				if (ThreadLogIds.ContainsKey(CurrentThreadId))
				{
					return ThreadLogIds[CurrentThreadId];
				}
				return null;
			}
		}
		/// <summary>
		/// Логгер
		/// </summary>
		public static TsLogger CurrentLogger {
			get {
				return _log;
			}
		}
		/// <summary>
		/// Логирует ошибку в файл.
		/// </summary>
		/// <param name="e">Ошибка</param>
		public static void AfterRequestError(Exception e)
		{
			var logger = CurrentLogger.Instance;
			logger.Error(string.Format("Error - text = {0} callStack = {1}", e.Message, e.StackTrace));
		}
		/// <summary>
		/// Начинает транзакцию логирования
		/// </summary>
		/// <param name="userConnection">Подключение пользователя</param>
		/// <param name="requesterName">Имя того кто делает запрос</param>
		/// <param name="reciverName">Имя получателя</param>
		/// <param name="bpmObjName">Имя объекта в Bpm</param>
		/// <param name="serviceObjName">Имя объекта в сервисе</param>
		/// <param name="additionalInfo">Дополнительная информация</param>
		/// <param name="isPrimary">Признак первичного импорта</param>
		/// <param name="useIfExist">Если для текущего потока уже есть транзакция, то используем ее</param>
		public static bool StartTransaction(UserConnection userConnection, string requesterName, string reciverName, string bpmObjName, string serviceObjName, string additionalInfo = "", bool isPrimary = false, bool useIfExist = false)
		{
			try
			{
				bool isNew = false;
				Guid id = CreateNewTransactionId(useIfExist, notExistAction: transactionId =>
				{
					CurrentLogger.CreateTransaction(transactionId, requesterName, reciverName, bpmObjName, serviceObjName, additionalInfo, isPrimary);
					isNew = true;
				});
				CurrentLogger.Instance.Info("StartTransaction " + id);
				return isNew;
			}
			catch (Exception e2)
			{
				CurrentLogger.Instance.Error(e2.ToString());
			}
			return false;
		}
		/// <summary>
		/// Завершает транзакцию логгирования
		/// </summary>
		/// <param name="userConnection">Подключение пользователя</param>
		public static void FinishTransaction(UserConnection userConnection)
		{
			CurrentLogger.FinishTransaction(CurrentTransLogId);
			ClearCurrentTransaction();
		}
		/// <summary>
		/// Создает id новой транзакции
		/// </summary>
		/// <returns></returns>
		private static Guid CreateNewTransactionId(bool useIfExist, Action<Guid> notExistAction)
		{
			var resultId = Guid.NewGuid();
			if (ThreadLogIds.ContainsKey(CurrentThreadId))
			{
				if (useIfExist)
				{
					return ThreadLogIds[CurrentThreadId].TransactionId;
				}
				ThreadLogIds[CurrentThreadId] = new LoggerState()
				{
					TransactionId = resultId
				};
				notExistAction(resultId);
			}
			else
			{
				ThreadLogIds.TryAdd(CurrentThreadId, new LoggerState()
				{
					TransactionId = resultId
				});
				notExistAction(resultId);
			}
			return resultId;
		}
		private static void ClearCurrentTransaction()
		{
			if (ThreadLogIds.ContainsKey(CurrentThreadId))
			{
				LoggerState removedState;
				ThreadLogIds.TryRemove(CurrentThreadId, out removedState);
			}
		}
		/// <summary>
		/// Логирование ошибки.
		/// </summary>
		/// <param name="e">Ошибка</param>
		/// <param name="additionalInfo">Дополнительная информаци</param>
		public static void Error(Exception e, string additionalInfo = null)
		{
			try
			{
				CurrentLogger.Instance.InfoFormat("logid:{0} Exception:{1} stack:{2} AdditionalInfo:{3}", CurrentTransLogId, e.ToString(), e.StackTrace, additionalInfo);
				CurrentLogger.Error(CurrentTransLogId, e.ToString(), e.StackTrace, additionalInfo);
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
		public static void Info(string message)
		{
			try
			{
				CurrentLogger.Instance.Info(message);
			}
			catch (Exception e)
			{
				CurrentLogger.Instance.Info(e.ToString());
			}
		}
		/// <summary>
		/// Логирование информации о запросе
		/// </summary>
		/// <param name="info">Информация о запросе</param>
		public static void Info(RequestLoggerInfo info)
		{
			try
			{
				var requestType = CsConstant.TsRequestType.Push;
				CurrentLogger.Instance.Info(string.Format("PushRequest - id = {0} method={1} requestType={4}\nurl={2}\njson={3}", CurrentTransLogId, info.RequestMethod, info.Url, info.JsonText, requestType));
				if (IsLoggedStackTrace)
				{
					info.AdditionalInfo += GetStackTrace();
				}
				var requestId = CurrentLogger.CreateRequest(CurrentTransLogId, info.RequestMethod.ToString(), info.Url, requestType, info.AdditionalInfo);
				if (requestId != null)
				{
					SetLastRequestId(requestId);
				}
			}
			catch (Exception e2)
			{
				CurrentLogger.Instance.Error(e2.ToString());
			}
		}
		/// <summary>
		/// Логирование информаци о ответе на запрос
		/// </summary>
		/// <param name="info">Информация о ответе на запрос</param>
		public static void Info(ResponseLoggerInfo info)
		{
			try
			{
				var requestType = CsConstant.TsRequestType.GetResponse;
				CurrentLogger.Instance.Info(string.Format("GetResponse - id = {0} requestType={2}\ntext={1}", CurrentTransLogId, info.ResponseText, requestType));
				if (IsLoggedStackTrace)
				{
					info.ResponseText += GetStackTrace();
				}
				var state = CurrentLogState;
				if (state != null)
				{
					CurrentLogger.CreateResponse(CurrentTransLogId, info.ResponseText, requestType, CurrentLogState.LastRequestId);
				}
			}
			catch (Exception e2)
			{
				CurrentLogger.Instance.Error(e2.ToString());
			}
		}
		/// <summary>
		/// Логирование о ошибке запроса
		/// </summary>
		/// <param name="info"> Информация об ошибке</param>
		public static void Info(RequestErrorLoggerInfo info)
		{
			try
			{
				if (IsLoggedStackTrace)
				{
					info.ResponseText += GetStackTrace();
				}
				var e = info.Exception;
				CurrentLogger.UpdateResponseError(CurrentTransLogId, e.Message, e.StackTrace, info.ResponseText, info.ResponseJson, CurrentLogState.LastRequestId);
				CurrentLogger.Instance.InfoFormat("error: {0}\ntext: {1}\njson: {2}\nid: {3}", e.ToString(), info.ResponseText, info.ResponseJson, CurrentLogState.LastRequestId);
			}
			catch (Exception e2)
			{
				CurrentLogger.Instance.Error(e2.ToString());
			}
		}
		/// <summary>
		/// Информация о ошибки при маппинге
		/// </summary>
		/// <param name="info">Информация об ошибке</param>
		public static void Info(MappingErrorLoggerInfo info)
		{
			try
			{
				var e = info.Exception;
				var item = info.Item;
				CurrentLogger.MappingError(CurrentTransLogId, e.ToString(), e.StackTrace, item.JSourcePath, item.TsSourcePath);
				CurrentLogger.Instance.InfoFormat("logid:{0} Exception:{1} stack:{2} serviceFieldPath:{3} bpmFieldPath:{4}", CurrentTransLogId, e.ToString(), e.StackTrace, item.JSourcePath, item.TsSourcePath);
			}
			catch (Exception e2)
			{
				CurrentLogger.Instance.Info(e2.ToString());
			}
		}
		/// <summary>
		/// Устанавливает идентификатор последнего запроса
		/// </summary>
		/// <param name="requestId">Идентификатор последнего запроса</param>
		private static void SetLastRequestId(Guid requestId)
		{
			var state = CurrentLogState;
			if (state != null)
			{
				state.LastRequestId = requestId;
			}
		}
		/// <summary>
		/// Возвращает стек
		/// </summary>
		/// <returns></returns>
		private static string GetStackTrace()
		{
			System.Diagnostics.StackTrace t = new System.Diagnostics.StackTrace();
			return "Stack: " + t.ToString();
		}
	}

	#endregion


	#region Class: LoggerHelper
	/*
		Project Path: ..\..\..\QueryConsole\Files\Logger\LoggerHelper.cs
		
	*/
	public static class LoggerHelper
	{
		/// <summary>
		/// Гарантирует выполнение Action в транзакции логгирования
		/// </summary>
		/// <param name="info">Информация о транзакции</param>
		/// <param name="action">Предикат</param>
		public static void DoInTransaction(LoggerInfo info, Action action)
		{
			try
			{
				var isNew = CreateTransaction(info);
				try
				{
					action();
				}
				catch (Exception e)
				{
					IntegrationLogger.Error(e);
				}
				if (isNew)
				{
					FinishTransaction(info);
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
		public static bool CreateTransaction(LoggerInfo info)
		{
			try
			{
				return IntegrationLogger.StartTransaction(info.UserConnection, info.RequesterName, info.ReciverName, info.BpmObjName,
					info.ServiceObjName, info.AdditionalInfo, false, info.UseExistTransaction);
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e);
				return false;
			}
		}
		/// <summary>
		/// Завершает текущую транзакцию
		/// </summary>
		/// <param name="info">Информация о транзакции</param>
		public static void FinishTransaction(LoggerInfo info)
		{
			IntegrationLogger.FinishTransaction(info.UserConnection);
		}
	}

	#endregion


	#region Class: LoggerState
	/*
		Project Path: ..\..\..\QueryConsole\Files\Logger\LoggerState.cs
		
	*/
	public class LoggerState
	{
		public Guid TransactionId;
		public Guid LastRequestId;
	}

	#endregion


	#region Class: TsLogger
	/*
		Project Path: ..\..\..\QueryConsole\Files\Logger\TsLogger.cs
		
	*/
	public class TsLogger
	{
		private global::Common.Logging.ILog _log;
		private global::Common.Logging.ILog _emptyLog;

		public global::Common.Logging.ILog Instance
		{
			get
			{
				if (!isLoggedFileActive)
				{
					return _emptyLog;
				}
				return _log;
			}
		}

		public bool isLoggedDbActive
		{
			get { return CsConstant.LoggerSettings.IsLoggedDbActive; }
		}

		public bool isLoggedFileActive
		{
			get { return CsConstant.LoggerSettings.IsLoggedFileActive; }
		}

		public UserConnection userConnection
		{
			get { return CsConstant.UserConnection; }
		}

		public TsLogger()
		{
			_log = global::Common.Logging.LogManager.GetLogger("TscIntegration") ??
			       global::Common.Logging.LogManager.GetLogger("Common");
			_emptyLog = global::Common.Logging.LogManager.GetLogger("NotExistingLogger");
		}

		public void CreateTransaction(Guid id, string requesterName, string resiverName, string entityName,
			string serviceEntityName, string additionalInfo = "", bool isPrimary = false)
		{
			try
			{
				if (!isLoggedDbActive)
				{
					return;
				}
				var textQuery = string.Format(@"
					merge
						TsIntegrLog il
					using
						(select '{0}' as Id) as src
					on
						il.Id = src.Id
					when matched then
						update
							set
								TsResiver = '{2}',
								TsName = '{3}',
								TsEntityName = '{4}',
								TsServiceEntityName = '{5}',
								TsAdditionalInfo = '{6}',
								TsIsPrimaryIntegrate = {7}
					when not matched then
						insert (TsResiver, TsName, TsEntityName, TsServiceEntityName, TsAdditionalInfo, Id, TsIsPrimaryIntegrate)
						values ('{2}', '{3}', '{4}', '{5}', '{6}', '{0}', {7});
				", id, DateTime.UtcNow, resiverName, requesterName, entityName, serviceEntityName, additionalInfo,
					Convert.ToInt32(isPrimary));
				var query = new CustomQuery(userConnection, textQuery);
				query.Execute();
			}
			catch (Exception e)
			{
				Instance.Info(e.ToString());
			}
		}

		public Guid CreateRequest(Guid logId, string method, string url, Guid requestType, string additionalInfo = "")
		{
			if (!isLoggedDbActive)
			{
				return Guid.Empty;
			}
			try
			{
				var resultId = Guid.NewGuid();
				var guidValueType = new GuidDataValueType(userConnection.DataValueTypeManager);
				var strValueType = new TextDataValueType(userConnection.DataValueTypeManager);
				var insert = new Insert(userConnection)
					.Into("TsIntegrationRequest")
					.Set("Id", Column.Parameter(resultId, guidValueType))
					.Set("TsIntegrLogId", Column.Parameter(logId, guidValueType))
					.Set("TsMethod", Column.Parameter(method, strValueType))
					.Set("TsUrl", Column.Parameter(url, strValueType))
					.Set("TsRequestTypeId", Column.Parameter(requestType, guidValueType))
					.Set("TsAdditionalInfo", Column.Parameter(additionalInfo, strValueType))
					.Set("TsStatusId", Column.Parameter(CsConstant.TsRequestStatus.Success)) as Insert;
				insert.Execute();
				return resultId;
			}
			catch (Exception e)
			{
				Instance.Info(e.ToString());
				return Guid.Empty;
			}
		}

		public void CreateResponse(Guid id, string text, Guid requestType, Guid requestId)
		{
			if (!isLoggedDbActive)
			{
				return;
			}
			try
			{
				if (id == Guid.Empty)
				{
					return;
				}
				var insert = new Insert(userConnection)
					.Into("TsIntegrationRequest")
					.Set("TsIntegrLogId", Column.Parameter(id))
					.Set("TsRequestTypeId", Column.Parameter(requestType)) as Insert;
				if (requestId != null)
				{
					insert.Set("TsParentId", Column.Parameter(requestId));
				}
				insert.Execute();
			}
			catch (Exception e)
			{
				Instance.Info(e.ToString());
			}
		}

		public void UpdateResponseError(Guid id, string errorText, string callStack, string json, string requestJson,
			Guid requestId)
		{
			if (!isLoggedDbActive)
			{
				return;
			}
			try
			{
				if (id == Guid.Empty)
				{
					return;
				}
				var errorId = Guid.NewGuid();
				errorText = string.Format("[{0}] {1}", Thread.CurrentThread.ManagedThreadId, errorText ?? "");
				var guidValueType = new GuidDataValueType(userConnection.DataValueTypeManager);
				var strValueType = new TextDataValueType(userConnection.DataValueTypeManager);
				var insert = new Insert(userConnection)
					.Into("TsIntegrationError")
					.Set("Id", Column.Parameter(errorId, guidValueType))
					.Set("TsErrorText", Column.Parameter(errorText, strValueType))
					.Set("TsCallStack", Column.Parameter(callStack, strValueType))
					.Set("TsRequestJson", Column.Parameter(requestJson != null ? requestJson : string.Empty, strValueType))
					.Set("TsResponseJson", Column.Parameter(json, strValueType)) as Insert;
				insert.Execute();
				var update = new Update(userConnection, "TsIntegrationRequest")
					.Set("TsErrorId", Column.Parameter(errorId, guidValueType))
					.Set("TsStatusId", Column.Parameter(CsConstant.TsRequestStatus.Error))
					.Where("Id").IsEqual(Column.Parameter(requestId, guidValueType)) as Update;
				update.Execute();
			}
			catch (Exception e)
			{
				Instance.Info(e.ToString());
			}
		}

		public void MappingError(Guid logId, string errorMessage, string callStack, string serviceFieldName,
			string bpmFieldName)
		{
			if (!isLoggedDbActive)
			{
				return;
			}
			try
			{
				if (logId == Guid.Empty)
				{
					return;
				}
				errorMessage = string.Format("[{0}] {1}", Thread.CurrentThread.ManagedThreadId, errorMessage ?? "");
				var guidValueType = new GuidDataValueType(userConnection.DataValueTypeManager);
				var insert = new Insert(userConnection)
					.Into("TsIntegrMappingError")
					.Set("TsErrorMessage", Column.Parameter(errorMessage ?? ""))
					.Set("TsCallStack", Column.Parameter(callStack ?? ""))
					.Set("TsServiceFieldName", Column.Parameter(serviceFieldName ?? ""))
					.Set("TsBpmFieldName", Column.Parameter(bpmFieldName ?? ""))
					.Set("TsIntegrLogId", Column.Parameter(logId, guidValueType)) as Insert;
				insert.Execute();
			}
			catch (Exception e)
			{
				Instance.Info(e.ToString());
			}
		}

		public void Error(Guid logId, string errorMessage, string callStack, string additionalInfo)
		{
			if (!isLoggedDbActive)
			{
				return;
			}
			try
			{
				if (logId == Guid.Empty)
				{
					return;
				}
				additionalInfo = string.Format("[{0}] {1}", Thread.CurrentThread.ManagedThreadId, additionalInfo ?? "");
				var insert = new Insert(userConnection)
					.Into("TsIntegrError")
					.Set("TsErrorText", Column.Parameter(errorMessage ?? ""))
					.Set("TsCallStack", Column.Parameter(callStack ?? ""))
					.Set("TsAdditionalInfo", Column.Parameter(additionalInfo))
					.Set("TsIntegrLogId", Column.Parameter(logId)) as Insert;
				insert.Execute();
			}
			catch (Exception e)
			{
				Instance.Info(e.ToString());
			}
		}

		public void FinishTransaction(Guid logId)
		{
			if (!isLoggedDbActive || logId == Guid.Empty)
			{
				return;
			}
			try
			{
				var insert = new Insert(userConnection)
					.Into("TsIntegrationRequest")
					.Set("TsIntegrLogId", Column.Parameter(logId))
					.Set("TsAdditionalInfo", Column.Const("Finish"))
					.Set("TsStatusId", Column.Parameter(CsConstant.TsRequestStatus.Success)) as Insert;
				insert.Execute();
			}
			catch (Exception e)
			{
				Instance.Info(e.ToString());
			}
		}
	}

	#endregion


	#region Class: BaseLoggerInfo
	/*
		Project Path: ..\..\..\QueryConsole\Files\Logger\LoggerInfo\BaseLoggerInfo.cs
		
	*/
	public class BaseLoggerInfo
	{
		public bool LogInDb;

		public BaseLoggerInfo()
		{
			LogInDb = true;
		}
	}

	#endregion


	#region Class: LoggerInfo
	/*
		Project Path: ..\..\..\QueryConsole\Files\Logger\LoggerInfo\LoggerInfo.cs
		
	*/
	public class LoggerInfo: BaseLoggerInfo
	{
		public UserConnection UserConnection;
		public string RequesterName;
		public string ReciverName;
		public string BpmObjName;
		public string ServiceObjName;
		public string AdditionalInfo;
		public bool UseExistTransaction;
		public static LoggerInfo GetBpmRequestLogInfo(UserConnection userConnection, string serviceName, string bpmObjName, string serviceObjName, string addInfo = "")
		{
			return new LoggerInfo()
			{
				UserConnection = userConnection,
				RequesterName = CsConstant.PersonName.Bpm,
				ReciverName = serviceName,
				ServiceObjName = serviceObjName,
				BpmObjName = bpmObjName,
				AdditionalInfo = addInfo
			};
		}
		public static LoggerInfo GetNotifyRequestLogInfo(UserConnection userConnection, string addInfo = "")
		{
			return new LoggerInfo()
			{
				UserConnection = userConnection,
				RequesterName = CsConstant.PersonName.Unknown,
				ReciverName = CsConstant.PersonName.Bpm,
				ServiceObjName = CsConstant.PersonName.Unknown,
				BpmObjName = CsConstant.PersonName.Unknown,
				AdditionalInfo = addInfo
			};
		}

		public LoggerInfo()
		{
			UseExistTransaction = true;
		}
	}

	#endregion


	#region Class: MappingErrorLoggerInfo
	/*
		Project Path: ..\..\..\QueryConsole\Files\Logger\LoggerInfo\MappingErrorLoggerInfo.cs
		
	*/
	public class MappingErrorLoggerInfo: BaseLoggerInfo
	{
		public Exception Exception;
		public MappingItem Item;
		public CsConstant.IntegrationInfo IntegrationInfo;
	}

	#endregion


	#region Class: RequestErrorLoggerInfo
	/*
		Project Path: ..\..\..\QueryConsole\Files\Logger\LoggerInfo\RequestErrorLoggerInfo.cs
		
	*/
	public class RequestErrorLoggerInfo: BaseLoggerInfo
	{
		public Exception Exception;
		public string ResponseText;
		public string ResponseJson;
	}

	#endregion


	#region Class: RequestLoggerInfo
	/*
		Project Path: ..\..\..\QueryConsole\Files\Logger\LoggerInfo\RequestLoggerInfo.cs
		
	*/
	public class RequestLoggerInfo : BaseLoggerInfo
	{
		public TRequstMethod RequestMethod;
		public string Url;
		public string JsonText;
		public string AdditionalInfo;
	}

	#endregion


	#region Class: ResponseLoggerInfo
	/*
		Project Path: ..\..\..\QueryConsole\Files\Logger\LoggerInfo\ResponseLoggerInfo.cs
		
	*/
	public class ResponseLoggerInfo: BaseLoggerInfo
	{
		public string ResponseText;
	}

	#endregion


	#region Class: JsonEntityHelper
	/*
		Project Path: ..\..\..\QueryConsole\Files\MappingManager\JsonHelper.cs
		
	*/
	public static class JsonEntityHelper
	{
		public static string RefName = @"#ref";
		public static object GetSimpleTypeValue(JToken jToken)
		{
			try
			{
				if(jToken is JValue)
				{
					return ((JValue)jToken).Value;
				}
				switch (jToken.Type)
				{
					case JTokenType.String:
						return jToken.Value<string>();
					case JTokenType.Integer:
						return jToken.Value<Int64>();
					case JTokenType.Float:
						return jToken.Value<float>();
					case JTokenType.Date:
						return jToken.Value<DateTime>();
					case JTokenType.TimeSpan:
						return jToken.Value<TimeSpan>();
					case JTokenType.Boolean:
						return jToken.Value<bool>();
					default:
						return null;
				}
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e, "GetSimpleTypeValue");
				throw;
			}
		}

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
				IntegrationLogger.Error(e, "GetSimpleTypeValue");
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
			} catch(Exception e)
			{
				IntegrationLogger.Error(e, "CreateColumnValues");
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
		public static List<JObject> GetCompositeJObjects(object colValue, string colName, string entityName, string handlerName, UserConnection userConnection, int maxCount = -1)
		{
			try
			{
				var jObjectsList = new List<JObject>();
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
						var integrationInfo = new CsConstant.IntegrationInfo(new JObject(), userConnection, CsConstant.TIntegrationType.Export, null, handlerName, "", item);
						var handler = (new IntegrationEntityHelper()).GetIntegrationHandler(integrationInfo);
						if (handler != null)
						{
							jObjectsList.Add(handler.ToJson(integrationInfo));
						}
					}
					catch (Exception e)
					{
						IntegrationLogger.Error(e, string.Format("GetCompositeJObjects, {1} - {0}", item.GetTypedColumnValue<Guid>("Id"), entityName));
					}
				}
				return jObjectsList;
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e, string.Format("GetCompositeJObjects {0} {1} {2} {3}", colValue, colName, entityName, handlerName));
				return new List<JObject>();
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
			esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, CsConstant.ServiceColumnInBpm.Identifier, externalId));
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


	#region Class: MappingHelper
	/*
		Project Path: ..\..\..\QueryConsole\Files\MappingManager\MappingHelper.cs
		
	*/
	public class MappingHelper
	{

		public string RefName = @"#ref";
		public bool _isInsertToDB;
		public List<MappingItem> MapConfig;
		public UserConnection UserConnection;
		public Queue<Action> MethodQueue;
		public RulesFactory RulesFactory;
		 

		public bool IsInsertToDB
		{
			get
			{
				try
				{
					_isInsertToDB = Terrasoft.Core.Configuration.SysSettings.GetValue(UserConnection, CsConstant.SysSettingsCode.IsInsertToDB, _isInsertToDB);
				}
				catch (Exception e)
				{
					IntegrationLogger.Error(e, "IsInsertToDB");
					_isInsertToDB = false;
				}
				return _isInsertToDB;
			}
		}
		 

		public MappingHelper()
		{
			_isInsertToDB = false;
			MethodQueue = new Queue<Action>();
			RulesFactory = new RulesFactory();
		}
		 

		public void StartMappByConfig(CsConstant.IntegrationInfo integrationInfo, string jName, List<MappingItem> mapConfig, bool withHeader = true)
		{
			try
			{
				switch (integrationInfo.IntegrationType)
				{
					case CsConstant.TIntegrationType.Import:
						{
							StartMappImportByConfig(integrationInfo, jName, mapConfig, withHeader);
							break;
						}
					case CsConstant.TIntegrationType.Export:
						{
							StartMappExportByConfig(integrationInfo, jName, mapConfig);
							break;
						}
					case CsConstant.TIntegrationType.ExportResponseProcess:
						{
							StartMappExportResponseProcessByConfig(integrationInfo, jName, mapConfig);
							break;
						}
				}
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e, "StartMappByConfig");
				throw;
			}
		}

		public void StartMappImportByConfig(IntegrationInfo integrationInfo, string jName, List<MappingItem> mapConfig, bool withHeader = true)
		{
			if (integrationInfo.IntegratedEntity == null)
				throw new Exception(string.Format("Integration Entity not exist {0} ({1})", jName));
			var entityJObj = withHeader ? integrationInfo.Data[jName] : integrationInfo.Data;
			foreach (var item in mapConfig)
			{
				if (item.MapIntegrationType == CsConstant.TIntegrationType.All || item.MapIntegrationType == CsConstant.TIntegrationType.Import)
				{
					try
					{
						JToken subJObj = null;
						if (!string.IsNullOrEmpty(item.Selector))
						{
							subJObj = entityJObj.SelectToken(item.Selector);
						}
						else
						{
							subJObj = GetJTokenByPath(entityJObj, item.JSourcePath, item.MapIntegrationType, integrationInfo.IntegrationType);
						}
						if (subJObj != null)
						{
							MapColumn(item, ref subJObj, integrationInfo);
						}
					}
					catch (Exception e)
					{
						IntegrationLogger.Error(e, "StartMappImportByConfig");
						if (CsConstant.IntegrationFlagSetting.AllowErrorOnColumnAssign)
						{
							throw;
						}
					}
				}
			}
		}

		public void StartMappExportByConfig(IntegrationInfo integrationInfo, string jName, List<MappingItem> mapConfig)
		{
			integrationInfo.Data = new JObject();
			if (integrationInfo.Data[jName] == null)
				integrationInfo.Data[jName] = new JObject();
			foreach (var item in mapConfig)
			{
				if (item.MapIntegrationType == CsConstant.TIntegrationType.All || item.MapIntegrationType == CsConstant.TIntegrationType.Export)
				{
					var jObjItem = (new JObject()) as JToken;
					try
					{
						MapColumn(item, ref jObjItem, integrationInfo);
					}
					catch (Exception e)
					{
						IntegrationLogger.Info(new MappingErrorLoggerInfo()
						{
							Exception = e,
							Item = item,
							IntegrationInfo = integrationInfo
						});
						if (!item.IgnoreError)
						{
							throw;
						}
						jObjItem = null;
					}
					if (integrationInfo.Data[jName][item.JSourcePath] != null && integrationInfo.Data[jName][item.JSourcePath].HasValues)
					{
						if (integrationInfo.Data[jName][item.JSourcePath] is JArray)
							((JArray)integrationInfo.Data[jName][item.JSourcePath]).Add(jObjItem);
					}
					else
					{
						var resultJ = GetJTokenByPath(integrationInfo.Data[jName], item.JSourcePath, item.MapIntegrationType, integrationInfo.IntegrationType);
						if (jObjItem == null && item.EFieldRequier)
							throw new ArgumentNullException("Field " + item.JSourcePath + " required!");
						if (jObjItem == null && !item.SerializeIfNull)
						{
							resultJ.Parent.Remove();
							continue;
						}
						if (jObjItem != null && jObjItem.ToString() == "0" && !item.SerializeIfZero)
						{
							resultJ.Parent.Remove();
							continue;
						}
						resultJ.Replace(jObjItem);
					}
				}
			}
		}

		public void StartMappExportResponseProcessByConfig(IntegrationInfo integrationInfo, string jName, List<MappingItem> mapConfig)
		{
			var entityJObj = integrationInfo.Data[jName];
			foreach (var item in mapConfig)
			{
				try
				{
					if (item.SaveOnResponse)
					{
						var subJObj = GetJTokenByPath(entityJObj, item.JSourcePath, item.MapIntegrationType);
						MapColumn(item, ref subJObj, integrationInfo);
					}
				}
				catch (Exception e)
				{
					IntegrationLogger.Error(e, "StartMappExportResponseProcessByConfig");
					if (CsConstant.IntegrationFlagSetting.AllowErrorOnColumnAssign)
					{
						throw;
					}
				}
			}
		}
		
		public void MapColumn(MappingItem mapItem, ref JToken jToken, IntegrationInfo integrationInfo)
		{
			try
			{
				if (UserConnection == null)
					UserConnection = integrationInfo.UserConnection;
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
								userConnection = UserConnection,
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
								userConnection = UserConnection,
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
				IntegrationLogger.Info(new MappingErrorLoggerInfo()
				{
					Exception = e,
					Item = mapItem,
					IntegrationInfo = integrationInfo
				});
			}
		}

		public void ExecuteOverRuleMacros(MappingItem mapItem, ref JToken jToken, IntegrationInfo integrationInfo)
		{
			if (mapItem.OverRuleMacros.IsNullOrEmpty() || (!mapItem.AllowNullToOverMacros && jToken == null))
			{
				return;
			}
			switch (integrationInfo.IntegrationType)
			{
				case TIntegrationType.ExportResponseProcess:
				case TIntegrationType.Import:
					jToken = TsMacrosHelper.GetMacrosResultImport(mapItem.OverRuleMacros, jToken, MacrosType.OverRule, integrationInfo) as JToken;
					break;
				case TIntegrationType.Export:
					jToken = JToken.FromObject(TsMacrosHelper.GetMacrosResultExport(mapItem.OverRuleMacros, jToken, MacrosType.OverRule, integrationInfo));
					break;
			}
		}

		public bool CheckIsExist(string entityName, int externalId, string externalIdPath = "TsExternalId", int entityExternalId = 0)
		{
			if (entityExternalId != 0)
			{
				return true;
			}
			if (externalId == 0)
			{
				return false;
			}
			var select = new Select(UserConnection)
							.Column(Func.Count(Column.Const(1))).As("Count")
							.From(entityName)
							.Where(externalIdPath).IsEqual(Column.Parameter(externalId)) as Select;
			using (DBExecutor dbExecutor = UserConnection.EnsureDBConnection())
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

		public void SaveEntity(Entity entity, string jName, string ServiceName, bool onResponse = false)
		{
			try
			{
				UserConnection = entity.UserConnection;
				if (IsInsertToDB)
				{
					switch (entity.StoringState)
					{
						case StoringObjectState.New:
							if (entity.PrimaryColumnValue == Guid.Empty)
							{
								entity.Save(false);
							}
							else
							{
								entity.InsertToDB(false, false);
							}
							break;
						case StoringObjectState.Changed:
							entity.UpdateInDB(false);
							break;
					}
				}
				else
				{
					try
					{
						entity.Save(false);
					} catch(Exception e)
					{
						IntegrationLogger.Error(e, string.Format("{0} {1} {2} {3}", entity.GetType().ToString(), jName, ServiceName, onResponse));
					}
				}
				ExecuteMapMethodQueue();
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e, string.Format("{0} {1} {2} {3}", entity.GetType().ToString(), jName, ServiceName, onResponse));
			}
		}

		public JObject GetJObject(string json)
		{
			return !string.IsNullOrEmpty(json) ? JObject.Parse(json) : null;
		}
		 

		private string PrepareColumn(string columnName, bool withId = false)
		{
			var endWithId = columnName.EndsWith("Id");
			return withId ? (endWithId ? columnName : columnName + "Id") : (endWithId ? columnName.Substring(0, columnName.Length - 2) : columnName);
		}

		private bool IsAllNotNullAndEmpty(params object[] values)
		{
			foreach (var value in values)
			{
				if (value == null || (value is string && string.IsNullOrEmpty(value as string)))
					return false;
			}
			return true;
		}

		private string GetFirstNotNull(params string[] strings)
		{
			return strings.FirstOrDefault(x => !string.IsNullOrEmpty(x));
		}

		private JToken GetJTokenByPath(JToken jToken, string path, TIntegrationType mapType = TIntegrationType.Import, TIntegrationType type = TIntegrationType.Import)
		{
			return type == TIntegrationType.Export ? jToken.GetJTokenByPath(path, mapType) : jToken.GetJTokenValueByPath(path, mapType);
		}

		private void ExecuteMapMethodQueue()
		{
			while (MethodQueue.Any())
			{
				var method = MethodQueue.Dequeue();
				method();
			}
		}
		 
	}

	#endregion


	#region Class: MappingItem
	/*
		Project Path: ..\..\..\QueryConsole\Files\MappingManager\MappingItem.cs
		
	*/
	public class MappingItem
	{

		public string TsSourcePath { get; set; }
		public string TsSourceName { get; set; }

		public string JSourceName { get; set; }
		public string JSourcePath { get; set; }

		public string TsDestinationPath { get; set; }
		public string TsDestinationName { get; set; }
		public string TsDetinationValueType { get; set; }
		public string TsDestinationResPath { get; set; }

		public TMapType MapType { get; set; }
		public TMapExecuteType MapExecuteType { get; set; }
		public CsConstant.TIntegrationType MapIntegrationType { get; set; }
		public bool IFieldRequier { get; set; }
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

		public string TsExternalSource {get;set;}
		public string TsExternalPath {get;set;}
		public string TsDestinationPathToSource {get;set;}
		public string TsDestinationPathToExternal {get;set;}
		public bool SerializeIfNull {get;set;}
		public bool SerializeIfZero {get;set;}
		public bool SerializeIfEmpty { get; set; }

		public bool Key {get;set;}

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
		public MappingItem()
		{

		}
		public override string ToString()
		{
			return string.Format("MappingItem: Path = {0} DecPath = {1} EntityName = {2} Type = {3} JObjType = {4}", TsSourcePath ?? "null", TsDestinationPath ?? "null", TsSourceName ?? "null", MapType.ToString() ?? "null", JSourceName ?? "null");
		}
		 
	}

	#endregion


	#region Class: RuleExportInfo
	/*
		Project Path: ..\..\..\QueryConsole\Files\MappingManager\RuleExportInfo.cs
		
	*/
	public class RuleExportInfo: RuleInfo
	{
	}

	#endregion


	#region Class: RuleImportInfo
	/*
		Project Path: ..\..\..\QueryConsole\Files\MappingManager\RuleImportInfo.cs
		
	*/
	public class RuleImportInfo: RuleInfo
	{
		/// <summary>
		/// Выполняется после сохранения объекта
		/// </summary>
		public Action AfterEntitySave;
	}

	#endregion


	#region Class: RuleInfo
	/*
		Project Path: ..\..\..\QueryConsole\Files\MappingManager\RuleInfo.cs
		
	*/
	public class RuleInfo
	{
		public MappingItem config;
		public Entity entity;
		public JToken json;
		public UserConnection userConnection;
		public CsConstant.TIntegrationType integrationType;
		public string action;
	}

	#endregion


	#region Class: RulesFactory
	/*
		Project Path: ..\..\..\QueryConsole\Files\MappingManager\RulesFactory.cs
		
	*/
	public class RulesFactory
	{
		public List<IMappRule> Rules;
		public RulesFactory() {
			Rules = new List<IMappRule>() {
				new SimpleMappRule(),
				new ReferensToEntityMappRule(),
				new CompositMappRule(),
				new ConstMappRule(),
				new ArrayOfCompositeObjectMappRule(),
				new ArrayOfReferenceMappRule(),
				new ComplexFieldMappRule(),
				new ManyToManyMappRule(),
				new ToDetailMappRule()
			};
		}
		public IMappRule GetRule(string type) {
			type = type.ToLower();
			return Rules.FirstOrDefault(x => x.Type == type);
		}
	}

	#endregion


	#region Class: TsMacrosHelper
	/*
		Project Path: ..\..\..\QueryConsole\Files\MappingManager\TsMacrosHelper.cs
		
	*/
	public static class TsMacrosHelper
	{
		public static Dictionary<string, Func<object, object>> MacrosDictImport = new Dictionary<string, Func<object, object>>() {
			{ "DateTimeToYearInteger", x => YearIntegerToDateTime(x)},
			{ "DateWithouTime", x => DateWithoutTime(x) },
			{ "TimeSpanToDate", x => TimeSpanToDate(x) },
			{ "TimeSpanToDateTime", x => TimeSpanToDateTime(x) },
			{ "ToDateTime", x => ToDateTime(x) }
		};
		public static Dictionary<string, Func<object, object>> MacrosDictExport = new Dictionary<string, Func<object, object>>() {
			{ "DateTimeToYearInteger", x => DateTimeToYearInteger(x)},
			{ "DateWithouTime", x => DateWithoutTime(x) },
			{ "TimeSpanToDate", x => DateToTimeSpan(x) },
			{ "TimeSpanToDateTime", x => DateToTimeSpan(x) }
		};
		public static Dictionary<string, Func<object, CsConstant.IntegrationInfo, object>> OverMacrosDictImport = new Dictionary<string, Func<object, CsConstant.IntegrationInfo, object>>() {
			{ "ConvertJson", (x, u) => ConvertStringToJson(x)},
			{ "ConvertJsonArray", (x, u) => ConvertJsonToArray(x)},
			{ "ToLdapName", (x, u)=> LdapNameToSimpleName(x) },
			{ "ToIsoFormat", (x, u) => FromIsoFormat(x) },
			{ "ParseOrderNumber", (x, u) => ParseOrderNumber(x) },
			{ "EmptyStringIfNull",(x, u) => NullIfEmptyString(x) }
		};
		public static Dictionary<string, Func<object, CsConstant.IntegrationInfo, object>> OverMacrosDictExport = new Dictionary<string, Func<object, CsConstant.IntegrationInfo, object>>() {
			{ "ConvertJson", (x, u) => ConvertJsonToString(x)},
			{ "ConvertJsonArray", (x, u) => ConvertArrayToJson(x)},
			{ "ToLdapName", (x, u) => ToLdapName(x) },
			{ "ToIsoFormat", (x, u) => ToIsoFormat(x) },
			{ "EmptyStringIfNull", (x, u) => EmptyStringIfNull(x) },
			{ "GetIsoByAccountId", (x, u) => GetIsoByAccountId(x, u) }
		};
		public static Dictionary<string, Action<object, UserConnection>> BeforeDeleteMacros = new Dictionary<string, Action<object, UserConnection>>() {
			{ "BeforeDeleteContactCommunication", (x, y) => BeforeDeleteContactCommunication(x, y) },
			{ "BeforeDeleteShipmentPosition", (x, y) => BeforeDeleteShipmentPosition(x, y) }
		};
		public static object GetMacrosResultImport(string macrosName, object value, MacrosType type = MacrosType.Rule, CsConstant.IntegrationInfo integrationInfo = null)
		{
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
			if (BeforeDeleteMacros.ContainsKey(macrosName))
			{
				BeforeDeleteMacros[macrosName](value, userConnection);
			}
		}
		public static Func<object, object> ToLdapName = (x) =>
		{
			var ldapName = CsConstant.IntegratorSettings.LdapDomainName;
			if (!string.IsNullOrEmpty(ldapName))
			{
				return string.Format(@"{0}@{1}", x.ToString(), ldapName);
			}
			return x;
		};
		public static Func<object, object> LdapNameToSimpleName = (x) =>
		{
			if (x == null)
			{
				return x;
			}
			var text = x.ToString();
			var parts = text.Split(new char[] { '@' });
			return parts.FirstOrDefault();
		};

		public static Func<object, object> DateTimeToYearInteger = (x) =>
		{
			if (x == null)
				return null;
			if (x is string)
			{
				DateTime dateTimeResult = DateTime.MinValue;
				if (DateTime.TryParse((string)x, out dateTimeResult))
				{
					return dateTimeResult.ToUniversalTime().Year;
				}
			}
			if (x is DateTime)
			{
				return ((DateTime)x).Year;
			}
			return x;
		};
		public static Func<object, object> DateWithoutTime = (x) =>
		{
			if (x == null)
				return null;
			if (x is string)
			{
				DateTime dateTimeResult = DateTime.MinValue;
				if (((string)x).Contains("T"))
				{
					var dateStr = (string)x;
					if (DateTime.TryParse(dateStr.Substring(0, dateStr.IndexOf("T")), out dateTimeResult))
					{
						return DateTime.SpecifyKind(dateTimeResult, DateTimeKind.Utc).Date;
					}
				}
				if (!DateTime.TryParse((string)x, out dateTimeResult))
				{
					return DateTime.SpecifyKind(dateTimeResult, DateTimeKind.Utc).Date;
				}
			}
			if (x is DateTime)
			{
				return ((DateTime)x).Date;
			}
			return x;
		};

		public static DateTime StartEpohDate = Convert.ToDateTime("1 January 1970");
		public static Func<object, object> TimeSpanToDate = (x) =>
		{
			if (x == null)
				return null;
			if (x is string)
			{
				var timeSpanInt = 0;
				if (int.TryParse((string)x, out timeSpanInt))
				{
					var timeSpan = TimeSpan.FromMilliseconds(timeSpanInt);
					return StartEpohDate.Add(timeSpan).Date;
				}
			}
			if (x is Int64)
			{
				var timeSpan = TimeSpan.FromMilliseconds((Int64)x);
				return StartEpohDate.Add(timeSpan).Date;
			}
			return x;
		};
		public static Func<object, object> TimeSpanToDateTime = (x) =>
		{
			if (x == null)
				return null;
			if (x is string)
			{
				var timeSpanInt = 0;
				if (int.TryParse((string)x, out timeSpanInt))
				{
					var timeSpan = TimeSpan.FromMilliseconds(timeSpanInt);
					return StartEpohDate.Add(timeSpan);
				}
			}
			if (x is int)
			{
				var timeSpan = TimeSpan.FromMilliseconds((int)x);
				return StartEpohDate.Add(timeSpan);
			}
			return x;
		};

		public static Func<object, object> ToDateTime = (x) =>
		{
			if (x == null)
				return null;
			if (x is string)
			{
				DateTime date;
				if (DateTime.TryParse((string)x, out date))
				{
					return date.ToUniversalTime();
				}
			}
			if (x is int)
			{
				var timeSpan = TimeSpan.FromMilliseconds((int)x);
				return StartEpohDate.Add(timeSpan);
			}
			if (x is Int64)
			{
				var timeSpan = TimeSpan.FromMilliseconds((Int64)x);
				return StartEpohDate.Add(timeSpan);
			}
			return x;
		};

		public static Func<object, object> ConvertJsonToString = (x) =>
		{
			if (x == null)
				return null;
			if (x is JToken)
			{
				return ((JToken)x).ToString();
			}
			return x;
		};
		public static Func<object, object> ConvertStringToJson = (x) =>
		{
			if (x == null)
				return null;
			if (x is string)
			{
				return JToken.Parse((string)x);
			}
			return x;
		};
		public static Func<object, object> ConvertJsonToArray = (x) =>
		{
			if (x == null)
				return null;
			if (x is string)
			{
				return JToken.Parse((string)x);
			}
			return x;
		};
		public static Func<object, object> ConvertArrayToJson = (x) =>
		{
			if (x == null)
				return null;
			if (x is IEnumerable)
			{
				return JArray.FromObject((IEnumerable)x);
			}
			return x;
		};


		public static Func<object, object> YearIntegerToDateTime = (x) =>
		{
			if (x == null)
				return null;
			if (x is string)
			{
				var year = 0;
				if (int.TryParse((string)x, out year))
				{
					return new DateTime(year, 1, 1);
				}
			}
			if (x is int)
			{
				return new DateTime((int)x, 1, 1);
			}
			if (x is Int64)
			{
				if ((Int64)x > Int32.MaxValue)
				{
					return new DateTime(Int32.MaxValue - 1, 1, 1);
				}
				return new DateTime(Convert.ToInt32((Int64)x), 1, 1);
			}
			return x;
		};
		public static Func<object, object> DateToTimeSpan = (x) =>
		{
			if (x == null)
				return null;
			if (x is string)
			{
				DateTime date;
				if (DateTime.TryParse((string)x, out date))
				{
					return (date - StartEpohDate).TotalMilliseconds;
				}
			}
			if (x is DateTime)
			{
				return (((DateTime)x) - StartEpohDate).TotalMilliseconds;
			}
			return x;
		};
		public static Func<object, CsConstant.IntegrationInfo, object> GetIsoByAccountId = (x, integrationInfo) =>
		{
			try
			{
				if (x == null)
					return null;
				Guid accountId;
				if (x is JValue && Guid.TryParse((x as JValue).Value.ToString(), out accountId))
				{
					var selectIso = new Select(integrationInfo.UserConnection)
										.Top(1)
										.Column("c", "TsCountryCode")
										.From("AccountAddress").As("a")
										.InnerJoin("Country").As("c")
										.On("a", "CountryId").IsEqual("c", "Id")
										.Where("a", "AccountId").IsEqual(Column.Parameter(accountId))
										.OrderByDesc("a", "CreatedOn") as Select;
					var seletWithTypeFilter = new Select(integrationInfo.UserConnection)
										.Top(1)
										.Column("c", "TsCountryCode")
										.From("AccountAddress").As("a")
										.InnerJoin("Country").As("c")
										.On("a", "CountryId").IsEqual("c", "Id")
										.Where("a", "AccountId").IsEqual(Column.Parameter(accountId))
										.And("a", "AddressTypeId").IsEqual(Column.Parameter(CsConstant.EntityConst.AddressType.Legal))
										.OrderByDesc("a", "CreatedOn") as Select;
					using (var dbExecutor = integrationInfo.UserConnection.EnsureDBConnection())
					{
						using (var reader = seletWithTypeFilter.ExecuteReader(dbExecutor))
						{
							if (reader.Read())
							{
								return reader.GetColumnValue<string>("TsCountryCode");
							}
						}
						using (var readerAll = selectIso.ExecuteReader(dbExecutor))
						{
							if (readerAll.Read())
							{
								return readerAll.GetColumnValue<string>("TsCountryCode");
							}
						}
					}
				}
			} catch(Exception e)
			{
				IntegrationLogger.Error(e, integrationInfo.ToString());
			}
			return string.Empty;
		};
		public static Action<object, UserConnection> BeforeDeleteContactCommunication = (x, userConnection) =>
		{
			if (x == null)
				return;
			if (x is Select)
			{
				var list = (Select)x;
				try
				{
					var updateAccountNotification = new Update(userConnection, "TsAccountNotification")
								.Set("TsCommunicationId", Column.Const(null))
								.Where("TsCommunicationId").In(list) as Update;
					updateAccountNotification.Execute();
				}
				catch (Exception e)
				{
					IntegrationLogger.Error(e);
				}
				try
				{
					var updateContactNotification = new Update(userConnection, "TsContactNotifications")
								.Set("TsCommunicationMeansId", Column.Const(null))
								.Where("TsCommunicationMeansId").In(list) as Update;
					updateContactNotification.Execute();
				}
				catch (Exception e)
				{
					IntegrationLogger.Error(e);
				}
			}
		};
		public static Action<object, UserConnection> BeforeDeleteShipmentPosition = (x, userConnection) =>
		{
			if (x == null)
				return;
			if (x is Select)
			{
				var list = (Select)x;
				try
				{
					var deleteReturnPosition = new Delete(userConnection)
								.From("TsShipmentPosition")
								.Where("TsShipmentId").In(list) as Delete;
					deleteReturnPosition.Execute();
				}
				catch (Exception e)
				{
					IntegrationLogger.Error(e);
				}
			}
		};
		public static Func<object, object> ToIsoFormat = (x) =>
		{
			if(x is JToken)
			{
				var value = ((JToken)x).Value<string>();
				if (!string.IsNullOrEmpty(value))
				{
					var isoFormat = TzdbDateTimeZoneSource.Default.MapTimeZoneId(TimeZoneInfo.FindSystemTimeZoneById(value));
					if (isoFormat != null && !string.IsNullOrEmpty(isoFormat))
					{
						return isoFormat;
					}
				}
			}
			return x;
		};
		public static Func<object, object> EmptyStringIfNull = (x) =>
		{
			if(x == null)
			{
				return string.Empty;
			}
			if (x is JToken)
			{
				var value = ((JToken)x).Value<string>();
				if (value == null)
				{
					return string.Empty;
				}
			}
			return x;
		};
		public static Func<object, object> FromIsoFormat = (x) =>
		{
			if (x is JToken)
			{
				var value = ((JToken)x).Value<string>();
				if (!string.IsNullOrEmpty(value))
				{
					var codeFormat = TzdbDateTimeZoneSource.Default.WindowsMapping.MapZones.FirstOrDefault(y => y.TzdbIds.Contains((string)value) && !string.IsNullOrEmpty(y.WindowsId));
					if (codeFormat != null)
					{
						return JToken.FromObject(codeFormat.WindowsId);
					}
				}
			}
			return x;
		};
		public static Func<object, object> ParseOrderNumber = (x) =>
		{
			if (x is JToken)
			{
				var value = ((JToken)x).Value<string>();
				if (!string.IsNullOrEmpty(value))
				{
					var orderNumberMatch = Regex.Match(value, @"\d+");
					if (orderNumberMatch.Success && orderNumberMatch.Length > 0 && !string.IsNullOrEmpty(orderNumberMatch.Value))
					{
						return JToken.FromObject(orderNumberMatch.Value);
					}
				}
			}
			return x;
		};
		public static Func<object, object> NullIfEmptyString = (x) =>
		{
			if (x is JToken)
			{
				var value = ((JToken)x).Value<string>();
				if (value == string.Empty)
				{
					return JToken.FromObject(null);
				}
			}
			return x;
		};
	}

	#endregion


	#region Class: ArrayOfCompositeObjectMappRule
	/*
		Project Path: ..\..\..\QueryConsole\Files\MappingManager\MappRule\ArrayOfCompositeObjectMappRule.cs
		
	*/
	public class ArrayOfCompositeObjectMappRule: BaseMappRule {
		public ArrayOfCompositeObjectMappRule() {
			_type = "arrayofcompositobject";
		}
		public override void Import(RuleImportInfo info) {
			if (info.integrationType == CsConstant.TIntegrationType.ExportResponseProcess && !info.config.SaveOnResponse) {
				return;
			}
			if (info.json is JArray) {
				var jArray = (JArray)info.json;
				var handlerName = info.config.HandlerName;
				var integrator = new IntegrationEntityHelper();
				var integrateIds = new List<QueryColumnExpression>();
				try {
					foreach (JToken jArrayItem in jArray) {
						JObject jObj = jArrayItem as JObject;
						handlerName = handlerName ?? jObj.Properties().First().Name;
						var objIntegrInfo = new CsConstant.IntegrationInfo(jObj, info.userConnection, info.integrationType, null, handlerName, CsConstant.IntegrationActionName.Update);
						objIntegrInfo.ParentEntity = info.entity;
						integrator.IntegrateEntity(objIntegrInfo);
						if (info.config.DeleteBeforeExport) {
							integrateIds.Add(Column.Parameter(objIntegrInfo.IntegratedEntity.GetTypedColumnValue<Guid>("Id")));
						}
					}
					if (info.config.DeleteBeforeExport) {
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
								TsMacrosHelper.ExecuteBeforeDeleteMacros(info.config.BeforeDeleteMacros, idSelect, info.userConnection);
							}
							var delete = new Delete(info.userConnection)
									.From(info.config.TsDestinationName)
									.Where("Id").In(idSelect) as Delete;
							delete.Execute();
						} else
						{
							var delete = new Delete(info.userConnection)
									.From(info.config.TsDestinationName)
									.Where(destColumnName).IsEqual(Column.Parameter(info.entity.GetColumnValue(info.config.TsSourcePath))) as Delete;
							delete.Execute();
						}
					}
				} catch (Exception e) {
					throw new Exception("Mapp Rule arrayofcompositobject, import", e);
				}
			}
		}
		public override void Export(RuleExportInfo info) {
			try {
				if (JsonEntityHelper.IsAllNotNullAndEmpty(info.entity, info.config.TsSourcePath, info.config.TsDestinationPath, info.config.TsDestinationName)) {
					var srcEntity = info.entity;
					var dscValue = srcEntity.GetColumnValue(info.config.TsSourcePath);
					string handlerName = JsonEntityHelper.GetFirstNotNull(info.config.HandlerName, info.config.TsDestinationName, info.config.JSourceName);
					var resultJObjs = JsonEntityHelper.GetCompositeJObjects(dscValue, info.config.TsDestinationPath, info.config.TsDestinationName, handlerName, info.userConnection);
					if (resultJObjs.Any()) {
						var jArray = (info.json = new JArray()) as JArray;
						resultJObjs.ForEach(x => jArray.Add(x));
					} else {
						info.json = null;
					}
				}
			} catch (Exception e) {
				throw new Exception("Mapp Rule arrayofcompositobject, export", e);
			}
		}
	}

	#endregion


	#region Class: ArrayOfReferenceMappRule
	/*
		Project Path: ..\..\..\QueryConsole\Files\MappingManager\MappRule\ArrayOfReferenceMappRule.cs
		
	*/
	public class ArrayOfReferenceMappRule: BaseMappRule
	{
		public ArrayOfReferenceMappRule()
		{
			_type = "arrayofreference";
		}
		public override void Import(RuleImportInfo info)
		{
			try
			{
				if (info.json != null && info.json is JArray)
				{
					var jArray = (JArray) info.json;
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
		public override void Export(RuleExportInfo info)
		{
			if (JsonEntityHelper.IsAllNotNullAndEmpty(info.entity, info.config.TsDestinationName, info.config.TsSourcePath, info.config.JSourceName))
			{
				var srcValue = info.entity.GetTypedColumnValue<Guid>(info.config.TsSourcePath);
				var jArray = new JArray();
				var resultList = JsonEntityHelper.GetColumnValues(info.userConnection, info.config.TsDestinationName, info.config.TsDestinationPath, srcValue, info.config.TsDestinationResPath);
				foreach (var resultItem in resultList)
				{
					var extId = int.Parse(resultItem.ToString());
					if (extId != 0)
					{
						jArray.Add(JToken.FromObject(CsReference.Create(extId, info.config.JSourceName)));
					}
				}
				info.json = jArray;
			}
			else
			{
				info.json = null;
			}
		}
	}

	#endregion


	#region Class: BaseMappRule
	/*
		Project Path: ..\..\..\QueryConsole\Files\MappingManager\MappRule\BaseMappRule.cs
		
	*/
	public abstract class BaseMappRule: IMappRule
	{
		protected string _type;
		public string Type
		{
			get
			{
				return _type;
			}
			set
			{
				_type = value;
			}
		}

		protected Entity _entity;
		public Entity Entity
		{
			get
			{
				return _entity;
			}
			set
			{
				_entity = value;
			}
		}

		protected JObject _json;
		public JObject Json
		{
			get
			{
				return _json;
			}
			set
			{
				_json = value;
			}
		}

		public virtual void Import(RuleImportInfo info)
		{
			throw new NotImplementedException();
		}

		public virtual void Export(RuleExportInfo info)
		{
			throw new NotImplementedException();
		}
	}

	#endregion


	#region Class: ComplexFieldMappRule
	/*
		Project Path: ..\..\..\QueryConsole\Files\MappingManager\MappRule\ComplexFieldMappRule.cs
		
	*/
	public class ComplexFieldMappRule: BaseMappRule
	{
		public ComplexFieldMappRule()
		{
			_type = "firstdestinationfield";
		}
		public override void Import(RuleImportInfo info)
		{
			object resultId = null;
			if (info.json != null)
			{
				var newValue = JsonEntityHelper.GetSimpleTypeValue(info.json);
				if (newValue != null && (info.json.Type != JTokenType.String || newValue.ToString() != ""))
				{
					resultId = JsonEntityHelper.GetColumnValues(info.userConnection, info.config.TsDestinationName, info.config.TsDestinationResPath, newValue, info.config.TsDestinationPath, 1).FirstOrDefault();
					if (info.config.CreateIfNotExist && (resultId == null || (resultId is string && (string)resultId == string.Empty) || (resultId is Guid && (Guid)resultId == Guid.Empty)))
					{
						Dictionary<string, string> defaultColumn = null;
						if(!string.IsNullOrEmpty(info.config.TsTag)) {
							defaultColumn = JsonEntityHelper.ParsToDictionary(info.config.TsTag, '|', ',');
							foreach(var columnKey in defaultColumn.Keys.ToList()) {
								string value = defaultColumn[columnKey];
								if (value.StartsWith("$")) {
									defaultColumn[columnKey] = GetAdvancedSelectTokenValue(info.json, value.Substring(1));
								}
							}
						}
						resultId = JsonEntityHelper.CreateColumnValues(info.userConnection, info.config.TsDestinationName, info.config.TsDestinationResPath, newValue, info.config.TsDestinationPath, 1, "CreateOn", Common.OrderDirection.Descending, defaultColumn).FirstOrDefault();
					}
				}
			}
			info.entity.SetColumnValue(info.config.TsSourcePath, resultId);
		}
		public override void Export(RuleExportInfo info)
		{
			object resultObject = null;
			if (JsonEntityHelper.IsAllNotNullAndEmpty(info.entity, info.config.TsDestinationName, info.config.TsSourcePath, info.config.TsDestinationPath, info.config.TsDestinationResPath))
			{
				var sourceValue = info.entity.GetColumnValue(info.config.TsSourcePath);
				resultObject = JsonEntityHelper.GetColumnValues(info.userConnection, info.config.TsDestinationName, info.config.TsDestinationPath, sourceValue, info.config.TsDestinationResPath).FirstOrDefault();
			}
			info.json = resultObject != null ? JToken.FromObject(resultObject) : null;
		}
		public string GetAdvancedSelectTokenValue(JToken jToken, string path) {
			if (path.StartsWith(".-") && jToken.Parent != null) {
				return GetAdvancedSelectTokenValue(jToken.Parent, path.Substring(2));
			}
			if (jToken != null) {
				var resultToken = jToken.SelectToken(path);
				if(resultToken != null) {
					return resultToken.Value<string>();
				}
			}
			return string.Empty;
		}
	}

	#endregion


	#region Class: CompositMappRule
	/*
		Project Path: ..\..\..\QueryConsole\Files\MappingManager\MappRule\CompositMappRule.cs
		
	*/
	public class CompositMappRule: BaseMappRule
	{
		public CompositMappRule()
		{
			_type = "compositobject";
		}
		public override void Import(RuleImportInfo info)
		{
			var integrator = new IntegrationEntityHelper();
			//var objIntegrInfo = new IntegrationInfo(jToken, integrationInfo.UserConnection, integrationInfo.IntegrationType, null, null, integrationInfo.Action);
			var jObject = info.json as JObject;
			var objIntegrInfo = new CsConstant.IntegrationInfo(jObject, info.userConnection, info.integrationType, null, jObject.Properties().First().Name, info.action);
			integrator.IntegrateEntity(objIntegrInfo);
		}
		public override void Export(RuleExportInfo info)
		{
			object resultJObj = null;
			if (JsonEntityHelper.IsAllNotNullAndEmpty(info.entity, info.config.TsSourcePath, info.config.TsDestinationPath, info.config.TsDestinationName, info.config.JSourceName))
			{
				var srcEntity = info.entity;
				var dscId = srcEntity.GetColumnValue(info.config.TsSourcePath);
				string handlerName = JsonEntityHelper.GetFirstNotNull(info.config.HandlerName, info.config.TsDestinationName, info.config.JSourceName);
				resultJObj = JsonEntityHelper.GetCompositeJObjects(dscId, info.config.TsDestinationPath, info.config.TsDestinationName, handlerName, info.userConnection, 1).FirstOrDefault();
			}
			info.json = resultJObj as JToken;
		}
	}

	#endregion


	#region Class: ConstMappRule
	/*
		Project Path: ..\..\..\QueryConsole\Files\MappingManager\MappRule\ConstMappRule.cs
		
	*/
	public class ConstMappRule: BaseMappRule
	{
		public ConstMappRule()
		{
			_type = "const";
		}
		public override void Import(RuleImportInfo info)
		{
			//throw new NotImplementedException();
		}
		public override void Export(RuleExportInfo info)
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
			info.json = resultValue != null ? JToken.FromObject(resultValue) : null;
		}
	}

	#endregion


	#region Class: ManyToManyMappRule
	/*
		Project Path: ..\..\..\QueryConsole\Files\MappingManager\MappRule\ManyToManyMappRule.cs
		
	*/
	public class ManyToManyMappRule: BaseMappRule
	{
		public ManyToManyMappRule()
		{
			_type = "manytomany";
		}
		public override void Import(RuleImportInfo info)
		{
			if (info.json != null && info.json.HasValues)
			{
				var jArray = info.json as JArray;
				foreach (var refItem in jArray)
				{
					var item = refItem[JsonEntityHelper.RefName];
					var externalId = int.Parse(item["id"].ToString());
					var type = item["type"];
					Tuple<Dictionary<string, string>, Entity> tuple = JsonEntityHelper.GetEntityByExternalId(info.config.TsExternalSource, externalId, info.userConnection, false, info.config.TsExternalPath);
					Dictionary<string, string> columnDict = tuple.Item1;
					Entity entity = tuple.Item2;
					if(entity != null) {
						if(!JsonEntityHelper.isEntityExist(info.config.TsDestinationName, info.userConnection, new Dictionary<string,object>() {
							{ info.config.TsDestinationPathToSource, info.entity.GetTypedColumnValue<Guid>(info.config.TsSourcePath) },
							{ info.config.TsDestinationPathToExternal, entity.GetTypedColumnValue<Guid>(columnDict[info.config.TsExternalPath]) }
						})) {
							var schema = info.userConnection.EntitySchemaManager.GetInstanceByName(info.config.TsDestinationName);
							var destEntity = schema.CreateEntity(info.userConnection);
							var firstColumn = schema.Columns.GetByName(info.config.TsDestinationPathToExternal).ColumnValueName;
							var secondColumn = schema.Columns.GetByName(info.config.TsDestinationPathToSource).ColumnValueName;
							destEntity.SetColumnValue(firstColumn, entity.GetTypedColumnValue<Guid>(columnDict[info.config.TsExternalPath]));
							destEntity.SetColumnValue(secondColumn, info.entity.GetTypedColumnValue<Guid>(info.config.TsSourcePath));
							destEntity.Save(false);
						}
					}
				}
			}
		}
		public override void Export(RuleExportInfo info)
		{
			
		}
	}

	#endregion


	#region Class: ReferensToEntityMappRule
	/*
		Project Path: ..\..\..\QueryConsole\Files\MappingManager\MappRule\ReferensToEntityMappRule.cs
		
	*/
	public class ReferensToEntityMappRule : BaseMappRule
	{
		public ReferensToEntityMappRule()
		{
			_type = "reftoguid";
		}

		public override void Import(RuleImportInfo info)
		{
			Guid? resultGuid = null;
			if (info.json != null && info.json.HasValues)
			{
				var refColumns = info.json[JsonEntityHelper.RefName];
				var externalId = int.Parse(refColumns["id"].ToString());
				var type = refColumns["type"].Value<string>();
				Func<Guid?> resultGuidAction = () => JsonEntityHelper.GetColumnValues(info.userConnection, info.config.TsDestinationName, info.config.TsExternalIdPath,
						externalId, info.config.TsDestinationPath, -1, "CreatedOn", Terrasoft.Common.OrderDirection.Descending,
						JsonEntityHelper.ParsToDictionary(info.config.TsTag, '|', ',')).FirstOrDefault() as Guid?;
				if (info.config.LoadDependentEntity)
				{
					DependentEntityLoader.LoadDependenEntity(type, externalId, info.userConnection, () =>
					{
						resultGuid = resultGuidAction();
					}, IntegrationLogger.SimpleLoggerErrorAction);
				}
				else
				{
					resultGuid = resultGuidAction();
				}
			}
			if (!info.config.IsAllowEmptyResult && (resultGuid == null || resultGuid.Value == Guid.Empty))
			{
				return;
			}
			info.entity.SetColumnValue(info.config.TsSourcePath, resultGuid);
		}

		public override void Export(RuleExportInfo info)
		{
			object resultObj = null;
			if (JsonEntityHelper.IsAllNotNullAndEmpty(info.entity, info.config.TsDestinationName, info.config.TsSourcePath,
				info.config.JSourceName, info.config.TsDestinationPath))
			{
				var resultValue =
					JsonEntityHelper.GetColumnValues(info.userConnection, info.config.TsDestinationName, info.config.TsDestinationPath,
							info.entity.GetTypedColumnValue<Guid>(info.config.TsSourcePath), info.config.TsExternalIdPath)
						.FirstOrDefault(x => (int) x > 0);
				if (resultValue != null)
				{
					var resultRef = CsReference.Create(int.Parse(resultValue.ToString()), info.config.JSourceName);
					resultObj = resultRef != null ? JToken.FromObject(resultRef) : null;
				}
			}
			info.json = resultObj as JToken;
		}
	}

	#endregion


	#region Class: SimpleMappRule
	/*
		Project Path: ..\..\..\QueryConsole\Files\MappingManager\MappRule\SimpleMappRule.cs
		
	*/
	public class SimpleMappRule : BaseMappRule
	{
		public SimpleMappRule() {
			_type = "simple";
		}
		public override void Import(RuleImportInfo info)
		{
			object value = JsonEntityHelper.GetSimpleTypeValue(info.json);
			if (!string.IsNullOrEmpty(info.config.MacrosName))
			{
				value = TsMacrosHelper.GetMacrosResultImport(info.config.MacrosName, value);
			}
			info.entity.SetColumnValue(info.config.TsSourcePath, value);
		}
		public override void Export(RuleExportInfo info)
		{
			var value = info.entity.GetColumnValue(info.config.TsSourcePath);
			var simpleResult = value != null ? JsonEntityHelper.GetSimpleTypeValue(value) : null;
			if (!string.IsNullOrEmpty(info.config.MacrosName))
			{
				simpleResult = TsMacrosHelper.GetMacrosResultExport(info.config.MacrosName, simpleResult);
				if (simpleResult is DateTime)
				{
					simpleResult = ((DateTime)simpleResult).ToString("yyyy-MM-dd");
				}
			}
			
			info.json = simpleResult != null ? JToken.FromObject(simpleResult) : null;
		}
	}

	#endregion


	#region Class: ToDetailMappRule
	/*
		Project Path: ..\..\..\QueryConsole\Files\MappingManager\MappRule\ToDetailMappRule.cs
		
	*/
	class ToDetailMappRule : BaseMappRule
	{
		public ToDetailMappRule()
		{
			_type = "todetail";
		}
		public override void Import(RuleImportInfo info)
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
					if (!string.IsNullOrEmpty(info.config.TsDetailTag)) {
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
		public override void Export(RuleExportInfo info)
		{
			object resultObject = null;
			var sourceValue = info.entity.GetColumnValue(info.config.TsSourcePath);
			var optionalColumns = JsonEntityHelper.ParsToDictionary(info.config.TsDetailTag, '|', ',');
			var detailValue = JsonEntityHelper.GetColumnValuesWithFilters(info.userConnection, info.config.TsDetailName, info.config.TsDetailPath, sourceValue, info.config.TsDetailResPath, optionalColumns).FirstOrDefault();
			if (info.config.TsTag == "simple")
			{
				resultObject = detailValue;
			}
			else if (info.config.TsTag == "stringtoguid")
			{
				resultObject = JsonEntityHelper.GetColumnValues(info.userConnection, info.config.TsDestinationName, info.config.TsDestinationPath, detailValue, info.config.TsDestinationResPath).FirstOrDefault();
			}
			info.json = resultObject != null ? JToken.FromObject(resultObject) : null;
		}

		public IEnumerable<Tuple<string, string>> ParseDetailTag(string tag)
		{
			if(string.IsNullOrEmpty(tag)) {
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


	#region Class: PrimaryExportParam
	/*
		Project Path: ..\..\..\QueryConsole\Files\PrimaryExport\PrimaryExportParam.cs
		
	*/
	public class PrimaryExportParam
	{
		public string EntityName;
		public bool OnlyNew;
		public EntityHandler EntityHandler;
		public Action<Select> FilterAction;
		public string ExternalIdName;
		public bool WithRate;
		public int RateCount;

		public PrimaryExportParam(string entityName, bool onlyNew, EntityHandler entityHandler, string externalIdName, Action<Select> filterAction = null, int rateCount = 100)
		{
			EntityName = entityName;
			OnlyNew = onlyNew;
			EntityHandler = entityHandler;
			FilterAction = filterAction;
			ExternalIdName = externalIdName;
			RateCount = rateCount;
		}
	}

	#endregion


	#region Class: PrimaryExportProvider
	/*
		Project Path: ..\..\..\QueryConsole\Files\PrimaryExport\PrimaryExportProvider.cs
		
	*/
	public class PrimaryExportProvider
	{
		private BaseServiceIntegrator _integrator;
		public UserConnection UserConnection;
		public PrimaryExportProvider(UserConnection userConnection)
		{
			UserConnection = userConnection;
			_integrator = new OrderServiceIntegrator(UserConnection);
		}

		public PrimaryExportProvider Run(PrimaryExportParam param)
		{
			try
			{
				var entitySelect = new Select(UserConnection)
					.Column("EntitySrc", "Id")
					.From(param.EntityName).As("EntitySrc") as Select;
				if (param.OnlyNew)
				{
					entitySelect.Where("EntitySrc", param.ExternalIdName).IsEqual(Column.Const(0));
				}
				if (param.FilterAction != null)
				{
					param.FilterAction(entitySelect);
				}
				using (var dbExecutor = UserConnection.EnsureDBConnection())
				{
					dbExecutor.ExecuteSelectWithPaging(entitySelect, 0, param.RateCount, "[EntitySrc].[CreatedOn]", reader =>
					{
						while (reader.Read())
						{
							var id = reader.GetColumnValue<Guid>("Id");
							_integrator.IntegrateBpmEntity(id, param.EntityName, param.EntityHandler);
						}
					}, IntegrationLogger.SimpleLoggerErrorAction);
				}
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e);
			}
			return this;
		}
	}

	#endregion


	#region Class: PrimaryExportScenario
	/*
		Project Path: ..\..\..\QueryConsole\Files\PrimaryExport\PrimaryExportScenario.cs
		
	*/
	public class PrimaryExportScenario
	{
		private readonly bool _exportOnlyNew;
		private int _rateCount;
		private PrimaryExportProvider _provider;

		public PrimaryExportScenario(UserConnection userConnection, bool exportOnlyNew, int rateCount)
		{
			_exportOnlyNew = exportOnlyNew;
			_rateCount = rateCount;
			_provider = new PrimaryExportProvider(userConnection);
		}

		public void Run()
		{
			try
			{
				_provider
					.Run(new PrimaryExportParam("TsLocSalMarket", _exportOnlyNew, null, "TsExternalId"))
					.Run(new PrimaryExportParam("SysAdminUnit", _exportOnlyNew, null, "TsExternalId",
						select => (select.HasCondition ? select.And("EntitySrc", "SysAdminUnitTypeValue") : select.Where("EntitySrc", "SysAdminUnitTypeValue")).IsLess(Column.Const(4)), _rateCount))
					.Run(new PrimaryExportParam("Contact", _exportOnlyNew, new ContactHandler(), "TsExternalId",
						select => select.InnerJoin("SysAdminUnit").As("sauUser").On("sauUser", "ContactId").IsEqual("EntitySrc", "Id"), _rateCount))
					.Run(new PrimaryExportParam("SysAdminUnit", _exportOnlyNew, null, "TsExternalId",
						select => (select.HasCondition ? select.And("EntitySrc", "SysAdminUnitTypeValue") : select.Where("EntitySrc", "SysAdminUnitTypeValue")).IsEqual(Column.Const(4)), _rateCount))
					.Run(new PrimaryExportParam("Account", _exportOnlyNew, new AccountHandler(), "TsExternalId", null, _rateCount))
					.Run(new PrimaryExportParam("Contact", _exportOnlyNew, new ContactHandler(), "TsExternalId", null, _rateCount))
					.Run(new PrimaryExportParam("SysAdminUnit", _exportOnlyNew, null, "TsExternalId",
						select => (select.HasCondition ? select.And("EntitySrc", "SysAdminUnitTypeValue") : select.Where("EntitySrc", "SysAdminUnitTypeValue")).IsEqual(Column.Const(4)), _rateCount))
					.Run(new PrimaryExportParam("Account", _exportOnlyNew, new AccountHandler(), "TsExternalId",
						select => (select.HasCondition ? select.And("EntitySrc", "PrimaryContactId") : select.Where("EntitySrc", "PrimaryContactId")).Not().IsNull(), _rateCount))
					.Run(new PrimaryExportParam("Case", _exportOnlyNew, null, "TsExternalId", null, _rateCount))
					.Run(new PrimaryExportParam("Account", _exportOnlyNew, new CounteragentHandler(), "TsOrderServiceId",
						select =>
						{
							select
								.LeftOuterJoin("SysAdminUnit").As("cPrimeUser")
								.On("cPrimeUser", "ContactId").IsEqual("EntitySrc", "PrimaryContactId")
								.LeftOuterJoin("Contact").As("c")
								.On("c", "AccountId").IsEqual("EntitySrc", "Id")
								.LeftOuterJoin("SysAdminUnit").As("cUser")
								.On("cUser", "ContactId").IsEqual("c", "Id");
							select.Where();
							(select.HasCondition ? select.And() : select.Where())
								.OpenBlock("cPrimeUser", "Id").Not().IsNull()
								.Or("cUser", "Id").Not().IsNull()
								.CloseBlock();
						}, _rateCount))
					.Run(new PrimaryExportParam("Contact", _exportOnlyNew, new ManagerInfoHandler(), "TsManagerInfoId",
						select =>
						{
							select
								.LeftOuterJoin("SysAdminUnit").As("cUser")
								.On("cUser", "ContactId").IsEqual("EntitySrc", "Id");
							select.Where();
							(select.HasCondition ? select.And("cUser", "Id") : select.Where("cUser", "Id")).Not().IsNull();
						}, _rateCount));
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e);
			}
		}
	}

	#endregion


	#region Class: PrimaryImportParam
	/*
		Project Path: ..\..\..\QueryConsole\Files\PrimaryImport\PrimaryImportParam.cs
		
	*/
	public class PrimaryImportParam
	{
		public string ServiceObjName;
		public int ExternalId;
		public bool WithUpdateExist;
		public int BatchLimit;
		public UserConnection UserConnection;
		public bool CreateReminding;
		public int SkipCount;
		public string Filter;
		public PrimaryImportParam(string serviceObjName, UserConnection userConnection, bool withUpdateExist = false, int externalId = 0,
			int batchLimit = 10, bool createReminding = false, int skipCount = 0, string filter = null)
		{
			ServiceObjName = serviceObjName;
			ExternalId = externalId;
			WithUpdateExist = withUpdateExist;
			BatchLimit = batchLimit;
			UserConnection = userConnection;
			CreateReminding = createReminding;
			SkipCount = skipCount;
			Filter = filter;
		}
	}

	#endregion


	#region Class: PrimaryImportProvider
	/*
		Project Path: ..\..\..\QueryConsole\Files\PrimaryImport\PrimaryImportProvider.cs
		
	*/
	public class PrimaryImportProvider
	{
		public PrimaryImportParam Settings;
		public BaseServiceIntegrator Integrator;
		public Guid LogId;
		public int TotalCount = 0;
		public PrimaryImportProvider(PrimaryImportParam settings)
		{
			Settings = settings;
			Integrator = IntegratorBuilder.Build(settings.ServiceObjName, settings.UserConnection);
		}
		public void Run()
		{
			try
			{
				//Todo Вынести в настройки
				int skip = Settings.SkipCount;
				var serviceRequestInfo = ServiceRequestInfo.CreateForExportInBpm(Settings.ServiceObjName);
				serviceRequestInfo.Limit = Settings.BatchLimit.ToString();
				serviceRequestInfo.AfterIntegrate = OnIntegrateFinish;
				serviceRequestInfo.ProgressAction = OnProgress;
				serviceRequestInfo.ServiceObjectId = Settings.ExternalId.ToString();
				serviceRequestInfo.UpdateIfExist = Settings.WithUpdateExist;
				serviceRequestInfo.ReupdateUrl = true;
				serviceRequestInfo.SortField = "createdAt";
				if(!string.IsNullOrEmpty(Settings.Filter))
				{
					serviceRequestInfo.Filters = Settings.Filter;
				}
				var logInfo = LoggerInfo.GetBpmRequestLogInfo(Settings.UserConnection, Integrator.ServiceName, "",
					Settings.ServiceObjName);
				LoggerHelper.DoInTransaction(logInfo, () =>
				{
					LogId = IntegrationLogger.CurrentTransLogId;
					do
					{
						serviceRequestInfo.Skip = skip.ToString();
						Integrator.GetRequest(serviceRequestInfo);
						skip += Settings.BatchLimit;
					} while (serviceRequestInfo.IntegrateCount < serviceRequestInfo.TotalCount && skip - Settings.BatchLimit <= serviceRequestInfo.TotalCount);
				});
			} catch(Exception e)
			{
				Terrasoft.Configuration.TsEntityLogger.MethodInfoError("PrimaryImportProvider [Run]", e.ToString());
			}
		}
		public void OnIntegrateFinish()
		{

		}
		public void OnProgress(int processedCount, int allCount)
		{
			try
			{
				if (CsConstant.PrimaryImportProviderConst.WithWatchProgress && LogId != Guid.Empty)
				{
					var update = new Update(Settings.UserConnection, "TsIntegrLog")
								.Set("TsIntegrateCount", Column.Parameter(processedCount))
								.Where("Id").IsEqual(Column.Parameter(LogId)) as Update;
					if (TotalCount != allCount)
					{
						TotalCount = allCount;
						update.Set("TsTotalCount", Column.Parameter(TotalCount));
					}
					update.Execute();
				}
			} catch(Exception e)
			{
				Terrasoft.Configuration.TsEntityLogger.MethodInfoError("OnProgress", e.ToString());
			}
		}
	}

	#endregion


	#region Class: PrimaryImportScenario
	/*
		Project Path: ..\..\..\QueryConsole\Files\PrimaryImport\PrimaryImportScenario.cs
		
	*/
	public class PrimaryImportScenario
	{
		public List<string> ServicesObject = new List<string>();
		public string StartObjName;
		public int Skip;
		public bool WithUpdateExist;
		public int BatchLimit;
		public UserConnection UserConnection;
		public Dictionary<string, string> Filters = new Dictionary<string, string>();
		public PrimaryImportScenario(UserConnection userConnection, string startObjName, int skip, bool withUpdateExist, int batchLimit, params string[] servicesObjectNames)
		{
			StartObjName = startObjName;
			Skip = skip;
			ServicesObject.AddRange(servicesObjectNames);
			UserConnection = userConnection;
			WithUpdateExist = withUpdateExist;
			BatchLimit = batchLimit;
		}
		public void Run()
		{
			if(!ServicesObject.Any())
			{
				return;
			}
			int skip = Skip;
			int startServiceObjIndex = 0;
			if(!string.IsNullOrEmpty(StartObjName))
			{
				startServiceObjIndex = ServicesObject.IndexOf(StartObjName);
			}
			for(int i = 0; i < ServicesObject.Count; i++)
			{
				if (i >= startServiceObjIndex)
				{
					var name = ServicesObject[i];
					string filter = string.Empty;
					if(Filters.ContainsKey(name))
					{
						filter = Filters[name];
					}
					ImportServiceObject(name, skip, filter);
					if (skip > 0)
					{
						skip = 0;
					}
				}
			}
		}

		internal PrimaryImportScenario AddFilters(string entityName, string filter)
		{
			if(!Filters.ContainsKey(entityName))
			{
				Filters.Add(entityName, filter);
				return this;
			}
			Filters[entityName] = filter;
			return this;
		}

		public void ImportServiceObject(string serviceObjName, int skip, string filter = null)
		{
			var options = new PrimaryImportParam(serviceObjName, UserConnection, WithUpdateExist, 0, BatchLimit, false, skip, filter: filter);
			var importProvider = new PrimaryImportProvider(options);
			importProvider.Run();
		}
	}

	#endregion


	#region Class: ServiceUrlMaker
	/*
		Project Path: ..\..\..\QueryConsole\Files\UrlMaker\ServiceUrlMaker.cs
		
	*/
	public class ServiceUrlMaker
	{
		public Dictionary<TServiceObject, string> baseUrls;
		public ServiceUrlMaker(Dictionary<TServiceObject, string> baseUrls)
		{
			this.baseUrls = baseUrls;
		}
		public virtual string Make(TServiceObject type, string objectName, string objectId, string filters, TRequstMethod method, string limit, string skip, string sort = null, string sortDirect = "asc")
		{
			return MakeUrl(baseUrls[type], objectName, objectId, filters, method, limit, skip, sort, sortDirect);
		}
		public virtual string Make(ServiceRequestInfo info)
		{
			return Make(info.Type, info.ServiceObjectName, info.ServiceObjectId, info.Filters, info.Method, info.Limit, info.Skip, info.SortField, info.SortDirection);
		}

		public static string MakeUrl(string baseUrl, string objectName, string objectId, string filters, TRequstMethod method, string limit, string skip, string sort = null, string sortDirect = "asc") {
			string resultUrl = baseUrl;
			resultUrl += "/" + objectName;
			if (!string.IsNullOrEmpty(objectId) && objectId != "0") {
				resultUrl += "/" + objectId;
				return resultUrl;
			}
			//resultUrl += "?sort[createdAt]=desc";
			if (!string.IsNullOrEmpty(filters)) {
				resultUrl += "?" + filters;
			}
			if (!string.IsNullOrEmpty(limit)) {
				resultUrl += (resultUrl.IndexOf("?") > -1 ? "&" : "?") + "limit=" + limit;
			}
			if (!string.IsNullOrEmpty(skip) && int.Parse(skip) > 0)
			{
				resultUrl += (resultUrl.IndexOf("?") > -1 ? "&" : "?") + "skip=" + skip;
			}
			if (!string.IsNullOrEmpty(sort))
			{
				resultUrl += (resultUrl.IndexOf("?") > -1 ? "&" : "?") + "sort[" + sort + "]=" + sortDirect;
			}
			return resultUrl;
		}

		public static string MakeUrl(string baseUrl, ServiceRequestInfo info) {
			return MakeUrl(baseUrl, info.ServiceObjectName, info.ServiceObjectId, info.Filters, info.Method, info.Limit, info.Skip);
		}
	}

	#endregion


	#region Class: TsAuto3nService
	/*
		Project Path: ..\..\..\QueryConsole\Files\WCFService\TsAuto3nService.cs
		
	*/
	[ServiceContract]
	[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
	public class TsAuto3nService {
		private static UserConnection _userConnection;
		private static object lockerObject = new object();
		private static UserConnection UserConnection {
			get {
				if (_userConnection == null) {
					lock (lockerObject)
					{
						_userConnection = (UserConnection)HttpContext.Current.Session["UserConnection"];
						if (_userConnection == null)
						{
							var appConnection = HttpContext.Current.Application["AppConnection"] as AppConnection;
							var systemUserConnection = appConnection.SystemUserConnection;
							var autoAuthorization = (bool)Terrasoft.Core.Configuration.SysSettings.GetValue(
								systemUserConnection, "ClientSiteIntegrationAutoAuthorization");
							if (autoAuthorization)
							{
								string userName = (string)Terrasoft.Core.Configuration.SysSettings.GetValue(
								systemUserConnection, "ClientSiteIntegrationUserName");
								if (!string.IsNullOrEmpty(userName))
								{
									string userPassword = (string)Terrasoft.Core.Configuration.SysSettings.GetValue(
									systemUserConnection, "ClientSiteIntegrationUserPassword");
									string workspace = appConnection.SystemUserConnection.Workspace.Name;
									_userConnection = new UserConnection(appConnection);
									_userConnection.Initialize();
									try
									{
										_userConnection.Login(userName, userPassword, workspace, TimeZoneInfo.Utc);
									}
									catch (Exception)
									{
										_userConnection = null;
									}
								}
							}
						}
					}
				}
				if (_userConnection == null) {
					throw new ArgumentException("Invalid login or password");
				}
				return _userConnection;
			}
		}

		[OperationContract]
		[WebInvoke(Method = "POST", UriTemplate = "NotifyChange", BodyStyle = WebMessageBodyStyle.Wrapped,
			RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
		public bool NotifyChange() {
			var userConnection = UserConnection;
			ThreadPool.QueueUserWorkItem((x) =>
			{
				var integrator = new IntegrationServiceIntegrator(userConnection);
				integrator.IniciateLoadChanges();
			});
			return true;
		}
	}

	#endregion


	#region Class: DynamicXmlParser
	/*
		Project Path: ..\..\..\QueryConsole\Files\XmlManager\DynamicXmlParser.cs
		
	*/
	public static class DynamicXmlParser {
		public static T StartMapXmlToObj<T>(XmlNode node, Type objType, object defObj = null, Func<string, string> prepareValuePredicate = null) {
			object resultObj = null;
			if (defObj == null) {
				resultObj = Activator.CreateInstance(objType);
			} else {
				resultObj = defObj.CloneObject();
			}
			var columnsName = objType.GetProperties().Where(x => x.MemberType == MemberTypes.Property).Select(x => x.Name).ToList();
			foreach (var columnName in columnsName) {
				PropertyInfo propertyInfo = objType.GetProperty(columnName);
				var xmlAttr = node.Attributes[columnName];
				if (xmlAttr != null) {
					var value = xmlAttr.Value;
					if (prepareValuePredicate != null) {
						value = prepareValuePredicate(value);
					}
					var propertyType = propertyInfo.PropertyType;
					if (propertyType.IsEnum || propertyType == typeof(int)) {
						propertyInfo.SetValue(resultObj, int.Parse(value));
					} else if (propertyType == typeof(bool)) {
						propertyInfo.SetValue(resultObj, value != "0");
					} else {
						propertyInfo.SetValue(resultObj, value);
					}
				}
			}
			if(resultObj is T) {
				return (T)resultObj;
			}
			return default(T);
		}
	}

	#endregion


	#region Class: IntegrationPath
	/*
		Project Path: ..\..\..\QueryConsole\Files\XmlManager\IntegrationPath.cs
		
	*/
	public class IntegrationPath {
		public string Name {get;set;}
		public string Path {get;set;}
		public string ServiceName {get;set;}
	}

	#endregion


	#region Class: IntegrationPathConfig
	/*
		Project Path: ..\..\..\QueryConsole\Files\XmlManager\IntegrationPathConfig.cs
		
	*/
	public class IntegrationPathConfig {
		public List<IntegrationPath> Paths;
	}

	#endregion


	#region Enum: TRequstMethod
	/*
		Project Path: ..\..\..\QueryConsole\Files\IntegratorHelper\IntegratorHelper.cs
		
	*/
	public enum TRequstMethod
	{
		GET,
		POST,
		PUT,
		DELETE
	}

	#endregion


	#region Enum: TServiceObject
	/*
		Project Path: ..\..\..\QueryConsole\Files\Integrators\TsServiceIntegrator.cs
		
	*/
	 

	public enum TServiceObject {
		Entity,
		Dict,
		Service
	}

	#endregion


	#region Enum: TMapType
	/*
		Project Path: ..\..\..\QueryConsole\Files\MappingManager\MappingHelper.cs
		
	*/

		public enum TMapType
	{
		RefToGuid = 0,
		Simple = 1,
		FirstDestinationField = 2,
		CompositObject = 3,
		ArrayOfCompositObject = 4,
		Const = 5,
		ArrayOfReference = 6,
		ManyToMany = 8,
		ToDetail = 9
	}

	#endregion


	#region Enum: TMapExecuteType
	/*
		Project Path: ..\..\..\QueryConsole\Files\MappingManager\MappingHelper.cs
		
	*/
	 

		public enum TMapExecuteType
	{
		AfterEntitySave = 0,
		BeforeEntitySave = 1
	}

	#endregion


	#region Enum: TConstType
	/*
		Project Path: ..\..\..\QueryConsole\Files\MappingManager\MappingHelper.cs
		
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


	#region Enum: MacrosType
	/*
		Project Path: ..\..\..\QueryConsole\Files\MappingManager\TsMacrosHelper.cs
		
	*/
	public enum MacrosType
	{
		Rule,
		OverRule
	}

	#endregion


	#region Interface: IMappingMethod
	/*
		Project Path: ..\..\..\QueryConsole\Files\TsClientServiceInegration.TsBase.cs
		
	*/

	public interface IMappingMethod {
		//TODO: Вынести методы маппера в отдельные сущности
		void Evaluate(MappingItem mappItem, CsConstant.IntegrationInfo integrationInfo);
	}

	#endregion


	#region Interface: IServiceIntegrator
	/*
		Project Path: ..\..\..\QueryConsole\Files\Integrators\TsServiceIntegrator.cs
		
	*/
		public interface IServiceIntegrator {
		void GetRequest(ServiceRequestInfo info);
		void IntegrateBpmEntity(Entity entity, EntityHandler handler = null, bool withLock = true);
	}

	#endregion


	#region Interface: DeliveryServiceLookupProvider
	/*
		Project Path: ..\..\..\QueryConsole\Files\Integrators\DeliveryServices\DeliveryServiceLookupProvider.cs
		
	*/
	public interface DeliveryServiceLookupProvider {
		List<Dictionary<string, string>> GetLookupValues(string query = null);
	}

	#endregion


	#region Interface: IMappRule
	/*
		Project Path: ..\..\..\QueryConsole\Files\MappingManager\IMapRule.cs
		
	*/
	public interface IMappRule
	{
		string Type { get; set; }
		Entity Entity { get; set; }
		JObject Json { get; set; }
		void Import(RuleImportInfo info);
		void Export(RuleExportInfo info);
	}

	#endregion

}