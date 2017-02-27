using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terrasoft.Core;
using Terrasoft.Core.DB;

namespace Terrasoft.TsConfiguration
{
	public class PrimaryExportScenario
	{
		private readonly bool _exportOnlyNew;
		private int _rateCount;
		private PrimaryExportProvider _provider;

		public PrimaryExportScenario(UserConnection userConnection, bool exportOnlyNew, int rateCount)
		{
			_exportOnlyNew = exportOnlyNew;
			_rateCount = rateCount;
			_provider = new PrimaryExportProvider(userConnection);
		}

		public void Run()
		{
			try
			{
				_provider
					.Run(new PrimaryExportParam("TsLocSalMarket", _exportOnlyNew, null, "TsExternalId"))
					.Run(new PrimaryExportParam("SysAdminUnit", _exportOnlyNew, null, "TsExternalId",
						select => (select.HasCondition ? select.And("EntitySrc", "SysAdminUnitTypeValue") : select.Where("EntitySrc", "SysAdminUnitTypeValue")).IsLess(Column.Const(4)), _rateCount))
					.Run(new PrimaryExportParam("Contact", _exportOnlyNew, new ContactHandler(), "TsExternalId",
						select => select.InnerJoin("SysAdminUnit").As("sauUser").On("sauUser", "ContactId").IsEqual("EntitySrc", "Id"), _rateCount))
					.Run(new PrimaryExportParam("SysAdminUnit", _exportOnlyNew, null, "TsExternalId",
						select => (select.HasCondition ? select.And("EntitySrc", "SysAdminUnitTypeValue") : select.Where("EntitySrc", "SysAdminUnitTypeValue")).IsEqual(Column.Const(4)), _rateCount))
					.Run(new PrimaryExportParam("Account", _exportOnlyNew, new AccountHandler(), "TsExternalId", null, _rateCount))
					.Run(new PrimaryExportParam("Contact", _exportOnlyNew, new ContactHandler(), "TsExternalId", null, _rateCount))
					.Run(new PrimaryExportParam("SysAdminUnit", _exportOnlyNew, null, "TsExternalId",
						select => (select.HasCondition ? select.And("EntitySrc", "SysAdminUnitTypeValue") : select.Where("EntitySrc", "SysAdminUnitTypeValue")).IsEqual(Column.Const(4)), _rateCount))
					.Run(new PrimaryExportParam("Account", _exportOnlyNew, new AccountHandler(), "TsExternalId",
						select => (select.HasCondition ? select.And("EntitySrc", "PrimaryContactId") : select.Where("EntitySrc", "PrimaryContactId")).Not().IsNull(), _rateCount))
					.Run(new PrimaryExportParam("Case", _exportOnlyNew, null, "TsExternalId", null, _rateCount))
					.Run(new PrimaryExportParam("Account", _exportOnlyNew, new CounteragentHandler(), "TsOrderServiceId",
						select =>
						{
							select
								.LeftOuterJoin("SysAdminUnit").As("cPrimeUser")
								.On("cPrimeUser", "ContactId").IsEqual("EntitySrc", "PrimaryContactId")
								.LeftOuterJoin("Contact").As("c")
								.On("c", "AccountId").IsEqual("EntitySrc", "Id")
								.LeftOuterJoin("SysAdminUnit").As("cUser")
								.On("cUser", "ContactId").IsEqual("c", "Id");
							select.Where();
							(select.HasCondition ? select.And() : select.Where())
								.OpenBlock("cPrimeUser", "Id").Not().IsNull()
								.Or("cUser", "Id").Not().IsNull()
								.CloseBlock();
						}, _rateCount))
					.Run(new PrimaryExportParam("Contact", _exportOnlyNew, new ManagerInfoHandler(), "TsManagerInfoId",
						select =>
						{
							select
								.LeftOuterJoin("SysAdminUnit").As("cUser")
								.On("cUser", "ContactId").IsEqual("EntitySrc", "Id");
							select.Where();
							(select.HasCondition ? select.And("cUser", "Id") : select.Where("cUser", "Id")).Not().IsNull();
						}, _rateCount));
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e);
			}
		}
	}
}
