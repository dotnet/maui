using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Xamarin.Forms.Controls.GalleryPages.TwoPaneViewGalleries
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class GridUsingDualScreenInfo : ContentPage
	{
		public DualScreen.DualScreenInfo DualScreenInfo { get; }
		public GridUsingDualScreenInfo()
		{
			InitializeComponent();
			DualScreenInfo = new DualScreen.DualScreenInfo(grid);
			BindingContext = this;
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			DualScreenInfo.PropertyChanged += OnInfoPropertyChanged;
			UpdateColumns();
		}

		protected override void OnDisappearing()
		{
			base.OnDisappearing();
			DualScreenInfo.PropertyChanged -= OnInfoPropertyChanged;
		}

		void UpdateColumns()
		{
			if (DualScreenInfo.SpanningBounds.Length > 0)
			{
				Column1Width = DualScreenInfo.SpanningBounds[0].Width;
				Column2Width = DualScreenInfo.HingeBounds.Width;
				Column3Width = DualScreenInfo.SpanningBounds[1].Width;
			}
			else
			{
				Column1Width = 100;
				Column2Width = 0;
				Column3Width = 100;
			}

			OnPropertyChanged(nameof(Column1Width));
			OnPropertyChanged(nameof(Column2Width));
			OnPropertyChanged(nameof(Column3Width));

		}

		void OnInfoPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			UpdateColumns();
		}

		public double Column1Width { get; set; }
		public double Column2Width { get; set; }
		public double Column3Width { get; set; }
	}
}