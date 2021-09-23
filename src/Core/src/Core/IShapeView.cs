using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	/// <summary>
	/// Represents a View that enables you to draw a shape to the screen.
	/// </summary>
	public interface IShapeView : IView, IStroke
	{
		/// <summary>
		/// Gets the Shape definition to render.
		/// </summary>
		IShape? Shape { get; }

		/// <summary>
		/// Determines how a Shape's contents are stretched to fill the view's layout space.
		/// </summary>
		PathAspect Aspect { get; }

		/// <summary>
		/// Indicates the brush used to paint the shape's interior.
		/// </summary>
		Paint? Fill { get; }
	}
}