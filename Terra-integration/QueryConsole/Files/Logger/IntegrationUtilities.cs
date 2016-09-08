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
	public static class IntegrationLogger
	{
		private static UniqueLogger<TsLogger> _log = new UniqueLogger<TsLogger>(() => new TsLogger());
		public static Dictionary<int, Guid> ThreadLogIds = new Dictionary<int, Guid>();


		public static void SetThreadLogId(int threadId, Guid logId)
		{
			if (ThreadLogIds.ContainsKey(threadId))
			{
				ThreadLogIds[threadId] = logId;
				return;
			}
			ThreadLogIds.Add(threadId, logId);
		}
		public static Guid CurrentLogId
		{
			get
			{
				int threadId = Thread.CurrentThread.ManagedThreadId;
				if (ThreadLogIds.ContainsKey(threadId))
				{
					return ThreadLogIds[threadId];
				}
				var newLogId = Guid.NewGuid();
				SetThreadLogId(threadId, newLogId);
				return newLogId;
			}
		}
		public static TsLogger CurrentLogger {
			get {
				return _log.GetValue(CurrentLogId);
			}
		}
		public static void SetCurentThreadLogId(Guid Id, UserConnection userConnection)
		{
			SetThreadLogId(Thread.CurrentThread.ManagedThreadId, Id);
		}

		public static void BeforeRequestError(Exception e) {
			var logger = _log.GetValue(CurrentLogId).Instance;
			logger.Error(string.Format("Error - text = {0} callStack = {1}", e.Message, e.StackTrace));
		}

		public static void AfterRequestError(Exception e)
		{
			var logger = _log.GetValue(CurrentLogId).Instance;
			logger.Error(string.Format("Error - text = {0} callStack = {1}", e.Message, e.StackTrace));
		}

		public static void StartTransaction(UserConnection userConnection, string requesterName, string reciverName, string bpmObjName, string serviceObjName, string additionalInfo = "")
		{
			var id = Guid.NewGuid();
			SetCurentThreadLogId(id, userConnection);
			var logger = _log.GetValue(id);
			logger.Instance.Info("StartTransaction" + id.ToString());
			logger.CreateTransaction(id, requesterName, reciverName, bpmObjName, serviceObjName, additionalInfo);
		}

		public static Guid PushRequest(TRequstMethod requestMethod, string url, string jsonText, string additionalInfo = "")
		{
			var id = CurrentLogId;
			var logger = _log.GetValue(id);
			var requestType = CsConstant.TsRequestType.Push;
			logger.Instance.Info(string.Format("PushRequest - id = {0} method={1} requestType={4}\nurl={2}\njson={3}", id, requestMethod, url, jsonText, requestType));
			return logger.CreateRequest(id, requestMethod.ToString(), url, requestType, additionalInfo);
		}


		public static void GetResponse(string text)
		{
			var id = CurrentLogId;
			var logger = _log.GetValue(id);
			var requestType = CsConstant.TsRequestType.GetResponse;
			logger.Instance.Info(string.Format("GetResponse - id = {0} requestType={2}\ntext={1}", id, text, requestType));
			logger.CreateResponse(id, text, requestType);
		}

		public static void ResponseError(Exception e, string text, string requestJson, Guid requestId)
		{
			var id = CurrentLogId;
			var logger = _log.GetValue(id);
			logger.UpdateResponseError(id, e.Message, e.StackTrace, text, requestJson, requestId);
		}

		public static Dictionary<string, int> IncDict = new Dictionary<string, int>();

		public static void MappingError(Exception e, MappingItem item, CsConstant.IntegrationInfo integrationInfo) {
			try {
				CurrentLogger.MappingError(CurrentLogId, e.ToString(), e.StackTrace, item.JSourcePath, item.TsSourcePath);
			} catch(Exception e2) {
				//TODO
			}
		}

		public static void Error(Exception e, string additionalInfo = null)
		{
			try {
				CurrentLogger.Error(CurrentLogId, e.ToString(), e.StackTrace, additionalInfo);
			} catch (Exception e2) {
				//TODO
			}
		}
	}

	public class TsLogger {
		private global::Common.Logging.ILog _log;
		public global::Common.Logging.ILog Instance {
			get {
				return _log;
			}
		}
		public UserConnection userConnection {
			get {
				return CsConstant.UserConnection;
			}
		}
		public TsLogger() {
			_log = global::Common.Logging.LogManager.GetLogger("TscIntegration") ?? global::Common.Logging.LogManager.GetLogger("Common");
		}

		public void CreateTransaction(Guid id, string requesterName, string resiverName, string entityName, string serviceEntityName, string additionalInfo="") {
			try {
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
								TsAdditionalInfo = '{6}'
					when not matched then
						insert (TsResiver, TsName, TsEntityName, TsServiceEntityName, TsAdditionalInfo, Id)
						values ('{2}', '{3}', '{4}', '{5}', '{6}', '{0}');
				", id, DateTime.UtcNow, resiverName, requesterName, entityName, serviceEntityName, additionalInfo);
				var query = new CustomQuery(userConnection, textQuery);
				query.Execute();
			} catch(Exception e) {
				Instance.Error(e.ToString());
			}
		}

		public Guid CreateRequest(Guid logId, string method, string url, Guid requestType, string additionalInfo = "") {
			try {
				var resultId = Guid.NewGuid();
				var insert = new Insert(userConnection)
							.Into("TsIntegrationRequest")
							.Set("Id", Column.Parameter(resultId))
							.Set("TsIntegrLogId", Column.Parameter(logId))
							.Set("TsMethod", Column.Parameter(method))
							.Set("TsUrl", Column.Parameter(url))
							.Set("TsRequestTypeId", Column.Parameter(requestType))
							.Set("TsAdditionalInfo", Column.Parameter(additionalInfo))
							.Set("TsStatusId", Column.Parameter(CsConstant.TsRequestStatus.Success)) as Insert;
				insert.Execute();
				return resultId;
			} catch (Exception e) {
				Instance.Error(e.ToString());
				return Guid.Empty;
			}
		}

		public void CreateResponse(Guid id, string text, Guid requestType)
		{
			try {
				var insert = new Insert(userConnection)
								.Into("TsIntegrationRequest")
								.Set("TsIntegrLogId", Column.Parameter(id))
								.Set("TsRequestTypeId", Column.Parameter(requestType)) as Insert;
				insert.Execute();
			} catch (Exception e) {
				Instance.Error(e.ToString());
			}
		}

		public void UpdateResponseError(Guid id, string errorText, string callStack, string json, string requestJson, Guid requestId) {
			try {
				var errorId = Guid.NewGuid();
				errorText = string.Format("[{0}] {1}", Thread.CurrentThread.ManagedThreadId, errorText ?? "");
				var insert = new Insert(userConnection)
							.Into("TsIntegrationError")
							.Set("Id", Column.Parameter(errorId))
							.Set("TsErrorText", Column.Parameter(errorText))
							.Set("TsCallStack", Column.Parameter(callStack))
							.Set("TsRequestJson", Column.Parameter(requestJson != null ? requestJson : string.Empty))
							.Set("TsResponseJson", Column.Parameter(json)) as Insert;
				insert.Execute();
				var update = new Update(userConnection, "TsIntegrationRequest")
							.Set("TsErrorId", Column.Parameter(errorId))
							.Set("TsStatusId", Column.Parameter(CsConstant.TsRequestStatus.Error))
							.Where("Id").IsEqual(Column.Parameter(requestId)) as Update;
				update.Execute();
			} catch (Exception e) {
				Instance.Error(e.ToString());
			}
		}

		public void MappingError(Guid logId, string errorMessage, string callStack, string serviceFieldName, string bpmFieldName) {
			try {
				errorMessage = string.Format("[{0}] {1}", Thread.CurrentThread.ManagedThreadId, errorMessage ?? "");
				var insert = new Insert(userConnection)
							.Into("TsIntegrMappingError")
							.Set("TsErrorMessage", Column.Parameter(errorMessage ?? ""))
							.Set("TsCallStack", Column.Parameter(callStack ?? ""))
							.Set("TsServiceFieldName", Column.Parameter(serviceFieldName ?? ""))
							.Set("TsBpmFieldName", Column.Parameter(bpmFieldName ?? ""))
							.Set("TsIntegrLogId", Column.Parameter(logId)) as Insert;
				insert.Execute();
			} catch (Exception e) {
				Instance.Error(e.ToString());
			}
		}

		public void Error(Guid logId, string errorMessage, string callStack, string additionalInfo) {
			try {
				additionalInfo = string.Format("[{0}] {1}", Thread.CurrentThread.ManagedThreadId, additionalInfo ?? "");
				var insert = new Insert(userConnection)
							.Into("TsIntegrError")
							.Set("TsErrorText", Column.Parameter(errorMessage ?? ""))
							.Set("TsCallStack", Column.Parameter(callStack ?? ""))
							.Set("TsAdditionalInfo", Column.Parameter(additionalInfo))
							.Set("TsIntegrLogId", Column.Parameter(logId)) as Insert;
				insert.Execute();
			} catch (Exception e) {
				Instance.Error(e.ToString());
			}
		}
	}

	public class LogTransactionInfo {
		public string RequesterName;
		public string ResiverName;
		public string LastUrl;
		public string LastMethod;
		public UserConnection UserConnection;
	}

	public class UniqueLogger<T>
	{
		private Func<T> _valueFactory;
		private Dictionary<Guid, T> Values;

		public UniqueLogger(Func<T> valueFactory) {
			_valueFactory = valueFactory;
			Values = new Dictionary<Guid,T>();
		}

		public T GetValue(Guid id) {
			if(!Values.ContainsKey(id) || Values[id] == null) {
				Values.Add(id, _valueFactory());
			}
			return Values[id];
		}
	}
}
