using System;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
{
	internal class PageContainer : ViewGroup
	{
		bool _disposed;

		public PageContainer(Context context, IVisualElementRenderer child, bool inFragment = false) : base(context)
		{
			Id = Platform.GenerateViewId();
			Child = child;
			IsInFragment = inFragment;
			AddView(child.View);
		}

		public IVisualElementRenderer Child { get; set; }

		public bool IsInFragment { get; set; }

		protected PageContainer(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
		{
		}

		protected override void OnLayout(bool changed, int l, int t, int r, int b)
		{
			Child.UpdateLayout();

			if (Child.View is PageViewGroup pageViewGroup)
			{
				// This is a way to handle situations where the root page is shimmed and uses fragments to host other pages (e.g., NavigationPageRenderer)
				// The old layout system would have set the width/height of the contained Page by now; the xplat layout call would have come
				// down the tree from PlatformRenderer and Platform's OnLayout. 

				// But if we're not using PlatformRenderer at the root, then this never happens. We're relying on native OnLayout calls, which 
				// PageContainer (in its override of ViewGroup's OnLayout) _should_ be doing, under normal circumstances. So we're special-casing
				// things here for PageViewGroup (the backing View for MAUI pages) and calling Layout. We'll skip calling it for any other types,
				// because we don't want to interfere with un-shimmed legacy renderers.

				if (Child.Element.Parent is IPageController ipc && !ipc.ContainerArea.IsEmpty)
				{
					(l, t, r, b) = Context.ToPixels(ipc.ContainerArea);
				}

				var mode = MeasureSpecMode.Exactly;
				var widthSpec = mode.MakeMeasureSpec(r - l);
				var heightSpec = mode.MakeMeasureSpec(b - t);

				pageViewGroup.Measure(widthSpec, heightSpec);
				pageViewGroup.Layout(l, t, r, b);
			}
			else if (Child.View is NavigationLayout nl)
			{
				// This is a way to handle situations where the root page is shimmed and uses fragments to host other pages (e.g., NavigationPageRenderer)
				// The old layout system would have set the width/height of the contained Page by now; the xplat layout call would have come
				// down the tree from PlatformRenderer and Platform's OnLayout. 

				// But if we're not using PlatformRenderer at the root, then this never happens. We're relying on native OnLayout calls, which 
				// PageContainer (in its override of ViewGroup's OnLayout) _should_ be doing, under normal circumstances. So we're special-casing
				// things here for PageViewGroup (the backing View for MAUI pages) and calling Layout. We'll skip calling it for any other types,
				// because we don't want to interfere with un-shimmed legacy renderers.

				if (Child.Element.Parent is IPageController ipc && !ipc.ContainerArea.IsEmpty)
				{
					(l, t, r, b) = Context.ToPixels(ipc.ContainerArea);
				}

				var mode = MeasureSpecMode.Exactly;
				var widthSpec = mode.MakeMeasureSpec(r - l);
				var heightSpec = mode.MakeMeasureSpec(b - t);

				nl.Measure(widthSpec, heightSpec);
				nl.Layout(l, t, r, b);
			}
		}

		protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
		{
			Child.View.Measure(widthMeasureSpec, heightMeasureSpec);
			SetMeasuredDimension(Child.View.MeasuredWidth, Child.View.MeasuredHeight);
		}

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
			{
				return;
			}

			_disposed = true;

			if (disposing)
			{
				if (Child != null)
				{
					RemoveView(Child.View);

					Child = null;
				}
			}

			base.Dispose(disposing);
		}
	}
}