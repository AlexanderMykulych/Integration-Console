using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terrasoft.Core;

namespace QueryConsole.Files.IntegratorTester {
	public class TesterManager: IEnumerable {
		public UserConnection UserConnection;
		public List<BaseIntegratorTester> Testers;
		public List<Action> Actions = new List<Action>();
		public List<Tuple<string, int, int>> Configs;

		public TesterManager(UserConnection userConnection, params BaseIntegratorTester[] testers) {
			UserConnection = userConnection;
			Testers = testers.ToList();
			Configs = new List<Tuple<string, int, int>>();
		}

		public void Add(string Name, int limit, int skip, int count = -1) {
			if(count == -1) {
				Configs.Add(new Tuple<string, int, int>(Name, limit, skip));
			} else {
				for(var i = 0; i < count; i++) {
					Configs.Add(new Tuple<string, int, int>(Name, limit, skip + i*limit));
				}
			}
		}

		public void GenerateActions() {
			for(var i = Configs.Count - 1; i >= 0; i--) {
				var name = Configs[i].Item1;
				var limit = Configs[i].Item2;
				var skip = Configs[i].Item3;
				var j = i;
				if(i == Configs.Count - 1) {
					Actions.Add(() => {
						Console.Title = "Start Integrate: " + name;
						var tester = GetTesterByEntityName(name);
						tester.Limit = limit;
						tester.Skip = skip;
						tester.ExportServiceEntity(name);
						Console.Title = "End Integrate: " + name;
					});
				} else {
					Actions.Add(() => {
						Console.Title = "Start Integrate: " + name;
						var tester = GetTesterByEntityName(name);
						tester.Limit = limit;
						tester.Skip = skip;
						tester.ExportServiceEntity(name, Actions[Configs.Count - 2 - j]);
					});
				}
			}
		}

		public void Run() {
			GenerateActions();
			Actions.Last()();
		}

		public BaseIntegratorTester GetTesterByEntityName(string name) {
			foreach(var tester in Testers) {
				if(tester.InitServiceEntitiesName().Contains(name)) {
					return tester;
				}
			}
			return null;
		}
			//new OrderServiceIntegratorTester(consoleApp.SystemUserConnection);
			//new ClientServiceIntegratorTester(consoleApp.SystemUserConnection);

		public IEnumerator GetEnumerator() {
			throw new NotImplementedException();
		}
	}
}
