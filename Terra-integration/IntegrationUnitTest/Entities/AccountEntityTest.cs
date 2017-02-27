using IntegrationUnitTest.IEntities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terrasoft.TsConfiguration;

namespace IntegrationUnitTest.Entities {
	public class AccountEntityTest: BaseEntityTest {
		public override int ExternalId {
			get {
				return 5;
			}
		}
		public override string JsonFileName {
			get {
				return "Account";
			}
		}
		public Dictionary<string, object> GetDbFieldsValues() {
			return new Dictionary<string, object>() {
				{ "TsB2B", true }
			};
		}

		[TestMethod]
		public void Test1() {
			var integrator = new IntegrationServiceIntegrator(CsConstant.UserConnection);
			var responceObj = GetJson().DeserializeJson();
			integrator.OnBusEventNotificationsDataRecived((JArray)responceObj["data"], CsConstant.UserConnection);
			var entity = EntityDbTester.GetEntityByExternalId("Account", ExternalId);

			foreach(var valuePair in GetDbFieldsValues()) {
				Assert.IsTrue(IsEntityHasValueInDb(entity, valuePair.Key, valuePair.Value));
			}
		}
	}
}
