using Microsoft.Maui.Controls;

namespace Microsoft.Maui.TestCases.Tests
{
	internal static class PlatformMethodQueries
	{
		public static readonly Dictionary<BindableProperty, Tuple<string[], bool>> ApplePropertyPlatformMethodDictionary = new()
		{
				{ ActivityIndicator.ColorProperty, Tuple.Create (new[] { "color" }, false) },
				{ ActivityIndicator.IsRunningProperty, Tuple.Create (new[] { "isAnimating" }, false) },
				{ Button.CornerRadiusProperty, Tuple.Create (new[] { "layer", "cornerRadius" }, false) },
				{ Button.BorderWidthProperty, Tuple.Create (new[] { "layer", "borderWidth" }, false) },
				{ Button.TextProperty, Tuple.Create (new[] { "titleLabel", "text" }, false) },
				{ Button.TextColorProperty, Tuple.Create (new[] { "titleLabel", "textColor" }, false) },
				{ ImageButton.CornerRadiusProperty, Tuple.Create (new[] { "layer", "cornerRadius" }, false) },
				{ ImageButton.BorderWidthProperty, Tuple.Create (new[] { "layer", "borderWidth" }, false) },
				{ View.AnchorXProperty, Tuple.Create (new[] { "getLayerTransformString" }, true) },
				{ View.AnchorYProperty, Tuple.Create (new[] { "lgetLayerTransformString" }, true) },
				{ View.BackgroundColorProperty, Tuple.Create (new[] { "backgroundColor" }, false) },
				{ View.IsEnabledProperty, Tuple.Create (new[] { "enabled" }, false) },
				{ View.OpacityProperty, Tuple.Create (new [] { "alpha" }, true) },
				{ View.RotationProperty, Tuple.Create (new[] { "getLayerTransformString" }, true) },
				{ View.RotationXProperty, Tuple.Create (new[] { "getLayerTransformString" }, true) },
				{ View.RotationYProperty, Tuple.Create (new[] { "getLayerTransformString" }, true) },
				{ View.ScaleProperty, Tuple.Create (new[] { "getLayerTransformString" }, true) },
			};

		public static readonly Dictionary<BindableProperty, Tuple<string[], bool>> WindowsPropertyPlatformMethodDictionary = new()
		{
				{ ActivityIndicator.ColorProperty, Tuple.Create(new[] { "getProgressDrawable", "getColor" }, false) },
				{ ActivityIndicator.IsRunningProperty, Tuple.Create(new[] { "isIndeterminate" }, false) },
				//{ BorderElement.BorderColorProperty, Tuple.Create(new[] { "getBackground" }, false) },
				{ Button.CornerRadiusProperty, Tuple.Create(new[] { "getBackground" }, false) },
				{ Button.BorderWidthProperty, Tuple.Create(new[] { "getBackground" }, false) },
				{ Button.ImageSourceProperty, Tuple.Create(new[] { "getBackground" }, false) },
				{ Button.TextProperty, Tuple.Create(new[] { "getText" }, false) },
				{ Button.TextColorProperty, Tuple.Create(new[] { "getCurrentTextColor" }, false) },
				{ ImageButton.CornerRadiusProperty, Tuple.Create(new[] { "getBackground" }, false) },
				{ ImageButton.BorderWidthProperty, Tuple.Create(new[] { "getBackground" }, false) },
				{ ImageButton.SourceProperty, Tuple.Create(new[] { "getBackground" }, false) },
				{ View.AnchorXProperty, Tuple.Create(new[] { "getPivotX" }, true) },
				{ View.AnchorYProperty, Tuple.Create(new[] { "getPivotY" }, true) },
				{ View.BackgroundColorProperty, Tuple.Create(new[] { "getBackground", "getColor" }, true) },
				{ View.IsEnabledProperty, Tuple.Create(new[] { "isEnabled" }, false) },
				{ View.OpacityProperty, Tuple.Create(new[] { "getAlpha" }, true) },
				{ View.RotationProperty, Tuple.Create(new[] { "getRotation" }, true) },
				{ View.RotationXProperty, Tuple.Create(new[] { "getRotationX" }, true) },
				{ View.RotationYProperty, Tuple.Create(new[] { "getRotationY" }, true) },
				{ View.ScaleProperty, Tuple.Create(new[] { "getScaleX", "getScaleY" }, true) },
			};

		public static readonly Dictionary<BindableProperty, Tuple<string[], bool>> AndroidPropertyPlatformMethodDictionary = new()
		{
				{ ActivityIndicator.ColorProperty, Tuple.Create(new[] { "getProgressDrawable", "getColor" }, false) },
				{ ActivityIndicator.IsRunningProperty, Tuple.Create(new[] { "indeterminate" }, false) },
				//{ BorderElement.BorderColorProperty, Tuple.Create(new[] { "getBackground" }, false) },
				{ Button.CornerRadiusProperty, Tuple.Create(new[] { "getBackground" }, false) },
				{ Button.BorderWidthProperty, Tuple.Create(new[] { "getBackground" }, false) },
				{ Button.ImageSourceProperty, Tuple.Create(new[] { "getBackground" }, false) },
				{ Button.TextProperty, Tuple.Create(new[] { "getText" }, false) },
				{ Button.TextColorProperty, Tuple.Create(new[] { "getCurrentTextColor" }, false) },
				{ ImageButton.CornerRadiusProperty, Tuple.Create(new[] { "getBackground" }, false) },
				{ ImageButton.BorderWidthProperty, Tuple.Create(new[] { "getBackground" }, false) },
				{ ImageButton.SourceProperty, Tuple.Create(new[] { "getBackground" }, false) },
				{ View.AnchorXProperty, Tuple.Create(new[] { "getPivotX" }, true) },
				{ View.AnchorYProperty, Tuple.Create(new[] { "getPivotY" }, true) },
				{ View.BackgroundColorProperty, Tuple.Create(new[] { "getBackground", "getColor" }, true) },
				{ View.IsEnabledProperty, Tuple.Create(new[] { "enabled" }, false) },
				{ View.OpacityProperty, Tuple.Create(new[] { "getAlpha" }, true) },
				{ View.RotationProperty, Tuple.Create(new[] { "getRotation" }, true) },
				{ View.RotationXProperty, Tuple.Create(new[] { "getRotationX" }, true) },
				{ View.RotationYProperty, Tuple.Create(new[] { "getRotationY" }, true) },
				{ View.ScaleProperty, Tuple.Create(new[] { "getScaleX", "getScaleY" }, true) },
			};
	}
}