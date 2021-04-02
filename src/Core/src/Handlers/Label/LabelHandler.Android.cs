using System;
using Android.Widget;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Maui.Handlers
{
	public partial class LabelHandler : ViewHandler<ILabel, TextView>
	{
		static Color DefaultTextColor { get; set; }
		static float LineSpacingAddDefault { get; set; }
		static float LineSpacingMultDefault { get; set; }

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
			LineSpacingAddDefault = nativeView.LineSpacingExtra;
			LineSpacingMultDefault = nativeView.LineSpacingMultiplier;
		}

		public static void MapText(LabelHandler handler, ILabel label)
		{
			handler.View?.UpdateText(label);
		}

		public static void MapTextColor(LabelHandler handler, ILabel label)
		{
			handler.View?.UpdateTextColor(label, DefaultTextColor);
		}

		public static void MapCharacterSpacing(LabelHandler handler, ILabel label)
		{
			handler.View?.UpdateCharacterSpacing(label);
		}

		public static void MapHorizontalTextAlignment(LabelHandler handler, ILabel label)
		{
			handler.View?.UpdateHorizontalTextAlignment(label);
		}

		public static void MapLineBreakMode(LabelHandler handler, ILabel label)
		{
			handler.View?.UpdateLineBreakMode(label);
		}

		public static void MapMaxLines(LabelHandler handler, ILabel label)
		{
			handler.View?.UpdateMaxLines(label);
		}

		public static void MapPadding(LabelHandler handler, ILabel label)
		{
			handler.View?.UpdatePadding(label);
		}

		public static void MapTextDecorations(LabelHandler handler, ILabel label)
		{
			handler.View?.UpdateTextDecorations(label);
		}

		public static void MapFont(LabelHandler handler, ILabel label)
		{
			_ = handler.Services ?? throw new InvalidOperationException($"{nameof(Services)} should have been set by base class.");

			var fontManager = handler.Services.GetRequiredService<IFontManager>();

			handler.View?.UpdateFont(label, fontManager);
		}
		public static void MapLineHeight(LabelHandler handler, ILabel label)
		{
			handler.View?.UpdateLineHeight(label, LineSpacingAddDefault, LineSpacingMultDefault);
		}
	}
}