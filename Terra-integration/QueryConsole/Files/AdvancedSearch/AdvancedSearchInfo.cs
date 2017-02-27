using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terrasoft.Core;
using Terrasoft.Core.DB;

namespace Terrasoft.TsConfiguration
{
	public class AdvancedSearchInfo
	{
		public string StoredProcedureName;
		public Guid Search(UserConnection userConnection, Action<StoredProcedure> procedureAction, Action<Exception> onErrorAction = null)
		{
			try
			{
				if (string.IsNullOrEmpty(StoredProcedureName) || procedureAction == null)
				{
					return Guid.Empty;
				}
				var searchProcedure = new StoredProcedure(userConnection, StoredProcedureName)
					.WithOutputParameter("ResultId", userConnection.DataValueTypeManager.GetInstanceByName("Guid")) as StoredProcedure;
				procedureAction(searchProcedure);
				searchProcedure.PackageName = userConnection.DBEngine.SystemPackageName;
				searchProcedure.Execute();
				var result = searchProcedure.Parameters.GetByName("ResultId").Value;
				if(result != null && result is Guid)
				{
					return (Guid)result;
				}
				return Guid.Empty;
			} catch(Exception e)
			{
				if(onErrorAction != null)
				{
					onErrorAction(e);
				}
			}
			return Guid.Empty;
		}
	}
}
