using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Terrasoft.Core;

namespace Terrasoft.TsConfiguration {

	[ServiceContract]
	[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
	public class TsAuto3nService {
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

		[OperationContract]
		[WebInvoke(Method = "POST", UriTemplate = "NotifyChange", BodyStyle = WebMessageBodyStyle.Wrapped,
			RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
		public bool NotifyChange() {
			try {
				IntegrationLogger.StartTransaction(UserConnection, CsConstant.PersonName.Unknown, CsConstant.PersonName.Bpm, "None", "None", "TsAuto3nService - NotifyChange triggered");
				var integrator = new IntegrationServiceIntegrator(UserConnection);
				integrator.IniciateLoadChanges();
				return true;
			} catch(Exception e) {
				IntegrationLogger.ErrorWithStartTransaction(UserConnection, e, CsConstant.PersonName.Unknown, CsConstant.PersonName.Bpm, "None", "None", "TsAuto3nService - NotifyChange triggered error");
				return false;
			}
		}
	}
}
