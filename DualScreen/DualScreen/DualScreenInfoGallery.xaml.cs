using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.DualScreen;
using Xamarin.Forms.Xaml;

namespace DualScreen
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class DualScreenInfoGallery : ContentPage
	{
		DualScreenInfo info;

		public DualScreenInfoGallery()
		{
			InitializeComponent();
			info = new DualScreenInfo(tpv);
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			DualScreenInfo.Current.PropertyChanged += OnCurrentPropertyChanged;
			info.PropertyChanged += OnTPVPropertyChanged;

			OnTPVPropertyChanged(null, null);
			OnCurrentPropertyChanged(null, null);
		}

		protected override void OnDisappearing()
		{
			base.OnDisappearing();
			DualScreenInfo.Current.PropertyChanged -= OnCurrentPropertyChanged;
			info.PropertyChanged -= OnTPVPropertyChanged;
		}

		void OnTPVPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (info.SpanningBounds.Length == 2)
			{
				tpvSpanningBounds1.Text = $"{info.SpanningBounds[0]}";
				tpvSpanningBounds2.Text = $"{info.SpanningBounds[1]}";
			}
			else
			{
				tpvSpanningBounds1.Text = "Not Spanned";
				tpvSpanningBounds2.Text = "Not Spanned";
			}

			tpvHingeBounds.Text = $"{info.HingeBounds}";
			tpvIsLandscape.Text = $"{info.IsLandscape}";
			tpvSpanMode.Text = $"{tpv.Mode}";
		}

		void OnCurrentPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (DualScreenInfo.Current.SpanningBounds.Length == 2)
			{
				deviceSpanningBounds1.Text = $"{DualScreenInfo.Current.SpanningBounds[0]}";
				deviceSpanningBounds2.Text = $"{DualScreenInfo.Current.SpanningBounds[1]}";
			}
			else
			{
				deviceSpanningBounds1.Text = "Not Spanned";
				deviceSpanningBounds2.Text = "Not Spanned";
			}

			deviceHingeBounds.Text = $"{DualScreenInfo.Current.HingeBounds}";
			deviceIsLandscape.Text = $"{DualScreenInfo.Current.IsLandscape}";
			deviceSpanMode.Text = $"{DualScreenInfo.Current.SpanMode}";
		}
	}
}