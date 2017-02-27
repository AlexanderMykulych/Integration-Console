using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Terrasoft.Core;

namespace Terrasoft.TsConfiguration
{
	public static class DependentEntityLoader
	{
		public static ConcurrentDictionary<int, int> ThreadLoadEntityLevel = new ConcurrentDictionary<int, int>();
		public static int CurrentThreadId
		{
			get
			{
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
						var serviceRequestInfo = ServiceRequestInfo.CreateForExportInBpm(name);
						serviceRequestInfo.Limit = "1";
						serviceRequestInfo.ServiceObjectId = externalId.ToString();
						serviceRequestInfo.UpdateIfExist = true;
						serviceRequestInfo.ReupdateUrl = true;
						var integrator = IntegratorBuilder.Build(name, userConnection);
						if (integrator != null)
						{
							integrator.GetRequest(serviceRequestInfo);
						}
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
			return level <= CsConstant.IntegratorSettings.LoadDependentEntityLevel;
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
