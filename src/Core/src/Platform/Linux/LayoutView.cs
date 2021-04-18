using Gdk;
using Gtk;

namespace Microsoft.Maui
{
	public class LayoutView : Fixed
	{
		protected override void OnAdjustSizeRequest(Orientation orientation, out int minimum_size, out int natural_size)
		{
			base.OnAdjustSizeRequest(orientation, out minimum_size, out natural_size);
		}

		protected override void OnSizeAllocated(Rectangle allocation)
		{
			base.OnSizeAllocated(allocation);
		}
	}
}