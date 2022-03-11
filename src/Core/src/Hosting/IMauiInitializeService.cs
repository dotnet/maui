using System;

namespace Microsoft.Maui.Hosting
{
	public interface IMauiInitializeService
	{
		void Initialize(IServiceProvider services);
	}

	public interface IMauiInitializeScopedService
	{
		void Initialize(IServiceProvider services);
	}
}