using System.ComponentModel;
using Microsoft.Maui.Graphics;
using UIKit;
using SizeF = CoreGraphics.CGSize;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
{
	public class ProgressBarRenderer : ViewRenderer<ProgressBar, UIProgressView>
	{
		[Microsoft.Maui.Controls.Internals.Preserve(Conditional = true)]
		public ProgressBarRenderer()
		{

		}

		public override SizeF SizeThatFits(SizeF size)
		{
			// progress bar will size itself to be as wide as the request, even if its inifinite
			// we want the minimum need size
			var result = base.SizeThatFits(size);
			return new SizeF(10, result.Height);
		}

		[PortHandler]
		protected override void OnElementChanged(ElementChangedEventArgs<ProgressBar> e)
		{
			if (e.NewElement != null)
			{
				if (Control == null)
					SetNativeControl(new UIProgressView(UIProgressViewStyle.Default));

				UpdateProgressColor();
				UpdateProgress();
			}

			base.OnElementChanged(e);
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

			if (Control == null)
				return;

			Control.TrackTintColor = color != null ? color.ToUIColor() : null;
		}

		void UpdateProgressColor()
		{
			Control.ProgressTintColor = Element.ProgressColor == null ? null : Element.ProgressColor.ToUIColor();
		}

		[PortHandler]
		void UpdateProgress()
		{
			Control.Progress = (float)Element.Progress;
		}
	}
}