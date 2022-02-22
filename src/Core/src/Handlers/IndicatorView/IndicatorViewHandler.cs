#nullable enable
#if __IOS__ || MACCATALYST
using PlatformView = Microsoft.Maui.Platform.MauiPageControl;
#elif MONOANDROID
using PlatformView = Microsoft.Maui.Platform.MauiPageControl;
#elif WINDOWS
using PlatformView = Microsoft.Maui.Platform.MauiPageControl;
#elif NETSTANDARD || (NET6_0 && !IOS && !ANDROID)
using PlatformView = System.Object;
#endif


namespace Microsoft.Maui.Handlers
{
	public partial class IndicatorViewHandler : IIndicatorViewHandler
	{
		public static PropertyMapper<IIndicatorView, IIndicatorViewHandler> IndicatorViewMapper = new(ViewMapper)
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

		public IndicatorViewHandler() : base(IndicatorViewMapper)
		{
		}

		public IndicatorViewHandler(PropertyMapper mapper) : base(mapper ?? IndicatorViewMapper)
		{
		}

		IIndicatorView IIndicatorViewHandler.VirtualView => VirtualView;

		PlatformView IIndicatorViewHandler.PlatformView => PlatformView;
	}
}
