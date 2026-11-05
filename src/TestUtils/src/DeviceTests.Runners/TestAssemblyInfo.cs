using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]
#if WINDOWS
[assembly: TestFramework(typeof(Microsoft.Maui.TestUtils.DeviceTests.Runners.WindowsUiTestFramework))]
#endif
