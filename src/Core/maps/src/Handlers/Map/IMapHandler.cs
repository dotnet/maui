#if __IOS__ || MACCATALYST
using PlatformView = Microsoft.Maui.Maps.Platform.MauiMKMapView;
#elif MONOANDROID
using Android.Gms.Maps;
using PlatformView = Android.Gms.Maps.MapView;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.FrameworkElement;
#elif TIZEN
using PlatformView = Tizen.NUI.BaseComponents.View;
#elif (NETSTANDARD || !PLATFORM) || (NET6_0 && !IOS && !ANDROID && !TIZEN)
using PlatformView = System.Object;
#endif

namespace Microsoft.Maui.Maps.Handlers
{
	/// <summary>
	/// Provides handler functionality for the <see cref="IMap"/> control.
	/// </summary>
	public interface IMapHandler : IViewHandler
	{
		/// <inheritdoc/>
		new IMap VirtualView { get; }

		/// <inheritdoc/>
		new PlatformView PlatformView { get; }
#if MONOANDROID
		/// <summary>
		/// Gets the Google Maps instance for Android.
		/// </summary>
		GoogleMap? Map { get; }
#endif
		/// <summary>
		/// Updates the specified map element on the platform.
		/// </summary>
		/// <param name="element">The map element to update.</param>
		void UpdateMapElement(IMapElement element);
	}
}
