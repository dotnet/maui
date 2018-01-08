using System.ComponentModel;

using SpecificVE = Xamarin.Forms.PlatformConfiguration.TizenSpecific.VisualElement;
using Specific = Xamarin.Forms.PlatformConfiguration.TizenSpecific.ProgressBar;
using EProgressBar = ElmSharp.ProgressBar;

namespace Xamarin.Forms.Platform.Tizen
{
	public class ProgressBarRenderer : ViewRenderer<ProgressBar, EProgressBar>
	{
		protected override void OnElementChanged(ElementChangedEventArgs<ProgressBar> e)
		{
			if (Control == null)
			{
				SetNativeControl(new EProgressBar(Forms.NativeParent));
			}

			if (e.NewElement != null)
			{
				if (e.NewElement.MinimumWidthRequest == -1 &&
				e.NewElement.MinimumHeightRequest == -1 &&
				e.NewElement.WidthRequest == -1 &&
				e.NewElement.HeightRequest == -1)
				{
					Log.Warn("Need to size request");
				}

				UpdateAll();
			}

			base.OnElementChanged(e);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);
			if (e.PropertyName == ProgressBar.ProgressProperty.PropertyName)
			{
				UpdateProgress();
			}
			else if (TizenPlatformServices.AppDomain.IsTizenSpecificAvailable && e.PropertyName == "ProgressBarPulsingStatus")
			{
				UpdatePulsingStatus();
			}
		}

		protected override void UpdateThemeStyle()
		{
			var themeStyle = SpecificVE.GetStyle(Element);
			if (!string.IsNullOrEmpty(themeStyle))
				Control.Style = themeStyle;
		}

		void UpdateAll()
		{
			UpdateProgress();
			if (TizenPlatformServices.AppDomain.IsTizenSpecificAvailable)
			{
				UpdatePulsingStatus();
			}
		}

		void UpdateProgress()
		{
			Control.Value = Element.Progress;
		}

		void UpdatePulsingStatus()
		{
			bool isPulsing = Specific.GetPulsingStatus(Element);
			if (isPulsing)
			{
				Control.PlayPulse();
			}
			else
			{
				Control.StopPulse();
			}
		}
	}
}

