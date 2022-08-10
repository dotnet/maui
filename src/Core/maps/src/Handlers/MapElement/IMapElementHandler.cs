#if __IOS__ || MACCATALYST
using PlatformView = MapKit.MKOverlayRenderer;
#elif MONOANDROID
using Android.Gms.Maps;
using Android.OS;
using Android.Runtime;
using PlatformView = Microsoft.Maui.Maps.Handlers.MapElementOptions;
#elif WINDOWS
using PlatformView = System.Object;
#elif TIZEN
using PlatformView = System.Object;
#elif (NETSTANDARD || !PLATFORM) || (NET6_0 && !IOS && !ANDROID && !TIZEN)
using PlatformView = System.Object;
#endif

namespace Microsoft.Maui.Maps.Handlers
{
#if MONOANDROID
	public class MapElementOptions : Android.Gms.Common.Internal.SafeParcel.AbstractSafeParcelable
	{
		public override void WriteToParcel(Parcel dest, [GeneratedEnum] ParcelableWriteFlags flags)
		{
			throw new System.NotImplementedException();
		}
	}
#endif

	public interface IMapElementHandler : IElementHandler
	{
		new IMapElement VirtualView { get; }
		new PlatformView PlatformView { get; }
	}
}
