using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terrasoft.Core;

namespace Terrasoft.TsConfiguration {
	class DeliveryServiceStreetProvider : DeliveryServiceServiceProvider {
		public DeliveryServiceStreetProvider(UserConnection userConnection, Dictionary<string, string> filters) : base(userConnection, filters) {
		}
		public override string DisplayFiledName {
			get {
				return "displayName";
			}
		}
		public override string EntityName {
			get {
				return "Street";
			}
		}
		public override string MethodName {
			get {
				return "streetSearch";
			}
		}
		public override Dictionary<string, string> SrcFields {
			get {
				if (srcFields == null) {
					srcFields = base.SrcFields;
					srcFields.Add("settlement", "settlement.#ref.id");
				}
				return srcFields;
			}
		}
	}
}
