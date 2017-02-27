using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terrasoft.Core;

namespace Terrasoft.TsConfiguration {
	public class DeliveryServiceAreaProvider : DeliveryServiceEntityProvider {
		public DeliveryServiceAreaProvider(UserConnection userConnection, Dictionary<string, string> filters) : base(userConnection, filters) {
		}
		public override string DisplayFiledName {
			get {
				return "displayName";
			}
		}
		public override string EntityName {
			get {
				return "Area";
			}
		}
		public override Dictionary<string, string> SrcFields {
			get {
				if (srcFields == null) {
					srcFields = base.SrcFields;
					srcFields.Add("region", "region.#ref.id");
				}
				return srcFields;
			}
		}
	}
}
