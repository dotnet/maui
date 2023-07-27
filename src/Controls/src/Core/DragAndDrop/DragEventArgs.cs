#nullable disable
using System;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Provides data for the <see cref="DropGestureRecognizer.DragOver"/> and <see cref="DropGestureRecognizer.DragLeave"/> events.
	/// </summary>
	public class DragEventArgs : EventArgs
	{

		/// <summary>
		/// Initializes a new instance of the <see cref="DragEventArgs"/> class.
		/// </summary>
		/// <param name="dataPackage">The data package associated with the drag source.</param>
		public DragEventArgs(DataPackage dataPackage)
		{
			Data = dataPackage;
			AcceptedOperation = DataPackageOperation.Copy;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DragEventArgs"/> class.
		/// </summary>
		/// <param name="dataPackage">The data package associated with the drag source.</param>
		/// <param name="position">The current location in the coordinate system of the drag.</param>
		public DragEventArgs(DataPackage dataPackage, Point position)
		{
			Data = dataPackage;
			Position = position;
			AcceptedOperation = DataPackageOperation.Copy;
		}

		/// <summary>
		/// Gets the data package associated with the drag source.
		/// </summary>
		public DataPackage Data { get; }

		/// <summary>
		/// Gets a value that specifies which operations are allowed by the drop target.
		/// </summary>
		public DataPackageOperation AcceptedOperation { get; set; }

		/// <summary>
		/// Gets the location of the point in coordinate system where dragging is being done.
		/// </summary>
		// TODO: JD - Confirm that it makes sense to have a private set for this property
		public Point Position { get; private set; }
	}
}
