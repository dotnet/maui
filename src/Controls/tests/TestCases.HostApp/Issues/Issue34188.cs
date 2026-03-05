using Microsoft.Maui.Layouts;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 34188, "Background color doesn't apply correctly to the spanned region on all the platforms.", PlatformAffected.All)]
public class Issue34188 : ContentPage
{
	public Issue34188()
	{
		Content = new Issue34188Grid
		{
			AutomationId = "CustomGrid",
			WidthRequest = 200,
			HeightRequest = 200
		};
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
			int y = 0;
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
