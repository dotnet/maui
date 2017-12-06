using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WProgressBar = System.Windows.Controls.ProgressBar;

namespace Xamarin.Forms.Platform.WPF
{
	public class ActivityIndicatorRenderer : ViewRenderer<ActivityIndicator, WProgressBar>
	{
		protected override void OnElementChanged(ElementChangedEventArgs<ActivityIndicator> e)
		{
			if (e.NewElement != null)
			{
				if (Control == null) // construct and SetNativeControl and suscribe control event
				{
					SetNativeControl(new WProgressBar());
				}

				UpdateIsIndeterminate();
				UpdateColor();
			}

			base.OnElementChanged(e);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == ActivityIndicator.IsRunningProperty.PropertyName)
				UpdateIsIndeterminate();
			else if (e.PropertyName == ActivityIndicator.ColorProperty.PropertyName)
				UpdateColor();
		}

		void UpdateColor()
		{
			Control.UpdateDependencyColor(WProgressBar.ForegroundProperty, Element.Color);
		}

		void UpdateIsIndeterminate()
		{
			Control.IsIndeterminate = Element.IsRunning;
		}
	}
}
