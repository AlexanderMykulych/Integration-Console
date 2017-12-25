using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Terrasoft.Core;
using Terrasoft.Core.Factories;

namespace Terrasoft.TsIntegration.Configuration
{
	public class TsiImportStarter
	{
		private UserConnection _userConnection;

		public TsiImportStarter(UserConnection userConnection)
		{
			_userConnection = userConnection;
		}
		public void StartImport(ManyTriggerServiceInfo info)
		{
			LoggerHelper.DoInLogBlock("Mass Run Trigger " + info.TriggerName, () =>
			{
				try
				{
					var triggerEngine = new TriggerEngine();
					var triggerInfo = triggerEngine.GetTriggerByName(info.TriggerName, _userConnection);
					if (triggerInfo != null)
					{
						var integrator = ClassFactory.Get<BaseIntegrator>();
						foreach (var primaryColumn in info.PrimaryColumnValue)
						{
							try
							{
								integrator.ExportWithRequest(_userConnection, primaryColumn, info.SchemaName, triggerInfo.Route);
							}
							catch (Exception e)
							{

								IntegrationLogger.ErrorFormat("{0}: {1", primaryColumn, e);
							}
						}
					}
					else
					{
						IntegrationLogger.Error(string.Format("Trigger {0} not found!", info.TriggerName));
					}
				}
				catch (Exception e)
				{
					IntegrationLogger.Error(e);
				}
			});
		}
	}

	public class ManyTriggerServiceInfo
	{
		public string TriggerName;
		public string SchemaName;
		public List<Guid> PrimaryColumnValue;
	}
}
