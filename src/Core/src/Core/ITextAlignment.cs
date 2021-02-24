using Microsoft.Maui;

namespace Microsoft.Maui
{
	public interface ITextAlignment : IView
	{
		TextAlignment HorizontalTextAlignment { get; }

		TextAlignment VerticalTextAlignment { get; }
	}
}