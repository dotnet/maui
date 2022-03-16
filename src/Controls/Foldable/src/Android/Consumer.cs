using System;
using Microsoft.Maui.Graphics;
using Android.Runtime;
using AndroidX.Window.Layout;
using Android.App;
using Microsoft.Maui.Platform;

namespace Microsoft.Maui.Foldable
{
	class Consumer : Java.Lang.Object, AndroidX.Core.Util.IConsumer
	{
		/// <summary>
		/// reference to context that is passed via dependencyservice...
		/// </summary>
		IFoldableContext foldableInfo;
		Rect WindowBounds;

		public void SetWindowSize(Rect sizePx)
		{
			WindowBounds = sizePx;
			foldableInfo.WindowBounds = WindowBounds;
		}

		public void Accept(Java.Lang.Object windowLayoutInfo)
		{
			var newLayoutInfo = windowLayoutInfo as AndroidX.Window.Layout.WindowLayoutInfo;

			if (newLayoutInfo == null)
			{
				global::Android.Util.Log.Info("JWM", "LayoutStateChangeCallback.Accept windowLayoutInfo was NULL");
				return;
			}

			var isSeparating = false; // we don't know if we'll find a displayFeature of not
			var foldingFeatureBounds = Rect.Zero;

			foreach (var displayFeature in newLayoutInfo.DisplayFeatures)
			{
				var foldingFeature = displayFeature.JavaCast<IFoldingFeature>();

				if (foldingFeature != null) // requires JavaCast as shown above, since DisplayFeatures collection might have mulitple types
				{
					isSeparating = foldingFeature.IsSeparating;

					foldingFeatureBounds =
						new Rect(
							foldingFeature.Bounds.Left,
							foldingFeature.Bounds.Top,
							foldingFeature.Bounds.Width(),
							foldingFeature.Bounds.Height());

					global::Android.Util.Log.Info("JWM2", "\n    IsSeparating: " + foldingFeature.IsSeparating
							+ "\n    Orientation: " + foldingFeature.Orientation  // FoldingFeature.OrientationVertical or Horizontal
							+ "\n    State: " + foldingFeature.State // FoldingFeature.StateFlat or StateHalfOpened
					);
				}
				else
				{
					global::Android.Util.Log.Info("JWM2", "DisplayFeature is not a fold or hinge (could be a cut-out)");
				}
			}

			foldableInfo.IsSeparating = isSeparating;// also invokes FoldingFeatureChanged
			foldableInfo.FoldingFeatureBounds = foldingFeatureBounds;// also invokes FoldingFeatureChanged
			foldableInfo.WindowBounds = WindowBounds; // also invokes FoldingFeatureChanged
		}

		/// <summary>
		/// Make the foldableContext available to receive data when fold/posture changes
		/// </summary>
		public void SetFoldableContext(object foldableContext)
		{
			foldableInfo = foldableContext as IFoldableContext;
			if (foldableInfo is null)
			{
				throw new ArgumentNullException(nameof(foldableContext));
			}
		}
	}
}