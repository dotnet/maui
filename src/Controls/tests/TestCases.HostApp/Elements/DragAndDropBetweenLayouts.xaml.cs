using System.Collections.ObjectModel;

namespace Maui.Controls.Sample
{
	public partial class DragAndDropBetweenLayouts : ContentView
	{
		bool _emittedDragOver = false;
		public ObservableCollection<Brush> AllColors { get; }
		public ObservableCollection<Brush> RainbowColors { get; }
		public DragAndDropBetweenLayouts()
		{
			InitializeComponent();
		}

		void OnDragStarting(object sender, DragStartingEventArgs e)
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

			dragStartEvent.Text = "DragStarting";

			dragStartRelativeSelf.Text = $"Drag Start relative to self: {(int)e.GetPosition(label).Value.X},{(int)e.GetPosition(label).Value.Y}";
			dragStartRelativeScreen.Text = $"Drag Start relative to screen: {(int)e.GetPosition(null).Value.X},{(int)e.GetPosition(null).Value.Y}";
			dragStartRelativeLabel.Text = $"Drag Start relative to this label: {(int)e.GetPosition(dragStartRelativeLabel).Value.X},{(int)e.GetPosition(dragStartRelativeLabel).Value.Y}";
		}

		void OnDropCompleted(object sender, DropCompletedEventArgs e)
		{
			var sl = (sender as Element).Parent.Parent as StackLayout;

			if (sl == SLAllColors)
				SLRainbow.Background = SolidColorBrush.White;
			else
				SLAllColors.Background = SolidColorBrush.White;

			dragCompletedEvent.Text = "DropCompleted";
		}

		void OnDragOver(object sender, DragEventArgs e)
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
				dragOverEvent.Text = "DragOver";
				_emittedDragOver = true;
			}

			dragRelativeDrop.Text = $"Drag relative to receiving layout: {(int)e.GetPosition(sl).Value.X},{(int)e.GetPosition(sl).Value.Y}";
			dragRelativeScreen.Text = $"Drag relative to screen: {(int)e.GetPosition(null).Value.X},{(int)e.GetPosition(null).Value.Y}";
			dragRelativeLabel.Text = $"Drag relative to this label: {(int)e.GetPosition(dragRelativeLabel).Value.X},{(int)e.GetPosition(dragRelativeLabel).Value.Y}";
		}

		void OnDragLeave(object sender, DragEventArgs e)
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

			dragLeaveEvent.Text = "DragLeave";
		}

		void OnDrop(object sender, DropEventArgs e)
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
				ranibowColors.Text = $"RainbowColorsAdd:{color.Text}";
			}
			else
			{
				SLRainbow.Children.Remove(color);
				SLAllColors.Children.Add(color);
			}

			dropRelativeLayout.Text = $"Drop relative to receiving layout: {(int)e.GetPosition(sl).Value.X},{(int)e.GetPosition(sl).Value.Y}";
			dropRelativeScreen.Text = $"Drop relative to screen: {(int)e.GetPosition(null).Value.X},{(int)e.GetPosition(null).Value.Y}";
			dropRelativeLabel.Text = $"Drop relative to this label: {(int)e.GetPosition(dropRelativeLabel).Value.X},{(int)e.GetPosition(dropRelativeLabel).Value.Y}";

			SLAllColors.Background = SolidColorBrush.White;
			SLRainbow.Background = SolidColorBrush.White;

			dropEvent.Text = "Drop";
		}

		void ResetLayouts(object sender, System.EventArgs e)
		{
			SLAllColors.Clear();
			SLRainbow.Clear();

			var leftLayoutColors = new string[] { "Red", "Purple", "Yellow", "Blue" };
			foreach (var color in leftLayoutColors)
			{
				SLAllColors.Add(RegenerateColorLabel(color));
			}

			SLRainbow.Add(RegenerateColorLabel("Green"));
			ResetTestLabels();
		}

		Label RegenerateColorLabel(string color)
		{
			var label = new Label { Text = color, AutomationId = color, HeightRequest = 50, BackgroundColor = Colors.AliceBlue };
			label.BackgroundColor = color switch
			{
				"Red" => Colors.Red,
				"Purple" => Colors.Purple,
				"Yellow" => Colors.Yellow,
				"Blue" => Colors.Blue,
				"Green" => Colors.Green,
				_ => Colors.White
			};

			var dragRecognizer = new DragGestureRecognizer();
			dragRecognizer.DragStarting += OnDragStarting;
			dragRecognizer.DropCompleted += OnDropCompleted;
			label.GestureRecognizers.Add(dragRecognizer);

			return label;
		}

		void ResetTestLabels()
		{
			dragStartEvent.Text = "DragStartEvents: ";
			dragOverEvent.Text = "DragOverEvents: ";
			dragCompletedEvent.Text = "DragCompletedEvents: ";
			ranibowColors.Text = "RainbowColorsAdd: ";
			dropEvent.Text = "DropEvents: ";

			dragStartRelativeSelf.Text = "Drag Start relative to self:";
			dragStartRelativeScreen.Text = "Drag Start relative to screen:";
			dragStartRelativeLabel.Text = "Drag Start relative to this label:";

			dragRelativeDrop.Text = "Drag relative to receiving layout:";
			dragRelativeScreen.Text = "Drag relative to screen:";
			dragRelativeLabel.Text = "Drag relative to this label:";

			dropRelativeLayout.Text = "Drop relative to receiving layout:";
			dropRelativeScreen.Text = "Drop relative to screen:";
			dropRelativeLabel.Text = "Drop relative to this label:";
		}
	}
}