using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	public interface IViewHandler : IElementHandler
	{
		bool HasContainer { get; set; }

		object? ContainerView { get; }

		new IView? VirtualView { get; }

		Size GetDesiredSize(double widthConstraint, double heightConstraint);

		void PlatformArrange(Rect frame);
	}
}