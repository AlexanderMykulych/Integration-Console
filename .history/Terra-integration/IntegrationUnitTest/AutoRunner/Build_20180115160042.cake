#tool "nuget:?package=NUnit.Runners&version=2.6.4"

Task("Run Test")
	.Does(() => {
		NUnit("c:\\Dev\R&D\\DynamicIntegration\\Integration-Console\\Terra-integration\\IntegrationUnitTest\\bin\\Release\\IntegrationUnitTest.dll", new NUnitSettings {
			Timeout = 60000,
			StopOnError = false
		});
	});

RunTarget("Run Test");