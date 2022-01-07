using System;
using System.ComponentModel;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	public partial class SwipeItem : MenuItem, ISwipeItemMenuItem
	{
		Paint IMenuElement.Background => new SolidPaint(BackgroundColor);
	}
}