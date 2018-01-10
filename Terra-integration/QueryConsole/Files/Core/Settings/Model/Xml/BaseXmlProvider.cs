using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Terrasoft.TsIntegration.Configuration
{
	public class BaseXmlProvider : IXmlProvider
	{
		protected virtual string _configRegex {
			get { return @"^.*\n*<MapingConfiguration>(?<config>[\s\S]+)<\/MapingConfiguration>"; }
		}

		protected virtual string _configTemplate {
			get { return "<?xml version=\"1.0\"?>\r\n<MapingConfiguration>\n{0}\n</MapingConfiguration>"; }
		}
		public string MergeXmls(List<string> xmls)
		{
			var regex = new Regex(_configRegex);
			var config = xmls.Select(x =>
				{
					var match = regex.Match(x);
					if (match.Success)
					{
						return match.Groups["config"].Value;
					}
					return null;
				})
				.Where(x => x != null)
				.Aggregate((x, y) => x + "\n" + y);

			return string.Format(_configTemplate, config);
		}
	}
}
