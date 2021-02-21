using Xamarin.Forms;

namespace Xamarin.Platform
{
	public interface IText : IFont
	{
		string Text { get; }

		Color TextColor { get; }
	}
}