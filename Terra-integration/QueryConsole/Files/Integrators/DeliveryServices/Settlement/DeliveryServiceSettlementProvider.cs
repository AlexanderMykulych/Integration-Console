using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terrasoft.Core;

namespace Terrasoft.TsConfiguration {
	public class DeliveryServiceSettlementProvider : DeliveryServiceEntityProvider {
		public DeliveryServiceSettlementProvider(UserConnection userConnection, Dictionary<string, string> filters) : base(userConnection, filters) {
		}
		public override string DisplayFiledName {
			get {
				return "displayName";
			}
		}
		public override string EntityName {
			get {
				return "Settlement";
			}
		}
		public override Dictionary<string, string> SrcFields {
			get {
				if (srcFields == null) {
					srcFields = base.SrcFields;
					srcFields.Add("region", "region.#ref.id");
					srcFields.Add("area", "area.#ref.id");
					srcFields.Add("country", "country.#ref.id");
				}
				return srcFields;
			}
		}
	}
}
