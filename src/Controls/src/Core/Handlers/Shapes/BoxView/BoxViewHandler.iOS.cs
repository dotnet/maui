namespace Microsoft.Maui.Controls.Handlers
{
	public partial class BoxViewHandler : ShapeViewHandler
	{
		public override void SetVirtualView(IView view)
		{
			base.SetVirtualView(view);

			if (PlatformView != null)
			{
				PlatformView.View = view;
			}
		}
	}
}