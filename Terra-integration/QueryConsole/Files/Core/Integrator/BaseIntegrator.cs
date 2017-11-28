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
}