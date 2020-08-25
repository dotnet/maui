using Xamarin.Forms;

namespace System.Maui
{
	public interface ILabel : IText
	{
		double LineHeight { get; }
#if !__MAUI__
		FormattedString FormattedText { get; }
#endif
		Font Font { get; }
		Color TextColor { get; }
		TextTransform TextTransform { get; }

		string UpdateFormsText(string text, TextTransform textTransform);
	}
}
