using System;
using System.Diagnostics.CodeAnalysis;
using Android.Content;
using Android.Content.Res;
using Android.Graphics.Drawables;
using Microsoft.Maui.Graphics;
using AColor = Android.Graphics.Color;
using AView = Android.Views.View;

namespace Microsoft.Maui.Platform;

static class MauiRippleDrawableExtensions
{
	const int MauiBackgroundDrawableId = 1001;
	const int MauiStrokeDrawableId = 1002;

	// Default obtained by running on Android 34 and inspecting the default drawable
	const int DefaultCornerRadius = 4;
	const int DefaultStrokeThicknessWithColor = 1;
	const int DefaultStrokeThicknessNoColor = 0;
	static readonly Color DefaultStrokeColor = Colors.Black;

	// Default obtained from Material source for 28+:
	// https://github.com/material-components/material-components-android/blob/778a9f2a490394906e97881bb29ac491a64c23d7/lib/java/com/google/android/material/resources/res/values-v28/dimens.xml#L29-L34
	// There is a different default for Android 22-27, but we are not using that one:
	// https://github.com/material-components/material-components-android/blob/778a9f2a490394906e97881bb29ac491a64c23d7/lib/java/com/google/android/material/resources/res/values-v21/dimens.xml#L36-L48
	const float DefaultRippleAlpha = 0.24f;

	internal static bool UpdateMauiRippleDrawableStroke(this AView platformView, IButtonStroke button)
	{
		if (!platformView.TryGetMauiBackground(out _, out _, out var gradientDrawable, out var strokeDrawable, out var maskDrawable))
		{
			return false;
		}

		var (width, color, radius) = button.GetStrokeProperties(platformView.Context!, false);

		strokeDrawable.SetStroke(width, color);
		strokeDrawable.SetCornerRadius(radius);

		// Set the background stroke as well in order to ensure the behavior
		// is the same where the background is inset half the stroke width.
		gradientDrawable.SetStroke(width, AColor.Transparent);

		// The other drawables need to be updated as the stroke is just a layer
		// and the background/mask are separate drawables.
		gradientDrawable.SetCornerRadius(radius);
		maskDrawable.SetCornerRadius(radius);

		return true;
	}

	internal static void UpdateMauiRippleDrawableBackground(this AView platformView,
		Paint? background,
		IButtonStroke stroke,
		Func<int?>? getEmptyBackgroundColor = null,
		Func<ColorStateList?>? getDefaultRippleColor = null,
		Action? beforeSet = null)
	{
		// Get the current background
		var recreateRipple = false;
		if (!platformView.TryGetMauiBackground(out var rippleDrawable, out var layerDrawable, out _, out var strokeDrawable, out _))
		{
			// The current background is the default one.

			if (background.IsNullOrEmpty())
			{
				// The new background is empty/default, so no need to do anything.
				return;
			}

			// We are going to set a new background, so make a brand new drawable.
			recreateRipple = true;
		}

		var (width, color, radius) = stroke.GetStrokeProperties(platformView.Context!, false);

		// The previous background may have had transparency or a gradient which cannot
		// be un-set, so we need an entirely new drawable.
		var backgroundDrawable = new GradientDrawable();
		backgroundDrawable.SetCornerRadius(radius);
		backgroundDrawable.SetStroke(width, AColor.Transparent);

		if (background is null)
		{
			// The new background is null/empty.

			var defaultColor = getEmptyBackgroundColor?.Invoke();
			if (defaultColor is null)
			{
				// There does not need to be any background so do nothing.
			}
			else
			{
				// We need to revert back to the "default background".

				// This might be expensive, but only occurs when the background is changed from a non-null
				// value to null while the app is running.

				backgroundDrawable.SetColor(defaultColor.Value);
			}
		}
		else
		{
			// Apply the background to the drawable
			background.ApplyTo(backgroundDrawable, platformView.Width, platformView.Height);
		}

		if (!recreateRipple)
		{
			// The current background is already a "MAUI" background, so we just need
			// to replace the background drawable in the layer drawable.
			//
			// The drawables cannot be null here as we checked in TryGetMauiBackground.
			// If it is null, then recreateRipple is true - meaning this code will not
			// be executing now.

			layerDrawable!.SetDrawableByLayerId(MauiBackgroundDrawableId, backgroundDrawable);
			rippleDrawable!.InvalidateSelf();
		}
		else
		{
			// This is the first time the background was set, so we are creating the
			// whole background from scratch.

			// Create the ripple mask.
			var maskDrawable = new GradientDrawable();
			maskDrawable.SetTint(AColor.White);
			maskDrawable.SetCornerRadius(radius);

			// Create the stroke layer.
			strokeDrawable = new GradientDrawable();
			strokeDrawable.SetCornerRadius(radius);
			var strokeColor = stroke.StrokeColor ?? Colors.Black;
			var strokeColorList = ColorStateListExtensions.CreateButton(strokeColor.ToPlatform());
			strokeDrawable.SetStroke(width, strokeColorList);

			// Create the entire drawable structure
			var rippleColor = getDefaultRippleColor?.Invoke() ?? ColorStateList.ValueOf(Colors.White.WithAlpha(DefaultRippleAlpha).ToPlatform());
			Drawable[] layers;
			rippleDrawable =
				new RippleDrawable(
					rippleColor,
					new InsetDrawable(
						layerDrawable = new LayerDrawable(
							layers = [backgroundDrawable, strokeDrawable]),
						0, 0, 0, 0),
					maskDrawable);

			// Assign IDs so we can find them later
			var idx = Array.IndexOf(layers, backgroundDrawable);
			layerDrawable.SetId(idx, MauiBackgroundDrawableId);
			idx = Array.IndexOf(layers, strokeDrawable);
			layerDrawable.SetId(idx, MauiStrokeDrawableId);

			beforeSet?.Invoke();

			// Finally, we replace the background and Android will tell us that we are now
			// responsible for everything - which we want.
			platformView.Background = rippleDrawable;
		}
	}

	internal static (int Thickness, ColorStateList Color, int Radius) GetStrokeProperties(this IButtonStroke button, Context context, bool defaultButtonLogic)
	{
		var strokeColor = button.StrokeColor ?? DefaultStrokeColor;
		var strokeColorList = ColorStateListExtensions.CreateButton(strokeColor.ToPlatform());

		var strokeWidth = button.StrokeThickness >= 0 && button.StrokeColor is not null
			? button.StrokeThickness
			: button.StrokeColor is not null
				? DefaultStrokeThicknessWithColor
				: DefaultStrokeThicknessNoColor;
		var strokeWidthPixels = (int)context.ToPixels(strokeWidth);

		var cornerRadius = button.CornerRadius >= 0
			? button.CornerRadius
			: DefaultCornerRadius;
		var cornerRadiusPixels = (int)context.ToPixels(cornerRadius);

		if (!defaultButtonLogic)
		{
			// Perform some "advanced" mathematics to match the default button formatting
			// where the corner radius and stroke width are all intertwined.
			// The GradientDrawable calculates the corners differently. Both ways are wrong
			// since we set the corner radius so it should just start filling the inside of
			// the shape. But, for some reason, if the stroke width gets too thick, then it 
			// all falls apart and forgets how to make corners round. Android is a wonder.

			cornerRadiusPixels -= strokeWidthPixels / 2;
		}

		return (strokeWidthPixels, strokeColorList, cornerRadiusPixels);
	}

	internal static bool TryGetMauiBackground(
		this AView platformView,
		[NotNullWhen(true)] out RippleDrawable? ripple,
		[NotNullWhen(true)] out LayerDrawable? buttonLayer,
		[NotNullWhen(true)] out GradientDrawable? buttonBackground,
		[NotNullWhen(true)] out GradientDrawable? buttonStroke,
		[NotNullWhen(true)] out GradientDrawable? maskDrawable) =>
		platformView.Background.TryGetMauiBackground(out ripple, out buttonLayer, out buttonBackground, out buttonStroke, out maskDrawable);

	internal static bool TryGetMauiBackground(
		this Drawable? drawable,
		[NotNullWhen(true)] out RippleDrawable? ripple,
		[NotNullWhen(true)] out LayerDrawable? buttonLayer,
		[NotNullWhen(true)] out GradientDrawable? buttonBackground,
		[NotNullWhen(true)] out GradientDrawable? buttonStroke,
		[NotNullWhen(true)] out GradientDrawable? maskDrawable)
	{
		if (drawable is RippleDrawable rippleDrawable &&
			rippleDrawable.NumberOfLayers >= 2 &&
			rippleDrawable.GetDrawable(0) is InsetDrawable insetDrawable &&
			insetDrawable.Drawable is LayerDrawable layerDrawable &&
			layerDrawable.FindDrawableByLayerId(MauiBackgroundDrawableId) is GradientDrawable backgroundGradientDrawable &&
			layerDrawable.FindDrawableByLayerId(MauiStrokeDrawableId) is GradientDrawable strokeGradientDrawable &&
			rippleDrawable.GetDrawable(1) is GradientDrawable materialShapeDrawable)
		{
			ripple = rippleDrawable;
			buttonLayer = layerDrawable;
			buttonBackground = backgroundGradientDrawable;
			buttonStroke = strokeGradientDrawable;
			maskDrawable = materialShapeDrawable;

			return true;
		}

		ripple = null;
		buttonLayer = null;
		buttonBackground = null;
		buttonStroke = null;
		maskDrawable = null;

		return false;
	}
}
