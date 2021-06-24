using System;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Maui
{
	public partial class MauiContext : IMauiContext
	{
		public MauiContext()
		{
			// Temporary hack until we fully remove Forms.Init
			Services = null!;
			Handlers = null!;
		}

		private MauiContext(IServiceProvider services)
		{
			Services = services ?? throw new ArgumentNullException(nameof(services));
			Handlers = Services.GetRequiredService<IMauiHandlersServiceProvider>();
		}

		public IServiceProvider Services { get; }

		public IMauiHandlersServiceProvider Handlers { get; }
	}
}