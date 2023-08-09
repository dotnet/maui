using System;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Provides data for the <see cref="DragGestureRecognizer.DragStarting"/> event.
	/// </summary>
	public class DragStartingEventArgs : EventArgs
	{
		Func<IElement?, Point?>? _getPosition;

		/// <summary>
		/// Initializes a new instance of the <see cref="DragStartingEventArgs"/> class.
		/// </summary>
		// TODO: JD - Maybe mark this as obsolete? 
		public DragStartingEventArgs()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DragStartingEventArgs"/> class.
		/// </summary>
		/// <param name="getPosition"> Something</param>
		public DragStartingEventArgs(Func<IElement?, Point?>? getPosition)
		{
			_getPosition = getPosition;
		}

		/// <summary>
		/// Gets or sets a value that indicates whether the event handler has handled the event or whether .NET MAUI should continue its own processing.
		/// </summary>
		public bool Handled { get; set; }

		/// <summary>
		/// Gets or sets a value that indicates whether the event should be canceled.
		/// </summary>
		public bool Cancel { get; set; }

		/// <summary>
		/// Gets the data package that accompanies the drag source.
		/// </summary>
		public DataPackage Data { get; private init; } = new DataPackage();

		/// <summary>
		/// Gets the location where dragging started relative to the specified element.
		/// </summary>
		/// <remarks>If <paramref name="relativeTo"/> is <see langword="null"/> then the position of where dragging started in the screen is returned.</remarks>
		/// <param name="relativeTo">Element whose position is used to calculate the relative position.</param>
		/// <returns>The point where dragging started relative to <paramref name="relativeTo"/>.</returns>
		public virtual Point? GetPosition(Element? relativeTo) =>
			_getPosition?.Invoke(relativeTo);
	}
}
