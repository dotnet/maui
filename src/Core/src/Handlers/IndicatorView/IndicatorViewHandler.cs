#if __IOS__ || MACCATALYST
using PlatformView = UIKit.UIPageControl;
#elif MONOANDROID
using PlatformView = Android.Views.View;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.FrameworkElement;
#elif NETSTANDARD || (NET6_0 && !IOS && !ANDROID)
using PlatformView = System.Object;
#endif


namespace Microsoft.Maui.Handlers
{
	public partial class IndicatorViewHandler : IIndicatorViewHandler
	{
		public static PropertyMapper<IIndicatorView, IIndicatorViewHandler> Mapper = new(ViewMapper)
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

		public IndicatorViewHandler(PropertyMapper mapper) : base(mapper ?? Mapper)
		{
		}

		IIndicatorView IIndicatorViewHandler.VirtualView => VirtualView;

		PlatformView IIndicatorViewHandler.PlatformView => PlatformView;
	}
}
