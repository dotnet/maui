using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.Dispatching
{
	public partial class Dispatcher : IDispatcher
	{
		[ThreadStatic]
		static IDispatcher? s_instance;

		public static IDispatcher? GetForCurrentThread() =>
			s_instance ??= GetForCurrentThreadImplementation();

		public bool IsInvokeRequired =>
			IsInvokeRequiredImplementation();

		public void BeginInvokeOnMainThread(Action action) =>
			BeginInvokeOnMainThreadImplementation(action);
	}

	class DispatcherInitializer : IMauiInitializeScopedService
	{
		public void Initialize(IServiceProvider services)
		{
			_ = services.GetRequiredService<IDispatcher>();
		}
	}
}