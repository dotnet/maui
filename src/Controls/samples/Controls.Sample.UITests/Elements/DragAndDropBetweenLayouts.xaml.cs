using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class DragAndDropBetweenLayouts : ContentView
	{
		bool _emittedDragOver = false;
		public ObservableCollection<Brush> AllColors { get; }
		public ObservableCollection<Brush> RainbowColors { get; }
		public DragAndDropBetweenLayouts()
		{
			InitializeComponent();
		}

		void AddEvent(string name)
		{
			events.Text += $"{name},";
		}

		private void OnDragStarting(object sender, DragStartingEventArgs e)
		{
			_emittedDragOver = false;
			var label = (sender as Element).Parent as Label;
			var sl = label.Parent as StackLayout;
			e.Data.Properties.Add("Color", label);
			e.Data.Properties.Add("Source", sl);

			if (sl == SLAllColors)
				SLRainbow.Background = SolidColorBrush.LightBlue;
			else
				SLAllColors.Background = SolidColorBrush.LightBlue;

			AddEvent(nameof(OnDragStarting));
		}

		private void OnDropCompleted(object sender, DropCompletedEventArgs e)
		{
			var sl = (sender as Element).Parent.Parent as StackLayout;

			if (sl == SLAllColors)
				SLRainbow.Background = SolidColorBrush.White;
			else
				SLAllColors.Background = SolidColorBrush.White;

			AddEvent(nameof(OnDropCompleted));
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

			if (!_emittedDragOver) // This can generate a lot of noise, only add it once
			{
				AddEvent(nameof(OnDragOver));
				_emittedDragOver = true;
			}
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

			AddEvent(nameof(OnDragLeave));
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

			var color = e.Data.Properties["Color"] as Label;

			if (SLAllColors.Children.Contains(color))
			{
				SLAllColors.Children.Remove(color);
				SLRainbow.Children.Add(color);
				AddEvent($"RainbowColorsAdd:{color.Text}");
			}
			else
			{
				SLRainbow.Children.Remove(color);
				SLAllColors.Children.Add(color);
				AddEvent($"AllColorsAdd:{color.Text}");
			}

			SLAllColors.Background = SolidColorBrush.White;
			SLRainbow.Background = SolidColorBrush.White;

			AddEvent(nameof(OnDrop));
		}
	}
}