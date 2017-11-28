using IntegrationInfo = Terrasoft.TsIntegration.Configuration.CsConstant.IntegrationInfo;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Ninject.Infrastructure.Language;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Serialization;
using System.ServiceModel.Activation;
using System.ServiceModel.Channels;
using System.ServiceModel.Web;
using System.ServiceModel;
using System.Text.RegularExpressions;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Web.Configuration;
using System.Web;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml.XPath;
using System.Xml;
using System;
using Terrasoft.Common;
using Terrasoft.Core.Configuration;
using Terrasoft.Core.DB;
using Terrasoft.Core.Entities;
using Terrasoft.Core.Factories;
using Terrasoft.Core;
using Terrasoft.UI.WebControls;
using TIntegrationType = Terrasoft.TsIntegration.Configuration.CsConstant.TIntegrationType;
namespace Terrasoft.TsIntegration.Configuration{
	public static class IntegrationLocker
	{
		public static ConcurrentDictionary<string, bool> LockerInfo = new ConcurrentDictionary<string, bool>();
		private static bool isLocckerActive {
			get {
				return CsConstant.IntegratorSettings.isLockerActive;
			}
		}
		public static void Lock(string id, string keyMixin = null)
		{
			if (!isLocckerActive)
			{
				return;
			}
			var key = GetKey(id, keyMixin);
			IntegrationLogger.LockInfo(string.Format("Lock => Schema Name: {0} Id: {1}", id, key));
			if (!LockerInfo.ContainsKey(key))
			{
				LockerInfo.TryAdd(key, true);
			}
		}
		public static void Unlock(string id, string keyValue = null)
		{
			if (!isLocckerActive)
			{
				return;
			}
			var key = GetKey(id, keyValue);
			IntegrationLogger.UnlockInfo(string.Format("Unlock => Schema Name: {0} Id: {1}", id, key));
			if (LockerInfo.ContainsKey(key))
			{
				bool removeItem;
				LockerInfo.TryRemove(key, out removeItem);
			}
		}

		public static bool CheckWithUnlock(string key, string keyValue = null)
		{
			if (!isLocckerActive)
			{
				return true;
			}
			if (!CheckUnLock(key, keyValue))
			{
				Unlock(key);
			}
			return CheckUnLock(key);
		}
		public static bool CheckUnLock(string key, string keyValue = null)
		{
			return !isLocckerActive || !LockerInfo.ContainsKey(GetKey(key, keyValue));
		}

		private static string GetKey(string key, string keyValue)
		{
			return string.Format("{0}_{1}_{2}", key, Thread.CurrentThread.ManagedThreadId, keyValue ?? "!");
		}
	}
}