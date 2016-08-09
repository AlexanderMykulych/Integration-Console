using QueryConsole.Files.Integrators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Terrasoft.Core;
using Terrasoft.TsConfiguration;

namespace QueryConsole.Files.WCFService {
	[ServiceContract]
	public interface IAuto3nService
	{
		bool NotifyChange();
	}
	public class TsAuto3nService: IAuto3nService {
		private UserConnection _userConnection;
		private UserConnection UserConnection {
			get {
				if (_userConnection == null) {
					_userConnection = (UserConnection)HttpContext.Current.Session["UserConnection"];
					if (_userConnection == null) {
						var appConnection = HttpContext.Current.Application["AppConnection"] as AppConnection;
						var systemUserConnection = appConnection.SystemUserConnection;
						var autoAuthorization = (bool)Terrasoft.Core.Configuration.SysSettings.GetValue(
							systemUserConnection, "ClientSiteIntegrationAutoAuthorization");
						if (autoAuthorization) {
							string userName = (string)Terrasoft.Core.Configuration.SysSettings.GetValue(
							systemUserConnection, "ClientSiteIntegrationUserName");
							if (!string.IsNullOrEmpty(userName)) {
								string userPassword = (string)Terrasoft.Core.Configuration.SysSettings.GetValue(
								systemUserConnection, "ClientSiteIntegrationUserPassword");
								string workspace = appConnection.SystemUserConnection.Workspace.Name;
								_userConnection = new UserConnection(appConnection);
								_userConnection.Initialize();
								try {
									_userConnection.Login(userName, userPassword, workspace, TimeZoneInfo.Utc);
								} catch (Exception) {
									_userConnection = null;
								}
							}
						}
					}
				}
				if (_userConnection == null) {
					throw new ArgumentException("Invalid login or password");
				}
				return _userConnection;
			}
		}
		public bool NotifyChange() {
			try {
				var integrator = new IntegrationServiceIntegrator(UserConnection);
				integrator.IniciateLoadChanges();
				return true;
			} catch(Exception e) {
				//TsEntityLogger.Error("AUTO3N Service Error, NotifyChange: {0}", e.ToString());
				return false;
			}
		}
	}
}
