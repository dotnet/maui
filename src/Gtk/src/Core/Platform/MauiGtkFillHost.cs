using System;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;

namespace Microsoft.Maui.Gtk.Platform
{
	/// <summary>
	/// A panel that hosts one virtual view (a page, a flyout or a detail) and makes it fill the panel's allocation: from inside its
	/// size-allocate it measures the view with the exact allocation as constraint and arranges it over the whole area.
	/// </summary>
	/// <remarks>
	/// Navigation, tab, flyout and modal containers hold their pages in these hosts, so a page never depends on a measure that GTK ran
	/// with unconstrained sizes. The host asks for no size of its own (the container decides), like <see cref="MauiGtkRootPanel"/>.
	/// </remarks>
	public sealed class MauiGtkFillHost : MauiGtkPanel
	{
		/// <summary>The virtual view that fills this host, or <see langword="null"/>.</summary>
		public IView? View { get; private set; }

		/// <summary>Replaces the hosted view.</summary>
		/// <param name="view">The virtual view, or <see langword="null"/> to empty the host.</param>
		/// <param name="mauiContext">The context used to create the platform view.</param>
		public void SetView(IView? view, IMauiContext mauiContext)
		{
			if (mauiContext is null)
				throw new ArgumentNullException(nameof(mauiContext));

			ClearChildren();
			View = view;

			if (view is not null)
				AddChild((global::Gtk.Widget)view.ToPlatform(mauiContext));
		}

		/// <summary>Empties the host (the platform view of the hosted view stays alive, only detached).</summary>
		public void Clear()
		{
			ClearChildren();
			View = null;
		}

		/// <inheritdoc/>
		protected override void OnMeasure(global::Gtk.Orientation orientation, int forSize,
			out int minimum, out int natural, out int minimumBaseline, out int naturalBaseline)
		{
			minimum = natural = 0;
			minimumBaseline = naturalBaseline = -1;
		}

		/// <inheritdoc/>
		protected override void ArrangeChildren(int width, int height)
		{
			var view = View;
			if (view is null || width <= 0 || height <= 0)
				return;

			view.Measure(width, height);
			view.Arrange(new Rect(0, 0, width, height));
		}
	}
}
