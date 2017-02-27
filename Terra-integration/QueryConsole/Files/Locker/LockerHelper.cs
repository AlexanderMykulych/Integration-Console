using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terrasoft.Core.Entities;

namespace Terrasoft.TsConfiguration {
	public static class LockerHelper {
		public static void DoWithEntityLock(object entityId, object schemaName, Action action, Action<Exception> OnExceptionAction = null, string keyMixin = null, bool withLock = true, bool withCheckLock = true) {
			if (!withLock || !withCheckLock || IntegrationLocker.CheckUnLock(schemaName, entityId, keyMixin)) {
				if (withLock) {
					IntegrationLocker.Lock(schemaName, entityId, keyMixin);
				}
				try {
					action();
				} catch(Exception e) {
					if(OnExceptionAction != null) {
						OnExceptionAction(e);
					}
				} finally {
					if (withLock) {
						IntegrationLocker.Unlock(schemaName, entityId, keyMixin);
					}
				}
			}
		}
	}
}
