using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terrasoft.Core.Entities;
using Terrasoft.TsConfiguration;

namespace IntegrationUnitTest {
	public class EntityDbTester {
		public static Entity GetEntityByExternalId(string name, int externalId) {
			return JsonEntityHelper.GetEntityByExternalId(name, externalId, CsConstant.UserConnection, true).Item2;
		}
	}
}
