using Android.Graphics.Drawables;

namespace Microsoft.Maui.Graphics
{
	public partial interface IBrush
	{
		/// <summary>
		/// Create a new Drawable that will render the current brush when added to a view.
		/// </summary>
		/// <returns>Returns a new Drawable.</returns>
		Drawable? ToDrawable();
	}
}