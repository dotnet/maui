using System;
#if ANDROID
using Android.Content;
#endif

namespace Microsoft.Maui
{
	public interface IMauiContext
	{
		IServiceProvider Services { get; }

		IMauiHandlersFactory Handlers { get; }

#if ANDROID
		Context? Context { get; }
#endif
	}
}
