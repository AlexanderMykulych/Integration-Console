using IntegrationInfo = Terrasoft.TsIntegration.Configuration.CsConstant.IntegrationInfo;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Ninject.Infrastructure.Language;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Serialization;
using System.ServiceModel.Activation;
using System.ServiceModel.Channels;
using System.ServiceModel.Web;
using System.ServiceModel;
using System.Text.RegularExpressions;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Web.Configuration;
using System.Web;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml.XPath;
using System.Xml;
using System;
using Terrasoft.Common;
using Terrasoft.Core.Configuration;
using Terrasoft.Core.DB;
using Terrasoft.Core.Entities;
using Terrasoft.Core.Factories;
using Terrasoft.Core;
using Terrasoft.UI.WebControls;
using TIntegrationType = Terrasoft.TsIntegration.Configuration.CsConstant.TIntegrationType;
namespace Terrasoft.TsIntegration.Configuration{
	public class MapperDbWorker : IMapperDbWorker
	{
		private IConnectionProvider _connectionProvider;

		private UserConnection userConnection
		{
			get { return _connectionProvider.Get<UserConnection>(); }
		}

		public MapperDbWorker(IConnectionProvider connectionProvider)
		{
			_connectionProvider = connectionProvider;
		}
		public bool IsExists(string entityName, string externalIdPath, object externalId)
		{
			var select = new Select(userConnection)
							.Column(Func.Count(Column.Const(1))).As("Count")
							.From(entityName)
							.Where(externalIdPath).IsEqual(Column.Parameter(externalId)) as Select;
			using (DBExecutor dbExecutor = userConnection.EnsureDBConnection())
			{
				using (IDataReader reader = select.ExecuteReader(dbExecutor))
				{
					while (reader.Read())
					{
						return DBUtilities.GetColumnValue<int>(reader, "Count") > 0;
					}
				}
			}
			return false;
		}
	}
}