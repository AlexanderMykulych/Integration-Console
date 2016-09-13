using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terrasoft.Common;
using Terrasoft.Core;
using Terrasoft.Core.DB;

namespace Terrasoft.TsConfiguration {
	public static class AccountEntityHelper {
		public static void ImportInOrderServiceIfNeed(Guid id, UserConnection userConnection) {
			try {
				var select = new Select(userConnection)
							.Top(1)
							.Column("a", "Id")
							.From("Account").As("a")
							.Where("a", "Id").IsEqual(Column.Parameter(id))
							.And("a", "TsOrderServiceId").IsEqual(Column.Const(0)) as Select;
				using(var dbExecutor = select.UserConnection.EnsureDBConnection()) {
					using(var reader = select.ExecuteReader(dbExecutor)) {
						if(reader.Read()) {
							var integrator = new OrderServiceIntegrator(userConnection);
							integrator.IntegrateBpmEntity(id, "Account", new CounteragentHandler());
						}
					}
				}
			} catch(Exception e) {
				IntegrationLogger.StartTransaction(userConnection, CsConstant.PersonName.Bpm, CsConstant.PersonName.OrderService, "Account", "Counteragent", string.Format("Id = '{{0}}'", id));
				IntegrationLogger.Error(e);
			}
		}
	}
}
