#nullable disable
using System;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Provides data for the <see cref="DragGestureRecognizer.DragStarting"/> event.
	/// </summary>
	public class DragStartingEventArgs : EventArgs
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="DragStartingEventArgs"/> class.
		/// </summary>
		// TODO: JD - Maybe mark this as obsolete? 
		public DragStartingEventArgs()
		{
			Data = new DataPackage();
			Position = Point.Zero;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DragStartingEventArgs"/> class.
		/// </summary>
		/// <param name="position">The location in the coordinate system where the drag started.</param>
		public DragStartingEventArgs(Point position)
		{
			Data = new DataPackage();
			Position = position;
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
		public DataPackage Data { get; private set; }

		/// <summary>
		/// Gets the location of the point in the coordinate system where dragging started.
		/// </summary>
		// TODO: JD - Confirm that it makes sense to have a private set for this property
		public Point Position { get; private set; }
	}
}
