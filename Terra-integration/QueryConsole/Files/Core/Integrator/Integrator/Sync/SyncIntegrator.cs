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
	[Override]
	public class SyncIntegrator : BaseIntegrator
	{
		public SyncIntegrator(IEntityPreparer entityPreparer, IIntegrationObjectWorker iObjectWorker,
			IServiceHandlerWorkers serviceHandlerWorker, IServiceRequestWorker serviceRequestWorker,
			ISyncExportChecker<Guid> syncExportChecker)
			: base(entityPreparer, iObjectWorker, serviceHandlerWorker, serviceRequestWorker)
		{
			SyncExportChecker = syncExportChecker;
		}

		public virtual ISyncExportChecker<Guid> SyncExportChecker { get; set; }
		//Log key=Integration Sync
		public override void Export(Guid id, string schemaName, string routeKey = null, Action<IIntegrationObject, ConfigSetting, BaseEntityHandler, Entity> OnGet = null)
		{
			string exportRouteKey = routeKey ?? schemaName;
			if (SyncExportChecker.IsSyncEnable(exportRouteKey))
			{
				SyncExportChecker.DoInSync(exportRouteKey, id, () => {
					base.Export(id, schemaName, routeKey, OnGet);
				});
			}
			else
			{
				base.Export(id, schemaName, routeKey, OnGet);
			}
		}

		
	}
}