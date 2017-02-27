using Newtonsoft.Json.Linq;
using NodaTime.TimeZones;
using NodaTime.TimeZones.Cldr;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Terrasoft.Common;
using Terrasoft.Core;
using Terrasoft.Core.DB;

namespace Terrasoft.TsConfiguration
{
	public enum MacrosType
	{
		Rule,
		OverRule
	}
	public static class TsMacrosHelper
	{
		public static Dictionary<string, Func<object, object>> MacrosDictImport = new Dictionary<string, Func<object, object>>() {
			{ "DateTimeToYearInteger", x => YearIntegerToDateTime(x)},
			{ "DateWithouTime", x => DateWithoutTime(x) },
			{ "TimeSpanToDate", x => TimeSpanToDate(x) },
			{ "TimeSpanToDateTime", x => TimeSpanToDateTime(x) },
			{ "ToDateTime", x => ToDateTime(x) }
		};
		public static Dictionary<string, Func<object, object>> MacrosDictExport = new Dictionary<string, Func<object, object>>() {
			{ "DateTimeToYearInteger", x => DateTimeToYearInteger(x)},
			{ "DateWithouTime", x => DateWithoutTime(x) },
			{ "TimeSpanToDate", x => DateToTimeSpan(x) },
			{ "TimeSpanToDateTime", x => DateToTimeSpan(x) }
		};
		public static Dictionary<string, Func<object, CsConstant.IntegrationInfo, object>> OverMacrosDictImport = new Dictionary<string, Func<object, CsConstant.IntegrationInfo, object>>() {
			{ "ConvertJson", (x, u) => ConvertStringToJson(x)},
			{ "ConvertJsonArray", (x, u) => ConvertJsonToArray(x)},
			{ "ToLdapName", (x, u)=> LdapNameToSimpleName(x) },
			{ "ToIsoFormat", (x, u) => FromIsoFormat(x) },
			{ "ParseOrderNumber", (x, u) => ParseOrderNumber(x) },
			{ "EmptyStringIfNull",(x, u) => NullIfEmptyString(x) }
		};
		public static Dictionary<string, Func<object, CsConstant.IntegrationInfo, object>> OverMacrosDictExport = new Dictionary<string, Func<object, CsConstant.IntegrationInfo, object>>() {
			{ "ConvertJson", (x, u) => ConvertJsonToString(x)},
			{ "ConvertJsonArray", (x, u) => ConvertArrayToJson(x)},
			{ "ToLdapName", (x, u) => ToLdapName(x) },
			{ "ToIsoFormat", (x, u) => ToIsoFormat(x) },
			{ "EmptyStringIfNull", (x, u) => EmptyStringIfNull(x) },
			{ "GetIsoByAccountId", (x, u) => GetIsoByAccountId(x, u) }
		};
		public static Dictionary<string, Action<object, UserConnection>> BeforeDeleteMacros = new Dictionary<string, Action<object, UserConnection>>() {
			{ "BeforeDeleteContactCommunication", (x, y) => BeforeDeleteContactCommunication(x, y) },
			{ "BeforeDeleteShipmentPosition", (x, y) => BeforeDeleteShipmentPosition(x, y) }
		};
		public static object GetMacrosResultImport(string macrosName, object value, MacrosType type = MacrosType.Rule, CsConstant.IntegrationInfo integrationInfo = null)
		{
			switch (type)
			{
				case MacrosType.Rule:
					if (MacrosDictImport.ContainsKey(macrosName) && MacrosDictImport[macrosName] != null)
					{
						return MacrosDictImport[macrosName](value);
					}
					return value;
				case MacrosType.OverRule:
					if (OverMacrosDictImport.ContainsKey(macrosName) && OverMacrosDictImport[macrosName] != null)
					{
						return OverMacrosDictImport[macrosName](value, integrationInfo);
					}
					return value;
				default:
					return value;
			}

		}
		public static object GetMacrosResultExport(string macrosName, object value, MacrosType type = MacrosType.Rule, CsConstant.IntegrationInfo integrationInfo = null)
		{
			switch (type)
			{
				case MacrosType.Rule:
					if (MacrosDictExport.ContainsKey(macrosName) && MacrosDictExport[macrosName] != null)
					{
						return MacrosDictExport[macrosName](value);
					}
					return value;
				case MacrosType.OverRule:
					if (OverMacrosDictExport.ContainsKey(macrosName) && OverMacrosDictExport[macrosName] != null)
					{
						return OverMacrosDictExport[macrosName](value, integrationInfo);
					}
					return value;
				default:
					return value;
			}
		}
		public static void ExecuteBeforeDeleteMacros(string macrosName, object value, UserConnection userConnection)
		{
			if (BeforeDeleteMacros.ContainsKey(macrosName))
			{
				BeforeDeleteMacros[macrosName](value, userConnection);
			}
		}
		public static Func<object, object> ToLdapName = (x) =>
		{
			var ldapName = CsConstant.IntegratorSettings.LdapDomainName;
			if (!string.IsNullOrEmpty(ldapName))
			{
				return string.Format(@"{0}@{1}", x.ToString(), ldapName);
			}
			return x;
		};
		public static Func<object, object> LdapNameToSimpleName = (x) =>
		{
			if (x == null)
			{
				return x;
			}
			var text = x.ToString();
			var parts = text.Split(new char[] { '@' });
			return parts.FirstOrDefault();
		};

		public static Func<object, object> DateTimeToYearInteger = (x) =>
		{
			if (x == null)
				return null;
			if (x is string)
			{
				DateTime dateTimeResult = DateTime.MinValue;
				if (DateTime.TryParse((string)x, out dateTimeResult))
				{
					return dateTimeResult.ToUniversalTime().Year;
				}
			}
			if (x is DateTime)
			{
				return ((DateTime)x).Year;
			}
			return x;
		};
		public static Func<object, object> DateWithoutTime = (x) =>
		{
			if (x == null)
				return null;
			if (x is string)
			{
				DateTime dateTimeResult = DateTime.MinValue;
				if (((string)x).Contains("T"))
				{
					var dateStr = (string)x;
					if (DateTime.TryParse(dateStr.Substring(0, dateStr.IndexOf("T")), out dateTimeResult))
					{
						return DateTime.SpecifyKind(dateTimeResult, DateTimeKind.Utc).Date;
					}
				}
				if (!DateTime.TryParse((string)x, out dateTimeResult))
				{
					return DateTime.SpecifyKind(dateTimeResult, DateTimeKind.Utc).Date;
				}
			}
			if (x is DateTime)
			{
				return ((DateTime)x).Date;
			}
			return x;
		};

		public static DateTime StartEpohDate = Convert.ToDateTime("1 January 1970");
		public static Func<object, object> TimeSpanToDate = (x) =>
		{
			if (x == null)
				return null;
			if (x is string)
			{
				var timeSpanInt = 0;
				if (int.TryParse((string)x, out timeSpanInt))
				{
					var timeSpan = TimeSpan.FromMilliseconds(timeSpanInt);
					return StartEpohDate.Add(timeSpan).Date;
				}
			}
			if (x is Int64)
			{
				var timeSpan = TimeSpan.FromMilliseconds((Int64)x);
				return StartEpohDate.Add(timeSpan).Date;
			}
			return x;
		};
		public static Func<object, object> TimeSpanToDateTime = (x) =>
		{
			if (x == null)
				return null;
			if (x is string)
			{
				var timeSpanInt = 0;
				if (int.TryParse((string)x, out timeSpanInt))
				{
					var timeSpan = TimeSpan.FromMilliseconds(timeSpanInt);
					return StartEpohDate.Add(timeSpan);
				}
			}
			if (x is int)
			{
				var timeSpan = TimeSpan.FromMilliseconds((int)x);
				return StartEpohDate.Add(timeSpan);
			}
			return x;
		};

		public static Func<object, object> ToDateTime = (x) =>
		{
			if (x == null)
				return null;
			if (x is string)
			{
				DateTime date;
				if (DateTime.TryParse((string)x, out date))
				{
					return date.ToUniversalTime();
				}
			}
			if (x is int)
			{
				var timeSpan = TimeSpan.FromMilliseconds((int)x);
				return StartEpohDate.Add(timeSpan);
			}
			if (x is Int64)
			{
				var timeSpan = TimeSpan.FromMilliseconds((Int64)x);
				return StartEpohDate.Add(timeSpan);
			}
			return x;
		};

		public static Func<object, object> ConvertJsonToString = (x) =>
		{
			if (x == null)
				return null;
			if (x is JToken)
			{
				return ((JToken)x).ToString();
			}
			return x;
		};
		public static Func<object, object> ConvertStringToJson = (x) =>
		{
			if (x == null)
				return null;
			if (x is string)
			{
				return JToken.Parse((string)x);
			}
			return x;
		};
		public static Func<object, object> ConvertJsonToArray = (x) =>
		{
			if (x == null)
				return null;
			if (x is string)
			{
				return JToken.Parse((string)x);
			}
			return x;
		};
		public static Func<object, object> ConvertArrayToJson = (x) =>
		{
			if (x == null)
				return null;
			if (x is IEnumerable)
			{
				return JArray.FromObject((IEnumerable)x);
			}
			return x;
		};


		public static Func<object, object> YearIntegerToDateTime = (x) =>
		{
			if (x == null)
				return null;
			if (x is string)
			{
				var year = 0;
				if (int.TryParse((string)x, out year))
				{
					return new DateTime(year, 1, 1);
				}
			}
			if (x is int)
			{
				return new DateTime((int)x, 1, 1);
			}
			if (x is Int64)
			{
				if ((Int64)x > Int32.MaxValue)
				{
					return new DateTime(Int32.MaxValue - 1, 1, 1);
				}
				return new DateTime(Convert.ToInt32((Int64)x), 1, 1);
			}
			return x;
		};
		public static Func<object, object> DateToTimeSpan = (x) =>
		{
			if (x == null)
				return null;
			if (x is string)
			{
				DateTime date;
				if (DateTime.TryParse((string)x, out date))
				{
					return (date - StartEpohDate).TotalMilliseconds;
				}
			}
			if (x is DateTime)
			{
				return (((DateTime)x) - StartEpohDate).TotalMilliseconds;
			}
			return x;
		};
		public static Func<object, CsConstant.IntegrationInfo, object> GetIsoByAccountId = (x, integrationInfo) =>
		{
			try
			{
				if (x == null)
					return null;
				Guid accountId;
				if (x is JValue && Guid.TryParse((x as JValue).Value.ToString(), out accountId))
				{
					var selectIso = new Select(integrationInfo.UserConnection)
										.Top(1)
										.Column("c", "TsCountryCode")
										.From("AccountAddress").As("a")
										.InnerJoin("Country").As("c")
										.On("a", "CountryId").IsEqual("c", "Id")
										.Where("a", "AccountId").IsEqual(Column.Parameter(accountId))
										.OrderByDesc("a", "CreatedOn") as Select;
					var seletWithTypeFilter = new Select(integrationInfo.UserConnection)
										.Top(1)
										.Column("c", "TsCountryCode")
										.From("AccountAddress").As("a")
										.InnerJoin("Country").As("c")
										.On("a", "CountryId").IsEqual("c", "Id")
										.Where("a", "AccountId").IsEqual(Column.Parameter(accountId))
										.And("a", "AddressTypeId").IsEqual(Column.Parameter(CsConstant.EntityConst.AddressType.Legal))
										.OrderByDesc("a", "CreatedOn") as Select;
					using (var dbExecutor = integrationInfo.UserConnection.EnsureDBConnection())
					{
						using (var reader = seletWithTypeFilter.ExecuteReader(dbExecutor))
						{
							if (reader.Read())
							{
								return reader.GetColumnValue<string>("TsCountryCode");
							}
						}
						using (var readerAll = selectIso.ExecuteReader(dbExecutor))
						{
							if (readerAll.Read())
							{
								return readerAll.GetColumnValue<string>("TsCountryCode");
							}
						}
					}
				}
			} catch(Exception e)
			{
				IntegrationLogger.Error(e, integrationInfo.ToString());
			}
			return string.Empty;
		};
		public static Action<object, UserConnection> BeforeDeleteContactCommunication = (x, userConnection) =>
		{
			if (x == null)
				return;
			if (x is Select)
			{
				var list = (Select)x;
				try
				{
					var updateAccountNotification = new Update(userConnection, "TsAccountNotification")
								.Set("TsCommunicationId", Column.Const(null))
								.Where("TsCommunicationId").In(list) as Update;
					updateAccountNotification.Execute();
				}
				catch (Exception e)
				{
					IntegrationLogger.Error(e);
				}
				try
				{
					var updateContactNotification = new Update(userConnection, "TsContactNotifications")
								.Set("TsCommunicationMeansId", Column.Const(null))
								.Where("TsCommunicationMeansId").In(list) as Update;
					updateContactNotification.Execute();
				}
				catch (Exception e)
				{
					IntegrationLogger.Error(e);
				}
			}
		};
		public static Action<object, UserConnection> BeforeDeleteShipmentPosition = (x, userConnection) =>
		{
			if (x == null)
				return;
			if (x is Select)
			{
				var list = (Select)x;
				try
				{
					var deleteReturnPosition = new Delete(userConnection)
								.From("TsShipmentPosition")
								.Where("TsShipmentId").In(list) as Delete;
					deleteReturnPosition.Execute();
				}
				catch (Exception e)
				{
					IntegrationLogger.Error(e);
				}
			}
		};
		public static Func<object, object> ToIsoFormat = (x) =>
		{
			if(x is JToken)
			{
				var value = ((JToken)x).Value<string>();
				if (!string.IsNullOrEmpty(value))
				{
					var isoFormat = TzdbDateTimeZoneSource.Default.MapTimeZoneId(TimeZoneInfo.FindSystemTimeZoneById(value));
					if (isoFormat != null && !string.IsNullOrEmpty(isoFormat))
					{
						return isoFormat;
					}
				}
			}
			return x;
		};
		public static Func<object, object> EmptyStringIfNull = (x) =>
		{
			if(x == null)
			{
				return string.Empty;
			}
			if (x is JToken)
			{
				var value = ((JToken)x).Value<string>();
				if (value == null)
				{
					return string.Empty;
				}
			}
			return x;
		};
		public static Func<object, object> FromIsoFormat = (x) =>
		{
			if (x is JToken)
			{
				var value = ((JToken)x).Value<string>();
				if (!string.IsNullOrEmpty(value))
				{
					var codeFormat = TzdbDateTimeZoneSource.Default.WindowsMapping.MapZones.FirstOrDefault(y => y.TzdbIds.Contains((string)value) && !string.IsNullOrEmpty(y.WindowsId));
					if (codeFormat != null)
					{
						return JToken.FromObject(codeFormat.WindowsId);
					}
				}
			}
			return x;
		};
		public static Func<object, object> ParseOrderNumber = (x) =>
		{
			if (x is JToken)
			{
				var value = ((JToken)x).Value<string>();
				if (!string.IsNullOrEmpty(value))
				{
					var orderNumberMatch = Regex.Match(value, @"\d+");
					if (orderNumberMatch.Success && orderNumberMatch.Length > 0 && !string.IsNullOrEmpty(orderNumberMatch.Value))
					{
						return JToken.FromObject(orderNumberMatch.Value);
					}
				}
			}
			return x;
		};
		public static Func<object, object> NullIfEmptyString = (x) =>
		{
			if (x is JToken)
			{
				var value = ((JToken)x).Value<string>();
				if (value == string.Empty)
				{
					return JToken.FromObject(null);
				}
			}
			return x;
		};
	}
}
