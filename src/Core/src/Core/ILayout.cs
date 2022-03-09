using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	/// <summary>
	/// Provides the base properties and methods for all Layout elements.
	/// Use Layout elements to position and size child elements in .NET MAUI applications.
	/// </summary>
	public interface ILayout : IView, IContainer, ISafeAreaView, IPadding
	{
		/// <summary>
		/// Measures the desired size of the ILayout within the given constraints.
		/// </summary>
		/// <param name="widthConstraint">The width limit for measuring the ILayout.</param>
		/// <param name="heightConstraint">The height limit for measuring the ILayout.</param>
		/// <returns>The desired size of the ILayout.</returns>
		Size CrossPlatformMeasure(double widthConstraint, double heightConstraint);

		/// <summary>
		/// Arranges the children of the ILayout within the given bounds.
		/// </summary>
		/// <param name="bounds">The bounds in which the ILayout's children should be arranged.</param>
		/// <returns>The actual size of the arranged ILayout.</returns>
		Size CrossPlatformArrange(Rect bounds);

		/// <summary>
		/// Specifies whether the ILayout clips its content to its boundaries.
		/// </summary>
		bool ClipsToBounds { get; }
	}
}
