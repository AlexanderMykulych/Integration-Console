using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Terrasoft.TsConfiguration {
	public static class JTokeExtension {
		public static JToken GetJTokenByPath(this JToken jToken, string path, CsConstant.TIntegrationType type = CsConstant.TIntegrationType.Import) {
			var pItems = path.Split('.');
			foreach (var pItem in pItems) {
				if (!jToken.HasValues || jToken[pItem] == null) {
					if (type != CsConstant.TIntegrationType.Import) {
						jToken[pItem] = new JObject();
					} else {
						return new JObject();
					}
				}
				jToken = jToken[pItem];
			}
			return jToken;
		}
		
		public static JToken GetJTokenValueByPath(this JToken jToken, string path, CsConstant.TIntegrationType type = CsConstant.TIntegrationType.Import)
		{
			var pItems = path.Split('.');
			foreach (var pItem in pItems)
			{
				if(jToken == null)
				{
					return null;
				}
				if(jToken is JObject)
				{
					jToken = jToken[pItem];
				} else if(jToken is JArray)
				{
					jToken = ((JArray)jToken).Last;
					if(jToken != null && jToken is JObject)
					{
						jToken = jToken[pItem];
					}
				}
			}
			return jToken;
		}

		public static T GetJTokenValuePath<T>(this JToken jToken, string path, CsConstant.TIntegrationType type = CsConstant.TIntegrationType.Import, T defValue = default(T)) {
			var resJToken = jToken.GetJTokenByPath(path, type);
			if(resJToken != null && resJToken.HasValues) {
				return resJToken.Value<T>();
			}
			return defValue;
		}
	}
}
