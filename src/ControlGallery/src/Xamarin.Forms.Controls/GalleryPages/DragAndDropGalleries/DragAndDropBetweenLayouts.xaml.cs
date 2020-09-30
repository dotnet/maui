using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Xamarin.Forms.Controls.GalleryPages.DragAndDropGalleries
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class DragAndDropBetweenLayouts : ContentPage
	{
		public ObservableCollection<Brush> AllColors { get; }
		public ObservableCollection<Brush> RainbowColors { get; }
		public DragAndDropBetweenLayouts()
		{
			InitializeComponent();
			AllColors = new ObservableCollection<Brush>();
			RainbowColors = new ObservableCollection<Brush>();

			AllColors.Add(SolidColorBrush.Red);
			AllColors.Add(SolidColorBrush.Orange);
			AllColors.Add(SolidColorBrush.Yellow);
			AllColors.Add(SolidColorBrush.Green);
			AllColors.Add(SolidColorBrush.Blue);
			AllColors.Add(SolidColorBrush.Indigo);
			AllColors.Add(SolidColorBrush.Violet);
			AllColors.Add(SolidColorBrush.Black);
			AllColors.Add(SolidColorBrush.Brown);
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			BindingContext = this;
		}

		private void OnDragStarting(object sender, DragStartingEventArgs e)
		{
			// e.Cancel = true;
			var boxView = (sender as Element).Parent as BoxView;
			var sl = boxView.Parent as StackLayout;
			e.Data.Properties.Add("Color", boxView.Background);
			e.Data.Properties.Add("Source", sl);

			if (sl == SLAllColors)
				SLRainbow.Background = SolidColorBrush.LightBlue;
			else
				SLAllColors.Background = SolidColorBrush.LightBlue;
		}

		private void OnDropCompleted(object sender, DropCompletedEventArgs e)
		{
			var sl = (sender as Element).Parent.Parent as StackLayout;

			if (sl == SLAllColors)
				SLRainbow.Background = SolidColorBrush.White;
			else
				SLAllColors.Background = SolidColorBrush.White;

		}

		private void OnDragOver(object sender, DragEventArgs e)
		{
			if (!e.Data.Properties.ContainsKey("Source"))
				return;

			//e.AcceptedOperation = DataPackageOperation.None;
			var sl = (sender as Element).Parent as StackLayout;
			if (e.Data.Properties["Source"] == sl)
			{
				e.AcceptedOperation = DataPackageOperation.None;
				return;
			}

			sl.Background = SolidColorBrush.LightPink;
		}

		private void OnDragLeave(object sender, DragEventArgs e)
		{
			if (!e.Data.Properties.ContainsKey("Source"))
				return;

			var sl = (sender as Element).Parent as StackLayout;
			if (e.Data.Properties["Source"] == sl)
			{
				e.AcceptedOperation = DataPackageOperation.None;
				return;
			}

			sl.Background = SolidColorBrush.LightBlue;
		}

		private void OnDrop(object sender, DropEventArgs e)
		{
			if (!e.Data.Properties.ContainsKey("Source"))
				return;

			var sl = (sender as Element).Parent as StackLayout;
			if (e.Data.Properties["Source"] == sl)
			{
				return;
			}

			var color = e.Data.Properties["Color"] as SolidColorBrush;

			if (AllColors.Contains(color))
			{
				AllColors.Remove(color);
				RainbowColors.Add(color);
			}
			else
			{
				RainbowColors.Remove(color);
				AllColors.Add(color);
			}

			SLAllColors.Background = SolidColorBrush.White;
			SLRainbow.Background = SolidColorBrush.White;
		}
	}
}