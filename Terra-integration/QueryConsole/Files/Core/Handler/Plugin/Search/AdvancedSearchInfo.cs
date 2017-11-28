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
	public class AdvancedSearchInfo
	{
		public string StoredProcedureName;
		//Log key=Handler Util
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
				if (result != null && result is Guid)
				{
					return (Guid)result;
				}
				return Guid.Empty;
			}
			catch (Exception e)
			{
				if (onErrorAction != null)
				{
					onErrorAction(e);
				}
			}
			return Guid.Empty;
		}
	}
}