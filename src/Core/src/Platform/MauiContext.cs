using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Animations;

namespace Microsoft.Maui
{
	public partial class MauiContext : IMauiContext
	{
		public MauiContext()
		{
			// Temporary hack until we fully remove Forms.Init
			Services = null!;
			Handlers = null!;
			AnimationManager = null!;
		}

		public MauiContext(IServiceProvider services)
		{
			Services = services ?? throw new ArgumentNullException(nameof(services));
			Handlers = Services.GetRequiredService<IMauiHandlersServiceProvider>();
			AnimationManager = Services.GetRequiredService<IAnimationManager>() ?? new AnimationManager();
			AnimationManager.Ticker = Services.GetRequiredService<ITicker>() ?? new NativeTicker(this);
		}

		public IServiceProvider Services { get; }

		public IMauiHandlersServiceProvider Handlers { get; }

		public IAnimationManager AnimationManager { get; protected set; }
	}
}