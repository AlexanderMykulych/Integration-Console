using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationUnitTest.IEntities {
	public interface IEntityTest {
		Dictionary<string, object> GetDbFieldsValues();
	}
}
