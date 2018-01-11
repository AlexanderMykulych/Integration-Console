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
	[ServiceContract]
	[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
	public class IntegrationService
	{
		private HttpContextBase _httpContext;
		protected virtual HttpContextBase CurrentHttpContext {
			get { return _httpContext ?? (_httpContext = new HttpContextWrapper(HttpContext.Current)); }
			set { _httpContext = value; }
		}
		private UserConnection _userConnection;
		protected virtual UserConnection UserConnection {
			get {
				if (_userConnection != null)
				{
					return _userConnection;
				}
				_userConnection = CurrentHttpContext.Session["UserConnection"] as UserConnection;
				if (_userConnection != null)
				{
					return _userConnection;
				}
				var appConnection = (AppConnection)CurrentHttpContext.Application["AppConnection"];
				_userConnection = appConnection.SystemUserConnection;
				return _userConnection;
			}
		}
		private IIntegrationObjectProvider _iObjectProvider;
		public virtual IIntegrationObjectProvider IObjectProvider {
			set {
				_iObjectProvider = value;
			}
			get {
				if (_iObjectProvider == null)
				{
					_iObjectProvider = ObjectFactory.Get<IIntegrationObjectProvider>();
				}
				return _iObjectProvider;
			}
		}
		[OperationContract]
		[WebGet(UriTemplate = "entity/{routeKey}/{id}")]
		public virtual Stream Get(string routeKey, string id)
		{
			return new IntegrationServiceWrapper(UserConnection, IObjectProvider).Get(routeKey, id);
		}

		[OperationContract]
		[WebInvoke(Method = "POST", UriTemplate = "entity/{routeKey}", BodyStyle = WebMessageBodyStyle.Bare)]
		public virtual Stream Post(string routeKey, Stream requestStream)
		{
			return new IntegrationServiceWrapper(UserConnection, IObjectProvider).Post(routeKey, requestStream);
		}
		[OperationContract]
		[WebInvoke(Method = "PUT", UriTemplate = "entity/{routeKey}", BodyStyle = WebMessageBodyStyle.Bare)]
		public virtual Stream Put(string routeKey, Stream requestStream)
		{
			return new IntegrationServiceWrapper(UserConnection, IObjectProvider).Put(routeKey, requestStream);
		}
		[OperationContract]
		[WebInvoke(Method = "POST", UriTemplate = "integrate/{endPointName}", BodyStyle = WebMessageBodyStyle.Bare)]
		public virtual Stream PostEndPoint(string endPointName, Stream requestStream)
		{
			return new IntegrationServiceWrapper(UserConnection, IObjectProvider).PostEndPoint(endPointName, requestStream);
		}
	}
}