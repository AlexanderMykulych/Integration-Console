using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terrasoft.Core;

namespace Terrasoft.TsConfiguration
{
	public static class LoggerHelper
	{
		/// <summary>
		/// Гарантирует выполнение Action в транзакции логгирования
		/// </summary>
		/// <param name="info">Информация о транзакции</param>
		/// <param name="action">Предикат</param>
		public static void DoInTransaction(LoggerInfo info, Action action)
		{
			try
			{
				var isNew = CreateTransaction(info);
				try
				{
					action();
				}
				catch (Exception e)
				{
					IntegrationLogger.Error(e);
				}
				if (isNew)
				{
					FinishTransaction(info);
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
		public static bool CreateTransaction(LoggerInfo info)
		{
			try
			{
				return IntegrationLogger.StartTransaction(info.UserConnection, info.RequesterName, info.ReciverName, info.BpmObjName,
					info.ServiceObjName, info.AdditionalInfo, false, info.UseExistTransaction);
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e);
				return false;
			}
		}
		/// <summary>
		/// Завершает текущую транзакцию
		/// </summary>
		/// <param name="info">Информация о транзакции</param>
		public static void FinishTransaction(LoggerInfo info)
		{
			IntegrationLogger.FinishTransaction(info.UserConnection);
		}
	}
}
