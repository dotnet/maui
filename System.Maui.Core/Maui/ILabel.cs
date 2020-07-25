namespace System.Maui
{
	public interface ILabel : IText
	{
		double LineHeight { get; }
		FormattedString FormattedText { get; }
		Font Font { get; }
		Color TextColor { get; }
		TextTransform TextTransform { get; }

		string UpdateFormsText(string text, TextTransform textTransform);
	}
}
