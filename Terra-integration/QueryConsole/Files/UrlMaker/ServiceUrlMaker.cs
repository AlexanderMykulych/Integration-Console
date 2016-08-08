using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terrasoft.TsConfiguration;

namespace QueryConsole.Files
{
	public class ServiceUrlMaker
	{
		public Dictionary<TServiceObject, string> baseUrls;
		public ServiceUrlMaker(Dictionary<TServiceObject, string> baseUrls)
		{
			this.baseUrls = baseUrls;
		}
		public virtual string Make(TServiceObject type, string objectName, string objectId, string filters, TRequstMethod method, string limit, string skip)
		{
			return MakeUrl(baseUrls[type], objectName, objectId, filters, method, limit, skip);
		}
		public virtual string Make(ServiceRequestInfo info)
		{
			return Make(info.Type, info.ServiceObjectName, info.ServiceObjectId, info.Filters, info.Method, info.Limit, info.Skip); ;
		}

		public static string MakeUrl(string baseUrl, string objectName, string objectId, string filters, TRequstMethod method, string limit, string skip) {
			string resultUrl = baseUrl;
			resultUrl += "/" + objectName;
			if (!string.IsNullOrEmpty(objectId) && objectId != "0") {
				resultUrl += "/" + objectId;
				return resultUrl;
			}
			//resultUrl += "?sort[createdAt]=desc";
			if (!string.IsNullOrEmpty(limit)) {
				resultUrl += (resultUrl.IndexOf("?") > -1 ? "&" : "?") + "limit=" + limit;
			}
			if (!string.IsNullOrEmpty(skip) && int.Parse(skip) > 0) {
				resultUrl += (resultUrl.IndexOf("?") > -1 ? "&" : "?") + "skip=" + skip;
			}
			if (!string.IsNullOrEmpty(filters)) {
				resultUrl += "?" + filters;
			}
			return resultUrl;
		}

		public static string MakeUrl(string baseUrl, ServiceRequestInfo info) {
			return MakeUrl(baseUrl, info.ServiceObjectName, info.ServiceObjectId, info.Filters, info.Method, info.Limit, info.Skip);
		}
	}
}
