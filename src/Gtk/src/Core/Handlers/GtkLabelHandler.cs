using System;
using Microsoft.Maui.Gtk.Hosting;
using Microsoft.Maui.Gtk.Platform;
using Microsoft.Maui.Handlers;
using GtkLabel = Gtk.Label;

namespace Microsoft.Maui.Gtk.Handlers
{
	/// <summary>
	/// <see cref="LabelHandler"/> over a <c>Gtk.Label</c>. Colour, font, letter spacing, decorations, line height and padding
	/// are CSS (see <see cref="GtkTextStyle"/>); wrapping is on (MAUI's default <c>WordWrap</c>); see <see cref="SetMaxLines"/> for line limits.
	/// </summary>
	public class GtkLabelHandler : LabelHandler
	{
		/// <summary>Creates the handler with the stock mappers.</summary>
		public GtkLabelHandler()
		{
		}

		/// <summary>Creates the handler with custom mappers.</summary>
		/// <param name="mapper">The property mapper, or <see langword="null"/> for the stock one.</param>
		/// <param name="commandMapper">The command mapper, or <see langword="null"/> for the stock one.</param>
		public GtkLabelHandler(IPropertyMapper? mapper, CommandMapper? commandMapper = null)
			: base(mapper, commandMapper)
		{
		}

		/// <inheritdoc/>
		protected override object CreatePlatformView() => new GtkLabel
		{
			Wrap = true,
			WrapMode = Pango.WrapMode.WordChar,
			Xalign = 0,
			Yalign = 0.5f,
		};

		internal static void ApplyMappers()
		{
			var mapper = Mapper;

			mapper.MapGtk<ILabel, ILabelHandler, GtkLabel>(nameof(ITextStyle.TextColor), (h, v, w) => GtkTextStyle.SetColor(w, v.TextColor));
			mapper.MapGtk<ILabel, ILabelHandler, GtkLabel>(nameof(ILabel.Text), (h, v, w) => w.Text = v.Text ?? string.Empty);
			mapper.MapGtk<ILabel, ILabelHandler, GtkLabel>(nameof(ITextStyle.CharacterSpacing), (h, v, w) => GtkTextStyle.SetCharacterSpacing(w, v.CharacterSpacing));
			mapper.MapGtk<ILabel, ILabelHandler, GtkLabel>(nameof(ITextStyle.Font), (h, v, w) => GtkTextStyle.SetFont(w, v.Font, GtkTextStyle.GetFontManager(h)));
			mapper.MapGtk<ILabel, ILabelHandler, GtkLabel>(nameof(ITextAlignment.HorizontalTextAlignment), (h, v, w) => UpdateHorizontalAlignment(w, v));
			mapper.MapGtk<ILabel, ILabelHandler, GtkLabel>(nameof(ITextAlignment.VerticalTextAlignment), (h, v, w) => UpdateVerticalAlignment(w, v));
			mapper.MapGtk<ILabel, ILabelHandler, GtkLabel>(nameof(ILabel.TextDecorations), (h, v, w) => GtkTextStyle.SetTextDecorations(w, v.TextDecorations));
			mapper.MapGtk<ILabel, ILabelHandler, GtkLabel>(nameof(ILabel.Padding), (h, v, w) => GtkTextStyle.SetPadding(w, v.Padding));
			mapper.MapGtk<ILabel, ILabelHandler, GtkLabel>(nameof(ILabel.LineHeight), (h, v, w) =>
				GtkStyle.For(w).Set("line-height", v.LineHeight >= 0 ? v.LineHeight.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture) : null));
		}

		static void UpdateHorizontalAlignment(GtkLabel label, ILabel view)
		{
			var alignment = view.HorizontalTextAlignment;
			label.Xalign = GtkTextStyle.ToAlignment(alignment, view.FlowDirection);
			label.Justify = alignment switch
			{
				TextAlignment.Center => global::Gtk.Justification.Center,
				TextAlignment.End => view.FlowDirection == FlowDirection.RightToLeft ? global::Gtk.Justification.Left : global::Gtk.Justification.Right,
				_ => view.FlowDirection == FlowDirection.RightToLeft ? global::Gtk.Justification.Right : global::Gtk.Justification.Left,
			};
		}

		static void UpdateVerticalAlignment(GtkLabel label, ILabel view) =>
			label.Yalign = view.VerticalTextAlignment switch
			{
				TextAlignment.Start => 0f,
				TextAlignment.End => 1f,
				_ => 0.5f,
			};

		/// <summary>Limits the label to <paramref name="maxLines"/> lines (ellipsised); values below 1 remove the limit.</summary>
		/// <remarks><c>MaxLines</c> is not part of <see cref="ILabel"/>; the Controls backend calls this from its own mapping.</remarks>
		/// <param name="label">The label.</param>
		/// <param name="maxLines">The maximum number of lines.</param>
		public static void SetMaxLines(GtkLabel label, int maxLines)
		{
			label.Lines = maxLines > 0 ? maxLines : 0;
			label.Ellipsize = maxLines > 0 ? Pango.EllipsizeMode.End : Pango.EllipsizeMode.None;
		}
	}
}
