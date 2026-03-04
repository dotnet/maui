using System;
using System.Collections.ObjectModel;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages
{
	public partial class DragAndDropBetweenLayouts
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
			var boxView = (View)((Element)sender)!.Parent;
			DragStartingTitle.IsVisible = true;
			DragStartingPositionLabel.Text = $"- Self X:{(int)e.GetPosition(boxView)!.Value.X}, Y:{(int)e.GetPosition(boxView)!.Value.Y}";
			DragStartingScreenPositionLabel.Text = $"- Screen X:{(int)e.GetPosition(null)!.Value.X}, Y:{(int)e.GetPosition(null)!.Value.Y}";
			DragStartingRelativePositionLabel.Text = $"- This label X:{(int)e.GetPosition(DragStartingRelativePositionLabel)!.Value.X}, Y:{(int)e.GetPosition(DragStartingRelativePositionLabel)!.Value.Y}";

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
			var sl = ((Element)sender).Parent as StackLayout;

			if (sl == SLAllColors)
				SLRainbow.Background = SolidColorBrush.White;
			else
				SLAllColors.Background = SolidColorBrush.White;

		}

		private void OnDragOver(object sender, DragEventArgs e)
		{
			var sl = (StackLayout)((Element)sender).Parent;

			if (!e.Data.Properties.ContainsKey("Source"))
				return;

			DragTitle.IsVisible = true;
			DragPositionLabel.Text = $"- Receiving layout X: {(int)e.GetPosition(sl)!.Value.X}, Y:{(int)e.GetPosition(sl)!.Value.Y}";
			DragScreenPositionLabel.Text = $"- Screen X: {(int)e.GetPosition(null)!.Value.X}, Y:{(int)e.GetPosition(null)!.Value.Y}";
			DragRelativePositionLabel.Text = $"- This label X: {(int)e.GetPosition(DragRelativePositionLabel)!.Value.X}, Y:{(int)e.GetPosition(DragRelativePositionLabel)!.Value.Y}";

			if (e.Data.Properties["Source"] == sl)
			{
				e.AcceptedOperation = DataPackageOperation.None;
				return;
			}

			sl.Background = SolidColorBrush.LightPink;
		}

		private void OnDragLeave(object sender, DragEventArgs e)
		{
			var sl = (StackLayout)((Element)sender).Parent;

			if (!e.Data.Properties.ContainsKey("Source"))
				return;

			DragTitle.IsVisible = true;
			DragPositionLabel.Text = $"- Receiving layout: Y:{(int)e.GetPosition(sl)!.Value.X}, Y:{(int)e.GetPosition(sl)!.Value.Y}";
			DragScreenPositionLabel.Text = $"- Screen: X:{(int)e.GetPosition(null)!.Value.X}, Y:{(int)e.GetPosition(null)!.Value.Y}";
			DragRelativePositionLabel.Text = $"- This label: X:{(int)e.GetPosition(DragRelativePositionLabel)!.Value.X}, Y:{(int)e.GetPosition(DragRelativePositionLabel)!.Value.Y}";

			if (e.Data.Properties["Source"] == sl)
			{
				e.AcceptedOperation = DataPackageOperation.None;
				return;
			}

			sl.Background = SolidColorBrush.LightBlue;
		}

		private void OnDrop(object sender, DropEventArgs e)
		{
			var sl = ((Element)sender).Parent as StackLayout;

			if (!e.Data.Properties.ContainsKey("Source"))
				return;

			if (e.Data.Properties["Source"] == sl)
			{
				return;
			}

			DropTitle.IsVisible = true;
			DropPositionLabel.Text = $"- Receiving layout: Y:{(int)e.GetPosition(sl)!.Value.X}, Y:{(int)e.GetPosition(sl)!.Value.Y}";
			DropScreenPositionLabel.Text = $"- Screen: X:{(int)e.GetPosition(null)!.Value.X}, Y:{(int)e.GetPosition(null)!.Value.Y}";
			DropRelativePositionLabel.Text = $"- This label: X:{(int)e.GetPosition(DropRelativePositionLabel)!.Value.X}, Y:{(int)e.GetPosition(DropRelativePositionLabel)!.Value.Y}";

			var color = (SolidColorBrush)e.Data.Properties["Color"];

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