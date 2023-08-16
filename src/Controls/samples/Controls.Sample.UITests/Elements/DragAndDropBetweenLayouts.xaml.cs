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
			var label = (Label)(sender as Element).Parent;
			var sl = label.Parent as StackLayout;
			e.Data.Properties.Add("Color", label);
			e.Data.Properties.Add("Source", sl);

			if (sl == SLAllColors)
				SLRainbow.Background = SolidColorBrush.LightBlue;
			else
				SLAllColors.Background = SolidColorBrush.LightBlue;

			AddEvent(nameof(OnDragStarting));

			dragStartRelativeSelf.Text = $"Drag Start relative to self: {(int)e.GetPosition(label).Value.X},{(int)e.GetPosition(label).Value.Y}";
			dragStartRelativeScreen.Text = $"Drag Start relative to screen: {(int)e.GetPosition(null).Value.X},{(int)e.GetPosition(null).Value.Y}";
			dragStartRelativeLabel.Text = $"Drag Start relative to this label: {(int)e.GetPosition(dragStartRelativeLabel).Value.X},{(int)e.GetPosition(dragStartRelativeLabel).Value.Y}";
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
			var sl = (StackLayout)(sender as Element).Parent;
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

			dragRelativeDrop.Text = $"Drag relative to receiving layout: {(int)e.GetPosition(sl).Value.X},{(int)e.GetPosition(sl).Value.Y}";
			dragRelativeScreen.Text = $"Drag relative to screen: {(int)e.GetPosition(null).Value.X},{(int)e.GetPosition(null).Value.Y}";
			dragRelativeLabel.Text = $"Drag relative to this label: {(int)e.GetPosition(dragRelativeLabel).Value.X},{(int)e.GetPosition(dragRelativeLabel).Value.Y}";
		}

		private void OnDragLeave(object sender, DragEventArgs e)
		{
			if (!e.Data.Properties.ContainsKey("Source"))
				return;

			var sl = (StackLayout)(sender as Element).Parent;
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