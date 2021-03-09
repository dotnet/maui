using Microsoft.Maui;

namespace Microsoft.Maui
{
	public interface IFont
	{
		FontAttributes FontAttributes { get; }
		string FontFamily { get; }
		double FontSize { get; }
	}
}