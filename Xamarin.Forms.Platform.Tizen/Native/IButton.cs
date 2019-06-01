using EColor = ElmSharp.Color;
using ESize = ElmSharp.Size;

namespace Xamarin.Forms.Platform.Tizen.Native
{
	public interface IButton
	{
		string Text { get; set; }

		double FontSize { get; set; }

		FontAttributes FontAttributes { get; set; }

		string FontFamily { get; set; }

		EColor TextColor { get; set; }

		Image Image { get; set; }

		ESize Measure(int availableWidth, int availableHeight);

		void UpdateStyle(string style);
	}
}