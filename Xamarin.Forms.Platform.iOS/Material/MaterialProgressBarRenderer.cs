using System.ComponentModel;
using UIKit;
using SizeF = CoreGraphics.CGSize;
using MProgressView = MaterialComponents.ProgressView;
using Xamarin.Forms;
using CoreGraphics;

[assembly: ExportRenderer(typeof(Xamarin.Forms.ProgressBar), typeof(Xamarin.Forms.Platform.iOS.Material.MaterialProgressBarRenderer), new[] { typeof(VisualRendererMarker.Material) })]
namespace Xamarin.Forms.Platform.iOS.Material
{
	public class MaterialProgressBarRenderer : ViewRenderer<ProgressBar, MProgressView>
	{
		public MaterialProgressBarRenderer()
		{
			VisualElement.VerifyVisualFlagEnabled();
		}

		protected override void OnElementChanged(ElementChangedEventArgs<ProgressBar> e)
		{
			if (e.NewElement != null)
			{
				if (Control == null)
					SetNativeControl(new MProgressView());

				UpdateProgressColor();
				UpdateProgress();
			}

			base.OnElementChanged(e);
			Control.SetHidden(false, true, (completion) => { });
			Element.WidthRequest = 10;
		}


		public override SizeF SizeThatFits(SizeF size)
		{
			var result = base.SizeThatFits(size);
			var height = result.Height;

			if(height == 0)
			{
				if(System.nfloat.IsInfinity(size.Height))
				{
					height = 5;
				}
				else
				{
					height = size.Height;
				}

			}
			return new SizeF(10, height);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == ProgressBar.ProgressColorProperty.PropertyName)
				UpdateProgressColor();
			else if (e.PropertyName == ProgressBar.ProgressProperty.PropertyName)
				UpdateProgress();
		}

		protected override void SetBackgroundColor(Color color)
		{
			base.SetBackgroundColor(color);

			if (Control == null || color == Color.Default)
				return;

			Control.TrackTintColor = color.ToUIColor();
		}

		void UpdateProgressColor()
		{
			if (Element.ProgressColor == Color.Default)
				return;

			Control.ProgressTintColor = Element.ProgressColor.ToUIColor();
		}

		void UpdateProgress()
		{
			Control.Progress = (float)Element.Progress;
		}
	}
}