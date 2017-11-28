using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terrasoft.Core;

namespace Terrasoft.Configuration.FileImport
{
	public class Test
	{
		public void Start(UserConnection userConnection)
		{
			var importer = new ServiceCallCaseImporter(new StorageEnumerator(), userConnection);
			importer.Import();
		}
	}

	public class StorageEnumerator: IStorageEnumerator
	{
		private List<IStorageRow> rows = new List<IStorageRow>();
		private int currentIndex;
		public StorageEnumerator()
		{
			rows.Add(new StorageRow(new Dictionary<string, string>()
			{
				{ "A", "Отправлено по e-mail" }
			}));
			currentIndex = 0;
		}
		public void Dispose()
		{
			rows.Clear();
		}

		public bool MoveNext()
		{
			if (rows.Count > currentIndex + 1)
			{
				currentIndex++;
				return true;
			}
			return false;
		}

		public void Reset()
		{
			currentIndex = 0;
		}

		public IStorageRow Current
		{
			get { return rows[currentIndex]; }
		}

		public void Load(byte[] bytes)
		{
			//TEST
		}

		object IEnumerator.Current
		{
			get { return Current; }
		}
	}
}
