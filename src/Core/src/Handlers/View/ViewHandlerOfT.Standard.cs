using System;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Handlers
{
	public abstract partial class ViewHandler<TVirtualView, TNativeView>
	{
		public override void NativeArrange(Rectangle rect)
		{

		}

		protected override void SetupContainer()
		{

		}

		protected override void RemoveContainer()
		{

		}
	}

	public partial class WidgetHandler<TVirtualView, TNativeView> : ViewHandler<TVirtualView, TNativeView>
	{
		public override Size GetDesiredSize(double widthConstraint, double heightConstraint) => throw new NotImplementedException();
	}
}