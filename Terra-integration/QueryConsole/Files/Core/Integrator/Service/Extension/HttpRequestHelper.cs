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
	public static class HttpRequestHelper
	{
		public static void SetHeaderValue(this HttpWebRequest request, string key, string value)
		{
			if (HeaderSetterDict.ContainsKey(key))
			{
				HeaderSetterDict[key](request, value);
			}
			else
			{
				request.Headers.Add(key, value);
			}
		}
		#region Header Setter
		public static Dictionary<string, Action<HttpWebRequest, string>> HeaderSetterDict = new Dictionary<string, Action<HttpWebRequest, string>>()
		{
			{"Accept", (request, value) => request.Accept=value},
			{"Connection", (request, value) => request.Connection=value},
			{"Content-Length", (request, value) => request.ContentLength=long.Parse(value)},
			{"Content-Type", (request, value) => request.ContentType=value},
			{"Date", (request, value) => request.Date=DateTime.Parse(value)},
			{"Expect", (request, value) => request.Expect=value},
			{"Host", (request, value) => request.Host=value},
			{"If-Modified-Since", (request, value) => request.IfModifiedSince=DateTime.Parse(value)},
			{"Referer", (request, value) => request.Referer=value},
			{"Transfer-Encoding", (request, value) => request.TransferEncoding=value },
			{"User-Agent", (request, value) => request.UserAgent=value}
		};
		#endregion
	}
}