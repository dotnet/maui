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

		internal MauiResizeableDrawable? ResizeableIcon => Icon as MauiResizeableDrawable;

		public override int IconGravity
		{
			get => base.IconGravity;
			set
			{
				// Intercept the grvity value and set the flag if it's bottom.
				ForceBottomIconGravity = value == IconGravityBottom;
				base.IconGravity = ForceBottomIconGravity ? IconGravityTop : value;
			}
		}

		internal bool ForceBottomIconGravity { get; private set; }

		protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
		{
			var availableWidth = MeasureSpec.GetMode(widthMeasureSpec) == MeasureSpecMode.Unspecified
				? int.MaxValue
				: MeasureSpec.GetSize(widthMeasureSpec);

			var availableHeight = MeasureSpec.GetMode(heightMeasureSpec) == MeasureSpecMode.Unspecified
				? int.MaxValue
				: MeasureSpec.GetSize(heightMeasureSpec);

			CalculateIconSize(availableWidth, availableHeight);

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

		internal void CalculateIconSize(int availableWidth, int availableHeight)
		{
			// bail if the icon isn't resizeable or the text layout is not available yet
			if (Icon is not MauiResizeableDrawable resizeable || Layout is null)
			{
				return;
			}

			var actual = resizeable.Drawable;

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

			resizeable.SetPreferredSize(
				Math.Max(0, (int)(actual.IntrinsicWidth * ratio)),
				Math.Max(0, (int)(actual.IntrinsicHeight * ratio)));

			// trigger a layout re-calculation
			Icon = null;
			Icon = resizeable;
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

		internal class MauiResizeableDrawable : LayerDrawable
		{
			public MauiResizeableDrawable(Drawable drawable)
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