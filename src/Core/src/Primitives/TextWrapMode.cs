namespace Microsoft.Maui
{
	public enum TextWrapMode
	{
		None,       // Text does not wrap, regardless of number of lines allowed
		Word,       // Text wraps at word boundaries
		Character   // Text wraps at character boundaries (possibly mid-word)
	}
}