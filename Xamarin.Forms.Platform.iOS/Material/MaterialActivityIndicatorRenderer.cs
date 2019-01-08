using System.ComponentModel;
using CoreGraphics;
using MaterialComponents;
using UIKit;
using Xamarin.Forms;
using MActivityIndicator = MaterialComponents.ActivityIndicator;

[assembly: ExportRenderer(typeof(Xamarin.Forms.ActivityIndicator), typeof(Xamarin.Forms.Platform.iOS.Material.MaterialActivityIndicatorRenderer), new[] { typeof(VisualRendererMarker.Material) })]

namespace Xamarin.Forms.Platform.iOS.Material
{
	public class MaterialActivityIndicatorRenderer : ViewRenderer<ActivityIndicator, MActivityIndicator>
	{
		SemanticColorScheme _defaultColorScheme;
		SemanticColorScheme _colorScheme;

		public MaterialActivityIndicatorRenderer()
		{
			VisualElement.VerifyVisualFlagEnabled();
		}

		protected override void OnElementChanged(ElementChangedEventArgs<ActivityIndicator> e)
		{
			_colorScheme?.Dispose();
			_colorScheme = CreateColorScheme();

			base.OnElementChanged(e);

			if (e.NewElement != null)
			{
				if (Control == null)
				{
					_defaultColorScheme = CreateColorScheme();

					SetNativeControl(CreateNativeControl());
				}

				UpdateColor();
				UpdateIsRunning();

				ApplyTheme();
			}
		}

		protected virtual SemanticColorScheme CreateColorScheme()
		{
			return MaterialColors.Light.CreateColorScheme();
		}

		protected virtual void ApplyTheme()
		{
			ActivityIndicatorColorThemer.ApplySemanticColorScheme(_colorScheme, Control);
		}

		protected override MActivityIndicator CreateNativeControl()
		{
			return new MActivityIndicator
			{
				IndicatorMode = ActivityIndicatorMode.Indeterminate
			};
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			var updatedTheme = false;
			if (e.PropertyName == ActivityIndicator.ColorProperty.PropertyName)
			{
				UpdateColor();
				updatedTheme = true;
			}
			else if (e.PropertyName == ActivityIndicator.IsRunningProperty.PropertyName)
			{
				UpdateIsRunning();
			}

			if (updatedTheme)
				ApplyTheme();
		}

		protected override void SetBackgroundColor(Color color)
		{
			// Do not call base to avoid the actual background view changing color.
			//base.SetBackgroundColor(color);

			if (Control == null)
				return;

			// TODO: Investigate whether or not we want to look for the track
			//       layer and change the color. This will be brittle.
			//       For now, just show/hide the track.

			if (color.IsDefault)
				Control.TrackEnabled = false;
			else
				Control.TrackEnabled = true;

			// handle the actual background color next to the main color
			UpdateColor();
		}

		void UpdateColor()
		{
			Color color = Element.Color;
			Color backColor = Element.BackgroundColor;

			if (!color.IsDefault)
				_colorScheme.PrimaryColor = color.ToUIColor();
			else if (!backColor.IsDefault)
				_colorScheme.PrimaryColor = backColor.ToUIColor();
			else
				_colorScheme.PrimaryColor = _defaultColorScheme.PrimaryColor;
		}

		void UpdateIsRunning()
		{
			bool isRunning = Element.IsRunning;

			if (Control.Animating == isRunning)
				return;

			if (isRunning)
				Control.StartAnimating();
			else
				Control.StopAnimating();
		}
	}
}