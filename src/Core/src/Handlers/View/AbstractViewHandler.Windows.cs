using Microsoft.Maui;

namespace Microsoft.Maui.Handlers
{
	public abstract partial class AbstractViewHandler<TVirtualView, TNativeView>
	{
		public void SetFrame(Rectangle rect)
		{

		}

		public virtual Size GetDesiredSize(double widthConstraint, double heightConstraint)
			=> Size.Zero;

		void SetupContainer()
		{

		}

		void RemoveContainer()
		{

		}
	}
}