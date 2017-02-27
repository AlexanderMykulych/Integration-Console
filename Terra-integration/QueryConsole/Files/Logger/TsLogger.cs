using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Terrasoft.Core;
using Terrasoft.Core.DB;

namespace Terrasoft.TsConfiguration
{
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
}
