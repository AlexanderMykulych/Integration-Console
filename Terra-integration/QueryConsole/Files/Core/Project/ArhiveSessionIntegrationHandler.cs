using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using Terrasoft.Core;
using Terrasoft.Core.DB;

namespace Terrasoft.TsIntegration.Configuration
{
	[IntegrationHandlerAttribute("ArhiveSessionIntegrationHandler")]
	public class ArhiveSessionIntegrationHandler: DefaultEntityHandler
	{
		protected virtual string IdsPath {
			get {
				return "GetCArchiveSessionOUT/item/id";
			}
		}

		protected virtual string DeleteEntityName
		{
			get { return "TsiArchiveSession"; }
		}

		protected virtual string EntityDeleteIdPath
		{
			get { return "TsiIdentifier"; }
		}

		protected virtual string DeleteLogCaption
		{
			get { return "Delete Archive Session"; }
		}

		protected virtual string DeleteEntityPrimaryColumnName {
			get { return "TsiPersAccProductId"; }
		}
		protected virtual string MainEntityPrimaryColumnName {
			get { return "Id"; }
		}
		public ArhiveSessionIntegrationHandler(ConfigSetting handlerConfig) : base(handlerConfig)
		{
		}

		protected override void BeforeMapping(CsConstant.IntegrationInfo integrationInfo)
		{
			base.BeforeMapping(integrationInfo);
			if (CheckIntegrationInfoForProcessResponse(integrationInfo))
			{
				DeleteOldSession(integrationInfo);
			}
		}

		private void DeleteOldSession(CsConstant.IntegrationInfo integrationInfo)
		{
			List<string> sessionIds = GetSessionIds(integrationInfo.Data);
			LoggerHelper.DoInLogBlock(DeleteLogCaption, () =>
			{
				DeleteArchiveSessionByNotThisIds(sessionIds);
			});
		}

		private void DeleteArchiveSessionByNotThisIds(List<string> sessionIds)
		{
			var delete = new Delete(ConnectionProvider.Get<UserConnection>())
				.From(DeleteEntityName)
				.Where(DeleteEntityPrimaryColumnName).IsEqual(Column.Parameter(MainEntityPrimaryColumnName)) as Delete;
			if (sessionIds.Any())
			{
				delete.And(EntityDeleteIdPath).Not().In(sessionIds.Select(x => Column.Parameter(x)).ToArray());
			}
			delete.Execute();
		}

		private List<string> GetSessionIds(IIntegrationObject data)
		{
			var xElement = data.GetObject() as XElement;
			if (xElement != null)
			{
				return xElement.XPathSelectElements(IdsPath).Select(x => x.Value).ToList();
			}
			return new List<string>();
		}
	}
}
