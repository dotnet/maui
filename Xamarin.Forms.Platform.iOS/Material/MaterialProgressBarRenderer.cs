using System;
using System.ComponentModel;
using CoreGraphics;
using MaterialComponents;
using UIKit;
using Xamarin.Forms;
using MProgressView = MaterialComponents.ProgressView;

[assembly: ExportRenderer(typeof(Xamarin.Forms.ProgressBar), typeof(Xamarin.Forms.Platform.iOS.Material.MaterialProgressBarRenderer), new[] { typeof(VisualRendererMarker.Material) })]

namespace Xamarin.Forms.Platform.iOS.Material
{
	public class MaterialProgressBarRenderer : ViewRenderer<ProgressBar, MProgressView>
	{
		const float BackgroundAlpha = 0.3f;

		BasicColorScheme _defaultColorScheme;
		BasicColorScheme _colorScheme;

		public MaterialProgressBarRenderer()
		{
			VisualElement.VerifyVisualFlagEnabled();
		}

		protected override void OnElementChanged(ElementChangedEventArgs<ProgressBar> e)
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

				UpdateProgressColor();
				UpdateProgress();

				ApplyTheme();
			}
		}

		protected virtual BasicColorScheme CreateColorScheme()
		{
			// TODO: Fix this once Google implements the new way.
			//       Right now, copy what is done with the activity indicator.

			var cs = MaterialColors.Light.CreateColorScheme();
			return new BasicColorScheme(
				cs.PrimaryColor,
				cs.PrimaryColor.ColorWithAlpha(BackgroundAlpha),
				cs.PrimaryColor);
		}

		protected virtual void ApplyTheme()
		{
			// TODO: Fix this once Google implements the new way.

#pragma warning disable CS0618 // Type or member is obsolete
			ProgressViewColorThemer.ApplyColorScheme(_colorScheme, Control);
#pragma warning restore CS0618 // Type or member is obsolete
		}

		protected override MProgressView CreateNativeControl()
		{
			return new MProgressView();
		}

		public override CGSize SizeThatFits(CGSize size)
		{
			var result = base.SizeThatFits(size);
			var height = result.Height;

			if (height == 0)
			{
				if (nfloat.IsInfinity(size.Height))
					height = 4;
				else
					height = size.Height;
			}

			return new CGSize(10, height);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			var updatedTheme = false;
			if (e.PropertyName == ProgressBar.ProgressColorProperty.PropertyName)
			{
				UpdateProgressColor();
				updatedTheme = true;
			}
			else if (e.PropertyName == ProgressBar.ProgressProperty.PropertyName)
			{
				UpdateProgress();
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

			UpdateAllColors();
			ApplyTheme();
		}

		void UpdateProgressColor()
		{
			UpdateAllColors();
		}

		void UpdateAllColors()
		{
			// TODO: Fix this once Google implements the new way.
			//       Right now, copy what is done with the activity indicator.

			Color progressColor = Element.ProgressColor;
			Color backgroundColor = Element.BackgroundColor;

			if (progressColor.IsDefault)
			{
				if(backgroundColor.IsDefault)
				{
					// reset everything to defaults
					_colorScheme = new BasicColorScheme(
						_defaultColorScheme.PrimaryColor,
						_defaultColorScheme.PrimaryLightColor,
						_defaultColorScheme.PrimaryColor);
				}
				else
				{
					// handle the case where only the background is set
					var background = backgroundColor.ToUIColor();

					// TODO: Potentially override background alpha to match material design.
					// TODO: Potentially override primary color to match material design.
					_colorScheme = new BasicColorScheme(
						_defaultColorScheme.PrimaryColor,
						background,
						_defaultColorScheme.PrimaryColor);

				}
			}
			else if (!progressColor.IsDefault)
			{
				if (backgroundColor.IsDefault)
				{
					// handle the case where only the progress is set
					var progress = progressColor.ToUIColor();

					_colorScheme = new BasicColorScheme(
						progress,
						progress.ColorWithAlpha(BackgroundAlpha),
						progress);
				}
				else
				{
					// handle the case where both are set
					var background = backgroundColor.ToUIColor();
					var progress = progressColor.ToUIColor();

					// TODO: Potentially override alpha to match material design.
					_colorScheme = new BasicColorScheme(
						progress,
						background,
						progress);
				}
			}
		}

		void UpdateProgress()
		{
			if (Control == null)
				return;

			Control.Progress = (float)Element.Progress;
		}
	}
}