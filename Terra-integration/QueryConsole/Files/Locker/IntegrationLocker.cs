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
		private static ConcurrentDictionary<string, LockerInfo> LockerInfo = new ConcurrentDictionary<string, LockerInfo>();
		private static bool isLocckerActive {
			get {
				return CsConstant.IntegratorSettings.isLockerActive;
			}
		}
		public static void Lock(string schemaName, Guid id)
		{
			if (!isLocckerActive) {
				return;
			}
			var key = GetKey(schemaName, id);
			if (!LockerInfo.ContainsKey(key))
			{
				LockerInfo.TryAdd(key, new LockerInfo()
				{
					Identifier = id,
					SchemaName = schemaName
				});
			}
		}
		public static void Unlock(string schemaName, Guid id)
		{
			if (!isLocckerActive) {
				return;
			}
			var key = GetKey(schemaName, id);
			if (LockerInfo.ContainsKey(key))
			{
				LockerInfo removeItem = null;
				LockerInfo.TryRemove(key, out removeItem);
			}
		}

		public static bool CheckWithUnlock(string schemaName, Guid id)
		{
			if(!isLocckerActive) {
				return true;
			}
			if(!CheckUnLock(schemaName, id))
			{
				Unlock(schemaName, id);
			}
			return CheckUnLock(schemaName, id);
		}
		public static bool CheckUnLock(string schemaName, Guid id)
		{
			return !isLocckerActive || !LockerInfo.ContainsKey(GetKey(schemaName, id));
		}

		private static string GetKey(string schemaName, Guid id)
		{
			return string.Format("{0}_{1}_{2}", id.ToString(), schemaName, Thread.CurrentThread.ManagedThreadId);
		}
	}

	public class LockerInfo
	{
		public string SchemaName;
		public Guid Identifier;
	}
}
