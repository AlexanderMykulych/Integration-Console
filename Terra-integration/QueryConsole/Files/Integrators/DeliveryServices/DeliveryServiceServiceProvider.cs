using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terrasoft.Core;

namespace Terrasoft.TsConfiguration {
	public abstract class DeliveryServiceServiceProvider : DeliveryServiceLookupProvider {
		public abstract string MethodName {
			get;
		}
		public abstract string EntityName {
			get;
		}
		public virtual string IdFiledName {
			get {
				return Settings.IdFieldName;
			}
		}
		public abstract string DisplayFiledName {
			get;
		}
		public virtual int FilterLimit {
			get {
				return Settings.RequestLimit;
			}
		}
		protected Dictionary<string, string> srcFields;
		public virtual Dictionary<string, string> SrcFields {
			get {
				if(srcFields == null) {
					srcFields = new Dictionary<string, string>() {
						{ "id", IdFiledName },
						{ "displayValue", DisplayFiledName }
					};
				}
				return srcFields;
			}
		}
		protected CsConstant.DeliveryServiceSettings.DeliveryServiceSettingsValue settings;
		public virtual CsConstant.DeliveryServiceSettings.DeliveryServiceSettingsValue Settings {
			get {
				if (settings == null) {
					if (!CsConstant.DeliveryServiceSettings.Settings.TryGetValue(GetType(), out settings)) {
						settings = CsConstant.DeliveryServiceSettings.Settings[typeof(DeliveryServiceServiceProvider)];
					}
				}
				return settings;
			}
		}
		public Dictionary<string, string> Filters;
		DeliveryServiceIntegrator integrator;
		public DeliveryServiceServiceProvider(UserConnection userConnection, Dictionary<string, string> filters) {
			integrator = new DeliveryServiceIntegrator(userConnection);
			Filters = filters ?? new Dictionary<string, string>();
		}
		public virtual List<Dictionary<string, string>> GetLookupValues(string query = null) {
			var result = new List<Dictionary<string, string>>();
			bool filtering = !string.IsNullOrEmpty(query);
			integrator.PushGetServiceRequest(MethodName, query, Filters, (resultJson, resutlUserConnection) => {
				var jObject = JObject.Parse(resultJson);
				JToken entities = jObject.SelectToken("data");
				if(entities == null)
				{
					var token = jObject.SelectToken(EntityName);
					if(token != null && token.HasValues)
					{
						entities = new JArray();
						((JArray)entities).Add(jObject);
					}
				}
				if (entities != null && entities.Any()) {
					foreach (var entity in entities) {
						JToken entityValues = entity[EntityName];
						result.Add(GetResult(entityValues));
						if (result.Count > FilterLimit) {
							break;
						}
					}
				}
			}, (errorText, errorUserConnection) => {
				IntegrationLogger.AfterRequestError(new Exception(errorText));
			});
			return result;
		}
		public virtual Dictionary<string, string> GetResult(JToken entityValues) {
			var result = new Dictionary<string, string>();
			foreach (var srcField in SrcFields)
			{
				var jToken = entityValues.SelectToken(srcField.Value);
				string value = string.Empty;
				if (jToken != null)
				{
					value = jToken.Value<string>();
				}
				result.Add(srcField.Key, value);
			}
			return result;
		}
	}
}
