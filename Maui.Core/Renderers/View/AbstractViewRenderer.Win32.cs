using System.Windows;

namespace System.Maui.Platform
{
	public partial class AbstractViewRenderer<TVirtualView, TNativeView> : IViewRenderer
	{
		public void SetFrame(Rectangle rect)
		{
			TypedNativeView.Arrange(new Rect(rect.Left, rect.Top, rect.Width, rect.Height));
		}

		public virtual SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			if (TypedNativeView == null)
			{
				return new SizeRequest(Size.Zero);
			}

			TypedNativeView.Measure(new Windows.Size(widthConstraint, heightConstraint));

			var desiredSize = new Size(TypedNativeView.DesiredSize.Width, TypedNativeView.DesiredSize.Height);

			return new SizeRequest(desiredSize);
		}

		void SetupContainer()
		{
		}

		void RemoveContainer()
		{
		}
	}
}
