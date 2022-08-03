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
	public partial interface ISliderHandler : IViewHandler
	{
		new ISlider VirtualView { get; }
		new PlatformView PlatformView { get; }
	}
}