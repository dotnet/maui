using System;
using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Lang;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Handlers
{
	public partial class LabelHandler : ViewHandler<ILabel, TextView>
	{
		static Color? DefaultTextColor { get; set; }
		static float LineSpacingAddDefault { get; set; }
		static float LineSpacingMultDefault { get; set; }

		protected override TextView CreateNativeView() => new TextView(Context);

		protected override void SetupDefaults(TextView nativeView)
		{
			if (nativeView.TextColors == null)
			{
				DefaultTextColor = null;
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
			handler.NativeView?.UpdateText(label);
		}

		public static void MapTextColor(LabelHandler handler, ILabel label)
		{
			handler.NativeView?.UpdateTextColor(label, DefaultTextColor!);
		}

		public static void MapCharacterSpacing(LabelHandler handler, ILabel label)
		{
			handler.NativeView?.UpdateCharacterSpacing(label);
		}

		public static void MapHorizontalTextAlignment(LabelHandler handler, ILabel label)
		{
			handler.NativeView?.UpdateHorizontalTextAlignment(label);
		}

		public static void MapLineBreakMode(LabelHandler handler, ILabel label)
		{
			handler.NativeView?.UpdateLineBreakMode(label);
		}

		public static void MapMaxLines(LabelHandler handler, ILabel label)
		{
			handler.NativeView?.UpdateMaxLines(label);
		}

		public static void MapPadding(LabelHandler handler, ILabel label)
		{
			handler.NativeView?.UpdatePadding(label);
		}

		public static void MapTextDecorations(LabelHandler handler, ILabel label)
		{
			handler.NativeView?.UpdateTextDecorations(label);
		}

		public static void MapFont(LabelHandler handler, ILabel label)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();

			handler.NativeView?.UpdateFont(label, fontManager);
		}

		public static void MapLineHeight(LabelHandler handler, ILabel label)
		{
			handler.NativeView?.UpdateLineHeight(label, LineSpacingAddDefault, LineSpacingMultDefault);
		}
	}
}