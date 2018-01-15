#tool "nuget:?package=NUnit.ConsoleRunner"

Task("RunTest")
	.Does(() => {
		NUnit3("../bin/Release/IntegrationUnitTest.dll");
	});

Task("BuildTestDashboards")
	.IsDependentOn("RunTest")
	.Does(() => {
		
	});

Task("Build")
	.IsDependentOn("BuildTestDashboards")
	.Does(() => {});

RunTarget("Build");