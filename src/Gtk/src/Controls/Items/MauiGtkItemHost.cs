using System;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Gtk.Platform;

namespace Microsoft.Maui.Controls.Gtk.Items
{
	/// <summary>
	/// The widget of one list row (or a header/footer): a <see cref="MauiGtkFillHost"/> that, unlike a page host, <em>asks</em> for the size
	/// of its content - the row's natural width is the content's width, and its height is the content's height at the width it is given -
	/// so a <c>Gtk.ListView</c> can lay rows out by their content.
	/// </summary>
	sealed class MauiGtkItemHost : MauiGtkFillHost
	{
		/// <summary>Supplies the width every row has regardless of its content (a carousel page); <see langword="null"/> or 0 follows the content.</summary>
		public Func<int>? FixedWidth { get; set; }

		/// <summary>
		/// A fixed row height in device-independent units (<c>ListView.RowHeight</c>), or 0 or less to follow the content. It is applied by this
		/// host, never by writing <c>HeightRequest</c> into the hosted view, which belongs to the user's template.
		/// </summary>
		public double RowHeight { get; set; }

		/// <summary>The template the current view was created from; a recycled row with the same template reuses the view.</summary>
		public DataTemplate? Template { get; set; }

		/// <summary>The row kind the current view was created for.</summary>
		public GtkRowKind? Kind { get; set; }

		/// <summary>The row this host shows, or <see langword="null"/> when it shows none.</summary>
		public GtkRow? Row { get; set; }

		/// <summary>Free-form data of the owner (the legacy list keeps the cell of the row here).</summary>
		public object? Tag { get; set; }

		/// <summary>The list item that currently shows this host, for selection state.</summary>
		public global::Gtk.ListItem? ListItem { get; set; }

		/// <inheritdoc/>
		protected override void OnMeasure(global::Gtk.Orientation orientation, int forSize,
			out int minimum, out int natural, out int minimumBaseline, out int naturalBaseline)
		{
			minimumBaseline = naturalBaseline = -1;
			minimum = natural = 0;

			var view = View;
			if (view is null)
				return;

			int fixedWidth = FixedWidth?.Invoke() ?? 0;

			if (orientation == global::Gtk.Orientation.Horizontal)
			{
				if (fixedWidth > 0)
				{
					minimum = natural = fixedWidth;
					return;
				}

				var size = view.Measure(double.PositiveInfinity, double.PositiveInfinity);
				natural = GtkInterop.ToPixels(size.Width);
				return;
			}

			if (RowHeight > 0)
			{
				minimum = natural = GtkInterop.ToPixels(RowHeight);
				return;
			}

			double width = fixedWidth > 0 ? fixedWidth : forSize >= 0 ? forSize : double.PositiveInfinity;
			var measured = view.Measure(width, double.PositiveInfinity);

			// A row must keep its content's height, otherwise the list would squash it.
			minimum = natural = GtkInterop.ToPixels(measured.Height);
		}
	}
}
