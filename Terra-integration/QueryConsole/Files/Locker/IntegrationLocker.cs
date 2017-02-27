using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Terrasoft.TsConfiguration
{
	public static class IntegrationLocker
	{
		public static ConcurrentDictionary<string, bool> LockerInfo = new ConcurrentDictionary<string, bool>();
		private static bool isLocckerActive {
			get {
				return CsConstant.IntegratorSettings.isLockerActive;
			}
		}
		public static void Lock(object schemaName, object id, string keyMixin = null)
		{
			if (!isLocckerActive) {
				return;
			}
			var key = GetKey(schemaName, id, keyMixin);
			IntegrationLogger.Info(string.Format("Lock => Schema Name: {0} Id: {1} key: {2}", schemaName, id, key));
			if (!LockerInfo.ContainsKey(key))
			{
				LockerInfo.TryAdd(key, true);
			}
		}
		public static void Unlock(object schemaName, object id, string keyValue = null)
		{
			if (!isLocckerActive) {
				return;
			}
			var key = GetKey(schemaName, id, keyValue);
			IntegrationLogger.Info(string.Format("Unlock => Schema Name: {0} Id: {1} key: {2}", schemaName, id, key));
			if (LockerInfo.ContainsKey(key))
			{
				bool removeItem;
				LockerInfo.TryRemove(key, out removeItem);
			}
		}

		public static bool CheckWithUnlock(object schemaName, object id, string keyValue = null)
		{
			if(!isLocckerActive) {
				return true;
			}
			if(!CheckUnLock(schemaName, id, keyValue))
			{
				Unlock(schemaName, id);
			}
			return CheckUnLock(schemaName, id);
		}
		public static bool CheckUnLock(object schemaName, object id, string keyValue = null)
		{
			return !isLocckerActive || !LockerInfo.ContainsKey(GetKey(schemaName, id, keyValue));
		}

		private static string GetKey(object schemaName, object id, string keyValue)
		{
			return string.Format("{0}_{1}_{2}_{3}", id, schemaName, Thread.CurrentThread.ManagedThreadId, keyValue ?? "!");
		}
	}
}
