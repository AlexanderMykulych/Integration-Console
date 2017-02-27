using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terrasoft.Core;

namespace Terrasoft.TsConfiguration {
	public abstract class DeliveryServiceEntityProvider : DeliveryServiceLookupProvider {
		public DeliveryServiceIntegrator integrator;
		public Dictionary<string, string> Filters;
		public abstract string EntityName {
			get;
		}
		public virtual string IdFieldName {
			get {
				return Settings.IdFieldName;
			}
		}
		public abstract string DisplayFiledName {
			get;
		}
		public virtual int Limit {
			get {
				return Settings.RequestLimit;
			}
		}
		protected CsConstant.DeliveryServiceSettings.DeliveryServiceSettingsValue settings;
		public virtual CsConstant.DeliveryServiceSettings.DeliveryServiceSettingsValue Settings {
			get {
				if (settings == null) {
					if (!CsConstant.DeliveryServiceSettings.Settings.TryGetValue(GetType(), out settings)) {
						settings = CsConstant.DeliveryServiceSettings.Settings[typeof(DeliveryServiceEntityProvider)];
					}
				}
				return settings;
			}
		}
		protected Dictionary<string, string> srcFields;
		public virtual Dictionary<string, string> SrcFields {
			get {
				if (srcFields == null) {
					srcFields = new Dictionary<string, string>() {
						{ "id", IdFieldName },
						{ "displayValue", DisplayFiledName }
					};
				}
				return srcFields;
			}
		}
		public DeliveryServiceEntityProvider(UserConnection userConnection, Dictionary<string, string> filters) {
			integrator = new DeliveryServiceIntegrator(userConnection);
			Filters = filters ?? new Dictionary<string, string>();
		}
		public virtual List<Dictionary<string, string>> GetLookupValues(string query = null) {
			var result = new List<Dictionary<string, string>>();
			bool filtering = !string.IsNullOrEmpty(query);
			integrator.PushGetEntityRequest(EntityName, Limit, query, DisplayFiledName, Filters, (resultJson, resutlUserConnection) => {
				var jObject = JObject.Parse(resultJson);
				if (Filters.ContainsKey("id"))
				{
					if (jObject != null)
					{
						JToken entityValues = jObject[EntityName];
						result.Add(GetResult(entityValues));
					}
				}
				else
				{
					var entities = jObject.SelectToken("data");
					if (entities != null && entities.Any())
					{
						foreach (var entity in entities)
						{
							JToken entityValues = entity[EntityName];
							result.Add(GetResult(entityValues));
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
			foreach (var srcField in SrcFields) {
				var jToken = entityValues.SelectToken(srcField.Value);
				string value = string.Empty;
				if(jToken != null) {
					value = jToken.Value<string>();
				}
				result.Add(srcField.Key, value);
			}
			return result;
		}
	}
}
