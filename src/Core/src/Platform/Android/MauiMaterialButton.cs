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
				// Intercept the gravity value and set the flag if it's bottom.
				ForceBottomIconGravity = value == IconGravityBottom;
				base.IconGravity = ForceBottomIconGravity ? IconGravityTop : value;
			}
		}

		internal bool ForceBottomIconGravity { get; private set; }

		protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
		{
			if (Icon is MauiResizableDrawable currentIcon)
			{
				// if there is BOTH an icon AND text, but the text layout has NOT been measured yet,
				// we need to measure JUST the text first to get the remaining space for the icon
				if (Layout is null && TextFormatted?.Length() > 0)
				{
					// remove the icon and measure JUST the text
					Icon = null;
					base.OnMeasure(widthMeasureSpec, heightMeasureSpec);

					// restore the icon
					Icon = currentIcon;
				}

				// determine the total client area available for BOTH the icon AND text to fit
				var availableWidth = MeasureSpec.GetMode(widthMeasureSpec) == MeasureSpecMode.Unspecified
					? int.MaxValue
					: MeasureSpec.GetSize(widthMeasureSpec);
				var availableHeight = MeasureSpec.GetMode(heightMeasureSpec) == MeasureSpecMode.Unspecified
					? int.MaxValue
					: MeasureSpec.GetSize(heightMeasureSpec);

				// calculate the icon size based on the remaining space
				CalculateIconSize(currentIcon, availableWidth, availableHeight);
			}

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

		void CalculateIconSize(MauiResizableDrawable resizable, int availableWidth, int availableHeight)
		{
			// bail if the text layout is not available yet, this is most likely a bug
			if (Layout is null)
			{
				return;
			}

			var actual = resizable.Drawable;

			var remainingWidth = availableWidth - PaddingLeft - PaddingRight;
			var remainingHeight = availableHeight - PaddingTop - PaddingBottom;

			if (IsIconGravityHorizontal)
			{
				remainingWidth -= IconPadding + GetTextLayoutWidth();
			}
			else
			{
				remainingHeight -= IconPadding + GetTextLayoutHeight();
			}

			var iconWidth = Math.Min(remainingWidth, actual.IntrinsicWidth);
			var iconHeight = Math.Min(remainingHeight, actual.IntrinsicHeight);

			var ratio = Math.Min(
				(double)iconWidth / actual.IntrinsicWidth,
				(double)iconHeight / actual.IntrinsicHeight);

			resizable.SetPreferredSize(
				Math.Max(0, (int)(actual.IntrinsicWidth * ratio)),
				Math.Max(0, (int)(actual.IntrinsicHeight * ratio)));

			// trigger a layout re-calculation
			Icon = null;
			Icon = resizable;
		}

		bool IsIconGravityHorizontal =>
			IconGravity is IconGravityTextStart or IconGravityTextEnd or IconGravityStart or IconGravityEnd;

		int GetTextLayoutWidth()
		{
			float maxWidth = 0;
			int lineCount = LineCount;
			for (int line = 0; line < lineCount; line++)
			{
				maxWidth = Math.Max(maxWidth, Layout!.GetLineWidth(line));
			}
			return (int)Math.Ceiling(maxWidth);
		}

		int GetTextLayoutHeight()
		{
			var layoutHeight = Layout!.Height;

			return layoutHeight;
		}

		internal class MauiResizableDrawable : LayerDrawable
		{
			public MauiResizableDrawable(Drawable drawable)
				: base([drawable])
			{
				PaddingMode = (int)LayerDrawablePaddingMode.Stack;
			}

			public Drawable Drawable => GetDrawable(0)!;

			public void SetPreferredSize(int width, int height)
			{
				if (OperatingSystem.IsAndroidVersionAtLeast(23))
				{
					SetLayerSize(0, width, height);
				}

				// TODO: find something that works for older versions
			}
		}
	}
}