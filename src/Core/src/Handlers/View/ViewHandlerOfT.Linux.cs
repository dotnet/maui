using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Handlers
{
	public partial class ViewHandler<TVirtualView, TNativeView> : INativeViewHandler
	{
		Gtk.Widget? INativeViewHandler.NativeView => (Gtk.Widget?)base.NativeView;

		public override void SetFrame(Rectangle rect)
		{

		}

		public override Size GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			return new Size(widthConstraint, heightConstraint);
		}

		protected override void SetupContainer()
		{

		}

		protected override void RemoveContainer()
		{

		}
	}
}