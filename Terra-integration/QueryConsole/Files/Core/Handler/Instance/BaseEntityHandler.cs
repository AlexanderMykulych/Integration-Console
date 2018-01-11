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
namespace Terrasoft.TsIntegration.Configuration
{
	public abstract class BaseEntityHandler
	{
		public ConfigSetting HandlerConfig;
		public BaseEntityHandler(ConfigSetting handlerConfig)
		{
			HandlerConfig = handlerConfig;
			Mapper = ObjectFactory.Get<IMapper>();
			ConnectionProvider = ObjectFactory.Get<IConnectionProvider>();
			HandlerEntityWorker = ObjectFactory.Get<IHandlerEntityWorker>();
			ServiceHandlerWorker = ObjectFactory.Get<IServiceHandlerWorkers>();
			HandlerKeyGenerator = ObjectFactory.Get<IHandlerKeyGenerator>();
			IntegrationObjectProvider = ObjectFactory.Get<IIntegrationObjectProvider>();
			TemplateFactory = ObjectFactory.Get<ITemplateFactory>();
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

		public IConnectionProvider ConnectionProvider { get; set; }

		private UserConnection userConnection {
			get {
				return ConnectionProvider.Get<UserConnection>();
			}
		}

		public virtual IHandlerEntityWorker HandlerEntityWorker { get; set; }
		public virtual IServiceHandlerWorkers ServiceHandlerWorker { get; set; }
		public virtual IHandlerKeyGenerator HandlerKeyGenerator { get; set; }
		public virtual IIntegrationObjectProvider IntegrationObjectProvider { get; set; }
		public virtual ITemplateFactory TemplateFactory { get; set; }
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
				integrationInfo.IntegratedEntity = HandlerEntityWorker.CreateEntity(EntityName);
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

		public virtual EntitySchemaQuery GetEntitySchemaQuery(ref MappingConfig mappingConfig)
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

		public virtual Entity CreateEntityForExportMyMapping(ref MappingConfig mappingConfig)
		{
			var esqEntity = GetEntitySchemaQuery(ref mappingConfig);
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
			LoggerHelper.DoInLogBlock("Handler: To Json", () =>
			{
				if (integrationInfo.IntegrationType == CsConstant.TIntegrationType.Export)
				{
					integrationInfo.TsExternalIdPath = ExternalIdPath;
					BeforeMapping(integrationInfo);
					var mapConfig = ServiceHandlerWorker.GetMappingConfig(HandlerConfig.DefaultMappingConfig);
					if (integrationInfo.IntegratedEntity == null)
					{
						integrationInfo.IntegratedEntity = CreateEntityForExportMyMapping(ref mapConfig);
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
			var integrator = ObjectFactory.Get<IIntegrator>();
			integrator.Import(integrationInfo.Data, route);
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
				result = Mapper.CheckIsExist(EntityName, integrationInfo.Data.GetProperty<string>(path), integrationInfo.TsExternalIdPath, externalId);
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
			Guid resultId = AdvancedSearchInfo.Search(procedure => AddParameterToSearchProcedure(integrationInfo, procedure), IntegrationLogger.SimpleLoggerErrorAction);
			if (resultId == Guid.Empty)
			{
				return false;
			}
			integrationInfo.IntegratedEntity = HandlerEntityWorker.GetEntityById(EntityName, resultId);
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
				entity = HandlerEntityWorker.GetEntityByExternalId(EntityName, integrationInfo.TsExternalIdPath, integrationInfo.Data.GetProperty<string>(path));
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
}