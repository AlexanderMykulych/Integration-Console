﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terrasoft.Core;
using Terrasoft.Core.Entities;
using static Terrasoft.TsConfiguration.CsConstant.IntegratorSettings;

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
		public DebugModeInfo DebugModeInfo
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
			var logId = Guid.NewGuid();
			IntegrationLogger.StartTransaction(logId, new LogTransactionInfo() {
				RequesterName = CsConstant.PersonName.Bpm,
				ResiverName = ServiceName,
				UserConnection = userConnection
			});
			info.LogId = logId;
			if (IsDebugMode)
			{
				info.ResponseData = DebugModeInfo.GetDebugDataJson();
				OnGetResponse(info);
			}
			else
			{
				integratorHelper.PushRequest(info.Method, info.FullUrl, info.RequestJson, (x, y, requestId) =>
				{
					info.ResponseData = x;
					OnGetResponse(info);
				}, userConnection, info.LogId,
				(x, y, requestId) =>
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
						if (string.IsNullOrEmpty(info.ServiceObjectId))
						{
							//IntegrationConsole.SetCurrentResponseSucces(responseJObj["total"].Value<int>(), responseJObj["skip"].Value<int>(), responseJObj["limit"].Value<int>());
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
					entityHelper.IntegrateEntity(integrationInfo);
					Console.WriteLine("Ok");
				break;
			}
			
		}

		public virtual void IntegrateBpmEntity(Entity entity, EntityHandler defHandler = null) {
			if(!IsIntegratorActive) {
				return;
			}
			var integrationInfo = CsConstant.IntegrationInfo.CreateForExport(userConnection, entity);
			var handlers = defHandler == null ? entityHelper.GetAllIntegrationHandler(integrationInfo) : new List<EntityHandler>() { defHandler };
			foreach(var handler in handlers) {
				integrationInfo = CsConstant.IntegrationInfo.CreateForExport(userConnection, entity);
				integrationInfo.Handler = handler;
				entityHelper.IntegrateEntity(integrationInfo);
				if (integrationInfo.Result != null && integrationInfo.Result.Type == CsConstant.IntegrationResult.TResultType.Success)
				{
					var json = integrationInfo.Result.Data.ToString();
					var requestInfo = integrationInfo.Handler.GetRequestInfo(integrationInfo);
					MakeRequest(requestInfo);
				}
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
