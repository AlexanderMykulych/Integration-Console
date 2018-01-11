using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Terrasoft.Core;
using Terrasoft.Core.Factories;

namespace Terrasoft.TsIntegration.Configuration
{
	public class IntegrationServiceWrapper
	{
		public IntegrationServiceWrapper(UserConnection userConnection)
		{
			_userConnection = userConnection;
		}
		public IntegrationServiceWrapper(UserConnection userConnection, IIntegrationObjectProvider iObjectProvider)
		{
			_userConnection = userConnection;
			_iObjectProvider = iObjectProvider;
		}
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
		//Log: key=Integration Service
		public virtual Stream Get(string routeKey, string id)
		{
			if (UserConnection == null)
			{
				return null;
			}
			var integrator = ObjectFactory.Get<IIntegrator>();
			IIntegrationObject integrObject = null;
			integrator.Export(new Guid(id), null, routeKey,
				(iObject, handlerConfig, handler, entity) =>
				{
					integrObject = iObject;
				});
			if (integrObject != null)
			{
				var stream = IObjectProvider.GetMemoryStream(integrObject);
				WebOperationContext.Current.OutgoingResponse.ContentType = IObjectProvider.GetContentType(integrObject);
				return stream;
			}
			return null;
		}
		//Log: key=Integration Service
		public virtual Stream Post(string routeKey, Stream requestStream)
		{
			return Import(routeKey, requestStream);
		}
		//Log: key=Integration Service
		public virtual Stream Put(string routeKey, Stream requestStream)
		{
			return Import(routeKey, requestStream);
		}
		//Log: key=Integration Service
		public virtual Stream PostEndPoint(string endPointName, Stream requestStream)
		{
			var settingProvider = ObjectFactory.Get<ISettingProvider>();
			var endPointConfig = settingProvider.SelectFirstByType<EndPointConfig>(x => x.Name == endPointName);
			if (endPointConfig != null)
			{
				var endPointHandler = new EndPointFactory().Get(endPointConfig.EndPointHandler);
				if (UserConnection == null)
				{
					return null;
				}
				string content = null;
				using (StreamReader reader = new StreamReader(requestStream))
				{
					content = reader.ReadToEnd();
				}
				var integrObject = IObjectProvider.Parse(content);
				var route = endPointHandler.GetImportRoute(integrObject);
				var integrator = ClassFactory.Get<BaseIntegrator>();
				Stream responseStream = null;
				integrator.Import(integrObject, route, integrationInfo =>
				{
					if (integrationInfo.IntegratedEntity != null)
					{
						var exportRoute = string.Format(endPointConfig.GetHandlerConfig("SuccesExportRouteFormat"), route);
						responseStream = Get(exportRoute, integrationInfo.IntegratedEntity.PrimaryColumnValue.ToString());
					}
				}, (info, exception) =>
				{
					responseStream = ProcessImportException(new ExceptionProcessData()
					{
						Exception = exception,
						IObject = integrObject,
						Info = info,
						Route = route,
						RequestContent = content
					}, endPointConfig);
				});
				WebOperationContext.Current.OutgoingResponse.ContentType = IObjectProvider.GetContentType(integrObject);
				return responseStream;
			}
			WebOperationContext.Current.OutgoingResponse.ContentType = "text/plain";
			return Error(string.Format("End Point {0} not found!", endPointName));
		}
		//Log: key=Integration Service
		protected Stream ProcessImportException(ExceptionProcessData processData, EndPointConfig endPointConfig)
		{
			var exceptionName = processData.Exception.GetType().Name;
			bool isCanProcessException = endPointConfig.GetHandlerConfig("ProcessException", "")
				.Split(';')
				.Any(x => x == exceptionName);
			if (isCanProcessException)
			{
				var strategyName = endPointConfig.GetHandlerConfig("ProcessStrategyHandler_" + exceptionName, "DefaultProcessStrategy");
				var strategy = new ExceptionProcessStrategyFactory().Get(strategyName);
				processData.Config = endPointConfig.GetHandlerConfig("ProcessStrategyHandlerConfig_" + exceptionName, "")
					.Split(';')
					.Where(x => !string.IsNullOrEmpty(x))
					.Select(x => x.Split('=').Where(y => !string.IsNullOrEmpty(y)).ToList())
					.Where(x => x.Count > 1)
					.ToDictionary(x => x[0], x => x[1]);
				return strategy.Process(processData);
			}
			return null;
		}
		//Log key=Integration Service
		protected virtual Stream Import(string routeKey, Stream requestStream)
		{
			if (UserConnection == null)
			{
				return null;
			}
			string content = null;
			using (StreamReader reader = new StreamReader(requestStream))
			{
				content = reader.ReadToEnd();
			}
			var integrObject = IObjectProvider.Parse(content);
			return Import(routeKey, integrObject);
		}
		//Log key=Integration Service
		protected virtual Stream Import(string routeKey, IIntegrationObject iObject)
		{
			var integrator = ClassFactory.Get<BaseIntegrator>();
			Guid resultId = Guid.Empty;
			integrator.Import(iObject, routeKey, integrationInfo =>
			{
				if (integrationInfo.IntegratedEntity != null)
				{
					resultId = integrationInfo.IntegratedEntity.PrimaryColumnValue;
				}
			});
			if (resultId != Guid.Empty)
			{
				return Get(routeKey, resultId.ToString());
			}
			WebOperationContext.Current.OutgoingResponse.ContentType = "text/plain";
			return Error("Результат не найдено");
		}
		//Log key=Integration Service
		protected virtual Stream Error(string message)
		{
			var stream = new MemoryStream();
			var encoding = new UTF8Encoding();
			var encodeData = encoding.GetBytes(message);
			stream.Write(encodeData, 0, encodeData.Length);
			stream.Seek(0, SeekOrigin.Begin);
			return stream;
		}
	}
}
