#nullable disable
using System;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	public partial class Layout
	{
		LargeTitleScrollCoordinator _largeTitleScrollCoordinator;

		private protected override void OnHandlerChangingCore(HandlerChangingEventArgs args)
		{
			_largeTitleScrollCoordinator?.Reset();
			base.OnHandlerChangingCore(args);
		}

		private protected override void OnParentChangingCore(Element oldParent, Element newParent)
		{
			if (oldParent != newParent)
				_largeTitleScrollCoordinator?.Reset();
			base.OnParentChangingCore(oldParent, newParent);
		}

		void PrepareLargeTitleScroll(Rect bounds)
		{
			IView scrollHost = null;
			var count = 0;
			if (LargeTitleScrollCoordinator.IsEligible(this, bounds))
				Collect(this, true, ref scrollHost, ref count);

			if (count != 1)
			{
				_largeTitleScrollCoordinator?.Reset();
				return;
			}

			(_largeTitleScrollCoordinator ??= new()).Prepare(this, bounds, scrollHost);
		}

		internal void AdjustLargeTitleScrollBounds(IView child, ref Rect bounds) =>
			_largeTitleScrollCoordinator?.AdjustBounds(child, ref bounds);

		// Finds visible large-title scrollers reachable through nested layouts. Layouts between the
		// root and the scroller must contain nothing else, because they are stretched under the bar.
		static void Collect(IView view, bool isRoot, ref IView host, ref int count)
		{
			if (view.Visibility != Visibility.Visible || view.Opacity <= 0.01)
				return;

			if (LargeTitleScrollCoordinator.IsLargeTitleScroller(view))
			{
				count++;
				host = view;
				if (view is View control && (control.Margin.Top != 0 ||
					control.VerticalOptions.Alignment != LayoutAlignment.Fill || control.HeightRequest >= 0))
					count += 2;
			}
			else if (view is Layout layout)
			{
				var visible = 0;
				foreach (var child in layout)
				{
					if (child.Visibility == Visibility.Visible)
						visible++;
				}

				var before = count;
				foreach (var child in layout)
					Collect(child, false, ref host, ref count);

				if (!isRoot && count != before && visible > 1)
					count += 2;
				if (!isRoot && count != before && layout is ISafeAreaView2 { HasExplicitSafeAreaEdges: true })
					count += 2;
				if (!isRoot && count != before && (layout.Margin.Top != 0 || layout.Padding.Top != 0))
					count += 2;
			}
		}

		/// <summary>
		/// Maps the abstract InputTransparent property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="layout">The associated <see cref="Layout"/> instance.</param>
		[Obsolete]
		public static void MapInputTransparent(LayoutHandler handler, Layout layout) { }

		/// <summary>
		/// Maps the abstract InputTransparent property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="layout">The associated <see cref="Layout"/> instance.</param>
		[Obsolete]
		public static void MapInputTransparent(ILayoutHandler handler, Layout layout) { }
	}
}
