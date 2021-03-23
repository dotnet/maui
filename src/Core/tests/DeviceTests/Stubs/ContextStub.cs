using System;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	class ContextStub : IMauiContext
	{
		public ContextStub(IServiceProvider services)
		{
			Services = services;
		}

		public IServiceProvider Services { get; }

		public IMauiHandlersServiceProvider Handlers =>
			Services.GetRequiredService<IMauiHandlersServiceProvider>();

#if __ANDROID__
		public Android.Content.Context Context => Platform.DefaultContext;
#endif
	}
}