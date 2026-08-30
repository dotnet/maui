using System.Runtime.ExceptionServices;
using System.Windows.Forms;
using ComponentsDispatcher = Microsoft.AspNetCore.Components.Dispatcher;
using DispatcherPriority = System.Windows.Threading.DispatcherPriority;
using WindowsDispatcher = System.Windows.Threading.Dispatcher;
using WinFormsApplication = System.Windows.Forms.Application;

namespace Microsoft.AspNetCore.Components.WebView.Windows.UnitTests;

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class WindowsDispatcherCollection : ICollectionFixture<WindowsDispatcherFixture>
{
	public const string Name = "BlazorWebView Windows dispatchers";
}

public sealed class WindowsDispatcherFixture : IDisposable
{
	private static readonly TimeSpan StartupTimeout = TimeSpan.FromSeconds(30);
	private readonly ApplicationContext _applicationContext;
	private readonly Thread _wpfThread;
	private readonly Thread _windowsFormsThread;

	public WindowsDispatcherFixture()
	{
		var wpfReady = new TaskCompletionSource<(WindowsDispatcher Native, ComponentsDispatcher Components)>(
			TaskCreationOptions.RunContinuationsAsynchronously);
		_wpfThread = StartStaThread("BlazorWebView WPF test dispatcher", () =>
		{
			try
			{
				var nativeDispatcher = WindowsDispatcher.CurrentDispatcher;
				wpfReady.SetResult((
					nativeDispatcher,
					new Wpf.WpfDispatcher(nativeDispatcher)));
				WindowsDispatcher.Run();
			}
			catch (Exception ex)
			{
				wpfReady.TrySetException(ex);
			}
		});

		(WpfNativeDispatcher, WpfDispatcher) = WaitForStartup(
			wpfReady.Task,
			"WPF");

		var windowsFormsReady = new TaskCompletionSource<(
			Control Control,
			ApplicationContext Context,
			ComponentsDispatcher Components)>(TaskCreationOptions.RunContinuationsAsynchronously);
		_windowsFormsThread = StartStaThread("BlazorWebView WinForms test dispatcher", () =>
		{
			try
			{
				WinFormsApplication.SetUnhandledExceptionMode(
					UnhandledExceptionMode.CatchException,
					threadScope: true);
				var context = new ApplicationContext();
				using var control = new Control();
				_ = control.Handle;
				windowsFormsReady.SetResult((
					control,
					context,
					new WindowsForms.WindowsFormsDispatcher(control)));
				WinFormsApplication.Run(context);
			}
			catch (Exception ex)
			{
				windowsFormsReady.TrySetException(ex);
			}
		});

		(WindowsFormsControl, _applicationContext, WindowsFormsDispatcher) = WaitForStartup(
			windowsFormsReady.Task,
			"Windows Forms");
	}

	public WindowsDispatcher WpfNativeDispatcher { get; }

	public ComponentsDispatcher WpfDispatcher { get; }

	public Control WindowsFormsControl { get; }

	public ComponentsDispatcher WindowsFormsDispatcher { get; }

	public async Task InvokeOnWpfDispatcher(Func<Task> workItem)
	{
		var task = await WpfNativeDispatcher
			.InvokeAsync(workItem)
			.Task
			.WaitAsync(TimeSpan.FromSeconds(5));
		await task.WaitAsync(TimeSpan.FromSeconds(5));
	}

	public async Task InvokeOnWindowsFormsDispatcher(Func<Task> workItem)
	{
		var asyncResult = WindowsFormsControl.BeginInvoke(workItem);
		var task = await Task<Task>.Factory
			.FromAsync(asyncResult, result => (Task)WindowsFormsControl.EndInvoke(result)!)
			.WaitAsync(TimeSpan.FromSeconds(5));
		await task.WaitAsync(TimeSpan.FromSeconds(5));
	}

	public void Dispose()
	{
		var exceptions = new List<Exception>();

		TryCleanup(
			() => WpfNativeDispatcher.BeginInvokeShutdown(DispatcherPriority.Send),
			exceptions);
		TryCleanup(
			() => WindowsFormsControl.BeginInvoke(_applicationContext.ExitThread),
			exceptions);

		if (!_wpfThread.Join(TimeSpan.FromSeconds(5)))
		{
			exceptions.Add(new TimeoutException("The WPF test dispatcher did not stop."));
		}

		if (!_windowsFormsThread.Join(TimeSpan.FromSeconds(5)))
		{
			exceptions.Add(new TimeoutException("The Windows Forms test dispatcher did not stop."));
		}

		TryCleanup(_applicationContext.Dispose, exceptions);

		if (exceptions.Count == 1)
		{
			ExceptionDispatchInfo.Capture(exceptions[0]).Throw();
		}

		if (exceptions.Count > 1)
		{
			throw new AggregateException("Multiple errors occurred while stopping the test dispatchers.", exceptions);
		}
	}

	private static TResult WaitForStartup<TResult>(Task<TResult> startup, string dispatcherName)
	{
		try
		{
			return startup
				.WaitAsync(StartupTimeout)
				.GetAwaiter()
				.GetResult();
		}
		catch (TimeoutException ex) when (!startup.IsCompleted)
		{
			throw new TimeoutException(
				$"The {dispatcherName} test dispatcher did not start within {StartupTimeout}.",
				ex);
		}
	}

	private static void TryCleanup(Action cleanup, List<Exception> exceptions)
	{
		try
		{
			cleanup();
		}
		catch (Exception ex)
		{
			exceptions.Add(ex);
		}
	}

	private static Thread StartStaThread(string name, ThreadStart start)
	{
		var thread = new Thread(start)
		{
			IsBackground = true,
			Name = name,
		};
		thread.SetApartmentState(ApartmentState.STA);
		thread.Start();
		return thread;
	}
}
