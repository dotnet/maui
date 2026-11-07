using System.Reflection;
using Microsoft.Maui.TestUtils.DeviceTests.Runners;
using Microsoft.Maui.TestUtils.DeviceTests.Runners.HeadlessRunner;

// Exercise the native entry point in both MTP's controller and its re-executed test host.
return await new HeadlessTestRunner(
	new HeadlessRunnerOptions(),
	new TestOptions { TestAssembly = Assembly.GetExecutingAssembly() })
	.RunWithControllerAsync(Path.Combine(Environment.CurrentDirectory, "TestResults"), args);
