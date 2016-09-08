using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terrasoft.Core;
using Terrasoft.Core.Entities;

namespace Terrasoft.TsConfiguration
{
		public interface IServiceIntegrator {
		void GetRequest(ServiceRequestInfo info);
		void IntegrateBpmEntity(Entity entity, EntityHandler handler = null);
	}
	 

	public enum TServiceObject {
		Entity,
		Dict
	}

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
		
		public static ServiceRequestInfo CreateForExportInBpm(string serviceObjectName, TServiceObject type = TServiceObject.Entity) {
			return new ServiceRequestInfo() {
				ServiceObjectName = serviceObjectName,
				Type = type
			};
		}
	}

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
			info.FullUrl = info.FullUrl ?? UrlMaker.Make(info);
			//IntegrationConsole.SetCurrentRequestUrl(info.FullUrl);
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
				}, userConnection, info.LogId,
				(x, y) =>
				{
					IntegrationLocker.Unlock(info.Entity.SchemaName, info.Entity.PrimaryColumnValue);
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
						if (string.IsNullOrEmpty(info.ServiceObjectId))
						{
							var objArray = responseJObj["data"] as JArray;
							resultObjects = objArray.Select(x => x as JObject);
						}
						else
						{
							resultObjects = new List<JObject>() {
								responseJObj
							};
						}
						foreach (var jObj in resultObjects)
						{
							IntegrateServiceEntity(jObj, info.ServiceObjectName);
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

		public virtual void IntegrateBpmEntity(Entity entity, EntityHandler defHandler = null) {
			IntegrateBpmEntity(entity.PrimaryColumnValue, entity.SchemaName, defHandler);
		}

		public virtual void IntegrateBpmEntity(Guid entityId, string schemaName, EntityHandler defHandler = null) {
			if (!IsIntegratorActive) {
				return;
			}
			try {
				if (IntegrationLocker.CheckUnLock(schemaName, entityId)) {
					var primaryColumnValue = entityId;
					EntitySchema entitySchema = userConnection.EntitySchemaManager.GetInstanceByName(schemaName);
					var entity = entitySchema.CreateEntity(userConnection);
					if (entity.FetchFromDB(primaryColumnValue, false)) {
						IntegrationLocker.Lock(entity.SchemaName, entity.PrimaryColumnValue);
						CsConstant.IntegrationInfo integrationInfo = null;
						var handlers = defHandler == null ? entityHelper.GetAllIntegrationHandler(entity.SchemaName, CsConstant.TIntegrationType.Export) : new List<EntityHandler>() { defHandler };
						foreach (var handler in handlers) {
							IntegrationLogger.StartTransaction(userConnection, CsConstant.PersonName.Bpm, handler.ServiceName, entity.GetType().Name, handler.JName, string.Format("{0} - {1}", entity.PrimaryColumnValue, entity.PrimaryDisplayColumnValue));
							try {
								integrationInfo = CsConstant.IntegrationInfo.CreateForExport(userConnection, entity);
								integrationInfo.Handler = handler;
								entityHelper.IntegrateEntity(integrationInfo);
								if (integrationInfo.Result != null && integrationInfo.Result.Type == CsConstant.IntegrationResult.TResultType.Success) {
									var json = integrationInfo.Result.Data.ToString();
									var requestInfo = integrationInfo.Handler.GetRequestInfo(integrationInfo);
									requestInfo.LogId = IntegrationLogger.CurrentLogId;
									MakeRequest(requestInfo);
								}
							} catch (Exception e) {
								IntegrationLogger.Error(e);
							}
						}
					}
				}
			} catch (Exception e) {
				IntegrationLogger.StartTransaction(userConnection, CsConstant.PersonName.Bpm, "None", schemaName, "None");
				IntegrationLogger.Error(e);
			}
		}
		public virtual void IntegrateServiceEntity(JObject serviceEntity, string serviceObjectName) {
			if (!IsIntegratorActive) {
				return;
			}
			var integrationInfo = CsConstant.IntegrationInfo.CreateForImport(userConnection, CsConstant.IntegrationActionName.Create, serviceObjectName, serviceEntity);
			entityHelper.IntegrateEntity(integrationInfo);
			if (integrationInfo.Result != null && integrationInfo.Result.Type == CsConstant.IntegrationResult.TResultType.Exception)
			{
				if (integrationInfo.Result.Exception == CsConstant.IntegrationResult.TResultException.OnCreateEntityExist)
				{
					integrationInfo = CsConstant.IntegrationInfo.CreateForImport(userConnection, CsConstant.IntegrationActionName.Update, serviceObjectName, serviceEntity);
					entityHelper.IntegrateEntity(integrationInfo);
				}
			}
		}
	}
}
