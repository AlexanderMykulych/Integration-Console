using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terrasoft.Core;

namespace Terrasoft.TsConfiguration
{
	public class PrimaryImportParam
	{
		public string ServiceObjName;
		public int ExternalId;
		public bool WithUpdateExist;
		public int BatchLimit;
		public UserConnection UserConnection;
		public bool CreateReminding;
		public int SkipCount;
		public string Filter;
		public PrimaryImportParam(string serviceObjName, UserConnection userConnection, bool withUpdateExist = false, int externalId = 0,
			int batchLimit = 10, bool createReminding = false, int skipCount = 0, string filter = null)
		{
			ServiceObjName = serviceObjName;
			ExternalId = externalId;
			WithUpdateExist = withUpdateExist;
			BatchLimit = batchLimit;
			UserConnection = userConnection;
			CreateReminding = createReminding;
			SkipCount = skipCount;
			Filter = filter;
		}
	}
}
