#tool "nuget:?package=NUnit.ConsoleRunner"
using Cake.Email.Common;
var testDll = Argument("testDll", "../bin/Debug/IntegrationUnitTest.dll");
var outputFolder = Argument("testDll", "../bin/Debug/IntegrationUnitTest.dll");
var reportUnitExePath = Argument("reportUnitExe", @"c:\Dev\R&D\DynamicIntegration\Integration-Console\Terra-integration\packages\ReportUnit.1.2.1\tools\ReportUnit.exe");
var reportUnitInput = Argument("reportUnitInput", @"C:\Dev\R&D\DynamicIntegration\Integration-Console\Terra-integration\IntegrationUnitTest\AutoRunner\");

Task("RunTest")
	.Does(() => {
		NUnit3(testDll);
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