using System;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Provides data for the <see cref="DropGestureRecognizer.Drop"/> event.
	/// </summary>
	public class DropEventArgs
	{
		Func<IElement?, Point?>? _getPosition;

		/// <summary>
		/// Initializes a new instance of the <see cref="DropEventArgs"/> class.
		/// </summary>
		/// <param name="view">The data package associated with the drop.</param>
		public DropEventArgs(DataPackageView view)
		{
			_ = view ?? throw new ArgumentNullException(nameof(view));
			Data = view;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DropEventArgs"/> class.
		/// </summary>
		/// <param name="view">The data package associated with the drop.</param>
		/// <param name="getPosition">Function used to get the position relative a specified <see cref="IElement"/>.</param>
		/// <param name="platformArgs">The platform-specific data associated with the drag.</param>
		internal DropEventArgs(DataPackageView? view, Func<IElement?, Point?>? getPosition, PlatformDropEventArgs platformArgs)
		{
			Data = view ?? new DataPackageView(new DataPackage());
			_getPosition = getPosition;
			PlatformArgs = platformArgs;
		}

		/// <summary>
		/// Gets the data package associated with the drop event.
		/// </summary>
		/// <remarks>This is a read-only version of a <see cref="DataPackage"/>.</remarks>
		public DataPackageView Data { get; }


		/// <summary>
		/// Gets or sets a value that indicates whether the event handler has handled the event or whether .NET MAUI should continue its own processing.
		/// </summary>
		public bool Handled { get; set; }

		/// <summary>
		/// Gets the platform-specific arguments associated with the <see cref="DropEventArgs"/>.
		/// </summary>
		public PlatformDropEventArgs? PlatformArgs { get; }

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
