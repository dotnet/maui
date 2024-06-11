using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;

namespace Controls.TestCases.App.Issues
{
	[Issue(IssueTracker.ManualTest, 20772, "Flickering occurs while updating the width of ContentView through PanGestureRecognizer", PlatformAffected.Android)]
	public partial class Issue20772 : ContentPage
	{
		public Issue20772()
		{
			Content = new CustomView();
		}
	}

	public abstract class ControlLayout : Layout
	{
		internal abstract Size LayoutArrangeChildren(Rect bounds);

		internal abstract Size LayoutMeasure(double widthConstraint, double heightConstraint);
	}

	internal class ControlLayoutManager : LayoutManager
	{
		ControlLayout layout;
		internal ControlLayoutManager(ControlLayout layout) : base(layout)
		{
			this.layout = layout;
		}

		public override Size ArrangeChildren(Rect bounds) => this.layout.LayoutArrangeChildren(bounds);

		public override Size Measure(double widthConstraint, double heightConstraint) => this.layout.LayoutMeasure(widthConstraint, heightConstraint);
	}

	public class CustomView : ControlLayout
	{
		CustomChild _customChild;
		CustomContent _customContent;

		public CustomView()
		{
			_customChild = new CustomChild();
			_customContent = new CustomContent();
			this.Children.Add(_customChild);
			this.Children.Add(_customContent);

		}

		protected override ILayoutManager CreateLayoutManager()
		{
			return new ControlLayoutManager(this);
		}

		internal override Size LayoutArrangeChildren(Rect bounds)
		{
			(_customChild as IView).Arrange(new Rect(0, 0, _customChild.WidthRequest, _customChild.HeightRequest));
			(_customContent as IView).Arrange(new Rect(_customChild.WidthRequest, 0, _customContent.WidthRequest, this._customContent.HeightRequest));
			return bounds.Size;
		}

		internal override Size LayoutMeasure(double widthConstraint, double heightConstraint)
		{
			(_customChild as IView).Measure(_customChild.WidthRequest, _customChild.HeightRequest);
			(_customContent as IView).Measure(_customContent.WidthRequest, _customContent.HeightRequest);
			return new Size(widthConstraint, heightConstraint);
		}
	}

	public class CustomChild : ContentView
	{
		Label _label;

		public CustomChild()
		{
			BackgroundColor = Colors.Pink;
			HeightRequest = 100;
			WidthRequest = 180;
			_label = new Label() { Text = "Custom Child", HeightRequest = 100, WidthRequest = 180 };
			Content = _label;
		}
	}

	public class CustomContent : ContentView
	{
		Label _label;

		public CustomContent()
		{
			AutomationId = "CustomContent";
			BackgroundColor = Colors.Yellow;
			HeightRequest = 100;
			WidthRequest = 180;
			_label = new Label() { Text = "Custom Content", HeightRequest = 100, WidthRequest = 180 };
			Content = _label;

			PanGestureRecognizer gestureRecognizer = new PanGestureRecognizer();
			gestureRecognizer.PanUpdated += OnGestureRecognizerPanUpdated;
			GestureRecognizers.Add(gestureRecognizer);
		}

		void OnGestureRecognizerPanUpdated(object sender, PanUpdatedEventArgs e)
		{
			switch (e.StatusType)
			{
				case GestureStatus.Running:
					this.OnResizing(e);
					break;

			}
		}

		double _oldData = 0;

		void OnResizing(PanUpdatedEventArgs e)
		{
			var draggedX = e.TotalX - _oldData;
			_oldData = e.TotalX;
			System.Diagnostics.Debug.WriteLine("Old = " + e.TotalX);
			((this.Parent as ControlLayout)!.Children[0] as ContentView)!.WidthRequest += draggedX;
			(this.Parent as ControlLayout)!.Children[0].InvalidateMeasure();
		}
	}
}