using System;
using Gtk;
using Microsoft.Maui.Controls;

namespace Microsoft.Maui.Handlers.ScrollView
{

	public class GtkScrollView : Gtk.ScrolledWindow
	{

		public ScrollOrientation ScrollOrientation { get; set; }

		protected override void OnGetPreferredHeightForWidth(int width, out int minimumHeight, out int naturalHeight)
		{
			base.OnGetPreferredHeightForWidth(width, out minimumHeight, out naturalHeight);
			// Child.GetPreferredHeightForWidth(width, out var childMinimumHeight, out var childNaturalHeight);
			//
			// minimumHeight = Math.Max(minimumHeight, childMinimumHeight);
			// naturalHeight = Math.Max(naturalHeight, childNaturalHeight);
			var o = this.ToScrollOrientation();

			// if (ScrollOrientation == ScrollOrientation.Vertical)
			// {
			// 	minimumHeight = childMinimumHeight;
			// 	naturalHeight = childNaturalHeight;
			// }

		}

		protected override void OnGetPreferredWidthForHeight(int height, out int minimumWidth, out int naturalWidth)
		{
			base.OnGetPreferredWidthForHeight(height, out minimumWidth, out naturalWidth);

			
			var o = this.ToScrollOrientation();
			
			if (ScrollOrientation == ScrollOrientation.Vertical)
			{
				Child.GetPreferredWidthForHeight(height, out var childMinimumWidth, out var childNaturalWidth);
				minimumWidth = Math.Max(minimumWidth, childMinimumWidth);
				naturalWidth = Math.Max(naturalWidth, childNaturalWidth);			
			}
		}

		protected override SizeRequestMode OnGetRequestMode()
		{
			var rm = base.OnGetRequestMode();

			return rm;
		}

	}

}