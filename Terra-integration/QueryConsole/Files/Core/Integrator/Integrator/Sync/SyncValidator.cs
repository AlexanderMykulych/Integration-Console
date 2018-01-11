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
	public class SyncValidator : ISyncExportChecker<Guid>
	{
		private IConnectionProvider _connectionProvider;

		private UserConnection userConnection {
			get { return _connectionProvider.Get<UserConnection>(); }
		}
		public SyncValidator(IConnectionProvider connectionProvider)
		{
			_connectionProvider = connectionProvider;
		}
		public class SyncValidatorInfo
		{
			public DateTime LastSyncDate;
			public Guid SyncId;
		}
		protected virtual string SyncTableName {
			get {
				return "TsiIntegrationSync";
			}
		}
		protected virtual string SyncRouteColumnName {
			get {
				return "TsiRoute";
			}
		}
		protected virtual string SyncKeyColumnName {
			get {
				return "TsiKey";
			}
		}
		protected virtual string SyncLastSyncDateColumnName {
			get {
				return "TsiLastSyncDate";
			}
		}
		//Log key=Integration Sync
		public virtual void DoInSync(string routeKey, Guid info, Action syncAction)
		{
			if (info == Guid.Empty)
			{
				return;
			}
			var routeConfig = SettingsManager.GetExportRoutes(routeKey).FirstOrDefault();
			SyncValidatorInfo lastSyncInfo = GetLastSyncDate(routeKey, info);
			if (lastSyncInfo != null)
			{
				if ((DateTime.UtcNow - lastSyncInfo.LastSyncDate).TotalMilliseconds >= routeConfig.SyncMilliseconds)
				{
					syncAction();
					UpdateSyncDate(lastSyncInfo);
				}
			}
			else
			{
				syncAction();
				InsertSyncDate(routeKey, info);
			}
		}
		//Log key=Integration Sync
		public virtual bool IsSyncEnable(string routeKey)
		{
			var routeConfig = SettingsManager.GetExportRoutes(routeKey).FirstOrDefault();
			return routeConfig.IsSyncEnable;
		}
		//Log key=Integration Sync
		protected virtual SyncValidatorInfo GetLastSyncDate(string routeKey, Guid info)
		{
			var select = new Select(userConnection)
					.Top(1)
					.Column("Id")
					.Column(SyncLastSyncDateColumnName)
					.From(SyncTableName)
					.Where(SyncRouteColumnName).IsEqual(Column.Parameter(routeKey))
					.And(SyncKeyColumnName).IsEqual(Column.Parameter(info)) as Select;
			SyncValidatorInfo lastSyncInfo = null;
			using (var dbExecutor = userConnection.EnsureDBConnection())
			{
				using (var reader = select.ExecuteReader(dbExecutor))
				{
					if (reader.Read())
					{
						lastSyncInfo = new SyncValidatorInfo()
						{
							LastSyncDate = reader.GetColumnValue<DateTime>(SyncLastSyncDateColumnName),
							SyncId = reader.GetColumnValue<Guid>("Id")
						};
					}
				}
			}
			return lastSyncInfo;
		}
		//Log key=Integration Sync
		protected virtual void UpdateSyncDate(SyncValidatorInfo lastSyncInfo)
		{
			var update = new Update(userConnection, SyncTableName)
				.Set(SyncLastSyncDateColumnName, Column.Parameter(DateTime.UtcNow))
				.Where("Id").IsEqual(Column.Parameter(lastSyncInfo.SyncId)) as Update;
			update.Execute();
		}
		//Log key=Integration Sync
		protected virtual void InsertSyncDate(string routeKey, Guid info)
		{
			var insert = new Insert(userConnection)
				.Into(SyncTableName)
				.Set(SyncRouteColumnName, Column.Parameter(routeKey))
				.Set(SyncKeyColumnName, Column.Parameter(info))
				.Set(SyncLastSyncDateColumnName, Column.Parameter(DateTime.UtcNow)) as Insert;
			insert.Execute();
		}
	}
}