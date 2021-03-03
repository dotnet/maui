using Microsoft.Maui;

namespace Microsoft.Maui
{
	public interface ISwitch : IView
	{
		bool IsToggled { get; set; }
		Color TrackColor { get; }
		Color ThumbColor { get; }
	}
}