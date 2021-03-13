using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Platform.iOS;

namespace Microsoft.Maui.Handlers
{
	public partial class LabelHandler : AbstractViewHandler<ILabel, MauiLabel>
	{
		protected override MauiLabel CreateNativeView() => new MauiLabel();

		public static void MapText(LabelHandler handler, ILabel label)
		{
			handler.TypedNativeView?.UpdateText(label);
		}

		public static void MapTextColor(LabelHandler handler, ILabel label)
		{
			handler.TypedNativeView?.UpdateTextColor(label);
		}

		public static void MapCharacterSpacing(LabelHandler handler, ILabel label)
		{
			handler.TypedNativeView?.UpdateCharacterSpacing(label);
		}

		public static void MapLineBreakMode(LabelHandler handler, ILabel label)
		{
			handler.TypedNativeView?.UpdateLineBreakMode(label);
		}

		public static void MapMaxLines(LabelHandler handler, ILabel label)
		{
			handler.TypedNativeView?.UpdateMaxLines(label);
		}

		public static void MapPadding(LabelHandler handler, ILabel label)
		{
			handler.TypedNativeView?.UpdatePadding(label);
		}

		public static void MapTextDecorations(LabelHandler handler, ILabel label)
		{
			handler.TypedNativeView?.UpdateTextDecorations(label);
		}

		public static void MapFont(LabelHandler handler, ILabel label)
		{
			var services = App.Current?.Services ??
				throw new InvalidOperationException($"Unable to find service provider, the App.Current.Services was null.");
			var fontManager = services.GetRequiredService<IFontManager>();

			handler.TypedNativeView?.UpdateFont(label, fontManager);
		}
	}
}