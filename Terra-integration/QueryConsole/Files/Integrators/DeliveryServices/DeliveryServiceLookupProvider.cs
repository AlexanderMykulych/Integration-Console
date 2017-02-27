using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Terrasoft.TsConfiguration {
	public interface DeliveryServiceLookupProvider {
		List<Dictionary<string, string>> GetLookupValues(string query = null);
	}
}
