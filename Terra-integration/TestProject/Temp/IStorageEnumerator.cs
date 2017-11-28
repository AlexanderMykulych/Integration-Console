using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Terrasoft.Configuration.FileImport
{
	public interface IStorageEnumerator: IEnumerator<IStorageRow>
	{
		void Load(byte[] bytes);
	}
}
