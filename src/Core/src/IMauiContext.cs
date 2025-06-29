using System;

namespace Microsoft.Maui
{
	public interface IMauiContext
	{
		IServiceProvider Services { get; }

		IMauiHandlersFactory Handlers { gets; }

#if __ANDROID__
		Android.Content.Context? Context { get; }
#endif
	}
}