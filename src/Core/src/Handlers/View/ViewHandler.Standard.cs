namespace Microsoft.Maui.Handlers
{
	public partial class ViewHandler
	{
		/// <summary>
		/// Maps a view's abstract <see cref="ITransform.TranslationX"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="view">The associated <see cref="IView"/> instance.</param>
		public static void MapTranslationX(IViewHandler handler, IView view) { }

		/// <summary>
		/// Maps a view's abstract <see cref="ITransform.TranslationY"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="view">The associated <see cref="IView"/> instance.</param>
		public static void MapTranslationY(IViewHandler handler, IView view) { }

		/// <summary>
		/// Maps a view's abstract <see cref="ITransform.Scale"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="view">The associated <see cref="IView"/> instance.</param>
		public static void MapScale(IViewHandler handler, IView view) { }

		/// <summary>
		/// Maps a view's abstract <see cref="ITransform.ScaleX"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="view">The associated <see cref="IView"/> instance.</param>
		public static void MapScaleX(IViewHandler handler, IView view) { }

		/// <summary>
		/// Maps a view's abstract <see cref="ITransform.ScaleY"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="view">The associated <see cref="IView"/> instance.</param>
		public static void MapScaleY(IViewHandler handler, IView view) { }

		/// <summary>
		/// Maps a view's abstract <see cref="ITransform.Rotation"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="view">The associated <see cref="IView"/> instance.</param>
		public static void MapRotation(IViewHandler handler, IView view) { }

		/// <summary>
		/// Maps a view's abstract <see cref="ITransform.RotationX"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="view">The associated <see cref="IView"/> instance.</param>
		public static void MapRotationX(IViewHandler handler, IView view) { }

		/// <summary>
		/// Maps a view's abstract <see cref="ITransform.RotationY"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="view">The associated <see cref="IView"/> instance.</param>
		public static void MapRotationY(IViewHandler handler, IView view) { }

		/// <summary>
		/// Maps a view's abstract <see cref="ITransform.AnchorX"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="view">The associated <see cref="IView"/> instance.</param>
		public static void MapAnchorX(IViewHandler handler, IView view) { }

		/// <summary>
		/// Maps a view's abstract <see cref="ITransform.AnchorY"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="view">The associated <see cref="IView"/> instance.</param>
		public static void MapAnchorY(IViewHandler handler, IView view) { }

		/// <summary>
		/// Maps the abstract <see cref="IView"/> to the platform-specific implementations of a <see cref="IContextFlyoutElement"/>.
		/// </summary>
		/// <remarks> If the <paramref name="view"/> can't be cast to a <see cref="IContextFlyoutElement"/>, this method does nothing.</remarks>
		/// <param name="handler">The associated handler.</param>
		/// <param name="view">The associated <see cref="IView"/> instance.</param>
		public static void MapContextFlyout(IViewHandler handler, IView view) { }
	}
}