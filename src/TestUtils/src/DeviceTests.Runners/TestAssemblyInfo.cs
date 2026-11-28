using Xunit;

[assembly: Xunit.v3.Parallelization(Mode = Xunit.Sdk.ParallelMode.None)]
#if WINDOWS
[assembly: TestFramework(typeof(Microsoft.Maui.TestUtils.DeviceTests.Runners.WindowsUiTestFramework))]
#endif
