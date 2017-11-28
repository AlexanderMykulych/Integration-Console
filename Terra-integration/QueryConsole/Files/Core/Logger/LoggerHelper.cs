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
	public static class LoggerHelper
	{
		/// <summary>
		/// Гарантирует выполнение Action в транзакции логгирования
		/// </summary>
		/// <param name="info">Информация о транзакции</param>
		/// <param name="action">Предикат</param>
		public static void DoInLogBlock(string blockName, Action action)
		{
			try
			{
				var oldBlockId = CreateBlock(blockName);
				try
				{
					action();
				}
				catch (Exception e)
				{
					IntegrationLogger.Error(e);
				}
				finally
				{
					FinishTransaction(oldBlockId);
				}
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e);
			}
		}
		/// <summary>
		/// Создает транзакцию логгирования
		/// </summary>
		/// <param name="info">Информация о транзакции</param>
		/// <returns>Возвращает используется уже рание создана транзакция или новая</returns>
		public static Guid CreateBlock(string blockName, TLogObjectType type = TLogObjectType.Block)
		{
			return IntegrationLogger.StartLogBlock(blockName, type);
		}
		/// <summary>
		/// Завершает текущую транзакцию
		/// </summary>
		/// <param name="info">Информация о транзакции</param>
		public static void FinishTransaction(Guid oldBlockId)
		{
			IntegrationLogger.FinishLogBlock();
			IntegrationLogger.CurrentLogBlockId = oldBlockId;
		}

		public static bool IsActive(string logName)
		{
			var logConfig = SettingsManager.GetLogConfig(logName);
			if (logConfig != null)
			{
				return logConfig.IsActive;
			}
			return false;
		}
	}
}