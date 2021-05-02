using Gtk;

namespace Microsoft.Maui.Handlers
{

	public partial class LabelHandler : ViewHandler<ILabel, Label>
	{

		// https://developer.gnome.org/gtk3/stable/GtkLabel.html
		protected override Label CreateNativeView()
		{
			return new Label();
		}

		public static void MapText(LabelHandler handler, ILabel label)
		{
			handler.NativeView?.UpdateText(label);
		}

		public static void MapTextColor(LabelHandler handler, ILabel label)
		{
			handler.NativeView?.UpdateTextColor(label.TextColor);
		}

		[MissingMapper]
		public static void MapCharacterSpacing(LabelHandler handler, ILabel label)
		{ }

		public static void MapFont(LabelHandler handler, ILabel label)
		{
			handler.MapFont(label);
		}

		public static void MapHorizontalTextAlignment(LabelHandler handler, ILabel label)
		{
			handler.NativeView?.UpdateTextAlignment(label);
		}

		public static void MapLineBreakMode(LabelHandler handler, ILabel label)
		{
			handler.NativeView?.UpdateLineBreakMode(label);
		}

		[MissingMapper]
		public static void MapTextDecorations(LabelHandler handler, ILabel label) { }

		[MissingMapper]
		public static void MapMaxLines(LabelHandler handler, ILabel label) { }

		public static void MapPadding(LabelHandler handler, ILabel label)
		{
			handler.NativeView.WithMargin(label.Padding);

		}

		[MissingMapper]
		public static void MapLineHeight(LabelHandler handler, ILabel label) { }

	}

}