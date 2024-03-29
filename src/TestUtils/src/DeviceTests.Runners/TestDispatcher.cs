#nullable enable
using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Dispatching;

namespace Microsoft.Maui.TestUtils.DeviceTests.Runners
{
	public static class TestDispatcher
	{
		static IDispatcher? s_dispatcher = null;
		static IDispatcherProvider? s_provider;

		public static IDispatcherProvider Provider
		{
			get
			{
				if (s_provider is null)
					s_provider = TestServices.Services.GetService<IDispatcherProvider>();

				if (s_provider is null)
					throw new InvalidOperationException($"Test app did not provide a dispatcher.");

				return s_provider;
			}
		}

		public static IDispatcher Current
		{
			get
			{
				if (s_dispatcher is null)
				{
					// throw new NotImplementedException("Fail because ApplicationDispatcher is internal...");

					var appDispatcherClass = typeof(Dispatcher).Assembly.GetType("Microsoft.Maui.Dispatching.ApplicationDispatcher");
					if (appDispatcherClass is null)
					{
						throw new Exception("Our reflection magic trying to get the internal ApplicationDispatcher failed...");
					}

					var appDispatcherInstance = TestServices.Services.GetService(appDispatcherClass);
					if (appDispatcherInstance is null)
					{
						throw new Exception("Our reflection magic trying to get the internal ApplicationDispatcher failed...");
					}

					var dispatcher = appDispatcherInstance?.GetType()?.GetProperty("Dispatcher")?.GetValue(appDispatcherInstance) as Dispatcher;
					if (dispatcher is null)
					{
						throw new Exception("The ApplicationDispatcher.Dispatcher is null.");
					}

					s_dispatcher = dispatcher;

					// s_dispatcher = TestServices.Services.GetService<ApplicationDispatcher>()?.Dispatcher;
				}

				if (s_dispatcher is null)
					throw new InvalidOperationException($"Test app did not provide a dispatcher.");

				return s_dispatcher;
			}
		}
	}
}