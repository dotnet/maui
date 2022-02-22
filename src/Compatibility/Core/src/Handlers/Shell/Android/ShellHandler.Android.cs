using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using AView = Android.Views.View;

namespace Microsoft.Maui.Controls.Platform.Compatibility.Compatibility
{
	public partial class ShellHandler : ViewHandler<Shell, ShellFlyoutRenderer>
	{
		ShellRenderer _ShellRenderer;
		protected override ShellFlyoutRenderer CreatePlatformView()
		{
			var drawerLayout = (_ShellRenderer as IShellContext)?.CurrentDrawerLayout;
			return (ShellFlyoutRenderer)drawerLayout;
		}


		public override void SetVirtualView(IView view)
		{
			_ShellRenderer = new ShellRenderer(Context);
			_ShellRenderer.SetVirtualView((Shell)view);
			base.SetVirtualView(view);
		}
	}
}
