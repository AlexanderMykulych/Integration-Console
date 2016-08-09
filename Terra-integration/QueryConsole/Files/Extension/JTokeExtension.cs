using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Terrasoft.CsConfiguration{
	public static class JTokeExtension {
		public static JToken GetJTokenByPath(this JToken jToken, string path, QueryConsole.Files.Constants.CsConstant.TIntegrationType type = QueryConsole.Files.Constants.CsConstant.TIntegrationType.Import) {
			var pItems = path.Split('.');
			foreach (var pItem in pItems) {
				if (!jToken.HasValues || jToken[pItem] == null) {
					if (type != QueryConsole.Files.Constants.CsConstant.TIntegrationType.Import) {
						jToken[pItem] = new JObject();
					} else {
						return new JObject();
					}
				}
				jToken = jToken[pItem];
			}
			return jToken;
		}

		public static T GetJTokenValuePath<T>(this JToken jToken, string path, QueryConsole.Files.Constants.CsConstant.TIntegrationType type = QueryConsole.Files.Constants.CsConstant.TIntegrationType.Import, T defValue = default(T)) {
			var resJToken = jToken.GetJTokenByPath(path, type);
			if(resJToken != null) {
				return resJToken.Value<T>();
			}
			return defValue;
		}
	}
}
