using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Handlers
{
	public abstract partial class ViewHandler<TVirtualView, TPlatformView>
	{
		public override void PlatformArrange(Rect rect)
		{

		}

		public override Size GetDesiredSize(double widthConstraint, double heightConstraint)
			=> Size.Zero;

		protected override void SetupContainer()
		{

		}

		protected override void RemoveContainer()
		{

		}
	}
}