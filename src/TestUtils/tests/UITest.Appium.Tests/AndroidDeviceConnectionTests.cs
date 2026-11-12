using System.Reflection;
using System.Xml.Linq;
using NUnit.Framework.Api;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Builders;
using UITest.Appium.NUnit;
using UITest.Core;
using Xunit;

[assembly: global::NUnit.Framework.NonTestAssembly]

namespace UITest.Appium.Tests;

public class AndroidDeviceConnectionTests
{
	[Fact]
	public void ReadyDeviceDoesNotReconnect()
	{
		var commands = new List<string[]>();
		var connection = new AndroidDeviceConnection(args =>
		{
			commands.Add(args);
			return "1\n";
		}, _ => throw new InvalidOperationException("Ready devices must not wait."), _ => { });

		connection.EnsureReady("emulator-5554");

		Assert.Equal(new[] { "-s", "emulator-5554", "shell", "getprop", "sys.boot_completed" }, Assert.Single(commands));
	}

	[Fact]
	public void DisconnectedDeviceCanRecover()
	{
		var probes = 0;
		var reconnects = 0;
		var waits = new List<TimeSpan>();
		var connection = new AndroidDeviceConnection(args =>
		{
			Assert.Equal("-s", args[0]);
			Assert.Equal("emulator-5554", args[1]);
			if (args[^1] == "reconnect")
			{
				reconnects++;
				return "";
			}
			if (++probes < 3)
				throw new IOException("device offline");
			return "1";
		}, waits.Add, _ => { });

		connection.EnsureReady("emulator-5554");
		connection.EnsureReady("emulator-5554");

		Assert.Equal(1, reconnects);
		Assert.Equal(4, probes);
		Assert.Equal(new[] { TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5) }, waits);
	}

	[Theory]
	[InlineData("offline")]
	[InlineData("unauthorized")]
	[InlineData("missing")]
	[InlineData("timeout")]
	[InlineData("booting")]
	public void UnavailableDeviceStopsAfterOneBoundedRecovery(string failure)
	{
		var commands = 0;
		var waits = 0;
		var messages = new List<string>();
		var connection = new AndroidDeviceConnection(_ =>
		{
			commands++;
			if (failure == "booting")
				return "0";
			if (failure == "timeout")
				throw new TimeoutException("ADB hung");
			throw new IOException(failure);
		}, _ => waits++, messages.Add);

		var error = Assert.Throws<AndroidDeviceUnavailableException>(() => connection.EnsureReady("emulator-5554"));
		var repeatedError = Assert.Throws<AndroidDeviceUnavailableException>(() => connection.EnsureReady("emulator-5554"));

		Assert.Same(error, repeatedError);
		Assert.Equal(5, commands);
		Assert.Equal(3, waits);
		Assert.Contains("emulator-5554", error.Message, StringComparison.Ordinal);
		Assert.Contains("Stopping UI test run", error.Message, StringComparison.Ordinal);
		Assert.Contains(error.Message, messages);
	}

	[Fact]
	public void RecoveryBudgetIsNotResetByAnAppiumRetry()
	{
		var ready = false;
		var reconnects = 0;
		var connection = new AndroidDeviceConnection(args =>
		{
			if (args[^1] == "reconnect")
			{
				reconnects++;
				ready = true;
			}
			return ready ? "1" : "0";
		}, _ => { }, _ => { });
		connection.EnsureReady(null);
		ready = false;

		Assert.Throws<AndroidDeviceUnavailableException>(() => connection.EnsureReady(null));
		Assert.Equal(1, reconnects);
	}

	[Fact]
	public void DeviceFailureRequestsGracefulNUnitStop()
	{
		var previous = TestExecutionContext.CurrentContext;
		var parent = new TestExecutionContext();
		var current = new TestExecutionContext(parent);
		current.EstablishExecutionEnvironment();
		try
		{
			var context = new TestContext();
			Assert.Throws<AndroidDeviceUnavailableException>(() => context.InitialSetup(new UnavailableServerContext()));
			Assert.Equal(TestExecutionStatus.StopRequested, parent.ExecutionStatus);
			Assert.Null(UITestContextBase.UITestContext);
		}
		finally
		{
			previous.EstablishExecutionEnvironment();
		}
	}

	[Fact]
	public void OrdinarySetupFailureDoesNotStopNUnit()
	{
		var previous = TestExecutionContext.CurrentContext;
		var current = new TestExecutionContext();
		current.EstablishExecutionEnvironment();
		try
		{
			Assert.Throws<InvalidOperationException>(() => new TestContext().InitialSetup(new UnavailableServerContext(false)));
			Assert.Equal(TestExecutionStatus.Running, current.ExecutionStatus);
		}
		finally
		{
			previous.EstablishExecutionEnvironment();
		}
	}

	[Fact]
	public void NUnitStopsBeforeNextFixtureAndProducesCancelledResults()
	{
		DeviceUnavailableFixture.TornDown = false;
		FollowingFixture.Started = false;
		var runner = new NUnitTestAssemblyRunner(new FixtureAssemblyBuilder());
		runner.Load(typeof(AndroidDeviceConnectionTests).Assembly, new Dictionary<string, object>
		{
			["NumberOfTestWorkers"] = 0,
		});
		runner.RunAsync(TestListener.NULL, TestFilter.Empty);

		Assert.True(runner.WaitForCompletion(5000), "NUnit must finish without waiting for later fixtures.");
		Assert.True(DeviceUnavailableFixture.TornDown);
		Assert.False(FollowingFixture.Started);
		Assert.Equal(TestStatus.Failed, runner.Result.ResultState.Status);
		var report = XDocument.Parse(runner.Result.ToXml(true).OuterXml);
		Assert.True(UITestRetry.IsCancelled(new[] { report }));

		var runReport = new XDocument(new XElement("test-run", new XAttribute("result", "Failed"), report.Root));
		XNamespace ns = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010";
		var trx = new XDocument(new XElement(ns + "TestRun",
			new XElement(ns + "ResultSummary", new XAttribute("outcome", "Passed"))));
		Assert.True(UITestRetry.ApplyNUnitFailures(trx, new[] { runReport }));
		Assert.Equal("Failed", (string)trx.Root.Element(ns + "ResultSummary").Attribute("outcome"));
	}

	[Theory]
	[InlineData("<test-run result='Failed' label='Cancelled' />", true)]
	[InlineData("<test-run result='Failed'><test-suite result='Failed' label='Cancelled' /></test-run>", true)]
	[InlineData("<test-run result='Failed'><test-case result='Failed' /></test-run>", false)]
	[InlineData("<test-run result='Passed' />", false)]
	public void CancelledRunsAreNotEligibleForBatchRetry(string xml, bool cancelled)
	{
		Assert.Equal(cancelled, UITestRetry.IsCancelled(new[] { XDocument.Parse(xml) }));
	}

	sealed class TestContext() : UITestContextBase(TestDevice.Android)
	{
		public override IConfig GetTestConfig() => new Config();
		public override void Close() => throw new NotSupportedException();
		public override void LaunchAppWithTest() => throw new NotSupportedException();
	}

	sealed class UnavailableServerContext(bool deviceUnavailable = true) : IServerContext
	{
		public IUIClientContext CreateUIClientContext(IConfig config) =>
			throw (deviceUnavailable
				? new AndroidDeviceUnavailableException("Android device is unavailable after bounded ADB recovery.")
				: new InvalidOperationException("Unrelated setup failure."));

		public void Dispose()
		{
		}
	}

	sealed class FixtureAssemblyBuilder : ITestAssemblyBuilder
	{
		public ITest Build(Assembly assembly, IDictionary<string, object> options)
		{
			var suite = new TestAssembly(assembly, assembly.FullName);
			var builder = new DefaultSuiteBuilder();
			suite.Add(builder.BuildFrom(new TypeWrapper(typeof(DeviceUnavailableFixture))));
			suite.Add(builder.BuildFrom(new TypeWrapper(typeof(FollowingFixture))));
			return suite;
		}

		public ITest Build(string assemblyName, IDictionary<string, object> options) =>
			throw new NotSupportedException();
	}

	public sealed class DeviceUnavailableFixture
	{
		public static bool TornDown;

		[global::NUnit.Framework.OneTimeSetUp]
		public void Setup() => new TestContext().InitialSetup(new UnavailableServerContext());

		[global::NUnit.Framework.Test]
		public void MustNotRun() => throw new InvalidOperationException("Device is unavailable.");

		[global::NUnit.Framework.OneTimeTearDown]
		public void TearDown() => TornDown = true;
	}

	public sealed class FollowingFixture
	{
		public static bool Started;

		[global::NUnit.Framework.OneTimeSetUp]
		public void Setup() => Started = true;

		[global::NUnit.Framework.Test]
		public void MustNotRun() => throw new InvalidOperationException("Run should already have stopped.");
	}
}
