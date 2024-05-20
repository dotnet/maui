#nullable enable
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Dispatching;

namespace Microsoft.Maui.TestUtils.DeviceTests.Runners
{
	public static class TestDispatcher
	{
		static IDispatcher? s_dispatcher;
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
					s_dispatcher = TestServices.Services.GetService<ApplicationDispatcher>()?.Dispatcher;

				if (s_dispatcher is null)
					throw new InvalidOperationException($"Test app did not provide a dispatcher.");

				return s_dispatcher;
			}
		}
	}
}