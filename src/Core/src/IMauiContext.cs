using System;

namespace Microsoft.Maui
{
	public interface IMauiContext
	{
		IServiceProvider Services { get; }

		IMauiHandlersFactory Handlers { get; }

#if __ANDROID__
		Android.Content.Context? Context { get; }
#endif
	}
}
