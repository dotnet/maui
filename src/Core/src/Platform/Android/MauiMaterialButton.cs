using System;
using Android.Content;
using Android.Graphics.Drawables;
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

		public MauiMaterialButton(Context context)
			: base(context)
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
				var icons = TextViewCompat.GetCompoundDrawablesRelative(this);
				if (icons[1] is { } icon)
				{
					TextViewCompat.SetCompoundDrawablesRelative(this, null, null, null, icon);
					icon.SetBounds(0, 0, icon.IntrinsicWidth, icon.IntrinsicHeight);
				}
			}
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