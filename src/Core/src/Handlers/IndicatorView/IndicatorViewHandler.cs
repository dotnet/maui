#nullable enable
#if __IOS__ || MACCATALYST
using PlatformView = Microsoft.Maui.Platform.MauiPageControl;
#elif MONOANDROID
using PlatformView = Microsoft.Maui.Platform.MauiPageControl;
#elif WINDOWS
using PlatformView = Microsoft.Maui.Platform.MauiPageControl;
#elif TIZEN
using PlatformView = Tizen.UIExtensions.ElmSharp.IndicatorView;
#elif (NETSTANDARD || !PLATFORM) || (NET6_0_OR_GREATER && !IOS && !ANDROID && !TIZEN)
using PlatformView = System.Object;
#endif


namespace Microsoft.Maui.Handlers
{
	public partial class IndicatorViewHandler : IIndicatorViewHandler
	{
		public static IPropertyMapper<IIndicatorView, IIndicatorViewHandler> Mapper = new PropertyMapper<IIndicatorView, IIndicatorViewHandler>(ViewMapper)
		{
			[nameof(IIndicatorView.Count)] = MapCount,
			[nameof(IIndicatorView.Position)] = MapPosition,
			[nameof(IIndicatorView.HideSingle)] = MapHideSingle,
			[nameof(IIndicatorView.MaximumVisible)] = MapMaximumVisible,
			[nameof(IIndicatorView.IndicatorSize)] = MapIndicatorSize,
			[nameof(IIndicatorView.IndicatorColor)] = MapIndicatorColor,
			[nameof(IIndicatorView.SelectedIndicatorColor)] = MapSelectedIndicatorColor,
			[nameof(IIndicatorView.IndicatorsShape)] = MapIndicatorShape
		};

		public static CommandMapper<IActivityIndicator, IIndicatorViewHandler> CommandMapper = new(ViewCommandMapper)
		{
		};

		public IndicatorViewHandler() : base(Mapper)
		{
		}

		public IndicatorViewHandler(IPropertyMapper mapper) : base(mapper ?? Mapper)
		{
		}

		IIndicatorView IIndicatorViewHandler.VirtualView => VirtualView;

		PlatformView IIndicatorViewHandler.PlatformView => PlatformView;
	}
}
