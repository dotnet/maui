namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 17531, "Make senders be the same for different gesture EventArgs")]
	public partial class Issue17531 : TestContentPage
	{
		public Issue17531()
		{
			InitializeComponent();
		}

		protected override void Init()
		{

		}

		void OnTap(object sender, TappedEventArgs e)
		{
			if (sender is View)
			{
				TapResultLabel.Text = "Success";
				TapResultLabel.BackgroundColor = Colors.Green;
			}
			else
			{
				TapResultLabel.Text = "Failed";
				TapResultLabel.BackgroundColor = Colors.Red;
			}
		}

		void OnDragStart(object sender, DragStartingEventArgs e)
		{
			if (sender is View)
			{
				DragResultLabel.Text = "Success";
				DragResultLabel.BackgroundColor = Colors.Green;
			}
			else
			{
				DragResultLabel.Text = "Failed";
				DragResultLabel.BackgroundColor = Colors.Red;
			}
		}

		void OnDragStarting(object sender, DragStartingEventArgs e)
		{
			var label = (Label)sender;
			var sl = label.Parent as StackLayout;
			e.Data.Properties.Add("Color", label);
			e.Data.Properties.Add("Source", sl);

			if (sl == SLAllColors)
				SLRainbow.Background = Brush.LightBlue;
			else
				SLAllColors.Background = Brush.LightBlue;
		}

		void OnDropCompleted(object sender, DropCompletedEventArgs e)
		{
			var sl = ((Element)sender).Parent as StackLayout;

			if (sl == SLAllColors)
				SLRainbow.Background = Brush.White;
			else
				SLAllColors.Background = Brush.White;
		}

		void OnDragOver(object sender, DragEventArgs e)
		{
			if (!e.Data.Properties.ContainsKey("Source"))
				return;

			var sl = ((StackLayout)sender);
			if (e.Data.Properties["Source"] == sl)
			{
				e.AcceptedOperation = DataPackageOperation.None;
				return;
			}

			sl.Background = Brush.LightPink;
		}

		void OnDragLeave(object sender, DragEventArgs e)
		{
			if (!e.Data.Properties.ContainsKey("Source"))
				return;

			var sl = (StackLayout)sender;
			if (e.Data.Properties["Source"] == sl)
			{
				e.AcceptedOperation = DataPackageOperation.None;
				return;
			}

			sl.Background = Brush.LightBlue;
		}

		void OnDrop(object sender, DropEventArgs e)
		{
			if (!e.Data.Properties.ContainsKey("Source"))
				return;

			var sl = (StackLayout)sender;
			if (e.Data.Properties["Source"] == sl)
			{
				return;
			}

			var color = e.Data.Properties["Color"] as Label;

			if (SLAllColors.Children.Contains(color))
			{
				SLAllColors.Children.Remove(color);
				SLRainbow.Children.Add(color);
			}
			else
			{
				SLRainbow.Children.Remove(color);
				SLAllColors.Children.Add(color);
			}

			SLAllColors.Background = Brush.White;
			SLRainbow.Background = Brush.White;

			if (sender is View)
			{
				DropResultLabel.Text = "Success";
				DropResultLabel.BackgroundColor = Colors.Green;
			}
			else
			{
				DropResultLabel.Text = "Failed";
				DropResultLabel.BackgroundColor = Colors.Red;
			}
		}
	}
}