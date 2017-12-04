using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terrasoft.Core;
using Terrasoft.Core.Entities;

namespace Terrasoft.TsIntegration.Configuration
{
	public class IntegrTsEntity : IIntegrationObject
	{
		private Entity _entity;
		private string _schemaName;
		public IntegrTsEntity(string schemaName)
		{
			_schemaName = schemaName;
			CreateNewEntity();
		}

		private void CreateNewEntity()
		{
			var connection = ObjectFactory.Get<IConnectionProvider>();
			if (connection != null)
			{
				var userConnection = connection.Get<UserConnection>();
				if (userConnection != null)
				{
					var schema = userConnection.EntitySchemaManager.GetInstanceByName(_schemaName);
					var entity = schema.CreateEntity(userConnection);
					entity.SetDefColumnValues();
					SetObject(entity);
				}
			}
		}

		public IntegrTsEntity(Entity entity)
		{
			_entity = entity;
		}
		public object GetObject()
		{
			return _entity;
		}

		public void SetObject(object obj)
		{
			_entity = (Entity)obj;
		}
		
		public T GetProperty<T>(string name, T defaultValue = default(T))
		{
			if (_entity != null)
			{
				return _entity.GetTypedColumnValue<T>(name);
			}
			return defaultValue;
		}

		public void SetProperty(string name, object obj)
		{
			if (_entity != null)
			{
				_entity.SetColumnValue(name, obj);
			}
		}

		public string GetRootName(string defaultValue = null)
		{
			if (_entity != null)
			{
				return _entity.SchemaName;
			}
			return defaultValue;
		}

		public IIntegrationObject GetSubObject(string path)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<IIntegrationObject> GetSubObjects(string path)
		{
			throw new NotImplementedException();
		}

		public void FromObject(object obj)
		{
			throw new NotImplementedException();
		}
	}
}
