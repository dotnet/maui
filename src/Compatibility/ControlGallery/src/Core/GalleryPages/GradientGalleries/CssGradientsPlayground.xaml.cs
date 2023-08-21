//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Android.App;
//using Android.Content;
//using Android.Content.PM;
//using Android.OS;
//using Android.Renderscripts;
//using Android.Runtime;
//using Android.Views;
//using Android.Widget;
//using Microsoft.Maui.Controls.ControlGallery.Issues;

using System;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery.GalleryPages.GradientGalleries
{
	public partial class CssGradientsPlayground : ContentPage
	{
		public CssGradientsPlayground()
		{
			InitializeComponent();
			BindingContext = new CssGradientsPlaygroundViewModel();
		}
	}

	[Preserve(AllMembers = true)]
	public class CssGradientsPlaygroundViewModel : BindableObject
	{
		string _css;
		string _error;
		GradientBrush _backgroundBrush;
		readonly BrushTypeConverter _brushTypeConverter;

		public CssGradientsPlaygroundViewModel()
		{
			_brushTypeConverter = new BrushTypeConverter();

			Css = "linear-gradient(90deg, rgb(255, 0, 0) 0%,rgb(255, 153, 51) 60%)";
			UpdateGradientBrush();
		}

		public string Css
		{
			get => _css;
			set
			{
				_css = value;

				if (!string.IsNullOrEmpty(_css))
					UpdateGradientBrush();

				OnPropertyChanged();
			}
		}

		public string Error
		{
			get => _error;
			set
			{
				_error = value;
				OnPropertyChanged();
			}
		}

		public GradientBrush BackgroundBrush
		{
			get => _backgroundBrush;
			set
			{
				_backgroundBrush = value;
				OnPropertyChanged();
			}
		}

		void UpdateGradientBrush()
		{
			try
			{
				var gradient = _brushTypeConverter.ConvertFromInvariantString(Css);

				BackgroundBrush = (GradientBrush)gradient;
				Error = string.Empty;
			}
			catch (Exception ex)
			{
				Error = ex.Message;
			}
		}
	}
}