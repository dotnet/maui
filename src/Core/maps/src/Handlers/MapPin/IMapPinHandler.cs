#if __IOS__ || MACCATALYST
using PlatformView = MapKit.IMKAnnotation;
#elif MONOANDROID
using Android.Gms.Maps;
using PlatformView = Android.Gms.Maps.Model.MarkerOptions;
#elif WINDOWS
using PlatformView = System.Object;
#elif TIZEN
using PlatformView = System.Object;
#elif (NETSTANDARD || !PLATFORM) || (NET6_0 && !IOS && !ANDROID && !TIZEN)
using PlatformView = System.Object;
#endif

namespace Microsoft.Maui.Maps.Handlers
{
	/// <summary>
	/// Provides handler functionality for <see cref="IMapPin"/> objects.
	/// </summary>
	public interface IMapPinHandler : IElementHandler
	{
		/// <inheritdoc/>
		new IMapPin VirtualView { get; }

		/// <inheritdoc/>
		new PlatformView PlatformView { get; }
	}
}
