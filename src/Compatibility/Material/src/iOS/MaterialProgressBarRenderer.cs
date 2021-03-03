using System;
using System.ComponentModel;
using CoreGraphics;
using MaterialComponents;
using UIKit;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Platform.iOS;
using MProgressView = MaterialComponents.ProgressView;

namespace Microsoft.Maui.Controls.Compatibility.Material.iOS
{
	public class MaterialProgressBarRenderer : ViewRenderer<ProgressBar, MProgressView>
	{
		SemanticColorScheme _defaultColorScheme;
		SemanticColorScheme _colorScheme;
		ContainerScheme _containerScheme;

		protected override void OnElementChanged(ElementChangedEventArgs<ProgressBar> e)
		{
			_colorScheme?.Dispose();
			_colorScheme = CreateSemanticColorScheme();
			_containerScheme?.Dispose();
			_containerScheme = new ContainerScheme();

			base.OnElementChanged(e);

			if (e.NewElement != null)
			{
				if (Control == null)
				{
					_defaultColorScheme = CreateSemanticColorScheme();

					SetNativeControl(CreateNativeControl());
				}

				UpdateProgressColor();
				UpdateProgress();

				ApplyTheme();
			}
		}

		[Obsolete]
		[EditorBrowsable(EditorBrowsableState.Never)]
		protected virtual BasicColorScheme CreateColorScheme()
		{
			return null;
		}

		protected virtual SemanticColorScheme CreateSemanticColorScheme()
		{
			return MaterialColors.Light.CreateColorScheme();
		}


		protected virtual void ApplyTheme()
		{
			_containerScheme.ColorScheme = _colorScheme;
			Control.ApplyTheme(_containerScheme);
			Control.TrackTintColor = _containerScheme.ColorScheme.BackgroundColor;
		}

		protected override MProgressView CreateNativeControl() => new MProgressView();

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

		void UpdateProgressColor() => UpdateAllColors();

		void UpdateAllColors()
		{
			Color progressColor = Element.ProgressColor;
			Color backgroundColor = Element.BackgroundColor;

			if (progressColor.IsDefault)
			{
				if(backgroundColor.IsDefault)
				{
					// reset everything to defaults
					_colorScheme = CreateSemanticColorScheme();
					var progress = _colorScheme.PrimaryColor;
					progress.GetRGBA(out _, out _, out _, out var alpha);
					_colorScheme.BackgroundColor = progress.ColorWithAlpha(alpha * MaterialColors.SliderTrackAlpha);
				}
				else
				{
					// handle the case where only the background is set
					var background = backgroundColor.ToUIColor();
					_colorScheme = new SemanticColorScheme() { BackgroundColor = background };

				}
			}
			else if (!progressColor.IsDefault)
			{
				if (backgroundColor.IsDefault)
				{
					// handle the case where only the progress is set
					var progress = progressColor.ToUIColor();

					progress.GetRGBA(out _, out _, out _, out var alpha);
					_colorScheme = new SemanticColorScheme()
					{
						PrimaryColor = progress,
						BackgroundColor = progress.ColorWithAlpha(alpha * MaterialColors.SliderTrackAlpha)
					};
				}
				else
				{
					// handle the case where both are set
					var background = backgroundColor.ToUIColor();
					var progress = progressColor.ToUIColor();

					_colorScheme = new SemanticColorScheme()
					{
						PrimaryColor = progress,
						BackgroundColor = background
					};
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