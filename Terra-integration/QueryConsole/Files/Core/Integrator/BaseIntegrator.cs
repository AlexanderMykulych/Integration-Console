using System.Linq;
using System;
using Terrasoft.Core.Entities;

namespace Terrasoft.TsIntegration.Configuration {
	public class BaseIntegrator : IIntegrator
	{
		public BaseIntegrator(IEntityPreparer entityPreparer, IIntegrationObjectWorker iObjectWorker,
			IServiceHandlerWorkers serviceHandlerWorker, IServiceRequestWorker serviceRequestWorker)
		{
			EntityPreparer = entityPreparer;
			IObjectWorker = iObjectWorker;
			ServiceHandlerWorker = serviceHandlerWorker;
			ServiceRequestWorker = serviceRequestWorker;
		}

		public virtual IEntityPreparer EntityPreparer { set; get; }

		public virtual IIntegrationObjectWorker IObjectWorker { set; get; }

		public virtual IServiceHandlerWorkers ServiceHandlerWorker { set; get; }

		public virtual IServiceRequestWorker ServiceRequestWorker { set; get; }
		//Log key=Integrator
		public virtual void ExportWithRequest(Guid id, string schemaName, string routeKey = null)
		{
			Export(id, schemaName, routeKey, (iObject, handlerConfig, handler, entity) =>
			{
				ServiceRequestWorker.MakeRequest(ServiceHandlerWorker, entity, handler, handlerConfig.Service, iObject.ToString());
			});
		}
		//Log key=Integrator
		public virtual void Export(Guid id, string schemaName, string routeKey = null, Action<IIntegrationObject, ConfigSetting, BaseEntityHandler, Entity> OnGet = null)
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
					Entity entity = EntityPreparer.Get(schemaName, id);
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
							var iObject = IObjectWorker.Get(handler, entity);
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
		public virtual void Import(IIntegrationObject iObject, string routeKey = null, Action<CsConstant.IntegrationInfo> onSuccess = null, Action<CsConstant.IntegrationInfo, Exception> onError = null)
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
					IObjectWorker.Import(ServiceHandlerWorker, handlerConfig, iObject, onSuccess, onError);
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