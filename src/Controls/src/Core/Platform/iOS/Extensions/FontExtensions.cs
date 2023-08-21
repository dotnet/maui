// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Platform
{
	public static partial class FontExtensions
	{
		public static UIFont ToUIFont(this Font self, IFontManager fontManager)
		{
			if (self.IsDefault)
				return fontManager.DefaultFont;

			return fontManager.GetFont(self) ?? fontManager.DefaultFont;
		}

		public static UIFont ToUIFont<TFontElement>(this TFontElement fontElement) where TFontElement : Element, IFontElement
			=> fontElement.ToFont().ToUIFont(fontElement.RequireFontManager());
	}
}