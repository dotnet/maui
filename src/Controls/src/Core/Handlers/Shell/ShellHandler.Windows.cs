using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls.Handlers
{
	public partial class ShellHandler : ViewHandler<Shell, ShellRenderer>
	{
		protected override ShellRenderer CreateNativeView()
		{
			return new ShellRenderer();
		}

		public override Size GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			return NativeView.GetDesiredSize(widthConstraint, heightConstraint);
		}

		public override void SetVirtualView(IView view)
		{
			base.SetVirtualView(view);
			NativeView.SetElement((Shell)view);
		}
	}
}
