using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Terrasoft.Core;
using Terrasoft.Core.DB;

namespace Terrasoft.TsIntegration.Configuration
{
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
				var insert = new Insert(ConnectionProvider.Get<UserConnection>())
					.Into("TsiTriggerQueue")
					.Set("TsiObjectName", Column.Parameter(integrationInfo.IntegratedEntity.SchemaName))
					.Set("TsiObjectId", Column.Parameter(id))
					.Set("TsiTriggerName", Column.Parameter(triggerName));
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
}