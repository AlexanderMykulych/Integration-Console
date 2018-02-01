#tool "nuget:?package=NUnit.ConsoleRunner"
using Cake.Email.Common;
var testDll = Argument("testDll", "../bin/Debug/IntegrationUnitTest.dll");
var outputFolder = Argument("outputFolder", "../bin/Debug/IntegrationUnitTest.dll");
var reportUnitExePath = Argument("reportUnitExe", @"c:\Dev\R&D\DynamicIntegration\Integration-Console\Terra-integration\packages\ReportUnit.1.2.1\tools\ReportUnit.exe");
var reportUnitInput = Argument("reportUnitInput", @"C:\Dev\R&D\DynamicIntegration\Integration-Console\Terra-integration\IntegrationUnitTest\AutoRunner\");

Task("RunTest")
	.Does(() => {
		var resultPath = string.Format("{0}/{1:yyyy-MM-dd_hh-mm-ss-tt}", outputFolder, DateTime.Now);
		NUnit3(testDll, new NUnit3Settings {
			Results = new List<NUnit3Result>() {
				new NUnit3Result() {
					OutputFile = resultPath
				}
			}
		}));
	});

Task("BuildTestDashboards")
	.IsDependentOn("RunTest")
	.Does(() => {
		StartProcess(reportUnitExePath, reportUnitInput);
	});


Task("Build")
	.IsDependentOn("BuildTestDashboards")
	.IsDependentOn("SendResultEmail")
	.Does(() => {});

RunTarget("Build");