using Microsoft.Maui;

namespace Microsoft.Maui
{
	public interface ITextInput : IText
	{
		Keyboard Keyboard { get; }
		bool IsSpellCheckEnabled { get; }
		int MaxLength { get; }
		string Placeholder { get; }
		bool IsReadOnly { get; }
	}
}