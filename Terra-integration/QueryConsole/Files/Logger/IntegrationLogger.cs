using System;
using System.Collections.Concurrent;
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
		/// <summary>
		/// Простой предикат для логгирования ошибки
		/// </summary>
		public static Action<Exception> SimpleLoggerErrorAction = e => IntegrationLogger.Error(e);
		/// <summary>
		/// Активный файловой-логгер
		/// </summary>
		private static TsLogger _log = new TsLogger();
		/// <summary>
		/// Справочник всех состояний логгеров
		/// </summary>
		public static ConcurrentDictionary<int, LoggerState> ThreadLogIds = new ConcurrentDictionary<int, LoggerState>();
		/// <summary>
		/// Признак - логировать stacktrace
		/// </summary>
		public static bool IsLoggedStackTrace {
			get {
				return CsConstant.LoggerSettings.IsLoggedStackTrace;
			}
		}
		/// <summary>
		/// Идентификатор текущего потока
		/// </summary>
		public static int CurrentThreadId {
			get { return Thread.CurrentThread.ManagedThreadId; }
		}
		/// <summary>
		/// Идентификатор текущей транзакции
		/// </summary>
		public static Guid CurrentTransLogId {
			get {
				if (ThreadLogIds.ContainsKey(CurrentThreadId))
				{
					return ThreadLogIds[CurrentThreadId].TransactionId;
				}
				return Guid.Empty;
			}
		}
		/// <summary>
		/// Текущее состояние логгера
		/// </summary>
		public static LoggerState CurrentLogState {
			get {
				if (ThreadLogIds.ContainsKey(CurrentThreadId))
				{
					return ThreadLogIds[CurrentThreadId];
				}
				return null;
			}
		}
		/// <summary>
		/// Логгер
		/// </summary>
		public static TsLogger CurrentLogger {
			get {
				return _log;
			}
		}
		/// <summary>
		/// Логирует ошибку в файл.
		/// </summary>
		/// <param name="e">Ошибка</param>
		public static void AfterRequestError(Exception e)
		{
			var logger = CurrentLogger.Instance;
			logger.Error(string.Format("Error - text = {0} callStack = {1}", e.Message, e.StackTrace));
		}
		/// <summary>
		/// Начинает транзакцию логирования
		/// </summary>
		/// <param name="userConnection">Подключение пользователя</param>
		/// <param name="requesterName">Имя того кто делает запрос</param>
		/// <param name="reciverName">Имя получателя</param>
		/// <param name="bpmObjName">Имя объекта в Bpm</param>
		/// <param name="serviceObjName">Имя объекта в сервисе</param>
		/// <param name="additionalInfo">Дополнительная информация</param>
		/// <param name="isPrimary">Признак первичного импорта</param>
		/// <param name="useIfExist">Если для текущего потока уже есть транзакция, то используем ее</param>
		public static bool StartTransaction(UserConnection userConnection, string requesterName, string reciverName, string bpmObjName, string serviceObjName, string additionalInfo = "", bool isPrimary = false, bool useIfExist = false)
		{
			try
			{
				bool isNew = false;
				Guid id = CreateNewTransactionId(useIfExist, notExistAction: transactionId =>
				{
					CurrentLogger.CreateTransaction(transactionId, requesterName, reciverName, bpmObjName, serviceObjName, additionalInfo, isPrimary);
					isNew = true;
				});
				CurrentLogger.Instance.Info("StartTransaction " + id);
				return isNew;
			}
			catch (Exception e2)
			{
				CurrentLogger.Instance.Error(e2.ToString());
			}
			return false;
		}
		/// <summary>
		/// Завершает транзакцию логгирования
		/// </summary>
		/// <param name="userConnection">Подключение пользователя</param>
		public static void FinishTransaction(UserConnection userConnection)
		{
			CurrentLogger.FinishTransaction(CurrentTransLogId);
			ClearCurrentTransaction();
		}
		/// <summary>
		/// Создает id новой транзакции
		/// </summary>
		/// <returns></returns>
		private static Guid CreateNewTransactionId(bool useIfExist, Action<Guid> notExistAction)
		{
			var resultId = Guid.NewGuid();
			if (ThreadLogIds.ContainsKey(CurrentThreadId))
			{
				if (useIfExist)
				{
					return ThreadLogIds[CurrentThreadId].TransactionId;
				}
				ThreadLogIds[CurrentThreadId] = new LoggerState()
				{
					TransactionId = resultId
				};
				notExistAction(resultId);
			}
			else
			{
				ThreadLogIds.TryAdd(CurrentThreadId, new LoggerState()
				{
					TransactionId = resultId
				});
				notExistAction(resultId);
			}
			return resultId;
		}
		private static void ClearCurrentTransaction()
		{
			if (ThreadLogIds.ContainsKey(CurrentThreadId))
			{
				LoggerState removedState;
				ThreadLogIds.TryRemove(CurrentThreadId, out removedState);
			}
		}
		/// <summary>
		/// Логирование ошибки.
		/// </summary>
		/// <param name="e">Ошибка</param>
		/// <param name="additionalInfo">Дополнительная информаци</param>
		public static void Error(Exception e, string additionalInfo = null)
		{
			try
			{
				CurrentLogger.Instance.InfoFormat("logid:{0} Exception:{1} stack:{2} AdditionalInfo:{3}", CurrentTransLogId, e.ToString(), e.StackTrace, additionalInfo);
				CurrentLogger.Error(CurrentTransLogId, e.ToString(), e.StackTrace, additionalInfo);
			}
			catch (Exception e2)
			{
				CurrentLogger.Instance.Info(e2.ToString());
			}
		}
		/// <summary>
		/// Логгирование в файл
		/// </summary>
		/// <param name="message">Сообщение</param>
		public static void Info(string message)
		{
			try
			{
				CurrentLogger.Instance.Info(message);
			}
			catch (Exception e)
			{
				CurrentLogger.Instance.Info(e.ToString());
			}
		}
		/// <summary>
		/// Логирование информации о запросе
		/// </summary>
		/// <param name="info">Информация о запросе</param>
		public static void Info(RequestLoggerInfo info)
		{
			try
			{
				var requestType = CsConstant.TsRequestType.Push;
				CurrentLogger.Instance.Info(string.Format("PushRequest - id = {0} method={1} requestType={4}\nurl={2}\njson={3}", CurrentTransLogId, info.RequestMethod, info.Url, info.JsonText, requestType));
				if (IsLoggedStackTrace)
				{
					info.AdditionalInfo += GetStackTrace();
				}
				var requestId = CurrentLogger.CreateRequest(CurrentTransLogId, info.RequestMethod.ToString(), info.Url, requestType, info.AdditionalInfo);
				if (requestId != null)
				{
					SetLastRequestId(requestId);
				}
			}
			catch (Exception e2)
			{
				CurrentLogger.Instance.Error(e2.ToString());
			}
		}
		/// <summary>
		/// Логирование информаци о ответе на запрос
		/// </summary>
		/// <param name="info">Информация о ответе на запрос</param>
		public static void Info(ResponseLoggerInfo info)
		{
			try
			{
				var requestType = CsConstant.TsRequestType.GetResponse;
				CurrentLogger.Instance.Info(string.Format("GetResponse - id = {0} requestType={2}\ntext={1}", CurrentTransLogId, info.ResponseText, requestType));
				if (IsLoggedStackTrace)
				{
					info.ResponseText += GetStackTrace();
				}
				var state = CurrentLogState;
				if (state != null)
				{
					CurrentLogger.CreateResponse(CurrentTransLogId, info.ResponseText, requestType, CurrentLogState.LastRequestId);
				}
			}
			catch (Exception e2)
			{
				CurrentLogger.Instance.Error(e2.ToString());
			}
		}
		/// <summary>
		/// Логирование о ошибке запроса
		/// </summary>
		/// <param name="info"> Информация об ошибке</param>
		public static void Info(RequestErrorLoggerInfo info)
		{
			try
			{
				if (IsLoggedStackTrace)
				{
					info.ResponseText += GetStackTrace();
				}
				var e = info.Exception;
				CurrentLogger.UpdateResponseError(CurrentTransLogId, e.Message, e.StackTrace, info.ResponseText, info.ResponseJson, CurrentLogState.LastRequestId);
				CurrentLogger.Instance.InfoFormat("error: {0}\ntext: {1}\njson: {2}\nid: {3}", e.ToString(), info.ResponseText, info.ResponseJson, CurrentLogState.LastRequestId);
			}
			catch (Exception e2)
			{
				CurrentLogger.Instance.Error(e2.ToString());
			}
		}
		/// <summary>
		/// Информация о ошибки при маппинге
		/// </summary>
		/// <param name="info">Информация об ошибке</param>
		public static void Info(MappingErrorLoggerInfo info)
		{
			try
			{
				var e = info.Exception;
				var item = info.Item;
				CurrentLogger.MappingError(CurrentTransLogId, e.ToString(), e.StackTrace, item.JSourcePath, item.TsSourcePath);
				CurrentLogger.Instance.InfoFormat("logid:{0} Exception:{1} stack:{2} serviceFieldPath:{3} bpmFieldPath:{4}", CurrentTransLogId, e.ToString(), e.StackTrace, item.JSourcePath, item.TsSourcePath);
			}
			catch (Exception e2)
			{
				CurrentLogger.Instance.Info(e2.ToString());
			}
		}
		/// <summary>
		/// Устанавливает идентификатор последнего запроса
		/// </summary>
		/// <param name="requestId">Идентификатор последнего запроса</param>
		private static void SetLastRequestId(Guid requestId)
		{
			var state = CurrentLogState;
			if (state != null)
			{
				state.LastRequestId = requestId;
			}
		}
		/// <summary>
		/// Возвращает стек
		/// </summary>
		/// <returns></returns>
		private static string GetStackTrace()
		{
			System.Diagnostics.StackTrace t = new System.Diagnostics.StackTrace();
			return "Stack: " + t.ToString();
		}
	}
}
