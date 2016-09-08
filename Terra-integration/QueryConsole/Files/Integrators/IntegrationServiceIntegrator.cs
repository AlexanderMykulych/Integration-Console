using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terrasoft.Core;
using Terrasoft.TsConfiguration;
using IntegrationInfo = Terrasoft.TsConfiguration.CsConstant.IntegrationInfo;

namespace Terrasoft.TsConfiguration {
	public class IntegrationServiceIntegrator {
		private UserConnection _userConnection;
		private List<string> ReadedNotificationIds = new List<string>();
		private IntegratorHelper _integratorHelper = new IntegratorHelper();
		private IntegrationEntityHelper _integrationEntityHelper;
		private string _basePostboxUrl {
			get {
				return Settings.BaseUrl[TServiceObject.Entity];
			}
		}
		private string _baseClientServiceUrl;
		private int _postboxId {
			get {
				return Settings.PostboxId;
			}
		}
		private int _notifyLimit {
			get {
				return Settings.NotifyLimit;
			}
		}
		private bool _isIntegratorActive {
			get {
				return Settings.IsIntegratorActive;
			}
		}
		private string _auth {
			get {
				return Settings.Auth;
			}
		}
		private CsConstant.IntegratorSettings.IntegratorIntegrationServiceSetting _Settings;
		public CsConstant.IntegratorSettings.IntegratorIntegrationServiceSetting Settings {
			get {
				if (_Settings == null) {
					_Settings = (CsConstant.IntegratorSettings.IntegratorIntegrationServiceSetting)CsConstant.IntegratorSettings.Settings[this.GetType()];
				}
				return _Settings;
			}
		}

		public UserConnection UserConnection {
			get {
				return _userConnection;
			}
		}
		public IntegrationEntityHelper IntegrationEntityHelper {
			get {
				return _integrationEntityHelper;
			}
		}


		public IntegrationServiceIntegrator(UserConnection userConnection) {
			_userConnection = userConnection;
			_integrationEntityHelper = new IntegrationEntityHelper();
		}


		/// <summary>
		/// Получает BusEventNotification, после чего вызывает OnBusEventNotificationsDataRecived
		/// </summary>
		/// <param name="withData"></param>
		public void GetBusEventNotification(bool withData = true) {
			if(!_isIntegratorActive) {
				return;
			}
			var url = GenerateUrl(
				withData == true ? TIntegratorRequest.BusEventNotificationData : TIntegratorRequest.BusEventNotification,
				TRequstMethod.GET,
				"0",
				"1",//_notifyLimit.ToString(),
				CsConstant.DefaultBusEventFilters,
				CsConstant.DefaultBusEventSorts
			);
			IntegrationLogger.StartTransaction(UserConnection, CsConstant.PersonName.Bpm, CsConstant.IntegratorSettings.Settings[this.GetType()].Name, "", "");
			var logId = IntegrationLogger.CurrentLogId;
			PushRequestWrapper(TRequstMethod.GET, url, "", (x, y) => {
				var responceObj = x.DeserializeJson();
				var busEventNotifications = (JArray)responceObj["data"];
				//var total = (responceObj["total"] as JToken).Value<int>();
				if (busEventNotifications != null) {
					OnBusEventNotificationsDataRecived(busEventNotifications, y);
				}
			}, logId);
		}

		public void IniciateLoadChanges() {
			GetBusEventNotification(true);
		}
		/// <summary>
		/// Всем нотификейшенам в ReadedNotificationIds ставит статус "Прочитано"
		/// </summary>
		public void SetNotifyRead() {
			var url = GenerateUrl(
				TIntegratorRequest.BusEventNotification,
				TRequstMethod.PUT
			);

			var json = ReadedNotificationIds.Select(x => new {
				isRead = true,
				id = x
			}).SerializeToJson();

			PushRequestWrapper(TRequstMethod.PUT, url, json, null, IntegrationLogger.CurrentLogId);
			ReadedNotificationIds.Clear();
		}

		/// <summary>
		/// Сохраняет нотификейшен, чтобы потом скопом поставить признак прочитано
		/// </summary>
		/// <param name="notifyId"></param>
		public void AddReadId(string notifyId) {
			ReadedNotificationIds.Add(notifyId);
		}

		/// <summary>
		/// Делает запрос в clientservice и если версия объекта в нем больше за версию в integrationservice, то обновляет объектом из clientservice
		/// </summary>
		/// <param name="integrationInfo"></param>
		public void CreatedOnEntityExist(IntegrationInfo integrationInfo) {
			string jName = integrationInfo.EntityName;
			var data = integrationInfo.Data[jName];
			int version = data.Value<int>("version");
			int jId = data.Value<int>("id");
			string url = string.Format("{0}/AUTO3N/{1}/{2}", _baseClientServiceUrl, jName, jId);

			PushRequestWrapper(TRequstMethod.GET, url, "", (x, y) => {
				var responceObj = JObject.Parse(x);
				var csData = responceObj[jName] as JObject;
				var csVersion = csData.Value<int>("version");
				if (csVersion >= version)
				{
					integrationInfo.Data = responceObj;
					integrationInfo.EntityName = jName;
					integrationInfo.Action = CsConstant.IntegrationActionName.Update;
					_integrationEntityHelper.IntegrateEntity(integrationInfo);
				}
			}, IntegrationLogger.CurrentLogId);
		}

		/// <summary>
		/// Срабатывает на получение нотификейшенов из integrationservice
		/// </summary>
		/// <param name="busEventNotifications"></param>
		/// <param name="userConnection"></param>
		public void OnBusEventNotificationsDataRecived(JArray busEventNotifications, UserConnection userConnection) {
			foreach (JObject busEventNotify in busEventNotifications) {
				var busEvent = busEventNotify[CsConstant.IntegrationEventName.BusEventNotify] as JObject;
				if (busEvent != null) {
					var data = busEvent["data"] as JObject;
					var objectType = busEvent["objectType"].ToString();
					var action = busEvent["action"].ToString();
					var system = busEvent["system"].ToString();
					var notifyId = busEvent["id"].ToString();
					var objectId = busEvent["objectId"].Value<int>();
					if (!string.IsNullOrEmpty(objectType) && data != null)
					{
						IntegrateServiceEntity(data, objectType);
					} else
					{
						if (system == CsConstant.IntegratorSettings.Settings[typeof(OrderServiceIntegrator)].Name)
						{
							var integrator = new OrderServiceIntegrator(userConnection);
							ExportServiceEntity(integrator, objectType, objectId);
						}
					}
					AddReadId(notifyId);
				}
			}
			SetNotifyRead();
		}
		public void IntegrateFromOrderService()
		{

		}

		public void ExportServiceEntity(BaseServiceIntegrator integrator, string name, int id, Action afterIntegrate = null)
		{
			var serviceRequestInfo = ServiceRequestInfo.CreateForExportInBpm(name);
			serviceRequestInfo.Limit = "1";
			serviceRequestInfo.Skip = "0";
			serviceRequestInfo.ServiceObjectId = id.ToString();
			serviceRequestInfo.AfterIntegrate = afterIntegrate;
			integrator.GetRequest(serviceRequestInfo);
		}
		public virtual void IntegrateServiceEntity(JObject serviceEntity, string serviceObjectName)
		{
			var integrationInfo = CsConstant.IntegrationInfo.CreateForImport(UserConnection, CsConstant.IntegrationActionName.Create, serviceObjectName, serviceEntity);
			IntegrationEntityHelper.IntegrateEntity(integrationInfo);
			if (integrationInfo.Result != null && integrationInfo.Result.Exception == CsConstant.IntegrationResult.TResultException.OnCreateEntityExist)
			{
				CreatedOnEntityExist(integrationInfo);
			}
		}
		/// <summary>
		/// Генерирует Url в integrationservice
		/// </summary>
		/// <param name="integratorRequestType">Тип возращаемой сущности</param>
		/// <param name="requstMethod">Тип запроса</param>
		/// <param name="skip">Сколько данныех пропустить от начала</param>
		/// <param name="limit">Сколько данных взять</param>
		/// <param name="filters">Фильтры</param>
		/// <param name="sorts">Сортировки</param>
		/// <returns></returns>
		public string GenerateUrl(TIntegratorRequest integratorRequestType, TRequstMethod requstMethod, string skip = null, string limit = null, Dictionary<string, string> filters = null, Dictionary<string, string> sorts = null) {
			string result = _basePostboxUrl;
			string filtersStr = "";
			string sortStr = "";
			string skipStr = "";
			string limitStr = "";
			//createdAt
			switch (requstMethod) {
				case TRequstMethod.GET:
				case TRequstMethod.PUT:

				break;
				default:
				throw new NotImplementedException();
			}


			switch (integratorRequestType) {
				case TIntegratorRequest.BusEventNotification:
				result += GenerateRouteToRequest("Postbox", _postboxId, "BusEventNotification");
				break;
				case TIntegratorRequest.BusEventNotificationData:
				result += GenerateRouteToRequest("Postbox", _postboxId, "BusEventNotificationData");
				break;
				case TIntegratorRequest.Postbox:
				result += GenerateRouteToRequest("Postbox");
				break;
			}


			if (!string.IsNullOrEmpty(skip))
				skipStr = string.Format("skip={0}", skip);


			if (!string.IsNullOrEmpty(limit))
				limitStr = string.Format("limit={0}", limit);


			if (filters != null && filters.Any()) {
				foreach (var filter in filters) {
					filtersStr += string.Format("filter[{0}]={1}&", filter.Key, filter.Value);
				}
				filtersStr = filtersStr.Remove(filtersStr.Length - 1);
			}


			if (sorts != null && sorts.Any()) {
				foreach (var sort in sorts) {
					sortStr += string.Format("sort[{0}]={1}&", sort.Key, sort.Value);
				}
				sortStr = sortStr.Remove(sortStr.Length - 1);
			}


			string paramStr = GenerateParamRoRequest(skipStr, limitStr, filtersStr, sortStr);
			if (!string.IsNullOrEmpty(paramStr)) {
				result += string.Format("?{0}", paramStr);
			}

			return result;
		}


		private string GenerateRouteToRequest(params object[] routes) {
			return "/" + routes.Aggregate((cur, next) => cur.ToString() + "/" + next.ToString()).ToString();
		}
		private string GenerateParamRoRequest(params string[] param) {
			var collection = param.Where(x => !string.IsNullOrEmpty(x));
			return collection.Any() ? collection.Aggregate((cur, next) => cur + "&" + next) : "";
		}
		private void PushRequestWrapper(TRequstMethod requestMethod, string url, string jsonText, Action<string, UserConnection> callback, Guid logId) {
			if (!_isIntegratorActive) {
				return;
			}
			_integratorHelper.PushRequest(requestMethod, url, jsonText, callback, UserConnection, logId, null, _auth);
		}


		public enum TIntegratorRequest {
			BusEventNotificationData,
			BusEventNotification,
			Postbox
		}

	}
}
