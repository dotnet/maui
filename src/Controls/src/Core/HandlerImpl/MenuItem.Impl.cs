#nullable disable
using System.Collections.Generic;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../docs/Microsoft.Maui.Controls/MenuItem.xml" path="Type[@FullName='Microsoft.Maui.Controls.MenuItem']/Docs/*" />
	public partial class MenuItem : IMenuElement
	{
		IReadOnlyList<IAccelerator> IMenuElement.Accelerators =>
			GetAccelerator(this) is Accelerator acc ? new[] { acc } : null;

		IImageSource IImageSourcePart.Source => this.IconImageSource;

		bool IImageSourcePart.IsAnimationPlaying => false;

		Color ITextStyle.TextColor => null;

		Font ITextStyle.Font => Font.Default;

		double ITextStyle.CharacterSpacing => 0;

		void IMenuElement.Clicked()
		{
			((IMenuItemController)this).Activate();
		}

		void IImageSourcePart.UpdateIsLoading(bool isLoading)
		{
		}
	}
}
