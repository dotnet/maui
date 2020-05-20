namespace System.Maui
{
	public interface ITextInput : IText
	{
		int MaxLength { get; }
		string Placeholder { get; }
		Color PlaceholderColor { get; }
		new string Text { get; set; }
		string IText.Text => Text;
	}
}