using Microsoft.Maui.Controls;
using Microsoft.Maui.Layouts;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 31674, "Label with TextType Html the label is measured as height 0", PlatformAffected.iOS | PlatformAffected.macOS)]
	public partial class Issue31674 : ContentPage
	{
		public Issue31674()
		{
			InitializeComponent();
		}
	}

	public class CustomLayout31674 : Layout
	{
		public Size ArrangeChildren(Rect bounds)
		{
			double yOffset = 0;
			foreach (var child in Children)
			{
				var desiredSize = child.DesiredSize;
				var rect = new Rect(bounds.Left, yOffset, desiredSize.Width, desiredSize.Height);
				child.Arrange(rect);
				yOffset += desiredSize.Height;
			}

			return new Size(bounds.Width, bounds.Height);
		}

		public new Size Measure(double widthConstraint, double heightConstraint)
		{
			double totalHeight = 0;
			foreach (var child in Children)
			{
				// We need this isEnsured condition check for virtualization and to ensure better performance while scrolling in large dataset.
				if ((child is CustomContentView31674) && !(child as CustomContentView31674).isEnsured)
				{
					child.Measure(widthConstraint, heightConstraint);
					var desiredSize = child.DesiredSize;
					totalHeight += desiredSize.Height;
				}
			}
			return new Size(widthConstraint, totalHeight);
		}

		protected override ILayoutManager CreateLayoutManager()
		{
			return new CustomLayoutManager31674(this);
		}
	}

	public class CustomLayoutManager31674 : LayoutManager
	{
		private CustomLayout31674 layout;

		public CustomLayoutManager31674(CustomLayout31674 layout) : base(layout)
		{
			this.layout = layout;
		}

		public override Size ArrangeChildren(Rect bounds)
		{
			return layout.ArrangeChildren(bounds);
		}

		public override Size Measure(double widthConstraint, double heightConstraint)
		{
			return layout.Measure(widthConstraint, heightConstraint);
		}
	}

	public class CustomContentView31674 : ContentView
	{
		public bool isEnsured;

		public CustomContentView31674()
		{
		}

		protected override Size MeasureOverride(double widthConstraint, double heightConstraint)
		{
			Size size = this.Content.DesiredSize;
			// This is the key part that triggers the issue: measuring with PositiveInfinity
			size = (this.Content as IView).Measure(widthConstraint, double.PositiveInfinity);
			isEnsured = true;
			return base.MeasureOverride(widthConstraint, size.Height);
		}

		protected override Size ArrangeOverride(Rect bounds)
		{
			return base.ArrangeOverride(bounds);
		}
	}
}
