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
	public class IntegratorHelper
	{

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="requestMethod">Get, Put, Post</param>
		/// <param name="url"></param>
		/// <param name="jsonText">Данные для отправки в формате json</param>
		/// <param name="callback">callback - для обработки ответа</param>
		/// <param name="userConnection"></param>
		public void PushRequest(TRequstMethod requestMethod, string url, string jsonText, Action<string, UserConnection> callback, UserConnection userConnection,
			 Action<string, UserConnection> errorCallback = null, string auth = null)
		{
			if (string.IsNullOrEmpty(url))
			{
				return;
			}

			LoggerHelper.DoInLogBlock("PushRequest", () =>
			{
				IntegrationLogger.Info(RequestLoggerInfo.GetMessage(requestMethod, url, auth, jsonText));
				MakeAsyncRequest(requestMethod, url, jsonText, callback, userConnection, errorCallback, auth);
			});
		}


		/// <summary>
		/// Делает асинхронный запрос
		/// </summary>
		/// <param name="requestMethod"></param>
		/// <param name="url"></param>
		/// <param name="jsonText"></param>
		/// <param name="callback"></param>
		/// <param name="userConnection"></param>
		private static void MakeAsyncRequest(TRequstMethod requestMethod, string url, string jsonText, Action<string, UserConnection> callback,
			 UserConnection userConnection = null,
			 Action<string, UserConnection> errorCallback = null, string auth = null)
		{
			try
			{
				var _request = WebRequest.Create(new Uri(url)) as HttpWebRequest;
				_request.Method = requestMethod.ToString();
				_request.ContentType = "application/json";
				_request.Headers.Add("authorization", string.IsNullOrEmpty(auth) ? "Basic YnBtb25saW5lOmJwbW9ubGluZQ==" : auth);
				_request.Headers.Add("cache-control", "no-cache");
				switch (requestMethod)
				{
					case TRequstMethod.POST:
					case TRequstMethod.PUT:
						if (string.IsNullOrEmpty(jsonText))
							return;
						jsonText = jsonText.Replace("ReferenceClientService", "#ref");
						AddDataToRequest(_request, jsonText);
						break;
				}
				try
				{
					var response = _request.GetResponse();
					using (Stream responseStream = response.GetResponseStream())
					using (StreamReader sr = new StreamReader(responseStream))
					{
						if (callback != null)
						{
							string responceText = sr.ReadToEnd();
							IntegrationLogger.Info(ResponseLoggerInfo.GetMessage(responceText));
							callback(responceText, userConnection);
						}
					}
				}
				catch (WebException e)
				{
					WebResponse response = e.Response;
					using (StreamReader sr = new StreamReader(response.GetResponseStream()))
					{
						string responceText = sr.ReadToEnd();
						IntegrationLogger.Error(RequestErrorLoggerInfo.GetMessage(e, responceText));
						if (errorCallback != null)
						{
							errorCallback(responceText, userConnection);
						}
					}
				}
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(RequestErrorLoggerInfo.GetMessage(e, "Ошибка при формировании запроса"));
				if (errorCallback != null)
				{
					errorCallback(e.Message, userConnection);
				}
			}
		}
		/// <summary>
		/// Добавляет данные к запросу
		/// </summary>
		/// <param name="request"></param>
		/// <param name="data"></param>
		private static void AddDataToRequest(HttpWebRequest request, string data)
		{
			if (string.IsNullOrEmpty(data))
				return;
			var encoding = new UTF8Encoding();
			data = data.Replace("ReferenceClientService", "#ref");
			var bytes = Encoding.UTF8.GetBytes(data);
			request.ContentLength = bytes.Length;

			using (var writeStream = request.GetRequestStream())
			{
				writeStream.Write(bytes, 0, bytes.Length);
			}
		}

		private class ResponceParams
		{
			public ResponceParams(HttpWebRequest request, Action<string, UserConnection> callback, UserConnection userConnection, string jsonData)
			{
				Request = request;
				Callback = callback;
				UserConnection = userConnection;
				JsonData = jsonData;
			}
			public HttpWebRequest Request;
			public Action<string, UserConnection> Callback;
			public UserConnection UserConnection;
			public string JsonData;
		}

	}
}