#tool "nuget:?package=NUnit.ConsoleRunner"
using Cake.Email.Common;

Task("RunTest")
	.Does(() => {
		NUnit3("../bin/Debug/IntegrationUnitTest.dll");
	});

Task("BuildTestDashboards")
	.IsDependentOn("RunTest")
	.Does(() => {
		StartProcess(@"c:\Dev\R&D\DynamicIntegration\Integration-Console\Terra-integration\packages\ReportUnit.1.2.1\tools\ReportUnit.exe",
			@"C:\Dev\R&D\DynamicIntegration\Integration-Console\Terra-integration\IntegrationUnitTest\AutoRunner\");
	});
	
Task("SendResultEmail")
	.Does(() => {
		var path = @"C:\Dev\R&D\DynamicIntegration\Integration-Console\Terra-integration\IntegrationUnitTest\AutoRunner\";

	});

Task("Build")
	.IsDependentOn("BuildTestDashboards")
	.IsDependentOn("SendResultEmail")
	.Does(() => {});

RunTarget("Build");