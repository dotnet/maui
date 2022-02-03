namespace Microsoft.Maui.Platform
{
	public static partial class ViewExtensions
	{
		public static void UpdateIsEnabled(this object nativeView, IView view) { }

		public static void UpdateVisibility(this object nativeView, IView view) { }

		public static void UpdateBackground(this object nativeView, IView view) { }

		public static void UpdateClipsToBounds(this object nativeView, IView view) { }

		public static void UpdateAutomationId(this object nativeView, IView view) { }

		public static void UpdateClip(this object nativeView, IView view) { }

		public static void UpdateShadow(this object nativeView, IView view) { }
		public static void UpdateBorder(this object nativeView, IView view) { }

		public static void UpdateOpacity(this object nativeView, IView view) { }

		public static void UpdateSemantics(this object nativeView, IView view) { }

		public static void UpdateFlowDirection(this object nativeView, IView view) { }

		public static void UpdateTranslationX(this object nativeView, IView view) { }

		public static void UpdateTranslationY(this object nativeView, IView view) { }

		public static void UpdateScale(this object nativeView, IView view) { }

		public static void UpdateRotation(this object nativeView, IView view) { }

		public static void UpdateRotationX(this object nativeView, IView view) { }

		public static void UpdateRotationY(this object nativeView, IView view) { }

		public static void UpdateAnchorX(this object nativeView, IView view) { }

		public static void UpdateAnchorY(this object nativeView, IView view) { }

		public static void InvalidateMeasure(this object nativeView, IView view) { }

		public static void UpdateWidth(this object nativeView, IView view) { }

		public static void UpdateHeight(this object nativeView, IView view) { }

		public static void UpdateMinimumHeight(this object nativeView, IView view) { }

		public static void UpdateMaximumHeight(this object nativeView, IView view) { }

		public static void UpdateMinimumWidth(this object nativeView, IView view) { }

		public static void UpdateMaximumWidth(this object nativeView, IView view) { }

		public static System.Threading.Tasks.Task<byte[]?> RenderAsPNG(this IView view)
			=> System.Threading.Tasks.Task.FromResult<byte[]?>(null);

		public static System.Threading.Tasks.Task<byte[]?> RenderAsJPEG(this IView view)
			=> System.Threading.Tasks.Task.FromResult<byte[]?>(null);
		internal static Graphics.Rectangle GetPlatformViewBounds(this IView view) => view.Frame;

		internal static System.Numerics.Matrix4x4 GetViewTransform(this IView view) => new System.Numerics.Matrix4x4();

		internal static Graphics.Rectangle GetBoundingBox(this IView view) => view.Frame;

		internal static object? GetParent(this object? view)
		{
			return null;
		}
	}
}
