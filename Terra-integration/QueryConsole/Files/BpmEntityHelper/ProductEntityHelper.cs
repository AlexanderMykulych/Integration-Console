using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terrasoft.Common;
using Terrasoft.Core;
using Terrasoft.Core.DB;

namespace Terrasoft.TsConfiguration {
	public static class ProductEntityHelper {
		public static Guid GetOrCreateProductByBrandAndOem(UserConnection userConnection, string brand, string oem, Action<object> insertOrUpdateAction = null) {
			try {
				var brandParam = Column.Parameter(brand);
				var oemParam = Column.Parameter(oem);
				var resultId = Guid.NewGuid();
				var select = new Select(userConnection)
								.Column("Id")
								.From("Product")
								.Where("Code").IsEqual(oemParam)
								.And("TsTradeMarkName").IsEqual(brandParam) as Select;
				using(var dbExecutor = userConnection.EnsureDBConnection()) {
					using(var reader = select.ExecuteReader(dbExecutor)) {
						if(reader.Read()) {
							resultId = DBUtilities.GetColumnValue<Guid>(reader, "Id");
							if(insertOrUpdateAction != null) {
								var update = new Update(userConnection, "Product")
											.Where("Id").IsEqual(Column.Parameter(resultId)) as Update;
								insertOrUpdateAction(update);
								update.Execute();
							}
						}
					}
				}

				var insert = new Insert(userConnection)
								.Into("Product")
								.Set("Id", Column.Parameter(resultId))
								.Set("Code", oemParam)
								.Set("TsTradeMarkName", brandParam);
				if (insertOrUpdateAction != null) {
					insertOrUpdateAction(insert);
				}
				insert.Execute();
				return resultId;
			} catch(Exception e) {
				IntegrationLogger.Error(e, string.Format("[GetOrCreateProductByBrandAndOem] param = (brand={0}, oem={1}, userConnection is null = {2})", brand, oem, userConnection == null));
				return Guid.Empty;
			}
		}
	}
}
