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
	public static class DependentEntityLoader
	{
		public static ConcurrentDictionary<int, int> ThreadLoadEntityLevel = new ConcurrentDictionary<int, int>();
		public static int CurrentThreadId {
			get {
				return Thread.CurrentThread.ManagedThreadId;
			}
		}
		/// <summary>
		/// Инициирует загрузку связаного объекта, если уровень загрузки не привышает значение системной настройки LoadDependentEntityLevel
		/// </summary>
		/// <param name="name">Имя в сервисе</param>
		/// <param name="externalId">Идентификатор в сервисе</param>
		/// <param name="userConnection">Подключение пользователя</param>
		/// <param name="afterIntegrate">Срабатывает после завершения интеграции. Если импорт не инициировался, то предикат не вызывается</param>
		/// <param name="onException">Срабатывает на исключение</param>
		public static void LoadDependenEntity(string name, int externalId, UserConnection userConnection, Action afterIntegrate = null, Action<Exception> onException = null)
		{
			try
			{
				AddCurrentLevel();
				if (!string.IsNullOrEmpty(name) && externalId > 0 && CheckCurrentLoadLevel())
				{
					try
					{
						var serviceRequestInfo = ServiceRequestInfo.CreateForExportInBpm();
						serviceRequestInfo.UpdateIfExist = true;
						//var integrator = new BaseServiceIntegrator(userConnection);
						//if (integrator != null)
						//{
						//	integrator.GetRequest(serviceRequestInfo);
						//}
					}
					catch (Exception e)
					{
						if (onException != null)
						{
							onException(e);
						}
					}
				}
			}
			catch (Exception e)
			{
				if (onException != null)
				{
					onException(e);
				}
			}
			finally
			{
				DecCurrentLevel();
			}
			if (afterIntegrate != null)
			{
				afterIntegrate();
			}
		}
		public static bool CheckCurrentLoadLevel()
		{
			var level = GetCurrentLevel();
			if (level < 0)
			{
				IntegrationLogger.Error(new Exception("level is less than zero!"));
				return false;
			}
			return level <= 2;
		}
		public static int GetCurrentLevel()
		{
			int level = -1;
			ThreadLoadEntityLevel.TryGetValue(CurrentThreadId, out level);
			return level;
		}

		public static void AddCurrentLevel()
		{
			if (!ThreadLoadEntityLevel.ContainsKey(CurrentThreadId))
			{
				ThreadLoadEntityLevel.TryAdd(CurrentThreadId, 0);
			}
			ThreadLoadEntityLevel[CurrentThreadId]++;
		}
		public static void DecCurrentLevel()
		{
			if (!ThreadLoadEntityLevel.ContainsKey(CurrentThreadId))
			{
				ThreadLoadEntityLevel.TryAdd(CurrentThreadId, 0);
			}
			ThreadLoadEntityLevel[CurrentThreadId]--;
		}
	}
}