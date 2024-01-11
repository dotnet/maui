using System;

namespace Microsoft.Maui.Hosting
{
	public interface IMauiInitializeService
	{
		void Initialize(IServiceProvider services);
	}

	// Obsolete/rework for NET9: https://github.com/dotnet/maui/issues/19591
	public interface IMauiInitializeScopedService
	{
		void Initialize(IServiceProvider services);
	}
}