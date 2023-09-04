using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	/// <summary>
	/// Defines members that view handlers should implement to provide mapping virtual views to platform views.
	/// </summary>
	public interface IViewHandler : IElementHandler
	{
		/// <summary>
		/// Gets or sets a value that indicates whether the <see cref="IElementHandler.PlatformView"/> is contained within a view.
		/// </summary>
		bool HasContainer { get; set; }

		/// <summary>
		/// Gets the container view for this view.
		/// </summary>
		object? ContainerView { get; }

		/// <summary>
		/// Gets the virtual view (.NET MAUI layer) that is managed by this handler.
		/// </summary>
		new IView? VirtualView { get; }

		/// <summary>
		/// Computes the actual size of a view based on the desired size and constraints. 
		/// </summary>
		/// <param name="widthConstraint">The constraint on the width of the view.</param>
		/// <param name="heightConstraint">The constraint on the height of the view.</param>
		/// <returns>The computed size for the view associated to this handler.</returns>
		Size GetDesiredSize(double widthConstraint, double heightConstraint);

		/// <summary>
		/// Positions child elements and determines a size for a view.
		/// </summary>
		/// <param name="frame">The size that the parent determines for the child views.</param>
		void PlatformArrange(Rect frame);
	}
}