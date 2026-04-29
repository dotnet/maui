using Microsoft.Maui.Layouts;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 34188, "Background color doesn't apply correctly to the spanned region on all the platforms.", PlatformAffected.All)]
public class Issue34188 : ContentPage
{
	StackLayout _outerStack;
	Label _instructionsLabel;
	public Issue34188()
	{
		_outerStack = new StackLayout();
		_outerStack.AutomationId = "OuterStack";
		_instructionsLabel = new Label
		{
			Text = "The spanning cell (green) at row=0, col=1 is arranged with height=200, spanning over both row 0 and row 1. Its green background should be visible throughout the entire 200px height area, not just the top 100px. Without the fix, the parent row's red background is visible in the lower half of the spanning area — this screenshot will not match the correct baseline.",
			Margin = new Thickness(10),
			AutomationId = "InstructionsLabel"
		};
		_outerStack.Children.Add(new Issue34188Grid
		{
			AutomationId = "CustomGrid",
			WidthRequest = 200,
			HeightRequest = 200
		});
		_outerStack.Children.Add(_instructionsLabel);
		Content = _outerStack;
	}

	class Issue34188Grid : Issue34188ControlLayout
	{
		public Issue34188Grid()
		{
			IsClippedToBounds = false;

			Add(new Issue34188Row(0) { Background = Colors.Red });
			Add(new Issue34188Row(1) { Background = Colors.Red });
		}

		protected override ILayoutManager CreateLayoutManager() => new Issue34188LayoutManager(this);

		internal override Size ArrangeContent(Rect bounds)
		{
			double y = bounds.Top;
			foreach (var child in this.Children)
			{
				child.Arrange(new Rect(bounds.Left, y, 200, 100));
				y += 100;
			}
			return new Size(200, 200);
		}

		internal override Size MeasureContent(double widthConstraint, double heightConstraint)
		{
			foreach (var child in this.Children)
			{
				child.Measure(200, 100);
			}
			return new Size(200, 200);
		}
	}

	class Issue34188Row : Issue34188ControlLayout
	{
		readonly int _rowIndex;

		public Issue34188Row(int index)
		{
			_rowIndex = index;
			IsClippedToBounds = false;

			Add(new Issue34188Box(index, 0)
			{
				Background = index == 0 ? Colors.Green : Colors.Yellow,
				AutomationId = $"Cell_{index}_0"
			});

			if (index == 0)
			{
				Add(new Issue34188Box(index, 1)
				{
					Background = Colors.Green,
					AutomationId = "SpanningCell"
				});
			}
		}

		protected override ILayoutManager CreateLayoutManager() => new Issue34188LayoutManager(this);

		internal override Size ArrangeContent(Rect bounds)
		{
			int x = 0;
			foreach (var child in this.Children)
			{
				if (child is Issue34188Box box && box.RowIndex == 0 && box.ColumnIndex == 1)
				{
					child.Arrange(new Rect(x, bounds.Y, 100, 200));
				}
				else
				{
					child.Arrange(new Rect(x, bounds.Y, 100, 100));
				}
				x += 100;
			}
			return new Size(200, 100);
		}

		internal override Size MeasureContent(double widthConstraint, double heightConstraint)
		{
			foreach (var child in this.Children)
			{
				if (child is Issue34188Box box && box.RowIndex == 0 && box.ColumnIndex == 1)
				{
					child.Measure(100, 200);
				}
				else
				{
					child.Measure(100, 100);
				}
			}
			return new Size(200, 100);
		}
	}

	class Issue34188Box : Border
	{
		public int RowIndex;
		public int ColumnIndex;

		public Issue34188Box(int rowIndex, int columnIndex)
		{
			RowIndex = rowIndex;
			ColumnIndex = columnIndex;
			Stroke = Color.FromArgb("#C49B33");
			StrokeThickness = 2;
		}
	}

	abstract class Issue34188ControlLayout : Layout
	{
		internal abstract Size ArrangeContent(Rect bounds);
		internal abstract Size MeasureContent(double widthConstraint, double heightConstraint);
	}

	class Issue34188LayoutManager : LayoutManager
	{
		readonly Issue34188ControlLayout _layout;

		internal Issue34188LayoutManager(Issue34188ControlLayout layout) : base(layout)
		{
			_layout = layout;
		}

		public override Size ArrangeChildren(Rect bounds) => _layout.ArrangeContent(bounds);
		public override Size Measure(double widthConstraint, double heightConstraint) => _layout.MeasureContent(widthConstraint, heightConstraint);
	}
}
