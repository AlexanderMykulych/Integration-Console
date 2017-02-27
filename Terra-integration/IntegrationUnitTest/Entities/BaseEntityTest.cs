using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terrasoft.Core.Entities;

namespace IntegrationUnitTest.Entities {
	public class BaseEntityTest {
		public abstract int ExternalId {
			get;
		}
		public abstract string JsonFileName {
			get;
		}
		public virtual string GetJson() {
			using(var fileStream = new StreamReader(string.Format("JsonFiles/{0}.json", JsonFileName))) {
				return fileStream.ReadToEnd();
			}
		}
		public virtual bool IsEntityHasValueInDb(Entity entity, string fieldName, object value) {
			try {
				return entity.GetColumnValue(fieldName) == value;
			} catch(Exception e) {
				Console.WriteLine(e.ToString());
				return false;
			}
		}
	}
}
