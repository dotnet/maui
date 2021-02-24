using Microsoft.Maui;

namespace Microsoft.Maui
{
	public interface IText : IFont
	{
		string Text { get; }

		Color TextColor { get; }
	}
}