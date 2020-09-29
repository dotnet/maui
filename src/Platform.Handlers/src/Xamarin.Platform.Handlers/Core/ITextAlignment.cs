using Xamarin.Forms;

namespace Xamarin.Platform
{
	public interface ITextAlignment : IView
	{
		TextAlignment HorizontalTextAlignment { get; }

		TextAlignment VerticalTextAlignment { get; }
	}
}