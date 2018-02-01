#tool "nuget:?package=NUnit.ConsoleRunner"
var testDll = Argument("testDll", "../bin/Debug/IntegrationUnitTest.dll");
var outputFolder = Argument("outputFolder", "../bin/Debug/IntegrationUnitTest.dll");
var reportUnitExePath = Argument("reportUnitExe", @"c:\Dev\R&D\DynamicIntegration\Integration-Console\Terra-integration\packages\ReportUnit.1.2.1\tools\ReportUnit.exe");

string resultPath = "";

Task("RunTest")
	.Does(() => {
		resultPath = string.Format("{0}/{1:yyyy-MM-dd_hh-mm-ss-tt}.xml", outputFolder, DateTime.Now);
		try{
			NUnit3(testDll, new NUnit3Settings {
				Results  = new List<NUnit3Result>() {
					new NUnit3Result() {
						OutputFile = new FilePath(resultPath)
					}
				}
			});
		} catch(Exception e) {
			Console.WriteLine(e);
		}
	});

Task("BuildTestDashboards")
	.IsDependentOn("RunTest")
	.Does(() => {
		Console.WriteLine(resultPath);
		StartProcess(reportUnitExePath, resultPath);
	});


Task("Build")
	.IsDependentOn("BuildTestDashboards")
	.Does(() => {});

RunTarget("Build");