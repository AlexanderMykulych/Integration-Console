using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terrasoft.Common;
using Terrasoft.Core;
using Terrasoft.Core.DB;

namespace Terrasoft.TsIntegration.Configuration
{
	public class BaseRepositorySettingProvider: IRepositorySettingsProvider
	{
		private readonly IConnectionProvider _connectionProvider;

		private UserConnection UserConnection
		{
			get
			{
				return _connectionProvider.Get<UserConnection>();
			}
		}
		public BaseRepositorySettingProvider(IConnectionProvider connectionProvider)
		{
			_connectionProvider = connectionProvider;
		}
		protected virtual string MappingTableName
		{
			get { return "TsMappingConfig";  }
		}
		protected virtual string MappingTablePrimeColumn {
			get { return "Id"; }
		}
		protected virtual string MappingTableXmlColumn {
			get { return "TsXmlConfig"; }
		}
		protected virtual string IntegrationRefTableName {
			get { return "TsIntegrMapping"; }
		}
		protected virtual string IntegrationRefTableJoinColumn {
			get { return "TsMappingConfigId"; }
		}
		protected virtual string IntegrationRefTableJoinColumn2 {
			get { return "TsIntegrationId"; }
		}
		protected virtual string IntegrationTableName {
			get { return "TsIntegration"; }
		}
		protected virtual string IntegrationTableJoinColumn {
			get { return "Id"; }
		}
		protected virtual string IntegrationTableActiveColumn {
			get { return "TsIsActive"; }
		}
		public virtual List<string> GetXmls()
		{
			var select = new Select(UserConnection)
				.Column("m", MappingTableXmlColumn).As("conf")
				.From(MappingTableName).As("m")
				.InnerJoin(IntegrationRefTableName).As("i")
					.On("i", IntegrationRefTableJoinColumn).IsEqual("m", MappingTablePrimeColumn)
				.InnerJoin(IntegrationTableName).As("ii")
					.On("ii", IntegrationTableJoinColumn).IsEqual("i", IntegrationRefTableJoinColumn2)
				.Where("ii", IntegrationTableActiveColumn).IsEqual(Column.Parameter(1)) as Select;

			var result = new List<string>();
			select.ExecuteReader(x => result.Add(x.GetColumnValue<string>("conf")));
			return result;
		}
	}
}
