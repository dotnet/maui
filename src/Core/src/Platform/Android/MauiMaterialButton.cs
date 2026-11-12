using System;
using Android.Content;
using Android.Graphics.Drawables;
using Android.Runtime;
using Android.Util;
using Android.Views;
using AndroidX.Core.Widget;
using Google.Android.Material.Button;

namespace Microsoft.Maui.Platform
{
	public class MauiMaterialButton : MaterialButton
	{
		// The default MaterialButton currently does not have a concept of bottom
		// gravity which we need for .NET MAUI.
		// In order to get this feature, we have added a custom gravity option
		// that serves as a flag to indicate that the icon should be placed at
		// the bottom.
		// The real gravity value is IconGravityTop in order to perform all the
		// normal layout calculations. We then set ForceBottomIconGravity for our
		// custom layout pass where we simply swap the icon from the top to the
		// bottom using SetCompoundDrawablesRelative.
		internal const int IconGravityBottom = 0x1000;
		global::Android.Graphics.Rect? _alignmentIconBounds;

		internal bool AlignTextAndImage { get; set; }

		public MauiMaterialButton(Context context)
			: base(MauiMaterialContextThemeWrapper.Create(context))
		{
		}

		protected MauiMaterialButton(nint javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
		{
		}

		public MauiMaterialButton(Context context, IAttributeSet? attrs) : base(MauiMaterialContextThemeWrapper.Create(context), attrs)
		{
		}

		public MauiMaterialButton(Context context, IAttributeSet? attrs, int defStyleAttr) : base(MauiMaterialContextThemeWrapper.Create(context), attrs, defStyleAttr)
		{
		}

		public override int IconGravity
		{
			get => base.IconGravity;
			set
			{
				// For IconGravityTextEnd and IconGravityTextStart, setting the Icon twice
				// is needed to work around the Android behavior that caused
				// https://github.com/dotnet/maui/issues/11755
				Drawable? savedIcon = null;
				if (base.IconGravity != value && (savedIcon = Icon) is not null)
				{
					Icon = null;
				}

				// Intercept the gravity value and set the flag if it's bottom.
				ForceBottomIconGravity = value == IconGravityBottom;
				base.IconGravity = ForceBottomIconGravity ? IconGravityTop : value;

				if (savedIcon is not null)
				{
					Icon = savedIcon;
				}
			}
		}

		internal bool ForceBottomIconGravity { get; private set; }

		protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
		{
			// calculate the icon size based on the remaining space
			CalculateIconSize(widthMeasureSpec, heightMeasureSpec);

			// re-measure with both text and icon
			base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
		}

		protected override void OnLayout(bool changed, int left, int top, int right, int bottom)
		{
			base.OnLayout(changed, left, top, right, bottom);

			// After the layout pass, we swap the icon from the top to the bottom.
			if (ForceBottomIconGravity)
			{
#pragma warning disable CS0618 // Type or member is obsolete
				var icons = TextViewCompat.GetCompoundDrawablesRelative(this);
#pragma warning restore CS0618 // Type or member is obsolete
				if (icons?[1] is { } icon)
				{
#pragma warning disable CS0618 // Type or member is obsolete
					TextViewCompat.SetCompoundDrawablesRelative(this, null, null, null, icon);
#pragma warning restore CS0618 // Type or member is obsolete
					icon.SetBounds(0, 0, icon.IntrinsicWidth, icon.IntrinsicHeight);
				}
			}
		}

		protected override void OnDraw(global::Android.Graphics.Canvas canvas)
		{
			if (!AlignTextAndImage || Icon is not { } icon || Layout is not { } layout || Length() == 0)
			{
				base.OnDraw(canvas);
				return;
			}

			// TextView anchors compound images separately from text. Move the image next to the
			// actual text block, then translate both together without moving the button background.
			var textLeft = float.MaxValue;
			var textRight = float.MinValue;
			for (var line = 0; line < layout.LineCount; line++)
			{
				textLeft = Math.Min(textLeft, layout.GetLineLeft(line));
				textRight = Math.Max(textRight, layout.GetLineRight(line));
			}
			textLeft += CompoundPaddingLeft;
			textRight += CompoundPaddingLeft;
			var textHeight = layout.Height;
			var verticalSpace = Math.Max(0, Height - ExtendedPaddingTop - ExtendedPaddingBottom - textHeight);
			var verticalGravity = Gravity & GravityFlags.VerticalGravityMask;
			var textTop = ExtendedPaddingTop + (verticalGravity == GravityFlags.Top ? 0
				: verticalGravity == GravityFlags.Bottom ? verticalSpace : verticalSpace / 2f);

			var imageWidth = IconSize > 0 ? IconSize : icon.IntrinsicWidth;
			var imageHeight = IconSize > 0 ? IconSize : icon.IntrinsicHeight;
			float imageLeft;
			float imageTop;
			float nativeImageLeft;
			float nativeImageTop;
			if (IsIconGravityHorizontal)
			{
				var imageOnLeft = (IconGravity is IconGravityStart or IconGravityTextStart) != (LayoutDirection == global::Android.Views.LayoutDirection.Rtl);
				imageLeft = imageOnLeft ? textLeft - IconPadding - imageWidth : textRight + IconPadding;
				imageTop = textTop + (textHeight - imageHeight) / 2f;
				nativeImageLeft = imageOnLeft ? PaddingLeft : Width - PaddingRight - imageWidth;
				nativeImageTop = CompoundPaddingTop + (Height - CompoundPaddingTop - CompoundPaddingBottom - imageHeight) / 2f;
			}
			else
			{
				imageLeft = (textLeft + textRight - imageWidth) / 2f;
				imageTop = ForceBottomIconGravity ? textTop + textHeight + IconPadding : textTop - IconPadding - imageHeight;
				nativeImageLeft = CompoundPaddingLeft + (Width - CompoundPaddingLeft - CompoundPaddingRight - imageWidth) / 2f;
				nativeImageTop = ForceBottomIconGravity ? Height - PaddingBottom - imageHeight : PaddingTop;
			}

			var groupLeft = Math.Min(textLeft, imageLeft);
			var groupTop = Math.Min(textTop, imageTop);
			var groupWidth = Math.Max(textRight, imageLeft + imageWidth) - groupLeft;
			var groupHeight = Math.Max(textTop + textHeight, imageTop + imageHeight) - groupTop;
			var horizontalSpace = Math.Max(0, Width - PaddingLeft - PaddingRight - groupWidth);
			var groupVerticalSpace = Math.Max(0, Height - PaddingTop - PaddingBottom - groupHeight);
			var horizontalGravity = TextAlignment switch
			{
				global::Android.Views.TextAlignment.ViewStart => GravityFlags.Start,
				global::Android.Views.TextAlignment.ViewEnd => GravityFlags.End,
				global::Android.Views.TextAlignment.Center => GravityFlags.CenterHorizontal,
				global::Android.Views.TextAlignment.TextStart => layout.GetParagraphDirection(0) == global::Android.Text.TextLayoutDirection.LeftToRight ? GravityFlags.Left : GravityFlags.Right,
				global::Android.Views.TextAlignment.TextEnd => layout.GetParagraphDirection(0) == global::Android.Text.TextLayoutDirection.LeftToRight ? GravityFlags.Right : GravityFlags.Left,
				_ => Gravity & (GravityFlags.HorizontalGravityMask | GravityFlags.RelativeLayoutDirection)
			};
			if ((horizontalGravity & GravityFlags.RelativeLayoutDirection) != 0 && LayoutDirection == global::Android.Views.LayoutDirection.Rtl)
			{
				horizontalGravity &= GravityFlags.HorizontalGravityMask;
				horizontalGravity = horizontalGravity == GravityFlags.Left ? GravityFlags.Right
					: horizontalGravity == GravityFlags.Right ? GravityFlags.Left : horizontalGravity;
			}
			else
				horizontalGravity &= GravityFlags.HorizontalGravityMask;
			var targetLeft = PaddingLeft + (horizontalGravity == GravityFlags.Left ? 0
				: horizontalGravity == GravityFlags.Right ? horizontalSpace : horizontalSpace / 2f);
			var targetTop = PaddingTop + (verticalGravity == GravityFlags.Top ? 0
				: verticalGravity == GravityFlags.Bottom ? groupVerticalSpace : groupVerticalSpace / 2f);

			var bounds = _alignmentIconBounds ??= new global::Android.Graphics.Rect();
			icon.CopyBounds(bounds);
			var left = (int)Math.Round(imageLeft - nativeImageLeft);
			var top = (int)Math.Round(imageTop - nativeImageTop);
			icon.SetBounds(left, top, left + imageWidth, top + imageHeight);
			var saveCount = canvas.Save();
			try
			{
				canvas.Translate(targetLeft - groupLeft, targetTop - groupTop);
				base.OnDraw(canvas);
			}
			finally
			{
				canvas.RestoreToCount(saveCount);
				icon.SetBounds(bounds.Left, bounds.Top, bounds.Right, bounds.Bottom);
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_alignmentIconBounds?.Dispose();
				_alignmentIconBounds = null;
			}
			base.Dispose(disposing);
		}

		void CalculateIconSize(int widthMeasureSpec, int heightMeasureSpec)
		{
			if (Icon is null)
				return;

			Drawable actual;

			if (Icon is MauiResizableDrawable resizableDrawable)
			{
				actual = resizableDrawable.Drawable;
			}
			else
			{
				actual = Icon;
			}

			// determine the total client area available for BOTH the icon AND text to fit
			var availableWidth = MeasureSpec.GetMode(widthMeasureSpec) == MeasureSpecMode.Unspecified
				? int.MaxValue
				: MeasureSpec.GetSize(widthMeasureSpec);
			var availableHeight = MeasureSpec.GetMode(heightMeasureSpec) == MeasureSpecMode.Unspecified
				? int.MaxValue
				: MeasureSpec.GetSize(heightMeasureSpec);

			var remainingWidth = availableWidth - PaddingLeft - PaddingRight;
			var remainingHeight = availableHeight - PaddingTop - PaddingBottom;

			if (IsIconGravityHorizontal)
			{
				remainingWidth -= IconPadding;
			}
			else
			{
				remainingHeight -= IconPadding;
			}

			var iconWidth = Math.Min(remainingWidth, actual.IntrinsicWidth);
			var iconHeight = Math.Min(remainingHeight, actual.IntrinsicHeight);

			// We don't use IconSize because IconSize makes everything a square
			// So if the image doesn't have equal width and height it'll distory the image
			if (OperatingSystem.IsAndroidVersionAtLeast(23) && Icon is MauiResizableDrawable resizable)
			{
				var ratio = Math.Min(
					(double)iconWidth / actual.IntrinsicWidth,
					(double)iconHeight / actual.IntrinsicHeight);

				if (resizable.SetPreferredSize(
					Math.Max(0, (int)(actual.IntrinsicWidth * ratio)),
					Math.Max(0, (int)(actual.IntrinsicHeight * ratio))))
				{
					// trigger a layout re-calculation
					Icon = null;
					Icon = resizable;
				}
			}
			else
			{
				IconSize = Math.Max(iconWidth, iconHeight);
			}
		}

		bool IsIconGravityHorizontal =>
			IconGravity is IconGravityTextStart or IconGravityTextEnd or IconGravityStart or IconGravityEnd;

		internal class MauiResizableDrawable : LayerDrawable
		{
			public MauiResizableDrawable(Drawable drawable)
				: base([drawable])
			{
				PaddingMode = (int)LayerDrawablePaddingMode.Stack;
				if (OperatingSystem.IsAndroidVersionAtLeast(23))
				{
					SetLayerSize(0, drawable.IntrinsicWidth, drawable.IntrinsicHeight);
				}
			}

			public Drawable Drawable => GetDrawable(0)!;

			public bool SetPreferredSize(int width, int height)
			{
				if (OperatingSystem.IsAndroidVersionAtLeast(23))
				{
					if (NumberOfLayers > 0 && GetLayerWidth(0) == width && GetLayerHeight(0) == height)
					{
						return false;
					}

					SetLayerSize(0, width, height);
					return true;
				}

				return false;

				// TODO: find something that works for older versions
			}
		}
	}
}