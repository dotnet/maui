namespace Microsoft.Maui.Controls.Shapes
{
	/// <summary>
	/// The base class for all path segment types that define a portion of a <see cref="PathFigure"/>.
	/// </summary>
	public abstract class PathSegment : BindableObject, IAnimatable
	{
		/// <summary>
		/// Signals the start of a batch of property changes to avoid triggering multiple updates.
		/// </summary>
		public void BatchBegin()
		{

		}

		/// <summary>
		/// Signals the end of a batch of property changes and applies the updates.
		/// </summary>
		public void BatchCommit()
		{

		}
	}
}