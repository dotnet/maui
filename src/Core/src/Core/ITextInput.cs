namespace Microsoft.Maui
{
	public interface ITextInput : IText
	{
		new string Text { get; set; }

		bool IsReadOnly { get; }
	}
}