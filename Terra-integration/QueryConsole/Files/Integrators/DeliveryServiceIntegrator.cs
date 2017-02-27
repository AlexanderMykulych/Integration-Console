using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terrasoft.Core;

namespace Terrasoft.TsConfiguration {
	public class DeliveryServiceIntegrator {
		public UserConnection userConnection;
		public IntegratorHelper integratorHelper;
		public ServiceUrlMaker urlMaker;
		private CsConstant.IntegratorSettings.IntegratorSetting _Settings;
		public CsConstant.IntegratorSettings.IntegratorSetting Settings {
			get {
				if (_Settings == null) {
					_Settings = CsConstant.IntegratorSettings.Settings[GetType()];
				}
				return _Settings;
			}
		}
		public DeliveryServiceIntegrator(UserConnection userConnection) {
			this.userConnection = userConnection;
			integratorHelper = new IntegratorHelper();
			urlMaker = new ServiceUrlMaker(Settings.BaseUrl);
		}

		public void PushGetEntityRequest(string entityName, int limit, string query, string filterFieldName, Dictionary<string, string> filters, Action<string, UserConnection> callback, Action<string, UserConnection> errorCallback) {
			if(!Settings.IsIntegratorActive) {
				return;
			}
			string filterStr = string.Empty;
			string url = string.Empty;
			if (filters != null && filters.ContainsKey("id"))
			{
				url = urlMaker.Make(TServiceObject.Entity, entityName, filters["id"], null, TRequstMethod.GET, null, null);
			}
			else
			{
				filterStr = string.Format("q[{0}]={{\"$like\":\"{1}%\"}}&sort[{0}]=asc", filterFieldName, query);
				if (filters != null && filters.Any())
				{
					filterStr += "&" + filters.Select(x => x.Key + "=" + x.Value).Aggregate((x, y) => x + "&" + y);
				}
				url = urlMaker.Make(TServiceObject.Entity, entityName, null, filterStr, TRequstMethod.GET, limit.ToString(), null);
			}
			
			integratorHelper.PushRequest(TRequstMethod.GET, url, null, callback, userConnection, errorCallback, Settings.Auth, false);
		}

		public void PushGetServiceRequest(string methodName, string query, Dictionary<string, string> filters, Action<string, UserConnection> callback, Action<string, UserConnection> errorCallback) {
			if (!Settings.IsIntegratorActive) {
				return;
			}
			if (!string.IsNullOrEmpty(query)) {
				filters.Add("query", query);
			}
			string filterStr = string.Empty;
			if (filters != null && filters.Any()) {
				filterStr = filters.Select(x => x.Key + "=" + x.Value).Aggregate((x, y) => x + "&" + y);
			}
			string url = urlMaker.Make(TServiceObject.Service, methodName, null, filterStr, TRequstMethod.GET, null, null);
			integratorHelper.PushRequest(TRequstMethod.GET, url, null, callback, userConnection, errorCallback, Settings.Auth, false);
		}
	}
}
