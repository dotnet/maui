using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	public partial class MenuItem : IMenuElement
	{
		IImageSource IImageSourcePart.Source => this.IconImageSource;

		bool IImageSourcePart.IsAnimationPlaying => false;

		Color ITextStyle.TextColor => null;

		Font ITextStyle.Font => Font.Default;

		double ITextStyle.CharacterSpacing => 0;

		void IImageSourcePart.UpdateIsLoading(bool isLoading)
		{
		}
	}
}
