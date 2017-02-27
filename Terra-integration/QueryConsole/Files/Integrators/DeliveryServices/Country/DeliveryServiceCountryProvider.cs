using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terrasoft.Core;

namespace Terrasoft.TsConfiguration {
	public class DeliveryServiceCountryProvider : DeliveryServiceEntityProvider {
		public DeliveryServiceCountryProvider(UserConnection userConnection, Dictionary<string, string> filters) : base(userConnection, filters) {
		}
		public override string EntityName {
			get {
				return "Country";
			}
		}
		public override string DisplayFiledName {
			get {
				return "name";
			}
			
		}
		public override Dictionary<string, string> SrcFields {
			get {
				if (srcFields == null) {
					srcFields = base.SrcFields;
					srcFields.Add("countryCode", "iso3166Number");
				}
				return srcFields;
			}
		}
	}
}
