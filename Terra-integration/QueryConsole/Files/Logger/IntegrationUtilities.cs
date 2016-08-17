using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Terrasoft.Core;
using Terrasoft.Core.DB;
using static Terrasoft.TsConfiguration.CsConstant;

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
		public static void SetCurentThreadLogId(Guid Id)
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

		public static void StartTransaction(LogTransactionInfo info)
		{
			var id = CurrentLogId;
			var logger = _log.GetValue(id);
			logger.info = info;
			logger.Instance.Info("StartTransaction" + id.ToString());
			logger.CreateTransaction(id);
		}

		public static void PushRequest(TRequstMethod requestMethod, string url, string jsonText, Guid requestId)
		{
			var id = CurrentLogId;
			var logger = _log.GetValue(id);
			var requestType = CsConstant.TsRequestType.Push;
			logger.Instance.Info(string.Format("PushRequest - id = {0} method={1} requestType={4}\nurl={2}\njson={3}", id, requestMethod, url, jsonText, requestType));
			logger.CreateRequest(id, requestMethod.ToString(), url, requestType, requestId);
		}


		public static void GetResponse(string text)
		{
			var id = CurrentLogId;
			var logger = _log.GetValue(id);
			var requestType = CsConstant.TsRequestType.GetResponse;
			logger.Instance.Info(string.Format("GetResponse - id = {0} requestType={2}\ntext={1}", id, text, requestType));
			logger.CreateResponse(id, text, requestType);
		}

		public static void ResponseError(Exception e, string text, Guid? requestId, string requestJson)
		{
			var id = CurrentLogId;
			if (!requestId.HasValue)
				return;
			var logger = _log.GetValue(id);
			//logger.Instance.Info(string.Format("ResponseError - id = {0} requestId={1}", id.Value, requestId));
			logger.UpdateResponseError(id, requestId.Value, e.Message, e.StackTrace, text, requestJson);
			//Console.WriteLine(string.Format("ResponseError - id = {0} requestId={1} message={2}", id.Value, requestId, text));
		}

		
		

		

		public static void AfterSaveError(Exception e, string typeName)
		{
			//IntegrationConsole.EntityIntegratedError(typeName);
			//var buff = Console.ForegroundColor;
			//Console.ForegroundColor = ConsoleColor.Red;
			//Console.WriteLine("Error: " + typeName);
			//Console.ForegroundColor = buff;
		}

        public static Dictionary<string, int> IncDict = new Dictionary<string, int>();
		public static void SuccessSave(string typeName) {
			Console.Write("Save OK");
			//IntegrationConsole.EntityIntegratedSuccess(typeName);
			if (IncDict.ContainsKey(typeName))
            {
                //Console.WriteLine((++IncDict[typeName]).ToString() + ".Save: " + typeName);
            } else
            {
                IncDict.Add(typeName, 0);
                //Console.WriteLine((++IncDict[typeName]).ToString() + ".Save: " + typeName);
            }
		}

		public static void MappingError(Exception e, MappingItem item, IntegrationInfo integrationInfo) {
			////IntegrationConsole.AddMappingError();
		}

		public static void Error(Exception e, string additionalInfo = null)
		{

		}
	}

	public class TsLogger {
		private global::Common.Logging.ILog _log;
		public LogTransactionInfo info;
		public global::Common.Logging.ILog Instance {
			get {
				return _log;
			}
		}
		public TsLogger() {
			_log = global::Common.Logging.LogManager.GetLogger("TscIntegration") ?? global::Common.Logging.LogManager.GetLogger("Common");
		}

		public void CreateTransaction(Guid id) {
			//var insert = new Insert(info.UserConnection)
			//			.Into("TsIntegrLog")
			//			.Set("Id", Column.Parameter(id))
			//			.Set("TsDate", Column.Parameter(DateTime.UtcNow))
			//			.Set("TsResiver", Column.Parameter(info.ResiverName))
			//			.Set("TsName", Column.Parameter(info.RequesterName)) as Insert;
			//insert.Execute();
		}

		public void CreateRequest(Guid logId, string method, string url, Guid requestType, Guid requestId) {
			//info.LastUrl = url;
			//info.LastMethod = method;
			//var insert = new Insert(info.UserConnection)
			//			.Into("TsIntegrationRequest")
			//			.Set("Id", Column.Parameter(requestId))
			//			.Set("TsIntegrLogId", Column.Parameter(logId))
			//			.Set("TsRequester", Column.Parameter(info.RequesterName))
			//			.Set("TsResiver", Column.Parameter(info.ResiverName))
			//			.Set("TsMethod", Column.Parameter(method))
			//			.Set("TsUrl", Column.Parameter(url))
			//			.Set("TsRequestTypeId", Column.Parameter(requestType))
			//			.Set("TsStatusId", Column.Parameter(CsConstant.TsRequestStatus.Success)) as Insert;
			//insert.Execute();
		}

		public void CreateResponse(Guid id, string text, Guid requestType)
		{
			//var insert = new Insert(info.UserConnection)
			//			.Into("TsIntegrationRequest")
			//			.Set("TsIntegrLogId", Column.Parameter(id))
			//			.Set("TsRequester", Column.Parameter(info.ResiverName))
			//			.Set("TsResiver", Column.Parameter(info.RequesterName))
			//			.Set("TsMethod", Column.Parameter(info.LastMethod))
			//			.Set("TsUrl", Column.Parameter(info.LastUrl))
			//			.Set("TsRequestTypeId", Column.Parameter(requestType)) as Insert;
			//insert.Execute();
		}

		public void UpdateResponseError(Guid id, Guid requestId, string errorText, string callStack, string json, string requestJson) {
			//var errorId = Guid.NewGuid();
			//var insert = new Insert(info.UserConnection)
			//			.Into("TsIntegrationError")
			//			.Set("Id", Column.Parameter(errorId))
			//			.Set("TsErrorText", Column.Parameter(errorText))
			//			.Set("TsCallStack", Column.Parameter(callStack))
			//			.Set("TsRequestJson", Column.Parameter(requestJson != null ? requestJson : string.Empty))
			//			.Set("TsResponseJson", Column.Parameter(json)) as Insert;
			//insert.Execute();
			//var update = new Update(info.UserConnection, "TsIntegrationRequest")
			//			.Set("TsErrorId", Column.Parameter(errorId))
			//			.Set("TsStatusId", Column.Parameter(CsConstant.TsRequestStatus.Error))
			//			.Where("Id").IsEqual(Column.Parameter(requestId)) as Update;
			//update.Execute();
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
