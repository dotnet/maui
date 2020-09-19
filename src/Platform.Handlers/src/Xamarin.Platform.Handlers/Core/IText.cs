using Xamarin.Forms;

namespace Xamarin.Platform
{
	public interface IText : IFont, ITextAlignment
	{
		string Text { get; }

		Color Color { get; }

		Font Font { get; }

		TextTransform TextTransform { get; }

		double CharacterSpacing { get; }
	}
}