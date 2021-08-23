using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls.Handlers
{
	public partial class ShellHandler : ViewHandler<Shell, ShellView>
	{
		public override void SetVirtualView(IView view)
		{
			base.SetVirtualView(view);
			NativeView.SetElement((Shell)view, MauiContext);
		}

		protected override ShellView CreateNativeView()
		{
			return new ShellView(NativeParent);
		}
	}
}
