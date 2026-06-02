#if __IOS__ || MACCATALYST
using PlatformView = MapKit.IMKAnnotation;
#elif MONOANDROID
using PlatformView = Android.Gms.Maps.Model.MarkerOptions;
#elif WINDOWS
using PlatformView = System.Object;
#elif TIZEN
using PlatformView = System.Object;
#elif (NETSTANDARD || !PLATFORM) || (NET6_0 && !IOS && !ANDROID && !TIZEN)
using PlatformView = System.Object;
#endif
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Maps.Handlers
{
	/// <summary>
	/// Handler for <see cref="IMapPin"/> objects that manages the platform-specific implementation.
	/// </summary>
	public partial class MapPinHandler : IMapPinHandler
	{
		/// <summary>
		/// The property mapper that maps cross-platform properties to platform-specific methods.
		/// </summary>
		public static IPropertyMapper<IMapPin, IMapPinHandler> Mapper = new PropertyMapper<IMapPin, IMapPinHandler>(ElementMapper)
		{
			[nameof(IMapPin.Location)] = MapLocation,
			[nameof(IMapPin.Label)] = MapLabel,
			[nameof(IMapPin.Address)] = MapAddress,
			[nameof(IMapPin.ImageSource)] = MapImageSource,
		};

		/// <summary>
		/// Initializes a new instance of the <see cref="MapPinHandler"/> class with the default mapper.
		/// </summary>
		public MapPinHandler() : base(Mapper)
		{

		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MapPinHandler"/> class with an optional custom mapper.
		/// </summary>
		/// <param name="mapper">The property mapper to use, or <see langword="null"/> to use the default.</param>
		public MapPinHandler(IPropertyMapper? mapper = null)
		: base(mapper ?? Mapper)
		{
		}

		IMapPin IMapPinHandler.VirtualView => VirtualView;

		PlatformView IMapPinHandler.PlatformView => PlatformView;
	}
}
