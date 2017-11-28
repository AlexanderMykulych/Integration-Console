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
	public static class IntegrationLogger
	{
		public static Action<Exception> SimpleLoggerErrorAction = e => IntegrationLogger.Error(e);
		private static TsLogger _log = new TsLogger();
		public static ConcurrentDictionary<int, Guid> ThreadLogIds = new ConcurrentDictionary<int, Guid>();
		public static int CurrentThreadId {
			get { return Thread.CurrentThread.ManagedThreadId; }
		}
		public static Guid CurrentLogBlockId {
			get {
				if (ThreadLogIds.ContainsKey(CurrentThreadId))
				{
					return ThreadLogIds[CurrentThreadId];
				}
				return Guid.Empty;
			}
			set {
				if (value == Guid.Empty)
				{
					//Если Empty, то чистим запись в этом потоке
					Guid oldBlockId;
					ThreadLogIds.TryRemove(CurrentThreadId, out oldBlockId);
					return;
				}
				if (ThreadLogIds.ContainsKey(CurrentThreadId))
				{
					ThreadLogIds[CurrentThreadId] = value;
				}
				else
				{
					ThreadLogIds.TryAdd(CurrentThreadId, value);
				}
			}
		}
		public static TsLogger CurrentLogger {
			get {
				return _log;
			}
		}
		public static Guid StartLogBlock(string blockName, TLogObjectType type = TLogObjectType.Block)
		{
			try
			{
				var oldBlockId = CurrentLogBlockId;
				CurrentLogBlockId = Guid.NewGuid();
				CurrentLogger.CreateBlock(blockName, CurrentLogBlockId, oldBlockId, type);
				return oldBlockId;
			}
			catch (Exception e2)
			{
				CurrentLogger.Instance.Error(e2.ToString());
			}
			return Guid.Empty;
		}

		public static void FinishLogBlock()
		{
			CurrentLogger.UpdateBlockTime(CurrentLogBlockId, DateTime.UtcNow);
		}
		public static void Error(string message)
		{
			try
			{
				CurrentLogger.Error(CurrentLogBlockId, message);
			}
			catch (Exception e2)
			{
				CurrentLogger.Instance.Info(e2.ToString());
			}
		}
		public static void ErrorFormat(string format, params object[] args)
		{
			Error(string.Format(format, args));
		}
		public static void Error(Exception e)
		{
			try
			{
				CurrentLogger.Error(CurrentLogBlockId, e.ToString());
			}
			catch (Exception e2)
			{
				CurrentLogger.Instance.Info(e2.ToString());
			}
		}
		public static void ErrorMapping(string message)
		{
			try
			{
				CurrentLogger.ErrorMapping(CurrentLogBlockId, message);
			}
			catch (Exception e2)
			{
				CurrentLogger.Instance.Info(e2.ToString());
			}
		}
		/// <summary>
		/// Логгирование в файл
		/// </summary>
		/// <param name="message">Сообщение</param>
		public static void Info(string message, TLogObjectType type = TLogObjectType.Info)
		{
			try
			{
				CurrentLogger.Info(CurrentLogBlockId, message, type);
			}
			catch (Exception e)
			{
				CurrentLogger.Instance.Info(e.ToString());
			}
		}
		public static void InfoFormat(string format, params object[] args)
		{
			Info(string.Format(format, args));
		}
		public static void InfoTypeFormat(string format, TLogObjectType type = TLogObjectType.Info, params object[] args)
		{
			Info(string.Format(format, args), type);
		}

		public static void InfoArguments(string key, string format, params object[] args)
		{
			InfoTypeFormat(format, TLogObjectType.InfoMethodArgs, args);
		}

		public static void InfoReturn(string key, object arg)
		{
			InfoTypeFormat("Return: {0}", TLogObjectType.InfoMethodResult, arg);
		}
		public static void Warning(string message)
		{
			try
			{
				CurrentLogger.Info(CurrentLogBlockId, message, TLogObjectType.Warning);
			}
			catch (Exception e)
			{
				CurrentLogger.Instance.Info(e.ToString());
			}
		}
		public static void LockInfo(string message)
		{
			try
			{
				CurrentLogger.Info(CurrentLogBlockId, message, TLogObjectType.Lock);
			}
			catch (Exception e)
			{
				CurrentLogger.Instance.Info(e.ToString());
			}
		}
		public static void UnlockInfo(string message)
		{
			try
			{
				CurrentLogger.Info(CurrentLogBlockId, message, TLogObjectType.Unlock);
			}
			catch (Exception e)
			{
				CurrentLogger.Instance.Info(e.ToString());
			}
		}
		public static void InfoMapping(string message)
		{
			try
			{
				CurrentLogger.InfoMapping(CurrentLogBlockId, message);
			}
			catch (Exception e)
			{
				CurrentLogger.Instance.Info(e.ToString());
			}
		}

		internal static void WarningFormat(string format, params object[] args)
		{
			Warning(string.Format(format, args));
		}
	}
}