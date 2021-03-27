using Microsoft.UI.Xaml.Media;

namespace Microsoft.Maui.Graphics
{
	public partial interface IBrush
	{
		/// <summary>
		/// Create a new Brush that will render the current brush when added to a view.
		/// </summary>
		/// <returns>Returns a new Brush.</returns>
		Brush? ToNative();
	}
}