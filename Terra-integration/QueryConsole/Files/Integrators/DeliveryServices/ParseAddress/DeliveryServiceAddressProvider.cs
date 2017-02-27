using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terrasoft.Core;

namespace Terrasoft.TsConfiguration
{
	public class DeliveryServiceAddressProvider : DeliveryServiceServiceProvider
	{
		public DeliveryServiceAddressProvider(UserConnection userConnection, Dictionary<string, string> filters) : base(userConnection, filters)
		{
		}

		public override string DisplayFiledName {
			get {
				return "displayName";
			}
		}

		public override string EntityName {
			get {
				return "AddressSearchResult";
			}
		}

		public override string MethodName {
			get {
				return "addressSearch";
			}
		}
		public override Dictionary<string, string> SrcFields {
			get {
				if (srcFields == null)
				{
					srcFields = new Dictionary<string, string>();
					srcFields.Add("house", "house");
					srcFields.Add("zipCode", "zipCode");
					srcFields.Add("settlement", "settlement.Settlement.displayName");
					srcFields.Add("settlementId", "settlement.Settlement.id");
					srcFields.Add("region", "settlement.Settlement.region.#ref.name");
					srcFields.Add("regionId", "settlement.Settlement.region.#ref.id");
					srcFields.Add("area", "settlement.Settlement.area.#ref.name");
					srcFields.Add("areaId", "settlement.Settlement.area.#ref.id");
					srcFields.Add("country", "settlement.Settlement.country.#ref.name");
					srcFields.Add("countryId", "settlement.Settlement.country.#ref.id");
					srcFields.Add("street", "street.Street.displayName");
				}
				return srcFields;
			}
		}
	}
}
