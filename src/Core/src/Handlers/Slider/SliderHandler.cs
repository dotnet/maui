#nullable enable
#if __IOS__ || MACCATALYST
using PlatformView = UIKit.UISlider;
#elif MONOANDROID
using PlatformView = Android.Widget.SeekBar;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.Controls.Slider;
#elif TIZEN
using PlatformView = ElmSharp.Slider;
#elif (NETSTANDARD || !PLATFORM) || (NET6_0_OR_GREATER && !IOS && !ANDROID && !TIZEN)
using PlatformView = System.Object;
#endif

namespace Microsoft.Maui.Handlers
{
	public partial class SliderHandler : ISliderHandler
	{
		public static IPropertyMapper<ISlider, ISliderHandler> Mapper = new PropertyMapper<ISlider, ISliderHandler>(ViewHandler.ViewMapper)
		{
			[nameof(ISlider.Maximum)] = MapMaximum,
			[nameof(ISlider.MaximumTrackColor)] = MapMaximumTrackColor,
			[nameof(ISlider.Minimum)] = MapMinimum,
			[nameof(ISlider.MinimumTrackColor)] = MapMinimumTrackColor,
			[nameof(ISlider.ThumbColor)] = MapThumbColor,
			[nameof(ISlider.ThumbImageSource)] = MapThumbImageSource,
			[nameof(ISlider.Value)] = MapValue,
		};

		public static CommandMapper<ISlider, ISliderHandler> CommandMapper = new(ViewCommandMapper)
		{
		};

		public SliderHandler() : base(Mapper)
		{
		}

		public SliderHandler(IPropertyMapper? mapper = null) : base(mapper ?? Mapper)
		{
		}

		ISlider ISliderHandler.VirtualView => VirtualView;

		PlatformView ISliderHandler.PlatformView => PlatformView;
	}
}