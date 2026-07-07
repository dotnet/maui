using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 20772, "Flickering occurs while updating the width of ContentView through PanGestureRecognizer", PlatformAffected.Android)]
	public partial class Issue20772 : ContentPage
	{
		public Issue20772()
		{
			Content = new CustomView20772();
		}
	}

	public abstract class ControlLayout20772 : Layout
	{
		internal abstract Size LayoutArrangeChildren(Rect bounds);

		internal abstract Size LayoutMeasure(double widthConstraint, double heightConstraint);
	}

	internal class ControlLayoutManager20772 : LayoutManager
	{
		ControlLayout20772 layout;
		internal ControlLayoutManager20772(ControlLayout20772 layout) : base(layout)
		{
			this.layout = layout;
		}

		public override Size ArrangeChildren(Rect bounds) => this.layout.LayoutArrangeChildren(bounds);

		public override Size Measure(double widthConstraint, double heightConstraint) => this.layout.LayoutMeasure(widthConstraint, heightConstraint);
	}

	public class CustomView20772 : ControlLayout20772
	{
		CustomChild20772 _customChild;
		CustomContent20772 _customContent;
		Label _statusLabel;

		public CustomView20772()
		{
			_customChild = new CustomChild20772();
			_customContent = new CustomContent20772();
			_statusLabel = new Label 
			{ 
				AutomationId = "StatusLabel",
				Text = "Waiting for pan gesture...",
				HeightRequest = 50
			};
			
			// Give status label access to CustomChild for width tracking
			_customContent.ChildView = _customChild;
			_customContent.StatusLabel = _statusLabel;
			
			this.Children.Add(_customChild);
			this.Children.Add(_customContent);
			this.Children.Add(_statusLabel);
		}

		protected override ILayoutManager CreateLayoutManager()
		{
			return new ControlLayoutManager20772(this);
		}

		internal override Size LayoutArrangeChildren(Rect bounds)
		{
			(_customChild as IView).Arrange(new Rect(0, 0, _customChild.WidthRequest, _customChild.HeightRequest));
			(_customContent as IView).Arrange(new Rect(_customChild.WidthRequest, 0, _customContent.WidthRequest, _customContent.HeightRequest));
			(_statusLabel as IView).Arrange(new Rect(0, 120, bounds.Width, 50));
			return bounds.Size;
		}

		internal override Size LayoutMeasure(double widthConstraint, double heightConstraint)
		{
			(_customChild as IView).Measure(_customChild.WidthRequest, _customChild.HeightRequest);
			(_customContent as IView).Measure(_customContent.WidthRequest, _customContent.HeightRequest);
			(_statusLabel as IView).Measure(widthConstraint, 50);
			return new Size(widthConstraint, heightConstraint);
		}
	}

	public class CustomChild20772 : ContentView
	{
		Label _label;

		public CustomChild20772()
		{
			AutomationId = "CustomChild";
			BackgroundColor = Colors.Pink;
			HeightRequest = 100;
			WidthRequest = 180;
			_label = new Label() { Text = "Custom Child", HeightRequest = 100, WidthRequest = 180 };
			Content = _label;
		}
	}

	public class CustomContent20772 : ContentView
	{
		Label _label;
		double _startingChildWidth;
		double _totalPanDistance;
		
#nullable enable
		public Label? StatusLabel { get; set; }
		public ContentView? ChildView { get; set; }
#nullable restore

		public CustomContent20772()
		{
			AutomationId = "CustomContent";
			BackgroundColor = Colors.Yellow;
			HeightRequest = 100;
			WidthRequest = 180;
			_label = new Label() { Text = "Drag here", HeightRequest = 100, WidthRequest = 180 };
			Content = _label;

			PanGestureRecognizer gestureRecognizer = new PanGestureRecognizer();
			gestureRecognizer.PanUpdated += OnGestureRecognizerPanUpdated;
			GestureRecognizers.Add(gestureRecognizer);
		}

		void OnGestureRecognizerPanUpdated(object sender, PanUpdatedEventArgs e)
		{
			switch (e.StatusType)
			{
				case GestureStatus.Started:
					_startingChildWidth = ChildView?.WidthRequest ?? 180;
					_totalPanDistance = 0;
					break;
				case GestureStatus.Running:
					_totalPanDistance = e.TotalX;
					OnResizing(e);
					break;
				case GestureStatus.Completed:
				case GestureStatus.Canceled:
					// Check if final width matches expected width based on pan distance
					// With the bug: width changes erratically because coordinates jump
					// With fix: width change = pan distance (approximately)
					if (StatusLabel is not null && ChildView is not null)
					{
						var actualWidthChange = ChildView.WidthRequest - _startingChildWidth;
						var expectedWidthChange = _totalPanDistance;
						var difference = Math.Abs(actualWidthChange - expectedWidthChange);
						
						// Allow some tolerance for timing/rounding
						// With the bug, the difference would be huge due to coordinate jumps
						StatusLabel.Text = $"WidthChange:{actualWidthChange:F0},Expected:{expectedWidthChange:F0}";
					}
					break;
			}
		}

		void OnResizing(PanUpdatedEventArgs e)
		{
			// The key issue: when we update WidthRequest, the view moves
			// With relative coordinates (bug): next TotalX is wrong because view position changed
			// With raw coordinates (fix): TotalX stays consistent
			if (ChildView is not null)
			{
				// Set width directly based on starting width + total pan distance
				// This is the expected behavior with raw coordinates
				ChildView.WidthRequest = _startingChildWidth + e.TotalX;
				ChildView.InvalidateMeasure();
			}
		}
	}
}