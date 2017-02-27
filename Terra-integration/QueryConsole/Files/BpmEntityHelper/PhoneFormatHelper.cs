using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Terrasoft.TsConfiguration
{
	public class PhoneFormatHelper
	{
		public static List<Func<string, string>> Formaters = new List<Func<string, string>>() {
			FormaterRemoveFirtPlusSeven
		};
		public static List<string> ToAllFormats(string startPhone)
		{
			var phones = new List<string>()
			{
				startPhone
			};
			Formaters.ForEach(formater => phones.Add(formater(startPhone)));
			return phones;
		}
		public static string FormaterRemoveFirtPlusSeven(string phone)
		{
			string strToken = "+7";
			if(phone != null)
			{
				phone = phone.Trim();
				if(phone.StartsWith(strToken))
				{
					phone = phone.Replace(strToken, "8");
				}
				phone = Regex.Replace(phone, "[^0-9]+", string.Empty);
			}
			return phone;
		}
	}
}
