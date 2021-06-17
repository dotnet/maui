using System;

namespace Microsoft.Maui
{
	public interface IMauiContext
	{
		TargetIdiom Idiom { get; }

		IServiceProvider Services { get; }

		IMauiHandlersServiceProvider Handlers { get; }

#if __ANDROID__
		Android.Content.Context? Context { get; }
#endif
	}
}
