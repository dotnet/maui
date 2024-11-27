using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Storage;
using Xunit;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]

namespace Microsoft.Maui.TestUtils.DeviceTests.Runners.VisualRunner
{
	public class DeviceRunner : ITestListener, ITestRunner
	{
		readonly SynchronizationContext context = SynchronizationContext.Current;
		readonly AsyncLock executionLock = new AsyncLock();
		readonly ITestNavigation _navigation;
		readonly TestRunLogger _logger;
		volatile bool cancelled;

		public DeviceRunner(IReadOnlyCollection<Assembly> testAssemblies, ITestNavigation navigation, ILogger logger)
		{
			TestAssemblies = testAssemblies;
			_navigation = navigation;
			_logger = new TestRunLogger(logger);
		}

		public IReadOnlyCollection<Assembly> TestAssemblies { get; }

		public void RecordResult(TestResultViewModel result)
		{
			_logger.LogTestResult(result);
		}

		public Task RunAsync(TestCaseViewModel test)
		{
			return RunAsync(new[] { test });
		}

		public Task RunAsync(IEnumerable<TestCaseViewModel> tests, string message = null)
		{
			var groups = tests
				.GroupBy(t => t.AssemblyFileName)
				.Select(g => new AssemblyRunInfo(
					g.Key,
					GetConfiguration(Path.GetFileNameWithoutExtension(g.Key)),
					g.ToList()))
				.ToList();

			return RunAsync(groups, message);
		}

		public async Task RunAsync(IReadOnlyList<AssemblyRunInfo> runInfos, string message = null)
		{
			using (await executionLock.LockAsync())
			{
				if (message == null)
				{
					message = runInfos.Count > 1 || runInfos.FirstOrDefault()?.TestCases.Count > 1
						? "Run Multiple Tests"
						: runInfos.FirstOrDefault()?.TestCases.FirstOrDefault()?.DisplayName;
				}

				_logger.LogTestStart(message);

				try
				{
					await RunTests(() => runInfos);
				}
				finally
				{
					_logger.LogTestComplete();
				}
			}
		}

		public event Action<string> OnDiagnosticMessage;

		public Task<IReadOnlyList<TestAssemblyViewModel>> DiscoverAsync()
		{
			var tcs = new TaskCompletionSource<IReadOnlyList<TestAssemblyViewModel>>();

			RunAsync(() =>
			{
				try
				{
					var runInfos = DiscoverTestsInAssemblies();
					var list = runInfos.Select(ri => new TestAssemblyViewModel(ri, _navigation, this)).ToList();

					tcs.SetResult(list);
				}
				catch (Exception e)
				{
					tcs.SetException(e);
				}
			});

			return tcs.Task;
		}

		IEnumerable<AssemblyRunInfo> DiscoverTestsInAssemblies()
		{
			var result = new List<AssemblyRunInfo>();

			try
			{
				foreach (var assm in TestAssemblies)
				{
#if WINDOWS
					var nameWithoutExt = assm.GetName().Name;
					var assemblyFileName = FileSystemUtils.PlatformGetFullAppPackageFilePath($"{nameWithoutExt}.dll");
#elif ANDROID
					// this is required to exist, but is not used
					var assemblyFileName = assm.GetName().Name + ".dll";
					assemblyFileName = Path.Combine(Android.App.Application.Context.CacheDir.AbsolutePath, assemblyFileName);
					if (!File.Exists(assemblyFileName))
						File.Create(assemblyFileName).Close();
#else
					var assemblyFileName = assm.Location;
#endif

					var configuration = GetConfiguration(assemblyFileName);
					var discoveryOptions = TestFrameworkOptions.ForDiscovery(configuration);

					try
					{
						if (cancelled)
							break;

						using (var framework = new XunitFrontController(AppDomainSupport.Denied, assemblyFileName, null, false))
						using (var sink = new TestDiscoverySink(() => cancelled))
						{
							framework.Find(false, sink, discoveryOptions);
							sink.Finished.WaitOne();

							result.Add(new AssemblyRunInfo(
								assemblyFileName,
								configuration,
								sink.TestCases.Select(tc => new TestCaseViewModel(assemblyFileName, tc)).ToList()));
						}
					}
					catch (Exception e)
					{
						Debug.WriteLine(e);
					}
				}
			}
			catch (Exception e)
			{
				Debug.WriteLine(e);
			}

			return result;
		}

		static TestAssemblyConfiguration GetConfiguration(string assemblyName)
		{
			var stream = GetConfigurationStreamForAssembly(assemblyName);
			if (stream != null)
			{
				using (stream)
				{
					return ConfigReader.Load(stream);
				}
			}

			return new TestAssemblyConfiguration();
		}

		static Stream GetConfigurationStreamForAssembly(string assemblyName)
		{
#if __ANDROID__
			var assets = Android.App.Application.Context.Assets;
			var allAssets = assets.List(string.Empty);

			if (allAssets.Contains($"{assemblyName}.xunit.runner.json"))
				return assets.Open($"{assemblyName}.xunit.runner.json");

			if (allAssets.Contains("xunit.runner.json"))
				return assets.Open("xunit.runner.json");
#else

			// See if there's a directory with the assm name. this might be the case for appx
			if (Directory.Exists(assemblyName))
			{
				if (File.Exists(Path.Combine(assemblyName, $"{assemblyName}.xunit.runner.json")))
				{
					return File.OpenRead(Path.Combine(assemblyName, $"{assemblyName}.xunit.runner.json"));
				}

				if (File.Exists(Path.Combine(assemblyName, "xunit.runner.json")))
				{
					return File.OpenRead(Path.Combine(assemblyName, "xunit.runner.json"));
				}
			}

			// Fallback to working dir

			// look for a file called assemblyName.xunit.runner.json first 
			if (File.Exists($"{assemblyName}.xunit.runner.json"))
			{
				return File.OpenRead($"{assemblyName}.xunit.runner.json");
			}

			if (File.Exists("xunit.runner.json"))
			{
				return File.OpenRead("xunit.runner.json");
			}
#endif

			return null;
		}

		Task RunTests(Func<IReadOnlyList<AssemblyRunInfo>> testCaseAccessor)
		{
			var tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);

			void Handler()
			{
				var toDispose = new List<IDisposable>();

				try
				{
					cancelled = false;
					var assemblies = testCaseAccessor();
					var parallelizeAssemblies = assemblies.All(runInfo => runInfo.Configuration.ParallelizeAssemblyOrDefault);

					if (parallelizeAssemblies)
					{
						assemblies
							.Select(runInfo => RunTestsInAssemblyAsync(toDispose, runInfo))
							.ToList()
							.ForEach(@event => @event.WaitOne());
					}
					else
					{
						foreach (var runInfo in assemblies)
						{
							RunTestsInAssembly(toDispose, runInfo);
						}
					}
				}
				catch (Exception e)
				{
					tcs.SetException(e);
				}
				finally
				{
					toDispose.ForEach(disposable => disposable.Dispose());
					tcs.TrySetResult(null);
				}
			}

			RunAsync(Handler);

			return tcs.Task;
		}

		void RunTestsInAssembly(List<IDisposable> toDispose, AssemblyRunInfo runInfo)
		{
			if (cancelled)
				return;

			var assemblyFileName = runInfo.AssemblyFileName;

			var longRunningSeconds = runInfo.Configuration.LongRunningTestSecondsOrDefault;

			var controller = new XunitFrontController(AppDomainSupport.Denied, assemblyFileName);

			lock (toDispose)
				toDispose.Add(controller);

			var xunitTestCases = runInfo.TestCases
				.Select(tc => new { vm = tc, tc = tc.TestCase })
				.Where(tc => tc.tc.UniqueID != null)
				.ToDictionary(tc => tc.tc, tc => tc.vm);

			var executionOptions = TestFrameworkOptions.ForExecution(runInfo.Configuration);

			var diagSink = new DiagnosticMessageSink(d => context.Post(_ => OnDiagnosticMessage?.Invoke(d), null), runInfo.AssemblyFileName, executionOptions.GetDiagnosticMessagesOrDefault());

			var deviceExecSink = new DeviceExecutionSink(xunitTestCases, this, context);

			IExecutionSink resultsSink = new ExecutionSink(deviceExecSink, new ExecutionSinkOptions
			{
				CancelThunk = () => cancelled
			});
			if (longRunningSeconds > 0)
				resultsSink = new ExecutionSink(resultsSink, new ExecutionSinkOptions
				{
					CancelThunk = () => cancelled,
					DiagnosticMessageSink = diagSink,
					LongRunningTestTime = TimeSpan.FromSeconds(longRunningSeconds)
				});

			var assm = new XunitProjectAssembly() { AssemblyFilename = runInfo.AssemblyFileName };
			deviceExecSink.OnMessage(new TestAssemblyExecutionStarting(assm, executionOptions));

			controller.RunTests(xunitTestCases.Select(tc => tc.Value.TestCase).ToList(), resultsSink, executionOptions);
			resultsSink.Finished.WaitOne();

			deviceExecSink.OnMessage(new TestAssemblyExecutionFinished(assm, executionOptions, resultsSink.ExecutionSummary));
		}

		ManualResetEvent RunTestsInAssemblyAsync(List<IDisposable> toDispose, AssemblyRunInfo runInfo)
		{
			var @event = new ManualResetEvent(false);

			void Handler()
			{
				try
				{
					RunTestsInAssembly(toDispose, runInfo);
				}
				finally
				{
					@event.Set();
				}
			}

			RunAsync(Handler);

			return @event;
		}

		static async void RunAsync(Action action)
		{
			var task = Task.Factory.StartNew(action, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);

			try
			{
				await task;
			}
			catch (Exception e)
			{
				if (Debugger.IsAttached)
				{
					Debugger.Break();
					Debug.WriteLine(e);
				}
			}
		}
	}
}
