using CoreAnimation;
using CoreGraphics;

namespace Microsoft.Maui.Graphics
{
	public partial interface IBrush
	{
		/// <summary>
		/// Create a new CALayer that will render the current brush when added to a view.
		/// </summary>
		/// <param name="frame">The initial frame of the layer.</param>
		/// <returns>Returns a new CALayer.</returns>
		CALayer? ToCALayer(CGRect frame = default);
	}
}