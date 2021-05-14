using System;
using UIKit;

namespace Microsoft.Maui
{
	public interface IFontManager
	{
		UIFont DefaultFont { get; }

		UIFont GetFont(Font font, double defaultFontSize = 0);

		double GetFontSize(Font font, double defaultFontSize = 0);
	}
}