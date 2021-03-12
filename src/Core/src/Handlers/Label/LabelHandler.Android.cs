using System;
using Android.Widget;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Maui.Handlers
{
	public partial class LabelHandler : AbstractViewHandler<ILabel, TextView>
	{
		static Color DefaultTextColor { get; set; }

		protected override TextView CreateNativeView() => new TextView(Context);

		protected override void SetupDefaults(TextView nativeView)
		{
			if (nativeView.TextColors == null)
			{
				DefaultTextColor = Color.Default;
			}
			else
			{
				DefaultTextColor = Color.FromUint((uint)nativeView.TextColors.DefaultColor);
			}
		}

		public static void MapText(LabelHandler handler, ILabel label)
		{
			handler.TypedNativeView?.UpdateText(label);
		}

		public static void MapTextColor(LabelHandler handler, ILabel label)
		{
			handler.TypedNativeView?.UpdateTextColor(label, DefaultTextColor);
		}

		public static void MapCharacterSpacing(LabelHandler handler, ILabel label)
		{
			handler.TypedNativeView?.UpdateCharacterSpacing(label);
		}

		public static void MapHorizontalTextAlignment(LabelHandler handler, ILabel label)
		{
			handler.TypedNativeView?.UpdateHorizontalTextAlignment(label);
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
			var services = App.Current?.Services
				?? throw new InvalidOperationException($"Unable to find service provider, the App.Current.Services was null.");
			var fontManager = services.GetRequiredService<IFontManager>();

			handler.TypedNativeView?.UpdateFont(label, fontManager);
		}
	}
}