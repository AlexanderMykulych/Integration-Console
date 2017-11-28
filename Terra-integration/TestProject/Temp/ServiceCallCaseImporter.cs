using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Terrasoft.Common;
using Terrasoft.Core;
using Terrasoft.Core.DB;
using Terrasoft.Core.Entities;
//using global::Common.Logging;
using Terrasoft.Configuration.TsiBase;

namespace Terrasoft.Configuration.FileImport
{
	public class ServiceCallCaseImporter
	{
		private IStorageEnumerator _enumerator;
		private UserConnection _userConnection;
		private string BulkEntityConfigName = "TsiBulkCallCaseFile";
		private Guid _lockerId;
		private MappingConfig _config;
		private EntitySchema _entitySchema;
		//private static readonly ILog _log = LogManager.GetLogger("Common");
		private TsiServiceCallCaseSavedService _eventHandler;

		public ServiceCallCaseImporter(IStorageEnumerator enumerator, UserConnection userConnection)
		{
			_enumerator = enumerator;
			_userConnection = userConnection;
			_lockerId = Guid.NewGuid();
			_entitySchema = _userConnection.EntitySchemaManager.GetInstanceByName("TsiServiceCallCase");
			_eventHandler = new TsiServiceCallCaseSavedService(_userConnection);
		}

		public void Import()
		{
			ReadImportConfig();
			while (_enumerator.MoveNext())
			{
				var row = _enumerator.Current;
				try
				{
					var serviceCall = CreateCase(row);
					CreateCharacteristics(row, serviceCall);
					_eventHandler.OnServiceCallCaseSaved(serviceCall);
				}
				catch (Exception e)
				{
					//_log.Error(e.ToString());
				}
			}
		}

		private void CreateCharacteristics(IStorageRow row, Entity serviceCallCase)
		{
			var inserts = CreateCharacteristicInserts(row, serviceCallCase);
			using (var dbExecutor = _userConnection.EnsureDBConnection())
			{
				try
				{
					dbExecutor.StartTransaction();
					foreach (var insert in inserts)
					{
						insert.Execute(dbExecutor);
					}
					dbExecutor.CommitTransaction();
				}
				catch (Exception e)
				{
					dbExecutor.RollbackTransaction();
					//_log.Error(e.ToString());
				}
			}
		}

		private List<Insert> CreateCharacteristicInserts(IStorageRow row, Entity serviceCallCase)
		{
			var result = new List<Insert>();
			foreach (var characterConfig in _config.Characteristic)
			{
				result.Add(CreateCharacteristicInsert(row, characterConfig, serviceCallCase));
			}
			return result.Where(x => x != null).ToList();
		}

		private Insert CreateCharacteristicInsert(IStorageRow row, MappingCharacteristicConfig characterConfig, Entity serviceCallCase)
		{
			object value = characterConfig.Value;
			if (!string.IsNullOrEmpty(characterConfig.FileColumn))
			{
				var rowValue = row.GetByKey(characterConfig.FileColumn);
				if (rowValue != null)
				{
					value = GetCharacteristicValue(characterConfig, rowValue.GetValue());
				}
			}
			if (value == null)
			{
				return null;
			}
			var insert = new Insert(_userConnection)
				.Set("TsiCaseId", Column.Parameter(serviceCallCase.PrimaryColumnValue))
				.Set("TsiCaseCharTypeId", Column.Parameter(characterConfig.CharTypeId))
				.Set("TsiCategoryCharacter", Column.Parameter(Guid.Parse(characterConfig.Name)))
				.Set("TsiCharacteristic", Column.Parameter(characterConfig.CharacterId))
				.Set("TsiLookupName", Column.Parameter(characterConfig.LookupName))
				.Set(characterConfig.CharTypeName, Column.Parameter(value))
				.Into("TsiCaseCharacteristicValue");
			return insert;
		}

		private object GetCharacteristicValue(MappingCharacteristicConfig characterConfig, string value)
		{
			if (characterConfig.CharTypeId == TsiCharTypeConsts.TsiGuidValue)
			{
				var entitySchema = _userConnection.EntitySchemaManager.GetInstanceByName(characterConfig.LookupName);
				return GetValueByDisplayName(characterConfig.LookupName, entitySchema.PrimaryDisplayColumn.ColumnValueName,
					entitySchema.PrimaryColumn.ColumnValueName, value);
			}
			return value;
		}

		private Entity CreateCase(IStorageRow row)
		{
			var entity = _entitySchema.CreateEntity(_userConnection);
			entity.SetDefColumnValues();
			foreach (var columnConfig in _config.Columns)
			{
				SetColumnValue(entity, columnConfig, row);
			}
			entity.Save(false);
			return entity;
		}

		public void SetColumnValue(Entity entity, MappingColumnsConfig columnConfig, IStorageRow row)
		{
			var column = _entitySchema.Columns.FirstOrDefault(x => x.Name == columnConfig.Name);
			if (column != null)
			{
				string fileValue = null;
				if (!string.IsNullOrEmpty(columnConfig.FileColumn))
				{
					var storageValue = row.GetByKey(columnConfig.FileColumn);
					if (storageValue != null)
					{
						fileValue = storageValue.GetValue();
					}
				}
				if (column.DataValueType.UId == DataValueType.LookupDataValueTypeUId)
				{
					if (!string.IsNullOrEmpty(fileValue))
					{
						SetLookupColumn(entity, column, fileValue);
					}
					else if (!string.IsNullOrEmpty(columnConfig.Value))
					{
						var value = Guid.Parse(columnConfig.Value);
						if (value != Guid.Empty)
						{
							entity.SetColumnValue(column.ColumnValueName, value);
						}
					}
					return;
				}
				object valueObject = null;
				string valueString = null;
				if (!string.IsNullOrEmpty(fileValue))
				{
					valueString = fileValue;
				}
				valueString = columnConfig.Value;
				if (column.DataValueType.UId == DataValueType.DateDataValueTypeUId ||
						 column.DataValueType.UId == DataValueType.DateTimeDataValueTypeUId)
				{
					valueObject = DateTime.Parse(valueString);
				}
				else if (column.DataValueType.UId == DataValueType.BooleanDataValueTypeUId)
				{
					valueObject = bool.Parse(valueString);
				}
				else
				{
					if (!string.IsNullOrEmpty(fileValue))
					{
						entity.SetColumnValue(column.ColumnValueName, fileValue);
						return;
					}
				}
				entity.SetColumnValue(column.ColumnValueName, valueObject);
			}
		}
		private void SetLookupColumn(Entity entity, EntitySchemaColumn column, string fileValue)
		{
			var lookupName = column.ReferenceSchema.Name;
			var displayColumnName = column.ReferenceSchema.PrimaryDisplayColumn.Name;
			var primaryColumnName = column.ReferenceSchema.PrimaryColumn.Name;
			GetValueByDisplayName(lookupName, displayColumnName, primaryColumnName, fileValue);
			var result = GetValueByDisplayName(lookupName, displayColumnName, primaryColumnName, fileValue);
			if (result != Guid.Empty)
			{
				entity.SetColumnValue(column.ColumnValueName, result);
			}
		}

		private Guid GetValueByDisplayName(string lookupName, string displayColumnName, string primaryColumnName, string value)
		{
			var select = new Select(_userConnection)
				.Top(1)
				.Column(primaryColumnName)
				.From(lookupName)
				.Where(displayColumnName).IsEqual(Column.Parameter(value)) as Select;
			return select.ExecuteScalar<Guid>();
		}

		public void ReadImportConfig()
		{
			LockConfigRecord();
			ReadConfigRecord();
		}

		public void LockConfigRecord()
		{
			var update = new Update(_userConnection, BulkEntityConfigName)
				.Set("TsiLockerId", Column.Parameter(_lockerId))
				.Where("Id").In(new Select(_userConnection)
					.Top(1)
					.Column("Id")
					.From(BulkEntityConfigName)
					.Where("TsiLockerId").IsNull()
					.OrderByAsc("CreatedOn")) as Update;
			update.Execute();
		}

		public void ReadConfigRecord()
		{
			var esq = new EntitySchemaQuery(_userConnection.EntitySchemaManager, BulkEntityConfigName);
			esq.RowCount = 1;
			esq.AddAllSchemaColumns();
			esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, "TsiLockerId", _lockerId));
			var entity = esq.GetEntityCollection(_userConnection).FirstOrDefault();
			if (entity != null)
			{
				var file = entity.GetBytesValue("TsiFile");
				var mapping = entity.GetTypedColumnValue<string>("TsiMappingConfig");
				_enumerator.Load(file);
				_config = JsonConvert.DeserializeObject<MappingConfig>(mapping);
			}
		}
	}
}
