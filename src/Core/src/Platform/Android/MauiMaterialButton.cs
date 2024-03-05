using System;
using Android.Content;
using Android.Graphics.Drawables;
using Google.Android.Material.Button;

namespace Microsoft.Maui.Platform
{
	public class MauiMaterialButton : MaterialButton
	{
		// Currently Material doesn't have any bottom gravity options
		// so we just move the layout to the bottom using
		// SetCompoundDrawablesRelative during Layout
		internal const int IconGravityBottom = 128; // needs to be a flags number

		public MauiMaterialButton(Context context)
			: base(context)
		{
		}

		internal MauiResizeableDrawable? ResizeableIcon => Icon as MauiResizeableDrawable;

		protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
		{
			var availableWidth = MeasureSpec.GetSize(widthMeasureSpec);
			var availableHeight = MeasureSpec.GetSize(heightMeasureSpec);

			CalculateIconSize(availableWidth, availableHeight);

			base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
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
			IconGravity == IconGravityTextStart ||
			IconGravity == IconGravityTextEnd ||
			IconGravity == IconGravityStart ||
			IconGravity == IconGravityEnd;

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