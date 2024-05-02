using System;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Provides data for the <see cref="DropGestureRecognizer.DragOver"/> and <see cref="DropGestureRecognizer.DragLeave"/> events.
	/// </summary>
	public class DragEventArgs : EventArgs
	{
		Func<IElement?, Point?>? _getPosition;

		/// <summary>
		/// Initializes a new instance of the <see cref="DragEventArgs"/> class.
		/// </summary>
		/// <param name="dataPackage">The data package associated with the drag source.</param>
		public DragEventArgs(DataPackage dataPackage)
		{
			Data = dataPackage;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DragEventArgs"/> class.
		/// </summary>
		/// <param name="dataPackage">The data package associated with the drag source.</param>
		/// <param name="getPosition">The current location in the coordinate system of the drag.</param>
		/// <param name="platformArgs">The platform-specific data associated with the drag.</param>
		internal DragEventArgs(DataPackage dataPackage, Func<IElement?, Point?>? getPosition, PlatformDragEventArgs platformArgs)
		{
			Data = dataPackage;
			_getPosition = getPosition;
			PlatformArgs = platformArgs;
		}

		/// <summary>
		/// Gets the data package associated with the drag source.
		/// </summary>
		public DataPackage Data { get; }

		/// <summary>
		/// Gets or sets a value that specifies which operations are allowed by the drop target.
		/// </summary>
		public DataPackageOperation AcceptedOperation { get; set; } = DataPackageOperation.Copy;

		/// <summary>
		/// Gets the platform-specific arguments associated with the <see cref="DragEventArgs"/>.
		/// </summary>
		public PlatformDragEventArgs? PlatformArgs { get; }

		/// <summary>
		/// Gets the location of the drag relative to the specified element.
		/// </summary>
		/// <remarks>If <paramref name="relativeTo"/> is <see langword="null"/> then the position relative to the screen is returned.</remarks>
		/// <param name="relativeTo">Element whose position is used to calculate the relative position.</param>
		/// <returns>The point where dragging is occurring relative to <paramref name="relativeTo"/>.</returns>
		public virtual Point? GetPosition(Element? relativeTo) =>
			_getPosition?.Invoke(relativeTo);
	}
}
