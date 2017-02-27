using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terrasoft.Core;
using Terrasoft.Core.DB;

namespace Terrasoft.Configuration {
	public class UsrEquipmentHelper {
		public void ClearPrimaryOnAddressExcept(UserConnection userConnection, Guid addressId, Guid equipmentId) {
			try {
				var update = new Update(userConnection, "UsrEquipmentAddress")
							.Set("Primary", Column.Const(false))
							.Where("UsrEquipmentId").IsEqual(Column.Parameter(equipmentId))
							.And("Id").IsNotEqual(Column.Parameter(addressId)) as Update;
				update.Execute();
			} catch(Exception e) {
				//TODO: Logged
			}
		}
		public void SetPrimaryOnAddress(UserConnection userConnection, Guid addressId) {
			try {
				var update = new Update(userConnection, "UsrEquipmentAddress")
							.Set("Primary", Column.Const(true))
							.And("Id").IsEqual(Column.Parameter(addressId)) as Update;
				update.Execute();
			} catch(Exception e) {
				//TODO: Logged
			}
		}
	}
}
