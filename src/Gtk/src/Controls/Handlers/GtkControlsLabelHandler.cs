using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls.Gtk.Platform;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Gtk.Fonts;
using Microsoft.Maui.Gtk.Handlers;
using Microsoft.Maui.Gtk.Platform;
using Microsoft.Maui.Handlers;
using GtkLabel = Gtk.Label;

namespace Microsoft.Maui.Controls.Gtk.Handlers
{
	/// <summary>
	/// The label handler for Controls' <see cref="Label"/>: <see cref="GtkLabelHandler"/> plus the Controls-level text features that
	/// <c>ILabel</c> does not carry: <see cref="Label.FormattedText"/> (Pango markup with one <c>span</c> per <see cref="Span"/>),
	/// <see cref="TextType.Html"/>, <see cref="Label.TextTransform"/>, <see cref="Label.LineBreakMode"/> and <see cref="Label.MaxLines"/>.
	/// </summary>
	/// <remarks>
	/// <para>These mappings run after the Core mapping of the same key, so they replace the plain text it set. Colour, font, character
	/// spacing, decorations and line height of the label stay CSS on the widget (<c>LineHeight</c> is Core's); spans add discrete Pango
	/// attributes only for what they set.</para>
	/// <para>Tap recognizers of a <see cref="Span"/> are hit-tested through the Pango layout (<c>GtkLabelSpanGestures</c>), honour
	/// <c>Buttons</c> and <c>NumberOfTapsRequired</c>, can be added and removed at run time, and <c>Span.Region</c> is filled. A tap on a span
	/// does not also fire the label's own tap recognizers. Other recognizer kinds on a span are not supported.</para>
	/// </remarks>
	public class GtkControlsLabelHandler : GtkLabelHandler
	{
		/// <summary>Creates the handler with the Controls mappers.</summary>
		public GtkControlsLabelHandler()
			: base(ControlsMapper, null)
		{
		}

		/// <summary>Creates the handler with custom mappers.</summary>
		/// <param name="mapper">The property mapper, or <see langword="null"/> for the Controls one.</param>
		/// <param name="commandMapper">The command mapper, or <see langword="null"/> for the stock one.</param>
		public GtkControlsLabelHandler(IPropertyMapper? mapper, CommandMapper? commandMapper = null)
			: base(mapper ?? ControlsMapper, commandMapper)
		{
		}

		/// <inheritdoc/>
		protected override void DisconnectHandler(object platformView)
		{
			if (platformView is GtkLabel widget)
				GtkLabelSpanGestures.For(widget)?.Dispose();
			base.DisconnectHandler(platformView);
		}

		static void UpdateText(IElementHandler handler, Label label, GtkLabel widget)
		{
			var gestures = GtkLabelSpanGestures.For(widget) ?? new GtkLabelSpanGestures(widget, label);
			if (label.HasFormattedTextSpans)
			{
				var fonts = GtkTextStyle.GetFontManager(handler) as GtkFontManager;
				var ranges = new List<GtkLabelMarkup.SpanRange>();
				widget.Markup = GtkLabelMarkup.FromFormattedText(label, fonts is null ? null : family => fonts.GetFontFamily(family), ranges);
				gestures.Update(ranges);
				return;
			}

			gestures.Update(null);
			if (label.TextType == TextType.Html)
			{
				widget.Markup = GtkLabelMarkup.FromHtml(label.Text);
			}
			else
			{
				widget.Text = TextTransformUtilities.GetTransformedText(label.Text ?? string.Empty, label.TextTransform);
			}
		}

		static void UpdateLineBreak(GtkLabel widget, Label label)
		{
			var mode = label.LineBreakMode;
			int max = label.MaxLines > 0 ? label.MaxLines : 0;
			bool truncates = mode is LineBreakMode.HeadTruncation or LineBreakMode.MiddleTruncation or LineBreakMode.TailTruncation;

			// A truncating label is one line unless MaxLines asks for more.
			widget.Wrap = mode is LineBreakMode.WordWrap or LineBreakMode.CharacterWrap || (truncates && max > 1);
			widget.WrapMode = mode == LineBreakMode.CharacterWrap ? Pango.WrapMode.Char : Pango.WrapMode.WordChar;
			widget.Ellipsize = mode switch
			{
				LineBreakMode.HeadTruncation => Pango.EllipsizeMode.Start,
				LineBreakMode.MiddleTruncation => Pango.EllipsizeMode.Middle,
				LineBreakMode.TailTruncation => Pango.EllipsizeMode.End,
				_ => max > 0 ? Pango.EllipsizeMode.End : Pango.EllipsizeMode.None,
			};
			widget.Lines = max;
		}

		static void Map(PropertyMapper<ILabel, ILabelHandler> mapper, IPropertyMapper<ILabel, ILabelHandler> previous, string key, Action<ILabelHandler, Label, GtkLabel> action)
		{
			mapper.Add(key, (handler, view) =>
			{
				previous.GetProperty(key)?.Invoke(handler, view);

				if (view is Label label && handler.PlatformView is GtkLabel widget)
					action(handler, label, widget);
			});
		}

		internal static readonly IPropertyMapper<ILabel, ILabelHandler> ControlsMapper = CreateMapper();

		static IPropertyMapper<ILabel, ILabelHandler> CreateMapper()
		{
			var previous = GtkLabelHandler.GtkMapper;
			var mapper = new PropertyMapper<ILabel, ILabelHandler>(previous);

			Map(mapper, previous, nameof(ILabel.Text), UpdateText);
			Map(mapper, previous, nameof(Label.FormattedText), UpdateText);
			Map(mapper, previous, nameof(Label.LineBreakMode), (h, label, widget) => UpdateLineBreak(widget, label));
			Map(mapper, previous, nameof(Label.MaxLines), (h, label, widget) => UpdateLineBreak(widget, label));

			return mapper;
		}
	}
}
