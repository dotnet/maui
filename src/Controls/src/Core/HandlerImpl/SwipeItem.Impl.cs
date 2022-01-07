using System;
using System.ComponentModel;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	public partial class SwipeItem : MenuItem, Maui.ISwipeItemMenuItem
	{
		Paint IMenuElement.Background => new SolidPaint(BackgroundColor);
	}
}