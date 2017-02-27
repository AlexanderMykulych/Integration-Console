using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodaTime.TimeZones;
using NodaTime.TimeZones.Cldr;
using Terrasoft.Core;
using Terrasoft.Core.DB;
using Terrasoft.Core.Entities;
using Terrasoft.Common;

namespace Terrasoft.TsConfiguration
{
	public static class AddressHelper
	{
		public static void UpdateAddressFromDeliveryService(UserConnection userConnection, Entity addressEntity, Action<Exception> onErrorAction = null, bool withForceUpdate = false)
		{
			try
			{
				var address = addressEntity.GetTypedColumnValue<string>("Address");
				if (!string.IsNullOrEmpty(address))
				{
					var countryId = addressEntity.GetTypedColumnValue<Guid>("CountryId");
					var regionId = addressEntity.GetTypedColumnValue<Guid>("RegionId");
					var cityId = addressEntity.GetTypedColumnValue<Guid>("CityId");
					var street = addressEntity.GetTypedColumnValue<string>("TsStreet");
					var house = addressEntity.GetTypedColumnValue<string>("TsHouse");
					var areaId = addressEntity.GetTypedColumnValue<Guid>("TsDistrictId");
					var zip = addressEntity.GetTypedColumnValue<string>("Zip");
					if (withForceUpdate || IsOneOfNullOrEmptyString(street, house, zip) || IsOneOfEmptyGuid(countryId, regionId, cityId, areaId))
					{
						var addressSearchProvider = new DeliveryServiceAddressProvider(userConnection, null);
						var addressSearchResult = addressSearchProvider.GetLookupValues(address).FirstOrDefault();
						if (addressSearchResult != null && addressSearchResult.Any())
						{
							//For Strings
							SetNewValueIfNeed(addressEntity, "TsStreet", street, addressSearchResult["street"], withForceUpdate);
							SetNewValueIfNeed(addressEntity, "TsHouse", house, addressSearchResult["house"], withForceUpdate);
							SetNewValueIfNeed(addressEntity, "Zip", zip, addressSearchResult["zipCode"], withForceUpdate);
							//For Guides
							SetNewValueIfNeed(userConnection, addressEntity, "CountryId", countryId, addressSearchResult, "country", withForceUpdate);
							SetNewValueIfNeed(userConnection, addressEntity, "CityId", cityId, addressSearchResult, "settlement", withForceUpdate);
							SetRegionValue(userConnection, addressEntity, "RegionId", regionId, addressSearchResult, "region", withForceUpdate);
							SetNewValueIfNeed(userConnection, addressEntity, "TsDistrictId", areaId, addressSearchResult, "area", withForceUpdate);
							addressEntity.UpdateInDB(false);
						} else
						{
							if (withForceUpdate)
							{
								//For Strings
								addressEntity.SetColumnValue("TsStreet", string.Empty);
								addressEntity.SetColumnValue("TsHouse", string.Empty);
								addressEntity.SetColumnValue("Zip", string.Empty);
								//For Guides
								addressEntity.SetColumnValue("CountryId", null);
								addressEntity.SetColumnValue("CityId", null);
								addressEntity.SetColumnValue("RegionId", null);
								addressEntity.SetColumnValue("TsDistrictId", null);
								addressEntity.UpdateInDB(false);
							}
						}
					}
				}
			} catch(Exception e)
			{
				if(onErrorAction != null)
				{
					onErrorAction(e);
				}
				Terrasoft.Configuration.TsEntityLogger.MethodInfoError("UpdateAddressFromDeliveryService", e.ToString());
			}
		}
		public static bool IsOneOfNullOrEmptyString(params string[] strings)
		{
			return strings.Any(x => string.IsNullOrEmpty(x));
		}
		public static bool IsOneOfEmptyGuid(params Guid[] guides)
		{
			return guides.Any(x => x == Guid.Empty);
		}
		public static void SetNewValueIfNeed(Entity addressEntity, string columnName, string currentValue, string newValue, bool forceUpdate = false)
		{
			if(forceUpdate || string.IsNullOrEmpty(currentValue) && !string.IsNullOrEmpty(newValue))
			{
				addressEntity.SetColumnValue(columnName, newValue);
			}
		}
		public static void SetNewValueIfNeed(UserConnection userConnection, Entity addressEntity, string columnName, Guid currentValue, Dictionary<string, string> addressInfo, string newValueName, bool forceUpdate = false)
		{
			try
			{
				string newValue = string.Empty;
				if (addressInfo.TryGetValue(newValueName, out newValue) && (forceUpdate || currentValue == Guid.Empty) && !string.IsNullOrEmpty(newValue))
				{
					int newExternalId = 0;
					if (addressInfo.ContainsKey(newValueName + "Id"))
					{
						newExternalId = int.Parse(addressInfo[newValueName + "Id"]);
					}
					var columnSchema = addressEntity.Schema.Columns.GetByColumnValueName(columnName);
					if (columnSchema != null)
					{
						var schemaName = columnSchema.ReferenceSchema.Name;
						var displayColumnName = columnSchema.ReferenceSchema.PrimaryDisplayColumn.Name;
						var primaryColumnName = columnSchema.ReferenceSchema.PrimaryColumn.Name;
						var select = new Select(userConnection)
									.Top(1)
									.Column(primaryColumnName)
									.Column("TsExternalId")
									.From(schemaName)
									.Where(displayColumnName).IsEqual(Column.Parameter(newValue)) as Select;
						using (var dbExecutor = select.UserConnection.EnsureDBConnection())
						{
							using (var reader = select.ExecuteReader(dbExecutor))
							{
								if (reader.Read())
								{
									var id = reader.GetColumnValue<Guid>(primaryColumnName);
									var externalId = reader.GetColumnValue<int>("TsExternalId");
									addressEntity.SetColumnValue(columnName, id);
									if (externalId == 0)
									{
										var update = new Update(userConnection, schemaName)
													.Set("TsExternalId", Column.Parameter(newExternalId))
													.Where(primaryColumnName).IsEqual(Column.Parameter(id));
										update.Execute();
									}
									return;
								}
							}
						}
						var resultId = Guid.NewGuid();
						var insert = new Insert(userConnection)
										.Set(primaryColumnName, Column.Parameter(resultId))
										.Set(displayColumnName, Column.Parameter(newValue))
										.Set("TsExternalId", Column.Parameter(newExternalId))
										.Into(schemaName) as Insert;
						insert.Execute();
						addressEntity.SetColumnValue(columnName, resultId);
					}
				} else if(forceUpdate)
				{
					addressEntity.SetColumnValue(columnName, null);
				}
			} catch(Exception e)
			{
				Terrasoft.Configuration.TsEntityLogger.MethodInfoError("SetNewValueIfNeed", string.Format("{0} {1} {2} {3}", columnName, currentValue, newValueName, e.ToString()));
			}
		}

		public static void SetRegionValue(UserConnection userConnection, Entity addressEntity, string columnName,
			Guid currentValue, Dictionary<string, string> addressInfo, string newValueName, bool forceUpdate = false)
		{
			try
			{
				string newValue = string.Empty;
				if (addressInfo.TryGetValue(newValueName, out newValue) && (forceUpdate || currentValue == Guid.Empty) && !string.IsNullOrEmpty(newValue))
				{
					int newExternalId = 0;
					if (addressInfo.ContainsKey(newValueName + "Id"))
					{
						newExternalId = int.Parse(addressInfo[newValueName + "Id"]);
					}
					var columnSchema = addressEntity.Schema.Columns.GetByColumnValueName(columnName);
					if (columnSchema != null)
					{
						var schemaName = columnSchema.ReferenceSchema.Name;
						var displayColumnName = columnSchema.ReferenceSchema.PrimaryDisplayColumn.Name;
						var primaryColumnName = columnSchema.ReferenceSchema.PrimaryColumn.Name;
						Guid timeZoneId = Guid.Empty;
						Dictionary<string, string> regionValue = null;
						var regionProvider = new DeliveryServiceRegionProvider(userConnection, new Dictionary<string, string>()
										{
											{ "id", newExternalId.ToString() }
										});
						var select = new Select(userConnection)
									.Top(1)
									.Column(primaryColumnName)
									.Column("TsExternalId")
									.Column("TsTimeZoneId")
									.From(schemaName)
									.Where(displayColumnName).IsEqual(Column.Parameter(newValue)) as Select;
						using (var dbExecutor = select.UserConnection.EnsureDBConnection())
						{
							using (var reader = select.ExecuteReader(dbExecutor))
							{
								if (reader.Read())
								{
									var id = reader.GetColumnValue<Guid>(primaryColumnName);
									var externalId = reader.GetColumnValue<int>("TsExternalId");
									addressEntity.SetColumnValue(columnName, id);
									if (externalId == 0)
									{
										timeZoneId = reader.GetColumnValue<Guid>("TsTimeZoneId");
										if (timeZoneId == Guid.Empty)
										{
											regionValue = regionProvider.GetLookupValues().FirstOrDefault();
											if (regionValue != null)
											{
												timeZoneId = GetTimeZoneId(userConnection, regionValue["timeZone"]);
											}
										}
										var update = new Update(userConnection, schemaName)
													.Set("TsExternalId", Column.Parameter(newExternalId))
													.Where(primaryColumnName).IsEqual(Column.Parameter(id)) as Update;
										if (timeZoneId != Guid.Empty)
										{
											update.Set("TsTimeZoneId", Column.Parameter(timeZoneId));
										}
										update.Execute();
									}
									return;
								}
							}
						}
						regionValue = regionProvider.GetLookupValues().FirstOrDefault();
						if (regionValue != null)
						{
							timeZoneId = GetTimeZoneId(userConnection, regionValue["timeZone"]);
						}
						var resultId = Guid.NewGuid();
						var insert = new Insert(userConnection)
										.Set(primaryColumnName, Column.Parameter(resultId))
										.Set(displayColumnName, Column.Parameter(newValue))
										.Set("TsExternalId", Column.Parameter(newExternalId))
										.Into(schemaName) as Insert;
						if (timeZoneId != Guid.Empty)
						{
							insert.Set("TsTimeZoneId", Column.Parameter(timeZoneId));
						}
						insert.Execute();
						addressEntity.SetColumnValue(columnName, resultId);
					}
				}
				else if (forceUpdate)
				{
					addressEntity.SetColumnValue(columnName, null);
				}
			}
			catch (Exception e)
			{
				Terrasoft.Configuration.TsEntityLogger.MethodInfoError("SetNewValueIfNeed", string.Format("{0} {1} {2} {3}", columnName, currentValue, newValueName, e.ToString()));
			}
		}

		public static Guid GetTimeZoneId(UserConnection userConnection, string value)
		{
			MapZone windowsZone = TzdbDateTimeZoneSource.Default.WindowsMapping.MapZones.FirstOrDefault(x => x.TzdbIds.Contains(value));
			if (windowsZone != null)
			{
				var windowsZoneId = windowsZone.WindowsId;
				var timeZoneSelect = new Select(userConnection)
							.Column("Id")
							.From("TimeZone")
							.Where("Code").IsEqual(Column.Parameter(windowsZoneId)) as Select;
				return timeZoneSelect.ExecuteScalar<Guid>();
			}
			return Guid.Empty;
		}

	}
}
