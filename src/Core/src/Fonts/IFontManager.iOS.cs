﻿using System;
using UIKit;

namespace Microsoft.Maui
{
	public interface IFontManager
	{
		UIFont DefaultFont { get; }

		UIFont GetFont(Font font);

		nfloat GetFontSize(Font font);
	}
}