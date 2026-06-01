using System;
using Microsoft.Maui.Platform;

namespace Microsoft.Maui.Maps.Handlers
{
	internal static class MapElementPlatformOptions
	{
		internal static TResult? InvokeWithOptions<TOptions, TResult>(
			IMapElement mapElement,
			IMauiContext mauiContext,
			Func<TOptions, TResult> callback)
			where TOptions : Java.Lang.Object
			where TResult : class
		{
			IElementHandler? handler = null;
			Java.Lang.Object? platformOptions = null;

			try
			{
				handler = mapElement.ToHandler(mauiContext);
				platformOptions = handler.PlatformView as Java.Lang.Object;

				if (platformOptions is not TOptions options)
					return null;

				return callback(options);
			}
			finally
			{
				try
				{
					handler?.DisconnectHandler();
				}
				finally
				{
					platformOptions?.Dispose();
				}
			}
		}
	}
}
