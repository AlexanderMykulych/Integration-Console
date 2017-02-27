using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System;
using Terrasoft.Core.DB;
using Terrasoft.Core;

namespace Terrasoft.Configuration {

	#region Class: TsBotService
	/*
		Project Path: ..\..\..\QueryConsole\TempFile\TsBotService.cs
		
	*/
	[ServiceContract]
	[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
	public class TsBotService
	{
		private UserConnection _userConnection;
		private UserConnection UserConnection {
			get {
				if (_userConnection == null)
				{
					_userConnection = (UserConnection)HttpContext.Current.Session["UserConnection"];
					if (_userConnection == null)
					{
						var appConnection = HttpContext.Current.Application["AppConnection"] as AppConnection;
						var systemUserConnection = appConnection.SystemUserConnection;
						var autoAuthorization = (bool)Terrasoft.Core.Configuration.SysSettings.GetValue(
							systemUserConnection, "ClientSiteIntegrationAutoAuthorization");
						if (autoAuthorization)
						{
							string userName = (string)Terrasoft.Core.Configuration.SysSettings.GetValue(
							systemUserConnection, "ClientSiteIntegrationUserName");
							if (!string.IsNullOrEmpty(userName))
							{
								string userPassword = (string)Terrasoft.Core.Configuration.SysSettings.GetValue(
								systemUserConnection, "ClientSiteIntegrationUserPassword");
								string workspace = appConnection.SystemUserConnection.Workspace.Name;
								_userConnection = new UserConnection(appConnection);
								_userConnection.Initialize();
								try
								{
									_userConnection.Login(userName, userPassword, workspace, TimeZoneInfo.Utc);
								}
								catch (Exception)
								{
									_userConnection = null;
								}
							}
						}
					}
				}
				if (_userConnection == null)
				{
					throw new ArgumentException("Invalid login or password");
				}
				return _userConnection;
			}
		}
		[OperationContract]
		[WebInvoke(Method = "POST", UriTemplate = "ExecuteAction", BodyStyle = WebMessageBodyStyle.Wrapped,
			RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
		public BotResponse ExecuteAction(string action, string data)
		{
			BotResponse response = null;
			try
			{
				if(action == "create.case")
				{
					var creator = new BotCreateCase(UserConnection);
					var number = creator.CreateCase(data);
					response = new BotCreateCaseResponse()
					{
						Number = number
					};
				}
				return response;
			}
			catch (Exception e)
			{
				return response;
			}
		}
	}

	#endregion


	#region Class: BotCreateCase
	/*
		Project Path: ..\..\..\QueryConsole\TempFile\TsBotService.cs
		
	*/
	public class BotCreateCase
	{
		UserConnection UserConnection;
		public BotCreateCase(UserConnection userConnection)
		{
			UserConnection = userConnection;
		}

		public string CreateCase(string data)
		{
			var dataToken = JToken.Parse(data);
			JToken titleToken = dataToken.SelectToken("Data.Title");
			JToken finishDateToken = dataToken.SelectToken("Data.FinishDate");
			if(titleToken != null && finishDateToken != null)
			{
				var title = titleToken.Value<string>();
				var finishDate = finishDateToken.Value<DateTime>();
				return CreateCase(title, finishDate);
			}
			return string.Empty;
		}
		public string CreateCase(string title, DateTime finishDate)
		{
			var caseSchema = UserConnection.EntitySchemaManager.GetInstanceByName("Case");
			var caseEntity = caseSchema.CreateEntity(UserConnection);
			caseEntity.SetDefColumnValues();
			caseEntity.SetColumnValue("Title", title);
			caseEntity.SetColumnValue("SolutionDate", finishDate);
			caseEntity.Save(false);
			return caseEntity.GetTypedColumnValue<string>("Number");
		}
	}

	#endregion


	#region Class: BotResponse
	/*
		Project Path: ..\..\..\QueryConsole\TempFile\TsBotService.cs
		
	*/
	[DataContract]
	public class BotResponse
	{
	}

	#endregion


	#region Class: BotCreateCaseResponse
	/*
		Project Path: ..\..\..\QueryConsole\TempFile\TsBotService.cs
		
	*/
	[DataContract]
	public class BotCreateCaseResponse: BotResponse
	{
		[DataMember]
		public string Number;
	}

	#endregion


	#region Class: UsrEquipmentHelper
	/*
		Project Path: ..\..\..\QueryConsole\TempFile\UsrEquipmentHelper.cs
		
	*/
	public class UsrEquipmentHelper {
		public void ClearPrimaryOnAddressExcept(UserConnection userConnection, Guid addressId, Guid equipmentId) {
			try {
				var update = new Update(userConnection, "UsrEquipmentAddress")
							.Set("Primary", Column.Const(false))
							.Where("UsrEquipmentId").IsEqual(Column.Parameter(equipmentId))
							.And("Id").IsNotEqual(Column.Parameter(addressId)) as Update;
				update.Execute();
			} catch(Exception e) {
				//TODO: Logged
			}
		}
		public void SetPrimaryOnAddress(UserConnection userConnection, Guid addressId) {
			try {
				var update = new Update(userConnection, "UsrEquipmentAddress")
							.Set("Primary", Column.Const(true))
							.And("Id").IsEqual(Column.Parameter(addressId)) as Update;
				update.Execute();
			} catch(Exception e) {
				//TODO: Logged
			}
		}
	}

	#endregion

}