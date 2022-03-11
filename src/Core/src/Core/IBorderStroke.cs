using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	/// <summary>
	/// Define how the Shape outline is painted on Layouts.
	/// </summary>
	public interface IBorderStroke : IStroke
	{
		/// <summary>
		/// Defines the shape of the border.
		/// </summary>
		IShape? Shape { get; }
	}
}