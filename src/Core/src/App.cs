using System;

namespace Microsoft.Maui
{
	public abstract class App : IApp
	{
		public IServiceProvider? Services { get; private set; }

		internal void SetServiceProvider(IServiceProvider provider)
		{
			Services = provider;
		}
	}
}