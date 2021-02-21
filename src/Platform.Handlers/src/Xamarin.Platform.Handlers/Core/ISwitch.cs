using Xamarin.Forms;

namespace Xamarin.Platform
{
	public interface ISwitch : IView
	{
		bool IsToggled { get; set; }
		Color TrackColor { get; }
		Color ThumbColor { get; }
	}
}