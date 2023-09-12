using System;
using System.ComponentModel;
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
		public DragStartingEventArgs()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DragStartingEventArgs"/> class.
		/// </summary>
		/// <param name="getPosition">Function used to get the position relative a specified <see cref="IElement"/>.</param>
		/// <param name="platformArgs">The platform-specific data associated with the drag.</param>
		internal DragStartingEventArgs(Func<IElement?, Point?>? getPosition, PlatformDragStartingEventArgs? platformArgs)
		{
			_getPosition = getPosition;
			PlatformArgs = platformArgs;
		}

		/// <summary>
		/// Gets or sets a value that indicates whether the event handler has handled the event or whether .NET MAUI should continue its own processing.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("Use PlatformArgs to handle customization. On Windows, set the PlatformArgs.Handled property to true if changing DragStartingEventArgs.")]
		public bool Handled { get; set; }

		/// <summary>
		/// Gets or sets a value that indicates whether the event should be canceled.
		/// </summary>
		public bool Cancel { get; set; }

		/// <summary>
		/// Gets the platform-specific arguments associated with the <see cref="DragStartingEventArgs"/>.
		/// </summary>
		public PlatformDragStartingEventArgs? PlatformArgs { get; }

		/// <summary>
		/// Gets the data package that accompanies the drag source.
		/// </summary>
		public DataPackage Data { get; } = new DataPackage();

		/// <summary>
		/// Gets the location where dragging started relative to the specified element.
		/// </summary>
		/// <remarks>If <paramref name="relativeTo"/> is <see langword="null"/> then the position relative to the screen is returned.</remarks>
		/// <param name="relativeTo">Element whose position is used to calculate the relative position.</param>
		/// <returns>The point where dragging started relative to <paramref name="relativeTo"/>.</returns>
		public virtual Point? GetPosition(Element? relativeTo) =>
			_getPosition?.Invoke(relativeTo);
	}
}
