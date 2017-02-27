using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terrasoft.Common;
using Terrasoft.Core.DB;

namespace Terrasoft.TsConfiguration
{
	public static class DbExecutorHelper
	{
		public static void ExecuteSelectWithPaging(this DBExecutor dbExecutor, Select select, int startSkip, int rowCount, string orderColumn, Action<IDataReader> readerAction, Action<Exception> OnErrorAction = null)
		{
			select.Column(Column.Const("[ROWCOUNT]")).As("RowCount");
			var wrapSelect = new Select(select.UserConnection)
					.Column(Column.Asterisk())
					.From(select).As("src") as Select;
			var sqlText = ReplaceRowCount(wrapSelect.GetSqlText(), orderColumn);
			bool isReaderEmpty = false;
			int pageIndex = 0;
			while (!isReaderEmpty)
			{
				var pagingSqlText = WrapWithPaging(sqlText, startSkip + pageIndex * rowCount, rowCount);
				using (var reader = dbExecutor.ExecuteReader(pagingSqlText, select.Parameters) as SqlDataReader)
				{
					if (reader == null)
					{
						return;
					}
					if (!reader.HasRows)
					{
						isReaderEmpty = true;
					}
					try
					{
						readerAction(reader);
					}
					catch (Exception e)
					{
						if (OnErrorAction != null)
						{
							OnErrorAction(e);
						}
					}
				}
				pageIndex++;
			}
		}

		private static string ReplaceRowCount(string sqlText, string orderColumn)
		{
			return sqlText.Replace("N'[ROWCOUNT]'", "row_number() over(order by " + orderColumn + ")");
		}
		private static string WrapWithPaging(string sqlText, int skip, int top)
		{

			return sqlText + string.Format("\nWHERE [src].[RowCount] >= {0} and [src].[RowCount] < {1}\n", skip, skip + top);
		}
	}
}
