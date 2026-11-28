#nullable enable
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Sdk;
using Xunit.v3;

namespace Microsoft.Maui.TestUtils.DeviceTests.Runners;

/// <summary>
/// Runs Windows device tests on the application's UI thread, including plain facts
/// which build another MauiApp and therefore initialize an application dispatcher.
/// </summary>
public class WindowsUiTestFramework : XunitTestFramework
{
	static Func<Func<Task>, Task>? s_dispatch;

	public static void ConfigureDispatcher(Func<Func<Task>, Task> dispatch) =>
		s_dispatch = dispatch ?? throw new ArgumentNullException(nameof(dispatch));

	internal static Task RunOnUiThread(Func<Task> action)
	{
		var dispatch = s_dispatch ?? throw new InvalidOperationException("The Windows test UI dispatcher was not initialized.");
		var context = ExecutionContext.Capture();
		return dispatch(() =>
		{
			if (context is null)
				return action();

			// A native UI thread predates MTP. Keep xUnit's ambient test context when
			// crossing the dispatcher without replacing the UI synchronization context.
			Task? execution = null;
			ExecutionContext.Run(context, _ => execution = action(), null);
			return execution!;
		});
	}

	protected override ITestFrameworkExecutor CreateExecutor(Assembly assembly) =>
		new UiExecutor(new XunitTestAssembly(assembly, configFilePath: null,
			assemblyName: null, assemblyPath: null, targetFramework: null, uniqueID: null, version: assembly.GetName().Version));

	sealed class UiExecutor(IXunitTestAssembly assembly) : XunitTestFrameworkExecutor(assembly)
	{
		public override ValueTask RunTestCases(IReadOnlyCollection<IXunitTestCase> testCases,
			IMessageSink executionMessageSink, ITestFrameworkExecutionOptions executionOptions,
			CancellationToken cancellationToken) =>
			new(RunOnUiThread(() => base.RunTestCases(testCases, executionMessageSink, executionOptions, cancellationToken).AsTask()));
	}
}
