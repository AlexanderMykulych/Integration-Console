using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Terrasoft.Core.DB;

namespace Terrasoft.TsConfiguration
{
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
}
