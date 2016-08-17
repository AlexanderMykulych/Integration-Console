using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Terrasoft.TsConfiguration
{
	public static class IntegrationLocker
	{
		private static Dictionary<string, LockerInfo> LockerInfo = new Dictionary<string, LockerInfo>();
		private static int CurrentId
		{
			get
			{
				return Thread.CurrentThread.ManagedThreadId;
			}
		}
		public static void Lock(string schemaName, Guid id)
		{
			var key = GetKey(schemaName);
			if (!LockerInfo.ContainsKey(key))
			{
				LockerInfo.Add(key, new LockerInfo()
				{
					Identifier = id,
					SchemaName = schemaName
				});
			}
		}
		public static void Unlock(string schemaName)
		{
			var key = GetKey(schemaName);
			if (LockerInfo.ContainsKey(key))
			{
				LockerInfo.Remove(key);
			}
		}

		public static bool CheckWithUnlock(string schemaName, Guid id)
		{
			if(!CheckUnLock(schemaName, id))
			{
				Unlock(schemaName);
			}
			return CheckUnLock(schemaName, id);
		}
		public static bool CheckUnLock(string schemaName, Guid id)
		{
			return !LockerInfo.ContainsKey(GetKey(schemaName));
		}

		private static string GetKey(string schemaName)
		{
			return string.Format("{0}_{1}", CurrentId.ToString(), schemaName);
		}
	}

	public class LockerInfo
	{
		public string SchemaName;
		public Guid Identifier;
	}
}
