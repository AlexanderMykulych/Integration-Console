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
	[IntegrationService("BaseService")]
	public class BaseIntegrationService : IIntegrationService
	{
		//Log key=Integration Service
		public virtual WebRequest Create(ServiceConfig config, string content)
		{
			try
			{
				var request = WebRequest.Create(new Uri(config.Url)) as HttpWebRequest;
				request.Method = config.Method;
				if (config.Headers != null)
				{
					config.Headers.ForEach(header =>
					{
						request.SetHeaderValue(header.Key, header.Value);
					});
				}
				switch (config.Method)
				{
					case "POST":
					case "PUT":
						if (!string.IsNullOrEmpty(content))
						{
							AddDataToRequest(request, content);
						}
						break;
				}
				return request;
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(string.Format("Create Request Error\nConfig: {1}\nContent:{2}\nError: {0}", e, config, content));
			}
			return null;
		}
		//Log key=Integration Service
		protected virtual void AddDataToRequest(HttpWebRequest request, string data)
		{
			if (string.IsNullOrEmpty(data))
				return;
			IntegrationLogger.Info("Data:" + data);
			var encoding = new UTF8Encoding();
			var bytes = Encoding.UTF8.GetBytes(data);
			request.ContentLength = bytes.Length;
			using (var writeStream = request.GetRequestStream())
			{
				writeStream.Write(bytes, 0, bytes.Length);
			}
		}
		//Log key=Integration Service
		public virtual void SendRequest(WebRequest request, Action<WebResponse> OnResponse, Action<WebException> OnException)
		{
			try
			{
				WebResponse response = null;
				LoggerHelper.DoInLogBlock("Send Request", () =>
				{
					try
					{
						response = request.GetResponse();
					}
					catch (WebException e)
					{
						if (OnException != null)
						{
							OnException(e);
						}
					}
				});
				if (OnResponse != null)
				{
					OnResponse(response);
				}
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e);
			}
		}
		//Log key=Integration Service
		public virtual string GetContentFromResponse(WebResponse response)
		{
			using (StreamReader sr = new StreamReader(response.GetResponseStream()))
			{
				return sr.ReadToEnd();
			}
		}
	}
}