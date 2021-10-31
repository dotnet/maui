#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.Graphics.Drawables;
using Android.Views;
using Android.Widget;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	public partial class HighlightLayer
	{
		global::Android.App.Activity? _activity;
		HashSet<AndroidView> Views = new HashSet<AndroidView>();

		public bool AddHighlight(Maui.IView view)
		{
			if (this._activity == null)
			{
				var window = this.Window as Window;
				if (window != null)
					this._activity = window.NativeActivity;
			}

			var nativeView = view.GetNative(true);
			if (nativeView == null)
				return false;
			var highlightedView = nativeView;
			var highlightedViewOriginalBackground = highlightedView.Background;
			if (highlightedView == null)
				return false;
			var aview = new AndroidView(view, highlightedView, highlightedViewOriginalBackground);
			this.SetColorOnNativeView(aview, true);
			return this.Views.Add(aview);
		}

		public bool RemoveHighlight(Maui.IView view)
		{
			var aview = this.Views.FirstOrDefault(n => n.view == view);
			if (aview != null)
			{
				this.SetColorOnNativeView(aview, false);
				return this.Views.Remove(aview);
			}
			return false;
		}

		public void ClearHighlights()
		{
			foreach (var view in Views)
			{
				this.SetColorOnNativeView(view, false);
			}
			this.Views.Clear();
		}

		private void SetColorOnNativeView(AndroidView view, bool highlightColor)
		{
			var drawable = !highlightColor ? view.highlightedViewOriginalBackground : GetHighlightBackground(view.highlightedViewOriginalBackground);
#pragma warning disable CS0618 // Type or member is obsolete
			this._activity?.RunOnUiThread(() => view.highlightedView.SetBackgroundDrawable(drawable));
#pragma warning restore CS0618 // Type or member is obsolete
		}

		private Drawable GetHighlightBackground(Drawable? highlightedViewOriginalBackground)
		{
			var gd = new GradientDrawable();
			gd.SetColor(Android.Graphics.Color.Red);
			gd.SetAlpha(255 / 2);

			Drawable highlightedBackground;
			if (highlightedViewOriginalBackground == null)
				highlightedBackground = gd;
			else
				highlightedBackground =
					new LayerDrawable(new[] { highlightedViewOriginalBackground, gd });
			return highlightedBackground;
		}

		internal class AndroidView
		{
			public Maui.IView view;
			public Android.Views.View highlightedView;
			public Drawable? highlightedViewOriginalBackground;

			public AndroidView(Maui.IView view, Android.Views.View aview, Drawable? background)
			{
				this.view = view;
				this.highlightedView = aview;
				this.highlightedViewOriginalBackground = background;
			}
		}
	}
}
