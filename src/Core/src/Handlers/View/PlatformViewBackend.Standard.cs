using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Handlers
{
	/// <summary>
	/// Platform services that a backend running on the platform-less (<c>net10.0</c>) assemblies supplies
	/// for every <see cref="ViewHandler{TVirtualView, TPlatformView}"/>: measuring, arranging and wrapping
	/// the platform view in a container. Without a backend those operations are no-ops.
	/// </summary>
	/// <remarks>
	/// Used by out-of-tree platform backends (for example the GTK 4 backend in <c>src/Gtk</c>), which subclass the
	/// stock handlers and create real platform views from <c>CreatePlatformView</c>.
	/// </remarks>
	internal interface IPlatformViewBackend
	{
		/// <summary>Measures <paramref name="handler"/>'s platform view within the given constraints.</summary>
		Size GetDesiredSize(IViewHandler handler, double widthConstraint, double heightConstraint);

		/// <summary>Positions <paramref name="handler"/>'s platform view at <paramref name="frame"/>, relative to its parent.</summary>
		void PlatformArrange(IViewHandler handler, Rect frame);

		/// <summary>Wraps the platform view in a container; set it with <see cref="ViewHandler.SetContainerView(object?)"/>.</summary>
		void SetupContainer(IViewHandler handler);

		/// <summary>Unwraps the platform view from its container and clears <see cref="IViewHandler.ContainerView"/>.</summary>
		void RemoveContainer(IViewHandler handler);
	}

	/// <summary>Holds the process-wide <see cref="IPlatformViewBackend"/>.</summary>
	internal static class PlatformViewBackend
	{
		/// <summary>
		/// The active backend, or <see langword="null"/> (the default) for the no-op behaviour of the platform-less assemblies.
		/// </summary>
		public static IPlatformViewBackend? Current { get; set; }
	}
}
