using Xamarin.Forms;

namespace Xamarin.Platform
{
	public interface IFont
	{
		FontAttributes FontAttributes { get; }
		string FontFamily { get; }
		double FontSize { get; }
	}
}